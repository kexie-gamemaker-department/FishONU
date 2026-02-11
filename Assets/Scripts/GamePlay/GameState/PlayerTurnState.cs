namespace FishONU.GamePlay.GameState
{
    public class PlayerTurnState : GameState
    {
        protected override void OnServerEnter(GameStateManager manager)
        {
            base.OnServerEnter(manager);

            // 重设所有玩家的 turn 状态
            var currentPlayer = manager.players[manager.currentPlayerIndex];
            foreach (var player in manager.players)
            {
                player.isOwnersTurn = player.guid == currentPlayer.guid;
            }
        }
    }
}