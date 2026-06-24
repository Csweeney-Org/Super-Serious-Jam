using Assets.Scripts.Throwables;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.AIBehaviours
{
    public class NavMap : MonoBehaviour
    {
        public bool DrawGizmos = true;
        public Dictionary<IPointOfInterest, NavMeshPath> Paths = new Dictionary<IPointOfInterest, NavMeshPath>(30);
        private static readonly int NavMeshAgentMask = 1 << 0 | 1 << 2;

        public IEnumerable<IPointOfInterest> SearchForPointsOfInterest()
        {
            //Linq makes this quite simple to search and combine - but linq has heap allocations. Replace with Zlinq later - a non-allocating implementation for Unity
            //https://github.com/Cysharp/ZLinq
            var players = GameObject.FindObjectsByType<SpinCharacterController>(FindObjectsSortMode.None)
                   .Select(playerunit => new PointOfInterest<SpinCharacterController>(playerunit) as IPointOfInterest);
            var pickups = GameObject.FindObjectsByType<ItemPickup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(item => item.IsAvailableToPickUp)
                .Select(pickup => new PointOfInterest<ItemPickup>(pickup) as IPointOfInterest);

            return players.Union(pickups);
        }
        public Dictionary<IPointOfInterest, NavMeshPath> GenerateNavPathsToPointsOfInterest(IEnumerable<IPointOfInterest> pointsOfInterest)
        {
            var currentPosition = this.transform.position;
            Paths.Clear();
            foreach (var pointOfInterest in pointsOfInterest)
            {
                //TODO: Probably want to pool and reuse NavMeshPath objects instead of allocating more memory
                Paths[pointOfInterest] = new NavMeshPath();
                NavMesh.CalculatePath(
                    currentPosition,
                    pointOfInterest.Position,
                    NavMeshAgentMask,
                    Paths[pointOfInterest]
                    );
            }
            return Paths;
        }

        public void OnDrawGizmos()
        {
            if (!DrawGizmos) return;
            var allPaths = Paths.ToArray();
            for (int i = 0; i < allPaths.Length; i++)
            {
                DrawGizmoForPath(allPaths[i].Value);
            }
        }
        private void DrawGizmoForPath(NavMeshPath path)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
                Gizmos.DrawSphere(path.corners[i + 1], 0.25f);
            }
        }
    }

    public struct PointOfInterest<T> : IPointOfInterest where T : MonoBehaviour
    {
        public Type Type { get; private set; }
        public T Value { get; private set; }
        public Vector3 Position { get; private set; }

        public PointOfInterest(T obj)
        {
            Value = obj;
            Position = obj.transform.position;
            Type = typeof(T);
        }
    }
    public interface IPointOfInterest
    {
        Type Type { get; }
        Vector3 Position { get; }
    }
}
