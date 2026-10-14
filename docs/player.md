# El jugador: controles, acciones y personajes

Leyenda como en `state_machine.md`: **[C]** código · **[O]** observado en openMSX · **[O*]** observado forzando RAM · **[I]** interpretación.

## Controles

La lectura de entrada (b00:4C75) junta joystick y teclado en un byte, E008 (E009 = recién pulsadas) [C]:

| Bit | Teclado | Joystick | Acción |
|---|---|---|---|
| 0 | ↑ | arriba | **saltar**; subir escaleras [O] |
| 1 | ↓ | abajo | bajar escaleras; entrar por puertas y objetos de sala [O] |
| 2 / 3 | ← / → | izquierda / derecha | andar; también en el aire, durante el salto [O] |
| 4 | espacio | botón A | **espada** (b03:A747: E508 = estado del golpe, sonido 0x13) [C][O] |
| 5 | M | botón B | **arma secundaria** elegida en E510 (b03:A72B) [C][O] |

Fuera de esa rutina se leen otras teclas:
- **F1**: menú de objetos (estado 11).
- **F2**: pausa (estado 9).
- **L** en el título: contraseña (estado 18).
- **F5**: continuar partida, si se activó con ZEUS [G]. Lo lee b01:3EF3 (E032) [C].
- **CTRL**: usar la Biblia, que congela a los enemigos (b02:9BB6) [O*].

## Acciones (las mismas para los dos personajes)

- **Andar** a izquierda y derecha: 52 px/s [O].
- **Saltar** con ↑. Se puede dirigir a izquierda y derecha en el aire. La altura depende del personaje (ver abajo).
- **Subir y bajar escaleras.** Para bajar hay que estar alineado con la escalera: x del jugador = x de la escalera + 8.
- **Espada.** Rompe piedras (15 de vida) y daña enemigos. El objeto 0x25 suma 1 de daño por golpe.
- **Arma secundaria** con M: gasta munición. Con la flecha, cada disparo resta 1 al primer contador del marcador (E046, que son las **flechas**) y crea un proyectil en E540 y siguientes [O*]. Hay seis armas:
  - las cuatro que se recogen en el castillo: 1 flecha, 2 flecha de cerámica, 3 fuego rodante y 4 fuego;
  - la 5, la **mina** (b03:A72B), y la 6, la **lupa** (b03:AD46, activa E517). Las dos se compran en santuarios; con la lupa, M delante de una lápida revela la palabra del jefe [G].
- **Entrar** por puertas y objetos de sala con ↓.
- **Usar objetos y cambiar de personaje** en el menú de objetos (F1).
- **Pronunciar palabras (hechizos).** En la sala del jefe de cada mundo se escribe con el teclado la palabra de ese mundo (YOMAR, ELOHIM…) y aparece el jefe (estado 15; ver `state_machine.md`). La palabra se descubre leyendo la lápida del mundo con la lupa [G]. No hay magia de ataque: el daño a distancia sale de las armas secundarias.
- **Palabras secretas fuera de esa sala** [G]:
  - UMBRELLA, con la daga, elimina a los murciélagos;
  - ZEUS, escrita en pausa, activa la continuación con F5. F5 es la tecla que lee b01:3EF3 (E032).

## Los dos personajes

**Popolon** (E501 = 1) y **Aphrodite** (E501 = 0) [O].
- Cada uno tiene sus vidas (E058/E059), su vitalidad y su experiencia. La rutina de b01:618D actualiza la vitalidad poco a poco, a partir de un incremento pendiente por personaje (E05E y E061).
- **Cambio a voluntad**: en el menú de objetos (F1), sube el cursor hasta los personajes, pulsa espacio y sal del menú [O].
- **Cambio forzado**: al perder una vida, si al personaje no le quedan vidas, el juego pasa al otro (b00:4502) [C].

| | Popolon | Aphrodite |
|---|---|---|
| Altura máxima de salto | **54 px** | 42 px |
| Tabla del salto | b03:A951, 0x1A | b03:A953, 0x18 |
| En el agua | pierde 1 de vitalidad cada ~0,15 s y se ahoga en poco más de 1 s | pierde 1 cada 8–10 s |
| Puertas (b02:9A25) [C] | necesita el Necklace (0x0D) para el mundo 7 | necesita la Conch Shell (0x23) para el mundo 5 |
| Según la guía [G] | 2 flechas y 2 minas a la vez; el salto depende de cuánto mantienes ↑; empuja piedras y cruza puertas giratorias | 3 flechas y 3 minas; salto fijo |

Datos de la tabla:
- **Salto** [C][O]: medido manteniendo ↑, la mayor altura en varias salas.
- **Agua** [C][O]:
  - b03:B058 marca "en el agua" (E511) y lleva un contador (E524). Cuando el contador supera el umbral del personaje (Popolon 0, Aphrodite 0xFE), resta 1 de vitalidad y suena 0x25.
  - Medido en el mundo 2, sala 11, sin enemigos.
  - En el agua, b03:A103 no deja que Aphrodite baje de y = 0xB9, salvo en la sala 11 del mundo 7.

## Pendiente

- Las otras ramas por personaje del banco 3 (A9C1, AE9A, B06D, B301, B337) y las del banco 2 (904E, 9089, 96BA): posibles diferencias de alcance, daño o defensa.
- Qué hacen F5 y CTRL.
- Contrastar en el código lo que dice la guía [G]: número de flechas y minas por personaje, y salto variable de Popolon.
