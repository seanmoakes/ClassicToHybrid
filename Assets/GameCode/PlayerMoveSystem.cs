using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TwoStickClassicExample
{
    public class PlayerMoveSystem : ComponentSystem
    {
        public struct Data
        {
            public int Length;
            public GameObjectArray GameObject;
            public ComponentArray<Transform2D> Transform;
            public ComponentArray<PlayerInput> Input;
        }

        [Inject] private Data m_Data;

        protected override void OnUpdate()
        {
            if (m_Data.Length == 0)
                return;

            var settings = TwoStickBootstrap.Settings;

            float dt = Time.deltaTime;
            var firingPlayers = new List<GameObject>();

            // Loop through all players to update transforms and list players that are firing shots
            for (int index = 0; index < m_Data.Length; ++index)
            {
                var transform = m_Data.Transform[index];
                var playerInput = m_Data.Input[index];

                transform.Position += dt * playerInput.Move * settings.playerMoveSpeed;

                if (playerInput.Fire)
                {
                    transform.Heading = math.normalize(playerInput.Shoot);
                    playerInput.FireCooldown = settings.playerFireCoolDown;

                    firingPlayers.Add(m_Data.GameObject[index]);
                }
            }

            // Loop the firing players to init shots from their position
            foreach (var player in firingPlayers)
            {
                var newShotData = new ShotSpawnData()
                {
                    Position = player.GetComponent<Transform2D>().Position,
                    Heading = player.GetComponent<Transform2D>().Heading,
                    Faction = player.GetComponent<Faction>()
                };

                ShotSpawnSystem.SpawnShot(newShotData);
            }
        }
    }
}