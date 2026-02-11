using System;
using FishONU.Player;
using Mirror;
using Mirror.Examples.Common.Controllers.Tank;
using UnityEngine;
using UnityEngine.UI;

namespace FishONU.UI
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private Button submitCardButton;
        [SerializeField] private Button drawCardButton;

        public static GameUI Instance;

        private PlayerController player;

        private bool isTurn;

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
        }

        private void OnDestroy()
        {
            if (player != null) UnbindPlayer();
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

        private void RefreshView()
        {
            submitCardButton.interactable = isTurn;
            drawCardButton.interactable = isTurn;
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


        private void OnTurnSwitch(bool oldValue, bool newValue)
        {
            isTurn = newValue;
            RefreshView();
        }
    }
}