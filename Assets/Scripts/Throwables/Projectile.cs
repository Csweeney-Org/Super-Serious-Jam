using Assets.Scripts.Throwables;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float LaunchForce = 10f;
    public BoingData BoingProperties;

    public Rigidbody RB;
    public Collider[] Colliders;
    public MeshFilter MeshFilter;

    private ItemPickup linkedItem;
    private SpinCharacterController shooterUnit;
    private ProjectilePool parentPool;

    private bool Launched;

    public void LaunchFrom(Vector3 launchPosition, Vector3 forward, SpinCharacterController shooter)
    {
        shooterUnit = shooter;

        RB.position = launchPosition; //This can have unpredictable results if we teleport this inside of another collider. 
        foreach (Collider col in Colliders)
            col.enabled = true;

        RB.isKinematic = false;
        RB.linearVelocity = Vector3.zero;
        RB.angularVelocity = Vector3.zero;
        Launched = true;
        RB.AddForce(forward * LaunchForce); // ForceMode.Impulse makes weirdly fast push
    }
    public void SetupForItem(ItemPickup item)
    {
        linkedItem = item;
        BoingProperties = item.BoingProperties;
        this.MeshFilter.sharedMesh = item.Mesh.sharedMesh;
    }
    public Projectile RegisterWithPool(ProjectilePool pool)
    {
        parentPool = pool;
        return this;
    }
    public void EnableItemColliders()
    {
        foreach (Collider col in Colliders)
        {
            col.enabled = true;
        }
    }
    public void DisableItemColliders()
    {
        foreach (Collider col in Colliders)
        {
            col.enabled = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!Launched) return;
        //All human/AI player objects will have an inventory I guess
        SpinCharacterController collidedUnit = collision.gameObject.GetComponentInParent<SpinCharacterController>();

        // Prevent selfcollision
        if (collidedUnit != null)
        {
            if (collidedUnit == shooterUnit) return;
            BOING(collidedUnit);

        }
        else if (collidedUnit == null)
        {
            //Collided with something that is not a player/npc -> drop as item
            DropItemAndExpire();
            return;
        }

    }

    private void BOING(SpinCharacterController hitUnit)
    {
        print($"BOING! {gameObject.name} has hit {hitUnit.name}");

        //RB.linearVelocity = Vector3.zero;
        //RB.angularVelocity = Vector3.zero;

        //TODO: Add differentiation between player/enemy and walls/environment (no damage)
        hitUnit.TakeDamage(BoingProperties.Damage);

        Vector3 directionRandom = new Vector3(
            Random.Range(-1f, 1f),
            0.5f,
            Random.Range(-1f, 1f)
        ).normalized;

        hitUnit.ApplyForce(
            directionRandom * BoingProperties.HorizontalForce +
            Vector3.up * BoingProperties.BounceStrength
        );

        /* We do nott yet have torque implemented
        rb.AddTorque(
            Random.insideUnitSphere * rotationForce,
            ForceMode.Impulse
        );
        */

        if (BoingProperties.CanBounce)
        {
            //TODO: This item could live on for another collision
            BoingProperties = BoingData.WithoutBounce(BoingProperties);
        }
        else
        {
            //No bounce configure, expire this item
            DropItemAndExpire();
        }
    }

    private void DropItemAndExpire()
    {
        Launched = false;
        // Move original object ro current location
        if (linkedItem != null)
        {
            linkedItem.transform.SetParent(null);
            linkedItem.DeployToPosition(new Vector3(transform.position.x, 0, transform.position.z));
        }
        parentPool.ReturnProjectile(this);
    }

}

