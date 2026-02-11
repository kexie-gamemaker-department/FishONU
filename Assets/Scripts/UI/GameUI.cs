using FishONU.GamePlay.GameState;
using FishONU.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishONU.UI
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private Button submitCardButton;
        [SerializeField] private Button drawCardButton;
        [SerializeField] private TextMeshProUGUI currentTurnText;
        [SerializeField] public GameStateManager gm;

        public static GameUI Instance;

        private PlayerController player;

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
        }

        private void Start()
        {
            if (submitCardButton == null) Debug.LogError("SubmitCardButton is null");
            else submitCardButton.onClick.AddListener(OnSubmitCardButtonClick);

            if (drawCardButton == null) Debug.LogError("DrawCardButton is null");
            else drawCardButton.onClick.AddListener(OnDrawCardButtonClick);

            if (currentTurnText == null) Debug.LogError("CurrentTurnText is null");
            else currentTurnText.text = "";

            if (gm == null) Debug.LogError("GameStateManager is null");
            else
            {
                gm.OnStateEnumChange += OnGameStatEnumChange;
            }
        }

        private void OnDestroy()
        {
            if (player != null) UnbindPlayer();
            if (gm != null)
            {
                gm.OnStateEnumChange -= OnGameStatEnumChange;
            }
        }

        public void BindPlayer(PlayerController playerController)
        {
            if (playerController == null)
            {
                Debug.LogError("PlayerController is null");
                return;
            }

            player = playerController;

            // bind event;
            player.OnTurnViewSwitch += OnTurnSwitch;

            // initial refresh view.
            isTurn = player.isOwnersTurn;
            RefreshView();
        }

        private void UnbindPlayer()
        {
            if (player == null)
            {
                Debug.LogWarning("PlayerController is null");
                return;
            }

            player.OnTurnViewSwitch -= OnTurnSwitch;
        }

        #region View

        private bool isTurn;
        private string currentPlayerDisplayName;

        private void RefreshView()
        {
            submitCardButton.interactable = isTurn;
            drawCardButton.interactable = isTurn;

            currentTurnText.text = currentPlayerDisplayName != null ? $"当前回合: {currentPlayerDisplayName}" : "";
        }

        private void OnSubmitCardButtonClick()
        {
            if (player == null)
            {
                Debug.LogError("PlayerController is null");
                return;
            }

            player.TryPlayCard();
        }

        private void OnDrawCardButtonClick()
        {
            if (player == null)
            {
                Debug.Log("PlayerController is null");
                return;
            }

            player.TryDrawCard();
        }

        #endregion

        #region Out Delegate Handler

        private void OnTurnSwitch(bool oldValue, bool newValue)
        {
            isTurn = newValue;
            RefreshView();
        }

        private void OnGameStatEnumChange(GameStateEnum oldValue, GameStateEnum newValue)
        {
            var p = gm.GetCurrentPlayer();

            if (p == null)
            {
                Debug.LogError("OnGameStateEnumChange: PlayerController is null");
                return;
            }

            currentPlayerDisplayName = p.displayName;
            RefreshView();
        }

        #endregion
    }
}