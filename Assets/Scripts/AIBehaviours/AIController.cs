using Assets.Scripts.AIBehaviours.Strategies;
using Assets.Scripts.Throwables;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.AIBehaviours
{
    internal class AIController : MonoBehaviour
    {
        [Header("Input Properties")]
        public NavMap navMap;
        public SpinCharacterController selfUnit;
        public IStrategy Strategy; //Currently this needs to be set in code until we think of a way to get it wrapped in a ScriptableObject for editor drag and drop

        public float MaxPathingErrorDistance = 0.5f;

        [Header("Dynamic Outputs")]
        public int cornersIndex;
        public IPointOfInterest CurrentTarget;
        public NavMeshPath CurrentPath;

        public void Start()
        {
            BattleEvents.OnItemPickedUp += ProcessItemRemovedFromMap;
            Strategy = new ShortSightedStrategy();
            AssessNewTarget();
        }
        public void FixedUpdate()
        {
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
