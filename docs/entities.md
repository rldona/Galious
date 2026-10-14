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

Totales: 1124 generadores, 125 objetos de sala, 129 objetos recogibles y 245 piedras.

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
  - En el castillo solo se pone E61C = 1. Lo lee b03:A003 junto con el bit 2 de E068 y una secuencia de 4 direcciones (tabla A056); está sin descifrar.
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

**Generadores periódicos por sala** [C] (b04:61A8–62B9, que se ejecuta al cargar y en cada frame en b04:62F9–647B): no tocan los muros.
- Según el mundo y la sala activan E650, E653, E659, E65C, E65F, E662, E665 o E668.
- Cada uno es un temporizador que, al llegar a 0, crea un enemigo con la rutina de creación b04:65DA: los tipos 09, 08, 0D, 21, 30, 38, 35 y 3E, en una posición fija o algo aleatoria (R).
- Ejemplos: en las salas 58, 71, 113 y 114 del castillo (E650); en el mundo 6, salas 2, 3, 4, 7, 8, 17 y 18, el tipo 3E cada 16 ticks (E668).

## Objetos de sala: puertas, muros que se cierran y paredes que se rompen [C]

Al cargar la sala, b08:9D4C reparte cada objeto de la tabla A7BC según el nibble bajo de su tipo. Cada clase tiene su estado en RAM y su rutina por frame (b00:5376 las llama):

| Clase | Estado | Qué es | Dónde |
|---|---|---|---|
| 1 (tipo 0x01) | EA00, 2 huecos, columna C5 | **puerta con llave del castillo**: empujando contra ella unos 30 frames, a su altura exacta, gasta 1 llave (b09:A1F2, BCD en E04A) y suena 0x22 [C] | 15 salas del castillo |
| 2 (0x02) | EA30, 2 huecos, C6 C7 C8 | **puerta con llave de los mundos**: lo mismo, con sonido 0x21; no actúa si no tienes llaves [C] | 34 en los mundos |
| 3 (0x03, 0x13) | EA50, 1 hueco, CF; el bit 4 del tipo da el sentido | sin descifrar | 15 en los mundos |
| 4 (0x04) | EA60, 4 huecos, D0 D1 D2 | **puertas giratorias**: su rutina consulta E098, la flag del Bread & Water [C]. Según la guía [G], Popolon las cruza siempre y Aphrodite solo con ese objeto | 23 en los mundos |
| 5 (0x05) | EAA0 | **muro que se cierra a tu espalda** (b09:A415–A4EC) [C][O]. Al pasar por su columna anota tu dirección; cuando avanzas 16 px más, crece un tile cada 15 frames hasta 3 y marca la sala (bit 1 persistente), así que al volver ya está cerrado | 16 salas del castillo |
| 6 (0x06) | EAB0 | espera a que el jugador esté en y = 0x78. Por la guía, son las **piedras que aparecen si esperas en el borde** (mundo 5) [C][G] | mundo 5, sala 11 |
| 7 (0x07–0x37) | EA20 (se dibuja como bloque 4×3 de F0) | **pared que se rompe a espadazos**, ver abajo [C][O] | castillo 97 (camino de la cruz), mundo 2 sala 4, mundo 4 sala 16, mundo 5 sala 7, mundo 6 sala 6, mundo 9 sala 26 |

**Contraste con el mapa de mars2000you** (marcas N, "muro temporal que aparece después de pasar"): 14 de sus 16 marcas N caen en salas con objeto de clase 5. Sus marcas T, "escalera que puede desaparecer", caen en 12 de las 13 salas de la tabla de escaleras extra. Sus marcas FY, puntos donde aparece un hada, están en las salas 25, 91 y 110 del castillo (ver "Hadas").

## Paredes que suenan y se rompen [C][O]

Son los objetos de clase 7 (b08:A634).
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

## Santuarios y tiendas [C][G]

El tipo de sala de dios (E690, tabla del castillo en `castle.json` → `gods`) elige qué se puede hacer en ella. Las tiendas venden tres artículos: lista en b02:9253 + 2·tipo, pares (precio en BCD, objeto). b02:918C hace la compra:
- comprueba que tienes monedas suficientes (E048);
- las resta (b02:922B);
- activa la flag del objeto y suena 0x2D.

Casos especiales:
- el carcaj da **+100 flechas** (dos sumas de 50);
- la llave da **+1 llave**;
- los escudos van **en orden**: no se puede comprar uno sin tener el anterior (E097 + 0x2A ≥ objeto).

| Tipo | Sala | Dios (según la guía) | Se vende |
|---|---|---|---|
| 1 | castillo 72 | Demeter | contraseña (sin tienda) |
| 2 | castillo 115 | Hefesto | Mine 30, llave 5, carcaj 20 |
| 3 | castillo 87 | la Muerte | resurrección: comprueba 100 flechas y 100 monedas (b02:8FD8); para entrar hace falta la Salt |
| 4 | castillo 143 | Ares | carcaj 20, Copper Shield 30, Magnifying Glass 20 |
| 5 | castillo 65 | Ares | carcaj 20, Silver Shield 40, llave 5 |
| 6 | castillo 5 | Hefesto | Harp 30, carcaj 20, llave 5 |
| 7 | castillo 55 | Hefesto | carcaj 20, Conch Shell 25, llave 5 |
| 8 | castillo 54 | sin tienda: Asclepio o Atenea, consejo o curación [G] | — |
| 9 | castillo 4 | Ares | carcaj 20, Gold Shield 50, llave 5 |
| 10 | castillo 10 | Ares | carcaj 20, Hat Helmet 50, llave 5 |
| 11 | castillo 32 | Demeter | carcaj 20, Necklace 25, llave 5 |
| 12 | mundo 5 | Demeter | Oar 30 |
| 13 | mundo 8 | Demeter | Carpet 30 |

Los precios coinciden con los de la guía de GameFAQs.

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

## Catálogo de sprites## Catálogo de sprites

`extracted/entities/sprites_<tipo>.png` pinta los sprites de 16×16 que carga cada tipo, en blanco. Algunos reconocibles: 0x25 esqueleto, 0x0E y 0x49 murciélagos, 0x05 serpiente, 0x0A caballero, 0x08 y 0x31 fuego, 0x32 flecha, 0x54 huesos (el proyectil del jefe del mundo 1, que se dibuja con tiles). El tipo de generador no siempre es un tipo de sprite: algunos (0x50, 0x51, 0x04, 0x3A) se dibujan con tiles.

## Pendiente
- La semántica de cada tipo de enemigo: IA, vida, daño y qué arma le afecta.
- Los gráficos especiales de la sala del jefe y la animación del agua y la lava.
- El evento del castillo (E61C → b03:A003, secuencia de direcciones).
