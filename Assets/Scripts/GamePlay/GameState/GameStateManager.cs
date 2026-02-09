using System;
using System.Collections.Generic;
using FishONU.CardSystem;
using FishONU.CardSystem.CardArrangeStrategy;
using Mirror;
using UnityEditor;
using UnityEngine;

namespace FishONU.GamePlay.GameState
{
    public class GameStateManager : NetworkBehaviour
    {
        // TODO:
        public enum GameStateEnum
        {
            None, // 特殊的状态，对应 null
            Prepare, // 游戏开始的准备阶段
            PlayerTurn, // 玩家阶段
            EffectProcessing, // 功能牌效果阶段
            GameOver, // 游戏结束阶段
        }

        public GameState CurrentState { get; private set; }

        [SyncVar(hook = nameof(OnStateChange))]
        public GameStateEnum currentStateEnum = GameStateEnum.None;

        [Header("预制体")] public GameObject drawPilePrefab;
        public Transform drawPileSpawnAnchor;
        public GameObject discardPilePrefab;
        public Transform discardPileSpawnAnchor;

        [Header("游戏")] public List<GameObject> players = new List<GameObject>();
        public GameObject drawPile;
        public GameObject discardPile;

        private void Start()
        {
            if (drawPilePrefab == null) Debug.LogError("drawPilePrefab is null");
            if (discardPilePrefab == null) Debug.LogError("discardPilePrefab is null");
            if (drawPileSpawnAnchor == null) Debug.LogError("drawPileSpawnAnchor is null");
            if (discardPileSpawnAnchor == null) Debug.LogError("discardPileSpawnAnchor is null");
        }

        [Server]
        public void StartGame()
        {
            // 防止重复启动
            if (CurrentState != null) return;

            // 虽然不懂怎么样但是我觉得在这写一个准没错
            if (isClient && !isServer)
            {
                Debug.LogError("Client can't start game");
                return;
            }

            Debug.Log("StartGame");


            drawPile = Instantiate(drawPilePrefab, drawPileSpawnAnchor.position, drawPileSpawnAnchor.rotation);
            NetworkServer.Spawn(drawPile);


            discardPile = Instantiate(discardPilePrefab, discardPileSpawnAnchor.position,
                discardPileSpawnAnchor.rotation);
            NetworkServer.Spawn(discardPile);

            // TODO: 先用着，后面再重写
            var currentPlayers = GameObject.FindGameObjectsWithTag("Player");
            players.AddRange(currentPlayers);


            ChangeState(new PrepareState());
        }

        [Client]
        private void OnStateChange(GameStateEnum oldValue, GameStateEnum newValue)
        {
            if (isServer) return;

            switch (newValue)
            {
                case GameStateEnum.Prepare:
                    ChangeState(new PrepareState());
                    break;
                case GameStateEnum.EffectProcessing:
                    ChangeState(new EffectProcessingState());
                    break;
                case GameStateEnum.PlayerTurn:
                    ChangeState(new PlayerTurnState());
                    break;
                case GameStateEnum.GameOver:
                    ChangeState(new GameOverState());
                    break;
                case GameStateEnum.None:
                    break;
            }
        }

        public void ChangeState(GameState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;

            if (isServer)
            {
                switch (CurrentState)
                {
                    case PrepareState:
                        currentStateEnum = GameStateEnum.Prepare;
                        break;
                    case EffectProcessingState:
                        currentStateEnum = GameStateEnum.EffectProcessing;
                        break;
                    case PlayerTurnState:
                        currentStateEnum = GameStateEnum.PlayerTurn;
                        break;
                    case GameOverState:
                        currentStateEnum = GameStateEnum.GameOver;
                        break;
                    default:
                        Debug.LogError("Unknown state: " + CurrentState);
                        break;
                }
            }

            newState.Initialize(this);
            CurrentState.Enter();
        }
    }
}