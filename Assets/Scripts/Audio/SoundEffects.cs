using UnityEngine;

namespace ProjetoFinal.Audio
{
    public static class SoundEffects
    {
        /// <summary>Toca na posição do ouvinte, sem atenuação (interface, ações do próprio jogador).</summary>
        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            var listener = Object.FindAnyObjectByType<AudioListener>();
            var position = listener != null ? listener.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        public static void PlayRandom(AudioClip[] clips, float volume = 1f)
        {
            if (clips == null || clips.Length == 0) return;
            Play(clips[Random.Range(0, clips.Length)], volume);
        }

        /// <summary>
        /// Som "espacial" de um ponto do mundo: some com a distância horizontal até o jogador e puxa para o lado de onde vem.
        /// Usa a distância em X (jogo lateral), então a câmera afastada no eixo Z não atrapalha.
        /// </summary>
        public static void PlayAt(AudioClip clip, Vector3 position, float volume = 1f, float nearDistance = 4f, float farDistance = 16f)
        {
            if (clip == null) return;
            var listenerX = ListenerX();
            float dx = position.x - listenerX;
            float attenuated = ProximityAudio.VolumeAt(Mathf.Abs(dx), nearDistance, farDistance, volume);
            if (attenuated <= 0.01f) return;
            var go = new GameObject("OneShot_" + clip.name);
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = attenuated;
            source.spatialBlend = 0;
            source.panStereo = Mathf.Clamp(dx / Mathf.Max(1, farDistance), -0.8f, 0.8f);
            source.Play();
            Object.Destroy(go, clip.length + 0.1f);
        }

        public static void PlayRandomAt(AudioClip[] clips, Vector3 position, float volume = 1f, float nearDistance = 4f, float farDistance = 16f)
        {
            if (clips == null || clips.Length == 0) return;
            PlayAt(clips[Random.Range(0, clips.Length)], position, volume, nearDistance, farDistance);
        }

        // Referência de quem "ouve": o jogador; sem ele, o AudioListener (câmera).
        private static float ListenerX()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) return player.transform.position.x;
            var listener = Object.FindAnyObjectByType<AudioListener>();
            return listener != null ? listener.transform.position.x : 0;
        }
    }
}
