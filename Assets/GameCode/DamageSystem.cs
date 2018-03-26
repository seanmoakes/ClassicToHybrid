using Unity.Entities;
using UnityEngine;

namespace TwoStickClassicExample
{
    public class PlayerDamageSystem : ComponentSystem
    {

        public struct ReceiverData
        {
            public int Length;
            public ComponentArray<Health> Health;
            public ComponentArray<Faction> Faction;
            public ComponentArray<Position2D> Position;
        }

        [Inject] ReceiverData m_Receivers;

        public struct ShotData
        {
            public int Length;
            public ComponentArray<Shot> Shot;
            public ComponentArray<Position2D> Position;
            public ComponentArray<Faction> Faction;
        }
        [Inject] ShotData m_Shots;

        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}