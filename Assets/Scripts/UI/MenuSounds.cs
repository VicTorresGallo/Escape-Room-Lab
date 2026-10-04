using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoomLab
{
    /// <summary>
    /// Da sonido a todos los botones hijos de este objeto (normalmente el Canvas):
    /// un sonido al pasar el ratón por encima y otro al pulsar.
    /// Así no hay que configurar los botones uno a uno, y los que se añadan después también suenan.
    /// Incluye los botones ocultos al empezar, como los del panel de pausa.
    /// </summary>
    public class MenuSounds : MonoBehaviour
    {
        [Tooltip("AudioSource 2D para los sonidos de la interfaz.")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverClip;
        [SerializeField] private AudioClip clickClip;
        [Range(0f, 1f)]
        [SerializeField] private float hoverVolume = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float clickVolume = 1f;

        // Tiempo mínimo entre dos sonidos de hover. Se usa tiempo sin escalar para que funcione en pausa.
        private const float MinHoverInterval = 0.08f;
        private float lastHoverTime = float.NegativeInfinity;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            // true: incluye los botones de paneles desactivados (el menú de pausa empieza oculto).
            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                button.onClick.AddListener(PlayClick);

                ButtonHoverSound hover = button.gameObject.AddComponent<ButtonHoverSound>();
                hover.Init(this, button);
            }
        }

        /// <summary>
        /// El audio no depende de Time.timeScale, así que estos sonidos funcionan también con el juego en pausa.
        /// </summary>
        public void PlayClick()
        {
            Play(clickClip, clickVolume);
        }

        public void PlayHover()
        {
            // Red de seguridad: dos hovers casi a la vez (por ejemplo, al cruzar dos botones pegados) suenan como uno.
            if (Time.unscaledTime - lastHoverTime < MinHoverInterval)
            {
                return;
            }

            lastHoverTime = Time.unscaledTime;
            Play(hoverClip, hoverVolume);
        }

        private void Play(AudioClip clip, float volume)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }
    }
}
