using ProjetoFinal.Audio;
using UnityEngine;

// Munição largada por zumbis. Coletada ao encostar; some depois de um tempo (piscando no fim).
[RequireComponent(typeof(Collider2D))]
public sealed class AmmoPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int amount = 8;
    [SerializeField, Min(1)] private float lifetime = 20;
    [SerializeField, Min(0)] private float blinkDuration = 4;
    [SerializeField] private Transform visual;
    [SerializeField] private AudioClip pickupSound;
    private float expiresAt;
    private Vector3 visualStart;
    private SpriteRenderer[] renderers;
    public int Amount => amount;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        expiresAt = Time.time + lifetime;
        if (visual != null) visualStart = visual.localPosition;
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void Initialize(int bullets) => amount = Mathf.Max(1, bullets);

    private void Update()
    {
        float remaining = expiresAt - Time.time;
        if (remaining <= 0) { Destroy(gameObject); return; }
        if (visual != null) visual.localPosition = visualStart + Vector3.up * (Mathf.Sin(Time.time * 4) * 0.06f);
        bool visible = remaining > blinkDuration || Mathf.Repeat(remaining, 0.3f) > 0.12f;
        foreach (var r in renderers) if (r != null) r.enabled = visible;
    }

    // Stay: se a reserva estava cheia, coleta assim que houver espaço.
    private void OnTriggerStay2D(Collider2D other) => TryCollect(other);
    private void OnTriggerEnter2D(Collider2D other) => TryCollect(other);

    private void TryCollect(Collider2D other)
    {
        var combat = other.GetComponentInParent<PlayerCombat>();
        if (combat == null || !combat.isActiveAndEnabled) return;
        int added = combat.AddAmmo(amount);
        if (added <= 0) return;
        amount -= added;
        if (amount > 0) return;
        SoundEffects.Play(pickupSound);
        Destroy(gameObject);
    }
}
