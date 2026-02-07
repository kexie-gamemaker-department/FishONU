using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Mirror;
using UnityEngine;

namespace FishONU.CardSystem
{
    [System.Serializable]
    public class OwnerInventory : BaseInventory
    {
        private Dictionary<string, GameObject> cardObjs = new();

        public readonly List<CardInfo> cards = new();
        private readonly SyncList<CardInfo> _syncCards = new();

        public override int CardNumber => cards.Count;

        public enum ArrangeType
        {
            HorizontalCenter, // 横向均匀居中（手牌专用，推荐）
            StackOffset, // 叠放偏移（弃牌堆专用，顶牌在正位，下面的牌偏移）
            StackOverlap // 完全堆叠（牌库专用，所有牌叠在一起）
        }

        [Header("卡牌排版配置")] public ArrangeType cardArrangeType;
        public float cardWidth = 1.3f;
        public float cardHeight = 1.9f;
        public Vector2 stackOffset = new Vector2(0.15f, -0.15f); // 叠放偏移
        public float smoothMoveTime = 0.2f; // 卡牌平滑移动时间（0则瞬移）
        public Vector3 cardSpawnPos;


        public override void OnStartClient()
        {
            _syncCards.Callback += OnSyncCardChange;
        }

        public override void OnStopClient()
        {
            _syncCards.Callback -= OnSyncCardChange;
        }


        [Client]
        public override void DebugAddCard(CardInfo cardInfo = null)
        {
            cardInfo ??= new CardInfo();

            cards.Add(cardInfo);

            InstantiateAllCards();
            ArrangeAllCards();
        }

        [Client]
        public override void DebugRemoveCard(CardInfo cardInfo = null)
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
            cards.AddRange(_syncCards);

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
                    var targetPos = new Vector3(i * cardWidth, 0, 0);
                    t.DOKill();
                    t.transform.DOMove(targetPos, 0.5f).SetEase(Ease.InOutQuad);
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

                return a.face.CompareTo(b.face);
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
                cardObj.transform.position = cardSpawnPos;
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
                // TODO: add more animation
                // Destroy(cardObjs[guid]);
                var obj = cardObjs[guid];

                if (obj.TryGetComponent<CardObj>(out var card))
                {
                    card.FadeOutAndDestory();
                }
                else
                {
                    Destroy(obj);
                }

                // if (obj.TryGetComponent<SpriteRenderer>(out var sp))
                // {
                //     sp.DOFade(0, 0.5f).OnComplete(() => { Destroy(obj); });
                // }
                // else
                // {
                //     Destroy(obj);
                // }

                cardObjs.Remove(guid);
            }
        }
    }
}