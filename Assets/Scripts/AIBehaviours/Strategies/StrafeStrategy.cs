using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Throwables;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.AIBehaviours.Strategies
{
    public class StrafeStrategy : IStrategy
    {
        private Transform playerTransform;
        private float optimalDistance = 10f; // Distance AI wants to maintain
        
        public StrafeStrategy(Transform player) => playerTransform = player;

        public IPointOfInterest Execute(Dictionary<IPointOfInterest, NavMeshPath> possibleTargets, SpinCharacterController self)
        {
            // Find an item or node that keeps distance but engageable 
            var itemTargets = possibleTargets.Keys.OfType<PointOfInterest<ItemPickup>>().ToList();
            
            if (itemTargets.Count > 0)
            {
                return itemTargets.OrderBy(item => 
                    Mathf.Abs(Vector3.Distance(item.Position, playerTransform.position) - 
                    optimalDistance)).First();
            }
            return possibleTargets.Keys.First(); // Fallback
        }
    }
}
