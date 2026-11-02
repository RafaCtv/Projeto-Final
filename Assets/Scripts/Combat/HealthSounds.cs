using ProjetoFinal.Audio;
using UnityEngine;

[RequireComponent(typeof(Health))]
public sealed class HealthSounds : MonoBehaviour
{
    [SerializeField] private AudioClip[] hurt;
    [SerializeField] private AudioClip death;
    [SerializeField, Range(0, 1)] private float volume = 0.8f;
    private Health health;
    private int last;

    private void Awake()
    {
        health = GetComponent<Health>();
        last = health.Current;
    }

    private void OnEnable() { health.Changed += OnChanged; health.Died += OnDied; }
    private void OnDisable() { health.Changed -= OnChanged; health.Died -= OnDied; }

    private void OnChanged(Health value)
    {
        if (value.Current < last && value.IsAlive) SoundEffects.PlayRandom(hurt, volume);
        last = value.Current;
    }

    private void OnDied(Health value) => SoundEffects.Play(death, volume);
}
