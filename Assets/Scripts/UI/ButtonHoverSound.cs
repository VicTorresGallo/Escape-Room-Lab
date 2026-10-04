using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EscapeRoomLab
{
    /// <summary>
    /// Avisa a MenuSounds cuando el ratón entra en un botón o cuando se selecciona con teclado o mando.
    /// No se añade a mano: MenuSounds lo pone en cada botón al empezar.
    /// </summary>
    public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        private MenuSounds menuSounds;
        private Button button;

        public void Init(MenuSounds sounds, Button target)
        {
            menuSounds = sounds;
            button = target;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Unity vuelve a enviar OnPointerEnter al botón cuando el ratón pasa del fondo al texto hijo
            // (o al revés) sin haber salido del botón. En ese caso reentered es true y no debe sonar otra vez.
            if (eventData.reentered)
            {
                return;
            }

            PlayIfInteractable();
        }

        public void OnSelect(BaseEventData eventData)
        {
            // Al pasar el ratón, Unity también selecciona el botón: solo suena una vez si ya ha sonado por el ratón.
            if (eventData is not PointerEventData)
            {
                PlayIfInteractable();
            }
        }

        private void PlayIfInteractable()
        {
            if (menuSounds != null && button != null && button.IsInteractable())
            {
                menuSounds.PlayHover();
            }
        }
    }
}
