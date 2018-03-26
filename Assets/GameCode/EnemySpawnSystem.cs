using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TwoStickClassicExample
{
    // Spawns new enemies.
    // public class EnemySpawnSystem : MonoBehaviour
    public class EnemySpawnSystem : ComponentSystem
    {
        //public int SpawnedEnemyCount;
        //public float Cooldown;
        //public Random.State RandomState;

        public struct State
        {
            public int Length;
            public ComponentArray<EnemySpawnSystemState> S;
        }

        [Inject] private State m_State;

        //void Start()
        public static void SetupComponentData()
        {
            var oldState = Random.state;
            Random.InitState(0xaf77);

            var state = TwoStickBootstrap.Settings.EnemySpawnState;

            //Cooldown = 0.0f;
            state.Cooldown = 0.0f;

            //SpawnedEnemyCount = 0;
            state.SpawnedEnemyCount = 0;

            //RandomState = Random.state;
            state.RandomState = Random.state;

            Random.state = oldState;
        }

        // protected void Update()
        protected override void OnUpdate()
        {
            var state = m_State.S[0];

            var oldState = Random.state;
            
            //Random.state = RandomState;
            Random.state = state.RandomState;

            //Cooldown -= Time.deltaTime;
            state.Cooldown -= Time.deltaTime;

            //if (Cooldown <= 0.0f)
            if (state.Cooldown <= 0.0f)
            {
                var settings = TwoStickBootstrap.Settings;
                var enemy = Object.Instantiate(settings.EnemyPrefab);

                ComputeSpawnLocation(enemy);

                //SpawnedEnemyCount++;
                state.SpawnedEnemyCount++;

                //Cooldown = ComputeCooldown(SpawnedEnemyCount);
                state.Cooldown = ComputeCooldown(state.SpawnedEnemyCount);

            }

            //RandomState = Random.state;
            state.RandomState = Random.state;
            Random.state = oldState;
        }

        private float ComputeCooldown(int stateSpawnedEnemyCount)
        {
            return 0.15f;
        }

        private void ComputeSpawnLocation(GameObject enemy)
        {
            var settings = TwoStickBootstrap.Settings;
            
            float r = Random.value;
            float x0 = settings.playfield.xMin;
            float x1 = settings.playfield.xMax;
            float x = x0 + (x1 - x0) * r;

            enemy.GetComponent<Position2D>().Value = new float2(x, settings.playfield.yMax);
            enemy.GetComponent<Heading2D>().Value = new float2(0, -TwoStickBootstrap.Settings.enemySpeed);
        }
    }

}
