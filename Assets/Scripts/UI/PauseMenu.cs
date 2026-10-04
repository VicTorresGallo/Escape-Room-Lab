using UnityEngine;
using UnityEngine.InputSystem;

namespace EscapeRoomLab
{
    /// <summary>
    /// Menú de pausa del laboratorio. Con Esc (o Start en mando) congela el juego,
    /// desactiva el control del jugador, libera el cursor y muestra el panel de pausa.
    /// Los botones del panel llaman a Resume() y GoToMainMenu().
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Tooltip("Acción Player/Pause.")]
        [SerializeField] private InputActionReference pauseAction;
        [Tooltip("Panel con los botones de pausa. Empieza oculto.")]
        [SerializeField] private GameObject pausePanel;
        [Tooltip("SceneLoader de la escena, para volver al menú con fundido.")]
        [SerializeField] private SceneLoader sceneLoader;
        [Tooltip("Nombre de la escena del menú de inicio.")]
        [SerializeField] private string mainMenuScene = "MainMenu";

        [Tooltip("Scripts del jugador que se desactivan en pausa (FirstPersonController, ObjectGrabber). Si se deja vacío, se buscan en la escena.")]
        [SerializeField] private Behaviour[] playerControls;

        public bool IsPaused { get; private set; }

        // Al ir al menú se bloquea la pausa para que Esc no la vuelva a abrir durante el fundido.
        private bool leaving;

        private void Awake()
        {
            if (playerControls == null || playerControls.Length == 0)
            {
                playerControls = new Behaviour[]
                {
                    FindAnyObjectByType<FirstPersonController>(),
                    FindAnyObjectByType<ObjectGrabber>()
                };
            }

            pausePanel.SetActive(false);
        }

        private void OnEnable()
        {
            pauseAction.action.Enable();
        }

        private void OnDisable()
        {
            // Si se sale de la escena estando en pausa, el tiempo no debe quedarse congelado.
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (!leaving && pauseAction.action.WasPressedThisFrame())
            {
                if (IsPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        public void Pause()
        {
            IsPaused = true;

            // timeScale 0 detiene la física y todo lo que use Time.deltaTime (pesas, puertas, balanza).
            Time.timeScale = 0f;
            SetPlayerControlsEnabled(false);
            pausePanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            pausePanel.SetActive(false);

            // FirstPersonController vuelve a bloquear el cursor al activarse.
            SetPlayerControlsEnabled(true);
        }

        public void GoToMainMenu()
        {
            leaving = true;

            // El juego sigue congelado durante el fundido (que usa tiempo sin escalar);
            // OnDisable restaura timeScale al descargarse la escena.
            sceneLoader.LoadScene(mainMenuScene);
        }

        /// <summary>
        /// Desactivar los scripts del jugador impide moverse, mirar y coger objetos durante la pausa
        /// (el ratón no depende de timeScale, así que congelar el tiempo no basta).
        /// Al desactivar ObjectGrabber, el objeto que se lleve en la mano se suelta.
        /// </summary>
        private void SetPlayerControlsEnabled(bool enabled)
        {
            foreach (Behaviour control in playerControls)
            {
                if (control != null)
                {
                    control.enabled = enabled;
                }
            }
        }
    }
}
