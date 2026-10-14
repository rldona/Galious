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

- **Andar** a izquierda y derecha: 2 px/tick, 60 px/s [O] (ver «Física del movimiento»).
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
| En la lava | muere en ~1,2 s | igual que Popolon |
| Puertas (b02:9A25) [C] | necesita el Necklace (0x0D) para el mundo 7 | necesita la Conch Shell (0x23) para el mundo 5 |
| Según la guía [G] | 2 flechas y 2 minas a la vez; el salto depende de cuánto mantienes ↑; empuja piedras y cruza puertas giratorias | 3 flechas y 3 minas; salto fijo |

Datos de la tabla:
- **Salto** [C][O]: medido manteniendo ↑, la mayor altura en varias salas.
- **Agua** [C][O]:
  - b03:B058 marca "en el agua" (E511) y lleva un contador (E524). Cuando el contador supera el umbral del personaje (Popolon 0, Aphrodite 0xFE), resta 1 de vitalidad y suena 0x25.
  - Medido en el mundo 2, sala 11, sin enemigos.
  - En el agua, b03:A103 no deja que Aphrodite baje de y = 0xB9, salvo en la sala 11 del mundo 7.
- **Lava** [C][O]: tiles B8/B9, E511 = 2. El umbral es 3 para los dos personajes (b03:B07D). Medido en el mundo 8, sala 16: los dos se hunden y mueren en ~1,2 s. Detalles y Carpet en [world_maps.md](world_maps.md#agua-y-lava-co).

## Física del movimiento, medida frame a frame [O]

`tools/openmsx/jump_measure.py` coloca al jugador en la sala 1 del castillo (suelo en y = 120) y registra y, x y E500 en cada frame (`player_trace.tcl`). Datos crudos: `logs/omsx/jump_measure*.json`.

La lógica avanza un **tick cada 2 frames** (30 ticks/s a 60 Hz). Todos los valores van en px por tick.

| Movimiento | Valor |
|---|---|
| Andar | **2 px/tick** (60 px/s), sin aceleración ni frenada. El "52 px/s" de antes salía de una medida más basta |
| Despegue | ↑ se lee en el tick siguiente (E500 = 1). **2 ticks de preparación**: el de la pulsación (aún anda 2 px) y uno quieto; sube en el tercero. Si se suelta ↑ en ellos, Popolon no salta |
| Subida | tabla **6 6 6 6 6 5 5 4 4 3 2 1**. Popolon la empieza en la entrada 0 (**54 px**). Aphrodite en la entrada 2 (**42 px**): las dos tablas de b03:A951/A953 son la misma, desplazada |
| Punto más alto | 4 ticks quieto |
| Bajada | tabla **1 2 3 4 4 5 5 6**, y luego 6 hasta tocar suelo (velocidad máxima de caída: 6 px/tick) |
| Soltar ↑ (solo Popolon) | tras 1 tick sin moverse, salta a la última entrada de la subida (1), hace los 4 ticks arriba y baja. Aphrodite hace siempre el salto entero |
| Chocar con el techo | igual que soltar ↑. El tope de la cabeza está entre 9 y 13 px por encima de los pies (medido en la sala 97: choca en y = 110 y el paso de 1 px aún entra) |
| Control en el aire durante el salto | 2 px/tick subiendo y en el primer tick arriba; **3 px/tick** desde el segundo tick arriba hasta el tick en que aterriza |
| Caer por un borde (E500 = 2) | la caída empieza en la entrada "3" de la tabla de bajada (3 4 4 5 5 6…) y **no hay control horizontal** |
| Aterrizar | el último paso puede meter los pies en el suelo; se corrige al borde superior de la fila |
| Apoyo en el suelo | solo cuenta el centro de los pies, **x − 4 … x + 4** (las paredes sí usan la caja de 16 px, x − 8 … x + 7). Mundo 1, sala 2, hueco de 2 tiles en x = 120–135: se cae en x = 124/125 viniendo por la izquierda y en 130/131 por la derecha |
| Escalera (E500 = 3) | 1 px/tick |

**Espada** [O] (`tools/openmsx/sword_measure.py`, datos en `logs/omsx/sword_measure*.json`):

| | Valor |
|---|---|
| Duración del golpe | **4 ticks** (E508 = 2). Se activa con una pulsación nueva; mantener espacio no repite el golpe, y una pulsación hecha durante el golpe se ignora |
| En el suelo | el jugador **se para** mientras golpea |
| En el aire | se puede golpear y el movimiento en el aire sigue igual |
| En la escalera | no se puede golpear |
| Alcance | una piedra recibe el golpe si su borde está a **18 px o menos** del centro del jugador (x + 18); a 20 ya no. Probado desde la misma altura |
| Daño a las piedras | un golpe por ataque: Popolon quita 5 de 15 (3 golpes); Aphrodite, 1 (15 golpes) |

**Agua y lava** [O] (`tools/openmsx/liquid_measure.py`; agua: castillo, sala 137; lava: mundo 8, sala 16):

| | Valor |
|---|---|
| Vitalidad | se cuenta en **unidades**: al empezar, 8 de 8 (E052/E053 Aphrodite, E056/E057 Popolon). La barra mide 4 px por unidad (de ahí el "32" de la guía) |
| Movimiento vertical | a **1/4**: las tablas de salto y caída avanzan una entrada cada 4 ticks y cada paso se reparte en 4. Hundirse a velocidad máxima: 6/4 = 1,5 px/tick (1 y 2 alternos). Aphrodite salta los mismos 42 px desde el fondo, pero en unos 40 ticks |
| Movimiento horizontal | a la mitad: andar, 1 px/tick |
| Fondo | no se baja de y = 184 (en la lava del mundo 8, aunque la fila 19 también sea lava) |
| Daño | 1 unidad cada **4 ticks**: Popolon en el agua y los dos en la lava. Aphrodite en el agua, 1 cada **255 ticks** (contador E524). Con 8 unidades, Popolon se ahoga en ~1,1 s |
| Muerte | al llegar a 0: E500 = 5 y estado 6 (vida perdida) |

Estados de E500 vistos: 0 de pie o andando, 1 salto, 2 caída, 3 escalera, 4 retroceso, 7 arrastrado por una puerta giratoria.

## Pendiente

- Las otras ramas por personaje del banco 3 (A9C1, AE9A, B06D, B301, B337) y las del banco 2 (904E, 9089, 96BA): posibles diferencias de alcance, daño o defensa.
- Qué hacen F5 y CTRL.
- Contrastar en el código lo que dice la guía [G]: número de flechas y minas por personaje, y salto variable de Popolon.
