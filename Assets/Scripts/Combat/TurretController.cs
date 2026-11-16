using System.Collections;
using System.Collections.Generic;
using ProjetoFinal.Audio;
using UnityEngine;

// Torreta automática: a cabeça gira até o zumbi mais próximo e atira sozinha.
public sealed class TurretController : MonoBehaviour
{
    [SerializeField] private Projectile projectile;
    [SerializeField] private Transform muzzle;
    [Tooltip("Parte que gira para mirar (a arte original aponta para a direita).")]
    [SerializeField] private Transform head;
    [SerializeField] private SpriteRenderer muzzleFlash;
    [SerializeField] private AudioClip shotSound;
    [SerializeField, Range(0, 1)] private float shotVolume = 0.45f;
    [SerializeField, Min(1)] private float range = 20;
    [SerializeField, Min(1)] private int damage = 15;
    [SerializeField, Min(0.05f)] private float shotInterval = 1.2f;
    [SerializeField, Min(1)] private float turnSpeed = 360;
    [Tooltip("Só atira quando a mira está a menos deste ângulo do alvo.")]
    [SerializeField, Range(1, 45)] private float aimTolerance = 12;
    [SerializeField, Min(0)] private float recoil = 0.12f;
    private GameManager game;
    private float nextShot;
    private float aimAngle;
    private Vector3 headRest, headScale;
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    public ZombieController Target { get; private set; }
    public void Initialize(GameManager session) => game = session;

    private void Awake()
    {
        if (head != null) { headRest = head.localPosition; headScale = head.localScale; }
        if (muzzleFlash != null) muzzleFlash.enabled = false;
    }

    private void Update()
    {
        if (game == null || !game.IsRunning) return;
        Target = FindTarget();
        if (Target == null) return;
        Vector2 toTarget = Target.AimPosition - (Vector2)(head != null ? head.position : muzzle.position);
        bool aimed = Aim(toTarget);
        if (!aimed || Time.time < nextShot) return;
        nextShot = Time.time + shotInterval;
        var shot = Instantiate(projectile, muzzle.position, Quaternion.identity, game.ProjectileRoot);
        shot.Initialize(Target.AimPosition - (Vector2)muzzle.position, damage, game);
        SoundEffects.PlayAt(shotSound, transform.position, shotVolume, 6, 20);
        StopCoroutine(nameof(Kick));
        StartCoroutine(nameof(Kick));
    }

    // Gira a cabeça em direção ao alvo; espelha quando o alvo está à esquerda para a arte não ficar de cabeça para baixo.
    private bool Aim(Vector2 direction)
    {
        float desired = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aimAngle = Mathf.MoveTowardsAngle(aimAngle, desired, turnSpeed * Time.deltaTime);
        if (head != null)
        {
            bool left = Mathf.Abs(Mathf.DeltaAngle(aimAngle, 0)) > 90;
            head.localScale = new Vector3(headScale.x, Mathf.Abs(headScale.y) * (left ? -1 : 1), headScale.z);
            head.localRotation = Quaternion.Euler(0, 0, aimAngle);
        }
        return Mathf.Abs(Mathf.DeltaAngle(aimAngle, desired)) <= aimTolerance;
    }

    private IEnumerator Kick()
    {
        if (muzzleFlash != null) muzzleFlash.enabled = true;
        var back = -(Vector3)(Quaternion.Euler(0, 0, aimAngle) * Vector3.right) * recoil;
        for (float t = 0; t < .12f; t += Time.deltaTime)
        {
            if (head != null) head.localPosition = headRest + back * (1 - t / .12f);
            if (muzzleFlash != null && t > .05f) muzzleFlash.enabled = false;
            yield return null;
        }
        if (head != null) head.localPosition = headRest;
        if (muzzleFlash != null) muzzleFlash.enabled = false;
    }

    // Zumbis chegam pelos dois lados: prioriza o mais próximo da torre.
    private ZombieController FindTarget()
    {
        overlaps.Clear(); var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(1 << 9);
        Physics2D.OverlapCircle(muzzle.position, range, filter, overlaps);
        ZombieController best = null;
        float bestDistance = float.PositiveInfinity;
        foreach (var hit in overlaps)
        {
            var zombie = hit.GetComponentInParent<ZombieController>();
            if (zombie == null || !zombie.IsAlive) continue;
            float distance = Mathf.Abs(zombie.transform.position.x - transform.position.x);
            if (distance < bestDistance) { best = zombie; bestDistance = distance; }
        }
        return best;
    }
}
