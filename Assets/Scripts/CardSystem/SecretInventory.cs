using System;
using System.Collections.Generic;
using DG.Tweening;
using FishONU.CardSystem.CardArrangeStrategy;
using Mirror;
using UnityEngine;

namespace FishONU.CardSystem
{
    [RequireComponent(typeof(OwnerInventory))]
    [Serializable]
    public class SecretInventory : BaseInventory
    {
        [SyncVar(hook = nameof(OnSyncCardChange))]
        public int CardNumber;

        private OwnerInventory ownerInventory;

        public int LocalCardNumber { get; private set; }

        public List<GameObject> localCards = new();


        #region View

        protected override void Start()
        {
            base.Start();
            ownerInventory = GetComponent<OwnerInventory>();
            if (ownerInventory == null) Debug.LogError("OwnerInventory not found");
        }

        public override IArrangeStrategy GetDefaultArrangeStrategy()
        {
            return new LinearArrange
            {
                PositionOffset = new(0.007f, 0.01f, -0.01f),
                StartPosition = cardSpawnPosition
            };
        }

        [Client]
        private void ClientAddCard(int cardNumber)
        {
            LocalCardNumber += cardNumber;
            RefreshView();
        }

        [Client]
        private void ClientRemoveCard(int cardNumber)
        {
            LocalCardNumber -= cardNumber;
            RefreshView();
        }

        [Client]
        private void ClientSetCardNumber(int cardNumber)
        {
            LocalCardNumber = cardNumber;
            RefreshView();
        }

        [Client]
        public override void ArrangeAllCards()
        {
            for (int i = 0; i < localCards.Count; i++)
            {
                var t = localCards[i].transform;
                ArrangeStrategy.Calc(i, localCards.Count, out var position, out var rotation, out var scale);
                t.DOKill();
                t.DOLocalMove(position, 0.5f).SetEase(Ease.InOutQuad);
                t.DOLocalRotate(rotation, 0.5f).SetEase(Ease.InOutQuad);
                t.DOScale(scale, 0.5f).SetEase(Ease.InOutQuad);
            }
        }

        [Client]
        public override void InstantiateAllCards()
        {
            // figure out how many cards to instantiate
            var delta = LocalCardNumber - localCards.Count;
            if (delta > 0)
            {
                // add more card
                for (int i = 0; i < delta; i++)
                {
                    var obj = Instantiate(cardPrefab, gameObject.transform);
                    obj.GetComponent<CardObj>()?.Load(new CardData(Color.Black, Face.Back));
                    localCards.Add(obj);
                }
            }
            else if (delta < 0)
            {
                // destroy surplus card
                for (int i = localCards.Count - 1; i >= LocalCardNumber; i--)
                {
                    var obj = localCards[i];
                    if (obj.TryGetComponent<CardObj>(out var card))
                    {
                        card.FadeOutAndDestroy();
                    }
                    else
                    {
                        Destroy(obj);
                    }

                    localCards.RemoveAt(i);
                }
            }
        }

        #endregion

        #region Network

        public override void OnStartClient()
        {
            base.OnStartClient();

            // TODO: 中途加入 / 重连 ?
            ClientSetCardNumber(CardNumber);

            GetComponent<OwnerInventory>().RefreshView();
            RefreshView();
        }

        public override void OnStartServer()
        {
            if (ownerInventory == null) ownerInventory = GetComponent<OwnerInventory>();
            if (ownerInventory == null) Debug.LogError("OwnerInventory not found");

            // 跟踪 OwnerInventory 的卡牌数量
            ownerInventory.Cards.Callback += OnOwnerSyncCardNumberChange;

            // 手动更新
            CardNumber = ownerInventory.Cards.Count;
        }

        public override void OnStopServer()
        {
            ownerInventory.Cards.Callback -= OnOwnerSyncCardNumberChange;
        }

        [Client]
        private void OnSyncCardChange(int oldValue, int newValue)
        {
            // 如果是自身则不进行显示视觉
            if (isLocalPlayer) return;

            ClientSetCardNumber(newValue);
        }

        [ClientRpc]
        public void RpcManualSyncCardView(int count)
        {
            ClientSetCardNumber(count);
        }

        [Server]
        private void OnOwnerSyncCardNumberChange(SyncList<CardData>.Operation operation, int index, CardData card1,
            CardData card2)
        {
            CardNumber = ownerInventory.Cards.Count;
        }

        #endregion

        #region Debug

        [Client]
        public void DebugClientAddCard()
        {
            ClientAddCard(1);
        }

        [Client]
        public void DebugRemoveCard()
        {
            ClientRemoveCard(1);
        }

        #endregion
    }
}