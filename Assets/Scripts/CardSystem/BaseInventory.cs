using Mirror;
using UnityEngine;

namespace FishONU.CardSystem
{
    public class BaseInventory : NetworkBehaviour
    {
        // TODO: 抽象出 Base Inventory 类
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Vector3 spawnCardPosition;

        [Client]
        public virtual void DebugAddCard()
        {
        }

        [Client]
        public virtual void DebugRemvoeCard()
        {
        }
    }
}