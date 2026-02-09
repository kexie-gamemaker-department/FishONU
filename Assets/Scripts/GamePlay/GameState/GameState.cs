using Unity.VisualScripting;
using UnityEngine.PlayerLoop;

namespace FishONU.GamePlay.GameState
{
    public abstract class GameState
    {
        protected GameStateManager Manager { get; set; }

        public virtual void Initialize(GameStateManager manager)
        {
            Manager = manager;
        }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void Update()
        {
        }
    }
}