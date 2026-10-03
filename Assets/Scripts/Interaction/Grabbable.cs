using System;
using UnityEngine;

namespace EscapeRoomLab
{
    /// <summary>
    /// Marca un objeto con física como agarrable por el jugador (ObjectGrabber).
    /// Guarda la configuración original del Rigidbody para restaurarla al soltarlo.
    /// Desactivar el componente impide agarrar el objeto (lo usa CubeSlot al resolver el código).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        public Rigidbody Body { get; private set; }
        public Collider[] Colliders { get; private set; }
        public bool IsHeld { get; private set; }

        /// <summary>
        /// Aviso al ser agarrado. Se lanza antes de preparar el Rigidbody, así quien lo escucha
        /// (por ejemplo, la ranura de un cubo) puede liberarlo antes de que el jugador lo mueva.
        /// </summary>
        public event Action<Grabbable> Grabbed;

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
            Grabbed?.Invoke(this);

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
