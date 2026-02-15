using FishONU.GamePlay.GameState;
using FishONU.Network;
using FishONU.Player;
using Mirror;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishONU.UI
{
    public class GameViewModel
    {
        public ReadOnlyReactiveProperty<GameStateEnum> StateEnum { get; }
        public ReadOnlyReactiveProperty<string> CurrentPlayerName { get; }
        public ReadOnlyReactiveProperty<bool> IsMyTurn { get; }
        public ReadOnlyReactiveProperty<bool> IsGaming { get; }
        public ReadOnlyReactiveProperty<bool> ShowColorPalette { get; }
        public ReadOnlyReactiveProperty<List<string>> GameRankName { get; }
        public ReadOnlyReactiveProperty<int> TablePlayerCount { get; }

        public ReadOnlyReactiveProperty<string[]> SeatNames { get; }

        public ReadOnlyReactiveProperty<FishONU.CardSystem.Color> CurrentGameColor { get; }

        public GameViewModel(GameStateManager gm, PlayerController pl, TableManager tm)
        {
            StateEnum = Observable.EveryValueChanged(gm, x => x.syncStateEnum)
                .ToReadOnlyReactiveProperty(GameStateEnum.None);

            IsMyTurn = Observable.EveryValueChanged(gm, x => x.currentPlayerIndex)
                .CombineLatest(StateEnum, (_, _) => gm.GetCurrentPlayer()?.guid == pl.guid)
                .Select(index => gm.GetCurrentPlayer()?.guid == pl.guid)
                .ToReadOnlyReactiveProperty(false);

            CurrentPlayerName = Observable.EveryValueChanged(gm, x => x.currentPlayerIndex)
                .Select(index => gm.GetCurrentPlayer()?.displayName ?? "")
                .ToReadOnlyReactiveProperty("");

            ShowColorPalette = Observable.CombineLatest(
                StateEnum,
                IsMyTurn,
                (state, isMyTurn) => state == GameStateEnum.WaitingForColor && isMyTurn
                )
                .ToReadOnlyReactiveProperty(false);

            GameRankName = Observable.EveryValueChanged(gm, x => x.syncStateEnum)
                .Where(state => state == GameStateEnum.GameOver)
                .Select(_ => gm.finishedRankList.Select(
                    guid =>
                    {
                        var player = PlayerController.FindPlayerByGuid(guid);
                        if (player != null) return player.displayName;
                        return "";
                    })
                        .ToList())
                .ToReadOnlyReactiveProperty(new List<string>());

            TablePlayerCount = tm.OnSeatChangeAsObservable()
                .Where(_ => NetworkServer.active)
                .ToReadOnlyReactiveProperty(0);

            SeatNames = Observable.Interval(TimeSpan.FromSeconds(2)) // TODO: 暴力解，后面改
                    .AsUnitObservable()
                    .Merge(Observable.Timer(TimeSpan.FromSeconds(1)).AsUnitObservable())
                    .Select(_ =>
                    {
                        string[] names = new string[4] { "", "", "", "" };
                        var allPlayers = GameObject.FindGameObjectsWithTag("Player")
                            .Select(go => go.GetComponent<PlayerController>())
                            .Where(p => p != null);

                        foreach (var p in allPlayers)
                        {
                            // 计算该玩家相对于本地玩家的视觉位置
                            int localIndex = SeatHelper.CalcLocalSeatIndex(pl.seatIndex, p.seatIndex);

                            if (localIndex >= 0 && localIndex < 4)
                            {
                                names[localIndex] = p.displayName;
                            }
                        }
                        return names;
                    })
                    .ToReadOnlyReactiveProperty(new string[4] { "", "", "", "" });

            // 监听 topCardData 的变化
            //CurrentGameColor = Observable.EveryValueChanged(gm, x => x.topCardData)
            CurrentGameColor = Observable.Interval(TimeSpan.FromSeconds(0.7))
                .Select(_ =>
                {
                    var card = gm.topCardData;
                    if (card == null) return FishONU.CardSystem.Color.Black;
                    // 只有当是黑牌且 secondColor 被指定了才换色
                    return (card.color == FishONU.CardSystem.Color.Black && card.secondColor != FishONU.CardSystem.Color.Black)
                        ? card.secondColor
                        : card.color;
                })
                .ToReadOnlyReactiveProperty(FishONU.CardSystem.Color.Black);
        }
    }

    public class GameUI : MonoBehaviour
    {
        [Header("玩家操作")]
        [SerializeField] private Button submitCardButton;

        [SerializeField] private Button drawCardButton;

        [SerializeField] private GameObject secondColorPalette;
        [SerializeField] private Button secondColorChooseRedButton;
        [SerializeField] private Button secondColorChooseBlueButton;
        [SerializeField] private Button secondColorChooseGreenButton;
        [SerializeField] private Button secondColorChooseYellowButton;

        [SerializeField] private Button startGameButton;

        [Header("信息显示")]
        [SerializeField] private TextMeshProUGUI currentPlayerText;
        [SerializeField] private TextMeshProUGUI gameRank;

        [SerializeField] private TextMeshProUGUI[] seatNameTexts;

        [Header("数据")]
        [SerializeField] private GameStateManager gm;
        [SerializeField] private TableManager tm;

        public static GameUI Instance;

        private PlayerController player;

        private GameViewModel _viewModel;

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
        }

        private void Start()
        {
            if (submitCardButton == null) Debug.LogError("SubmitCardButton is null");

            if (drawCardButton == null) Debug.LogError("DrawCardButton is null");

            if (currentPlayerText == null) Debug.LogError("CurrentTurnText is null");
            else currentPlayerText.text = "";

            if (gm == null) Debug.LogError("GameStateManager is null");
            if (tm == null) Debug.LogError("TableManager is null");

            startGameButton.gameObject.SetActive(NetworkServer.active);
            startGameButton.interactable = false;
        }


        public void BindPlayer(PlayerController playerController)
        {
            if (playerController == null)
            {
                Debug.LogError("PlayerController is null");
                return;
            }

            if (player != null)
            {
                Debug.LogError("PlayerController is already binded");
                return;
            }

            player = playerController;
            _viewModel = new GameViewModel(gm, player, tm);

            var d = Disposable.CreateBuilder();

            // action

            #region

            submitCardButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromMilliseconds(1000)) // 防抖动，限制一秒只能触发一次
                .Subscribe(_ =>
                {
                    if (player == null) Debug.LogWarning("PlayerController is null");
                    else player.TryPlayCard();
                })
                .AddTo(ref d);

            drawCardButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromMilliseconds(1000))
                .Subscribe(_ =>
                {
                    if (player == null) Debug.LogWarning("PlayerController is null");
                    else player.TryDrawCard();
                })
                .AddTo(ref d);

            var colorButtons = new[]
            {
                (secondColorChooseRedButton, CardSystem.Color.Red),
                (secondColorChooseBlueButton, CardSystem.Color.Blue),
                (secondColorChooseGreenButton, CardSystem.Color.Green),
                (secondColorChooseYellowButton, CardSystem.Color.Yellow)
            };

            foreach (var (btn, color) in colorButtons)
            {
                btn.OnClickAsObservable()
                    .ThrottleFirst(TimeSpan.FromMilliseconds(1000))
                    .Subscribe(_ => SelectSecondColor(color))
                    .AddTo(ref d);
            }

            startGameButton.OnClickAsObservable()
                .Where(_ => NetworkServer.active)
                .Subscribe(_ => gm.StartGame())
                .AddTo(ref d);

            #endregion

            // view

            #region view bind

            _viewModel.IsMyTurn
                .Subscribe(interactive =>
                {
                    Debug.Log($"My Turn Status: {interactive}");
                    submitCardButton.interactable = interactive;
                    drawCardButton.interactable = interactive;
                })
                .AddTo(ref d);

            _viewModel.CurrentPlayerName
                .CombineLatest(_viewModel.StateEnum, (name, state) => (name, state))
                .CombineLatest(_viewModel.CurrentGameColor, (tuple, color) => (tuple.name, tuple.state, color))
                // 确保有初始值，防止流阻塞
                .Subscribe(x =>
                {
                    if (x.state is GameStateEnum.None or GameStateEnum.GameOver)
                    {
                        currentPlayerText.text = "";
                        return;
                    }

                    // 只有在游戏进行中且有名时才赋值
                    if (!string.IsNullOrEmpty(x.name))
                    {
                        string colorIndicator = x.color == FishONU.CardSystem.Color.Black ? "" : " ●";
                        currentPlayerText.text = $"当前回合: {x.name}{colorIndicator}";
                    }
                })
                .AddTo(ref d);

            _viewModel.GameRankName
                .CombineLatest(_viewModel.StateEnum, (names, state) => (names, state))
                .Subscribe(x =>
                {
                    var (rankList, state) = x;

                    gameRank.text = "";

                    if (state == GameStateEnum.GameOver &&
                        rankList.Count > 0)
                    {
                        gameRank.text += "结算：\n";
                        for (int i = 0; i < rankList.Count; i++)
                        {
                            gameRank.text += $"{i + 1}. {rankList[i]}\n";
                        }
                    }
                })
                .AddTo(ref d);

            _viewModel.ShowColorPalette
                .Subscribe(show =>
                {
                    secondColorPalette.SetActive(show);
                })
                .AddTo(ref d);

            _viewModel.StateEnum
                .Where(_ => NetworkServer.active)
                .Subscribe(state => startGameButton.gameObject.SetActive(state is (GameStateEnum.None or GameStateEnum.GameOver)))
                .AddTo(ref d);

            _viewModel.StateEnum
                .Where(_ => NetworkServer.active)
                .CombineLatest(_viewModel.TablePlayerCount, (state, count) => (state, count))
                .Subscribe(x =>
                {
                    // TODO: 也许可以不用那么频繁触发这个，一个 state 变化这里就重新触发了
                    startGameButton.interactable = x.state is (GameStateEnum.None or GameStateEnum.GameOver) &&
                        x.count >= 2;
                }
                )
                .AddTo(ref d);

            // 绑定座位名字显示
            _viewModel.SeatNames
                .Subscribe(names =>
                {
                    for (int i = 0; i < seatNameTexts.Length; i++)
                    {
                        if (i < names.Length)
                        {
                            seatNameTexts[i].text = names[i];
                        }
                    }
                })
                .AddTo(ref d);

            _viewModel.CurrentGameColor
                .Subscribe(color =>
                {
                    currentPlayerText.color = GetUnityColor(color);
                })
                .AddTo(ref d);

            #endregion

            d.RegisterTo(destroyCancellationToken);
        }

        #region Handler

        public void SelectSecondColor(CardSystem.Color color)
        {
            if (player == null)
            {
                Debug.LogError("PlayerController is null");
                return;
            }

            player.TrySetWildColor(color);
        }

        #endregion Handler

        #region Configs

        private Color GetUnityColor(FishONU.CardSystem.Color cardColor)
        {
            return cardColor switch
            {
                FishONU.CardSystem.Color.Red => Color.red,
                FishONU.CardSystem.Color.Blue => new Color(0.2f, 0.6f, 1f), // 稍微亮一点的蓝，防背景黑
                FishONU.CardSystem.Color.Green => Color.green,
                FishONU.CardSystem.Color.Yellow => Color.yellow,
                _ => Color.white // 黑色或背面时默认白色
            };
        }

        #endregion
    }
}