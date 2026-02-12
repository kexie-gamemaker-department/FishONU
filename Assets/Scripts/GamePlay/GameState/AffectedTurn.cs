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

            // 重设所有玩家的 turn 状态
            foreach (var player in manager.players)
            {
                player.isOwnersTurn = false;
            }

            if (manager.topCardData == null)
            {
                Debug.LogError("Top card data is null");
                return;
            }

            switch (manager.topCardData.face)
            {
                case Face.Skip:
                    ProcessSkip(manager);
                    break;
                case Face.Reverse:
                    ProcessReverse(manager);
                    break;
            }
        }

        #region Server

        private void ProcessSkip(GameStateManager manager)
        {
            // 下一个玩家
            manager.TurnIndexNext();

            // 限定玩家只能抽牌过
            var p = manager.GetCurrentPlayer();

            // TODO: 有空重构吧
            manager.DrawCard(p.guid); // 抽卡指令会自动进下一回合

            // manager.TurnIndexNext();
            // manager.ChangeState(GameStateEnum.PlayerTurn);
        }

        private void ProcessReverse(GameStateManager manager)
        {
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

            // TODO: bug 反转要在结束回合前，而不是结束回合后
            manager.EndTurn(false);
        }

        #endregion
    }
}