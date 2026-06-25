using Assets.Scripts.AIBehaviours.Strategies;
using Assets.Scripts.CharactrerControllers;
using Assets.Scripts.Throwables;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace Assets.Scripts.AIBehaviours
{
        public enum AIState
    {
        Survival,       // Weight low -> flee and grab items
        Steamroll,      // Weight advantage -> engage and steamroll
        Strafe,         // Too heavy -> strafe and throw
        Allrounder      // Default behavior -> try to damage and pick up weight
    }

    internal class AIController : MonoBehaviour
    {
        [Header("Input Properties")]
        public NavMap navMap;
        public SpinCharacterController selfUnit;
        public IStrategy Strategy; //Currently this needs to be set in code until we think of a way to get it wrapped in a ScriptableObject for editor drag and drop

        public float MaxPathingErrorDistance = 1.5f;

        [Header("AI Thresholds")]
        [Tooltip("Health percentage below which AI enters Survival state")]
        public float CriticalWeightThreshold = 20f; 
        [Tooltip("Weight above which AI is to slow and enters rapid fire mode")]
        public float OverweightThreshold = 100f;
        [Tooltip("Weight difference required to attempt steamrolling the player")]
        public float SteamrollWeightAdvantage = 80f;

        [Header("Dynamic Outputs")]
        public AIState CurrentState;
        public int cornersIndex;
        public IPointOfInterest CurrentTarget;
        public NavMeshPath CurrentPath;

        private SpinCharacterController playerUnit;
        private float stateEvaluationTimer = 0f;
        private const float StateEvaluationInterval = 0.5f; //Not sure if that is too often
        private Vector3 lastPosition; // AI gets stuck alot, so here goes nothing
        private float stuckTimer = 0f;
        private IPointOfInterest blacklistedTarget;
        private float blacklistTimer = 0f;

        public void Start()
        {
            //TODO: Listener needs to be unsubscribed in OnDisable
            BattleEvents.OnItemPickedUp += ProcessItemRemovedFromMap;
            playerUnit = FindObjectsByType<SpinCharacterController>(FindObjectsSortMode.None).FirstOrDefault(unit => unit != selfUnit);

            EvaluateStateAndStrategy(); // Should default to Allrounder state
            if (CurrentPath == null)
                AssessNewTarget();
        }
        public void FixedUpdate()
        {
            // Logic for State evaluation
            stateEvaluationTimer += Time.fixedDeltaTime;
            if (stateEvaluationTimer >= StateEvaluationInterval)
            {
                EvaluateStateAndStrategy();
                stateEvaluationTimer = 0f;
            }

            // Combat needs to be handled first -> can at least shoot if navigation is stuck
            HandleCombat();

            if (blacklistTimer > 0)
            {
                blacklistTimer -= Time.fixedDeltaTime;
                if (blacklistTimer <= 0) 
                    blacklistedTarget = null;
            }

            if (selfUnit == null || CurrentPath == null || CurrentPath.corners.Length == 0 || 
                    cornersIndex >= CurrentPath.corners.Length)
            {
                AssessNewTarget();
                return;
            }

            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= 0.5f) 
            {
                if (Vector3.Distance(selfUnit.transform.position, lastPosition) < 0.2f)
                {
                    Debug.LogWarning($"{selfUnit.name} got stuck! Nudging and picking new target.");
                    
                    // Blacklist offending target (will be removed after some time)
                    blacklistedTarget = CurrentTarget;
                    blacklistTimer = 3f;

                    // If AI is stuck, slighly push & asses new target
                    selfUnit.rigidBody.AddForce(-selfUnit.transform.forward * 5f, ForceMode.Impulse);

                    AssessNewTarget();
                    stuckTimer = 0f;
                    return; 
                }
                lastPosition = selfUnit.transform.position;
                stuckTimer = 0f;
            }
            
            if (Vector3.Distance(selfUnit.transform.position, CurrentPath.corners[cornersIndex]) <= MaxPathingErrorDistance)
            {
                cornersIndex = cornersIndex + 1;
                if (cornersIndex >= CurrentPath.corners.Length)
                {
                    AssessNewTarget();
                    return;
                }
            }
            
            selfUnit.ApplyMovementForce(CurrentPath.corners[cornersIndex] - selfUnit.transform.position);
            
            //TODO: Some extra cases that are not yet solved in this code.
            // 1) What happens when this unit is knocked back to be closer to the previous node
            // 2) While pickup items have static position, other spinning units are moving targets. Maybe the generic Point of Interest script actually needs to resolve position dynamically for player units
            // 3) More advanced planning involving multiple look ahead steps is left to another day to solve - if we get there :)
        }

        public void AssessNewTarget()
        {
            var pointsOfInterest = navMap.SearchForPointsOfInterest();
            if (pointsOfInterest == null || !pointsOfInterest.Any()) return;

            var possibleTargetPaths = navMap.GenerateNavPathsToPointsOfInterest(pointsOfInterest);

            // Ignore target that leads to dead end (getting stuck)
            if (blacklistedTarget != null && possibleTargetPaths.ContainsKey(blacklistedTarget))
                possibleTargetPaths.Remove(blacklistedTarget);
            
            if (possibleTargetPaths.Count == 0) return;

            CurrentTarget = Strategy.Execute(possibleTargetPaths, selfUnit);
            
            if (CurrentTarget != null && possibleTargetPaths.ContainsKey(CurrentTarget))
            {
                CurrentPath = possibleTargetPaths[CurrentTarget];
                cornersIndex = CurrentPath.corners.Length > 1 ? 1 : 0; 
            }
            else
                CurrentPath = null;
            
        }

        private void EvaluateStateAndStrategy()
        {
            if (playerUnit == null || selfUnit.CurrentToppleHealth <= 0)
                return;

            AIState previousState = CurrentState;

            if (selfUnit.CurrentToppleHealth < selfUnit.MaxToppleHealth ||
                selfUnit.Inventory.TotalWeight <= CriticalWeightThreshold)
            {
                CurrentState = AIState.Survival;
                Strategy = new SurvivalStrategy(playerUnit.transform);
            }
            else if (selfUnit.Inventory.TotalWeight > playerUnit.Inventory.TotalWeight + 
                                                        SteamrollWeightAdvantage)
            {
                CurrentState = AIState.Steamroll;
                Strategy = new SteamrollStrategy(playerUnit);
            }
            else if (selfUnit.Inventory.TotalWeight >= OverweightThreshold)
            {
                CurrentState = AIState.Strafe;
                Strategy = new StrafeStrategy(playerUnit.transform);
            }
            else
            {
                CurrentState = AIState.Allrounder;
                Strategy = new AllrounderStrategy(playerUnit.transform);
            }

            // Force new calculation immediatly after state change
            if (previousState != CurrentState)
                AssessNewTarget();
        }

        private void HandleCombat()
        {
            if (selfUnit.Inventory.TotalWeight <= 0) 
                return;

            AimController aim = selfUnit.Inventory.aimController;

            switch (CurrentState)
            { // TODO: Get rid of magic numbers and include variables
                case AIState.Strafe:
                    // EvaluateTarget checks line of sight as well as predicting next player position
                    if(aim.EvaluateTarget(selfUnit, playerUnit, 0.85f))
                        selfUnit.Inventory.ThrowItemFromInventory();
                    break;
                
                case AIState.Steamroll:
                    // Throw if player runs away
                    Vector3 dirToPlayer = (playerUnit.transform.position - selfUnit.transform.position).normalized;
                    bool playerIsEscaping = Vector3.Dot(playerUnit.rigidBody.linearVelocity,
                                                        dirToPlayer) > 0.5f;
                    if (playerIsEscaping && aim.EvaluateTarget(selfUnit, playerUnit, 0.90f)) 
                        selfUnit.Inventory.ThrowItemFromInventory();
                    break;
                
                case AIState.Allrounder:
                    if (aim.EvaluateTarget(selfUnit, playerUnit, 0.90f)) 
                        selfUnit.Inventory.ThrowItemFromInventory();
                    break;
                
                case AIState.Survival:
                    break;
            }
        }

        private void ProcessItemRemovedFromMap(ItemPickup eventItem)
        {
            if (CurrentTarget is PointOfInterest<ItemPickup> targetItem && targetItem.Value == eventItem)
            {
                //The item this AI was moving towards is no longer accessable, reassess behaviour
                AssessNewTarget();
            }
        }

        public void OnValidate()
        {
            navMap ??= GetComponent<NavMap>();
            if (navMap == null)
            {
                Debug.LogError($"Expected to find a navMap on gameobject {this.gameObject.name}", this);
            }
            selfUnit ??= GetComponentInParent<SpinCharacterController>();
            if (selfUnit == null)
            {
                Debug.LogError($"Expected to find a SpinCharacterController for AIController {this.gameObject.name}", this);
            }
        }
    }
}
