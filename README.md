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

<!-- TODO: elemento narrativo (por qué está encerrado el jugador). -->

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

<!-- TODO: revisar cuando estén definidos en InputSystem_Actions -->

| Acción | Tecla |
|---|---|
| Moverse | `WASD` |
| Mirar | Ratón |
| Interactuar / coger | `E` |
| Pausa | `Esc` |

## Cómo ejecutar el proyecto

| Herramienta | Versión |
|---|---|
| Unity | **6000.6.4f1** (Unity 6) |
| Render pipeline | Universal Render Pipeline (URP) 17.6.0 |
| Input | Input System 1.20.0 |

1. Clona el repositorio.
2. En **Unity Hub**, pulsa **Add → Add project from disk** y selecciona la carpeta `EscapeRoomLab`.
3. Ábrelo con Unity **6000.6.4f1**. La primera vez tarda unos minutos porque Unity regenera la carpeta `Library/`.
4. Abre la escena `Assets/Scenes/MainMenu.unity` <!-- TODO: confirmar nombre --> y pulsa **Play**.

## Puzles y mecánicas

| # | Puzle | Tipo | Qué hace el jugador | Resultado |
|---|---|---|---|---|
| 1 | **Balanzas** | Físico (Rigidbody) | Coge pesas de distintos tamaños y las coloca en el platillo hasta equilibrar cada balanza. El tamaño de la pesa indica su peso. | Cada balanza equilibrada libera un cubo con un número. |
| 2 | **La nota** | Exploración | Busca por la sala una nota escondida con el código **3496**. | Conoce el orden de los cubos. |
| 3 | **Cubos numerados** | Colocación y orientación | Encaja los cubos en las ranuras en el orden de la nota, girándolos para que el número quede hacia fuera. | Si es correcto, se abre la puerta. Si hay un error, la luz parpadea en rojo y la puerta no se abre. |

### Diseño

- **Primera pista:** al empezar hay una balanza claramente desequilibrada con una pesa al lado, para que el jugador entienda enseguida que tiene que equilibrarla.
- **Nota escondida:** la nota no está a la vista (en un cajón, detrás de un cuadro o bajo un objeto que hay que mover). Así el juego no se reduce a manipular objetos y obliga a explorar.
- **Comprobación del orden:** la puerta solo se abre cuando los 4 cubos están colocados en el orden exacto de la nota y con la orientación correcta.

### Decisiones pendientes

- **Número de balanzas:** cuatro balanzas (una por cubo) es la opción más costosa de colocar y ajustar. Las alternativas son dos balanzas con dos cubos cada una, o una sola balanza que libere los 4 cubos. El script es el mismo en los tres casos, así que se puede decidir según el tiempo disponible.
- **Colores de los cubos:** la nota ya da el orden con los números, así que los colores no hacen falta para el código. Pueden servir para ligar cada balanza con su cubo (balanza roja, cubo rojo) y que el jugador entienda qué desbloquea qué.

## Requisitos de la prueba

| Requisito | Estado | Dónde se cumple |
|---|---|---|
| Mínimo dos puzles o mecánicas distintas | ⬜ | Balanzas, nota escondida y cubos numerados |
| Al menos una mecánica física (Rigidbody) | ⬜ | Balanzas con pesas |
| Menú de inicio | ⬜ | |
| Pantalla final al escapar | ⬜ | Al abrirse la puerta |
| Sonido (efectos y/o música) | ⬜ | |
| Feedback de las acciones del jugador | ⬜ | Balanza que se equilibra, cubo liberado, luz roja al fallar, puerta que se abre |
| Al menos una pieza de arte propia o retocada | ⬜ | Ver [Arte propio](#arte-propio) |
| Build para Windows en `.zip` | ⬜ | |

**Extras opcionales**

| Extra | Estado | Dónde se usa |
|---|---|---|
| Animaciones (Animator) | ⬜ | |
| Secuencias (Timeline / Playable Director) | ⬜ | |
| Partículas (Particle System / VFX Graph) | ⬜ | |
| Iluminación trabajada y post-procesado | ⬜ | |
| Shaders o materiales propios (Shader Graph) | ⬜ | |
| Elemento narrativo / pantalla final cuidada | ⬜ | |

## Arquitectura y estructura del código

<!-- TODO: explicar la arquitectura (sistema de interacción, gestión de puzles, eventos, GameManager...) y por qué. -->

```
Assets/
├── Scenes/      # Escenas del juego
├── Scripts/     # Código C#
├── Prefabs/     # Objetos reutilizables
├── Art/         # Modelos, materiales y texturas
├── Audio/       # Música y efectos
└── Settings/    # Configuración de URP y post-procesado
```

Decisiones técnicas:
- **URP**, para trabajar la iluminación y el post-procesado con buen rendimiento.
- **Input System**, en lugar del Input Manager clásico, con un único mapa de acciones.

## Arte, sonido y assets externos

### Arte propio

<!-- TODO: indicar qué pieza(s) se han hecho o retocado y con qué herramienta (Blender, Photoshop, Shader Graph...). Incluir captura si es posible. -->

### Assets externos

| Asset | Autor / fuente | Licencia | Uso en el juego |
|---|---|---|---|
| | | | |

## Proceso de desarrollo

<!-- TODO: resumen del proceso: idea inicial, cómo se ajustó el alcance, problemas encontrados y cómo se resolvieron. -->

1. **Preparación:** creación del proyecto con la plantilla 3D (URP) de Unity 6, configuración del `.gitignore` y del repositorio.
2. **Diseño:** definición del flujo del juego (balanzas → cubos → nota → puerta). Las partes más costosas, como el número de balanzas, se dejaron abiertas para ajustar el alcance a los 4 días.

## Registro de tiempo

El tiempo se mide con [Clockify](https://clockify.me).

| Día | Horario | Horas | Tareas |
|---|---|---|---|
| 03/10/2026 | <!-- TODO --> | 1 h 55 min | Gestión y puesta en marcha del proyecto: creación con la plantilla 3D (URP), `.gitignore`, repositorio, diseño de los puzles y README |
| **Total** | | **1 h 55 min** | |

## Uso de la IA

### Herramientas

| Herramienta | Para qué |
|---|---|
| Claude Code (Claude Opus 5.5) | Revisión de configuración, documentación, apoyo con el código |
| <!-- TODO: otras (generación de imágenes, audio...) --> | |

### Ejemplos concretos

- **Idea general del juego y puzles.** Propuso varias ambientaciones y mecánicas posibles. Elegí el laboratorio con balanzas y cubos numerados, y la IA ayudó a detectar decisiones pendientes como el número de balanzas o el uso de colores para ligar cada balanza con su cubo.
- **Estructura de este README.** Generó la estructura de la documentación a partir del enunciado de la prueba.
<!-- TODO: más ejemplos a lo largo del desarrollo -->

### Cuándo la IA no dio lo que necesitaba

- **Primera versión del README.** La primera versión se generó sin el enunciado y era genérica: no incluía el registro de tiempo, el apartado de IA ni los assets externos. La resolví pasándole como contexto el PDF del enunciado y el diseño de los puzles.
<!-- TODO: más casos -->

## Posibles mejoras

<!-- TODO: qué se añadiría con más tiempo. -->

---

**Autor: Vicente Torres Gallo** [@VicTorresGallo](https://github.com/VicTorresGallo)
