using System;
using System.Collections.Generic;
using System.Net.Security;
using FishONU.CardSystem;
using FishONU.CardSystem.CardArrangeStrategy;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace FishONU.GamePlay.GameState
{
    [Serializable]
    public struct GameStateData
    {
        public int currentPlayerIndex; // 如果是玩家回合，出牌玩家的座位号
        public bool turnDirection; // true 是顺时针，false 是逆时针
        public int drawPenaltyStack; // +2 +4 累积牌数
        public CardData topCardData; // 弃牌堆顶牌
    }

    public class GameStateManager : NetworkBehaviour
    {
        // TODO:

        [SyncVar(hook = nameof(OnStateChange))]
        private GameStateEnum CurrentStateEnum = GameStateEnum.None;

        [SyncVar] private GameStateData data = new GameStateData()
        {
            currentPlayerIndex = 0,
            turnDirection = true,
            drawPenaltyStack = 0,
            topCardData = new CardData()
        };

        public GameState LocalState { get; private set; }


        [Header("预制体")] public GameObject drawPilePrefab;
        public Transform drawPileSpawnAnchor;
        public GameObject discardPilePrefab;
        public Transform discardPileSpawnAnchor;

        [Header("游戏")] public List<GameObject> players = new List<GameObject>();
        public GameObject drawPile;
        public GameObject discardPile;

        public override void OnStartServer()
        {
            // load anchor
            LoadPileAnchor();

            InstantiatePile();
        }

        #region StateData

        // 动态状态机弄起来太麻烦了，还不如用静态状态机然后状态全塞这里

        #endregion

        #region View

        [Server]
        public void LoadPileAnchor()
        {
            // TODO:
            // 这里后面得重写
            // 后面在聚合到一个 GameObject 上获取数据吧

            Debug.Log("Server Load Pile Anchor");

            if (drawPileSpawnAnchor == null)
                drawPileSpawnAnchor = GameObject.Find("CustomDrawPileAnchor")?.transform;
            if (drawPileSpawnAnchor == null) Debug.LogError("drawPileSpawnAnchor is null");

            if (discardPileSpawnAnchor == null)
                discardPileSpawnAnchor = GameObject.Find("CustomDiscardPileAnchor")?.transform;
            if (discardPileSpawnAnchor == null) Debug.LogError("discardPileSpawnAnchor is null");
        }

        [Server]
        public void InstantiatePile()
        {
            Debug.Log("Server Instantiate Pile");

            drawPile = Instantiate(drawPilePrefab, drawPileSpawnAnchor.position, drawPileSpawnAnchor.rotation);
            NetworkServer.Spawn(drawPile);


            discardPile = Instantiate(discardPilePrefab, discardPileSpawnAnchor.position,
                discardPileSpawnAnchor.rotation);
            NetworkServer.Spawn(discardPile);
        }

        #endregion

        #region Network

        [Server]
        public void StartGame()
        {
            // 防止重复启动
            if (LocalState != null && LocalState is not NoneState)
            {
                Debug.LogError($"Game already started at state {LocalState}");
                return;
            }


            // 虽然不懂怎么样但是我觉得在这写一个准没错
            if (isClient && !isServer)
            {
                Debug.LogError("Remote Client can't start game");
                return;
            }

            Debug.Log("StartGame");

            // TODO: 先用着，后面再重写
            var currentPlayers = GameObject.FindGameObjectsWithTag("Player");
            players.AddRange(currentPlayers);


            ChangeState(GameStateEnum.Prepare);
        }

        public void ChangeState(GameStateEnum stateEnum)
        {
            LocalState?.Exit(this);
            LocalState = stateEnum.GetState();

            LocalState.Enter(this);

            if (isServer)
                CurrentStateEnum = stateEnum;
        }

        [Client]
        public void OnStateChange(GameStateEnum oldValue, GameStateEnum newValue)
        {
            if (isServer) return;
            ChangeState(newValue);
        }

        #endregion

        #region Gameplay

        #endregion
    }
}