using FishONU.CardSystem;
using UnityEngine;

namespace FishONU.GamePlay.GameState
{
    /// <summary>
    /// 受到卡片效果的玩家回
    ///
    /// 例如当打出带有效果的功能牌时进入这个状态
    /// </summary>
    public class AffectedPlayerTurn : PlayerTurnState
    {
        protected override void OnServerEnter(GameStateManager manager)
        {
            base.OnServerEnter(manager);

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
            }
        }

        #region Server

        private void ProcessSkip(GameStateManager manager)
        {
            // 限定玩家只能抽牌过
            var p = manager.GetCurrentPlayer();
            p.CmdDrawCard();
            manager.TurnIndexNext();
            manager.ChangeState(GameStateEnum.PlayerTurn);
        }

        #endregion
    }
}