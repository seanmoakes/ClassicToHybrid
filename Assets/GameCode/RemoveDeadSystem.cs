using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TwoStickClassicExample
{
    public class RemoveDeadSystem : ComponentSystem
    {
        public struct Entities
        {
            public int Length;
            public GameObjectArray gameObjects;
            public ComponentArray<Health> healths;
        }

        struct PlayerCheck
        {
            public int Length;
            [ReadOnly] public ComponentArray<PlayerInput> PlayerInput;
        }

        [Inject] private PlayerCheck m_PlayerCheck;
        [Inject] private Entities entities;

        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}