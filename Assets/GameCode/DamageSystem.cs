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
            if (0 == m_Receivers.Length || 0 == m_Shots.Length)
                return;

            var settings = TwoStickBootstrap.Settings;

            for(int pIndex = 0; pIndex< m_Receivers.Length; ++pIndex)
            {
                float damage = 0.0f;

                Faction.Type receiverFaction = m_Receivers.Faction[pIndex].Value;
                float2 receiverPos = m_Receivers.Position[pIndex].Value;

                float collisionRadius = GetCollisionRadius(settings, receiverFaction);
                float collisionRadiusSquared = collisionRadius * collisionRadius;

                for (int sIndex = 0; sIndex < m_Shots.Length; sIndex++)
                {
                    if(m_Shots.Faction[sIndex].Value != receiverFaction)
                    {
                        float2 shotPos = m_Shots.Position[sIndex].Value;
                        float2 delta = shotPos - receiverPos;
                        float distSquared = math.dot(delta, delta);

                        if (distSquared <= collisionRadiusSquared)
                        {
                            var shot = m_Shots.Shot[sIndex];                            
                            damage += shot.Energy;
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