# Enemigos, objetos y superposiciones (fase 3)

Leyenda como en `state_machine.md`: **[C]** código · **[O]** observado en openMSX.

```
python3 tools/extract/entities.py        # extracted/entities/: entities.json, sprites_<tipo>.png
python3 tools/extract/overlays.py --check  # valida contra la VRAM real de las 320 salas
python3 tools/extract/render_rooms.py    # salas y mapas con superposiciones; <mundo>_entities.png
```

Todas las coordenadas son de pantalla en píxeles. La Y incluye las 32 líneas del marcador, así que la fila de tile es Y/8 y la Y dentro de la sala es Y − 32.

## Qué hay en cada sala (desde la ROM)

| Dato | Tabla | Bancos | Formato | Rutina |
|---|---|---|---|---|
| Tipos de sprite que carga la sala | 9737[E041] | 10–12 | bytes terminados en 0, una lista por sala | b00:5960 |
| Gráficos de cada tipo de sprite | 7C72[tipo] | 10–12 | entradas (hueco, puntero RLE) → patrones de sprite en 1800 + hueco·8 | b00:59A6 |
| Generadores de enemigos | B5CB[E041] | 4–6 | 2 bytes: `t` (tipo y bit 7), `p` → Y = p & F8, X = (p & 7) << 5 \| bit7 << 4; la lista acaba en un byte con `& 7F = 0` | b04:6000 |
| Objetos de sala | A7BC[E041] | 7–9 | 2 bytes: tipo, `p` → Y = p & E0, X = (p & 1F)·8; despacho por (tipo & 0F) | b08:9D00 |
| Objeto recogible (uno por sala como mucho) | B6FC[E041] | 9 | 3 bytes (sala, b1, b2): tipo = b1 & 3F, Y = b2 & F8, X = (b2 & 7) << 5 \| b1.7 << 4 \| b1.6 << 3 | b00:5DC9 |
| Piedras (solo castillo) | B8A2 | 9 | 1 byte por piedra, listas por sala: Y = (b & E0) \| 8, X = (b & 1F)·8 | b01:62EF |
| Escaleras extra | A6EB | 9 | pares (mundo, sala); datos de 5 bytes en A705 + 5k: Y, X, alto, ?, ? | b09:A761 |
| Emblema del jefe | 9BE2[E041] | 8 | puntero a 8×8 tiles; en (12, 8), o (12, 10) en el mundo 6; solo con E615 = 1 | b00:55CE |

Totales: 1134 generadores (10 son las piedras 0x5C de la sala 6 del mundo 10, `ending.md`), 125 objetos de sala, 129 objetos recogibles y 245 piedras.

**Persistencia** [C]: cada piedra tiene un bit (contador global E6A0 y campo de bits en (E60E)) y cada objeto recogible su flag en E06F + tipo, así que lo roto o recogido no vuelve. El estado 18 (nueva partida o contraseña) reinicia E016–E0FF, E200–EAFE y EC00–F03F.

**Mecánica descubierta** [C][O]: en el castillo, casi todos los objetos recogibles están **escondidos debajo de una piedra**, en la misma posición; al romperla aparece el objeto. Si se pinta el objeto encima de la piedra, falla la comparación con la VRAM en 68 salas; si se pinta debajo, coinciden.

**Comprobado jugando** [O] (castillo, sala 12, `tools/openmsx/break_item.py`): la piedra tiene 15 de vida (byte +6 de su entrada en EC30) y cae a espadazos. Al romperla queda a la vista el objeto (0x2D); al pisarlo suma 50 al primer contador. Al salir y volver, ni la piedra ni el objeto reaparecen.

## Superposiciones (b01:71D9)

Al cargar la sala, el banco 1 pinta sobre el mapa de nombres, en este orden: escaleras extra (b01:732F/733C, desde EC00/EC08), huecos (735F, desde EAC0/EAC9), piedras (737D, desde EC30, 3 entradas de 8 bytes)…

| Entidad | Sello (tiles) |
|---|---|
| Puerta de mundo | 4×3: `5A 5A 5A 5A / 5A 5B 5B 5A / 5A 5B 5B 5A` |
| Piedra | 2×2 `BA BB / BC BD`. El contenido de esos tiles depende de la zona: es el mismo sello que el cartel de los dioses |
| Cartel de Demeter (contraseña) | 2×2 `8D 93 / 8E 94` |
| Escalera extra | columna de `9A 9B` del alto indicado |
| Objeto recogible | tipos 7–2C: cofre 2×2 `76 77 / 78 79`; ≥ 2D: tiles propios (t − 2D)·4 + 60…; < 7: sin tiles (b01:6C56) |
| Generador 0x51 | estatua 2×3 `F1 F4 / F2 F5 / F3 F6` |
| Generador 0x32 | estatua que dispara flechas (el sprite 0x32 es la flecha), 2×3 `FA F7 / FB F8 / FC F9` |
| Generador 0x50 | 2×3 `F1 F2 / F5 F3 / F0 F0` |
| Generador 0x3A | 2×2 `F8 F7 / F9 FB` |
| Generador 0x04 | tile `99` |
| Objetos 1, 2, 3, 4, 0x13 | columna 1×3: `C5` · `C6 C7 C8` · `CF` · `D0 D1 D2` · `CF` |
| Objetos 7, 0x17, 0x27, 0x37 | pasadizo 4×3 (`F0`) |

**Validación** [O]: el mapa de nombres previsto (disposición + superposiciones) coincide tile a tile con la VRAM en **298 de 311 salas**. Las diferencias restantes:
- Castillo, sala 143: un cartel 8D 93 / 8E 94 donde la tabla de dioses da otro sello.

Con los eventos de sala (sección siguiente) y el destello de las armas secundarias, quedan **310 de 311** salas idénticas.

Con las superposiciones, el render se parece a las capturas en un 99,1 % en el castillo y un 98,2–99,0 % en los mundos. En la sala del jefe se queda en el 94 %, porque carga gráficos propios para los tiles del emblema.

## Eventos de sala: muros que se abren y escaleras que aparecen

`python3 tools/extract/room_events.py` → `extracted/entities/room_events.json`. No son tablas: son cadenas de comparaciones (mundo, sala) en el código del banco 8. El script las ejecuta con un trazador Z80 mínimo, sala por sala, y anota lo que escriben en la RAM.

**Enemigos marcados** [C][O]:
- Al cargar una sala de la lista b04:60AE (por mundo), E610 = 1.
- Cada enemigo que nace en ella, si su tipo está en b04:6584 (23 tipos: 2F 2D 24 0F 2E 22 0C 0E 43 10 2A 4C 16 1F 13 40 15 0A 34 14 12 07 17), suma 1 a E622 y queda marcado con el bit 3 de IX+2.
- Al morir uno marcado, b03:B6AE resta 1. Cuando llega a 0:
  - En un mundo se ejecuta el evento de la sala (b08:9D5F).
  - En el castillo solo se pone E61C = 1, que usa la rutina de objetos con condición (ver abajo).
- Suena 0x2E una sola vez, guardado con el bit 4 del byte persistente de la sala.

**Bloques de muro** [C][O]: un bloque 4×3 de ladrillo (tiles F0) tapa un paso. Está cerrado mientras su flag (E0A1–E0AC) vale 0, y desaparece cuando se cumple su condición:

| Mundo | Sala del muro | Se abre al… |
|---|---|---|
| 1 | 12 | matar a los marcados de la misma sala [O] |
| 2 | 13 | matar a los marcados de la sala 13 |
| 3 | 4 | **subir 5 veces seguidas** por el pozo de la sala 4, que enlaza hacia arriba consigo misma. E0A0 cuenta las subidas y salir hacia la izquierda por la parte baja lo reinicia (b00:479C) [O] |
| 4 | 15 | matar a los marcados de la sala 11 |
| 5 | 13 | matar a los marcados de la sala 14 |
| 6 | 8 | matar a los marcados de la sala 5 |
| 7 | 8 | matar a los marcados de la sala 8 |
| 7 | 14 | matar a los marcados de la sala 15 |
| 8 | 3 y 20 | matar a los marcados de la misma sala |
| 9 | 26 | matar a los marcados de la misma sala |

En el mundo 1, sala 12, el muro de arriba a la izquierda (filas 0–2, columnas 0–3) se abre en cuanto muere el cuarto enemigo marcado y queda abierto al volver.

**Escaleras ocultas** [C][O]: el evento activa el bit 3 del byte persistente de una sala. Al cargar esa sala, b08:9E61 dibuja una escalera extra (EC08: activa, alto, dirección en el mapa de nombres).
- Aparecen en la propia sala en el mundo 1 (10), el mundo 2 (7, 14), el mundo 3 (12, 15), el mundo 6 (11), el mundo 7 (18) y el mundo 9 (2, 10, 18).
- En otra sala: en el mundo 4, la sala 8 abre la escalera de la 15; en el 5, la 3 abre la 16; en el 6, la 15 abre la 8; en el 8, la 8 abre la 5.
- Comprobado en el mundo 1, sala 10 [O]: al morir el último marcado aparece una escalera de 3 tiles en la columna 21 y sigue ahí al volver.

**Puente** [C][O*]: en el mundo 2, sala 11, si tienes el objeto 0x12 (E081), aparece una fila de 16 tiles C4 sobre el agua (EAC8).

**Generadores periódicos por sala** [C]: no tocan los muros. Datos exportados en `extracted/unity/<mapa>.json`, `rooms[].periodic` (campos en `tools/extract/periodic.py`). Las fichas de los tipos creados están en `enemy_ai.md`, "IA de enemigos — generados periódicamente".
- **Al cargar la sala** (b00:516F → b04:6186): pone a 0 E650–E66F y llama a 61E4, 6200, 6239, 6249, 6291, 62A4, 62B9 y 61A8. Cada una arma su generador (bit 0 = 1) si coinciden el mundo (E041: 1 = castillo, n + 1 = mundo n) y la sala (E042).
- **En cada tick** (bucle principal b00:53CF → b04:62DB, la misma pasada que la IA de enemigos): avanzan 634A (E653), 6374 (E659), 639D (E65C), 63C3 (E65F), 63E9 (E662), 643A (E665), 6460 (E668) y 62F9 (E650). **Los temporizadores cuentan ticks** (1 tick = 2 frames). No avanzan mientras el libro congela (E0D5 ≠ 0).
- **Creación** (b04:65DA): A = tipo, E = x, D = y, B → E624, velocidad 0. Falla, sin reintento, si E620 ≥ E635 (16 enemigos) o si no caben sus sprites (E621 + n > E636). El temporizador sigue su curso y el intento se pierde. Los enemigos propios de la sala comparten ese límite. Es el único tope: no hay máximo por generador, salvo E668.
- **Ciclo de espera de los generadores simples** (E653, E659, E65C, E65F, E665): un tick para rearmarse (pone la espera) y la espera completa. Con espera 144 sale un intento cada **145 ticks**, el primero en el tick 145.
- **Tandas** (E650 y E662): contador = 3 (tabla 633A[E045]), primera espera 112 = 0x70 − 2·E045 y luego 32 entre intentos. Al vaciarse se rearma (1 tick). Los intentos caen en los ticks 113, 145 y 177, después en 290, 322 y 354, y así sucesivamente: 3 cada 177 ticks. La x sale de la tabla 6336[contador & 3] y el contador baja antes de leerla: **0x60, 0xC0, 0x40**, en ese orden.

| Gen. | Mundo: salas | Tipo | Primer intento / periodo (ticks) | Nace en (x, y) | Condiciones | ¿Se acaba? |
|---|---|---|---|---|---|---|
| E650 | castillo: 58, 71, 113, 114 | 0x09 bola que rebota | tandas de 3: 113, +32, +32; la siguiente, 113 después | x = 0x60, 0xC0, 0x40; y = 0 | — | no |
| E653 | castillo: 144–147; mundo 8: 15–19; mundo 9: 3–5; mundo 10: 2–3 | 0x08 bola de fuego | espera al azar: 1 + (R & 0x3F) + 0x30 = **49..112**, también la primera | y = 0xA0 (0xB0 en el mundo 8); la x la decide el tipo: 64..191 | **no se arma si se tiene el Carpet** (E087, objeto 0x18) al entrar en la sala (b04:6200) | no |
| E659 | castillo: 137–140 | 0x0D pez saltador | 145 / 145 | y = 0x90; x según el jugador (ver ficha); B = sala − 136 (1..4) | el temporizador no avanza mientras el jugador está en agua o lava (E511 ≠ 0) | no |
| E65C | castillo: 10, 17, 28, 56, 73, 80, 122, 131, 132; mundo 4: 10, 12; mundo 6: 8 | 0x21 caminante | 145 / 145 | y = 0; x al azar en 16–47, 80–111, 144–175 o 208–239 | — | no |
| E65F | mundo 5: 7–10 | 0x30 saltador de lava | 145 / 145 | y = 0x90; x al azar en 32–95 o 160–223 | — | no |
| E662 | castillo: 94, 95, 102 | 0x38 bola que sube | tandas, igual que E650 | x = 0x60, 0xC0, 0x40; y = 0xB0 | — | no |
| E665 | castillo: 47, 59; mundo 8: 2, 14 | 0x35 bola rodante | 145 / 145 | y = 0; x al azar, igual que 0x21 | — | no |
| E668 | mundo 6: 2, 3, 4, 7, 8, 17, 18 | 0x3E volador en zigzag | 16 / 16 | y = 0x28 (lo pone el tipo). Con 10, los 5 primeros en x = 0xB0 y los 5 últimos en x = 0x40; con 5, todos en x = 0x40 | — | **sí**: 5 intentos (salas 2, 4 y 18) o 10 (salas 3, 7, 8 y 17) |

- **Posición.** El generador pasa una x en E, pero solo la usan 0x09 y 0x38. En E653, E659, E65C, E65F y E665 la E ni siquiera se carga (es basura), y la rutina de inicio del tipo la sobrescribe. 0x3E sobrescribe x e y. Ninguno mira la posición del jugador, salvo el 0x0D.
- **Aleatorio.** R es el registro de refresco del Z80; en la remasterización basta un aleatorio uniforme. La x de 0x21, 0x35 y 0x30 se calcula como (R/8) XOR E003. La de 0x08 sale de (E604 & 0x7F) + 0x40, con E604 el contador de ticks.
- **E045** vale 0 en la práctica. Con otro valor cambia el tamaño de tanda (633A: 3,3,3,4,4,5,5,6,6,6,7,7,7,8,8,8), la primera espera de E650/E662 (0x70 − 2·E045) y la espera de los simples (0x90 − 2·E045).
- El mundo 6, sala 8, tiene dos generadores: E65C y E668.

## Objetos de sala: puertas, muros que se cierran y paredes que se rompen [C]

Al cargar la sala, b08:9D4C reparte cada objeto de la tabla A7BC según el nibble bajo de su tipo. Cada clase tiene su estado en RAM y su rutina por frame (b00:5376 las llama):

| Clase | Estado | Qué es | Dónde |
|---|---|---|---|
| 1 (tipo 0x01) | EA00, 2 huecos, columna C5 | **puerta con llave del castillo**: empujando contra ella unos 30 frames, a su altura exacta, gasta 1 llave (b09:A1F2, BCD en E04A) y suena 0x22 [C] | 15 salas del castillo |
| 2 (0x02) | EA30, 2 huecos, C6 C7 C8 | **puerta con llave de los mundos**: lo mismo, con sonido 0x21; no actúa si no tienes llaves [C] | 34 en los mundos |
| 3 (0x03, 0x13) | EA50, 1 hueco, CF; el bit 4 del tipo da el sentido | **puerta giratoria de un solo sentido** (b09:A20A, por frame A22D) [C]. Gira si el jugador está en estado normal (E500 = 0), a la altura exacta (y = y_obj + 0x18), pegado a ella y empujando 30 frames en el único sentido que admite: tipo 0x03 → **hacia la izquierda** (desde x = x_obj + 0x10, mirando a la izquierda); tipo 0x13 → **hacia la derecha** (desde x = x_obj − 8). Entonces el jugador pasa a E500 = 7 (lo arrastra la puerta), la puerta da 4 pasos de 6 frames y suena 0x23. No consulta el Bread & Water: la cruzan los dos personajes, pero solo en un sentido | 15 en los mundos |
| 4 (0x04) | EA60, 4 huecos, D0 D1 D2 | **puertas giratorias**: su rutina consulta E098, la flag del Bread & Water [C]. Según la guía [G], Popolon las cruza siempre y Aphrodite solo con ese objeto | 23 en los mundos |
| 5 (0x05) | EAA0 | **muro que se cierra a tu espalda** (b09:A415–A4EC) [C][O]. Al pasar por su columna anota tu dirección; cuando avanzas 16 px más, crece un tile cada 15 frames hasta 3 y marca la sala (bit 1 persistente), así que al volver ya está cerrado | 16 salas del castillo |
| 6 (0x06) | EAB0 | **piedras que aparecen para cruzar el agua** (b09:A52E–A62F) [C]: esperando en el borde de una orilla (y = 0x78, x en 0x18…0x1F o 0xE0…0xE7) 300 pasos sale la primera; tras 128 pasos encima de cada una sale la siguiente y se va la anterior (x = 0x38, 0x60, 0x90, 0xB8; sonido 0x04); tras la cuarta se va la última, y al llegar a la otra orilla se vuelve a armar | mundo 5, sala 11 |
| 7 (0x07–0x37) | EA20 (se dibuja como bloque 4×3 de F0) | **pared que se rompe a espadazos**, ver abajo [C][O] | castillo 97 (camino de la cruz), mundo 2 sala 4, mundo 4 sala 16, mundo 5 sala 7, mundo 6 sala 6, mundo 9 sala 26 |

### Puertas con llave, medidas en openMSX [O]

`tools/openmsx/keydoor_measure.py`, con 2 llaves. Castillo, sala 16 (clase 1); mundo 1, sala 4 (clase 2).
- **Geometría:** columna de 1 × 3 tiles en (x, y), entre techo y suelo; el jugador la empuja desde x − 8 o x + 16, a y + 0x18.
- **Empujar:** 30 ticks seguidos (el temporizador +7 baja de 30 a 1 y se reinicia si dejas de empujar). Al acabar gasta una llave (E04A, BCD) y el jugador pasa a E500 = 7.
- **Apertura:** la puerta encoge de 3 a 1 tile en 2 pasos de 6 ticks (+4). Luego arrastra al jugador 12 ticks a 2 px/tick (24 px) hasta el otro lado.
- **Castillo (clase 1):** se vuelve a cerrar en cuanto pasas (vuelve a 3 tiles y a la fase 0), así que **cada paso cuesta una llave**. Funciona en los dos sentidos.
- **Mundos (clase 2):** tras pasar, su entrada en EA30 queda a 0. La tomo como **abierta para siempre** [I]: la vuelta desde el otro lado no se pudo medir, porque el arrastre sacó al jugador de la sala.

### Puertas giratorias, muros que se cierran y paredes que se rompen, medidos en openMSX [O]

`tools/openmsx/room_object_measure.py`. Datos en `logs/omsx/room_object_measure.json`.
- **Puertas giratorias (clases 3 y 4):** empujar **30 ticks** pegado a la puerta (x − 8 o x + 16, y + 0x18). Luego la puerta gira (4 pasos de 6 ticks) y arrastra al jugador (E500 = 7) **24 px a 1 px/tick** hasta el otro lado. La puerta sigue cerrada después.
  - Clase 3 (mundo 2, sala 6, tipo 0x03): pasa hacia la izquierda; empujando hacia la derecha no ocurre nada.
  - Clase 4 (mundo 1, sala 7): Popolon pasa en los dos sentidos; **Aphrodite no la mueve** sin el Bread & Water.
- **Muro que se cierra (clase 5; castillo, sala 48, columna en x = 160):** anota tu sentido cuando tu centro pasa la columna (x = 168). Empieza a cerrarse cuando estás entero al otro lado (x = 176). Crece **desde arriba, 1 tile cada 15 ticks**, hasta 3 (+4 de su entrada en EAA0). La captura `logs/omsx/wall5_after.png` lo muestra.
  - Para medirlo hubo que forzar la invulnerabilidad (E518 = 1): los enemigos de la sala echaban atrás al jugador (E500 = 4).
- **Pared que se rompe (clase 7; castillo, sala 97, tipo 0x37):** Popolon la rompe con **3 espadazos**, uno por ataque. Aphrodite sin el Pendant no le quita nada.

**Contraste con el mapa de mars2000you** (marcas N, "muro temporal que aparece después de pasar"): 14 de sus 16 marcas N caen en salas con objeto de clase 5. Sus marcas T, "escalera que puede desaparecer", caen en 12 de las 13 salas de la tabla de escaleras extra. Sus marcas FY, puntos donde aparece un hada, están en las salas 25, 91 y 110 del castillo (ver "Hadas").

## Objetos que aparecen con una condición [C][O]

b02:9F82 (sigue en b03 a partir de A000) se ejecuta en el castillo, como mucho una vez por sala (E539). Si se cumple la condición, crea el objeto en la posición indicada (b00:5D88, en E740) y suena 0x2E:

| Sala | Objeto | Posición (x, y) | Condición |
|---|---|---|---|
| 48 | Hoi (0x13) | 0x50, 0x28 | matar a todos los enemigos marcados de la sala (E61C) [O: 7 enemigos 0x34] |
| 75 | Candle (0x16) | 0x20, 0xA8 | igual (2 caballeros 0x0A) [O*: con E61C forzado aparece]. No logré matar al segundo caballero en la prueba: muy pocos golpes le quitan vida |
| 24 | Saber (0x25) | 0xD0, 0x68 | igual, y además el bit 2 de E068 (un jefe derrotado; según la guía, el del mundo 6) [O: matando a los 4 murciélagos 0x0E. Con E068 forzado aparece; sin él, la sala queda limpia y no aparece] |
| 40 | Vase (0x1B) | 0x80, 0x88 | bit 2 de E066 (según la guía, tras recuperar el mundo 4) y una **secuencia de 9 pasos** contados en E536: 3 espadazos mirando a la derecha (E503 = 0), 3 mirando a la izquierda y 3 saltos. Un paso fuera de orden pone la cuenta a 0. **No comprueba la posición del jugador** [O: forzando E066, desde la entrada de la sala] |
| 43 | Dagger (0x26) | 0x60, 0x28 | estar **sobre el ascensor** (b09:A8CE, bit 0 de EC12/EC22) y pulsar **derecha, abajo, izquierda y arriba** (tabla A056). Cualquier otra tecla reinicia. Cada tecla debe mantenerse al menos 2 frames [O]. No hace falta matar a los murciélagos |

Las otras salas de evento del castillo (35, 53, 69, 77, 101 y 105) también activan E61C al limpiarlas, pero no se ha encontrado nada más que lo lea.

## Ascensores [C][O]

Bloques de 32 × 16 px (dos bloques de 16 × 16): ocupan x … x + 31, y … y + 15, y el jugador va de pie sobre su cara de arriba (sus pies en y). Solo hay ascensores en el castillo (E041 = 1). Datos: `tools/extract/lifts.py` → `extracted/unity/lifts.json`. Medidas: `tools/openmsx/lift_measure.py` (la demo 0 entera, y 160 s en la sala 1 para seguir los seis verticales).

**Verticales** (E4A0, 6 de 16 bytes; el de la sala actual se copia en EC10):
- Cada uno recorre una columna de salas a **2 px/tick** y **sigue moviéndose aunque el jugador esté en otra sala o en un mundo**. Solo se paran durante la carga de una sala.
- Subiendo, al bajar de y = 0x28 pasa a la sala de arriba y aparece en y = 0xB9. Bajando, al llegar a y = 0xC0 pasa a la de abajo y aparece en y = 0x28.
- Dan la vuelta en sus salas de los extremos: abajo, al llegar a su y (… 142, 144, 142); arriba, cuando y + 1 < su y (con la y de arriba 120: … 121, 119, 117, 119).
- Al empezar la partida, cada uno está en su sala de arriba, bajando (b01:63C2, tablas b09:BB24/BB54). La demo 0 pone el 6 en la sala 146, en y = 64, subiendo (b00:5FA8).

| Id | Salas (de arriba abajo) | x | Vuelta arriba | Vuelta abajo |
|---|---|---|---|---|
| 6 | 80 88 101 113 126 135 146 | 112 | sala 80, y 120 | sala 146, y 144 |
| 5 | 68 82 89 103 116 129 138 | 112 | 68, 88 | 138, 120 |
| 4 | 17 28 36 49 56 | 112 | 17, 56 | 56, 120 |
| 3 | 8 16 26 35 46 | 112 | 8, 88 | 46, 144 |
| 2 | 14 24 34 | 112 | 14, 88 | 34, 152 |
| 1 | 13 22 33 41 52 | 112 | 13, 152 | 52, 120 |

**Horizontales** (EC20; tabla b09:BA53): uno por sala, en las salas 5, 7, 8, 10, 22, 24, 28, 36, 37, 43, 46, 102, 126, 129 y 140.
- Vuelven a su sitio cada vez que se entra en la sala y van a 2 px/tick, primero hacia la derecha.
- Dan la vuelta mirando la x de **antes** de moverse, así que se pasan 2 px. En la sala 43, con los límites 48 y 176, el recorrido es 174, 176, 178, 176… y 50, 48, 46, 48. El jugador que va encima se desplaza lo mismo que el ascensor.

**Orden dentro del tick** (punto de ruptura en b03:B0C2 durante la demo 0):
1. Se mueve el jugador.
2. Se mira la colisión, con el ascensor aún donde estaba.
3. En el frame siguiente se mueven los ascensores y llevan al jugador (b03:A916): los pies a la altura del ascensor y, en el horizontal, lo que este se ha movido.

**Colisión** (b03:B0C2, cada ascensor de la sala; lx, ly = esquina del ascensor; x, y = centro de los pies):
- **Al bajarse:** 7 ticks sin colisión con ese ascensor (bits 3–6 de EC12/EC22).
- **Por los lados:** con los pies entre ly + 1 y ly + 31, si x − lx está entre −8 y −5, lo aparta a x = lx − 8; si está entre 37 y 40, a x = lx + 40. Un horizontal empuja así al jugador que está a su lado.
- **Encima:** con x − lx entre −2 y 34 y los pies entre ly y ly + 7, va encima. Basta con que ya fuera encima o con que no haya suelo de tiles bajo los pies (E519 bit 3).
- **Por debajo:** con los pies entre ly + 24 y ly + 31 y sin suelo de tiles, lo empuja a y = ly + 32. No se puede atravesar saltando desde abajo.
- **Como suelo:** ir encima (b03:A8CE) cuenta como suelo: de pie, saltando y cayendo (también en los 4 ticks quieto antes de caer). Encima no se pueden usar el fuego rodante, el fuego ni la mina.

**Cambio de sala** (b03:A7F8):
- Si un vertical cambia de sala con el jugador encima, el jugador va con él y aparece a su altura.
- Si se salta desde un vertical (E52B/E52D) y la y baja de 0x28, se sale por arriba aunque no haya escalera. Si ese ascensor sube, pasa también a la sala de arriba en el tick siguiente.
- La Dagger de la sala 43 se consigue sobre su ascensor horizontal (ver "Objetos que aparecen con una condición").

## Paredes que suenan y se rompen [C][O]

Son los objetos de clase 7 (b09:A634).
- **Estado:** EA20 = activa, posición y golpes restantes. Nace rota si el bit 2 del byte persistente de la sala está puesto.
- **Golpes necesarios** según el nibble alto del tipo (tabla A65D):

  | Tipo | Golpes |
  |---|---|
  | 0x07 | 30 |
  | 0x17 | 20 |
  | 0x27 | 10 |
  | 0x37 | 3 |

- **El golpe** (E526) cuenta si el jugador está a la altura de la pared y a menos de 14 px de su lado, mirando hacia ella.
- **Sonidos:** cada golpe suena **0x1E**, que es la pista de "la pared suena" de la guía. Al llegar a 0 la pared desaparece, se marca la sala (bit 2) y suena 0x2E.
- **Quién puede romperla:** b00:4DDB deja a **Popolon siempre**, y a **Aphrodite solo con el Pendant** (0x1C).
- **Comprobado en la sala 97 del castillo** (tipo 0x37, camino de la cruz): Popolon la rompe en 3 golpes; Aphrodite sin Pendant no le hace nada; con Pendant, 3 golpes.

## Hadas [C][O]

En las salas 25, 91 y 110 del castillo hay 12 enemigos **0x4E**: "polvo mágico" que deambula y no se puede golpear.
- **Cómo se invoca:** hay que quedarse quieto en un punto concreto durante 112 ticks de IA, unos 3,7 s (b04:8247):
  - sala 110: x = 0xC0–0xCF, y = 0x58;
  - sala 25: x = 0x60–0x6F, y = 0x58;
  - sala 91: x = 0x60–0x6F, y = 0x78.

  Coinciden con las marcas FY del mapa de mars2000you.
- **Qué pasa:** el polvo vuela al centro (0x70, 0x80). Cuando llega la última partícula aparece el **hada (0x4F)**, que revolotea por la sala. La primera vez suena 0x83 y se pone E63D = 1.
- **Comprobado en la sala 25** con Popolon a 6 de vitalidad: tras 4 s aparece el hada; al tocarla la vitalidad sube al máximo (8 en esa partida) y el hada desaparece.

## Santuarios y tiendas [C][O]

Resumen; el detalle está en `design_bible/08_santuarios.md` y los datos en `extracted/unity/shrines.json` (`tools/extract/shrines.py`). El tipo de sala de dios (E690) elige qué pasa en ella; la lógica por tipo está en b02:8F43:
- 8F72: la Muerte;
- 90C0: contraseña;
- 9126: tienda;
- 92E8: pista.

**Entrada:**
- **Castillo** (objeto E680, estado 16): ↓ mantenido con y = godY + 16…17 y x = godX + 6…10 (b02:9A5F). No vale con E500 = 4…7. El tipo 3 exige E099 (Salt). Casi siempre la casilla está tapada por una piedra.
- **Mundos:** la última sala (tablas b02:8F0A/8F14, mundos 1–9) pone E690 al cargarse y el estado 10 pasa al 14. Se llega subiendo por una escalera de la fila 0 de la sala de abajo, oculta salvo en los mundos 1 y 7.
- **Salida:** ↓.

**Tiendas** (b02:9253 + 2·tipo, tres pares precio BCD/objeto; compra en b02:918C):
- Escudo de plata o de oro fuera de orden (E097 + 0x2A < objeto): sonido 0x2F.
- Sin monedas (E048 < precio): mensaje 0x12 y sonido 0x2F.
- Si se compra: cobra (b02:922B) y aplica según el objeto:
  - carcaj: +100 flechas (2 × 0x50);
  - llave: +1;
  - escudos: E097 = 1/2/3;
  - el resto: E06F + objeto = 1.
- Después suena 0x2D y el hueco queda vendido (E6CC + hueco) hasta salir. No mira si ya tienes el objeto.

| Tipo | Sala | Dios (b02:954C) | Qué hay |
|---|---|---|---|
| 1 | castillo 72 | Demeter | contraseña |
| 2 | castillo 115 | Hefesto | Mine 30, llave 5, carcaj 20 |
| 3 | castillo 87 | la Muerte | resurrección: 100 flechas, 100 monedas y 20 llaves (b02:8FCE); un crédito por personaje (E05A/E05B) |
| 4 | castillo 143 | Ares | carcaj 20, Copper Shield 30, Magnifying Glass 20 |
| 5 | castillo 65 | Ares | carcaj 20, Silver Shield 40, llave 5 |
| 6 | castillo 5 | Hefesto | Harp 30, carcaj 20, llave 5 |
| 7 | castillo 55 | Hefesto | carcaj 20, Conch Shell 25, llave 5 |
| 8 | castillo 54 | Asclepio | solo pista: "SEE A FAIRY TO RESTORE YOUR VITALITY." (no cura) |
| 9 | castillo 4 | Ares | carcaj 20, Gold Shield 50, llave 5 |
| 10 | castillo 10 | Ares | carcaj 20, Hat Helmet 50, llave 5 |
| 11 | castillo 32 | Demeter | carcaj 20, Necklace 25, llave 5 |
| 12 | mundo 5, sala 18 | Demeter | Oar 30 |
| 13 | mundo 8, sala 21 | Demeter | Carpet 30 |
| 14–20 | salas especiales de los mundos 1, 2, 3, 4, 6, 7 y 9 | Demeter / Atenea | una pista (b02:9308) |

**Mensajes:** tabla b15:B09E. Cada uno es dirección destino en ED80 + tiles + FF; FE = línea nueva. El inglés es id + 0x20 cuando la BIOS no es japonesa (0x002B & 0x0F ≠ 0).
- Tiles: 1–10 = 0–9; 0x0B–0x24 = A–Z; 0x26 ×, 0x27 !, 0x29 ?, 0x2A ., 0x2B ,; 0x54/0x56/0x58 = iconos de flechas, monedas y llaves (2 tiles).
- Los nombres de los dioses son los mensajes 0x18 + dios.

## Escaleras que se retiran [C][O]

Las 13 escaleras extra del castillo (tabla b09:A6EB/A705, 5 bytes: y, x, alto, largo en píxeles, **modo**) se retiran después de usarlas.
- **Cuándo:** al terminar de recorrerla, b03:A608 compara el modo (EC0F) con la dirección en que la has usado (E50C: 1 subiendo, 2 bajando).
  - Modo 1: se retira al subirla.
  - Modo 2: se retira al bajarla.
  - Modo 3: se retira en los dos casos.
  - Se activa EC07 y suena 0x2E.
- **Cuánto tiempo:** b09:A7A4 pone el temporizador de esa escalera, E0AE + 2·n (n = EC00, el número de la escalera), a 60. Baja 1 por segundo aunque estés en otra sala. Mientras no llegue a 0, la escalera no se dibuja al entrar.
- **Comprobado** [O] en el castillo, sala 13 (modo 3): al bajarla desaparece entera en cuanto llegas abajo. El temporizador baja de 60 a 49 en 12 s y sigue contando desde la sala 12. Al volver con el temporizador a 0, la escalera está otra vez.
- Las salas 19 y 102 (modo 1) solo la retiran al subirla [O en la 19].
- Efecto de diseño [I]: no puedes volver por donde has venido hasta pasado un minuto.

## Barrido de tiles en openMSX [O]

`tools/openmsx/explore_tiles.py` recorre cada sala con el jugador invencible: en cada altura de suelo (y = 56, 88, 120, 152, 184) anda de un borde al otro saltando a ratos, vuelve, hace una tercera pasada pulsando abajo, y sube y baja cada escalera (pasadas `L<x>u` / `L<x>d`). `tile_watch.tcl` anota cada tile que cambia en plena partida. `tools/openmsx/analyze_tiles.py` quita el ruido (el redibujado al cambiar de sala y los objetos recogidos) y agrupa lo demás; resultado en `logs/omsx/analyze_tiles.txt`.

**Cobertura:** las 147 salas del castillo (1–19 en una primera pasada; 20–83 y 84–147 en `explore_a` y `explore_c`) y las 164 de los 10 mundos (`explore_b`). Sin el filtro de "tile anterior 00" solo aparecen redibujados completos de sala, así que no se pierde ningún muro que salga sobre un fondo vacío.

**Resultado:** ninguna mecánica nueva. Cada cambio corresponde a algo ya leído en el código, y ahora queda comprobado:

| Qué | Dónde | Tiles | Ya documentado en |
|---|---|---|---|
| **Muro que se cierra a tu espalda** (clase 5): una columna de 1 × 3 tiles se vuelve ladrillo F0, de arriba abajo | castillo 32, 44, 45, 61, 67, 70, 79, 90, 99 y 132, siempre en la columna del objeto (x / 8) y desde su fila (y − 32) / 8. En algunas pasadas solo se ven 1 o 2 tiles porque la pasada acaba antes | fondo (A2…B5, 9C…9E) → F0 | "Objetos de sala", clase 5 |
| **Escalera que se retira**: las 2 columnas × 7 filas pasan a fondo de golpe, al terminar de bajarla | castillo 13, 70, 86, 106, 116 y 133, en la posición de su escalera extra (modo 2 o 3) | 9A 9B → el ladrillo del fondo (A2…B5 o 9C…9F según la sala) | "Escaleras que se retiran" |
| **Bocas de pared** 0x3A y 0x50: al abrirse cambian 2 tiles de la estatua | castillo 3 y 16, con el jugador por debajo | F8 FB → FA FC (0x3A) y F2 F5 → F4 F6 (0x50) | `enemy_ai.md`, 0x3A / 0x50 |
| **Pistones** 0x04: su columna se dibuja con tiles que suben y bajan | castillo 117 y 128 | 99 (base), 9C 9D B4 B5 | `enemy_ai.md`, 0x04 |
| **Puertas giratorias** (clases 3 y 4): giran como un bloque de 3 × 3 tiles al empujarlas | mundo 6, sala 10; mundo 7, salas 6 y 8 | los tiles de la puerta (84…E0) entre sí | "Objetos de sala", clases 3 y 4 |

Detalles nuevos que deja el barrido:
- **Los pistones no son sólidos:** sus tiles (99, 9C, 9D, B4, B5) están por debajo de 0xBA, el primer tile sólido. Solo hacen daño al tocarlos. El remake ya los trata así.
- **La escalera desaparece entera de una vez**, sin animación: los 14 tiles cambian en el mismo momento.
- **En los mundos no aparece ningún muro al pasar.** Los únicos cambios de tiles son las puertas giratorias; los bloques de muro y las escaleras de los eventos de sala dependen de matar a los enemigos marcados y el barrido no mata a nadie.

**Comprobado después** [O] (`tools/openmsx/wall5_check.py`, `ladder_check.py`):
- **Muros que se cierran** de las salas 15, 17, 29 y 41: el objeto pasa por las mismas fases que el de la 48 (anota el sentido, cuenta 3 tiles cada 15 ticks). Los tiles nuevos se escriben en la VRAM, no en la copia del mapa de nombres de la RAM. En la 76 hay piedras a los dos lados de la columna: se anota el sentido, pero no se llega al punto de cierre sin romperlas.
- **Escaleras que se retiran** de las salas 21, 60, 73, 102, 124 y 136: todas activan EC07 al recorrerlas en su modo. Las de modo 3 (21 y 124) ya al subirlas.

**Lo que el barrido no llegó a activar** (no quiere decir que no funcione):
- Muros que se cierran de las salas 15, 17, 29, 41, 48 y 76 (el de la 48 está medido aparte, con `room_object_measure.py`).
- Escaleras que se retiran de las salas 19 y 102 (modo 1, al subir; la de la 19 está medida aparte), 21, 60, 73, 124 y 136.
- Los eventos de sala (enemigos marcados) y los objetos con condición.

## Catálogo de sprites

`extracted/entities/sprites_<tipo>.png` pinta los sprites de 16×16 que carga cada tipo, en blanco. Algunos reconocibles: 0x25 esqueleto, 0x0E y 0x49 murciélagos, 0x05 serpiente, 0x0A caballero, 0x08 y 0x31 fuego, 0x32 flecha, 0x54 huesos (el proyectil del jefe del mundo 1, que se dibuja con tiles). El tipo de generador no siempre es un tipo de sprite: algunos (0x50, 0x51, 0x04, 0x3A) se dibujan con tiles.

## Pendiente
- La semántica de cada tipo de enemigo: IA, vida, daño y qué arma le afecta.
- El evento del castillo (E61C → b03:A003, secuencia de direcciones).
