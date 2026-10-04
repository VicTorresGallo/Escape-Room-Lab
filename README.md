# Escape Room Lab

Minijuego 3D para PC de tipo *escape room*, desarrollado en Unity como prueba técnica de prácticas para **Virtual Zone**.

> **Estado:** en desarrollo.

---

## Índice

1. [Descripción](#descripción)
2. [Cómo jugar](#cómo-jugar)
3. [Cómo ejecutar el proyecto](#cómo-ejecutar-el-proyecto)
4. [Puzles y mecánicas](#puzles-y-mecánicas)
5. [Requisitos de la prueba](#requisitos-de-la-prueba)
6. [Arquitectura y estructura del código](#arquitectura-y-estructura-del-código)
7. [Arte, sonido y assets externos](#arte-sonido-y-assets-externos)
8. [Proceso de desarrollo](#proceso-de-desarrollo)
9. [Registro de tiempo](#registro-de-tiempo)
10. [Uso de la IA](#uso-de-la-ia)
11. [Posibles mejoras](#posibles-mejoras)

---

## Descripción

El jugador está encerrado en un **laboratorio**. Para escapar tiene que equilibrar unas balanzas con las pesas que encuentre por la sala. Cada balanza equilibrada libera un cubo con un número. Después debe encontrar una nota escondida con el código **3496** y encajar los cubos en ese orden, bien orientados, para abrir la puerta.

### Historia

El jugador es un periodista que investiga los experimentos del **Dr. Vidal**. Durante la visita, Vidal le ofrece un café drogado y el jugador despierta en su laboratorio con la puerta sellada. Vidal no confía en las llaves: su cerradura solo se abre con un código que esconde entre sus pesas y sus notas, y que el jugador debe descubrir antes de que vuelva.

La historia se presenta en el menú de inicio y se cierra en la pantalla final: el jugador escapa justo cuando se oyen pasos en el pasillo, con todo lo que ha visto en la memoria para destapar los planes de Vidal.

### Flujo del juego

1. Menú de inicio y arranque en el laboratorio cerrado.
2. Explorar la sala, encontrar las pesas y deducir qué pesa necesita cada balanza por su tamaño.
3. Cada balanza equilibrada desbloquea un cubo con un número.
4. Encontrar la nota con el código **3496**, escondida en la sala.
5. Encajar los cubos en las ranuras en ese orden, con el número visible hacia fuera.
6. La puerta se abre y aparece la pantalla final.

## Cómo jugar

### Build para Windows

1. Descarga y descomprime `EscapeRoomLab.zip`. <!-- TODO: enlace a la build -->
2. Ejecuta `EscapeRoomLab.exe`.

### Controles

| Acción | Tecla |
|---|---|
| Moverse | `WASD` |
| Mirar | Ratón |
| Coger / soltar objeto | `E` |
| Rotar el objeto que se sostiene | Mantener clic derecho y mover el ratón |
| Pausa / continuar | `Esc` |

## Cómo ejecutar el proyecto

| Herramienta | Versión |
|---|---|
| Unity | **6000.6.4f1** (Unity 6) |
| Render pipeline | Universal Render Pipeline (URP) 17.6.0 |
| Input | Input System 1.20.0 |

1. Clona el repositorio.
2. En **Unity Hub**, pulsa **Add → Add project from disk** y selecciona la carpeta `EscapeRoomLab`.
3. Ábrelo con Unity **6000.6.4f1**. La primera vez tarda unos minutos porque Unity regenera la carpeta `Library/`.
4. Abre la escena `Assets/Scenes/MainMenu.unity` y pulsa **Play**.

Para generar la build: **File → Build Profiles**, perfil **Windows** → **Build**. El perfil ya tiene las escenas en orden: `MainMenu`, `Laboratorio` y `Final`.

> **Nota:** si Unity pide importar *TMP Essentials*, no hace falta: ya están incluidos en `Assets/TextMesh Pro/`.

## Puzles y mecánicas

| # | Puzle | Tipo | Qué hace el jugador | Resultado |
|---|---|---|---|---|
| 1 | **Balanzas** | Físico (Rigidbody) | Coge pesas de distintos tamaños y las coloca en el platillo hasta equilibrar cada balanza. El tamaño de la pesa indica su peso. | La vitrina de cristal se abre y libera los cubos numerados. |
| 2 | **La nota** | Exploración | Busca por la sala una nota escondida con el código **3496** y la coge y gira para leerla. | Conoce el orden de los cubos. |
| 3 | **Cubos numerados** | Colocación y orientación | Encaja los cubos en las ranuras en el orden de la nota, girándolos para que el número quede hacia fuera y derecho. | Si es correcto, los cubos se bloquean y se abre la puerta. Si hay un error, la luz parpadea en rojo y los cubos salen expulsados para volver a intentarlo. |

### Estado de las mecánicas

| Mecánica | Estado |
|---|---|
| Jugador en primera persona (movimiento, vista, gravedad) | ✅ Hecho |
| Coger, soltar y rotar objetos con física | ✅ Hecho |
| Punto de mira que reacciona a los objetos agarrables | ✅ Hecho |
| Balanza que se inclina según el peso y detecta el equilibrio | ✅ Hecho |
| Cubos numerados, ranuras y comprobación del código | ✅ Hecho |
| Puerta que se abre con el código correcto | ✅ Hecho |
| Vitrina que se abre al equilibrar la balanza y libera los cubos | ✅ Hecho |
| Nota con el código, que se coge y se rota para leerla | ✅ Hecho (falta esconderla) |
| Menú de inicio con la historia, y pantalla final al escapar | ✅ Hecho |
| Menú de pausa con vuelta al menú principal | ✅ Hecho |
| Fundido a negro entre escenas | ✅ Hecho |
| Ayuda contextual de controles (coger, soltar, girar) | ✅ Hecho |
| Sonido: pasos, coger y soltar, golpes de objetos, vitrina, puerta, código correcto e incorrecto, botones | ✅ Hecho |
| Música de fondo en el laboratorio | ✅ Hecho (falta asignarla en el menú) |
| Balanza como interruptor: la vitrina se abre al equilibrarla y se cierra al desequilibrarla | ✅ Hecho |
| Laboratorio ambientado (arte) | ⬜ Pendiente |

### Diseño

- **Primera pista:** al empezar hay una balanza claramente desequilibrada con una pesa al lado, para que el jugador entienda enseguida que tiene que equilibrarla.
- **Nota escondida:** la nota no está a la vista (en un cajón, detrás de un cuadro o bajo un objeto que hay que mover). Así el juego no se reduce a manipular objetos y obliga a explorar.
- **Comprobación del orden:** la puerta solo se abre cuando los 4 cubos están colocados en el orden exacto de la nota y con la orientación correcta.
- **El 6 y el 9:** la ranura exige que el número no esté boca abajo, así que la orientación forma parte del puzle: un 6 girado parece un 9.
- **Pesas que se deducen a simple vista:** cada pesa tiene una masa acorde a su tamaño, para que el jugador pueda razonar qué combinación equilibra la balanza.

### Decisiones pendientes

- **Número de balanzas:** cuatro balanzas (una por cubo) es la opción más costosa de colocar y ajustar. Las alternativas son dos balanzas con dos cubos cada una, o una sola balanza que libere los 4 cubos. El script es el mismo en los tres casos, así que se puede decidir según el tiempo disponible.
- **Colores de los cubos:** la nota ya da el orden con los números, así que los colores no hacen falta para el código. Pueden servir para ligar cada balanza con su cubo (balanza roja, cubo rojo) y que el jugador entienda qué desbloquea qué.

## Requisitos de la prueba

| Requisito | Estado | Dónde se cumple |
|---|---|---|
| Mínimo dos puzles o mecánicas distintas | ✅ | Balanza con pesas, nota con el código y cubos numerados |
| Al menos una mecánica física (Rigidbody) | ✅ | Balanza con pesas: el peso real de los Rigidbody inclina el brazo |
| Menú de inicio | ✅ | Escena `MainMenu`: título, historia, botones Jugar y Salir |
| Pantalla final al escapar | ✅ | Escena `Final`: se carga con un fundido al terminar de abrirse la puerta |
| Sonido (efectos y/o música) | ✅ | Música de fondo, pasos, coger y soltar, golpes según la fuerza del choque, vitrina, puerta, código correcto e incorrecto y botones de los menús |
| Feedback de las acciones del jugador | 🟨 | Hecho: punto de mira, ayuda contextual de controles, brazo de la balanza que se inclina, cubos que encajan con una transición, luz roja que parpadea al fallar, puerta que se abre, sonidos de cada acción y golpes cuyo volumen depende de la fuerza del choque. Falta: partículas |
| Al menos una pieza de arte propia o retocada | ⬜ | Ver [Arte propio](#arte-propio) |
| Build para Windows en `.zip` | 🟨 | Primera build generada y probada; falta la definitiva y el `.zip` |

**Extras opcionales**

| Extra | Estado | Dónde se usa |
|---|---|---|
| Animaciones (Animator) | ⬜ | |
| Secuencias (Timeline / Playable Director) | ⬜ | |
| Partículas (Particle System / VFX Graph) | ⬜ | |
| Iluminación trabajada y post-procesado | ⬜ | |
| Shaders o materiales propios (Shader Graph) | ⬜ | |
| Elemento narrativo / pantalla final cuidada | 🟨 | Historia del Dr. Vidal en el menú y en la pantalla final |

## Arquitectura y estructura del código

```
Assets/
├── Audio/          # Music/ y SFX/ (Player, Objects, Puzzles, UI)
├── Canvas/         # Prefab Menú: Canvas de las pantallas de menú
├── Inputs/         # Mapa de acciones del Input System (PlayerControls)
├── Materials/      # Cristal
├── Prefabs/        # Player, Balanza, Pesas, CuboNumerado, CodeLock, Puerta, Vitrina, Nota
├── Scenes/         # MainMenu, Laboratorio, Final
├── Scripts/
│   ├── Player/       # FirstPersonController, Footsteps
│   ├── Interaction/  # Grabbable, ObjectGrabber, ImpactSound
│   ├── Puzzles/      # ScaleBalance, ScalePlate, NumberCube, CubeSlot, CodeLock, Door
│   └── UI/           # Crosshair, InteractionPrompt, SceneLoader, PauseMenu, MenuSounds, ButtonHoverSound
├── Settings/       # Configuración de URP, post-procesado y Build Profile de Windows
└── TextMesh Pro/   # Recursos de TextMesh Pro (números de los cubos)
```

Todo el código está en el namespace `EscapeRoomLab` y comentado en español.

### Scripts

| Script | Responsabilidad |
|---|---|
| `FirstPersonController` | Movimiento con `CharacterController`, gravedad y vista con el ratón (el cuerpo gira en horizontal y la cámara en vertical). |
| `Grabbable` | Marca un objeto con Rigidbody como agarrable y restaura su física al soltarlo. Avisa con el evento `Grabbed`. |
| `ObjectGrabber` | Raycast desde la cámara (con una esfera de margen para objetos pequeños). Mueve el objeto agarrado dando velocidad al Rigidbody y lo rota con el ratón. |
| `Crosshair` | Punto de mira que crece al apuntar a algo agarrable y se oculta al sostener un objeto. |
| `ScalePlate` | Trigger sobre un platillo que suma la masa de los Rigidbody que tiene encima (ignora los que se llevan en la mano). |
| `ScaleBalance` | Compara los dos platillos e inclina el brazo. Funciona como un interruptor: lanza `onBalanced` al equilibrarse y `onUnbalanced` al desequilibrarse. |
| `NumberCube` | Número del cubo y orientación de su cara (dirección del número y de su parte de arriba). |
| `CubeSlot` | Encaja un cubo bien orientado con una transición. Lo libera si el jugador lo coge y lo expulsa si el código falla. |
| `CodeLock` | Comprueba el orden de los cubos de las ranuras; al acertar abre la puerta y al fallar hace parpadear la luz. |
| `Door` | Abre y cierra una puerta girando sobre su bisagra con una curva de animación; si cambia de sentido a mitad, da la vuelta desde donde está. Se usa para la puerta de salida y para la vitrina. |
| `SceneLoader` | Cambia de escena con un fundido a negro, cierra el juego y libera el cursor en los menús. |
| `PauseMenu` | Con `Esc` congela el juego, desactiva el control del jugador y muestra el panel de pausa. |
| `InteractionPrompt` | Texto de ayuda en la parte inferior de la pantalla que muestra solo el control útil en cada momento: coger, soltar y girar, o cómo girar el objeto. |
| `Footsteps` | Sonido de pasos cada cierta distancia recorrida por el suelo, con clips al azar sin repetir y variación de tono. |
| `ImpactSound` | Sonido de golpe al chocar un objeto, con volumen según la fuerza del choque y un tiempo mínimo entre golpes. |
| `MenuSounds` + `ButtonHoverSound` | Sonido de hover y de clic en todos los botones de un Canvas, sin configurarlos uno a uno. |

### Decisiones técnicas

- **Puzles desacoplados con `UnityEvent`.** Cada puzle avisa con un evento (`onBalanced`, `onUnlocked`, `onCubePlaced`...) y lo que ocurre después se configura en el Inspector. Así la balanza no necesita saber qué cubo libera ni el candado qué puerta abre, y se pueden añadir sonidos o partículas sin tocar código.
- **Objetos agarrados movidos con física.** En lugar de teletransportar el objeto a la mano, se le da velocidad hacia el punto de agarre. Así sigue chocando con paredes y muebles y se puede soltar con algo de inercia.
- **Balanza por código, sin *joints*.** El peso se lee de los Rigidbody reales, pero la inclinación del brazo se calcula por código. Es más estable y fácil de ajustar que simularla con articulaciones físicas.
- **Controles enseñados en contexto.** En lugar de una pantalla de instrucciones, un texto en la parte inferior muestra solo el control útil en cada momento. Así el jugador aprende a coger y girar objetos justo cuando lo necesita.
- **Pausa con `timeScale` y scripts desactivados.** `Time.timeScale = 0` congela la física y las animaciones, pero el ratón no depende del tiempo, así que además se desactivan los scripts del jugador. Los fundidos usan tiempo sin escalar para funcionar con el juego en pausa.
- **Sonido desde los eventos.** Casi todos los sonidos de los puzles se conectan en el Inspector con `AudioSource.PlayOneShot` en los eventos que ya existían (`onOpening`, `onUnlocked`, `onWrongCode`...), sin código nuevo. El volumen general baja y sube con los fundidos para que la música no se corte de golpe al cambiar de escena.
- **Configuración desde el Inspector.** Velocidades, ángulos, tolerancias y colores son campos `[SerializeField]` con `Tooltip`, para ajustar el juego sin recompilar.
- **URP**, para trabajar la iluminación y el post-procesado con buen rendimiento.
- **Input System** con `InputActionReference`: las acciones se asignan en el Inspector y los scripts no dependen del nombre del asset de input.

## Arte, sonido y assets externos

### Arte propio

<!-- TODO: indicar qué pieza(s) se han hecho o retocado y con qué herramienta (Blender, Photoshop, Shader Graph...). Incluir captura si es posible. -->

### Assets externos

| Asset | Autor / fuente | Licencia | Uso en el juego |
|---|---|---|---|
| TextMesh Pro (fuente Liberation Sans) | Unity Technologies | Unity Companion License / SIL OFL | Números de los cubos y textos de la interfaz |
| [Impact Sounds](https://kenney.nl/assets/impact-sounds) | Kenney | CC0 | Pasos (`footstep_concrete`), golpes de las pesas (`impactMetal_heavy`) y de los cubos (`impactPunch_medium`) |
| [Interface Sounds](https://kenney.nl/assets/interface-sounds) | Kenney | CC0 | Coger y soltar (`select_002`, `drop_003`), código correcto (`confirmation_004`), código incorrecto (`error_008`), botones de los menús (`switch_001`, `confirmation_001`) |
| [Door creak](https://pixabay.com/sound-effects/?utm_content=100169) | [freesound_community](https://pixabay.com/es/users/freesound_community-46691455/) en Pixabay | Pixabay Content License | Puertas |
| [Squeaky door](https://pixabay.com/sound-effects/?utm_content=43353) | [freesound_community](https://pixabay.com/es/users/freesound_community-46691455/) en Pixabay | Pixabay Content License | Puertas |
| Skrzypienie (crujido) | [u_kt8eskwh7o](https://pixabay.com/es/users/u_kt8eskwh7o-44221995/) en Pixabay | Pixabay Content License | Puertas |
| Hidden Crime | [Universfield](https://pixabay.com/es/users/universfield-28281460/) en Pixabay | Pixabay Content License | Música de fondo del laboratorio, **recortada por mí** |
| <!-- TODO: modelos y texturas del laboratorio --> | | | |

**Sobre las licencias:**
- **CC0 (Kenney):** dominio público. Se puede usar sin atribución; aun así se citan aquí.
- **Pixabay Content License:** uso gratuito, también en juegos, sin atribución obligatoria. Se cita a los autores igualmente, con los enlaces que facilita Pixabay. No permite redistribuir los archivos sueltos como si fueran propios, cosa que no se hace: solo se usan dentro del juego.
- **Ningún asset de audio exige atribución obligatoria.** Los créditos se incluyen por transparencia, tal como pide el enunciado.

## Proceso de desarrollo

### Planificación y alcance

El plazo de entrega es el **lunes 05/10/2026 a las 17:00**, pero por disponibilidad personal solo puedo trabajar en la prueba hasta las **13:00** de ese día. Con ese margen se priorizó tener cuanto antes el juego **completo en gris** (de principio a fin, sin arte) y dejar el arte, el sonido y el pulido para el final:

| Cuándo | Objetivo |
|---|---|
| Sábado | Jugador, agarre de objetos, balanza, cubos, cerradura y puerta |
| Domingo mañana | Nota, menú de inicio, pantalla final y primera build |
| Domingo tarde y noche | Montar el laboratorio, pieza de arte propia, sonido, feedback y build casi definitiva |
| Lunes mañana | Solo arreglos pequeños, build final, documentación y entrega con margen |

Recortes decididos para llegar: empezar con **una sola balanza** (el script permite añadir más duplicando el prefab), puerta animada por código en lugar de Animator, feedback con sonidos, luces y partículas sencillas, y sin extras caros como multijugador o cinemáticas. Sin salto ni sprint: en un escape room pequeño no aportan.

### Pasos

1. **Preparación:** creación del proyecto con la plantilla 3D (URP) de Unity 6, configuración del `.gitignore` y del repositorio.
2. **Diseño:** definición del flujo del juego (balanzas → cubos → nota → puerta). Las partes más costosas, como el número de balanzas, se dejaron abiertas para ajustar el alcance a los 4 días.
3. **Base jugable:** jugador en primera persona y sistema para coger, soltar y rotar objetos con física, que es la base de todos los puzles. Después se añadió un punto de mira y un margen de puntería porque costaba coger los objetos pequeños.
4. **Puzle físico:** balanza que suma la masa real de las pesas e inclina el brazo. Se monta primero una sola balanza en gris; añadir más es duplicar el prefab.
5. **Cubos, código y puerta:** ranuras que encajan los cubos según su orientación, cerradura que comprueba el orden y puerta que se abre.
6. **Vitrina y nota:** vitrina de cristal que se abre al equilibrar la balanza, reutilizando el script `Door` de la puerta (sin código nuevo), y nota con el código que se coge y se rota para leerla. Con esto el juego ya se puede completar de principio a fin en gris.
7. **Menús y build:** menú de inicio con la historia, pantalla final, menú de pausa y fundidos entre escenas, con un único script (`SceneLoader`) reutilizado en todas las escenas. Se generó la primera build para Windows para detectar pronto los problemas que solo aparecen fuera del editor.
8. **Ayuda de controles:** coger con `E` y girar con clic derecho resultaba poco intuitivo, así que se añadió un texto contextual en la parte inferior de la pantalla que indica qué hacer en cada momento.
9. **Sonido:** música de fondo, pasos, coger y soltar, golpes de los objetos, vitrina, puerta, cerradura y botones. La mayoría se conectó desde los eventos de los puzles sin código nuevo; solo hicieron falta scripts para los pasos, los golpes y los botones. La balanza pasó a funcionar como un interruptor: la vitrina se abre al equilibrarla y se cierra al desequilibrarla.

### Problemas encontrados

- **Cámara del jugador demasiado alta y duplicada.** La posición de la cámara es relativa al centro de la cápsula, no a los pies, así que la altura de los ojos era 0.6 y no 1.6. Además había que borrar la Main Camera de la escena (el prefab trae la suya) y ponerle el tag `MainCamera`.
- **Tecla de interactuar lenta.** La acción `Interact` de la plantilla tenía la interacción *Hold*, que obliga a mantener la `E` un momento. Se quitó para que responda al pulsar.
- **Script del jugador fuera del prefab.** El `FirstPersonController` se había añadido a la copia de la escena y no al prefab. Se aplicó al prefab con *Overrides → Apply All* antes de hacer el commit.
- **Balanza invertida.** El brazo y los platillos se movían en sentidos contrarios porque el script daba por hecho en qué extremo del brazo estaba cada platillo. Se cambió para que lo detecte solo, así funciona con cualquier orientación.
- **Cubos que volvían a encajar solos.** Al fallar el código, los cubos expulsados volvían a encajar en el mismo instante: al quitarles el modo kinemático, Unity reevalúa el trigger como si el cubo saliera y volviera a entrar. Se resolvió marcando el cubo expulsado para que no encaje en ninguna ranura hasta que el jugador lo vuelva a coger.
- **Cerradura sin ranuras.** El campo de ranuras del `CodeLock` estaba vacío. Se cambió el script para que, si no se asignan, use automáticamente las ranuras hijas en el orden de la jerarquía.
- **Cubos que parecían no encajar.** Las ranuras eran cubos visibles del mismo tamaño que los cubos numerados, así que al encajar no se notaba nada. La solución es quitar el modelo de las ranuras y agrandar su trigger; además se añadieron mensajes en la consola para comprobar cuándo encaja cada cubo. La luz de la cerradura tampoco estaba asignada, así que fallar el código no mostraba nada.
- **Eventos mal conectados en el Inspector.** La puerta no se abría porque en el evento se había arrastrado el prefab (o el script) desde el panel Project en lugar del objeto de la escena, y se había elegido una función de `GameObject` en lugar de `Door.Open()`. Además, la hoja de la puerta no era hija de la bisagra.
- **Bisagra que parecía girar desde el centro.** Era solo la herramienta del editor en modo *Center*: en modo *Pivot* se ve que la bisagra está en el borde de la puerta.
- **"No cameras rendering" en los menús.** Al borrar la cámara de las escenas de menú, la interfaz se seguía viendo (Canvas en modo *Overlay*), pero Unity avisaba de que ninguna cámara dibujaba la escena. Se añadió una cámara con fondo negro y sin Audio Listener.
- **Escenas fuera de la build.** Las escenas nuevas no estaban en la lista de la build, así que el juego habría arrancado en el laboratorio y no habría encontrado la pantalla final. Se añadieron en el orden `MainMenu`, `Laboratorio`, `Final`.
- **Fundido y panel de pausa diminutos.** La pausa funcionaba pero no se veía el panel, y el fundido era un cuadrado pequeño en el centro. Ambos tenían los anchors estirados a toda la pantalla pero con márgenes negativos que les restaban casi todo el tamaño. Se pusieron los márgenes (Left, Top, Right y Bottom) a 0.
- **Pasos que no sonaban.** El script de pasos exigía un `CharacterController` en el mismo objeto y se había puesto en un hijo del jugador, así que Unity le añadió uno nuevo que nunca se movía. Se cambió el script para que use el del jugador (buscándolo hacia arriba en la jerarquía) y se quitó el sobrante.
- **Sonido de la vitrina mudo.** El evento estaba bien conectado, pero el *Pitch* del Audio Source estaba en -1.43 (cambiado sin querer en el Inspector). Con pitch negativo el clip se reproduce al revés y termina al instante. Se puso a un valor positivo.
- **Hover de los botones que sonaba dos veces.** Unity vuelve a avisar al botón de que el ratón "entra" cuando pasa del fondo al texto hijo. Se ignoran esas reentradas (`eventData.reentered`) y se añadió un tiempo mínimo entre dos sonidos de hover.
- **Balanza que se quedaba bloqueada.** Al resolverse, la balanza se quedaba nivelada aunque se quitaran las pesas, lo que resultaba extraño. Se cambió a un funcionamiento de interruptor, con un margen de 0,5 s para que una pesa que rebota no abra y cierre la vitrina en bucle.

## Registro de tiempo

El tiempo se mide con [Clockify](https://clockify.me).

| Día | Horas | Tareas |
|---|---|---|
| 03/10/2026 | 2 h | Gestión y puesta en marcha del proyecto: creación con la plantilla 3D (URP), `.gitignore`, repositorio, diseño de los puzles y README |
| 03/10/2026 | 1 h | Personaje en primera persona: movimiento, vista, coger y rotar objetos |
| 03/10/2026 | 1 h 30 min | Balanza y pesas |
| 03/10/2026 | 1 h 15 min | Cubos numerados, ranuras, cerradura de código y puerta |
| 03/10/2026 | 1 h | Vitrina que abre la balanza y nota con el código |
| 04/10/2026 | 1 h 15 min | Escenas y menús: menú de inicio, pantalla final, menú de pausa y fundidos |
| 04/10/2026 | 20 min | Primera build para Windows |
| 04/10/2026 | 30 min | Ayuda contextual de controles |
| 04/10/2026 | 3 h 15 min | Sonido: búsqueda de sonidos y música, recorte de la pista, scripts de pasos, golpes y botones, balanza como interruptor |
| **Total** | **12 h 5 min** | |

## Uso de la IA

### Herramientas

| Herramienta | Para qué |
|---|---|
| Claude Code (Claude Opus 5.5), desde la extensión de VS Code | Planificación, código C#, diagnóstico de errores, git y documentación |
| Herramienta de IA para generar música | Intento de generar la música de fondo (descartado, ver abajo) |

### Cómo trabajé con la IA

El reparto de trabajo fue siempre el mismo: yo decidía qué quería conseguir y montaba los objetos en el editor de Unity; la IA proponía el enfoque, escribía los scripts y me explicaba cómo configurarlos. Después yo probaba en Unity y, si algo fallaba, le describía lo que veía o le pegaba el error de la consola. Como los archivos de escena y prefab de Unity son texto, la IA podía leerlos para comprobar cómo había montado los objetos y encontrar el fallo.

### Ejemplos concretos

**Planificación y documentación**
- **Idea general del juego y puzles.** Propuso varias ambientaciones y mecánicas posibles. Elegí el laboratorio con balanzas y cubos numerados, y la IA ayudó a detectar decisiones pendientes como el número de balanzas o el uso de colores para ligar cada balanza con su cubo.
- **Hoja de ruta.** A partir del diseño propuso el orden de trabajo (primero el juego completo en gris, después arte y sonido). Cuando le dije que solo podía trabajar hasta el lunes a las 13:00, reajustó el plan por días y propuso recortes concretos, como empezar con una sola balanza.
- **Estructura de este README.** Generó la estructura de la documentación a partir del enunciado de la prueba y la fue actualizando con el progreso.

**Personaje en primera persona**
- **Revisión de lo que ya tenía.** Antes de escribir código, revisó el prefab y los inputs que había creado. Me recomendó reutilizar el mapa de acciones de la plantilla (que ya tenía `Move`, `Look` e `Interact`) y no llamar al asset `PlayerInput`, porque choca con un componente del Input System del mismo nombre.
- **Script `FirstPersonController`.** Lo escribió con `CharacterController`, gravedad, vista con el ratón limitada a ±80° y cursor bloqueado. Usó `InputActionReference` para asignar las acciones desde el Inspector. Dejó fuera el salto y el sprint porque no aportan en un escape room.
- **Puesta en marcha en la escena.** Al preguntarle cómo probarlo, revisó la escena y el prefab y detectó tres problemas antes de darle a Play: la cámara duplicada en la escena, la cámara del jugador 60 cm por encima de la cabeza y la falta del tag `MainCamera`.

**Coger y rotar objetos**
- **Agarre con física.** Diseñó el agarre moviendo el objeto con velocidad en lugar de teletransportarlo, para que siga chocando con el escenario. Añadió la acción `Rotate` al mapa de input y quitó la interacción *Hold* de `Interact`, que hacía que la `E` respondiera tarde.
- **Punto de mira y ayuda para apuntar.** Le comenté que costaba coger los objetos pequeños. Añadió un punto de mira que crece al apuntar a algo agarrable y un segundo intento de detección con una esfera de 10 cm de radio cuando el rayo no da en el objeto.

**Puzles**
- **Balanza y pesas.** Propuso calcular la inclinación por código a partir de la masa real de los Rigidbody, en lugar de usar *joints* físicos, que son más difíciles de ajustar. También me explicó qué significa "conectar las referencias" en el Inspector.
- **Cubos, ranuras, cerradura y puerta.** Escribió los cuatro scripts y me indicó qué tipo de objeto de Unity crear para cada parte (objeto vacío, cubo, luz, trigger) y cómo colocarlos.
- **Diagnóstico leyendo los archivos de Unity.** Cuando la puerta no se abría, revisó los archivos `.unity` y `.prefab` y detectó que el evento apuntaba al prefab del panel Project en lugar del objeto de la escena, que la función elegida era la de `GameObject` en lugar de `Door.Open()` y que la hoja de la puerta no era hija de la bisagra.

**Menús, pausa y build**
- **Menús y fundidos.** Escribió un único script, `SceneLoader`, para cambiar de escena con fundido, salir del juego y liberar el cursor, y me explicó qué objetos de UI crear en cada pantalla y cómo conectar los botones. También propuso el texto narrativo del menú, que adapté.
- **Menú de pausa.** Añadió la acción `Pause` al input y el script `PauseMenu`. Me explicó por qué no basta con `Time.timeScale = 0` (el ratón no depende del tiempo) y por qué los fundidos deben usar tiempo sin escalar.
- **Revisión de la configuración de la build.** Comprobó en los archivos del proyecto que las escenas estaban en el orden correcto, que el perfil de Windows no estaba en modo *Development* y me avisó de que el perfil usa su propia lista de escenas.
- **Diagnóstico de la interfaz.** Cuando el panel de pausa no se veía, leyó la escena y encontró que tanto el panel como el fundido tenían márgenes negativos que los dejaban casi sin tamaño.
- **Ayuda de controles.** Le comenté que coger con `E` y girar con clic derecho era poco intuitivo. Propuso un texto contextual que solo muestra el control útil en cada momento (en lugar de una pantalla de instrucciones), escribió el script `InteractionPrompt` y añadió a `ObjectGrabber` la propiedad `IsRotating` para saber cuándo se está girando un objeto.

**Sonido**
- **Dónde buscar sonido y música.** Me recomendó páginas con licencias claras (Kenney, Pixabay, Freesound, Incompetech) y herramientas de IA para generar música. Me explicó qué significa el aviso de *Content ID* de YouTube en una pista de Pixabay y por qué no afecta al juego.
- **Scripts de sonido.** Escribió `Footsteps` (pasos según la distancia recorrida), `ImpactSound` (golpes con volumen según la fuerza del choque) y `MenuSounds` (sonido en todos los botones de un Canvas a la vez). Para el resto me explicó cómo conectar `AudioSource.PlayOneShot` desde los eventos que ya existían, sin código.
- **Fundido de audio.** Propuso bajar el volumen general junto con el fundido a negro, para que la música no se corte de golpe al cambiar de escena.
- **Revisión final del sonido.** Al terminar, revisó todas las escenas y prefabs y me avisó de lo que faltaba: la música del menú sin clip asignado, las ranuras sin sonido al encajar un cubo y la música del laboratorio con un volumen casi inaudible.

**Git**
- **Commits.** Propuso cómo dividir el trabajo en commits pequeños, uno por funcionalidad, y sus mensajes. Me explicó cómo separar el título del detalle con varios `-m`. Antes de cada commit revisaba que no se colaran cambios de otra funcionalidad y que los `.meta` fueran incluidos. Los commits hechos con su ayuda llevan la línea `Co-Authored-By: Claude`.
- **Renombrar la escena.** Me recomendó renombrarla desde Unity y no desde el explorador de archivos, para conservar su identificador interno (GUID) y no romper las referencias.
<!-- TODO: más ejemplos a lo largo del desarrollo -->

### Cuándo la IA no dio lo que necesitaba

- **Primera versión del README.** La primera versión se generó sin el enunciado y era genérica: no incluía el registro de tiempo, el apartado de IA ni los assets externos. La resolví pasándole como contexto el PDF del enunciado y el diseño de los puzles.
- **Balanza invertida.** La primera versión del script suponía una orientación concreta del brazo y, en mi escena, brazo y platillos se movían al revés. Al describirle el fallo, cambió el script para que detecte solo en qué lado está cada platillo.
- **Cubos que volvían a encajar.** La primera versión de las ranuras volvía a encajar al instante los cubos expulsados tras un código incorrecto. Tras probarlo y explicárselo, identificó la causa (el cambio de modo kinemático reevalúa el trigger) y cambió el enfoque.
- **Cerradura sin ranuras.** El script exigía asignar las ranuras a mano y saltaba un error si se olvidaba. Al pegarle el error de la consola, lo cambió para que las busque solo entre los hijos.
- **Ningún aviso al encajar los cubos.** Al principio no había forma de saber si un cubo había encajado y parecía que no pasaba nada. Al revisar la escena encontró que la luz de la cerradura no estaba asignada y que la puerta estaba mal conectada, y añadió mensajes en la consola para comprobar cuándo encaja cada cubo y si el código es correcto.
- **Panel de pausa invisible.** Su primera sugerencia (corregir la escala del panel) no bastó: el panel seguía sin verse. Al contarle que el fundido también salía como un cuadrado pequeño, volvió a leer la escena y encontró la causa real, los márgenes negativos de ambos objetos.
- **Música generada con IA.** Intenté generar la música de fondo con IA, con el *prompt* `dark ambient, mysterious abandoned laboratory, subtle electronic hum, slow tension, loopable, instrumental`. La pista resultante era aprovechable pero tenía un momento con el volumen muy alto, y para seguir generando y descargar hacía falta pagar. Lo resolví buscando una pista con licencia libre en Pixabay y recortándola con Audacity.
- **Pasos que no sonaban.** El script de pasos obligaba a tener un `CharacterController` en el mismo objeto; al ponerlo en un hijo del jugador, Unity añadió uno nuevo que nunca se movía. Al revisar el prefab encontró el problema y cambió el script.
- **Código sin probar.** La IA no puede ejecutar Unity, así que nunca probó sus scripts: cada cambio lo probé yo en el editor. Los fallos de arriba solo aparecieron al jugar.
<!-- TODO: más casos -->

## Posibles mejoras

<!-- TODO: completar al final -->

- **Sensibilidad para mando.** La vista está ajustada para el ratón; con el stick derecho gira muy despacio. Haría falta una sensibilidad separada para el mando.
- **Más balanzas.** Una balanza por cubo, cada una con su color, para ligar visualmente cada balanza con el cubo que libera.

---

**Autor: Vicente Torres Gallo** [@VicTorresGallo](https://github.com/VicTorresGallo)
