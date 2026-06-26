using Assets.Scripts.AIBehaviours.Strategies;
using Assets.Scripts.Throwables;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.AIBehaviours
{
    public class AllrounderStrategy : IStrategy
    {
        private Transform playerTransform;
        
        // Just like the other magic numbers chosen by pure chance
        private const float PreferredEngagementDistance = 7f;

        public AllrounderStrategy(Transform player) 
        {
            playerTransform = player;
        }

        public IPointOfInterest Execute(Dictionary<IPointOfInterest, NavMeshPath> possibleTargets, SpinCharacterController self)
        {
            IPointOfInterest bestTarget = null;
            float bestScore = float.MaxValue; 

            foreach (var kvp in possibleTargets) // kvp = key-value-pair
            {
                var target = kvp.Key;
                float distanceToSelf = Vector3.Distance(self.transform.position, target.Position);
                
                float score = distanceToSelf;

                if (target.Type == typeof(ItemPickup))
                    //TODO: Remove magic number
                    score *= 0.9f; 

                else if (target.Type == typeof(SpinCharacterController))
                {
                    if (self.Inventory.TotalWeight <= 0)
                        // Not much weight -> we do not want to engage close up (might be ok toleave out)
                        //TODO: Remove magic number
                        score *= 5.0f; 
                    else
                    {
                        // Apply some pressure to the player but keep collecting items
                        if (distanceToSelf < PreferredEngagementDistance)
                        //TODO: Remove magic number
                            score *= 2.0f;
                    }
                }
                // Keep track of the lowest score
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = target;
                }
            }
            // Fallback
            return bestTarget ?? possibleTargets.Keys.FirstOrDefault();
        }
    }
}
