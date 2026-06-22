using UnityEngine;

public class SpinCharacterController : MonoBehaviour, ICollidable
{
    public Rigidbody rigidBody;
    public ThrowableInventory Inventory;

    [Header("Initialization Properties")]
    [field: SerializeField] public float maxSpeed { get; private set; }
    [field: SerializeField] public float maxTurnRate { get; private set; }
    [field: SerializeField] public Vector3 Velocity { get; private set; } = Vector3.zero;

    [Header("Live Dynamic Properties")]
    [field: SerializeField] public float Mass { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float Inertia { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction">Assumes pre-normalized for performance</param>
    /// <param name="magnitude"></param>
    public void ApplyForce(Vector3 direction, float magnitude = 1f)
    {
        rigidBody.AddForce(direction * magnitude, ForceMode.Force);
    }
    /// <summary>
    /// Helper method to ApplyForce, but this is specifically for player/AI controlled movement inputs 
    /// where the magnitude of the force should be the characters acceleration
    /// </summary>
    /// <param name="direction"></param>
    public void ApplyMovementForce(Vector3 direction)
    {
        if (rigidBody.linearVelocity.magnitude < maxSpeed || Vector3.Dot(direction, rigidBody.linearVelocity) < 0)
        {
            //Prevent further player input driven force unless it is in a direction that will result in slower velocity
            ApplyForce(direction, Speed);
        }
        else
        {
            //This might get spammy, enable it when you need to debug
            Debug.Log($"Movement has been input for controller {gameObject.name} but further movement would exceed maximum speed. Ignoring");
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        // Handle collision logic here
    }
    public void OnTriggerExit(Collider other)
    {
        // Handle collision exit logic here
    }

    public void OnValidate()
    {
        Inventory ??= GetComponent<ThrowableInventory>();
    }
}
