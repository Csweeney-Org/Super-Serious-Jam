using Assets.Scripts.CharactrerControllers;
using Assets.Scripts.Throwables;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableInventory : MonoBehaviour
{
    [Header("Espacios donde se guardan")]
    public Transform[] CarryPositions;
    public SpinCharacterController Owner;
    public AimController aimController;
    private Queue<ItemPickup> throwableQueue = new Queue<ItemPickup>();
    [field: SerializeField] private ProjectilePool projectilePool;
    // Open question: do we want to sart from 0 weight or assign a base weight (initialize different enemies)

    // Script to add in sound effects to pull from
    public AK.Wwise.Event Item_Throw;
    public AK.Wwise.Event Item_Crash;
    public AK.Wwise.Event Player_Damage;
    [field: SerializeField] public float TotalWeight { get; private set; } = 0f;

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
        // Assumes weight property exists in BoingData
        TotalWeight += newItem.Weight;

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

        ItemPickup thrownItem = throwableQueue.Dequeue();
        TotalWeight -= thrownItem.Weight;
        // Safety check to avoid negative values entering damage calculation
        if (TotalWeight < 0f) TotalWeight = 0f;

        projectilePool.GetProjectileForItem(thrownItem)
            .LaunchFrom(this.transform.position + 2 * aimController.CurrentAimDirection, aimController.CurrentAimDirection, Owner);
        //Plays the throw SFX. Try finding out how to get it to play from the thrown object with attenuation
        Item_Throw.Post(gameObject);
        thrownItem.HideItem();  
        //Fire from slightly ahead of thrower to prevent early self collisions
        //ReorganizeQueue();
    }

    private void ReorganizeQueue()
    {
        //Aeryj: I am not sure what this function was doing sorry, please let me know :)
        //Enzo: I believe it was for flipping the order of items to be thrown
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

