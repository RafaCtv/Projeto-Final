using ProjetoFinal.Input;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField, Min(0)] private float moveSpeed = 4;
    [SerializeField] private Transform visual;

    private Rigidbody2D body;
    private InputAction move;
    private float horizontal;
    private Vector3 visualScale;
    public float FacingDirection => visual != null && visual.localScale.x < 0 ? -1 : 1;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (visual != null) visualScale = visual.localScale;
        // Ação compartilhada do GameInput: respeita o remapeamento feito na tela de Controles.
        move = GameInput.Gameplay.FindAction("Move", true);
    }

    private void OnEnable() => move?.Enable();

    private void Update()
    {
        float x = move.ReadValue<Vector2>().x;
        horizontal = Mathf.Abs(x) > 0.01f ? Mathf.Sign(x) : 0;
        if (horizontal != 0 && visual != null)
            visual.localScale = new Vector3(Mathf.Abs(visualScale.x) * horizontal, visualScale.y, visualScale.z);
    }

    private void FixedUpdate()
    {
        // Gravity and contact resolution retain control of the vertical axis.
        body.linearVelocity = new Vector2(horizontal * moveSpeed, body.linearVelocity.y);
    }

    private void OnDisable()
    {
        move?.Disable();
        horizontal = 0;
        if (body != null) body.linearVelocity = new Vector2(0, body.linearVelocity.y);
    }
}
