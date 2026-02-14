using FishONU.GamePlay.GameState;
using FishONU.Player;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using R3;
using System.Linq;

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

        public GameViewModel(GameStateManager gm, PlayerController pl)
        {
            StateEnum = Observable.EveryValueChanged(gm, x => x.syncStateEnum)
                .ToReadOnlyReactiveProperty(GameStateEnum.None);

            IsMyTurn = Observable.EveryValueChanged(gm, x => x.currentPlayerIndex)
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

        [Header("信息显示")]
        [SerializeField] private TextMeshProUGUI currentPlayerText;
        [SerializeField] private TextMeshProUGUI gameRank;

        [Header("数据")]
        [SerializeField] public GameStateManager gm;

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
            _viewModel = new GameViewModel(gm, player);

            var d = Disposable.CreateBuilder();

            // action
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

            // view

            _viewModel.IsMyTurn
                .Subscribe(interactive =>
                {
                    submitCardButton.interactable = interactive;
                    drawCardButton.interactable = interactive;
                })
                .AddTo(ref d);

            _viewModel.CurrentPlayerName
                .CombineLatest(_viewModel.StateEnum, (name, state) => (name, state))
                .Subscribe(x =>
                {
                    currentPlayerText.text = "";
                    if (x.state is not (GameStateEnum.None or GameStateEnum.GameOver))
                        currentPlayerText.text = $"当前回合: {x.name}";
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

            // TODO: ...

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
    }
}