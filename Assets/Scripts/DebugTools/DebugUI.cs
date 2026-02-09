using System;
using FishONU.CardSystem;
using FishONU.GamePlay.GameState;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace FishONU.DebugTools
{
    public class DebugUI : NetworkBehaviour
    {
        public MenuTab display = MenuTab.None;

        public enum MenuTab
        {
            None,
            Network,
            Card
        }

        public string address;
        public string port;

        public void Awake()
        {
            address = "127.0.0.1";
            port = "7777";
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                display = display == MenuTab.None ? MenuTab.Card : MenuTab.None;
            }
        }

        private void OnGUI()
        {
            if (!isLocalPlayer) return;
            if (display == MenuTab.None) return;

            GUI.Box(new Rect(20, 20, 400, 400), "Debug Menu");
            GUILayout.BeginArea(new Rect(25, 45, 350, 300));

            switch (display)
            {
                case MenuTab.Network:
                    DrawGameStateMenu();
                    break;
                case MenuTab.Card:
                    DrawCardMenu();
                    break;
            }

            GUILayout.EndArea();
        }

        private void DrawGameStateMenu()
        {
            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Start Game", GUILayout.Width(100)))
                GameObject.FindWithTag("GameStateManager")?.GetComponent<GameStateManager>()?.StartGame();
            GUILayout.EndVertical();
        }

        private void DrawCardMenu()
        {
            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Add Card", GUILayout.Width(100)))
            {
                // 寻找 inventory 然后 add card
                GetComponent<OwnerInventory>()?.DebugCmdAddCard();
            }

            if (GUILayout.Button("Remove Card", GUILayout.Width(100)))
            {
                GetComponent<OwnerInventory>()?.DebugCmdRemoveCard();
            }

            if (GUILayout.Button("Arrange Card", GUILayout.Width(100)))
            {
                GetComponent<OwnerInventory>()?.ArrangeAllCards();
            }


            if (GUILayout.Button("Add Secret Card", GUILayout.Width(100)))
            {
                // 寻找 inventory 然后 add card
                GameObject.FindWithTag("Player").GetComponent<SecretInventory>()?.DebugAddCard();
            }

            if (GUILayout.Button("Remove Secret Card", GUILayout.Width(100)))
            {
                GameObject.FindWithTag("Player").GetComponent<SecretInventory>()?.DebugRemoveCard();
            }

            if (GUILayout.Button("Arrange Secret Card", GUILayout.Width(100)))
            {
                GameObject.FindWithTag("Player").GetComponent<SecretInventory>()?.ArrangeAllCards();
            }

            if (GUILayout.Button("Add Discard Card", GUILayout.Width(100)))
            {
                // 寻找 inventory 然后 add card
                GameObject.FindWithTag("Player").GetComponent<DiscardInventory>()?.AddCard();
            }

            GUILayout.EndVertical();
        }
    }
}