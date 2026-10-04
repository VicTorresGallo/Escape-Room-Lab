using UnityEngine;

namespace EscapeRoomLab
{
    /// <summary>
    /// Reproduce un sonido de golpe cuando el objeto choca con algo (pesas que caen al suelo,
    /// cubos que golpean la mesa...). El volumen depende de la fuerza del golpe, así que
    /// un roce apenas se oye y una caída suena fuerte.
    /// Va en objetos con Rigidbody y necesita un AudioSource en el mismo objeto.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
    public class ImpactSound : MonoBehaviour
    {
        [Tooltip("Sonidos de golpe. Se elige uno al azar en cada choque para que no suene repetitivo.")]
        [SerializeField] private AudioClip[] clips;

        [Tooltip("Velocidad de choque mínima para que suene (m/s). Evita ruidos al apoyarse o deslizarse.")]
        [SerializeField] private float minImpactSpeed = 0.5f;
        [Tooltip("Velocidad de choque a la que el sonido alcanza su volumen máximo (m/s).")]
        [SerializeField] private float maxImpactSpeed = 5f;
        [Tooltip("Variación aleatoria del tono, para que cada golpe suene un poco distinto.")]
        [SerializeField] private float pitchVariation = 0.1f;
        [Tooltip("Tiempo mínimo entre dos sonidos, para que un objeto que rebota no suene en ráfaga.")]
        [SerializeField] private float cooldown = 0.1f;

        private AudioSource audioSource;
        private float lastPlayTime = float.NegativeInfinity;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;

            // Sonido 3D: se oye más fuerte cuanto más cerca está el jugador.
            audioSource.spatialBlend = 1f;
        }

        private void OnCollisionEnter(Collision collision)
        {
            float speed = collision.relativeVelocity.magnitude;
            if (clips == null || clips.Length == 0 || speed < minImpactSpeed || Time.time - lastPlayTime < cooldown)
            {
                return;
            }

            lastPlayTime = Time.time;

            // Entre un 20 % (golpe flojo) y el 100 % (golpe fuerte) del volumen.
            float volume = Mathf.Lerp(0.2f, 1f, Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, speed));
            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
        }
    }
}
