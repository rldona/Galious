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
- **Arma secundaria** con M: cada disparo gasta 1 flecha (E046). Seis armas: 1 flecha, 2 flecha de cerámica, 3 fuego rodante, 4 fuego (las cuatro, en el castillo), 5 mina y 6 lupa (las dos, en santuarios). Detalle en «Armas secundarias», más abajo.
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
| Armas secundarias [C][O] | 2 proyectiles y 2 minas a la vez (3 flechas con Earrings, 3 minas con Bracelet); quita 5 a las piedras | 3 y 3; quita 1 a las piedras (5 con el Ring) |
| Según la guía [G] | el salto depende de cuánto mantienes ↑; empuja piedras y cruza puertas giratorias | salto fijo |

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
| Despegue | ↑ se lee en el tick siguiente (E500 = 1). **2 ticks de preparación**: el de la pulsación (aún anda 2 px) y uno quieto; sube en el tercero. Si se suelta ↑ en ellos, Popolon no salta: un toque de 4 frames solo mueve los pies (cambia de sprite) y uno de 10 ya da un salto corto de 19 px [O]. En el remake la entrada se lee en cada fotograma y se guarda hasta el tick siguiente, así que ni un toque corto ni soltar y volver a pulsar entre dos ticks se pierden |
| Subida | tabla **6 6 6 6 6 5 5 4 4 3 2 1**. Popolon la empieza en la entrada 0 (**54 px**). Aphrodite en la entrada 2 (**42 px**): las dos tablas de b03:A951/A953 son la misma, desplazada |
| Punto más alto | 4 ticks quieto |
| Bajada | tabla **1 2 3 4 4 5 5 6**, y luego 6 hasta tocar suelo (velocidad máxima de caída: 6 px/tick) |
| Soltar ↑ (solo Popolon) | tras 1 tick sin moverse, salta a la última entrada de la subida (1), hace los 4 ticks arriba y baja. Aphrodite hace siempre el salto entero |
| Borde de arriba | el salto no sale por arriba: la cabeza choca con el marcador por encima de y = 24 (mundo 1, sala 1: desde y = 56 sube hasta 37). Solo las escaleras llevan a la sala de arriba [O]. La franja del marcador no es pared: con la cabeza en ella el salto sigue avanzando de lado (castillo, sala 73, se puede saltar el hueco entre las dos plataformas de arriba) [I] |
| Chocar con el techo | igual que soltar ↑. El tope de la cabeza está entre 9 y 13 px por encima de los pies (medido en la sala 97: choca en y = 110 y el paso de 1 px aún entra). El remake mira el techo con el ancho del suelo (x − 4 … x + 4), para que una pared pegada al cuerpo tras una caída no corte el salto [I] |
| Control en el aire durante el salto | 2 px/tick subiendo y en el primer tick arriba; **3 px/tick** desde el segundo tick arriba hasta el tick en que aterriza |
| Caer por un borde (E500 = 2) | la caída empieza en la entrada "3" de la tabla de bajada (3 4 4 5 5 6…) y **no hay control horizontal** |
| Aterrizar | el último paso puede meter los pies en el suelo; se corrige al borde superior de la fila |
| Apoyo en el suelo | solo cuenta el centro de los pies, **x − 4 … x + 4** (las paredes sí usan la caja de 16 px, x − 8 … x + 7). Mundo 1, sala 2, hueco de 2 tiles en x = 120–135: se cae en x = 124/125 viniendo por la izquierda y en 130/131 por la derecha |
| Escalera (E500 = 3) | 1 px/tick. A media escalera ← y → no hacen nada, ni con ↑ (medido en el castillo, sala 1) |
| Escalera que atraviesa un suelo | bajando, al llegar al suelo **queda de pie** (E500 = 0) aunque se mantenga ↓; con una pulsación nueva de ↓ se engancha a la de abajo **cruzando la fila del suelo de golpe** (y 120 → 128). Subiendo, al entrar 1 px en la fila del suelo (y = 127) salta encima (y = 120) y queda de pie; para seguir por la de arriba hay que volver a pulsar ↑ |
| Pulsaciones | saltar y engancharse a una escalera piden **una pulsación nueva**: con ↑ mantenido solo hay un salto (300 frames medidos) |

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

**Golpes de los enemigos** [O][C] (`tools/openmsx/contact_measure.py` y `hitbox_measure.py`, con el esqueleto 0x25 del castillo, sala 104):

| | Valor |
|---|---|
| Daño | por tipo de enemigo, de la tabla **b04:64E5 + tipo** (b03:B39A); el 0x19 usa otra según su tamaño (1, 2, 4, 8). Con el Hat Helmet (E088) se divide entre 2, salvo si es 1 [C]. El esqueleto quita 3 [O] |
| Retroceso (E500 = 4) | **6 ticks a 4 px/tick** alejándose del enemigo, con un arco de −1 −1 0 0 +1 +1 px; acaba de pie [O] |
| Invulnerabilidad | E518 = 1 durante **42 ticks** (84 frames) tras el golpe [O] |
| Caja de contacto | horizontal: la del jugador (x − 8 … x + 7) contra la del enemigo (el esqueleto, de dos sprites, mide 32 px). Vertical: daña con los pies entre 2 y 30 px por debajo de la y del enemigo (el jugador cuenta como 16 px de alto) [O] |
| Espada contra enemigos | 1 de vida por ataque (2 con el Saber), desde 18 px (x + 18), igual que con las piedras [O][C] |

Estados de E500 vistos: 0 de pie o andando, 1 salto, 2 caída, 3 escalera, 4 retroceso, 7 arrastrado por una puerta giratoria.

## Armas secundarias

Código en b03 (grupo 0): entrada A72B, creación y movimiento A9CF–AE97, contra enemigos B492–B5E7, contra el jugador B40C, contra piedras B7D5–B85C y contra el jefe BB24–BD52. Medido con `tools/openmsx/weapon_measure.py` (salida en `logs/omsx/weapon_measure.txt`). Números para Unity: `extracted/unity/weapons.json` (`tools/extract/weapons.py`, que documenta cada campo). Valores en px y ticks (1 tick = 2 frames). Coordenadas: y del jugador = fila de arriba del tile que pisa (E505); x = su centro (E507).

**Disparo** [C][O]
- M pide **una pulsación nueva**: mantenerla no repite el disparo. Disparar **no para al jugador** ni tiene animación: se sigue andando o saltando.
- Sin flechas (E046 = 0) no sale nada. Las armas 1–5 gastan **1 flecha** al crearse el proyectil (b03:AA99). Si no se puede crear, no gasta.
- **Magic Rod** (0x09, E078): en el combate contra el jefe (E0D2 ≠ 0) no gasta flechas.
- Huecos: 3 para proyectiles (E540/E560/E580, cuenta E51C), compartidos por las armas 1–4, y 3 para minas (E5A0/E5C0/E5E0, cuenta E51D). Al cambiar de sala se borran todos.
- Límite a la vez (b03:A995–A9CA), contando todo lo vivo del mismo grupo de huecos:

  | | Popolon | Aphrodite |
  |---|---|---|
  | Flecha y cerámica | 2; **3 con Earrings** (0x1D, E08C) | 3 |
  | Fuego rodante y fuego | 2 (las Earrings no cuentan) | 3 |
  | Mina | 2; **3 con Bracelet** (0x1E, E08D) | 3 |

- Dónde se puede disparar:
  - **flechas**: de pie, saltando o cayendo (E500 < 4). No en la escalera (E503 ≥ 2) ni durante el retroceso;
  - **fuego rodante, fuego y mina**: solo de pie en el suelo (E500 = 0) y no sobre el ascensor (bit 0 de EC12/EC22). El fuego rodante y la mina, además, con E52A = 0.
- Orientación: E503 (0 derecha, 1 izquierda).
- Al crearlo, el proyectil ya se mueve una vez en ese mismo tick.
- Cualquier proyectil muere si, tras moverse, x < 8, x ≥ 248 o y ≥ 184 (b03:AD61).
- **Pared** = tile ≥ 0xBA (≥ 0xF0 durante el combate con el jefe).

**Cada arma** [C][O]

| Arma | Sale en (respecto a x, y del jugador) | Movimiento por tick | Termina |
|---|---|---|---|
| 1 Flecha | x (derecha) o x − 8 (izquierda); y' = (y − 8) redondeada a 8 hacia abajo, mínimo 32. Primera posición visible: x + 8 / x − 16 | **8 px** en línea recta | pared en su tile o en el siguiente; borde; **primer enemigo** |
| 2 Flecha de cerámica | igual | **4 px** en línea recta | pared o borde. **Atraviesa enemigos y jefes** |
| 3 Fuego rodante | x − 4, y − 8 | **4 px**, **rodea la plataforma** (abajo) | pared o suelo de frente; enemigo; al acabar la vuelta |
| 4 Fuego | x − 8, y − 8. El primer tick solo se asienta: baja 4 hasta quedar a 4 px del suelo | **4 px** por el suelo (y = suelo − 4). Si no hay suelo bajo (x, y + 4) ni bajo (x + 8, y + 4), **cae recto a 4 px/tick**; al tocar suelo sigue en el mismo sentido | pared, borde o enemigo |
| 5 Mina | x − 8, y − 4, quieta. Necesita suelo sólido en (x − 6, y) y (x + 7, y), es decir, bajo sus dos bordes | — | al estallar (abajo) |
| 6 Lupa | no crea nada (abajo) | | |

- **Fuego rodante** (estados 2–4 del hueco):
  - va por encima (y = suelo − 8);
  - donde se acaba la plataforma, baja por el lateral a 4 px/tick (x alineada a 8; +8 si iba a la izquierda);
  - cuando el lateral deja de ser sólido, pasa por debajo en sentido contrario (y alineada a 8; en la sala 3 del castillo, plataforma en y = 88–95: arriba y = 80, debajo y = 96);
  - sube por el otro lateral y vuelve arriba;
  - tras **una vuelta** muere al cruzar la x en la que nació;
  - si al bajar encuentra suelo debajo, o al subir encuentra techo, muere. En un suelo que ocupa toda la sala, simplemente corre hasta el borde.
- **Mina:**
  - Mientras el jugador siga encima no se arma: hace falta que salga de 0 ≤ y − y_mina < 21 y −8 ≤ x − x_mina ≤ 24. Hasta entonces nada la detona.
  - Armada, parpadea (colores 0F 07 05 04 05 07, uno cada 4 ticks). **No tiene mecha**: espera hasta que la toque un enemigo, el jefe o el jugador.
  - Al estallar se desplaza (y − 12, x − 8) y dura **70 ticks**. Suena 0x26.
  - **Daño al jugador**: 2 unidades de vitalidad (de 8), retroceso alejándose de la mina e invulnerabilidad normal. Una sola vez por mina. Con el jugador invulnerable (E518) la mina no lo ve [C][O].

**Daño** [C][O] (medido con el esqueleto 0x25, de 8 de vida, en la sala 104 del castillo)

| Arma | Enemigo | Con Saber (0x25, E094) | Jefe | Piedra (15 de vida) |
|---|---|---|---|---|
| Flecha, fuego rodante, fuego | 1, y el proyectil desaparece | 2 | 1, y desaparece aunque el jefe sea inmune | Popolon 5; Aphrodite 1, o 5 con el Ring |
| Cerámica | 1, y sigue. Vuelve a dañar al mismo enemigo cada vez que E604 es múltiplo de 16 (1 o 2 golpes al cruzar un esqueleto) | 2 | 1, igual cada 16 | igual que la flecha; la piedra la detiene |
| Mina | estalla al tocarla. **2 por golpe**, repetido cada vez que E604 es múltiplo de 32 mientras dura la explosión: hasta 3 golpes (8 → 6 → 4 → 2) | 4 por golpe | 2 | **no daña piedras** |

- **Jefe**: solo si el arma está en su máscara (b03:BD0A; bit 1 mina, 2 fuego rodante, 3 fuego, 4 cerámica, 5 flecha). Si no está, suena 0x1D (la mina, sin sonido). Ver `boss_ai.md`.
- **Inmunes**:
  - enemigos sin el bit 5 de IX+2 (no se les puede golpear);
  - vida FE/FF (invulnerables, `items_enemies.md`).
- **Escudos**: con el bit 2 de IX+2, o con el bit 1 si su lado (bit 0) no coincide con la orientación del proyectil, el enemigo lo **rebota**. Sin hacer daño, el proyectil invierte vx, sube a 8 px/tick y suena 0x1D. Las minas no rebotan.
- Los enemigos congelados con la Biblia (E0D5) siguen recibiendo daño [O].
- Sonidos: disparo 0x14 (flecha), 0x15 (cerámica), 0x16 (fuego rodante, fuego y mina). Golpe 0x1E, muerte 0x20, jefe 0x27.

**Cajas** (b03:B8B2, tabla B8EF) [C][O]. Caja cerrada del proyectil [y + dy, y + dy + alto] × [x + dx, x + dx + ancho]. Hay golpe si se solapa con la caja cerrada del enemigo, [ey + dy, ey + dy + alto] × [ex + dx, ex + dx + ancho] (b14:9006).

| Proyectil | dy, dx | alto × ancho |
|---|---|---|
| Flecha y cerámica | 2, 2, sobre (y, x) redondeados a 8 (se pintan como 2 tiles del fondo, no como sprite) | 3 × 12 |
| Fuego rodante | 0, 0 | 8 × 8 |
| Fuego | 0, 0 | 4 × 8 |
| Mina sin estallar | 0, 2 | 4 × 12 |
| Explosión | −16, 0, sobre la posición ya desplazada (es decir, −28, −8 desde donde se puso) | 32 × 32 |

- Piedra: [y, y + 16] × [x, x + 20].
- Jugador contra la mina: [y − 16, y] × [x − 8, x + 8]. Por ejemplo, la mina puesta en x_m estalla con el jugador en el mismo suelo y x entre x_m − 6 y x_m + 22.

**Lupa** (arma 6) [C]
- Con M, E517 = 1 (b03:AD46) y b03:B194 lo consume en el mismo tick. No gasta flechas.
- Si la sala tiene lápida (E678 ≠ 0) y el jugador está cerca, E01B = tipo de lápida y el juego pasa al **estado 12**, la pantalla del mensaje, de la que se sale con M. «Cerca» quiere decir y − 16 − y_lápida entre −16 y 16, y x − 8 − x_lápida entre −16 y 16.
- Las lápidas salen de b02:9C83 + 2·(E041 − 2), con 3 bytes por lápida (sala, tipo y posición). Tipo 0 → E678 = 0xFF; el tipo 1–10 está en una sola sala por mundo [I: la de la palabra del jefe]. La lista va en `weapons.json`, en `magnifier.tombstones`.

## Pendiente

- Las otras ramas por personaje del banco 3 (A9C1, AE9A, B06D, B301, B337) y las del banco 2 (904E, 9089, 96BA): posibles diferencias de alcance, daño o defensa.
- Qué hacen F5 y CTRL.
- Contrastar en el código lo que dice la guía [G] sobre el salto variable de Popolon (los límites de flechas y minas ya están comprobados).
- Armas secundarias: dónde se filtra M para que haga falta una pulsación nueva (A71E lee E008); qué hacen con IX+1C = 1 los tipos 0x0F, 0x25, 0x27, 0x49, 0x52 y 0x54 al recibir un proyectil (b03:B592); qué es E52A, que impide el fuego rodante y la mina; cajas propias de los tipos 0x06, 0x57, 0x60 y 0x62 (b03:B4DF).
