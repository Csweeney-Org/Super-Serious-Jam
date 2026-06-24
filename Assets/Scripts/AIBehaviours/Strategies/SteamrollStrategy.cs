using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.AIBehaviours.Strategies
{
public class SteamrollStrategy : IStrategy
{
    private SpinCharacterController targetPlayer;
    public SteamrollStrategy(SpinCharacterController player) => targetPlayer = player;

    public IPointOfInterest Execute(Dictionary<IPointOfInterest, NavMeshPath> possibleTargets, SpinCharacterController self)
    {
        // Ram directly into the player (currently does not do any damage)
        var playerTarget = possibleTargets.Keys.FirstOrDefault(t => t.Type == typeof(SpinCharacterController) &&
                                                                t.Position == targetPlayer.transform.position);
        
        return playerTarget;
    }
}
}
