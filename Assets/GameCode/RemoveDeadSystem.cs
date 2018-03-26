using System.Collections.Generic;
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

            var toDestroy = new List<GameObject>();
            var isPlayerDead = m_PlayerCheck.Length == 0;

            for (int i = 0; i < entities.Length; i++)
            {
                if (entities.healths[i].Value <= 0 || isPlayerDead)
                {
                    toDestroy.Add(entities.gameObjects[i]);
                }
            }

            foreach (var go in toDestroy)
            {
                Object.Destroy(go);
            }
        }
    }
}