using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomLab
{
    /// <summary>
    /// Cerradura que comprueba el código formado por los cubos de varias ranuras.
    /// Cuando todas las ranuras están ocupadas compara los números, en orden, con el código:
    /// si coinciden lanza onUnlocked (abrir la puerta); si no, la luz parpadea en rojo
    /// y los cubos se expulsan para volver a intentarlo.
    /// </summary>
    public class CodeLock : MonoBehaviour
    {
        [Tooltip("Ranuras en el orden en que se lee el código (de izquierda a derecha). Si se deja vacío, se usan las CubeSlot hijas en el orden de la jerarquía.")]
        [SerializeField] private CubeSlot[] slots;
        [Tooltip("Código correcto: un número por ranura, en el mismo orden.")]
        [SerializeField] private int[] code = { 3, 4, 9, 6 };

        [Header("Feedback")]
        [Tooltip("Luz que parpadea al fallar y se pone verde al acertar. Opcional.")]
        [SerializeField] private Light feedbackLight;
        [SerializeField] private Color errorColor = Color.red;
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private int blinkCount = 3;
        [SerializeField] private float blinkInterval = 0.15f;

        [Header("Eventos")]
        [Tooltip("Código correcto: abrir la puerta, sonido de éxito...")]
        public UnityEvent onUnlocked;
        [Tooltip("Código incorrecto: sonido de error...")]
        public UnityEvent onWrongCode;

        public bool IsUnlocked { get; private set; }

        // Mientras parpadea la luz de error no se vuelve a comprobar el código.
        private bool showingError;

        private Color originalLightColor;
        private bool originalLightEnabled;

        private void Awake()
        {
            // GetComponentsInChildren devuelve los hijos en el orden de la jerarquía (de arriba abajo).
            if (slots == null || slots.Length == 0)
            {
                slots = GetComponentsInChildren<CubeSlot>();
            }

            if (slots.Length != code.Length)
            {
                Debug.LogError($"{name}: hay {slots.Length} ranuras pero el código tiene {code.Length} números.", this);
            }

            if (feedbackLight != null)
            {
                originalLightColor = feedbackLight.color;
                originalLightEnabled = feedbackLight.enabled;
            }
        }

        private void OnEnable()
        {
            foreach (CubeSlot slot in slots)
            {
                slot.CubePlaced += OnCubePlaced;
            }
        }

        private void OnDisable()
        {
            foreach (CubeSlot slot in slots)
            {
                slot.CubePlaced -= OnCubePlaced;
            }
        }

        private void OnCubePlaced(CubeSlot slot)
        {
            if (IsUnlocked || showingError || !AllSlotsFilled())
            {
                return;
            }

            if (IsCodeCorrect())
            {
                Debug.Log($"{name}: código correcto.", this);
                Unlock();
            }
            else
            {
                Debug.Log($"{name}: código incorrecto.", this);
                StartCoroutine(WrongCodeRoutine());
            }
        }

        private bool AllSlotsFilled()
        {
            foreach (CubeSlot slot in slots)
            {
                if (slot.CurrentCube == null)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsCodeCorrect()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].CurrentCube.Number != code[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Los cubos se quedan encajados para siempre: se bloquean para que ya no se puedan sacar.
        /// </summary>
        private void Unlock()
        {
            IsUnlocked = true;
            foreach (CubeSlot slot in slots)
            {
                slot.Lock();
            }
            SetLight(true, successColor);
            onUnlocked.Invoke();
        }

        /// <summary>
        /// Parpadeo rojo y expulsión de todos los cubos, para que el jugador vea claramente que se ha equivocado.
        /// </summary>
        private IEnumerator WrongCodeRoutine()
        {
            showingError = true;
            onWrongCode.Invoke();

            for (int i = 0; i < blinkCount; i++)
            {
                SetLight(true, errorColor);
                yield return new WaitForSeconds(blinkInterval);
                SetLight(originalLightEnabled, originalLightColor);
                yield return new WaitForSeconds(blinkInterval);
            }

            foreach (CubeSlot slot in slots)
            {
                slot.Eject();
            }

            showingError = false;
        }

        private void SetLight(bool lightEnabled, Color color)
        {
            if (feedbackLight != null)
            {
                feedbackLight.enabled = lightEnabled;
                feedbackLight.color = color;
            }
        }
    }
}
