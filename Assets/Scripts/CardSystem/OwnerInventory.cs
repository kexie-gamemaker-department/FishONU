using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FishONU.CardSystem.CardArrangeStrategy;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FishONU.CardSystem
{
    [System.Serializable]
    public class OwnerInventory : BaseInventory
    {
        private Dictionary<string, GameObject> localCardObjs = new();

        public readonly List<CardData> LocalCards = new();
        public readonly SyncList<CardData> Cards = new();

        public int LocalCardNumber => LocalCards.Count;

        #region View

        public override IArrangeStrategy GetDefaultArrangeStrategy()
        {
            return new CenterLinearWithArc
            {
                CenterPosition = cardSpawnPosition,
                PositionOffset = new(0.65f, 0.1f, 0f),
                RotationOffset = new(0f, 0f, -5f)
            };
        }

        [Client]
        public void ClientAddCard(CardData cardData)
        {
            if (cardData == null) return;

            LocalCards.Add(cardData);

            RefreshView();
        }

        [Client]
        public void ClientRemoveCard(CardData cardData)
        {
            if (LocalCards.Count == 0) return;

            foreach (var c in LocalCards)
            {
                if (c.Guid != cardData.Guid) continue;

                LocalCards.Remove(c);
                break;
            }

            RefreshView();
        }

        [Client]
        private void SortAllCards()
        {
            LocalCards.Sort((a, b) =>
            {
                var colorCmp = a.color.CompareTo(b.color);
                if (colorCmp != 0) return colorCmp;

                var faceCmp = a.face.CompareTo(b.face);
                if (faceCmp != 0) return faceCmp;

                return String.Compare(a.Guid, b.Guid, StringComparison.Ordinal);
            });
        }

        [Client]
        public override void ArrangeAllCards()
        {
            if (LocalCards.Count == 0) return;

            SortAllCards();

            for (var i = 0; i < LocalCards.Count; i++)
            {
                var guid = LocalCards[i].Guid;
                if (localCardObjs.TryGetValue(guid, out var obj))
                {
                    var t = obj.transform;
                    ArrangeStrategy.Calc(i, LocalCards.Count, out var pos, out var rotation, out var scale);
                    t.DOKill();
                    t.transform.DOLocalMove(pos, 0.5f).SetEase(Ease.InOutQuad);
                    t.transform.DOLocalRotate(rotation, 0.5f).SetEase(Ease.InOutQuad);
                    t.transform.DOScale(scale, 0.5f).SetEase(Ease.InOutQuad);
                }
            }
        }


        [Client]
        public override void InstantiateAllCards()
        {
            // instantiate new cards
            foreach (var card in LocalCards)
            {
                if (localCardObjs.ContainsKey(card.Guid)) continue;

                var cardObj = Instantiate(cardPrefab, gameObject.transform);
                cardObj.transform.localPosition = cardSpawnPosition;
                cardObj.GetComponent<CardObj>().Load(card);
                localCardObjs.Add(card.Guid, cardObj);
            }

            // clean non-exist card
            var cardGuidSet = new HashSet<string>(LocalCards.Select(c => c.Guid));
            var toRemove = new List<string>();
            foreach (var pair in localCardObjs)
            {
                if (cardGuidSet.Contains(pair.Key)) continue;

                toRemove.Add(pair.Key);
            }

            foreach (var guid in toRemove)
            {
                var obj = localCardObjs[guid];

                if (obj.TryGetComponent<CardObj>(out var card))
                {
                    card.FadeOutAndDestory();
                }
                else
                {
                    Destroy(obj);
                }


                localCardObjs.Remove(guid);
            }
        }

        #endregion

        #region Network

        public override void OnStartClient()
        {
            // 如果是 host 模式，防止显示其他人的手牌
            if (!isLocalPlayer) return;

            Cards.Callback += OnSyncCardChange;
        }

        public override void OnStopClient()
        {
            // 如果是 host 模式，防止显示其他人的手牌
            if (!isLocalPlayer) return;

            Cards.Callback -= OnSyncCardChange;
        }

        [Client]
        private void OnSyncCardChange(SyncList<CardData>.Operation op, int index, CardData oldItem, CardData newItem)
        {
            LocalCards.Clear();
            LocalCards.AddRange(Cards);

            // TODO: 实现增量更新
            InstantiateAllCards();
            ArrangeAllCards();
        }

        [Server]
        public void PlayCard(CardData card)
        {
            // TODO: play card
            Debug.Log($"play card: face: {card.face.ToString()}; color: {card.color.ToString()}");
            LocalCards.Remove(card);
        }

        #endregion

        #region Debug

        [Command]
        public void DebugCmdAddCard()
        {
            var cardInfo = CardInfoFactory.CreateRandomCard();

            Cards.Add(cardInfo);
        }

        [Command]
        public void DebugCmdRemoveCard()
        {
            if (Cards.Count == 0) return;

            Cards.RemoveAt(Random.Range(0, Cards.Count));
        }

        #endregion
    }
}