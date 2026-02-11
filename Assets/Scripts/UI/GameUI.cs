using System;
using FishONU.Player;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace FishONU.UI
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private Button submitCardButton;

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
            submitCardButton.onClick.AddListener(OnSubmitCardButtonClick);
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

        private void OnSubmitCardButtonClick()
        {
            if (player == null) Debug.LogError("NetworkClient.localPlayer is null");

            player.GetComponent<PlayerController>().TryPlayCard();
        }

        private void OnTurnSwitch(bool oldValue, bool newValue)
        {
            submitCardButton.interactable = newValue;
        }
    }
}