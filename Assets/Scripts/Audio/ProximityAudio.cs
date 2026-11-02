using UnityEngine;

namespace ProjetoFinal.Audio
{
    /// <summary>Volume cresce conforme se aproxima do alvo; sem alvo, usa o objeto com a tag "Player".</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class ProximityAudio : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private AudioClip loop;
        [SerializeField] private AudioClip[] occasional;
        [SerializeField] private Vector2 occasionalInterval = new Vector2(3f, 7f);
        [Range(0f, 1f)] [SerializeField] private float maxVolume = 0.6f;
        [Tooltip("Distância em que o volume é máximo.")]
        [Min(0f)] [SerializeField] private float nearDistance = 2f;
        [Tooltip("Distância a partir da qual não se ouve nada.")]
        [Min(0f)] [SerializeField] private float farDistance = 22f;

        private AudioSource source;
        private float nextOccasional;

        public float CurrentVolume { get; private set; }

        public static float VolumeAt(float distance, float near, float far, float max)
        {
            if (far <= near) return distance <= near ? max : 0f;
            return max * (1f - Mathf.Clamp01((distance - near) / (far - near)));
        }

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.clip = loop;
        }

        private void OnEnable()
        {
            if (target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) target = player.transform;
            }
            ScheduleOccasional();
            UpdateVolume();
            if (loop != null) source.Play();
        }

        private void OnDisable() => source.Stop();

        private void Update()
        {
            UpdateVolume();
            if (occasional != null && occasional.Length > 0 && Time.time >= nextOccasional)
            {
                if (CurrentVolume > 0f) source.PlayOneShot(occasional[Random.Range(0, occasional.Length)]);
                ScheduleOccasional();
            }
        }

        private void UpdateVolume()
        {
            CurrentVolume = target != null
                ? VolumeAt(Vector2.Distance(transform.position, target.position), nearDistance, farDistance, maxVolume)
                : 0f;
            source.volume = CurrentVolume;
        }

        private void ScheduleOccasional() => nextOccasional = Time.time + Random.Range(occasionalInterval.x, occasionalInterval.y);
    }
}
