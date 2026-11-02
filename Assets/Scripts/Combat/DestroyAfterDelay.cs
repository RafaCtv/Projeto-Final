using UnityEngine;

public sealed class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField, Min(0)] private float delay = 0.4f;

    private void Start() => Destroy(gameObject, delay);
}
