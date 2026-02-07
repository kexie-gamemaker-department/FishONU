using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using UnityEngine;

namespace FishONU.CardSystem
{
    [System.Serializable]
    public class SecretInventory : BaseInventory
    {
        [SerializeField] [SyncVar(hook = nameof(OnSyncCardChange))]
        private int syncCardNumber;

        [SerializeField] private int cardNumber;

        public override int CardNumber => cardNumber;

        public Vector3 cardSpaceOffset;

        public List<GameObject> cards = new();


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
                t.DOKill();
                t.DOMove(cardSpawnPosition + cardSpaceOffset * i, 0.5f).SetEase(Ease.InOutQuad);
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

            InstantiateAllCards();
            ArrangeAllCards();
        }
    }
}