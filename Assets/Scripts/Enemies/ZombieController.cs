using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public sealed class ZombieController : MonoBehaviour
{
    [SerializeField] private ZombieData data;
    [SerializeField] private TargetSensor attackSensor;
    [SerializeField] private Animator animator;
    [Tooltip("Transforms espelhados para virar o zumbi (visual e área de ataque). Vazio = animator + sensor.")]
    [SerializeField] private Transform[] facingPivots;
    private static readonly int IsWalkingParameter = Animator.StringToHash("IsWalking");
    private Rigidbody2D body;
    private bool stopped;
    private Health health;
    private GameManager game;
    private Collider2D baseCollider;
    private ZombieBrain brain;
    private Vector3[] pivotScales;
    private float nextAttack;
    public int AttackDamage { get; private set; }
    public ZombieData Data => data;
    public Health Health => health;
    public ZombieGoal Goal => brain != null ? brain.Goal : ZombieGoal.Base;
    // A arte original olha para a esquerda (-1).
    public float Facing => brain != null ? brain.Facing : -1;
    public bool IsAlive => !stopped && health != null && health.IsAlive;
    public Vector2 AimPosition => transform.Find("AimPoint").position;
    public event Action<ZombieController> Defeated;
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        if (health != null && data != null) health.Initialize(data.MaxHealth);
        AttackDamage = data != null ? data.Damage : 0;
        brain = new ZombieBrain(data);
        CachePivots();
    }
    private void OnEnable() { if (health != null) health.Died += OnDied; }
    private void OnDisable() { if (health != null) health.Died -= OnDied; }
    public void Initialize(GameManager session, int waveNumber = 1)
    {
        game = session;
        baseCollider = game != null && game.Base != null ? game.Base.GetComponent<Collider2D>() : null;
        if (baseCollider != null) brain.FaceTowards(transform.position.x, baseCollider.bounds.center.x);
        ApplyFacing();
        if (data == null || health == null) return;
        int growth = Mathf.Max(0, waveNumber - 1);
        health.Initialize(Mathf.CeilToInt(data.MaxHealth * (1 + .15f * growth)));
        AttackDamage = Mathf.CeilToInt(data.Damage * (1 + .1f * growth));
    }
    private void FixedUpdate()
    {
        if (stopped || data == null || (game != null && !game.IsRunning)) return;
        Health target = FindTarget();
        float direction = target != null ? 0 : Think();
        body.linearVelocity = new Vector2(direction * data.Speed, body.linearVelocity.y);
        ApplyFacing();
        if (animator != null) animator.SetBool(IsWalkingParameter, Mathf.Abs(body.linearVelocityX) > Mathf.Epsilon);
        if (target != null && Time.time >= nextAttack)
        {
            nextAttack = Time.time + data.AttackInterval;
            target.TakeDamage(DamageFor(target));
        }
    }
    private float Think()
    {
        Health player = game != null ? game.Player : null;
        bool playerAvailable = player != null && player.IsAlive && player.isActiveAndEnabled;
        Vector2 playerPosition = playerAvailable ? (Vector2)player.transform.position : Vector2.zero;
        // Sem base conhecida, mantém o comportamento antigo: segue para a esquerda.
        float minX = baseCollider != null ? baseCollider.bounds.min.x : float.NegativeInfinity;
        float maxX = baseCollider != null ? baseCollider.bounds.max.x : float.NegativeInfinity;
        return brain.Decide(transform.position, playerAvailable, playerPosition, minX, maxX);
    }
    public int DamageFor(Health target)
    {
        var kind = ZombieTargets.Classify(target);
        if (kind == ZombieTargetKind.None || data == null) return 0;
        return Mathf.Max(1, Mathf.CeilToInt(AttackDamage * data.DamageMultiplier(kind)));
    }
    private Health FindTarget() => attackSensor != null ? attackSensor.FindTarget(ZombieTargets.IsAttackable) : null;

    private void CachePivots()
    {
        if (facingPivots == null || facingPivots.Length == 0)
            facingPivots = new[] { animator != null ? animator.transform : null, attackSensor != null ? attackSensor.transform : null };
        pivotScales = new Vector3[facingPivots.Length];
        for (int i = 0; i < facingPivots.Length; i++)
            if (facingPivots[i] != null) pivotScales[i] = facingPivots[i].localScale;
    }
    // Espelha os pivôs: a escala original corresponde a olhar para a esquerda.
    private void ApplyFacing()
    {
        float sign = Facing > 0 ? -1 : 1;
        for (int i = 0; i < facingPivots.Length; i++)
        {
            if (facingPivots[i] == null) continue;
            var scale = pivotScales[i];
            facingPivots[i].localScale = new Vector3(Mathf.Abs(scale.x) * sign, scale.y, scale.z);
        }
    }
    private void OnDied(Health value)
    {
        if (stopped) return;
        StopWithoutReward();
        foreach (var collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        if (game != null && data != null) game.AwardCoins(data.CoinReward);
        Defeated?.Invoke(this);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    public void StopWithoutReward()
    {
        stopped = true;
        if (body != null) body.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetBool(IsWalkingParameter, false);
    }
    public void DespawnWithoutReward()
    {
        StopWithoutReward(); gameObject.SetActive(false); Destroy(gameObject);
    }
    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = new Color(1, .3f, .2f, .8f);
        Gizmos.DrawWireCube(transform.position, new Vector3(data.PlayerDetectionRange * 2, data.VerticalDetectionRange * 2, 0));
        Gizmos.color = new Color(1, .7f, .2f, .5f);
        Gizmos.DrawWireCube(transform.position, new Vector3(data.PlayerLoseRange * 2, data.VerticalDetectionRange * 2, 0));
    }
}
