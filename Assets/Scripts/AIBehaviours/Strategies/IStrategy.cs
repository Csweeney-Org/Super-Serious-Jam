using System.Collections.Generic;
using UnityEngine.AI;

namespace Assets.Scripts.AIBehaviours.Strategies
{
    public interface IStrategy
    {
        public IPointOfInterest Execute(Dictionary<IPointOfInterest, NavMeshPath> possibleTargets, SpinCharacterController self);
    }
}