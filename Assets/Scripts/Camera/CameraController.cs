using UnityEngine;
using UnityEngine.InputSystem;

// Move o alvo da CinemachineCamera; suavização e limites ficam no Cinemachine.
public sealed class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float mapMinX = -20;
    [SerializeField] private float mapMaxX = 70;
    [SerializeField, Min(0)] private float manualSpeed = 12;
    private InputAction pan, recenter;
    public bool IsManual { get; private set; }
    public bool InputEnabled { get; set; } = true;

    private void Awake()
    {
        pan = new InputAction("CameraPan", InputActionType.Value);
        pan.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
        recenter = new InputAction("CameraHome", InputActionType.Button, "<Keyboard>/home");
    }
    private void Start() { if (target != null) SetX(target.position.x); }
    private void OnEnable() { pan?.Enable(); recenter?.Enable(); }
    private void LateUpdate()
    {
        if (!InputEnabled) return;
        float input = pan.ReadValue<float>();
        if (recenter.WasPressedThisFrame()) IsManual = false;
        else if (Mathf.Abs(input) > 0.01f)
        {
            IsManual = true;
            SetX(transform.position.x + input * manualSpeed * Time.deltaTime);
            return;
        }
        if (!IsManual && target != null) SetX(target.position.x);
    }
    // Limita o alvo à faixa que a câmera consegue mostrar, para o pan manual não acumular além da borda.
    private float ClampX(float x)
    {
        var view = Camera.main;
        float halfWidth = view != null ? view.orthographicSize * view.aspect : 0;
        if (halfWidth * 2 >= mapMaxX - mapMinX) return (mapMinX + mapMaxX) * 0.5f;
        return Mathf.Clamp(x, mapMinX + halfWidth, mapMaxX - halfWidth);
    }
    private void SetX(float x) => transform.position = new Vector3(ClampX(x), transform.position.y, transform.position.z);
    private void OnDisable() { pan?.Disable(); recenter?.Disable(); }
    private void OnDestroy() { pan?.Dispose(); recenter?.Dispose(); }
}
