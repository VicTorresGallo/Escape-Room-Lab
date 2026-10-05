using UnityEngine;

namespace EscapeRoomLab
{
    /// <summary>
    /// Permite al jugador empujar objetos con Rigidbody al caminar contra ellos (por ejemplo, la silla).
    /// Los objetos con más masa que maxPushMass no se mueven.
    /// El CharacterController no aplica fuerzas por sí solo, así que se hace en OnControllerColliderHit,
    /// que Unity llama cada vez que el movimiento del jugador choca con un collider.
    /// Va en el mismo objeto que el CharacterController.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerPush : MonoBehaviour
    {
        [Tooltip("Velocidad (m/s) a la que se desplaza el objeto empujado. Algo menor que la del jugador para que se note el peso.")]
        [SerializeField] private float pushSpeed = 2f;
        [Tooltip("Masa máxima (kg) que el jugador puede empujar. Por encima, el objeto no se mueve.")]
        [SerializeField] private float maxPushMass = 30f;

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            if (body == null || body.isKinematic || body.mass > maxPushMass)
            {
                return;
            }

            // No empujar lo que se pisa: si el choque es hacia abajo, el jugador está encima del objeto.
            if (hit.moveDirection.y < -0.3f)
            {
                return;
            }

            // Solo se empuja en horizontal, en la dirección en la que camina el jugador.
            // Se fija la velocidad (como en el ejemplo de Unity) en lugar de sumar fuerza: este método se llama
            // cada frame mientras se empuja, y sumar impulsos haría que el objeto saliera disparado.
            Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized;
            Vector3 velocity = pushDirection * pushSpeed;
            velocity.y = body.linearVelocity.y;
            body.linearVelocity = velocity;
        }
    }
}
