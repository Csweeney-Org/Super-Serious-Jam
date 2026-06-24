using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Throwables;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.AIBehaviours.Strategies
{
    public class SurvivalStrategy : IStrategy
    {
        private Transform playerTransform;
        public SurvivalStrategy(Transform player) => playerTransform = player;

        public IPointOfInterest Execute(Dictionary<IPointOfInterest, NavMeshPath> possibleTargets, SpinCharacterController self)
        {
            // Filter out the player, look only for items
            var itemTargets = possibleTargets.Keys.OfType<PointOfInterest<ItemPickup>>().ToList();
            
            if (itemTargets.Count == 0) 
                return null;

            // Find the item that maximizes distance from the player while minimizing distance from self
            return itemTargets.OrderByDescending(item => 
                Vector3.Distance(item.Position, playerTransform.position) - 
                Vector3.Distance(item.Position, self.transform.position)
            ).First();
        }
    }
}
