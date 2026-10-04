using UnityEngine;

namespace EscapeRoomLab
{
    /// <summary>
    /// Sonido de pasos del jugador. Suena un paso cada cierta distancia recorrida por el suelo,
    /// así que el ritmo se ajusta solo a la velocidad: andar despacio da pasos más espaciados.
    /// No suena en el aire ni al estar quieto. Puede ir en el jugador o en un hijo suyo:
    /// usa el CharacterController del jugador (el primero que encuentra hacia arriba en la jerarquía).
    /// </summary>
    public class Footsteps : MonoBehaviour
    {
        [Tooltip("AudioSource para los pasos. Conviene uno propio, separado del de coger y soltar objetos.")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Sonidos de paso. Se elige uno al azar, sin repetir el anterior, para que no suene mecánico.")]
        [SerializeField] private AudioClip[] clips;

        [Tooltip("Metros recorridos entre un paso y el siguiente.")]
        [SerializeField] private float stepDistance = 1.6f;
        [Tooltip("Velocidad mínima (m/s) para que cuente como andar; evita pasos al empujarse contra una pared.")]
        [SerializeField] private float minSpeed = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float volume = 0.5f;
        [Tooltip("Variación aleatoria del tono de cada paso.")]
        [SerializeField] private float pitchVariation = 0.08f;

        private CharacterController controller;
        private float distanceSinceLastStep;
        private int lastClipIndex = -1;

        private void Awake()
        {
            controller = GetComponentInParent<CharacterController>();
            if (controller == null)
            {
                Debug.LogError($"{name}: Footsteps necesita un CharacterController en este objeto o en un padre.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            // Solo cuenta el movimiento horizontal: la gravedad no debe generar pasos.
            Vector3 velocity = controller.velocity;
            velocity.y = 0f;
            float speed = velocity.magnitude;

            if (!controller.isGrounded || speed < minSpeed)
            {
                // Al pararse, el siguiente paso suena enseguida en lugar de esperar media zancada.
                distanceSinceLastStep = stepDistance * 0.5f;
                return;
            }

            // Con el juego en pausa Time.deltaTime es 0, así que no se acumulan pasos.
            distanceSinceLastStep += speed * Time.deltaTime;
            if (distanceSinceLastStep >= stepDistance)
            {
                distanceSinceLastStep = 0f;
                PlayStep();
            }
        }

        private void PlayStep()
        {
            if (audioSource == null || clips == null || clips.Length == 0)
            {
                return;
            }

            int index = Random.Range(0, clips.Length);
            if (clips.Length > 1 && index == lastClipIndex)
            {
                index = (index + 1) % clips.Length;
            }
            lastClipIndex = index;

            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clips[index], volume);
        }
    }
}
