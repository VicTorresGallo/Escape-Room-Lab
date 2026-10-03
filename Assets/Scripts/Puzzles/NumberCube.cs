using UnityEngine;

namespace EscapeRoomLab
{
    /// <summary>
    /// Cubo con un número en una de sus caras. Se encaja en una CubeSlot,
    /// que comprueba que la cara del número mira hacia fuera y que no está del revés.
    /// La orientación importa: un 6 puesto boca abajo parece un 9.
    /// </summary>
    [RequireComponent(typeof(Grabbable))]
    public class NumberCube : MonoBehaviour
    {
        [Tooltip("Número que muestra el cubo.")]
        [SerializeField] private int number;
        [Tooltip("Dirección local de la cara que lleva el número (flecha azul en la escena al seleccionar el cubo).")]
        [SerializeField] private Vector3 numberFace = Vector3.forward;
        [Tooltip("Dirección local hacia la que apunta la parte de arriba del número (flecha verde).")]
        [SerializeField] private Vector3 numberUp = Vector3.up;

        public int Number => number;
        public Grabbable Grabbable { get; private set; }
        public Rigidbody Body => Grabbable.Body;

        /// <summary>
        /// True tras ser expulsado de una ranura: no se vuelve a encajar en ninguna
        /// hasta que el jugador lo coja, para que no se recoloque solo nada más salir.
        /// </summary>
        public bool WaitingForPlayer { get; set; }

        /// <summary>Dirección, en el mundo, hacia la que mira el número.</summary>
        public Vector3 FaceDirection => transform.TransformDirection(numberFace).normalized;

        /// <summary>Dirección, en el mundo, de la parte de arriba del número.</summary>
        public Vector3 UpDirection => transform.TransformDirection(numberUp).normalized;

        /// <summary>
        /// Orientación del número relativa al cubo. La ranura la usa para calcular
        /// cómo girar el cubo para que el número quede recto y mirando hacia fuera.
        /// </summary>
        public Quaternion LocalNumberRotation => Quaternion.LookRotation(numberFace, numberUp);

        private void Awake()
        {
            Grabbable = GetComponent<Grabbable>();
            Grabbable.Grabbed += _ => WaitingForPlayer = false;
        }

        // Dibuja en la escena hacia dónde mira el número, para configurar numberFace y numberUp sin adivinar.
        private void OnDrawGizmosSelected()
        {
            Vector3 center = transform.position;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(center, center + FaceDirection * 0.4f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, center + UpDirection * 0.25f);
        }
    }
}
