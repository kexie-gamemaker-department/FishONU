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
        [SerializeField] [SyncVar(hook = nameof(OnSyncCardChange))]
        private int syncCardNumber;

        private OwnerInventory ownerInventory;

        [SerializeField] private int cardNumber;
        public override int CardNumber => cardNumber;

        public List<GameObject> cards = new();

        private void Awake()
        {
            ArrangeStrategy = new LinearArrange
            {
                PositionOffset = new Vector3(0.1f, 0.13f, 0f),
                StartPosition = cardSpawnPosition
            };
        }

        protected override void Start()
        {
            base.Start();
            ownerInventory = GetComponent<OwnerInventory>();
            if (ownerInventory == null) Debug.LogError("OwnerInventory not found");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // 中途加入 / 重连
            OnSyncCardChange(0, syncCardNumber);
        }

        public override void OnStartServer()
        {
            if (ownerInventory == null) ownerInventory = GetComponent<OwnerInventory>();
            if (ownerInventory == null) Debug.LogError("OwnerInventory not found");

            ownerInventory.syncCards.Callback += OnOwnerSyncCardNumberChange;

            syncCardNumber = ownerInventory.syncCards.Count;
        }

        public override void OnStopServer()
        {
            ownerInventory.syncCards.Callback -= OnOwnerSyncCardNumberChange;
        }

        [Server]
        private void OnOwnerSyncCardNumberChange(SyncList<CardInfo>.Operation operation, int i, CardInfo card1,
            CardInfo card2)
        {
            syncCardNumber = ownerInventory.syncCards.Count;
        }


        [Client]
        public override void DebugAddCard(CardInfo cardInfo = null)
        {
            cardNumber++;
            InstantiateAllCards();
            ArrangeAllCards();
        }

        [Client]
        public override void DebugRemoveCard(CardInfo cardInfo = null)
        {
            if (cardNumber == 0) return;

            cardNumber--;
            InstantiateAllCards();
            ArrangeAllCards();
        }


        public override void ArrangeAllCards()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                var t = cards[i].transform;
                ArrangeStrategy.Calc(i, cards.Count, out var position, out var rotation, out var scale);
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
            var delta = cardNumber - cards.Count;
            if (delta > 0)
            {
                // add more card
                for (int i = 0; i < delta; i++)
                {
                    var obj = Instantiate(cardPrefab, gameObject.transform);
                    obj.GetComponent<CardObj>()?.Load(new CardInfo(Color.Black, Face.Back));
                    cards.Add(obj);
                }
            }
            else if (delta < 0)
            {
                // destroy surplus card
                for (int i = cards.Count - 1; i >= cardNumber; i--)
                {
                    var obj = cards[i];
                    if (obj.TryGetComponent<CardObj>(out var card))
                    {
                        card.FadeOutAndDestory();
                    }
                    else
                    {
                        Destroy(obj);
                    }

                    cards.RemoveAt(i);
                }
            }
        }

        [Client]
        private void OnSyncCardChange(int oldValue, int newValue)
        {
            // 如果是自身则不进行显示视觉
            if (isLocalPlayer) return;

            cardNumber = newValue;

            InstantiateAllCards();
            ArrangeAllCards();
        }
    }
}