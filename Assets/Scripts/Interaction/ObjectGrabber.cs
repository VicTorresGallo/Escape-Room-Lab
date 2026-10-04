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
        [Tooltip("Margen de puntería, en metros. Si el rayo no da justo en un objeto, se busca con una esfera de este radio para que los objetos pequeños sean fáciles de coger.")]
        [SerializeField] private float aimAssistRadius = 0.1f;

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

        [Header("Sonido")]
        [Tooltip("AudioSource del jugador para los sonidos de coger y soltar. Opcional.")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip grabClip;
        [SerializeField] private AudioClip releaseClip;

        private Grabbable heldObject;

        // Rotación del objeto relativa a la cámara: así gira con el jugador cuando este mira a otro lado.
        private Quaternion rotationRelativeToCamera;

        private Collider[] playerColliders;

        public bool IsHolding => heldObject != null;

        /// <summary>True mientras se mantiene Rotate con un objeto en la mano. Lo usa InteractionPrompt.</summary>
        public bool IsRotating { get; private set; }

        /// <summary>
        /// Objeto agarrable al que apunta el jugador en este momento (null si no hay ninguno).
        /// Lo usa el punto de mira para cambiar de aspecto.
        /// </summary>
        public Grabbable LookTarget { get; private set; }

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
            // Sin sonido: se desactiva al pausar o al cambiar de escena, no porque el jugador suelte el objeto.
            Release(playSound: false);
        }

        private void Update()
        {
            LookTarget = IsHolding ? null : FindLookTarget();

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
        /// Busca el Grabbable al que apunta el centro de la cámara.
        /// Primero prueba con un rayo exacto; si no da en nada agarrable, repite con una esfera
        /// (aimAssistRadius) para perdonar pequeños fallos de puntería. En ambos casos se usa
        /// el primer impacto, así que una pared delante sigue bloqueando el agarre.
        /// </summary>
        private Grabbable FindLookTarget()
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, grabRange, grabMask, QueryTriggerInteraction.Ignore))
            {
                Grabbable direct = GetAvailableGrabbable(hit.collider);
                if (direct != null)
                {
                    return direct;
                }
            }

            if (aimAssistRadius > 0f &&
                Physics.SphereCast(ray, aimAssistRadius, out hit, grabRange, grabMask, QueryTriggerInteraction.Ignore))
            {
                return GetAvailableGrabbable(hit.collider);
            }

            return null;
        }

        /// <summary>
        /// Devuelve el Grabbable del collider si se puede agarrar: no debe estar ya en la mano
        /// ni desactivado (por ejemplo, un cubo encajado en su ranura).
        /// GetComponentInParent permite agarrar objetos cuyo collider está en un hijo.
        /// </summary>
        private static Grabbable GetAvailableGrabbable(Collider hitCollider)
        {
            Grabbable grabbable = hitCollider.GetComponentInParent<Grabbable>();
            return grabbable != null && grabbable.enabled && !grabbable.IsHeld ? grabbable : null;
        }

        /// <summary>
        /// Agarra el objeto al que se está apuntando, si hay alguno.
        /// </summary>
        private void TryGrab()
        {
            Grabbable grabbable = LookTarget;
            if (grabbable == null)
            {
                return;
            }

            heldObject = grabbable;
            heldObject.OnGrabbed();
            rotationRelativeToCamera = Quaternion.Inverse(cameraTransform.rotation) * heldObject.Body.rotation;

            // Evita que el objeto empuje al jugador o se meta en su cápsula.
            SetCollisionWithPlayer(heldObject, false);
            PlayClip(grabClip);
        }

        /// <summary>
        /// Suelta el objeto. Conserva su velocidad actual, así que se puede dar un pequeño impulso al soltarlo.
        /// </summary>
        private void Release(bool playSound = true)
        {
            if (!IsHolding)
            {
                return;
            }

            SetCollisionWithPlayer(heldObject, true);
            heldObject.OnReleased();
            heldObject = null;
            playerController.LookEnabled = true;

            if (playSound)
            {
                PlayClip(releaseClip);
            }
        }

        private void PlayClip(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Mientras se mantiene Rotate, el ratón gira el objeto en lugar de la vista.
        /// Se gira sobre los ejes de la cámara para que el movimiento sea intuitivo:
        /// ratón a los lados = girar en horizontal, ratón arriba/abajo = voltear hacia delante/atrás.
        /// </summary>
        private void HandleRotation()
        {
            IsRotating = IsHolding && rotateAction.action.IsPressed();
            playerController.LookEnabled = !IsRotating;

            if (!IsRotating)
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
