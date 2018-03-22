using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TwoStickClassicExample
{
    // ComponentSystem is a class that can work with both 
    // components and GameObjects*
    public class RemoveDeadSystem : ComponentSystem
    {
        // The component group needed to work with this system
        public struct Entities
        {
            // The Length of the componentgroup**
            public int Length;

            // The Game object associated with the entity
            // Restricts the group to GameObject based Entities.
            public GameObjectArray gameObjects;
            
            // Lets us access entities with a health component.
            public ComponentArray<Health> healths;
        }

        // A struct to determine if there is still a player in the game 
        struct PlayerCheck
        {
            public int Length;
            [ReadOnly] public ComponentArray<PlayerInput> PlayerInput;
        }

        // Component group injection gives us a component group that can be accessed by index
        // eg. entities.health[0]
        [Inject] private PlayerCheck m_PlayerCheck;
        [Inject] private Entities entities;

        protected override void OnUpdate()
        {
            // See if we have any players
            var playerDead = m_PlayerCheck.Length == 0;

            // List to hold any GameObject to be destroyed
            var toDestroy = new List<GameObject>();

            // Out of Bounds Checking
            var settings = TwoStickBootstrap.Settings;
            var minY = settings.playfield.yMin;
            var maxY = settings.playfield.yMax;

            // Iterate over all entities matching the declared ComponentGroup required types
            for (int i = 0; i < entities.Length; i++)
            {
                var position = entities.gameObjects[i].GetComponent<Transform2D>().Position;
                // Logic from Health.cs and Enemy.cs
                if (entities.healths[i].Value <= 0 || playerDead || position.y > maxY || position.y < minY)
                {
                    toDestroy.Add(entities.gameObjects[i]);
                }
            }

            // Destroy all objects that we need to
            foreach (var go in toDestroy)
            {
                Object.Destroy(go);
            }
        }
    }
}