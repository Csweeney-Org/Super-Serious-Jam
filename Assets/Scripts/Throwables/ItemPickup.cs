using UnityEngine;

namespace Assets.Scripts.Throwables
{
    public class ItemPickup : MonoBehaviour
    {
        public BoingData BoingProperties = new BoingData(5, 3, 5, true, 50, 20); // BounceStrength, HorizontalForce, RotationForce, CanBounce, Damage, Weight
        public MeshFilter Mesh;
        public Collider PickupCollider;
        public AK.Wwise.Event Item_Pickup;
        public bool IsAvailableToPickUp { get; private set; } = true;

        public float Weight;

        //TODO: Might need to have public weight, damage variables

        public void OnTriggerEnter(Collider other)
        {
            var collidingUnit = other.GetComponentInParent<ThrowableInventory>();
            if (collidingUnit == null) return;
            Weight = BoingProperties.Weight;

            if (collidingUnit.TryPickupItem(this))
            {
                PickupCollider.enabled = false;
                IsAvailableToPickUp = false;
                BattleEvents.InvokeItemPickedUpEvent(this);
                AkUnitySoundEngine.PostEvent("Item_Pickup", gameObject);
            }
        }

        /// <summary>
        /// Intended to be used after an item is thrown, but before it reappears on the map
        /// </summary>
        public void HideItem()
        {
            this.gameObject.SetActive(false);
        }

        public void DeployToPosition(Vector3 position)
        {
            IsAvailableToPickUp = true;
            PickupCollider.enabled = true;
            this.transform.position = position;
            this.transform.localScale = Vector3.one;
            this.gameObject.SetActive(true);
        }


        public void OnValidate()
        {
            PickupCollider ??= GetComponentInChildren<Collider>();
            if (PickupCollider == null || !PickupCollider.isTrigger)
            {
                Debug.LogError($"Expected to find a trigger collider for ItemPickup {this.name}");
            }
            Mesh ??= GetComponentInChildren<MeshFilter>();
            if (Mesh == null)
            {
                Debug.LogError($"Expected to find a mesh filterr for ItemPickup {this.name}");
            }
        }
    }
}
