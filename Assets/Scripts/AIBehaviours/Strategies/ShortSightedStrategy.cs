using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.AIBehaviours.Strategies
{
    public class ShortSightedStrategy : IStrategy
    {
        public IPointOfInterest Execute(Dictionary<IPointOfInterest, NavMeshPath> possibleTargets, SpinCharacterController self)
        {
            return possibleTargets
                .Where(target => !(target.Key is PointOfInterest<SpinCharacterController> characterTarget && characterTarget.Value == self)) //Exclude aiming for yourself
                .OrderBy(target => Vector3.Distance(target.Key.Position, self.transform.position)) //Sort ascending
                .First() //Get closest point of interest
                .Key;
        }
    }
}
