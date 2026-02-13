using UnityEngine;

namespace FishONU.GamePlay.GameState
{
    /// <summary>
    /// 游戏结束状态
    /// </summary>
    public class GameOverState : GameState
    {
        protected override void OnServerEnter(GameStateManager manager)
        {
            base.OnServerEnter(manager);

            // 最后一个倒霉蛋补位
            foreach (var p in manager.players)
            {
                if (manager.finishedRankList.Contains(p.guid)) continue;
                manager.finishedRankList.Add(p.guid);
            }

            Debug.Log("Game Over");
        }
    }
}