using ProjetoFinal.Input;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField, Min(0)] private float moveSpeed = 4;
    [SerializeField] private Transform visual;
    [SerializeField] private Animator animator;

    [Header("Corrida")]
    [SerializeField, Min(1)] private float sprintMultiplier = 1.6f;
    [SerializeField, Min(1)] private float maxStamina = 100;
    [SerializeField, Min(0)] private float staminaDrainPerSecond = 30;
    [SerializeField, Min(0)] private float staminaRegenPerSecond = 22;
    [Tooltip("Pausa antes de a estamina começar a recuperar.")]
    [SerializeField, Min(0)] private float staminaRegenDelay = 0.8f;
    [Tooltip("Depois de zerar, só volta a correr ao recuperar esta quantidade.")]
    [SerializeField, Min(0)] private float minimumStaminaToSprint = 25;

    private static readonly int IsRunningParameter = Animator.StringToHash("IsRunning");
    private static readonly int IsSprintingParameter = Animator.StringToHash("IsSprinting");
    private Rigidbody2D body;
    private InputAction move, sprint;
    private float horizontal;
    private bool sprinting;
    private Vector3 visualScale;
    public float FacingDirection => visual != null && visual.localScale.x < 0 ? -1 : 1;
    public Stamina Stamina { get; private set; }
    public bool IsSprinting => sprinting;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (visual != null) visualScale = visual.localScale;
        Stamina = new Stamina(maxStamina, staminaDrainPerSecond, staminaRegenPerSecond, staminaRegenDelay, minimumStaminaToSprint);
        // Ações compartilhadas: respeitam o remapeamento da tela de Controles.
        move = GameInput.Gameplay.FindAction("Move", true);
        sprint = GameInput.Gameplay.FindAction("Sprint", true);
    }

    private void OnEnable() { move?.Enable(); sprint?.Enable(); }

    private void Update()
    {
        float x = move.ReadValue<Vector2>().x;
        horizontal = Mathf.Abs(x) > 0.01f ? Mathf.Sign(x) : 0;
        if (horizontal != 0 && visual != null)
            visual.localScale = new Vector3(Mathf.Abs(visualScale.x) * horizontal, visualScale.y, visualScale.z);
    }

    private void FixedUpdate()
    {
        sprinting = Stamina.Tick(Time.fixedDeltaTime, sprint.IsPressed(), horizontal != 0);
        float speed = moveSpeed * (sprinting ? sprintMultiplier : 1);
        // Gravity and contact resolution retain control of the vertical axis.
        body.linearVelocity = new Vector2(horizontal * speed, body.linearVelocity.y);
        if (animator != null)
        {
            animator.SetBool(IsRunningParameter, Mathf.Abs(body.linearVelocityX) > Mathf.Epsilon);
            animator.SetBool(IsSprintingParameter, sprinting);
        }
    }

    private void OnDisable()
    {
        move?.Disable(); sprint?.Disable();
        horizontal = 0; sprinting = false;
        if (body != null) body.linearVelocity = new Vector2(0, body.linearVelocity.y);
        if (animator != null) { animator.SetBool(IsRunningParameter, false); animator.SetBool(IsSprintingParameter, false); }
    }
}
