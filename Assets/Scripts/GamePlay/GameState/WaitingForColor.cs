using UnityEngine;

namespace FishONU.GamePlay.GameState
{
    public class WaitingForColor : GameState
    {

        protected override void OnServerEnter(GameStateManager manager)
        {
            // 重设所有玩家的 turn 状态
            var currentPlayer = manager.GetCurrentPlayer();
            foreach (var player in manager.players)
            {
                player.isOwnersTurn = player.guid == currentPlayer.guid;
            }

            Debug.Log("Waiting for current player to pick a color..."); ;
        }
    }
}
