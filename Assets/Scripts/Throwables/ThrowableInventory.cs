using Assets.Scripts.Throwables;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.CharactrerControllers;

public class ThrowableInventory : MonoBehaviour
{
    [Header("Espacios donde se guardan")]
    public Transform[] CarryPositions;
    public SpinCharacterController Owner;
    public AimController aimController;
    private Queue<ItemPickup> throwableQueue = new Queue<ItemPickup>();
    [field: SerializeField] private ProjectilePool projectilePool;

    private void Start()
    {
        projectilePool ??= GameObject.FindAnyObjectByType<ProjectilePool>();
        if (projectilePool == null) Debug.LogError($"{gameObject.name} Expected to resolve a projectile pool in the current scene, but did not.");
    }

    public bool TryPickupItem(ItemPickup newItem)
    {
        if (throwableQueue.Count >= CarryPositions.Length)
        {
            Debug.Log("No hay espacios disponibles");
            return false;
        }

        throwableQueue.Enqueue(newItem);
        //Set transform of the picked up Item as a child of this unit
        int index = throwableQueue.Count - 1;
        newItem.transform.SetParent(CarryPositions[index]);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localRotation = Quaternion.identity;
        print($"{Owner.name} picked up item {newItem.name}");
        return true;
    }

    /// <summary>
    /// Throws the first object in inventory queue. Does nothing if inventory is empty
    /// </summary>
    public void ThrowItemFromInventory()
    {
        if (throwableQueue.Count == 0) return;
        projectilePool.GetProjectileForItem(throwableQueue.Dequeue())
            .LaunchFrom(this.transform.position + this.transform.forward, aimController.CurrentAimDirection);
        //Fire from slightly ahead of thrower to prevent early self collisions
        //ReorganizeQueue();
    }

    private void ReorganizeQueue()
    {
        //Aeryj: I am not sure what this function was doing sorry, please let me know :)
        ItemPickup[] objects = throwableQueue.ToArray();
        for (int i = 0; i < objects.Length; i++)
        {
            //objects[i].AttachToPoint(CarryPositions[i]);
        }
    }

    public void OnValidate()
    {
        if (CarryPositions.Length == 0)
        {
            Debug.LogError($"Inventory for {gameObject.name} expects to have at least one transform provided to attach items to");
        }
        if (Owner == null)
        {
            Debug.LogError($"Inventory {gameObject.name} does not have an owner ");
        }
        if (aimController == null)
        {
            aimController = gameObject.GetComponentInChildren<AimController>();
        }
    }
}

