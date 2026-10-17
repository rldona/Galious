# Changelog

Historial de cambios de **Galious: The Hollow Kingdom**. Cada versión tiene su etiqueta (`vX.Y.Z`) y su Release en
GitHub. Formato inspirado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/); versiones con
[SemVer](https://semver.org/lang/es/).

## [Sin publicar]

### Cambiado
- Sensación de Hollow Knight encendida: `HitStop` (el juego se para 0,1 s al acertar un golpe), `CoyoteJump` (2 ticks
  para saltar tras salir de un borde) y `JumpBuffer` (saltar pulsado justo antes de aterrizar). Las demos del original
  se reproducen sin ellas.

- CI: las acciones de GitHub, en sus versiones con Node.js 24 (checkout, cache, GameCI, Releases, Pages), y los
  trabajos fijados en Ubuntu 24.04 (`ubuntu-latest` pasa a Ubuntu 26 el 19 de octubre de 2026).

### Corregido
- Wiki: en «la colisión bajo el arte», el castillo va primero y con su colisión; añadidos los mundos 8 y 10.
- CI de pruebas: la de las demos escribía su traza en `logs/unity`, que no está en el repo, y el resumen de resultados
  no tenía permiso para publicarse.

## [1.0.0] - 2026-10-17

El proyecto en GitHub, con CI, wiki y versión web.

### Añadido
- Repositorio privado `rldona/Galious` con todo dentro (la ROM, lo importado de ella y el arte de Hollow Knight) para
  que el CI compile el juego completo.
- CI con GitHub Actions y GameCI: pruebas EditMode en cada push; builds de Windows, Linux, macOS y web a mano o con una
  etiqueta `v*`, subidas a una Release; la wiki publicada en GitHub Pages.
- Wiki de cómo se hizo (`wiki/`, https://rldona.github.io/Galious/): arquitectura, Unity, el mundo por cajas, texturas,
  ambiente, efectos, música, enemigos, jefes, mundos, rendimiento, builds y pruebas, con el original frente al remake en
  el castillo y cada mundo.
- README con la portada, cómo jugar en cada plataforma, los controles y cómo compilar y modificar.
- Versión web (WebGL) con plantilla propia: a pantalla completa, con la portada del juego mientras carga, sin nada de
  Unity, y la partida guardada en el navegador. En la web no sale «Salir del juego».
- Nombre «Galious - The Hollow Kingdom», icono, versión 1.0.0 e identificador `com.rldona.galious`; las builds se llaman
  `Galious.app`, `Galious.exe` y `Galious.x86_64`, y llevan al lado el arte para la biblioteca de Steam (portada
  vertical, cubierta horizontal, héroe, logo e icono), un `.desktop` en Linux y la descripción en el `Info.plist` de
  macOS.
- La partida de «Maze of Knight» se trae sola a la carpeta nueva la primera vez.

### Cambiado
- `RemakeRules.SlashBoxFromArt`: la caja del golpe es la del tajo dibujado (hacia delante 20 × 16 px en vez de 10 × 4;
  arriba y abajo, 22 de alto): ya no es frustrante dar a piedras, enemigos y jefes.
- `RemakeRules.HollowBosses`: todos los jefes reciben golpes en todo el cuerpo, y golpear la maza del False Knight le
  hace daño.

## [0.9.0] - 2026-10-17

Linux, SteamOS y Windows; jefes y combate más justos.

### Añadido
- Builds para Linux y SteamOS (Steam Deck, Legion Go) y para Windows, sin la pantalla de «Made with Unity».
- La lápida de la palabra brilla, se ilumina y avisa de cómo leerla con la Lupa; los bancos y Cornifer se apartan de
  las lápidas.

### Cambiado
- `RemakeRules.HollowBosses`: el False Knight recibe golpes en todo el cuerpo y tocar a un jefe quita de 1 a 3
  máscaras (en el original, de 8 a 40 de vitalidad, cuando se empieza con 8).
- `RemakeRules.HollowMessages`: sin los mensajes de abajo («Enemigo derrotado», «Piedra rota»…); solo los avisos
  necesarios.
- El interior de los muros, en negro puro (la gradación lo volvía gris azulado o verdoso).

### Corregido
- Halo cuadrado claro detrás de las piedras (la oclusión ambiental las contaba como muro).
- Adornos del escenario que se salían de sus plataformas.
- Vibración en Linux: una vibración larga y luego ninguna.
- El «WORLD n» en inglés que se repetía al entrar en un mundo.

## [0.8.0] - 2026-10-17

Música, santuarios, ambiente y rendimiento.

### Añadido
- Música de Hollow Knight por zona y por jefe, normalizada a −14 LUFS; las fanfarrias siguen con el chip.
- Santuarios en español con los dioses de Hollow Knight, puertas de los mundos con fachada y estandartes, tiendas en
  cueva y escaleras nuevas.
- Ambiente de cada zona medido en las salas de Hollow Knight: gradación, luz del Caballero, oscuridad con farol,
  partículas, bruma, jirones de niebla y oclusión ambiental.
- Cornifer en el castillo; pantalla completa a la resolución nativa al arrancar.
- Recorrido de pruebas que se pasa el juego entero (`-recorrido`) y medida de rendimiento (`-rendimiento`).

### Cambiado
- Movimiento fluido a 60–120 FPS: la lógica sigue a 30 ticks por segundo y el dibujo se interpola.
- Menos basura de memoria por fotograma: de unos 6000 a unos 2200 bytes.

## [0.7.0] - 2026-10-16

Galious: The Hollow Kingdom.

### Añadido
- Solo el Caballero (`RemakeRules.SoloKnight`).
- Menú principal y de pausa como en Hollow Knight, con su fuente en toda la interfaz.
- Bancos y sombra al morir, alma y concentración, inventario con amuletos y muescas, diario del cazador, Cornifer y el
  mapa dibujado a mano.
- Mando y opciones como en Hollow Knight; nadar como en Hollow Knight y la lava devuelve al último suelo firme.
- Agua y lava con el arte y los efectos de Hollow Knight.

## [0.6.0] - 2026-10-16

Fusión con el arte de Hollow Knight.

### Añadido
- El Caballero como protagonista; enemigos, proyectiles y jefes con criaturas de Hollow Knight y todas sus acciones.
- Objetos, piedras, puertas, ascensores y escaleras con arte de Hollow Knight; su marcador; el False Knight con maza.
- Escenario de Hollow Knight sobre la colisión de las salas, por capas y por zonas: Cruces Olvidados en el castillo y,
  en los diez mundos, Sendero Verde, Canales Reales, la Colmena, Picos de Cristal, Ciudad de las Lágrimas, Páramos
  Fúngicos, el Abismo, Nido Profundo, Confines del Reino y Palacio Blanco.

## [0.5.0] - 2026-10-15

Estilo visual propio del remake.

### Añadido
- URP 2D con capas, luces, niebla y paletas por mundo; personajes, enemigos, objetos e interfaz pintados por código.
- Golpes con chispas y sacudida, combate del jefe como cambio de luz, retratos de los dioses.
- Arte dibujado: motor en perspectiva, pipeline de SVG a PNG, el castillo y los diez mundos.

### Cambiado
- `RemakeRules.NoRoomChangePause`: sin el parón al cambiar de sala.
- Puertas de mundo cerradas hasta la Gran Llave, con la apertura medida en el original.

## [0.4.0] - 2026-10-14

El juego original completo en Unity.

### Añadido
- Los 53 tipos de enemigo y sus 25 hijos, botín y generadores periódicos.
- Armas secundarias (flecha, cerámica, fuego rodante, fuego, mina y lupa), inventario y efectos de los objetos.
- Santuarios, tiendas y eventos de sala; los 10 jefes con invocación, combate, derrota y Gran Llave.
- El final (sala del bebé, castillo, epílogo y créditos), logo, título, demos e historia, ascensores del castillo.
- Sonido con el PSG, efectos de los jefes y el resto del backlog del original.

## [0.3.0] - 2026-10-14

Primera versión jugable en Unity.

### Añadido
- Proyecto Unity 6 e importador de salas desde la ROM.
- Escena jugable con la física medida del original y build para macOS.
- Puertas de mundo, salidas al castillo, espada y piedras; agua, lava, vitalidad y puertas con llave.
- Puertas giratorias, muros que se cierran, paredes que se rompen y escaleras que se retiran.
- Primeros enemigos con el combate medido.

## [0.2.0] - 2026-10-14

Biblia de diseño.

### Añadido
- *Galious Design Bible*: 11 capítulos en `docs/design_bible`.
- Colisión de los tiles: clases leídas del código, cuadrícula por sala y 91/91 caídas validadas en openMSX.
- Lava, animación del agua y la lava, puesta en escena de los jefes y puerta de un sentido.

## [0.1.0] - 2026-10-14

Ingeniería inversa de *Knightmare II: The Maze of Galious*.

### Añadido
- Herramientas de extracción de la ROM y medición en openMSX; mapas, salas, entidades y objetos de sala.
- Jugador, objetos (con los nombres de la guía de GameFAQs) y objetos con condición del castillo, comprobados en el
  emulador.
- IA de los 53 enemigos y de los 10 jefes, con el ritmo del bucle medido (30 ticks por segundo).
- Música (driver, 21 canciones, grabación del PSG y conversión a MIDI) y los 48 efectos de sonido.

[Sin publicar]: https://github.com/rldona/Galious/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/rldona/Galious/compare/v0.9.0...v1.0.0
[0.9.0]: https://github.com/rldona/Galious/compare/v0.8.0...v0.9.0
[0.8.0]: https://github.com/rldona/Galious/compare/v0.7.0...v0.8.0
[0.7.0]: https://github.com/rldona/Galious/compare/v0.6.0...v0.7.0
[0.6.0]: https://github.com/rldona/Galious/compare/v0.5.0...v0.6.0
[0.5.0]: https://github.com/rldona/Galious/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/rldona/Galious/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/rldona/Galious/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/rldona/Galious/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/rldona/Galious/releases/tag/v0.1.0
