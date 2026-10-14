# Bucle de atracción: logo, título, demos e historia (estados 0–3)

Leyenda como en `state_machine.md`: **[C]** código · **[O]** observado en openMSX · **[I]** interpretación.
Datos para Unity: `extracted/unity/attract.json` (`tools/extract/attract.py`, lo llama `unity_bundle.py`; campos en la cabecera del módulo).
Medidas: `tools/openmsx/attract_measure.py` (un registro por frame desde el encendido y capturas en `logs/omsx/attract/`; `run1/` tiene la vuelta completa logo → título → demo 0 → logo → título → historia → logo → título → demo 1). Las capturas y los registros no se versionan.

Tiempos en frames de vídeo a 60 Hz. Los estados 0, 1 y 3 y la historia corren **una vez por frame** [C][O]. La demo corre en el motor del juego y cuenta **ticks** (medido: 2,2 frames por tick) [O].

## Ciclo [C][O]

```
encendido → 0 logo → 1 título → 2 demo 0 → 0 → 1 → 2 historia → 0 → 1 → 2 demo 1 → 0 → 1 → 2 historia → …
```

- **E025** cuenta las vueltas del estado 2: par = demo jugada, impar = historia. **E023** elige la demo (0 o 1) y cambia al acabar cada demo. Al encender, E023 = E025 = 0.
- Medido desde el encendido con C-BIOS: el juego arranca en el frame ~238. El título empieza en el 601 y la demo 0 en el 920. El logo vuelve en el 3360, la historia empieza en el 4055 y la demo 1 en el 6458.

**Teclas** (`attract_check_start`, b00:4BFA; se ejecuta después del estado en los estados 0–2) [C][O]:

| Estado | Tecla | Efecto |
|---|---|---|
| 0 | flecha, espacio o M (o el joystick), por flanco | título desde el principio; no cuenta vuelta |
| 1 | espacio o M (máscara 0x30) | E002 = 0x40 y estado 3. Las flechas no hacen nada |
| 1 | L **mantenida** (SNSMAT fila 4, bit 1; b02:98E5, se mira antes que el espacio) | estado 18, contraseña |
| 2 | flecha, espacio o M | título. La vuelta cuenta como hecha: E025++, y en una demo jugada también E023 ^= 1. Música 0 |
| 2 (demo) | F5 (fila 7, bit 1), por flanco | E00D ^= 1: calla la música de la demo o vuelve a pedirla. Los efectos suenan siempre (b01:7EDD, b00:43D8) |

Otras teclas del teclado no cuentan: b00:4C75 solo lee las flechas, el espacio, la M y el joystick.

## Estado 0: logo de Konami [C][O]

- **Sub 0**, 1 frame: registros del VDP (R7 = 0xE4, fondo azul oscuro), pantalla borrada y petición de sonido **0**, que silencia todo (b00:4E02). Se prepara el logo: un bloque de **21×6 tiles** en la fila 8, columna 7 (x 56, y 64), con patrones en RLE (b09:BCEF) y colores a 0.
- **Sub 1**, 96 frames: el logo aparece **de arriba abajo**. Cada 2 frames se pone en blanco una línea de píxeles de las 21 columnas a la vez: 6 filas × 8 líneas = 48 líneas (b01:6985).
- **Sub 2**, 256 frames: el logo se queda quieto (E004 = 0, cuenta 256). Al acabar, b00:42F1 monta el título, que tarda unos 10 frames.
- **Total**: 353 frames según el código y **366 medidos** [O]. No hay texto ni música.
- Termina en el estado 1, o antes con cualquier tecla.

## Estado 1: título [C][O]

**Pantalla** (fondo negro, R7 = 0xE0):
- El **logo** es un dibujo de 14×12 tiles en la fila 1, columna 9 (b14:933F): el escudo con THE MAZE OF GALIOUS y KNIGHTMARE II. Lleva además 4 sprites azules fijos (b14:93E7).
- **Textos** (fuente pequeña: 21–3A = A–Z, 10–19 = 0–9, 1A = ©; b01:7D51):

| Texto | Columna | Fila | Parpadea |
|---|---|---|---|
| ©KONAMI 1987 | 10 | 15 | no |
| PUSH SPACE KEY | 9 | 17 | **no** |

**Animación** (b01:6251, una vez por frame):
1. 48 frames quieto (F02B).
2. **Destello** por la espada: un sprite cian (b14:9407: y 0x47, x 0x60, patrón 0x10, color 7) avanza 2 px por frame hasta x 0x98, en 28 frames. Cambia de patrón (+4) en x 0x6E, 0x7C y 0x8A. Al llegar, se oculta.
3. **Estrella** en la punta (x 0xA0, y 0x3F): 4 dibujos de 16 frames (b14:93F7, 93FF, 93FF, 93F7). Cada paso empieza cuando E003 & 15 = 0, así que la fase dura 64 frames más una espera de 0 a 15.
4. 120 frames quieto (F02C). Después E019 = 0 → estado 2.

- **Duración**: 264–279 frames (medidos 264, 268 y 272) [O]. **Sin música**.

## Estado 2: demo jugada o historia [C][O]

Tabla de 4 subestados en b00:4340.

| Sub | Qué hace | Duración |
|---|---|---|
| 0 | **barrido**: una columna por frame (filas 4–23) de izquierda a derecha, 32 frames (b00:4D1E). Después prepara la demo (E025 par) o la historia (impar) | 55 frames antes de la demo, 61 antes de la historia [O] |
| 1 | demo jugada | demo 0: 1107 ticks = **2440 frames**; demo 1: 1292 ticks = **2743 frames** [O] |
| 2 | historia que sube | **1710 frames** [O] |
| 3 | espera a que calle la música (b00:50F5) | <1 frame: la orden FB del texto ya la ha parado |

Al acabar la vuelta: E025++. Si era una demo, también E023 ^= 1 (b00:43CB). Después se vuelve al estado 0 (E000 = FF y luego ++).

### Demo jugada [C][O]

**Es una secuencia de teclas grabada que se reproduce en el motor real.**
- Cada tick, b01:7E22 mete la combinación de la entrada actual como si fuera del teclado: E009 = mantenidas y E008 = flancos (b00:4C6B).
- El resto (física, enemigos, cambios de sala, daño) es el juego normal.
- **No hay texto de DEMO**: se ve el marcador normal (CASTLE).

**Preparación** (b01:7D82):
- Se inicia como una partida nueva (b00:4CB0), conservando E022/E023.
- Sitio de inicio: E041/E042 = palabra de b01:7DD9[E023].
- Arma: Arrow (E070 = 1), elegida (E510 = 1), y **100 flechas** (E046 = 0x0100, BCD).
- Música **0x88** (castillo), salvo que F5 la haya apagado (E00D).
- Al acabar la demo se borran E070, E510 y E046 (b00:438C).

**Fin de la música**: E033 = b01:7DE3[E023] (28 o 32). Se resta 1 cada vez que E604 & 0x1F = 0. E604 cuenta ticks y se pone a 0 en cada sala. Con E033 = 0 se pide 0xFF y la música calla antes del final: demo 0 en el tick 1026 de 1107, demo 1 en el 1140 de 1292 [O].

**Entradas** (b15:BAFD → b15:BB01 y b15:BBBF): pares (ticks, teclas).
- Teclas: bit 0 arriba, 1 abajo, 2 izquierda, 3 derecha, 4 espacio, 5 M.
- Una entrada dura *ticks* ticks, contando el tick en que se carga.
- Las teclas FF acaban la demo en ese mismo tick (E019 = 0).

| Demo | Inicio | Posición (x, y) | Entradas | Ticks | Salas recorridas [O] |
|---|---|---|---|---|---|
| 0 | castillo, sala 146 | (128, 56) | 95 | 1107 | 146 → 135 → 126 → 113 → 101 → 100 → 99 → 112 → 111 → 110 → 109 |
| 1 | castillo, sala 1 | (192, 120) | 125 | 1292 | 1 → 107 → 120 → 121 → 132 → 133 → 143 → 133 |

- **Posición inicial** (b00:528E): sale de la tabla b09:B106[E041], byte [E042]: y = (b & 0xF0) + 8, x = (b & 0x0F) · 16. Es la misma regla que al empezar partida.
- Al empezar la demo 0, Popolon está sobre una plataforma de bloques que sube por las salas 146 → 113 [O].
- Las listas completas, con cada tecla desglosada, están en `attract.json` (`demos[].inputs`).
- Teclas que usan las demos:
  - demo 0: 4 ←, 5 ↑←, 21 ↑←espacio, 20 ←espacio, 16 espacio, 32 M (dispara flechas), 2 ↓, 6 ↓←, 8 →, 1 ↑;
  - demo 1: 8 →, 9 ↑→, 41 ↑→M, 12 ←→, 36 ←M, 40 →M, 10 ↓→, 1 ↑, 2 ↓, 4 ←, 6 ↓←, 16 espacio, 32 M.

**Azar** [C][I]:
- No se siembra: rnd = R del Z80 rotado XOR E003 (b05:81F1), y E003 no se reinicia.
- Las demos de Konami están grabadas para que salgan bien con enemigos que varían poco.
- En el remake conviene reproducir la secuencia en un motor determinista y comprobar que llega a las mismas salas. Si no llega, se puede grabar una demo propia con las mismas salas.

### Historia (E025 impar) [C][O]

- Pantalla negra. Usa la rutina del epílogo (b01:6A1E inicio, 6ADC paso; ver `ending.md`) con el texto de **b15:AE28** (b01:6AC5, llamado desde b00:4369).
- Música **0x86** (Story), pedida directamente (b00:4371): suena aunque E00D la tenga apagada.
- La fuente es la grande de 8×16 del epílogo, y 0x45 es el apóstrofo.
- El texto entra por abajo y sube 1 px por paso: el código da un paso cada 2 frames, y medido son **3 frames**.
- Secuencia:
  1. 192 px de subida: STORY queda arriba (fila 0) y LATER... abajo (filas 22–23).
  2. Espera de 255 pasos (**513 frames**).
  3. Sube 10 filas más.
  4. FB: se para la música.
  5. Sube 14 filas más y FF: E02E = 0.
- Medido: 384 pasos × 3 frames + 513 = **1710 frames** [O].
- Solo hay versión en inglés. Texto (columna 13 el título y 2 el resto; cada línea ocupa 2 filas):

```
STORY
POPOLON WENT INTO MT.ATOS TO
RESCUE APHRODITE KIDNAPPED
BY HUDNOS. BUT IT WAS A TRAP
MADE BY GALIOUS. HE ATTACKED
THE CASTLE DURING POPOLON'S
ABSENCE AND ALSO KIDNAPPED
AND IMPRISONED A BABY FROM
THE HEAVEN WHO WAS SUPPOSED
TO BE BORN TO POPOLON AND
APHRODITE SEVERAL YEARS
LATER...
```

## Estado 3: inicio de partida [C][O]

- **Sub 0**, 1 frame: música **0x87** (castillo, con su entrada), y E004 = 45.
- **Sub 1**, 45 frames:
  - el título se queda, pero sin animar;
  - **PLAY START** (fila 17, columna 11; b01:7D71) sustituye a PUSH SPACE KEY;
  - parpadea: visible si E004 & 4 = 0, con E004 bajando de 44 a 1. Queda 1 frame oculto y luego alterna 4 visibles y 4 ocultos.
- **Sub 2**: borra la pantalla y los sprites e inicia la partida (b00:4CB0); E007 = 1 → estado 4.
- Después vienen el barrido de 32 frames y la carga de la sala, y el **estado 5**. Desde la tecla hasta el juego hay **104 frames** medidos (espacio en el frame 700, estado 3 en el 701, estado 4 en el 750, estado 5 en el 804).

**Estado inicial** (b00:4CB0, 4CE6, b02:99B3) [C][O]:

| Qué | Valor |
|---|---|
| Sala | castillo (E041 = 1), sala 1, x = 192, y = 120 (byte 0x7C de b09:B106), mirando a la derecha (E503 = 0) |
| Personaje | Popolon (E501 = 1) |
| Vitalidad | 8 de 8 los dos (E052/E053, E056/E057) |
| Experiencia | 0; máximo 0x30 (E051, E055) |
| Vidas de reserva | Aphrodite 1 (E058), Popolon 0 (E059): 2 vidas en total |
| Créditos de resurrección | 1 cada uno (E05A, E05B) |
| Flechas, monedas, llaves | 0 |
| Objetos y arma secundaria | ninguno (E070 = E510 = 0) |
| Puertas | todas cerradas (E062+ a 0) |
| Puerta del mundo 10 | E06E = R & 3 (al azar), E06D = sala de b02:99C4[E06E] = 9, 30, 39 o 1 |

- Se borran E016–E0FE y E200–F03E.
- Con otro cartucho de Konami en el segundo slot (F0F8, b00:4CF1), hay extras: 99 resurrecciones, y 100 flechas, monedas y llaves con vitalidad 0x30. No hace falta reproducirlo.

## Para el remake

- **Orden**: logo (366) → título (≈264 + 0–15) → barrido (≈55) → demo o historia, alternando, con las dos demos por turno.
- **Teclas**:
  - en el título, espacio o M empieza la partida y L mantenida va a la contraseña;
  - en el logo y en la demo, cualquier tecla de juego vuelve al título.
- Las demos están en `attract.json`: un componente de *input replay* que mete las teclas por tick en el mismo controlador del jugador. Al empezar, el estado de la demo es el de una partida nueva más el Arrow y 100 flechas.
- La historia reutiliza el componente del epílogo (`ending.json`) con `storyLines`, a 3 frames por px.
