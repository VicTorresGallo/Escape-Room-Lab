using UnityEngine;

namespace EscapeRoomLab
{
    /// <summary>
    /// Bombilla visible para una luz: copia el color de la luz a la emisión de su malla
    /// y se apaga cuando la luz se apaga. Así una luz que cambia de color por código
    /// (por ejemplo, la de CodeLock al fallar o acertar) tiene un origen visible en la escena.
    /// La malla necesita un material con emisión activada.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class LightBulb : MonoBehaviour
    {
        [Tooltip("Luz cuyo color sigue la bombilla. Si se deja vacío, se busca en este objeto o en sus padres.")]
        [SerializeField] private Light source;
        [Tooltip("Intensidad del brillo de la bombilla encendida.")]
        [SerializeField] private float emissionIntensity = 2f;
        [Tooltip("Brillo que conserva la bombilla apagada (0 = totalmente apagada).")]
        [Range(0f, 1f)]
        [SerializeField] private float offBrightness = 0.05f;

        private Renderer bulbRenderer;
        private MaterialPropertyBlock propertyBlock;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            bulbRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();

            if (source == null)
            {
                source = GetComponentInParent<Light>();
            }
        }

        /// <summary>
        /// Se actualiza cada frame porque CodeLock cambia el color y el estado de la luz en corrutinas (parpadeo).
        /// El color se aplica con un MaterialPropertyBlock para no modificar el material compartido.
        /// </summary>
        private void LateUpdate()
        {
            if (source == null)
            {
                return;
            }

            bool on = source.enabled && source.gameObject.activeInHierarchy;
            Color color = source.color;

            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(EmissionColorId, color * (on ? emissionIntensity : offBrightness));
            bulbRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
