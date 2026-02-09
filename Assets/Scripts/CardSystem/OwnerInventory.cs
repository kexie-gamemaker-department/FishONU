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
        private Dictionary<string, GameObject> cardObjs = new();

        public readonly List<CardInfo> cards = new();
        public readonly SyncList<CardInfo> syncCards = new();

        public override int CardNumber => cards.Count;

        private void Awake()
        {
            // card width and height is 1.3f * 1.9f enough.
            // ArrangeStrategy = new LinearArrange
            // {
            //     StartPosition = cardSpawnPosition,
            //     PositionOffset = new Vector3(1.3f, 0f, 0f)
            // };

            ArrangeStrategy ??= new CenterLinearWithArc
            {
                CenterPosition = cardSpawnPosition,
                PositionOffset = new(0.65f, 0.1f, 0f),
                RotationOffset = new(0f, 0f, -5f)
            };
        }


        public override void OnStartClient()
        {
            // 如果是 host 模式，防止显示其他人的手牌
            if (!isLocalPlayer) return;

            syncCards.Callback += OnSyncCardChange;
        }

        public override void OnStopClient()
        {
            // 如果是 host 模式，防止显示其他人的手牌
            if (!isLocalPlayer) return;

            syncCards.Callback -= OnSyncCardChange;
        }

        [Command]
        public void DebugCmdAddCard()
        {
            var cardInfo = new CardInfo((Color)Random.RandomRange(0, 4), (Face)Random.RandomRange(0, 15));

            syncCards.Add(cardInfo);
        }

        [Command]
        public void DebugCmdRemoveCard()
        {
            if (syncCards.Count == 0) return;

            syncCards.RemoveAt(Random.Range(0, syncCards.Count));
        }

        [Server]
        public override void DebugAddCard(CardInfo cardInfo = null)
        {
            cardInfo ??= new CardInfo(Color.Blue, Face.DrawTwo);

            syncCards.Add(cardInfo);
        }

        [Server]
        public void DebugRemoveCard()
        {
            if (cards.Count == 0) return;

            syncCards.RemoveAt(Random.Range(0, cards.Count));
        }


        [Client]
        public void ClientAddCard(CardInfo cardInfo = null)
        {
            cardInfo ??= new CardInfo();

            cards.Add(cardInfo);

            InstantiateAllCards();
            ArrangeAllCards();
        }

        [Client]
        public void ClientRemoveCard(CardInfo cardInfo = null)
        {
            if (cards.Count == 0) return;

            // 随机删牌
            if (cardInfo == null)
            {
                var count = cards.Count;
                var index = Random.Range(0, count);
                cardInfo = cards[index];
            }

            cards.Remove(cardInfo);

            InstantiateAllCards();
            ArrangeAllCards();
        }


        [Client]
        private void OnSyncCardChange(SyncList<CardInfo>.Operation op, int index, CardInfo oldItem, CardInfo newItem)
        {
            cards.Clear();
            cards.AddRange(syncCards);

            // TODO: 实现增量更新
            InstantiateAllCards();
            ArrangeAllCards();
        }


        [Server]
        public void PlayCard(CardInfo card)
        {
            // TODO: play card
            Debug.Log($"play card: face: {card.face.ToString()}; color: {card.color.ToString()}");
            cards.Remove(card);
        }

        [Client]
        public override void ArrangeAllCards()
        {
            if (cards.Count == 0) return;

            SortAllCards();

            for (var i = 0; i < cards.Count; i++)
            {
                var guid = cards[i].Guid;
                if (cardObjs.TryGetValue(guid, out var obj))
                {
                    var t = obj.transform;
                    ArrangeStrategy.Calc(i, cards.Count, out var pos, out var rotation, out var scale);
                    t.DOKill();
                    t.transform.DOLocalMove(pos, 0.5f).SetEase(Ease.InOutQuad);
                    t.transform.DOLocalRotate(rotation, 0.5f).SetEase(Ease.InOutQuad);
                    t.transform.DOScale(scale, 0.5f).SetEase(Ease.InOutQuad);
                }
            }
        }

        [Client]
        private void SortAllCards()
        {
            cards.Sort((a, b) =>
            {
                var colorCmp = a.color.CompareTo(b.color);
                if (colorCmp != 0) return colorCmp;

                var faceCmp = a.face.CompareTo(b.face);
                if (faceCmp != 0) return faceCmp;

                return String.Compare(a.Guid, b.Guid, StringComparison.Ordinal);
            });
        }

        [Client]
        public override void InstantiateAllCards()
        {
            // instantiate new cards
            foreach (var card in cards)
            {
                if (cardObjs.ContainsKey(card.Guid)) continue;

                var cardObj = Instantiate(cardPrefab, gameObject.transform);
                cardObj.transform.localPosition = cardSpawnPosition;
                cardObj.GetComponent<CardObj>().Load(card);
                cardObjs.Add(card.Guid, cardObj);
            }

            // clean non-exist card
            var cardGuidSet = new HashSet<string>(cards.Select(c => c.Guid));
            var toRemove = new List<string>();
            foreach (var pair in cardObjs)
            {
                if (cardGuidSet.Contains(pair.Key)) continue;

                toRemove.Add(pair.Key);
            }

            foreach (var guid in toRemove)
            {
                var obj = cardObjs[guid];

                if (obj.TryGetComponent<CardObj>(out var card))
                {
                    card.FadeOutAndDestory();
                }
                else
                {
                    Destroy(obj);
                }


                cardObjs.Remove(guid);
            }
        }
    }
}