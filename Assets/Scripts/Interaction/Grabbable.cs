using UnityEngine;

namespace EscapeRoomLab
{
    /// <summary>
    /// Marca un objeto con física como agarrable por el jugador (ObjectGrabber).
    /// Guarda la configuración original del Rigidbody para restaurarla al soltarlo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        public Rigidbody Body { get; private set; }
        public Collider[] Colliders { get; private set; }
        public bool IsHeld { get; private set; }

        private bool originalUseGravity;
        private RigidbodyInterpolation originalInterpolation;

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
            Colliders = GetComponentsInChildren<Collider>();
        }

        /// <summary>
        /// Prepara el Rigidbody para seguir la mano del jugador:
        /// sin gravedad y con interpolación para que no tiemble al mover la cámara.
        /// </summary>
        public void OnGrabbed()
        {
            IsHeld = true;
            originalUseGravity = Body.useGravity;
            originalInterpolation = Body.interpolation;

            Body.useGravity = false;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        /// <summary>
        /// Devuelve el Rigidbody a su estado original para que vuelva a caer y chocar con normalidad.
        /// </summary>
        public void OnReleased()
        {
            IsHeld = false;
            Body.useGravity = originalUseGravity;
            Body.interpolation = originalInterpolation;
        }
    }
}
