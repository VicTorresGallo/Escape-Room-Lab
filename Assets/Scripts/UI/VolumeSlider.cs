using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoomLab
{
    /// <summary>
    /// Control de volumen general para un Slider de la interfaz (menú de inicio o pausa).
    /// Guarda el valor en PlayerPrefs para que se mantenga entre escenas y partidas.
    /// SceneLoader multiplica este volumen por el de sus fundidos, así que no se pisan.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class VolumeSlider : MonoBehaviour
    {
        private const string PrefsKey = "MasterVolume";

        /// <summary>Volumen general elegido por el jugador, de 0 a 1 (por defecto, 1).</summary>
        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(PrefsKey, 1f);
            private set => PlayerPrefs.SetFloat(PrefsKey, value);
        }

        private void Start()
        {
            Slider slider = GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(MasterVolume);
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(float value)
        {
            MasterVolume = value;
            AudioListener.volume = value;
        }
    }
}
