using System.Collections.Generic;
using UnityEngine;

public sealed class TurretController : MonoBehaviour
{
    [SerializeField] private Projectile projectile;
    [SerializeField] private Transform muzzle;
    [SerializeField, Min(1)] private float range = 20;
    [SerializeField, Min(1)] private int damage = 15;
    [SerializeField, Min(0.05f)] private float shotInterval = 0.6f;
    private GameManager game;
    private float nextShot;
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    public ZombieController Target { get; private set; }
    public void Initialize(GameManager session) => game = session;
    private void Update()
    {
        if (game == null || !game.IsRunning) return;
        Target = FindTarget();
        if (Target == null || Time.time < nextShot) return;
        nextShot = Time.time + shotInterval;
        var shot = Instantiate(projectile, muzzle.position, Quaternion.identity, game.ProjectileRoot);
        shot.Initialize(Target.AimPosition - (Vector2)muzzle.position, damage, game);
    }
    private ZombieController FindTarget()
    {
        overlaps.Clear(); var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(1 << 9);
        Physics2D.OverlapCircle(muzzle.position, range, filter, overlaps);
        ZombieController best = null;
        foreach (var hit in overlaps)
        {
            var zombie = hit.GetComponentInParent<ZombieController>();
            if (zombie != null && zombie.IsAlive && (best == null || zombie.transform.position.x < best.transform.position.x)) best = zombie;
        }
        return best;
    }
}
