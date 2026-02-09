using System;
using System.Collections.Generic;
using DG.Tweening;
using FishONU.CardSystem.CardArrangeStrategy;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FishONU.CardSystem
{
    public class DiscardInventory : BaseInventory
    {
        // TODO: 这个类有点乱，得整理重写一下了
        public int maxCardNumbers = 50;

        private readonly Dictionary<string, GameObject> cardObjs = new();
        public readonly SyncList<CardInfo> syncCards = new();

        public override int CardNumber => cardObjs.Count;

        public void Awake()
        {
            ArrangeStrategy ??= new RandomSpreadArrange
            {
                CenterPosition = Vector3.zero,
                MaxOffset = 1f,
                MaxRotaion = 30f,
            };
        }

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
            if (card != null) ClientAddCard(card);
        }

        [Client]
        private void OnCardClear()
        {
            ClientClearAllCard();
        }

        [Client]
        public void ClientAddCard(CardInfo cardInfo)
        {
            if (cardInfo == null) return;

            var obj = Instantiate(cardPrefab, gameObject.transform);
            cardObjs.Add(cardInfo.Guid, obj);

            // 移除前面的卡
            // Note: 只是不显示了，实际数据还在 SyncList 里面
            // if (cardObjs.Count > maxCardNumbers)
            // {
            //     var removeObj = cardObjs[0];
            //     cardObjs.RemoveAt(0);
            //     Destroy(removeObj);
            // }
            // Note: 因为不能直接和 摆卡策略 兼容，所以暂时注释了
            // 而且必要性存疑，因为卡堆那边可是 180 张实体直出，感觉没太大必要担心。

            var t = obj.transform;
            t.localPosition = cardSpawnPosition;

            obj.GetComponent<CardObj>().Load(cardInfo);

            // TODO: animation / arrange

            var index = cardObjs.Count - 1;
            ArrangeStrategy.Calc(index, index + 1,
                out var pos,
                out var rot,
                out var scale);

            t.DOLocalMove(pos, 0.5f)
                .SetEase(Ease.OutQuad);

            t.DOLocalRotate(rot, 0.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuad);

            t.DOScale(scale, 0.5f);
        }

        [Client]
        public void ClientRemoveCard(CardInfo cardInfo)
        {
            if (cardInfo == null) return;

            if (!cardObjs.ContainsKey(cardInfo.Guid)) return;

            var obj = cardObjs[cardInfo.Guid];
            if (obj == null) return;

            obj.GetComponent<CardObj>().FadeOutAndDestory();
        }

        [Command]
        public void DebugCmdAddCard()
        {
            var cardInfo = new CardInfo(Color.Green, Face.DrawTwo);
            syncCards.Add(cardInfo);
        }


        [Command]
        public void DebugCmdRemoveCard()
        {
            syncCards.RemoveAt(syncCards.Count - 1);
        }


        [Client]
        public void ClientClearAllCard()
        {
            foreach (var pair in cardObjs)
            {
                if (pair.Value.TryGetComponent<CardObj>(out var c))
                    c.FadeOutAndDestory();
                else
                    Destroy(pair.Value);
            }

            cardObjs.Clear();
        }

        [Client]
        public override void ArrangeAllCards()
        {
            if (syncCards.Count == 0) return;

            for (var i = 0; i < syncCards.Count; i++)
            {
                var guid = syncCards[i].Guid;
                if (cardObjs.TryGetValue(guid, out var obj))
                {
                    var t = obj.transform;
                    ArrangeStrategy.Calc(i, syncCards.Count, out var pos, out var rotation, out var scale);
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
            foreach (var cardInfo in syncCards)
            {
                if (cardObjs.ContainsKey(cardInfo.Guid)) continue;

                var cardObj = Instantiate(cardPrefab, gameObject.transform);
                cardObj.transform.localPosition = cardSpawnPosition;
                cardObj.GetComponent<CardObj>().Load(cardInfo);
                cardObjs.Add(cardInfo.Guid, cardObj);
            }

            // clean non-exist card
            var cardGuidSet = new HashSet<string>();
            foreach (var cardInfo in syncCards)
            {
                cardGuidSet.Add(cardInfo.Guid);
            }

            var toRemove = new List<string>();
            foreach (var pair in cardObjs)
            {
                if (cardGuidSet.Contains(pair.Key)) continue;

                toRemove.Add(pair.Key);
            }

            foreach (var guid in toRemove)
            {
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

                cardObjs.Remove(guid);
            }
        }

        [ClientRpc]
        public void RpcManualSyncCardView(CardInfo cardInfo)
        {
            ClientAddCard(cardInfo);
        }
    }
}