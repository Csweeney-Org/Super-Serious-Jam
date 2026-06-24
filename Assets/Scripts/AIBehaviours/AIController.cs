using Assets.Scripts.AIBehaviours.Strategies;
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

        public float MaxPathingErrorDistance = 0.5f;

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

        public void Start()
        {
            //TODO: Listener needs to be unsubscribed in OnDisable
            BattleEvents.OnItemPickedUp += ProcessItemRemovedFromMap;
            playerUnit = FindObjectsByType<SpinCharacterController>(FindObjectsSortMode.None).FirstOrDefault(unit => unit != selfUnit);

            EvaluateStateAndStrategy(); // Should default to Allrounder state
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
            
            // Logic for performing movement 
            if (Vector3.Distance(selfUnit.transform.position, CurrentPath.corners[cornersIndex]) <= MaxPathingErrorDistance)
            {
                cornersIndex = cornersIndex + 1;//If we are close to a path node, don't worry about hitting it perfectly and start aiming for the next one
                if (cornersIndex < CurrentPath.corners.Length)
                {
                    print($"{selfUnit} is sufficiently close to navMesh node {CurrentPath.corners[cornersIndex - 1]}. Now aiming for next position {CurrentPath.corners[cornersIndex]}");
                }
                else
                {
                    //Ran out of nnodes to aim for -> Time to recalc priorities
                    AssessNewTarget();
                }
            }
            selfUnit.ApplyMovementForce(CurrentPath.corners[cornersIndex] - selfUnit.transform.position);
            //TODO: Some extra cases that are not yet solved in this code.
            // 1) What happens when this unit is knocked back to be closer to the previous node
            // 2) While pickup items have static position, other spinning units are moving targets. Maybe the generic Point of Interest script actually needs to resolve position dynamically for player units
            // 3) More advanced planning involving multiple look ahead steps is left to another day to solve - if we get there :)

            HandleCombat();
        }

        public void AssessNewTarget()
        {
            var possibleTargetPaths = navMap.GenerateNavPathsToPointsOfInterest(navMap.SearchForPointsOfInterest());
            CurrentTarget = Strategy.Execute(possibleTargetPaths, selfUnit);
            CurrentPath = possibleTargetPaths[CurrentTarget];
            cornersIndex = 0;
            //Should probably have some exception handling if any part of this fails

            if (CurrentTarget is PointOfInterest<ItemPickup> itemTarget)
            {
                print($"{selfUnit.gameObject.name} AI has selected a new item target: {itemTarget.Value.name} @ {itemTarget.Position} ");

            }
            else if (CurrentTarget is PointOfInterest<SpinCharacterController> spinUnitTarget)
            {
                print($"{selfUnit.gameObject.name} AI has selected a new spining unit target to ram: {spinUnitTarget.Value.name} @ {spinUnitTarget.Position}");
            }
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
            Vector3 vectorToPlayer = playerUnit.transform.position - selfUnit.transform.position;
            Vector3 directionToPlayer = vectorToPlayer.normalized;

            // Projectiles are flying slow -> AI needs to lead it's shot
            // Inventory already holds a reference of the aimcontroller
            float alignment = Vector3.Dot(selfUnit.Inventory.aimController.CurrentAimDirection, directionToPlayer);

            switch (CurrentState)
            { // TODO: Get rid of magic numbers and include variables
                case AIState.Strafe:
                    if(alignment > 0.92f)
                        selfUnit.Inventory.ThrowItemFromInventory();
                    break;
                
                case AIState.Steamroll:
                    // Throw if player runs away
                    bool playerIsEscaping = Vector3.Dot(playerUnit.rigidBody.linearVelocity,
                                                        directionToPlayer) > 0.5f;
                    if (playerIsEscaping && alignment > 0.95f) 
                        selfUnit.Inventory.ThrowItemFromInventory();
                    break;
                
                case AIState.Allrounder:
                    if (alignment > 0.98f) 
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
