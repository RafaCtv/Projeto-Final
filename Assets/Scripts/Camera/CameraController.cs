using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public sealed class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float mapMinX = -20;
    [SerializeField] private float mapMaxX = 70;
    [SerializeField, Min(0.01f)] private float followSmoothTime = 0.25f;
    [SerializeField, Min(0)] private float manualSpeed = 12;
    private Camera view;
    private InputAction pan, recenter;
    private float velocity;
    public bool IsManual { get; private set; }
    public bool InputEnabled { get; set; } = true;

    private void Awake()
    {
        view = GetComponent<Camera>();
        pan = new InputAction("CameraPan", InputActionType.Value);
        pan.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
        recenter = new InputAction("CameraHome", InputActionType.Button, "<Keyboard>/home");
    }
    private void OnEnable() { pan?.Enable(); recenter?.Enable(); }
    private void LateUpdate()
    {
        if (!InputEnabled) return;
        float input = pan.ReadValue<float>();
        if (recenter.WasPressedThisFrame())
        {
            IsManual = false;
            velocity = 0;
            if (target != null) SetX(target.position.x);
        }
        else if (Mathf.Abs(input) > 0.01f)
        {
            IsManual = true;
            velocity = 0;
            SetX(transform.position.x + input * manualSpeed * Time.deltaTime);
        }
        else if (!IsManual && target != null)
            SetX(Mathf.SmoothDamp(transform.position.x, ClampX(target.position.x), ref velocity, followSmoothTime));
        else SetX(transform.position.x);
    }
    private float ClampX(float x)
    {
        float halfWidth = view.orthographicSize * view.aspect;
        if (halfWidth * 2 >= mapMaxX - mapMinX) return (mapMinX + mapMaxX) * 0.5f;
        return Mathf.Clamp(x, mapMinX + halfWidth, mapMaxX - halfWidth);
    }
    private void SetX(float x) => transform.position = new Vector3(ClampX(x), transform.position.y, transform.position.z);
    private void OnDisable() { pan?.Disable(); recenter?.Disable(); }
    private void OnDestroy() { pan?.Dispose(); recenter?.Dispose(); }
}
