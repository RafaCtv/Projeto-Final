using System.Collections;
using UnityEngine;

namespace ProjetoFinal.Audio
{
    /// <summary>Crossfade em tempo não escalado, para funcionar com o jogo pausado.</summary>
    public sealed class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip playOnStart;
        [Range(0f, 1f)] [SerializeField] private float volume = 0.5f;
        [Min(0f)] [SerializeField] private float fadeDuration = 1.2f;

        private AudioSource[] sources;
        private int active;
        private Coroutine fade;

        public AudioClip Current { get; private set; }

        private void Awake() => EnsureSources();

        // Outros componentes podem pedir música no próprio OnEnable, antes do Awake deste.
        private void EnsureSources()
        {
            if (sources != null) return;
            sources = new AudioSource[2];
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i] = gameObject.AddComponent<AudioSource>();
                sources[i].loop = true;
                sources[i].playOnAwake = false;
                sources[i].spatialBlend = 0f;
                sources[i].volume = 0f;
            }
        }

        private void Start()
        {
            if (playOnStart != null && Current == null) Play(playOnStart);
        }

        public void Play(AudioClip clip)
        {
            if (clip == null || clip == Current) return;
            EnsureSources();
            Current = clip;
            var previous = sources[active];
            active = 1 - active;
            var next = sources[active];
            next.clip = clip;
            next.volume = 0f;
            next.Play();
            StartFade(previous, next);
        }

        public void Stop()
        {
            EnsureSources();
            Current = null;
            StartFade(sources[active], null);
        }

        private void StartFade(AudioSource fadeOut, AudioSource fadeIn)
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(Crossfade(fadeOut, fadeIn));
        }

        private IEnumerator Crossfade(AudioSource fadeOut, AudioSource fadeIn)
        {
            float startOut = fadeOut != null ? fadeOut.volume : 0f;
            for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                float k = t / fadeDuration;
                if (fadeOut != null) fadeOut.volume = Mathf.Lerp(startOut, 0f, k);
                if (fadeIn != null) fadeIn.volume = Mathf.Lerp(0f, volume, k);
                yield return null;
            }
            if (fadeOut != null) { fadeOut.volume = 0f; fadeOut.Stop(); }
            if (fadeIn != null) fadeIn.volume = volume;
            fade = null;
        }
    }
}
