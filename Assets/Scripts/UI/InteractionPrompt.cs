using TMPro;
using UnityEngine;

namespace EscapeRoomLab
{
    /// <summary>
    /// Texto de ayuda contextual bajo el punto de mira. Muestra solo los controles que
    /// tienen sentido en cada momento, para que el jugador los aprenda al usarlos:
    /// - Apuntando a un objeto agarrable: "[E] Coger".
    /// - Con un objeto en la mano: "[E] Soltar   [Clic dcho.] Girar".
    /// - Girando el objeto: "Mueve el ratón para girarlo".
    /// Sin nada que hacer, el texto se desvanece.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class InteractionPrompt : MonoBehaviour
    {
        [Tooltip("Agarrador del jugador. Si se deja vacío, se busca en la escena.")]
        [SerializeField] private ObjectGrabber grabber;

        [Header("Textos")]
        [TextArea] [SerializeField] private string grabText = "<b>[E]</b> Coger";
        [TextArea] [SerializeField] private string holdText = "<b>[E]</b> Soltar     <b>[Clic dcho.]</b> Girar";
        [TextArea] [SerializeField] private string rotateText = "Mueve el ratón para girarlo";

        [Header("Fundido")]
        [Tooltip("Rapidez con la que aparece y desaparece el texto.")]
        [SerializeField] private float fadeSpeed = 10f;

        private TMP_Text label;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
            label.alpha = 0f;

            // El texto nunca debe bloquear clics de la interfaz.
            label.raycastTarget = false;

            if (grabber == null)
            {
                grabber = FindAnyObjectByType<ObjectGrabber>();
            }
        }

        private void Update()
        {
            string text = GetPromptText();

            // Solo se cambia el texto cuando hay algo que mostrar; así, al desvanecerse,
            // se sigue viendo el último mensaje en lugar de quedarse vacío de golpe.
            if (text != null)
            {
                label.text = text;
            }

            float targetAlpha = text != null ? 1f : 0f;
            label.alpha = Mathf.MoveTowards(label.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
        }

        /// <summary>
        /// Elige el mensaje según lo que está haciendo el jugador, o null si no hay que mostrar nada.
        /// Con el agarrador desactivado (por ejemplo, en pausa) no se muestra ninguno.
        /// </summary>
        private string GetPromptText()
        {
            if (grabber == null || !grabber.isActiveAndEnabled)
            {
                return null;
            }

            if (grabber.IsRotating)
            {
                return rotateText;
            }

            if (grabber.IsHolding)
            {
                return holdText;
            }

            return grabber.LookTarget != null ? grabText : null;
        }
    }
}
