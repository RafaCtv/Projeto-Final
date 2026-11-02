using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public sealed class ZombieController : MonoBehaviour
{
    [SerializeField] private ZombieData data;
    [SerializeField] private TargetSensor attackSensor;
    [SerializeField] private Animator animator;
    private static readonly int IsWalkingParameter = Animator.StringToHash("IsWalking");
    private Rigidbody2D body;
    private bool stopped;
    private Health health;
    private GameManager game;
    private float nextAttack;
    public ZombieData Data => data;
    public Health Health => health;
    public bool IsAlive => !stopped && health != null && health.IsAlive;
    public Vector2 AimPosition => transform.Find("AimPoint").position;
    public event Action<ZombieController> Defeated;
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        if (health != null && data != null) health.Initialize(data.MaxHealth);
    }
    private void OnEnable() { if (health != null) health.Died += OnDied; }
    private void OnDisable() { if (health != null) health.Died -= OnDied; }
    public void Initialize(GameManager session) => game = session;
    private void FixedUpdate()
    {
        if (stopped || data == null || (game != null && !game.IsRunning)) return;
        Health target = FindTarget();
        body.linearVelocity = new Vector2(target != null ? 0 : -data.Speed, body.linearVelocity.y);
        if (animator != null) animator.SetBool(IsWalkingParameter, Mathf.Abs(body.linearVelocityX) > Mathf.Epsilon);
        if (target != null && Time.time >= nextAttack)
        {
            nextAttack = Time.time + data.AttackInterval;
            target.TakeDamage(data.Damage);
        }
    }
    private Health FindTarget() => attackSensor != null ? attackSensor.FindTarget(IsAttackable) : null;

    private static bool IsAttackable(Health value) =>
        value.GetComponent<BuildingInstance>() != null || value.GetComponent<BaseCore>() != null || value.GetComponent<PlayerController>() != null;
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
}
