using UnityEngine;

// A Layer Collision Matrix só deixa a layer Projectile tocar a layer Zombie.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class Projectile : MonoBehaviour
{
    [SerializeField, Min(1)] private float speed = 24;
    [SerializeField, Min(0.1f)] private float lifetime = 3;
    [SerializeField] private GameObject hitEffect;
    private Rigidbody2D body;
    private int damage;
    private GameManager game;
    private bool spent;

    private void Awake() => body = GetComponent<Rigidbody2D>();

    public void Initialize(Vector2 heading, int amount, GameManager session)
    {
        var direction = heading.normalized;
        damage = amount; game = session;
        transform.right = direction;
        body.linearVelocity = direction * speed;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!spent && game != null && !game.IsRunning) Remove();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spent) return;
        var zombie = other.GetComponentInParent<ZombieController>();
        if (zombie == null || !zombie.IsAlive) return;
        zombie.Health.TakeDamage(damage);
        if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
        Remove();
    }

    private void Remove() { spent = true; gameObject.SetActive(false); Destroy(gameObject); }
}
