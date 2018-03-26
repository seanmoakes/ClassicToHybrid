using Unity.Mathematics;
using UnityEngine;

namespace TwoStickClassicExample
{

    public class PlayerInput : MonoBehaviour
    {
        [HideInInspector] public float2 Move;
        [HideInInspector] public float2 Shoot;
        [HideInInspector] public float FireCooldown;

        public bool Fire => FireCooldown <= 0.0 && math.length(Shoot) > 0.5f;

        //private void Update()
        //{            
        //    var settings = TwoStickBootstrap.Settings;

        //    GetComponent<Position2D>().Value += Time.deltaTime * Move * settings.playerMoveSpeed;

        //    if (Fire)
        //    {
        //        GetComponent<Heading2D>().Value = math.normalize(Shoot);

        //        FireCooldown = settings.playerFireCoolDown;
                
        //        var newShotData = new ShotSpawnData()
        //        {
        //            Position = GetComponent<Position2D>().Value,
        //            Heading = GetComponent<Heading2D>().Value,
        //            Faction = GetComponent<Faction>()
        //        };
                
        //        ShotSpawnSystem.SpawnShot(newShotData);
        //    }
        //}
    }
}
