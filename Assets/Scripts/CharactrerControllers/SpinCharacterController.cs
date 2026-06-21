using UnityEngine;

public class SpinCharacterController : MonoBehaviour, ICollidable
{
    [Header("Initialization Properties")]
    [field: SerializeField] public float maxSpeed { get; private set; }
    [field: SerializeField] public float maxTurnRate { get; private set; }
    [field: SerializeField] public Vector3 Velocity { get; private set; } = Vector3.zero;

    [Header("Live Dynamic Properties")]
    [field: SerializeField] public float Mass { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float Inertia { get; private set; }


    public void ApplyForce(Vector3 direction, float magnitude = 1f)
    {
        this.transform.position += (direction.normalized * magnitude);
    }

    public void OnTriggerEnter(Collider other)
    {
        // Handle collision logic here
    }
    public void OnTriggerExit(Collider other)
    {
        // Handle collision exit logic here
    }
}
