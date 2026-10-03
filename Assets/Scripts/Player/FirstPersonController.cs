using UnityEngine;
using UnityEngine.InputSystem;

namespace EscapeRoomLab
{
    /// <summary>
    /// Controlador del jugador en primera persona.
    /// Mueve al jugador con un CharacterController y gira la vista con el ratón:
    /// el giro horizontal rota todo el cuerpo y el vertical solo la cámara,
    /// para que el movimiento siempre vaya en la dirección hacia la que se mira.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Cámara hija del jugador. Si se deja vacío, se busca automáticamente.")]
        [SerializeField] private Transform cameraTransform;

        [Header("Input")]
        [Tooltip("Acción Player/Move (Vector2).")]
        [SerializeField] private InputActionReference moveAction;
        [Tooltip("Acción Player/Look (Vector2, delta del ratón).")]
        [SerializeField] private InputActionReference lookAction;

        [Header("Movimiento")]
        [Tooltip("Velocidad al andar, en metros por segundo.")]
        [SerializeField] private float walkSpeed = 3f;
        [Tooltip("Aceleración de la gravedad (negativa).")]
        [SerializeField] private float gravity = -9.81f;

        [Header("Vista")]
        [Tooltip("Grados que gira la vista por cada píxel que se mueve el ratón.")]
        [SerializeField] private float lookSensitivity = 0.1f;
        [Tooltip("Ángulo máximo para mirar arriba y abajo, para que la cámara no dé la vuelta.")]
        [SerializeField] private float maxPitch = 80f;

        // Pequeña fuerza hacia abajo estando en el suelo para que isGrounded no parpadee en rampas o escalones.
        private const float GroundedVelocity = -2f;

        private CharacterController controller;
        private float verticalVelocity;
        private float pitch;

        /// <summary>
        /// Si es false, el ratón no gira la vista. Lo usa ObjectGrabber mientras se rota un objeto.
        /// </summary>
        public bool LookEnabled { get; set; } = true;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraTransform == null)
            {
                cameraTransform = GetComponentInChildren<Camera>().transform;
            }
        }

        private void OnEnable()
        {
            moveAction.action.Enable();
            lookAction.action.Enable();
            SetCursorLocked(true);
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
            lookAction.action.Disable();
            SetCursorLocked(false);
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
        }

        /// <summary>
        /// Gira el cuerpo en horizontal y la cámara en vertical según el ratón.
        /// El delta del ratón ya es el movimiento de este frame, así que no se multiplica por Time.deltaTime.
        /// </summary>
        private void HandleLook()
        {
            if (!LookEnabled)
            {
                return;
            }

            Vector2 look = lookAction.action.ReadValue<Vector2>() * lookSensitivity;

            transform.Rotate(Vector3.up, look.x);

            // Se resta porque mover el ratón hacia arriba (y positivo) debe inclinar la cámara hacia arriba (x negativo).
            pitch = Mathf.Clamp(pitch - look.y, -maxPitch, maxPitch);
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        /// <summary>
        /// Mueve al jugador relativo a hacia dónde mira y aplica la gravedad.
        /// Todo se hace en una sola llamada a Move para que isGrounded sea fiable.
        /// </summary>
        private void HandleMovement()
        {
            Vector2 input = moveAction.action.ReadValue<Vector2>();

            // ClampMagnitude evita que en diagonal se ande más rápido.
            Vector3 move = transform.right * input.x + transform.forward * input.y;
            move = Vector3.ClampMagnitude(move, 1f) * walkSpeed;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = GroundedVelocity;
            }
            verticalVelocity += gravity * Time.deltaTime;
            move.y = verticalVelocity;

            controller.Move(move * Time.deltaTime);
        }

        /// <summary>
        /// Bloquea y oculta el cursor durante el juego, o lo libera para los menús.
        /// </summary>
        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
