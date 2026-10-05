using UnityEngine;

namespace EscapeRoomLab
{
    /// <summary>
    /// Orienta este objeto para que mire siempre hacia un objetivo. Pensado para focos (Spot Light)
    /// que deben iluminar algo concreto, como la nota con el código: si la nota se mueve, el foco la sigue.
    /// Funciona también en el editor ([ExecuteAlways]), así se ve el resultado sin darle a Play.
    /// </summary>
    [ExecuteAlways]
    public class AimAt : MonoBehaviour
    {
        [Tooltip("Objeto al que apunta el foco.")]
        [SerializeField] private Transform target;
        [Tooltip("Seguir al objetivo durante el juego (por ejemplo, si el jugador coge la nota). Si se desactiva, solo apunta al empezar.")]
        [SerializeField] private bool followTarget = true;

        private void Start()
        {
            Aim();
        }

        private void LateUpdate()
        {
            if (followTarget || !Application.isPlaying)
            {
                Aim();
            }
        }

        private void Aim()
        {
            if (target != null)
            {
                transform.LookAt(target.position);
            }
        }
    }
}
