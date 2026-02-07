using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using UnityEngine;

namespace FishONU.CardSystem
{
    [System.Serializable]
    public class SecretInventory : NetworkBehaviour
    {
        [SerializeField] private GameObject cardPrefab;


        [SerializeField] [SyncVar(hook = nameof(OnSyncCardChange))]
        private int syncCardNumber;

        [SerializeField] private int cardNumber;

        public int CardNumber => cardNumber;

        [Header("卡牌排版配置")] [SerializeField] public Vector3 cardSpawnPosition;
        public Vector3 cardSpaceOffset;

        public List<GameObject> cards = new();

        private void Start()
        {
            if (cardPrefab == null) Debug.LogError("CardPrefab is null");
            if (cardSpawnPosition == Vector3.zero)
                cardSpawnPosition = gameObject.transform.position;
        }

        [Client]
        public void DebugAddCard()
        {
            // TODO:
            cardNumber++;
            InstantiateAllCard();
            ArrangeAllCard();
        }

        [Client]
        public void DebugRemoveCard()
        {
            // TODO:
            if (cardNumber == 0) return;

            cardNumber--;
            InstantiateAllCard();
            ArrangeAllCard();
        }

        [Client]
        public void ArrangeAllCard()
        {
            // TODO:
            for (int i = 0; i < cards.Count; i++)
            {
                var t = cards[i].transform;
                t.DOKill();
                t.DOMove(cardSpawnPosition + cardSpaceOffset * i, 0.5f).SetEase(Ease.InOutQuad);
            }
        }

        [Client]
        public void InstantiateAllCard()
        {
            // figure out how many cards to instantiate
            var delta = cardNumber - cards.Count;
            if (delta > 0)
            {
                // add more card
                for (int i = 0; i < delta; i++)
                {
                    var obj = Instantiate(cardPrefab, gameObject.transform);
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

        private void OnSyncCardChange(int oldValue, int newValue)
        {
            cardNumber = newValue;

            InstantiateAllCard();
            ArrangeAllCard();
        }
    }
}