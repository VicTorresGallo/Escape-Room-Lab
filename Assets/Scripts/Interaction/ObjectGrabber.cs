using UnityEngine;
using UnityEngine.InputSystem;

namespace EscapeRoomLab
{
    /// <summary>
    /// Permite al jugador agarrar, soltar y rotar objetos con el componente Grabbable.
    /// El objeto se mueve con física (velocidad del Rigidbody), no teletransportándolo,
    /// para que siga chocando con paredes y muebles mientras se lleva en la mano.
    /// </summary>
    public class ObjectGrabber : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Cámara del jugador. El rayo para agarrar sale de su centro.")]
        [SerializeField] private Transform cameraTransform;
        [Tooltip("Se usa para bloquear el giro de la vista mientras se rota el objeto.")]
        [SerializeField] private FirstPersonController playerController;

        [Header("Input")]
        [Tooltip("Acción Player/Interact: agarrar y soltar.")]
        [SerializeField] private InputActionReference interactAction;
        [Tooltip("Acción Player/Rotate: mantener para rotar el objeto agarrado.")]
        [SerializeField] private InputActionReference rotateAction;
        [Tooltip("Acción Player/Look: el movimiento del ratón rota el objeto.")]
        [SerializeField] private InputActionReference lookAction;

        [Header("Agarrar")]
        [Tooltip("Distancia máxima a la que se puede agarrar un objeto.")]
        [SerializeField] private float grabRange = 2.5f;
        [Tooltip("Distancia delante de la cámara a la que se sostiene el objeto.")]
        [SerializeField] private float holdDistance = 1.5f;
        [Tooltip("Capas que puede alcanzar el rayo. Las paredes deben estar incluidas para no agarrar a través de ellas.")]
        [SerializeField] private LayerMask grabMask = ~0;

        [Header("Física al sostener")]
        [Tooltip("Rapidez con la que el objeto sigue al punto de agarre.")]
        [SerializeField] private float followSpeed = 15f;
        [Tooltip("Velocidad máxima del objeto, para que no salga disparado.")]
        [SerializeField] private float maxSpeed = 10f;
        [Tooltip("Si el objeto se queda atascado más lejos que esto del punto de agarre, se suelta.")]
        [SerializeField] private float breakDistance = 1.5f;

        [Header("Rotación")]
        [Tooltip("Grados que gira el objeto por cada píxel que se mueve el ratón.")]
        [SerializeField] private float rotateSensitivity = 0.3f;

        private Grabbable heldObject;

        // Rotación del objeto relativa a la cámara: así gira con el jugador cuando este mira a otro lado.
        private Quaternion rotationRelativeToCamera;

        private Collider[] playerColliders;

        public bool IsHolding => heldObject != null;

        private void Awake()
        {
            if (cameraTransform == null)
            {
                cameraTransform = GetComponentInChildren<Camera>().transform;
            }
            if (playerController == null)
            {
                playerController = GetComponent<FirstPersonController>();
            }

            // Incluye el CharacterController, que también es un Collider.
            playerColliders = GetComponentsInChildren<Collider>();
        }

        private void OnEnable()
        {
            interactAction.action.Enable();
            rotateAction.action.Enable();
            lookAction.action.Enable();
        }

        private void OnDisable()
        {
            Release();
        }

        private void Update()
        {
            if (interactAction.action.WasPressedThisFrame())
            {
                if (IsHolding)
                {
                    Release();
                }
                else
                {
                    TryGrab();
                }
            }

            HandleRotation();
        }

        private void FixedUpdate()
        {
            if (IsHolding)
            {
                MoveHeldObject();
            }
        }

        /// <summary>
        /// Lanza un rayo desde el centro de la cámara y agarra el primer Grabbable que encuentre.
        /// Si el rayo choca antes con una pared u otro objeto, no se agarra nada.
        /// </summary>
        private void TryGrab()
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, grabRange, grabMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            // GetComponentInParent permite agarrar objetos cuyo collider está en un hijo.
            Grabbable grabbable = hit.collider.GetComponentInParent<Grabbable>();
            if (grabbable == null || grabbable.IsHeld)
            {
                return;
            }

            heldObject = grabbable;
            heldObject.OnGrabbed();
            rotationRelativeToCamera = Quaternion.Inverse(cameraTransform.rotation) * heldObject.Body.rotation;

            // Evita que el objeto empuje al jugador o se meta en su cápsula.
            SetCollisionWithPlayer(heldObject, false);
        }

        /// <summary>
        /// Suelta el objeto. Conserva su velocidad actual, así que se puede dar un pequeño impulso al soltarlo.
        /// </summary>
        private void Release()
        {
            if (!IsHolding)
            {
                return;
            }

            SetCollisionWithPlayer(heldObject, true);
            heldObject.OnReleased();
            heldObject = null;
            playerController.LookEnabled = true;
        }

        /// <summary>
        /// Mientras se mantiene Rotate, el ratón gira el objeto en lugar de la vista.
        /// Se gira sobre los ejes de la cámara para que el movimiento sea intuitivo:
        /// ratón a los lados = girar en horizontal, ratón arriba/abajo = voltear hacia delante/atrás.
        /// </summary>
        private void HandleRotation()
        {
            bool rotating = IsHolding && rotateAction.action.IsPressed();
            playerController.LookEnabled = !rotating;

            if (!rotating)
            {
                return;
            }

            Vector2 look = lookAction.action.ReadValue<Vector2>() * rotateSensitivity;

            // Se multiplica por la izquierda para rotar sobre los ejes de la cámara, no los del objeto.
            Quaternion yaw = Quaternion.AngleAxis(-look.x, Vector3.up);
            Quaternion pitch = Quaternion.AngleAxis(look.y, Vector3.right);
            rotationRelativeToCamera = yaw * pitch * rotationRelativeToCamera;
        }

        /// <summary>
        /// Lleva el objeto hacia el punto de agarre dándole velocidad, en lugar de cambiar su posición,
        /// para que la física siga resolviendo los choques con el escenario.
        /// </summary>
        private void MoveHeldObject()
        {
            Rigidbody body = heldObject.Body;
            Vector3 holdPoint = cameraTransform.position + cameraTransform.forward * holdDistance;
            Vector3 toHoldPoint = holdPoint - body.position;

            // Si se ha quedado atrapado detrás de algo, se suelta para no arrastrarlo a través de la pared.
            if (toHoldPoint.magnitude > breakDistance)
            {
                Release();
                return;
            }

            body.linearVelocity = Vector3.ClampMagnitude(toHoldPoint * followSpeed, maxSpeed);
            body.angularVelocity = Vector3.zero;
            body.MoveRotation(cameraTransform.rotation * rotationRelativeToCamera);
        }

        private void SetCollisionWithPlayer(Grabbable grabbable, bool enabled)
        {
            foreach (Collider objectCollider in grabbable.Colliders)
            {
                foreach (Collider playerCollider in playerColliders)
                {
                    Physics.IgnoreCollision(objectCollider, playerCollider, !enabled);
                }
            }
        }
    }
}
