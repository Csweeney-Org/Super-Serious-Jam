using Assets.Scripts.Throwables;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float LaunchForce = 10f;
    public BoingData BoingProperties;

    public Rigidbody RB;
    public Collider[] Colliders;
    public MeshFilter MeshFilter;
    public void LaunchFrom(Vector3 launchPosition, Vector3 forward)
    {
        RB.position = launchPosition; //This can have unpredictable results if we teleport this inside of another collider. 
        foreach (Collider col in Colliders)
        {
            col.enabled = true;
        }
        RB.isKinematic = false;
        RB.linearVelocity = Vector3.zero;
        RB.angularVelocity = Vector3.zero;

        RB.AddForce(forward * LaunchForce);
    }
    public void SetupForItem(ItemPickup item)
    {
        BoingProperties = item.BoingProperties;
        this.MeshFilter.sharedMesh = item.Mesh.sharedMesh;
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
        //All human/AI player objects will have an inventory I guess
        SpinCharacterController collidedUnit = collision.gameObject.GetComponentInParent<SpinCharacterController>();

        if (collidedUnit == null)
        {
            //Collided with something that is not a player/npc
            Debug.Log("No se encontró SpinCharacterController");
            return;
        }

        BOING(collidedUnit);
    }

    private void BOING(SpinCharacterController hitUnit)
    {
        print("BOING!");

        //RB.linearVelocity = Vector3.zero;
        //RB.angularVelocity = Vector3.zero;

        Vector3 directionRandom = new Vector3(
            Random.Range(-1f, 1f),
            1f,
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
            DisableItemColliders();
            Destroy(this, 1f); //Replace with return to projectile pool
        }
    }

}

