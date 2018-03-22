using Unity.Mathematics;
using UnityEngine;

namespace TwoStickClassicExample
{

    public class Enemy : MonoBehaviour
    {
        private void Update()
        {

            var player = FindObjectOfType<PlayerInput>();
            if (!player)
                return;
         
            // Shooting
            var playerPos = player.GetComponent<Transform2D>().Position;

            var state = GetComponent<EnemyShootState>();

            state.Cooldown -= Time.deltaTime;
            
            if (state.Cooldown <= 0.0)
            {
                state.Cooldown = TwoStickBootstrap.Settings.enemyShootRate;
                var position = GetComponent<Transform2D>().Position;

                ShotSpawnData spawn = new ShotSpawnData()
                {
                    Position = position,
                    Heading = math.normalize(playerPos - position),
                    Faction = TwoStickBootstrap.Settings.EnemyFaction
                };
                ShotSpawnSystem.SpawnShot(spawn);
            }
        }
    }
}
