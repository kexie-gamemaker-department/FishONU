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

        private readonly Dictionary<string, GameObject> localCardObjs = new();
        public readonly SyncList<CardData> Cards = new();

        public int LocalCardNumber => localCardObjs.Count;

        #region View

        public override IArrangeStrategy GetDefaultArrangeStrategy()
        {
            return new RandomSpreadArrange
            {
                CenterPosition = Vector3.zero,
                MaxOffset = 1f,
                MaxRotaion = 30f,
            };
        }

        [Client]
        public void ClientFakeAddCard(CardData cardData)
        {
            if (cardData == null) return;

            var obj = Instantiate(cardPrefab, gameObject.transform);
            localCardObjs.Add(cardData.Guid, obj);

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

            obj.GetComponent<CardObj>().Load(cardData);

            // TODO: animation / arrange

            var index = localCardObjs.Count - 1;
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
        public void ClientFakeRemoveCard(CardData cardData)
        {
            if (cardData == null) return;

            if (!localCardObjs.ContainsKey(cardData.Guid)) return;

            var obj = localCardObjs[cardData.Guid];
            if (obj == null) return;

            obj.GetComponent<CardObj>().FadeOutAndDestory();
        }

        [Client]
        public void ClientFakeClearAllCard()
        {
            foreach (var pair in localCardObjs)
            {
                if (pair.Value.TryGetComponent<CardObj>(out var c))
                    c.FadeOutAndDestory();
                else
                    Destroy(pair.Value);
            }

            localCardObjs.Clear();
        }

        [Client]
        public override void ArrangeAllCards()
        {
            if (Cards.Count == 0) return;

            for (var i = 0; i < Cards.Count; i++)
            {
                var guid = Cards[i].Guid;
                if (localCardObjs.TryGetValue(guid, out var obj))
                {
                    var t = obj.transform;
                    ArrangeStrategy.Calc(i, Cards.Count, out var pos, out var rotation, out var scale);
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
            foreach (var cardInfo in Cards)
            {
                if (localCardObjs.ContainsKey(cardInfo.Guid)) continue;

                var cardObj = Instantiate(cardPrefab, gameObject.transform);
                cardObj.transform.localPosition = cardSpawnPosition;
                cardObj.GetComponent<CardObj>().Load(cardInfo);
                localCardObjs.Add(cardInfo.Guid, cardObj);
            }

            // clean non-exist card
            var cardGuidSet = new HashSet<string>();
            foreach (var cardInfo in Cards)
            {
                cardGuidSet.Add(cardInfo.Guid);
            }

            var toRemove = new List<string>();
            foreach (var pair in localCardObjs)
            {
                if (cardGuidSet.Contains(pair.Key)) continue;

                toRemove.Add(pair.Key);
            }

            foreach (var guid in toRemove)
            {
                // Destroy(cardObjs[guid]);
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
            Cards.OnChange += OnCardChange;
        }

        public override void OnStopClient()
        {
            Cards.OnChange -= OnCardChange;
        }

        [Client]
        private void OnCardChange(SyncList<CardData>.Operation arg1, int arg2, CardData arg3)
        {
            if (arg1 == SyncList<CardData>.Operation.OP_CLEAR)
            {
                ClientFakeClearAllCard();
                return;
            }

            RefreshView();
        }

        [ClientRpc]
        public void RpcManualSyncCardView(CardData cardData)
        {
            ClientFakeAddCard(cardData);
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
            Cards.RemoveAt(Cards.Count - 1);
        }

        #endregion
    }
}