using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoomLab
{
    /// <summary>
    /// Cambia de escena con un fundido a negro y gestiona el cursor de cada pantalla.
    /// Se usa desde los botones de los menús (OnClick) y desde eventos del juego,
    /// por ejemplo onOpened de la puerta de salida para ir a la pantalla final.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [Header("Fundido")]
        [Tooltip("CanvasGroup de una imagen negra a pantalla completa. Opcional: sin él, el cambio de escena es directo.")]
        [SerializeField] private CanvasGroup fadeGroup;
        [Tooltip("Segundos que dura cada fundido.")]
        [SerializeField] private float fadeDuration = 0.8f;
        [Tooltip("Empezar la escena en negro y aclarar al cargar.")]
        [SerializeField] private bool fadeInOnStart = true;

        [Header("Cursor")]
        [Tooltip("Mostrar y liberar el cursor al cargar esta escena. Activar en los menús, desactivar en el laboratorio.")]
        [SerializeField] private bool showCursor = true;

        private bool isLoading;

        private void Start()
        {
            // El jugador bloquea el cursor; en los menús hay que liberarlo para poder pulsar los botones.
            if (showCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (fadeGroup != null)
            {
                // El fundido no debe bloquear los clics mientras es transparente.
                fadeGroup.blocksRaycasts = false;

                if (fadeInOnStart)
                {
                    StartCoroutine(Fade(1f, 0f));
                }
                else
                {
                    fadeGroup.alpha = 0f;
                }
            }
        }

        /// <summary>
        /// Carga la escena indicada (por su nombre, tal como aparece en Build Profiles / Build Settings).
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isLoading)
            {
                return;
            }

            isLoading = true;
            StartCoroutine(LoadRoutine(sceneName));
        }

        /// <summary>
        /// Cierra el juego. En el editor de Unity no hace nada (Application.Quit solo funciona en la build),
        /// así que se detiene el modo Play para poder probar el botón.
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            if (fadeGroup != null)
            {
                // Bloquea los clics durante el fundido para que no se pulse otro botón.
                fadeGroup.blocksRaycasts = true;
                yield return Fade(fadeGroup.alpha, 1f);
            }

            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Cambia la opacidad del fundido de un valor a otro.
        /// Usa tiempo sin escalar para funcionar aunque el juego esté en pausa (Time.timeScale = 0).
        /// </summary>
        private IEnumerator Fade(float from, float to)
        {
            for (float time = 0f; time < fadeDuration; time += Time.unscaledDeltaTime)
            {
                fadeGroup.alpha = Mathf.Lerp(from, to, time / fadeDuration);
                yield return null;
            }
            fadeGroup.alpha = to;
        }
    }
}
