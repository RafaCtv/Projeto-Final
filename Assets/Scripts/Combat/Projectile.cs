using UnityEngine;

public sealed class Projectile : MonoBehaviour
{
    [SerializeField, Min(1)] private float speed = 24;
    [SerializeField, Min(0.1f)] private float lifetime = 3;
    private Vector2 direction;
    private int damage;
    private float expires;
    private GameManager game;
    private bool spent;
    public void Initialize(Vector2 heading, int amount, GameManager session)
    {
        direction = heading.normalized; damage = amount; game = session;
        expires = Time.time + lifetime;
        transform.right = direction;
    }
    private void FixedUpdate()
    {
        if (spent) return;
        if (Time.time >= expires || game == null || !game.IsRunning) { Remove(); return; }
        float distance = speed * Time.fixedDeltaTime;
        var hit = Physics2D.Raycast(transform.position, direction, distance, 1 << 9);
        if (hit.collider != null)
        {
            var zombie = hit.collider.GetComponentInParent<ZombieController>();
            if (zombie != null && zombie.IsAlive) zombie.Health.TakeDamage(damage);
            Remove(); return;
        }
        transform.position += (Vector3)(direction * distance);
    }
    private void Remove() { spent = true; gameObject.SetActive(false); Destroy(gameObject); }
}
