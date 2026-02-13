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
    [Serializable]
    public class OwnerInventory : BaseInventory
    {
        private Dictionary<string, GameObject> localCardObjs = new();

        public readonly List<CardData> LocalCards = new();
        public readonly SyncList<CardData> Cards = new();

        public int LocalCardNumber => LocalCards.Count;

        private CardData _highLightCard;

        public CardData HighlightCard
        {
            get => _highLightCard;
            set
            {
                if (value == null)
                {
                    Debug.LogError("HighlightCardObj is not CardObj");
                    return;
                }

                if (!localCardObjs.ContainsKey(value.guid))
                {
                    Debug.LogError("HighlightCard is not in localCardObjs");
                    return;
                }

                var oldValue = _highLightCard;
                if (_highLightCard != null && _highLightCard.guid == value.guid) _highLightCard = null;
                else _highLightCard = value;

                ApplyHighlightCard(oldValue);
            }
        }

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


        /// <summary>
        /// 重新加载数据到视图
        /// 
        /// 疑似 bug: 不要和 SetHighlightCard 在同一帧使用，因为 RefreshView 会覆盖动画
        /// 正常情况应该不会遇到，但是预防一下
        /// </summary>
        [Client]
        public override void RefreshView()
        {
            base.RefreshView();

            ApplyHighlightCard();
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
                    card.FadeOutAndDestroy();
                }
                else
                {
                    Destroy(obj);
                }


                localCardObjs.Remove(guid);
            }
        }


        [Client]
        private void ResetHighlightCardView(CardData cardData)
        {
            if (cardData == null) return;

            Debug.Log("ResetHighlightCard");

            var index = LocalCards.FindIndex(c => c.guid == cardData.guid);
            if (index == -1) return;
            if (!localCardObjs.TryGetValue(cardData.guid, out var obj))
            {
                Debug.LogError($"CardObj not found: {cardData.guid}");
                return;
            }

            var t = obj.transform;

            ArrangeStrategy.Calc(index, LocalCards.Count, out Vector3 pos, out var rot, out var scale);

            t.DOKill();
            t.DOLocalMove(pos, 0.2f).SetEase(Ease.InOutQuad);
            t.DOLocalRotate(rot, 0.2f).SetEase(Ease.InOutQuad);
            t.DOScale(scale, 0.2f).SetEase(Ease.InOutQuad);
        }


        [Client]
        private void SetHighlightCardView(CardData cardData)
        {
            if (cardData == null) return;

            Debug.Log($"SetHighlightCard: {cardData.guid}");

            // highlight new card
            var index = LocalCards.FindIndex(c => c.guid == cardData.guid);
            if (index == -1) return;
            if (!localCardObjs.TryGetValue(cardData.guid, out var obj))
            {
                Debug.LogError($"CardObj not found: {cardData.guid}");
                return;
            }

            // highlight animation
            var t = obj.transform;

            ArrangeStrategy.Calc(index, LocalCards.Count, out var pos, out var rot, out var scale);

            t.DOKill();
            t.DOLocalMove(pos + new Vector3(0, 0.2f, 0), 0.2f)
                .SetEase(Ease.InOutQuad);
            t.DOScale(1.2f, 0.2f).SetEase(Ease.InOutQuad);
        }

        [Client]
        private void ApplyHighlightCard(CardData oldValue = null)
        {
            if (oldValue != null)
            {
                if (_highLightCard != null && oldValue.guid == _highLightCard.guid) return;

                ResetHighlightCardView(oldValue);
            }

            if (_highLightCard == null) return;

            SetHighlightCardView(_highLightCard);
        }

        #endregion

        #region Network

        public override void OnStartClient()
        {
            // 如果是 host 模式，防止显示其他人的手牌
            if (!isLocalPlayer) return;

            Cards.Callback += OnSyncCardChange;

            RefreshView();
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

        #region GamePlay

        public bool HasCard(CardData cardData) => Cards.Contains(cardData);

        #endregion

        #region Debug

        [Command]
        public void DebugCmdAddCard()
        {
            var cardInfo = CardDataFactory.CreateRandomCard();

            Cards.Add(cardInfo);
        }

        [Command]
        public void DebugCmdAddSpecCard(Color cardColor, Face cardFace)
        {
            Cards.Add(new CardData
            {
                color = cardColor,
                face = cardFace
            });
        }

        [Command]
        public void DebugCmdAddReverseCard()
        {
            var card = new CardData(Color.Black, Face.Reverse);

            Cards.Add(card);
        }

        [Command]
        public void DebugCmdAddSkipCard()
        {
            Cards.Add(new CardData(Color.Black, Face.Skip));
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