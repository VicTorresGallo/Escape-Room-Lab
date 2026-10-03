using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomLab
{
    /// <summary>
    /// Puerta que se abre girando sobre su bisagra cuando se llama a Open()
    /// (por ejemplo, desde el evento onUnlocked de CodeLock).
    /// </summary>
    public class Door : MonoBehaviour
    {
        [Tooltip("Objeto que gira: un vacío colocado en el borde de la puerta, con la hoja como hija. Si se deja vacío, gira este objeto.")]
        [SerializeField] private Transform hinge;
        [Tooltip("Grados que gira al abrirse, sobre el eje Y local. Negativo para abrir hacia el otro lado.")]
        [SerializeField] private float openAngle = 90f;
        [Tooltip("Segundos que tarda en abrirse.")]
        [SerializeField] private float openDuration = 1.5f;
        [Tooltip("Ritmo de apertura: por defecto arranca y frena suavemente.")]
        [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Al empezar a abrirse: sonido de la puerta...")]
        public UnityEvent onOpening;
        [Tooltip("Al terminar de abrirse: mostrar la pantalla final...")]
        public UnityEvent onOpened;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (hinge == null)
            {
                hinge = transform;
            }
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            IsOpen = true;
            StartCoroutine(OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            onOpening.Invoke();

            Quaternion closedRotation = hinge.localRotation;
            Quaternion openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);

            for (float time = 0f; time < openDuration; time += Time.deltaTime)
            {
                float t = openCurve.Evaluate(time / openDuration);
                hinge.localRotation = Quaternion.Slerp(closedRotation, openRotation, t);
                yield return null;
            }
            hinge.localRotation = openRotation;

            onOpened.Invoke();
        }
    }
}
