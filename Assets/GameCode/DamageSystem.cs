using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TwoStickClassicExample
{
    public class PlayerDamageSystem : ComponentSystem
    {

        public struct ReceiverData
        {
            public int Length;
            public ComponentArray<Health> Health;
            public ComponentArray<Faction> Faction;
            public ComponentArray<Position2D> Position;
        }

        [Inject] ReceiverData m_Receivers;

        public struct ShotData
        {
            public int Length;
            public ComponentArray<Shot> Shot;
            public ComponentArray<Position2D> Position;
            public ComponentArray<Faction> Faction;
        }
        [Inject] ShotData m_Shots;

        protected override void OnUpdate()
        {
            // Collision
            var settings = TwoStickBootstrap.Settings;

            //var receivers = FindObjectsOfType(typeof(Health));

            //var faction = GetComponent<Faction>().Value;


            // foreach (Health health in receivers)
            for(int pIndex = 0; pIndex< m_Receivers.Length; ++pIndex)
            {
                float damage = 0.0f;

                // var receiverFaction = health.GetComponent<Faction>().Value;
                Faction.Type receiverFaction = m_Receivers.Faction[pIndex].Value;

                float collisionRadius = GetCollisionRadius(settings, receiverFaction);
                float collisionRadiusSquared = collisionRadius * collisionRadius;

                // float2 receiverPos = health.GetComponent<Position2D>().Value;
                float2 receiverPos = m_Receivers.Position[pIndex].Value;

                for (int sIndex = 0; sIndex < m_Shots.Length; sIndex++)
                {
                    // if (faction != receiverFaction)
                    if(m_Shots.Faction[sIndex].Value != receiverFaction)
                    {
                        // float2 shotPos = GetComponent<Position2D>().Value;
                        float2 shotPos = m_Shots.Position[sIndex].Value;
                        float2 delta = shotPos - receiverPos;
                        float distSquared = math.dot(delta, delta);

                        if (distSquared <= collisionRadiusSquared)
                        {
                            var shot = m_Shots.Shot[sIndex];
                            
                            // health.Value = health.Value - Energy;
                            damage += shot.Energy;
                            // Set the shot's time to live to zero, so it will be collected by the shot destroy system 
                            // Destroy(gameObject);
                            // break;
                            shot.TimeToLive = 0.0f;
                        }
                    }
                }
                var h = m_Receivers.Health[pIndex];
                h.Value = math.max(h.Value - damage, 0.0f);
            }
        }

        static float GetCollisionRadius(TwoStickExampleSettings settings, Faction.Type faction)
        {
            // This simply picks the collision radius based on whether the receiver is the player or not. 
            // In a real game, this would be much more sophisticated, perhaps with a CollisionRadius component. 
            return faction == Faction.Type.Player ? settings.playerCollisionRadius : settings.enemyCollisionRadius;
        }
    }
}