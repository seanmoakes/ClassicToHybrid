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

        private void Update()
        {          
            var settings = TwoStickBootstrap.Settings;

            Transform2D xform = GetComponent<Transform2D>();

            xform.Position += Time.deltaTime * Move * settings.playerMoveSpeed;

            if (Fire)
            {
                xform.Heading = math.normalize(Shoot);
                FireCooldown = settings.playerFireCoolDown;
                
                var newShotData = new ShotSpawnData()
                {
                    Position = xform.Position,
                    Heading = xform.Heading,
                    Faction = xform.GetComponent<Faction>()
                };
                
                ShotSpawnSystem.SpawnShot(newShotData);
            }
        }
    }
}
