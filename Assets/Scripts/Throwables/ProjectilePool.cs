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
            createFunc: CreateProjectile,
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
        projectile.gameObject.SetActive(true);
        return projectile;
    }
    public void ReturnProjectile(Projectile returningProjectile)
    {
        returningProjectile.RB.position = new Vector3(-100, -100, -100);
        returningProjectile.gameObject.SetActive(false);
        Pool.Release(returningProjectile);
    }
    public void OnValidate()
    {
        if (ProjectilePrefab == null) Debug.LogError($"Projectile Pool is missing a link to the projectile prefab");
    }

    private Projectile CreateProjectile()
    {
        return GameObject.Instantiate<Projectile>(ProjectilePrefab)
            .RegisterWithPool(this);
    }
}