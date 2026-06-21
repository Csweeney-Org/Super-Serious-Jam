using UnityEngine;

public interface ICollidable
{
    public float Mass { get; }
    public float Speed { get; }
    public float Inertia { get; } //This is like speed of the spin, and provides a kind of extra energy resistance to being moved by other objects. Not sure if we want this?
    public void OnTriggerEnter(Collider other);
    public void OnTriggerExit(Collider other);
}
