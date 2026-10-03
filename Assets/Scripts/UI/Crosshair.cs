using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoomLab
{
    /// <summary>
    /// Punto de mira en el centro de la pantalla.
    /// Cambia de tamaño y color según lo que hace el jugador, para que sepa cuándo puede agarrar algo:
    /// pequeño y tenue en reposo, más grande al apuntar a un objeto agarrable y oculto mientras sostiene uno.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class Crosshair : MonoBehaviour
    {
        [Tooltip("Agarrador del jugador, para saber a qué apunta. Si se deja vacío, se busca en la escena.")]
        [SerializeField] private ObjectGrabber grabber;

        [Header("Aspecto")]
        [SerializeField] private float idleSize = 6f;
        [SerializeField] private float targetSize = 14f;
        [SerializeField] private Color idleColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color targetColor = Color.white;
        [Tooltip("Ocultar el punto mientras se sostiene un objeto, para que no tape lo que se está examinando.")]
        [SerializeField] private bool hideWhileHolding = true;
        [Tooltip("Rapidez de la transición entre estados.")]
        [SerializeField] private float transitionSpeed = 15f;

        private Image image;
        private RectTransform rectTransform;

        private void Awake()
        {
            image = GetComponent<Image>();
            rectTransform = (RectTransform)transform;

            // El punto de mira nunca debe bloquear clics de la interfaz.
            image.raycastTarget = false;

            if (grabber == null)
            {
                grabber = FindAnyObjectByType<ObjectGrabber>();
            }
        }

        private void Update()
        {
            bool holding = grabber != null && grabber.IsHolding;
            bool aiming = grabber != null && grabber.LookTarget != null;

            float size = aiming ? targetSize : idleSize;
            Color color = aiming ? targetColor : idleColor;
            if (holding && hideWhileHolding)
            {
                color.a = 0f;
            }

            // Interpolación suave: el punto "crece" al pasar por encima de un objeto en vez de saltar.
            float t = transitionSpeed * Time.deltaTime;
            rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, new Vector2(size, size), t);
            image.color = Color.Lerp(image.color, color, t);
        }
    }
}
