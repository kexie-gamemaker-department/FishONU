using System;
using UnityEngine;

namespace FishONU.DebugTools
{
    public class DebugUI : MonoBehaviour
    {
        public bool Display = false;

        public string address { get; private set; }
        public string port { get; private set; }

        public void Awake()
        {
            address = "127.0.0.1";
            port = "7777";
        }

        private void OnGUI()
        {
            if (Display) DrawMenu();
        }

        private void DrawMenu()
        {
            GUI.Box(new Rect(20, 20, 400, 400), "Debug Menu");
            GUILayout.BeginArea(new Rect(25, 45, 350, 300));
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
            GUILayout.EndArea();
        }
    }
}