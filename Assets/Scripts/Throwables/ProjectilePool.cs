using Assets.Scripts.Throwables;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    public Projectile ProjectilePrefab;
    private ObjectPool<Projectile> Pool;

    private void Awake()
    {
        Pool = new ObjectPool<Projectile>(
            createFunc: () => GameObject.Instantiate<Projectile>(ProjectilePrefab),
            actionOnGet: (proj) => { },
            actionOnRelease: (proj) => { },
            actionOnDestroy: (proj) => { },
            defaultCapacity: 20
            );
    }

    public Projectile GetProjectileForItem(ItemPickup item)
    {
        var projectile = Pool.Get();
        projectile.SetupForItem(item);
        return projectile;
    }
    public void ReturnProjectile(Projectile returningProjectile)
    {
        Pool.Release(returningProjectile);
    }
    public void OnValidate()
    {
        if (ProjectilePrefab == null) Debug.LogError($"Projectile Pool is missing a link to the projectile prefab");
    }
}