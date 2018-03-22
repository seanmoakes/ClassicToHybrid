using Unity.Entities;
using UnityEngine;

namespace TwoStickClassicExample
{
    public class PlayerInputSystem : ComponentSystem
    {
        struct PlayerData
        {
            public PlayerInput Input;
        }
        // No need for code injection when simply looping through entities

        protected override void OnUpdate()
        {   
            // Get Time.deltaTime outside of the loop to prevent multiple calls
            float dt = Time.deltaTime;

            // GetEntities<T>() gives us the ComponentGroup matching the components
            // in struct 'T'
            foreach (var entity in GetEntities<PlayerData>())
            {
                var pi = entity.Input;

                // From PlayerInput's update method
                pi.Move.x = Input.GetAxis("Horizontal");
                pi.Move.y = Input.GetAxis("Vertical");
                pi.Shoot.x = Input.GetAxis("ShootX");
                pi.Shoot.y = Input.GetAxis("ShootY");
                
                pi.FireCooldown = Mathf.Max(0.0f, pi.FireCooldown - dt);
            }
        }
    }
}