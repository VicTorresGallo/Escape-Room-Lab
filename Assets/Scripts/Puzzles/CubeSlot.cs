using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomLab
{
    /// <summary>
    /// Ranura donde se encaja un NumberCube.
    /// Cuando el jugador suelta un cubo dentro de su trigger con el número mirando hacia fuera
    /// (y no del revés), lo coloca suavemente en su sitio y lo deja fijo.
    /// El jugador puede volver a cogerlo para sacarlo, hasta que CodeLock bloquea la ranura.
    /// Si la orientación no es buena, el cubo se queda suelto y el jugador puede recolocarlo.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CubeSlot : MonoBehaviour
    {
        [Tooltip("Punto donde queda el cubo. Su eje Z (azul) indica hacia dónde debe mirar el número y su eje Y (verde) hacia arriba. Si se deja vacío, se usa este objeto.")]
        [SerializeField] private Transform snapPoint;
        [Tooltip("Margen, en grados, con el que se acepta la orientación del cubo.")]
        [SerializeField] private float maxAngle = 35f;
        [Tooltip("Exigir que el número no esté girado ni boca abajo. Necesario para distinguir el 6 del 9.")]
        [SerializeField] private bool requireUpright = true;
        [Tooltip("Segundos que tarda el cubo en colocarse en su sitio.")]
        [SerializeField] private float snapDuration = 0.2f;
        [Tooltip("Velocidad con la que sale despedido el cubo al expulsarlo (código incorrecto).")]
        [SerializeField] private float ejectSpeed = 2f;

        [Header("Indicador visual")]
        [Tooltip("Malla que marca la ranura. Solo se ve como guía mientras está vacía y el jugador lleva un cubo numerado, " +
                 "y un instante como destello al anclar un cubo. Si se deja vacío, se usa el Renderer de este objeto.")]
        [SerializeField] private Renderer placeholder;
        [Tooltip("Material de la guía y del destello (verde translúcido).")]
        [SerializeField] private Material guideMaterial;

        [Header("Destello al anclar")]
        [Tooltip("Color inicial del destello; se desvanece hasta volverse transparente.")]
        [SerializeField] private Color flashColor = new Color(0.5f, 1f, 0.6f, 0.7f);
        [Tooltip("Cuánto crece el destello alrededor del cubo (0.15 = un 15 %).")]
        [SerializeField] private float punchScale = 0.15f;
        [Tooltip("Segundos que dura el destello.")]
        [SerializeField] private float placeEffectDuration = 0.4f;

        [Tooltip("Se lanza al encajar un cubo: sonido, partículas...")]
        public UnityEvent onCubePlaced;

        /// <summary>Aviso por código para CodeLock, que necesita saber qué ranura ha cambiado.</summary>
        public event Action<CubeSlot> CubePlaced;

        public NumberCube CurrentCube { get; private set; }

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private ObjectGrabber grabber;

        // True mientras se reproduce el destello de anclaje.
        private bool isFlashing;
        private Vector3 placeholderBaseScale;
        private MaterialPropertyBlock propertyBlock;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            if (snapPoint == null)
            {
                snapPoint = transform;
            }
            if (placeholder == null)
            {
                placeholder = GetComponent<Renderer>();
            }
            if (placeholder != null)
            {
                placeholderBaseScale = placeholder.transform.localScale;
            }

            propertyBlock = new MaterialPropertyBlock();
            grabber = FindAnyObjectByType<ObjectGrabber>();
            UpdatePlaceholder();
        }

        private void Update()
        {
            UpdatePlaceholder();
        }

        /// <summary>
        /// La ranura solo se ve como guía cuando está vacía y el jugador lleva un cubo numerado,
        /// o durante el destello al anclar. Con el cubo ya anclado se oculta y se ve el cubo original.
        /// Así la cerradura no muestra "cubos fantasma" que se confundan con los cubos de verdad.
        /// </summary>
        private void UpdatePlaceholder()
        {
            if (placeholder == null)
            {
                return;
            }

            bool carryingCube = grabber != null && grabber.IsHolding &&
                                grabber.HeldObject.TryGetComponent(out NumberCube _);
            placeholder.enabled = isFlashing || (CurrentCube == null && carryingCube);

            if (guideMaterial != null && placeholder.sharedMaterial != guideMaterial)
            {
                placeholder.sharedMaterial = guideMaterial;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            NumberCube cube = GetCube(other);
            if (cube == null || CurrentCube != null)
            {
                return;
            }

            // Solo se encaja cuando el jugador lo suelta, nunca mientras lo sostiene,
            // y no si acaba de ser expulsado y nadie lo ha cogido todavía.
            if (!cube.Grabbable.IsHeld && !cube.WaitingForPlayer && !cube.Body.isKinematic && IsAligned(cube))
            {
                Snap(cube);
            }
        }

        /// <summary>
        /// Saca el cubo de la ranura con un pequeño empujón hacia fuera.
        /// No volverá a encajar en ninguna ranura hasta que el jugador lo coja.
        /// </summary>
        public void Eject()
        {
            NumberCube cube = FreeCube();
            if (cube == null)
            {
                return;
            }

            cube.WaitingForPlayer = true;
            cube.Body.AddForce((snapPoint.forward + Vector3.up * 0.5f) * ejectSpeed, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Deja el cubo encajado fijo para siempre (código resuelto): ya no se puede coger.
        /// </summary>
        public void Lock()
        {
            if (CurrentCube != null)
            {
                CurrentCube.Grabbable.Grabbed -= OnCubeGrabbed;
                CurrentCube.Grabbable.enabled = false;
            }
        }

        /// <summary>
        /// El jugador ha cogido el cubo encajado: se suelta de la ranura para que pueda llevárselo.
        /// </summary>
        private void OnCubeGrabbed(Grabbable grabbable)
        {
            FreeCube();
        }

        /// <summary>
        /// Vacía la ranura y devuelve la física al cubo. Devuelve el cubo liberado (o null si no había).
        /// </summary>
        private NumberCube FreeCube()
        {
            NumberCube cube = CurrentCube;
            if (cube == null)
            {
                return null;
            }

            // Por si se saca mientras aún se estaba colocando o animando.
            StopAllCoroutines();
            ResetPlaceEffect();

            CurrentCube = null;
            cube.Grabbable.Grabbed -= OnCubeGrabbed;
            cube.Body.isKinematic = false;
            UpdatePlaceholder();
            return cube;
        }

        /// <summary>
        /// Al anclar: un destello verde translúcido envuelve el cubo, crece un poco y se desvanece.
        /// Siempre es algo más grande que el cubo, así sus caras no coinciden y no parpadean.
        /// El color se cambia con un MaterialPropertyBlock para no modificar el material compartido por las cuatro ranuras.
        /// </summary>
        private IEnumerator PlaceEffectRoutine()
        {
            if (placeholder == null)
            {
                yield break;
            }

            isFlashing = true;
            UpdatePlaceholder();

            Transform target = placeholder.transform;
            Color transparent = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

            for (float time = 0f; time < placeEffectDuration; time += Time.deltaTime)
            {
                float t = time / placeEffectDuration;

                // Parte un 5 % más grande que el cubo y crece en "campana" (sin(πt): 0 → 1 → 0).
                target.localScale = placeholderBaseScale * (1.05f + punchScale * Mathf.Sin(t * Mathf.PI));

                // Se desvanece: del color del destello a transparente.
                propertyBlock.SetColor(BaseColorId, Color.Lerp(flashColor, transparent, t));
                placeholder.SetPropertyBlock(propertyBlock);
                yield return null;
            }

            ResetPlaceEffect();
        }

        private void ResetPlaceEffect()
        {
            if (placeholder == null)
            {
                return;
            }

            isFlashing = false;
            placeholder.transform.localScale = placeholderBaseScale;
            propertyBlock.Clear();
            placeholder.SetPropertyBlock(propertyBlock);
            UpdatePlaceholder();
        }

        private static NumberCube GetCube(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            return body != null ? body.GetComponent<NumberCube>() : null;
        }

        /// <summary>
        /// El número debe mirar en la dirección de la ranura y, si se exige, con su parte de arriba hacia arriba.
        /// </summary>
        private bool IsAligned(NumberCube cube)
        {
            bool facingOut = Vector3.Angle(cube.FaceDirection, snapPoint.forward) <= maxAngle;
            bool upright = !requireUpright || Vector3.Angle(cube.UpDirection, snapPoint.up) <= maxAngle;
            return facingOut && upright;
        }

        private void Snap(NumberCube cube)
        {
            CurrentCube = cube;

            // Kinemático: deja de caer y de reaccionar a golpes. Si el jugador lo coge, OnCubeGrabbed lo libera.
            cube.Body.isKinematic = true;
            cube.Grabbable.Grabbed += OnCubeGrabbed;

            StartCoroutine(SnapRoutine(cube));
        }

        /// <summary>
        /// Lleva el cubo a su sitio con una transición corta en lugar de teletransportarlo,
        /// para que el jugador vea que ha encajado.
        /// </summary>
        private IEnumerator SnapRoutine(NumberCube cube)
        {
            Transform cubeTransform = cube.transform;
            Vector3 startPosition = cubeTransform.position;
            Quaternion startRotation = cubeTransform.rotation;

            // Rotación que deja el número mirando según snapPoint: (rotación de la ranura) × (inversa de la del número en el cubo).
            Quaternion targetRotation = snapPoint.rotation * Quaternion.Inverse(cube.LocalNumberRotation);

            for (float time = 0f; time < snapDuration; time += Time.deltaTime)
            {
                float t = Mathf.SmoothStep(0f, 1f, time / snapDuration);
                cubeTransform.SetPositionAndRotation(
                    Vector3.Lerp(startPosition, snapPoint.position, t),
                    Quaternion.Slerp(startRotation, targetRotation, t));
                yield return null;
            }
            cubeTransform.SetPositionAndRotation(snapPoint.position, targetRotation);

            StartCoroutine(PlaceEffectRoutine());

            Debug.Log($"{name}: encajado el cubo {cube.Number}.", this);
            onCubePlaced.Invoke();
            CubePlaced?.Invoke(this);
        }

        // Muestra en la escena la orientación que espera la ranura.
        private void OnDrawGizmos()
        {
            Transform point = snapPoint != null ? snapPoint : transform;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(point.position, point.position + point.forward * 0.4f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(point.position, point.position + point.up * 0.25f);
        }
    }
}
