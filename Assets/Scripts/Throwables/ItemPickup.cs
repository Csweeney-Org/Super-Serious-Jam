using UnityEngine;

namespace Assets.Scripts.Throwables
{
    public class ItemPickup : MonoBehaviour
    {
        public BoingData BoingProperties = new BoingData(5, 3, 5, true, 50, 20); // BounceStrength, HorizontalForce, RotationForce, CanBounce, Damage, Weight
        public MeshFilter Mesh;
        public Collider PickupCollider;

        public float Weight;

        //TODO: Might need to have public weight, damage variables

        public void OnTriggerEnter(Collider other)
        {
            var collidingUnit = other.GetComponentInChildren<ThrowableInventory>();
            if (collidingUnit == null) return;
            Weight = BoingProperties.Weight;

            if (collidingUnit.TryPickupItem(this))
            {
                PickupCollider.enabled = false;
            }
        }

        public void OnValidate()
        {
            PickupCollider ??= GetComponent<Collider>();
            if (PickupCollider == null || !PickupCollider.isTrigger)
            {
                Debug.LogError($"Expected to find a trigger collider for ItemPickup {this.name}");
            }
            Mesh ??= GetComponent<MeshFilter>();
            if (Mesh == null)
            {
                Debug.LogError($"Expected to find a mesh filterr for ItemPickup {this.name}");
            }
        }
    }
}
