using FishONU.GamePlay.GameState;
using FishONU.Player;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishONU.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("玩家操作")]
        [SerializeField] private Button submitCardButton;
        [SerializeField] private Button drawCardButton;
        // WARNING: 变色按钮用的是 Unity Event，所以得去 Button 的 Inspector 里面找到 Bind
        [SerializeField] private GameObject secondColorPalette;

        [Header("信息显示")]
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

            if (secondColorPalette == null) Debug.LogError("SecondColorPalette is null");

            if (currentTurnText == null) Debug.LogError("CurrentTurnText is null");
            else currentTurnText.text = "";

            if (gm == null) Debug.LogError("GameStateManager is null");
            else
            {
                gm.OnCurrentPlayerIndexChangeAction += OnCurrentPlayerChange;
                gm.OnStateEnumChangeAction += OnGameStateEnumChange;
            }
        }

        private void OnDestroy()
        {
            if (player != null) UnbindPlayer();
            if (gm != null)
            {
                gm.OnCurrentPlayerIndexChangeAction -= OnCurrentPlayerChange;
                gm.OnStateEnumChangeAction -= OnGameStateEnumChange;
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
        private bool isShowSecondColorPalette;
        private string currentPlayerDisplayName;

        public void SelectSecondColor(string colorString)
        {

            if (player == null)
            {
                Debug.LogError("PlayerController is null");
                return;
            }

            if (Enum.TryParse(colorString, out CardSystem.Color color))
            {
                player.TrySetWildColor(color);
            }
            else
            {
                Debug.LogError($"Color {colorString} is not valid");
                return;
            }
        }

        private void RefreshView()
        {
            submitCardButton.interactable = isTurn;
            drawCardButton.interactable = isTurn;

            currentTurnText.text = currentPlayerDisplayName != null ? $"当前回合: {currentPlayerDisplayName}" : "";

            secondColorPalette.SetActive(isShowSecondColorPalette);
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

        #region Outer Delegate Handler

        private void OnTurnSwitch(bool oldValue, bool newValue)
        {
            isTurn = newValue;
            RefreshView();
        }

        private void OnCurrentPlayerChange(int oldValue, int newValue)
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

        private void OnGameStateEnumChange(GameStateEnum oldValue, GameStateEnum newValue)
        {
            if (newValue == GameStateEnum.WaitingForColor &&
                player.guid == gm.GetCurrentPlayer().guid)
            {
                isShowSecondColorPalette = true;
            }
            else
            {
                isShowSecondColorPalette = false;
            }

            RefreshView();
        }

        #endregion
    }
}