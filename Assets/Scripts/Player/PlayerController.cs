using FishONU.CardSystem;
using FishONU.CardSystem.CardArrangeStrategy;
using Mirror;
using UnityEngine;

namespace FishONU.Player
{
    [RequireComponent(typeof(OwnerInventory))]
    [RequireComponent(typeof(SecretInventory))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField, HideInInspector] private OwnerInventory ownerInventory;
        [SerializeField, HideInInspector] private SecretInventory secretInventory;

        // TODO: 智能入座

        [SyncVar(hook = nameof(OnSeatIndexChange))]
        public int seatIndex;

        private void Start()
        {
            // 不用 DI 也行，简单来，反正在 Start 初始化
            if (ownerInventory == null)
            {
                ownerInventory = gameObject.GetComponent<OwnerInventory>();
                if (ownerInventory == null)
                    Debug.LogError("ownerInventory is null");
            }

            if (secretInventory == null)
            {
                secretInventory = gameObject.GetComponent<SecretInventory>();
                if (secretInventory == null)
                    Debug.LogError("SecretInventory is null");
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (secretInventory == null) secretInventory = GetComponent<SecretInventory>();

            secretInventory.ArrangeStrategy = new CenterLinearWithArc
            {
                CenterPosition = new(0f, 0f, 0f),
                PositionOffset = new(0.65f, 0.1f, 0f),
                RotationOffset = new(0f, 0f, -5f)
            };

            if (isServer)
            {
                SitDown();
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (isServer)
            {
                StandUp();
            }
        }


        [Server]
        public void SitDown()
        {
            var table = Object.FindFirstObjectByType<TableManager>();

            if (table == null)
            {
                Debug.Log("TableManager is null");
                return;
            }

            var assignedSeat = table.RequestSitDown(netIdentity);

            if (assignedSeat != -1)
            {
                seatIndex = assignedSeat;
                Debug.Log($"Server assigned seat {assignedSeat} to {gameObject.name}");
            }
            else
            {
                Debug.Log($"Server failed to assign seat to {gameObject.name}");
            }
        }

        [Server]
        public void StandUp()
        {
            var table = Object.FindFirstObjectByType<TableManager>();

            if (table == null)
            {
                Debug.Log("TableManager is null");
                return;
            }

            if (table.RequestStandUp(netIdentity) != -1)
            {
                Debug.Log($"Server try to stand up {gameObject.name} successfully");
            }
            else
            {
                Debug.Log($"Server try to stand up {gameObject.name}");
            }
        }

        [Command]
        public void CmdSitDown()
        {
            SitDown();
        }


        [Command]
        public void CmdStandUp()
        {
            StandUp();
        }

        [Client]
        public void OnSeatIndexChange(int oldValue, int newValue)
        {
            Debug.Log(
                $"{gameObject.name}'s seat index changed from {oldValue} to {newValue}, try to arrange all seats");
            TryArrangeAllSeats();
        }

        [Client]
        private static void TryArrangeAllSeats()
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in players)
            {
                player.GetComponent<PlayerController>().TrySit();
            }
        }


        [Client]
        private void TrySit()
        {
            if (NetworkClient.localPlayer == null)
            {
                Debug.Log("Try to ArrangeSeat failed cause localplayer is null.");
                return;
            }

            var localSeat = isLocalPlayer
                ? 0
                : SeatHelper.CalcLocalSeatIndex(
                    NetworkClient.localPlayer.GetComponent<PlayerController>().seatIndex,
                    seatIndex);

            // var players = GameObject.FindGameObjectsWithTag("Player");

            SeatHelper.SitAt(localSeat, gameObject);
        }


        [Command]
        private void CmdTryPlayCard(CardInfo card)
        {
            ValidateAndPlayCard(card);
        }

        [Server]
        private void ValidateAndPlayCard(CardInfo card)
        {
            var c = ownerInventory.cards.Find(c => c.Guid == card.Guid);
            if (c == null)
            {
                Debug.LogWarning(
                    $"Try to Play a card that is not in the inventory: {card.Guid} {card.face.ToString()} {card.color.ToString()}");
                return;
            }

            ownerInventory.PlayCard(c);
        }
    }
}