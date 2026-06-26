using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Throwables;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.AIBehaviours.Strategies
{
    public class AllrounderStrategy : IStrategy
    {
        private Transform playerTransform;

        public AllrounderStrategy(Transform player) 
        {
            playerTransform = player;
        }

        public IPointOfInterest Execute(Dictionary<IPointOfInterest, NavMeshPath> possibleTargets, SpinCharacterController self)
        {
            var playerTarget = possibleTargets.Keys.FirstOrDefault(t => t.Type == typeof(SpinCharacterController));
            var items = possibleTargets.Keys.OfType<PointOfInterest<ItemPickup>>().ToList();

            // Rule 1: If our inventory is full, or there are no items left, hunt the player immediately.
            if (self.Inventory.IsFull || items.Count == 0) 
                return playerTarget;

            // Rule 2: Find the absolute closest item to us
            var closestItem = items.OrderBy(i => Vector3.Distance(self.transform.position, i.Position)).First();

            // Rule 3: Compare distances. 
            float distanceToPlayer = Vector3.Distance(self.transform.position, playerTransform.position);
            float distanceToItem = Vector3.Distance(self.transform.position, closestItem.Position);

            // If the player is dangerously close (within 6 units), prioritize engaging the player. 
            // Otherwise, safely collect the closest item.
            if (distanceToPlayer < 6f && distanceToPlayer < distanceToItem)
            {
                return playerTarget;
            }

            return closestItem;
        }
    }
}
