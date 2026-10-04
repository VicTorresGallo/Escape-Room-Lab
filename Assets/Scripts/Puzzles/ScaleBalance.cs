using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomLab
{
    /// <summary>
    /// Balanza de dos platillos. Compara la masa de ambos, inclina el brazo hacia el lado
    /// que más pesa y funciona como un interruptor: al equilibrarse lanza onBalanced (abrir la vitrina)
    /// y al desequilibrarse lanza onUnbalanced (cerrarla).
    ///
    /// Montaje esperado:
    /// - El brazo (beam) gira sobre su eje Z local, con los platillos a lo largo de su eje X
    ///   (en qué extremo va cada uno se detecta solo).
    /// - Cada platillo es un objeto aparte (no hijo del brazo) con un Rigidbody kinemático
    ///   y un hijo con ScalePlate (collider trigger) que detecta lo que hay encima.
    /// </summary>
    public class ScaleBalance : MonoBehaviour
    {
        [Header("Platillos")]
        [SerializeField] private ScalePlate leftPlate;
        [SerializeField] private ScalePlate rightPlate;
        [Tooltip("Rigidbody kinemático del platillo izquierdo. Se mueve arriba y abajo con el brazo.")]
        [SerializeField] private Rigidbody leftPlateBody;
        [Tooltip("Rigidbody kinemático del platillo derecho. Se mueve arriba y abajo con el brazo.")]
        [SerializeField] private Rigidbody rightPlateBody;

        [Header("Brazo")]
        [SerializeField] private Transform beam;
        [Tooltip("Grados que se inclina el brazo por cada kg de diferencia.")]
        [SerializeField] private float degreesPerKg = 6f;
        [Tooltip("Inclinación máxima del brazo, en grados.")]
        [SerializeField] private float maxAngle = 20f;
        [Tooltip("Rapidez con la que el brazo alcanza su inclinación.")]
        [SerializeField] private float tiltSpeed = 3f;

        [Header("Equilibrio")]
        [Tooltip("Diferencia de masa (kg) que se considera equilibrada.")]
        [SerializeField] private float tolerance = 0.05f;
        [Tooltip("Segundos que debe mantenerse un estado (equilibrada o no) para cambiar. Evita que una pesa que rebota abra y cierre la vitrina en bucle.")]
        [SerializeField] private float balanceHoldTime = 0.5f;

        [Header("Feedback")]
        [Tooltip("Luz opcional que indica el estado de la balanza.")]
        [SerializeField] private Light statusLight;
        [SerializeField] private Color unbalancedColor = Color.red;
        [SerializeField] private Color balancedColor = Color.green;

        [Header("Eventos")]
        [Tooltip("Se lanza cada vez que la balanza pasa a estar equilibrada: abrir la vitrina, sonido, partículas...")]
        public UnityEvent onBalanced;
        [Tooltip("Se lanza cada vez que la balanza deja de estar equilibrada: cerrar la vitrina...")]
        public UnityEvent onUnbalanced;

        /// <summary>Si la balanza está equilibrada ahora mismo.</summary>
        public bool IsBalanced { get; private set; }

        /// <summary>Si se ha equilibrado alguna vez.</summary>
        public bool IsSolved { get; private set; }

        private float currentAngle;
        private float stateChangeTimer;
        private Quaternion beamStartRotation;
        private Vector3 leftPlateStart;
        private Vector3 rightPlateStart;

        // Posición de cada platillo a lo largo del brazo (su eje X local), con signo:
        // negativa si está en el lado -X, positiva si está en el +X. Se calcula sola en Awake,
        // así da igual cómo esté girada la balanza o en qué lado se haya puesto cada platillo.
        private float leftArm;
        private float rightArm;
        private Vector3 beamUp;

        private void Awake()
        {
            beamStartRotation = beam.localRotation;
            leftPlateStart = leftPlateBody.position;
            rightPlateStart = rightPlateBody.position;

            leftArm = Vector3.Dot(leftPlateStart - beam.position, beam.right);
            rightArm = Vector3.Dot(rightPlateStart - beam.position, beam.right);
            beamUp = beam.up;

            // Los platillos se mueven por código; con interpolación los objetos encima no tiemblan.
            foreach (Rigidbody plate in new[] { leftPlateBody, rightPlateBody })
            {
                plate.isKinematic = true;
                plate.interpolation = RigidbodyInterpolation.Interpolate;
            }

            SetStatusColor(unbalancedColor);
        }

        private void FixedUpdate()
        {
            float leftMass = leftPlate.TotalMass;
            float rightMass = rightPlate.TotalMass;
            float difference = leftMass - rightMass;

            CheckBalance(leftMass, rightMass, difference);

            // Girar el brazo un ángulo positivo sobre Z sube su extremo +X. Para que baje el platillo
            // que más pesa, el signo depende de en qué extremo esté el platillo izquierdo.
            float tiltDirection = leftArm < 0f ? 1f : -1f;

            float targetAngle = Mathf.Clamp(difference * degreesPerKg * tiltDirection, -maxAngle, maxAngle);
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, tiltSpeed * Time.fixedDeltaTime);
            ApplyTilt(currentAngle);
        }

        /// <summary>
        /// Cambia de estado (equilibrada / desequilibrada) cuando el nuevo estado se mantiene
        /// durante balanceHoldTime. Ese margen evita que una pesa que rebota abra y cierre la vitrina en bucle.
        /// Exige peso en los dos platillos: con ambos vacíos la diferencia también es 0.
        /// </summary>
        private void CheckBalance(float leftMass, float rightMass, float difference)
        {
            bool balancedNow = leftMass > 0f && rightMass > 0f && Mathf.Abs(difference) <= tolerance;

            if (balancedNow == IsBalanced)
            {
                stateChangeTimer = 0f;
                return;
            }

            stateChangeTimer += Time.fixedDeltaTime;
            if (stateChangeTimer < balanceHoldTime)
            {
                return;
            }

            stateChangeTimer = 0f;
            IsBalanced = balancedNow;

            if (IsBalanced)
            {
                IsSolved = true;
                SetStatusColor(balancedColor);
                onBalanced.Invoke();
            }
            else
            {
                SetStatusColor(unbalancedColor);
                onUnbalanced.Invoke();
            }
        }

        /// <summary>
        /// Gira el brazo y mueve cada platillo a la altura del extremo del brazo en el que está,
        /// para que brazo y platillos siempre se muevan juntos.
        /// </summary>
        private void ApplyTilt(float angle)
        {
            beam.localRotation = beamStartRotation * Quaternion.Euler(0f, 0f, angle);

            float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
            leftPlateBody.MovePosition(leftPlateStart + beamUp * (sin * leftArm));
            rightPlateBody.MovePosition(rightPlateStart + beamUp * (sin * rightArm));
        }

        private void SetStatusColor(Color color)
        {
            if (statusLight != null)
            {
                statusLight.color = color;
            }
        }
    }
}
