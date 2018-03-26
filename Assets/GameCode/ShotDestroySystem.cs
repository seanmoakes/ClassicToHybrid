using Unity.Collections;
using Unity.Entities;
using Unity.Transforms2D;
using UnityEngine;

namespace TwoStickClassicExample
{
    [UpdateAfter(typeof(ShotSpawnSystem))]
    [UpdateAfter(typeof(MoveForward2DSystem))]
    public class ShotDestroySystem : ComponentSystem
    {
        struct Data
        {
            public Shot Shot;
        }

        struct PlayerCheck
        {
            public int Length;
            [ReadOnly] public ComponentArray<PlayerInput> PlayerInput;
        }

        [Inject] private PlayerCheck m_PlayerCheck;

        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}