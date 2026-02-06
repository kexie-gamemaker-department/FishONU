using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace FishONU.CardSystem
{
    [System.Serializable]
    public class CardInventory : NetworkBehaviour
    {
        [SerializeField] private GameObject cardPrefab;
        private Dictionary<string, GameObject> cardGo;
        public SyncList<CardInfo> cards = new();

        public enum ArrangeType
        {
            HorizontalCenter, // 横向均匀居中（手牌专用，推荐）
            StackOffset, // 叠放偏移（弃牌堆专用，顶牌在正位，下面的牌偏移）
            StackOverlap // 完全堆叠（牌库专用，所有牌叠在一起）
        }

        [Header("卡牌排版配置")] public ArrangeType cardArrangeType;
        public float cardSpacing = 1.2f; // 间距
        public Vector2 stackOffset = new Vector2(0.15f, -0.15f); // 叠放偏移
        public float smoothMoveTime = 0.2f; // 卡牌平滑移动时间（0则瞬移）


        [ClientRpc]
        public void RpcAddCard(CardInfo cardInfo)
        {
            cards.Add(cardInfo);
            // TODO: display card
            // display card
            // Instantiate(cardPrefab);
        }

        [ClientRpc]
        public void RpcRemoveCard(CardInfo cardInfo)
        {
            cards.Remove(cardInfo);
            // TODO: display card
        }

        [Server]
        public void PlayCard(CardInfo card)
        {
            // TODO: play card
            Debug.Log($"play card: face: {card.face.ToString()}; color: {card.color.ToString()}");
            RpcRemoveCard(card);
        }

        [Client]
        private void ArrangeAllCard()
        {
            // TODO: arrange all card
            if (cards.Count == 0) return;
        }

        private void OnCardListChanged(List<CardInfo> old, List<CardInfo> cur)
        {
            ArrangeAllCard();
        }
    }
}