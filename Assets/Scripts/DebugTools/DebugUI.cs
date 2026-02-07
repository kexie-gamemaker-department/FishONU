using System;
using FishONU.CardSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace FishONU.DebugTools
{
    public class DebugUI : MonoBehaviour
    {
        public MenuTab display = MenuTab.None;

        public enum MenuTab
        {
            None,
            Network,
            Card
        }

        public string address { get; private set; }
        public string port { get; private set; }

        public void Awake()
        {
            address = "127.0.0.1";
            port = "7777";
        }

        private void OnGUI()
        {
            if (display == MenuTab.None) return;

            GUI.Box(new Rect(20, 20, 400, 400), "Debug Menu");
            GUILayout.BeginArea(new Rect(25, 45, 350, 300));

            switch (display)
            {
                case MenuTab.Network:
                    DrawNetworkMenu();
                    break;
                case MenuTab.Card:
                    DrawCardMenu();
                    break;
            }

            GUILayout.EndArea();
        }

        private void DrawNetworkMenu()
        {
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Address: ");
            address = GUILayout.TextField(address);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Port: ");
            port = GUILayout.TextField(port);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Connect", GUILayout.Width(100)))
            {
                Debug.Log($"Try to connect {address}:{port}");
            }

            GUILayout.EndVertical();
        }

        private void DrawCardMenu()
        {
            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Add Card", GUILayout.Width(100)))
            {
                // 寻找 inventory 然后 add card
                GameObject.FindWithTag("Player").GetComponent<OwnerInventory>()?.DebugAddCard(new CardInfo());
            }

            if (GUILayout.Button("Remove Card", GUILayout.Width(100)))
            {
                GameObject.FindWithTag("Player").GetComponent<OwnerInventory>()?.DebugRemoveCard();
            }

            if (GUILayout.Button("Arrange Card", GUILayout.Width(100)))
            {
                GameObject.FindWithTag("Player").GetComponent<OwnerInventory>()?.ArrangeAllCard();
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
                GameObject.FindWithTag("Player").GetComponent<SecretInventory>()?.ArrangeAllCard();
            }

            GUILayout.EndVertical();
        }
    }
}