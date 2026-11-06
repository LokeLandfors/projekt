using UnityEngine;

/// Put this on a projectile prefab (a small sphere with a Collider set to "Is Trigger").
public class EnemyProjectile : MonoBehaviour
{
    public float lifetime = 5f;

    Vector3 velocity;
    int damage;
    GameObject owner;

    public void Init(Vector3 vel, int dmg, GameObject shooter)
    {
        velocity = vel;
        damage = dmg;
        owner = shooter;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (owner != null && other.transform.IsChildOf(owner.transform)) return; // don't hit yourself
        if (other.GetComponentInParent<EnemyAI>() != null) return;              // no friendly fire

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target != null) target.TakeDamage(damage);

        Destroy(gameObject); // also dies when hitting walls
    }
}
