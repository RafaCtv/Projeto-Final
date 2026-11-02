using UnityEngine;

namespace ProjetoFinal.Audio
{
    /// <summary>Toca na posição do ouvinte para não sofrer atenuação 3D.</summary>
    public static class SoundEffects
    {
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
    }
}
