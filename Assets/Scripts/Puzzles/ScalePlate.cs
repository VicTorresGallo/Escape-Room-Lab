using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomLab
{
    /// <summary>
    /// Zona de detección de un platillo de la balanza.
    /// Va en un collider trigger colocado justo encima del platillo y suma la masa
    /// de los Rigidbody que hay dentro (incluidos los que están apilados).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ScalePlate : MonoBehaviour
    {
        // Cuántos colliders de cada Rigidbody hay dentro: un objeto con varios colliders
        // dispara OnTriggerEnter una vez por collider, así que se cuentan para no sacarlo antes de tiempo.
        private readonly Dictionary<Rigidbody, int> bodiesInside = new Dictionary<Rigidbody, int>();

        /// <summary>
        /// Masa total, en kg, de los objetos que hay sobre el platillo.
        /// No cuenta los objetos que el jugador tiene en la mano ni los kinemáticos.
        /// </summary>
        public float TotalMass
        {
            get
            {
                float total = 0f;
                foreach (Rigidbody body in bodiesInside.Keys)
                {
                    if (CountsAsWeight(body))
                    {
                        total += body.mass;
                    }
                }
                return total;
            }
        }

        private void Reset()
        {
            // Al añadir el componente desde el editor, el collider se configura como trigger.
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null)
            {
                return;
            }

            bodiesInside.TryGetValue(body, out int count);
            bodiesInside[body] = count + 1;
        }

        private void OnTriggerExit(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null || !bodiesInside.TryGetValue(body, out int count))
            {
                return;
            }

            if (count <= 1)
            {
                bodiesInside.Remove(body);
            }
            else
            {
                bodiesInside[body] = count - 1;
            }
        }

        /// <summary>
        /// Un objeto pesa en el platillo si existe, tiene física activa y no está en la mano del jugador.
        /// Se comprueba al sumar (y no al entrar) porque el estado puede cambiar mientras está dentro.
        /// </summary>
        private static bool CountsAsWeight(Rigidbody body)
        {
            if (body == null || body.isKinematic)
            {
                return false;
            }

            return !(body.TryGetComponent(out Grabbable grabbable) && grabbable.IsHeld);
        }
    }
}
