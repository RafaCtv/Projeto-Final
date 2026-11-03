using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public sealed class DamageFeedback : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(0, 1.8f);
    private Health health;
    private void OnEnable()
    {
        health = GetComponent<Health>();
        health.Damaged += Show;
    }
    private void OnDisable() { if (health != null) health.Damaged -= Show; }
    private void Show(Health target, int amount)
    {
        // Independent of the victim so lethal hits remain visible after despawn.
        var popup = new GameObject("DamageNumber");
        popup.transform.position = transform.position + (Vector3)offset;
        var text = popup.AddComponent<TextMeshPro>();
        text.text = "-" + amount;
        text.fontSize = 3;
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = new Vector2(3, .7f);
        text.color = GetComponent<ZombieController>() != null
            ? new Color(1, .88f, .55f) : new Color(1, .35f, .3f);
        text.renderer.sortingLayerName = "Foreground";
        text.renderer.sortingOrder = 110;
        popup.AddComponent<FloatingDamageNumber>();
    }
}

internal sealed class FloatingDamageNumber : MonoBehaviour
{
    private float age;
    private TMP_Text text;
    private void Awake() => text = GetComponent<TMP_Text>();
    private void Update()
    {
        age += Time.unscaledDeltaTime;
        transform.position += Vector3.up * (.85f * Time.unscaledDeltaTime);
        text.alpha = Mathf.Clamp01((.8f - age) / .35f);
        if (age >= .8f) Destroy(gameObject);
    }
}
