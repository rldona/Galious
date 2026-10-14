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

**Otro código parecido sin descifrar**: b04:61A8–62B9 compara también (mundo, sala) y activa E650, E653, E659, E65C, E65F, E662 y E668–E66A. No toca los muros; puede ser agua, corrientes o zonas de daño.

## Catálogo de sprites

`extracted/entities/sprites_<tipo>.png` pinta los sprites de 16×16 que carga cada tipo, en blanco. Algunos reconocibles: 0x25 esqueleto, 0x0E y 0x49 murciélagos, 0x05 serpiente, 0x0A caballero, 0x08 y 0x31 fuego, 0x32 flecha, 0x54 huesos (el proyectil del jefe del mundo 1, que se dibuja con tiles). El tipo de generador no siempre es un tipo de sprite: algunos (0x50, 0x51, 0x04, 0x3A) se dibujan con tiles.

## Pendiente
- La semántica de cada tipo de enemigo: IA, vida, daño y qué arma le afecta.
- Los gráficos especiales de la sala del jefe y la animación del agua y la lava.
- El evento del castillo (E61C → b03:A003, secuencia de direcciones) y las flags de b04:61A8.
