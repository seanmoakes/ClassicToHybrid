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

            /*
            SM - Allows us to create and add to a list of FiringPlayers
            */
            public GameObjectArray GameObject;

            /*
            SM - Player Components that we need to access. The Player Input
            component means this system operates only on players.
            */
            public ComponentArray<Position2D> Position;
            public ComponentArray<Heading2D> Heading;
            public ComponentArray<PlayerInput> Input;
        }

        // Inject the above into the array m_Data.
        [Inject] private Data m_Data;

        protected override void OnUpdate()
        {
            /*
             SM -Including Length gives a convenient way to check if
             we need to continue.
             */
            if (m_Data.Length == 0)
                return;

            var settings = TwoStickBootstrap.Settings;

            /*
             SM - Moving Time.deltaTime outside of loops is more efficient,
             and means all items in the loop receive the same value for deltaTime.
            */
            float dt = Time.deltaTime;
            var firingPlayers = new List<GameObject>();

            for (int index = 0; index < m_Data.Length; ++index)
            {
                var position = m_Data.Position[index];
                var heading = m_Data.Heading[index];

                var playerInput = m_Data.Input[index];

                //GetComponent<Position2D>().Value += Time.deltaTime * Move * settings.playerMoveSpeed;
                position.Value += dt * playerInput.Move * settings.playerMoveSpeed;

                //if(Fire)
                if (playerInput.Fire)
                {
                    // GetComponent<Heading2D>().Value = math.normalize(Shoot);
                    heading.Value = math.normalize(playerInput.Shoot);

                    // FireCooldown = settings.playerFireCoolDown;
                    playerInput.FireCooldown = settings.playerFireCoolDown;

                    firingPlayers.Add(m_Data.GameObject[index]);
                }
            }

            foreach (var player in firingPlayers)
            {
                var newShotData = new ShotSpawnData()
                {
                    Position = player.GetComponent<Position2D>().Value,
                    Heading = player.GetComponent<Heading2D>().Value,
                    Faction = player.GetComponent<Faction>()
                };

                ShotSpawnSystem.SpawnShot(newShotData);
            }
        }
    }
}