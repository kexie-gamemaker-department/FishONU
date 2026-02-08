using FishONU.CardSystem;
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


        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            CmdSitDown();
        }


        // [Client]
        // public override void OnStartClient()
        // {
        //     base.OnStartClient();
        //
        //     // 因为入座需要知道 local 和 remote，但是无法知道 local 和 remote 谁先存在，所以需要尝试入座
        //     TryArrangeAllSeats();
        // }
        //
        // [Client]
        // public override void OnStartLocalPlayer()
        // {
        //     base.OnStartLocalPlayer();
        //
        //     // 因为入座需要知道 local 和 remote，但是无法知道 local 和 remote 谁先存在，所以需要尝试入座
        //     TryArrangeAllSeats();
        // }

        [Command]
        public void CmdSitDown()
        {
            seatIndex = GameObject.FindGameObjectsWithTag("Player").Length - 1;

            Debug.Log($"player {gameObject.name} command to sit at {seatIndex}");
        }

        [Command]
        public void CmdStandUp()
        {
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