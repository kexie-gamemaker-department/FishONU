using System;
using System.Collections.Generic;
using System.Linq;
using FishONU.CardSystem;
using FishONU.Player;
using FishONU.Utils;
using Mirror;
using UnityEngine;
using Color = FishONU.CardSystem.Color;

namespace FishONU.GamePlay.GameState
{
    public class GameStateManager : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnStateChange))]
        private GameStateEnum syncStateEnum = GameStateEnum.None;

        private readonly SyncList<string> syncPlayersList = new();


        // TODO: 早晚得和 PlayerController 的座位合并一下
        [SyncVar(hook = nameof(OnCurrentPlayerIndexChange))]
        public int currentPlayerIndex;

        [SyncVar] public int turnDirection = 1;
        [SyncVar] public int drawPenaltyStack;
        [SyncVar] public CardData topCardData = new CardData();
        [SyncVar] public CardData effectingCardData; // 正在生效的功能卡

        public GameState LocalState { get; private set; }


        [Header("预制体")] public GameObject drawPilePrefab;
        public Transform drawPileSpawnAnchor;
        public GameObject discardPilePrefab;
        public Transform discardPileSpawnAnchor;

        [Header("游戏")] public List<PlayerController> players = new List<PlayerController>();
        public GameObject drawPile;
        public GameObject discardPile;

        public DiscardInventory discardPileInventory;
        public OwnerInventory drawPileInventory;

        public Action<GameStateEnum, GameStateEnum> OnStateEnumChangeAction;
        public Action<int, int> OnCurrentPlayerIndexChangeAction;


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

            drawPileInventory = drawPile.GetComponent<OwnerInventory>();
            if (drawPileInventory == null)
            {
                Debug.LogError("drawPileInventory is null");
            }

            NetworkServer.Spawn(drawPile);


            discardPile = Instantiate(discardPilePrefab, discardPileSpawnAnchor.position,
                discardPileSpawnAnchor.rotation);

            discardPileInventory = discardPile.GetComponent<DiscardInventory>();
            if (discardPileInventory == null)
            {
                Debug.LogError("discardPileInventory is null");
            }

            NetworkServer.Spawn(discardPile);
        }

        #endregion

        #region Network

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (isClientOnly)
                syncPlayersList.OnChange += OnPlayersListChange;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (isClientOnly)
                syncPlayersList.OnChange -= OnPlayersListChange;
        }


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
            var currentPlayers = GameObject.FindGameObjectsWithTag("Player")
                .Select(go => go.GetComponent<PlayerController>())
                .Where(p => p != null)
                .ToArray();
            players.AddRange(currentPlayers);
            syncPlayersList.AddRange(currentPlayers.Select(p => p.guid));


            ChangeState(GameStateEnum.Prepare);
        }

        public void ChangeState(GameStateEnum stateEnum)
        {
            LocalState?.Exit(this);

            var oldStateEnum = LocalState.GetEnum();

            LocalState = stateEnum.GetState();

            LocalState.Enter(this);

            OnStateEnumChangeAction?.Invoke(oldStateEnum, stateEnum);

            if (isServer)
                syncStateEnum = stateEnum;
        }

        [Client]
        public void OnStateChange(GameStateEnum oldValue, GameStateEnum newValue)
        {
            if (isServer) return;
            ChangeState(newValue);
        }

        [Client]
        private void OnPlayersListChange(SyncList<string>.Operation op, int index, string value)
        {
            PlayerController p;
            switch (op)
            {
                case SyncList<string>.Operation.OP_ADD:
                case SyncList<string>.Operation.OP_INSERT:
                    Debug.Log($"Player {value} joined");
                    p = PlayerController.FindPlayerByGuid(value);
                    if (p == null)
                    {
                        Debug.LogError($"Player {value} not found");
                        return;
                    }

                    players.Insert(index, p);
                    break;

                case SyncList<string>.Operation.OP_CLEAR:
                    Debug.Log("Players list cleared");
                    players.Clear();
                    break;

                case SyncList<string>.Operation.OP_SET:
                    Debug.Log($"Player {value} set at {index}");
                    p = PlayerController.FindPlayerByGuid(value);
                    if (p == null)
                    {
                        Debug.LogError($"Player {value} not found");
                        return;
                    }

                    players[index] = p;
                    break;

                case SyncList<string>.Operation.OP_REMOVEAT:
                    Debug.Log($"Player {value} removed at {index}");
                    players.RemoveAt(index);
                    break;

                default:
                    Debug.LogError($"Unknown operation {op}");
                    break;
            }
        }

        [Client]
        private void OnCurrentPlayerIndexChange(int oldValue, int newValue)
        {
            OnCurrentPlayerIndexChangeAction?.Invoke(oldValue, newValue);
        }

        #endregion

        #region Gameplay

        public PlayerController GetCurrentPlayer() => players[currentPlayerIndex];

        [Server]
        public void PlayCard(string playerGuid, CardData card)
        {
            if (playerGuid == null || card == null)
            {
                Debug.LogError($"PlayCard: guid: {playerGuid}, card: {card}");
                return;
            }

            // 删卡
            var player = PlayerController.FindPlayerByGuid(playerGuid);
            if (player == null)
            {
                Debug.Log($"Player {playerGuid} not found");
            }

            player.RemoveCard(card);
            discardPileInventory.Cards.Add(card);

            topCardData = card;
            effectingCardData = card;

            Debug.Log($"Player {playerGuid} plays card {card}");

            ChangeState(GameStateEnum.AffectedTurn);
        }

        [Server]
        public void DrawCard(string playerGuid)
        {
            if (playerGuid == null)
            {
                Debug.LogError($"DrawCard: playerGuid is null");
                return;
            }

            // 检查是否需要洗牌
            if (drawPileInventory.Cards.Count == 0)
            {
                ShuffleDrawPile();
            }

            // 抽卡
            var player = PlayerController.FindPlayerByGuid(playerGuid);
            if (player == null)
            {
                Debug.Log($"Player not found");
                return;
            }

            int countToDraw = Math.Max(drawPenaltyStack, 1);
            for (int i = 0; i < countToDraw; i++)
            {
                if (drawPileInventory.Cards.TryPop(out var card))
                {
                    player.AddCard(card);
                }
                else
                {
                    // 牌不够就洗牌
                    ShuffleDrawPile();
                    if (drawPileInventory.Cards.TryPop(out var card2))
                        player.AddCard(card2);
                    else
                    {
                        Debug.LogError($"drawPileInventory.Cards.TryPop() is null");
                        return;
                    }
                }
            }

            drawPenaltyStack = 0;

            Debug.Log($"Player {playerGuid}({player.displayName}) draws card");

            TurnIndexNext();
            ChangeState(GameStateEnum.PlayerTurn);
        }

        [Server]
        public void ShuffleDrawPile()
        {
            var cards = discardPileInventory.Cards
                .Select(c => c)
                .ToList();

            cards.FisherYatesShuffle();

            discardPileInventory.Cards.Clear();
            drawPileInventory.Cards.AddRange(cards);
        }

        [Server]
        public bool CanPlayerAction(string playerGuid)
        {
            if (playerGuid == null)
            {
                Debug.LogError($"IsPlayerTurn: playerGuid is null");
                return false;
            }

            // 不是玩家的回合不能打牌
            var index = players.FindIndex(p => p.guid == playerGuid);
            return index == currentPlayerIndex;
        }


        [Server]
        public bool CanCardPlay(CardData card)
        {
            if (card == null)
            {
                Debug.LogError($"CanPlayCard: card is null");
                return false;
            }

            // 如果有罚牌堆叠正在进行
            if (drawPenaltyStack > 0)
            {
                return card.face == Face.DrawTwo ||
                    card.face == Face.WildDrawFour;
            }

            if (card.color == Color.Black) // 黑牌肯定能打出
                return true;

            if (card.color == topCardData.color || // 同色可打出
                card.color == topCardData.secondColor || // 黑牌变成的颜色相同也可以打出
                card.face == topCardData.face) // 同牌面可打出
                return true;

            return false;
        }

        [Server]
        public bool CanDrawCard()
        {
            // 抽牌堆还有牌
            // TODO: 按理说应该是 drawPileController (PileController) 来弄的，回头再重构吧
            var ownerInventory = drawPile.GetComponent<OwnerInventory>();
            if (ownerInventory == null)
                return false;

            // TODO: 自动洗牌
            if (ownerInventory.Cards.Count == 0) ShuffleDrawPile();

            return ownerInventory.Cards.Count > 0;
        }

        [Server]
        public void TurnIndexNext()
        {
            // set index
            currentPlayerIndex = (currentPlayerIndex + turnDirection + players.Count) % players.Count;
        }

        #endregion
    }
}