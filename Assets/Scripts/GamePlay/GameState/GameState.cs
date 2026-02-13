using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace FishONU.GamePlay.GameState
{
    public enum GameStateEnum
    {
        None, // 特殊的状态，对应 null
        Prepare, // 游戏开始的准备阶段
        PlayerTurn, // 玩家阶段
        AffectedTurn, // 受功能牌效果影响的玩家回合
        WaitingForColor, // 变色牌等待玩家选择颜色
        GameOver, // 游戏结束阶段
    }

    public static class GameStateExtensions
    {
        public static GameState GetState(this GameStateEnum stateEnum) => stateEnum switch
        {
            GameStateEnum.None => GameState.None,
            GameStateEnum.Prepare => GameState.Prepare,
            GameStateEnum.AffectedTurn => GameState.AffectedTurn,
            GameStateEnum.WaitingForColor => GameState.WaitingForColor,
            GameStateEnum.GameOver => GameState.GameOver,
            GameStateEnum.PlayerTurn => GameState.PlayerTurn,
            _ => throw new ArgumentOutOfRangeException(nameof(stateEnum), $"无法识别的状态: {stateEnum}"),
        };

        public static GameStateEnum GetEnum(this GameState gameState) => gameState switch
        {
            NoneState => GameStateEnum.None,
            PrepareState => GameStateEnum.Prepare,
            AffectedTurn => GameStateEnum.AffectedTurn,
            PlayerTurnState => GameStateEnum.PlayerTurn, // TODO: 将 switch 判定从 belong to 改成 is
            WaitingForColor => GameStateEnum.WaitingForColor,
            GameOverState => GameStateEnum.GameOver,
            null => GameStateEnum.None,
            _ => throw new ArgumentOutOfRangeException(nameof(gameState), $"无法识别的状态类: {nameof(gameState)}"),
        };
    }

    public abstract class GameState
    {
        // protected GameStateManager Manager { get; set; }

        public static readonly NoneState None = new();
        public static readonly PrepareState Prepare = new();
        public static readonly PlayerTurnState PlayerTurn = new();
        public static readonly AffectedTurn AffectedTurn = new();
        public static readonly GameOverState GameOver = new();
        public static readonly WaitingForColor WaitingForColor = new();


        public virtual void Enter(GameStateManager manager)
        {
            if (manager.isServer) OnServerEnter(manager);
            if (manager.isClient) OnClientEnter(manager);
        }

        protected virtual void OnServerEnter(GameStateManager manager)
        {
        }

        protected virtual void OnClientEnter(GameStateManager manager)
        {
        }

        public virtual void Exit(GameStateManager manager)
        {
            if (manager.isServer) OnServerExit(manager);
            if (manager.isClient) OnClientExit(manager);
        }

        protected virtual void OnServerExit(GameStateManager manager)
        {
        }

        protected virtual void OnClientExit(GameStateManager manager)
        {
        }

        public virtual void Update(GameStateManager manager)
        {
            if (manager.isServer) OnServerUpdate(manager);
            if (manager.isClient) OnClientUpdate(manager);
        }

        protected virtual void OnServerUpdate(GameStateManager manager)
        {
        }

        protected virtual void OnClientUpdate(GameStateManager manager)
        {
        }
    }
}