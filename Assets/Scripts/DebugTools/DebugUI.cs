using System;
using System.Collections.Generic;
using System.Linq;
using FishONU.CardSystem;
using FishONU.GamePlay.GameState;
using FishONU.Player;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
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
            Card,
            GameState
        }

        private Dictionary<MenuTab, Action> _menuDrawer;

        public string address;
        public string port;

        public void Awake()
        {
            address = "127.0.0.1";
            port = "7777";
        }

        private void Start()
        {
            _menuDrawer = new Dictionary<MenuTab, Action>
            {
                { MenuTab.Network, DrawNetworkMenu }, // 假设你补上了这个
                { MenuTab.Card, DrawCardMenu },
                { MenuTab.GameState, DrawGameStateMenu }
            };
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                NextPage();
            }
        }

        private void NextPage()
        {
            // 获取枚举总数，实现自动循环
            int count = Enum.GetNames(typeof(MenuTab)).Length;
            display = (MenuTab)(((int)display + 1) % count);
        }

        private void OnGUI()
        {
            if (!isLocalPlayer) return;
            if (display == MenuTab.None) return;

            GUI.Box(new Rect(20, 20, 400, 400), "Debug Menu");
            GUILayout.BeginArea(new Rect(25, 45, 350, 300));

            // 直接执行对应的绘制函数
            if (_menuDrawer.TryGetValue(display, out var drawAction))
                drawAction.Invoke();

            GUILayout.EndArea();
        }


        // 这里的 T 是组件类型，action 是你要执行的操作
        private void DebugBtn<T>(string label, Action<T> action, string targetTag = "Self", float width = 250)
            where T : Component
        {
            if (GUILayout.Button(label, GUILayout.Width(width)))
            {
                var target = (targetTag == "Self")
                    ? GetComponent<T>()
                    : GameObject.FindWithTag(targetTag)?.GetComponent<T>();
                if (target != null) action(target);
            }
        }

        private void DrawNetworkMenu()
        {
        }

        private void DrawGameStateMenu()
        {
            GUILayout.BeginVertical("box");

            DebugBtn<GameStateManager>("Server Start Game", gm => gm.StartGame(), targetTag: "GameStateManager");

            GUILayout.EndVertical();
        }

        private void DrawCardMenu()
        {
            GUILayout.BeginVertical("box");

            DebugBtn<OwnerInventory>("Command Add Card", inv => inv.DebugCmdAddCard());
            DebugBtn<OwnerInventory>("Command Remove Card", inv => inv.DebugCmdRemoveCard());

            DebugBtn<OwnerInventory>("Command Add Reverse Card", inv => inv.DebugCmdAddReverseCard());
            DebugBtn<OwnerInventory>("Command Add Skip Card", inv => inv.DebugCmdAddSkipCard());

            if (GUILayout.Button("Command Add all +4 and +2 card", GUILayout.Width(250)))
            {
                var players = GameObject.FindObjectsOfType<PlayerController>();

                foreach (var player in players)
                {
                    player.ownerInventory.Cards.Add(new CardData(CardSystem.Color.Black, Face.WildDrawFour));
                    player.ownerInventory.Cards.Add(new CardData(CardSystem.Color.Red, Face.DrawTwo));
                }
            }

            DebugBtn<OwnerInventory>("Command Add DrawPile Card", inv => inv.DebugCmdAddCard(), "DrawPile");
            DebugBtn<OwnerInventory>("Command Add DrawPile Card", inv => inv.DebugCmdRemoveCard(), "DrawPile");

            DebugBtn<DiscardInventory>("Command Add Discard Card", inv => inv.DebugCmdAddCard(), "DiscardPile");
            DebugBtn<DiscardInventory>("Command Remove Discard Card", inv => inv.DebugCmdRemoveCard(), "DiscardPile");


            GUILayout.EndVertical();
        }
    }
}