namespace FishONU.GamePlay.GameState
{
    public class GameStateManager
    {
        public GameState CurrentState { get; private set; }

        public void ChangeState(GameState newState)
        {
            if (CurrentState != null) CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }
    }
}