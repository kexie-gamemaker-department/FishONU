using FishONU.CardSystem.CardArrangeStrategy;
using Mirror;
using UnityEngine;

namespace FishONU.CardSystem
{
    [System.Serializable]
    public abstract class BaseInventory : NetworkBehaviour
    {
        // TODO: 卡牌变化时应该有一个从某点获取卡牌的动画，比如说抽牌的时候能看到牌从牌堆跑到手牌里的动画（后面写吧）
        [SerializeField] protected GameObject cardPrefab;
        [SerializeField] protected Vector3 cardSpawnPosition;

        public abstract int CardNumber { get; }

        private IArrangeStrategy _arrangeStrategy;

        public IArrangeStrategy ArrangeStrategy
        {
            get => _arrangeStrategy;
            set
            {
                _arrangeStrategy = value;
                ArrangeAllCards();
            }
        }

        public virtual void DebugAddCard(CardInfo cardInfo = null)
        {
            // used to debug.
        }

        public virtual void DebugRemoveCard(CardInfo cardInfo = null)
        {
            // used to debug.
        }

        protected virtual void Start()
        {
            if (cardPrefab == null) Debug.LogError("CardPrefab is null");
            if (cardSpawnPosition == Vector3.zero)
                cardSpawnPosition = gameObject.transform.position;
        }


        public abstract void ArrangeAllCards();

        public abstract void InstantiateAllCards();
    }
}