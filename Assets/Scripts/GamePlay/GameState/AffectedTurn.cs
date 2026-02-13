using FishONU.CardSystem;
using UnityEngine;

namespace FishONU.GamePlay.GameState
{
    /// <summary>
    /// 受到卡片效果的玩家回
    ///
    /// 例如当打出带有效果的功能牌时进入这个状态
    /// </summary>
    public class AffectedTurn : GameState
    {
        protected override void OnServerEnter(GameStateManager manager)
        {
            base.OnServerEnter(manager);

            Debug.Log($"AffectedTurn: {manager.effectingCardData.color} {manager.effectingCardData.face}");

            // 重设所有玩家的 turn 状态
            foreach (var player in manager.players)
            {
                player.isOwnersTurn = false;
            }

            if (manager.effectingCardData == null)
            {
                // 需要处理的效果牌为空，则不处理效果
                manager.ChangeState(GameStateEnum.PlayerTurn);
                return;
            }

            switch (manager.effectingCardData.face)
            {
                case Face.Skip:
                    ProcessSkip(manager);
                    break;
                case Face.Reverse:
                    ProcessReverse(manager);
                    break;
                case Face.DrawTwo:
                    ProcessDrawTwo(manager);
                    break;
                case Face.Wild:
                    ProcessWild(manager);
                    break;
                case Face.WildDrawFour:
                    ProcessWildDrawFour(manager);
                    break;
                default:
                    // 普通牌的默认效果是进入下一回合
                    manager.TurnIndexNext();
                    manager.ChangeState(GameStateEnum.PlayerTurn);
                    break;
            }
        }

        #region Server

        private void ProcessSkip(GameStateManager manager)
        {
            manager.effectingCardData = null; // 消费令牌

            // 下一个玩家
            manager.TurnIndexNext();

            // 限定玩家只能抽牌过
            var p = manager.GetCurrentPlayer();

            //manager.DrawCard(p.guid); // jb 写了半天发现记错规则了

            manager.TurnIndexNext();
            manager.ChangeState(GameStateEnum.PlayerTurn);
        }

        private void ProcessReverse(GameStateManager manager)
        {
            manager.effectingCardData = null;

            // 反转出牌顺序
            switch (manager.turnDirection)
            {
                case 1:
                    manager.turnDirection = -1;
                    break;
                case -1:
                    manager.turnDirection = 1;
                    break;
            }


            manager.TurnIndexNext();
            manager.ChangeState(GameStateEnum.PlayerTurn);
        }

        private void ProcessDrawTwo(GameStateManager manager)
        {
            // 下一个玩家要么打 +2 要么 +4 或者选择抽牌清空抽牌堆叠
            manager.effectingCardData = null;

            manager.drawPenaltyStack += 2;

            manager.TurnIndexNext();
            manager.ChangeState(GameStateEnum.PlayerTurn);
        }

        private void ProcessWild(GameStateManager manager)
        {
            manager.effectingCardData = null;

            manager.ChangeState(GameStateEnum.WaitingForColor);
        }

        private void ProcessWildDrawFour(GameStateManager manager)
        {
            manager.effectingCardData = null;

            manager.drawPenaltyStack += 4;

            manager.ChangeState(GameStateEnum.WaitingForColor);
        }

        #endregion
    }
}