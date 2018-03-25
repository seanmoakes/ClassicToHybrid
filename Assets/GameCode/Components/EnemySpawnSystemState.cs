namespace TwoStickClassicExample
{
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;

    public class EnemySpawnSystemState : MonoBehaviour
    {
        public int SpawnedEnemyCount;
        public float Cooldown;
        public Random.State RandomState;
    }
}