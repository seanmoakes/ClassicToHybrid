using Unity.Entities;
using UnityEngine;

namespace TwoStickClassicExample
{
    public class EnemyShootSystem : ComponentSystem
    {
        public struct Data
        {
            public int Length;
            public ComponentArray<Position2D> Position;
            public ComponentArray<EnemyShootState> ShootState;
        }

        [Inject] private Data m_Data;

        public struct PlayerData
        {
            public int Length;
            public ComponentArray<Position2D> Position;
            public ComponentArray<PlayerInput> PlayerInput;
        }

        [Inject] private PlayerData m_Player;

        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}