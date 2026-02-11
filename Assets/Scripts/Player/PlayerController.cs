using System;
using FishONU.CardSystem;
using FishONU.CardSystem.CardArrangeStrategy;
using FishONU.GamePlay.GameState;
using FishONU.UI;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace FishONU.Player
{
    [RequireComponent(typeof(OwnerInventory))]
    [RequireComponent(typeof(SecretInventory))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField, HideInInspector] private OwnerInventory ownerInventory;
        [SerializeField, HideInInspector] private SecretInventory secretInventory;

        private GameStateManager gm;

        // TODO: 智能入座

        [SyncVar(hook = nameof(OnSeatIndexChange))]
        public int seatIndex;

        [SyncVar(hook = nameof(OnTurnSwitch))] public bool isOwnersTurn;

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

            if (gm == null)
            {
                gm = GameObject.FindGameObjectWithTag("GameStateManager").GetComponent<GameStateManager>();
                if (gm == null) Debug.LogError("GameStateManager is null");
            }
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (Camera.main != null)
                    {
                        Vector2 mousePosition = Mouse.current.position.ReadValue();

                        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePosition), Vector2.zero);

                        if (hit.collider != null && hit.collider.gameObject.CompareTag("Card"))
                        {
                            SelectCard(hit.collider.gameObject);
                        }
                    }
                }
            }
        }

        #region View

        public Action<bool, bool> OnTurnViewSwitch;

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
                Debug.Log("Try to ArrangeSeat failed cause local player is null.");
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

        [Client]
        private void SelectCard(GameObject card)
        {
            // highlight card
            var cardData = card.GetComponent<CardObj>().data;
            if (cardData == null)
            {
                Debug.LogError("Try to select a null CardObj");
                return;
            }

            ownerInventory.HighlightCard = cardData;
        }

        #endregion

        #region Network

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

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            // bind ui event
            GameUI.Instance.BindPlayer(this);
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

        [Client]
        public void OnSeatIndexChange(int oldValue, int newValue)
        {
            Debug.Log(
                $"{gameObject.name}'s seat index changed from {oldValue} to {newValue}, try to arrange all seats");
            TryArrangeAllSeats();
        }

        public void OnTurnSwitch(bool oldValue, bool newValue)
        {
            if (isClient) OnTurnViewSwitch?.Invoke(oldValue, newValue);
        }

        #endregion

        #region GamePlay

        [Client]
        public void TryPlayCard()
        {
            var card = ownerInventory.HighlightCard;

            if (card == null)
            {
                Debug.LogWarning("Try to play a null card");
                return;
            }

            CmdTryPlayCard(card);
        }

        [Command]
        public void CmdTryPlayCard(CardData card)
        {
            if (card == null)
            {
                Debug.LogError($"Try to Play a null card");
                return;
            }

            // validate card
            var c = ownerInventory.Cards.Find(c => c.guid == card.guid);
            if (c == null)
            {
                Debug.LogWarning(
                    $"Try to Play a card that is not in the inventory: {card.guid} {card.face.ToString()} {card.color.ToString()}");
                return;
            }

            // discard card
            // TODO: 应该是 gm 干的，不过先这样吧
            ownerInventory.Cards.Remove(c);
            gm.discardPile.GetComponent<DiscardInventory>().Cards.Add(c);
        }

        [Server]
        private void ValidateAndPlayCard(CardData card)
        {
            var c = ownerInventory.LocalCards.Find(c => c.Guid == card.Guid);
            if (c == null)
            {
                Debug.LogWarning(
                    $"Try to Play a card that is not in the inventory: {card.Guid} {card.face.ToString()} {card.color.ToString()}");
                return;
            }

            ownerInventory.PlayCard(c);
        }

        #endregion
    }
}