using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using UnityEngine;

namespace FishONU.CardSystem
{
    public class DiscardInventory : BaseInventory
    {
        public int maxCardNumbers = 50;

        private readonly List<GameObject> cardObjs = new();
        public readonly SyncList<CardInfo> syncCards = new();

        public override int CardNumber => cardObjs.Count;

        public override void OnStartClient()
        {
            syncCards.OnAdd += OnCardListAdd;
            syncCards.OnClear += OnCardClear;
        }

        public override void OnStopClient()
        {
            syncCards.OnAdd -= OnCardListAdd;
            syncCards.OnClear -= OnCardClear;
        }

        [Client]
        private void OnCardListAdd(int index)
        {
            var card = syncCards[index];
            if (card != null) AddCard(card);
        }

        [Client]
        private void OnCardClear()
        {
            ClearAllCard();
        }

        [Client]
        public void AddCard(CardInfo cardInfo)
        {
            var obj = Instantiate(cardPrefab, gameObject.transform);
            cardObjs.Add(obj);

            // 移除前面的卡
            // Note: 只是不显示了，实际数据还在 SyncList 里面
            if (cardObjs.Count > maxCardNumbers)
            {
                var removeObj = cardObjs[0];
                cardObjs.RemoveAt(0);
                Destroy(removeObj);
            }

            var t = obj.transform;

            t.position = cardSpawnPosition;

            obj.GetComponent<CardObj>().Load(cardInfo);

            // TODO: animation / arrange

            t.DOMove(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f), 0.5f)
                .SetEase(Ease.OutQuad);

            // 随机旋转摆放
            t.DORotate(new Vector3(0f, 0f, Random.Range(-30f, 30f)), 0.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuad);
        }

        [Client]
        public void ClearAllCard()
        {
            foreach (var obj in cardObjs)
            {
                if (obj.TryGetComponent<CardObj>(out var c))
                    c.FadeOutAndDestory();
                else
                    Destroy(obj);
            }

            cardObjs.Clear();
        }

        [Client]
        public override void ArrangeAllCards()
        {
        }

        [Client]
        public override void InstantiateAllCards()
        {
            // TODO: 
        }
    }
}