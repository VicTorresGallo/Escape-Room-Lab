using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomLab
{
    /// <summary>
    /// Puerta que se abre y se cierra girando sobre su bisagra.
    /// Open() y Close() se llaman desde eventos: onUnlocked de CodeLock para la puerta de salida,
    /// onBalanced y onUnbalanced de ScaleBalance para la vitrina.
    /// Si se pide abrir o cerrar a mitad de movimiento, la puerta da la vuelta desde donde esté.
    /// </summary>
    public class Door : MonoBehaviour
    {
        [Tooltip("Objeto que gira: un vacío colocado en el borde de la puerta, con la hoja como hija. Si se deja vacío, gira este objeto.")]
        [SerializeField] private Transform hinge;
        [Tooltip("Grados que gira al abrirse, sobre el eje Y local. Negativo para abrir hacia el otro lado.")]
        [SerializeField] private float openAngle = 90f;
        [Tooltip("Segundos que tarda en abrirse o cerrarse por completo.")]
        [SerializeField] private float openDuration = 1.5f;
        [Tooltip("Ritmo del movimiento: por defecto arranca y frena suavemente.")]
        [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Al empezar a abrirse: sonido de la puerta...")]
        public UnityEvent onOpening;
        [Tooltip("Al terminar de abrirse: mostrar la pantalla final...")]
        public UnityEvent onOpened;
        [Tooltip("Al empezar a cerrarse: sonido de la puerta...")]
        public UnityEvent onClosing;

        public bool IsOpen { get; private set; }

        private Quaternion closedRotation;
        private Quaternion openRotation;

        // Cuánto está abierta la puerta, de 0 (cerrada) a 1 (abierta). Permite invertir el movimiento a medias.
        private float openAmount;
        private Coroutine moveRoutine;

        private void Awake()
        {
            if (hinge == null)
            {
                hinge = transform;
            }

            closedRotation = hinge.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            IsOpen = true;
            onOpening.Invoke();
            MoveTo(1f, onOpened);
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;
            onClosing.Invoke();
            MoveTo(0f, null);
        }

        private void MoveTo(float target, UnityEvent onFinished)
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }
            moveRoutine = StartCoroutine(MoveRoutine(target, onFinished));
        }

        /// <summary>
        /// Avanza openAmount hacia el objetivo a velocidad constante y aplica la curva para suavizar el giro.
        /// </summary>
        private IEnumerator MoveRoutine(float target, UnityEvent onFinished)
        {
            while (!Mathf.Approximately(openAmount, target))
            {
                openAmount = Mathf.MoveTowards(openAmount, target, Time.deltaTime / openDuration);
                hinge.localRotation = Quaternion.Slerp(closedRotation, openRotation, openCurve.Evaluate(openAmount));
                yield return null;
            }

            moveRoutine = null;
            onFinished?.Invoke();
        }
    }
}
