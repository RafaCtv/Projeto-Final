using System.Collections;
using ProjetoFinal.Audio;
using UnityEngine;

// Reação visual/sonora ao levar dano: pisca, treme, solta um efeito e toca um som.
// O mesmo componente serve para o jogador (carne) e estruturas (impacto metálico), mudando só a configuração.
[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public sealed class HitReaction : MonoBehaviour
{
    [Tooltip("Transform que treme; vazio = o próprio objeto (cuidado com corpos físicos).")]
    [SerializeField] private Transform shakeTarget;
    [SerializeField, Min(0)] private float shakeAmount = 0.08f;
    [SerializeField, Min(0.01f)] private float duration = 0.18f;
    [SerializeField] private Color flashColor = new Color(1, .35f, .3f);
    [Tooltip("Efeito instanciado no ponto do impacto (faísca, poeira...).")]
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private Vector2 impactOffset = new Vector2(0, 1);
    [SerializeField, Min(0)] private float impactSpread = 0.4f;
    [SerializeField] private AudioClip[] impactSounds;
    [SerializeField, Range(0, 1)] private float volume = 0.7f;
    [Tooltip("Intervalo mínimo entre sons, para vários zumbis batendo juntos não estourarem o áudio.")]
    [SerializeField, Min(0)] private float soundCooldown = 0.12f;
    [Tooltip("Até esta distância do jogador o som toca no volume máximo.")]
    [SerializeField, Min(0)] private float hearingNear = 4;
    [Tooltip("A partir desta distância do jogador o som não é ouvido.")]
    [SerializeField, Min(0)] private float hearingFar = 14;

    private Health health;
    private SpriteRenderer[] renderers;
    private Color[] baseColors;
    private Vector3 restPosition;
    private Coroutine running;
    private float nextSound;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (shakeTarget == null) shakeTarget = transform;
        restPosition = shakeTarget.localPosition;
    }

    private void OnEnable() => health.Damaged += OnDamaged;

    private void OnDisable()
    {
        health.Damaged -= OnDamaged;
        Restore();
    }

    private void OnDamaged(Health target, int amount)
    {
        if (impactEffect != null)
        {
            var offset = impactOffset + Random.insideUnitCircle * impactSpread;
            Instantiate(impactEffect, transform.position + (Vector3)offset, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        }
        if (impactSounds != null && impactSounds.Length > 0 && Time.time >= nextSound)
        {
            nextSound = Time.time + soundCooldown;
            SoundEffects.PlayRandomAt(impactSounds, transform.position, volume, hearingNear, hearingFar);
        }
        if (!isActiveAndEnabled) return;
        if (running != null) Restore();
        running = StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        // Renderers são lidos a cada golpe: aparências/sprites podem trocar em tempo de jogo.
        renderers = GetComponentsInChildren<SpriteRenderer>();
        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) baseColors[i] = renderers[i].color;
        restPosition = shakeTarget.localPosition;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float k = 1 - t / duration;
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].color = Color.Lerp(baseColors[i], baseColors[i] * flashColor, k);
            shakeTarget.localPosition = restPosition + (Vector3)(Random.insideUnitCircle * shakeAmount * k);
            yield return null;
        }
        Restore();
    }

    private void Restore()
    {
        if (running != null) StopCoroutine(running);
        running = null;
        if (renderers != null)
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].color = baseColors[i];
        renderers = null;
        if (shakeTarget != null) shakeTarget.localPosition = restPosition;
    }
}
