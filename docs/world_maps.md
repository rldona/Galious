# Mapas del castillo y de los mundos (fase 3)

Leyenda como en `state_machine.md`: **[C]** código · **[O]** observado en openMSX · **[O*]** observado con ayuda forzada.

## Qué hay y cómo regenerarlo

```
python3 tools/extract/world_maps.py         # desde la ROM: extracted/maps/<mundo>.json y .svg
python3 tools/extract/stitch_maps.py 0.5    # mapa visual con capturas (necesita Pillow)
```

- `extracted/maps/<mundo>.json`: salas con su posición en la cuadrícula y sus vecinos, salidas al castillo, sala y palabra del jefe, y, en el castillo, puertas y salas de dioses.
- `extracted/maps/<mundo>.svg`: esquema de conexiones. Verde = entrada (sala 1), rojo = jefe, ocre = puerta de mundo (P n), morado = sala de dios (D tipo), amarillo = salida al castillo.
- `extracted/maps/<mundo>_visual.png`: cada sala con su captura real, colocada en la cuadrícula. **Contiene gráficos de Konami: solo para uso local.** Las capturas salen de `tools/openmsx/scan_rooms.py`, que carga cada sala con el cargador del propio juego (estado 10). Para entrar en cada mundo hacen falta los savestates de `tools/openmsx/world_states.py`.

## Resumen

| Mundo | E041 | Salas | Jefe (sala) | Palabra | Salida al castillo | Sala especial (estado 14) |
|---|---|---|---|---|---|---|
| Castillo | 1 | 147 (la 94 no se alcanza por enlaces) | — | — | — | — |
| 1 | 2 | 15 | 13 | YOMAR | sala 1 arriba (FE) | 15 |
| 2 | 3 | 15 | 9 | ELOHIM | sala 1 abajo (FD) | 15 |
| 3 | 4 | 16 | 13 | HAHAKLA | sala 1 abajo (FC) | 16 |
| 4 | 5 | 17 | 14 | BARECHET | sala 1 izquierda (FB) | 17 |
| 5 | 6 | 18 | 17 | HEOTYMEO | sala 1 derecha (FA) | 18 |
| 6 | 7 | 19 | 13 | LEPHA | sala 1 arriba (F9) | 19 |
| 7 | 8 | 20 | 10 | NAWABRA | sala 1 arriba (F8) | 20 |
| 8 | 9 | 21 | 7 | ASCHER | sala 1 arriba (F7) | 21 |
| 9 | 10 | 27 | 25 | XYWOLEH | sala 1 izquierda (F6) | 27 |
| 10 | 11 | 5 | 5 | HAMALECH | sala 1 izquierda (F5, ver abajo) | — |

En total son 320 salas. Todos los mundos se recorren enteros desde la sala 1 y la cuadrícula no tiene contradicciones, salvo la sala 4 del mundo 3, cuyo vecino de arriba es ella misma: un bucle del laberinto [C].

## Formatos en la ROM [C]

**Vecinos de cada sala.** b00:5218 mapea los bancos 7–9 y lee el puntero del mundo en `A9EF + 2·E041` (banco 9). Los 4 bytes de la sala están en `puntero + 4·sala` y se copian a E648–E64B: arriba, abajo, izquierda y derecha.
- Valores: `FF` = pared; `F6–FE` = salida al castillo, con `FF − n` para el mundo n, que dice a qué puerta se vuelve.
- El 0xF5 del mundo 10 queda por debajo del umbral `CP F6` de b03:A897, así que esa salida se gestiona en otro sitio (pendiente).
- Las salas van de 1 a N: la entrada 0 no se usa y la N cae donde empieza la tabla del mundo siguiente. El último mundo termina en una entrada `FF FF FF FF`. Comprobado con las 139 salas leídas en el emulador [O].

**Disposición de cada sala.** b00:5561 mapea los bancos 7–9 y b00:55F7 lee el puntero del mundo en `6ABE + 2·E041` (banco 7).
- Cada sala son 40 bytes en `puntero + 40·(sala − 1)`: 8×5 bloques de 32×32 px.
- Cada bloque n se define con 16 bytes en `6000 + 16·n` (banco 7): 4×4 tiles de 8 px.
- El resultado es un mapa de 32×20 tiles en el búfer ED80.
- Excepción: la sala 6 del mundo E041 = 6 usa un mapa fijo en 8DAE.
- La tabla de disposiciones de cada mundo tiene una sala menos que la de vecinos: la última sala (la "sala especial") no se dibuja con ella.

**Sala especial de cada mundo.** Es la última sala. Al cargarla el juego pasa al estado 14 en vez del 5 [O]: es una sala de dios dentro del mundo (Demeter en los mundos 1, 4 y 9), con mensajes como guardar la partida, pistas sobre el agua o una puerta que lleva al castillo. b02:8EFC la detecta con las tablas 8F0A (sala) y 8F14 (tipo en E690) [C].

**Puertas de mundo (castillo).**
- Salas en 9990 y posición (Y, X) en 9997, ambas en el banco 2. La puerta 10 es dinámica: sala en E06D, posición según E06E (tabla 99AB) y solo aparece si E06C & 3 [C].
- Dos puertas tienen condición extra: la 5 necesita E092 y la 7 necesita E07C, solo cuando E501 = 1 (b02:9A25) [C][O*].

**Salas de dioses (castillo).** Tabla en 9DDA (banco 2), con entradas de 4 bytes (sala, tipo, Y, X) terminada en 0 [C].

## Gráficos de las salas [C][O]

`python3 tools/extract/render_rooms.py` lo genera todo desde la ROM en unos segundos, sin dependencias, en `extracted/rooms/`:
- `<mundo>/<sala>.png`: cada sala.
- `<mundo>_map.png`: el mundo entero.
- `tiles_*.png`: los juegos de tiles.
- `rooms.json`: la disposición de cada sala en bloques.
- `blocks.json`: los 256 bloques de 4×4 tiles.

Con `--compare` (necesita Pillow) compara cada sala con su captura del emulador.

**Pantalla.** SCREEN 2 (R0 = 2, R1 = 0x62): colores en 0x0000, patrones en 0x2000, nombres en 0x3800, patrones de sprites en 0x1800, atributos de sprites en 0x3B00. Las salas cargan los mismos 256 tiles en los tres tercios; el marcador ocupa las filas 0–3 y la sala las 4–23.

**Compresión** (b00:5C40, o b00:5C46 si el destino ya está en HL): RLE hacia la VRAM. `00` fin · `80 lo hi` cambia de dirección · `81–FF` copia `n & 7F` bytes tal cual · `01–7F b` repite b n veces (`tools/extract/vram_rle.py`).

**Juegos de tiles** (b00:57A4 y b00:59F0; listas en el banco 10, con los bancos 10–12 mapeados):
- **Lista de piezas** (b00:5AB3): entradas de 6 bytes (máscara de tercios, puntero RLE de patrones, tile de destino, puntero RLE de colores), terminadas en 0.
- **Lista de espejos** (b00:5A8D): entradas de 4 bytes (tile origen, tile destino, flags, n). Copia n tiles invirtiendo los bits de cada byte, es decir, en espejo horizontal (b00:5B08).
- **Siempre:** piezas de 0x6000 y espejos de 0x6037.
- **Mundos:** las piezas de 0x6044. Todos los mundos comparten el mismo juego de tiles; cambian los bloques que usa cada sala.
- **Castillo:** la zona de cada sala es medio byte de 0x60D7 (el alto si la sala es par, el bajo si es impar) y elige entre 7 combinaciones (b00:5A2C–5A80).

| Zona | Listas |
|---|---|
| 0 | piezas 6069 |
| 1 | piezas 6076 |
| 2 | piezas 6083 + espejos 60A8 |
| 3 | piezas 6083 + 60C9 + espejos 60A8 |
| 4 | piezas 60B1 + 60A1 + espejos 60AC |
| 5 | piezas 60B1 + espejos 60AC |
| 6 | piezas 60D0 |

- **Marcador:** la fuente sale de los bancos 13–15 (9AD3 y 9B24, b00:5E1A), en los tres tercios y con color 0xF0. Los iconos de objetos salen de las tablas 6E34 (patrones) y 6E80 (colores), en b00:58E1.

**Comprobación.** Cada sala renderizada desde la ROM, comparada con la captura del emulador (zona de juego, píxel a píxel): el parecido medio está entre el 97,6 % y el 98,8 % en todos los mundos y en el castillo. Lo que no coincide son sprites, objetos superpuestos y la animación del agua (tiles 0xB6 y 0xB8, que cambian de fase). Agrupando los fallos por tile, ningún otro tile falla de forma sistemática.

**Paleta.** openMSX arranca con una máquina MSX2, cuya BIOS carga la paleta por defecto (3 bits por canal), así que las capturas usan esa paleta y `render_rooms.py` también.

## Colisión de los tiles [C][O]

`python3 tools/extract/collision.py` → `extracted/maps/collision/<mapa>.json`: 20 filas × 32 columnas por sala, incluidas las superposiciones (piedras, muros, estatuas, puertas, escaleras extra). La sala del jefe trae además la versión de combate.

El juego decide por el **número de tile** del mapa de nombres en RAM (ED00 + fila·32 + columna):

| Clase | Tiles | Código | Leyenda |
|---|---|---|---|
| sólido | ≥ **0xBA**; **≥ 0xF0** durante el combate con un jefe (E0D2 ≠ 0) | b00:554D pone el umbral en F020; b00:54FF–554C (jugador); b00:4DC8 (enemigos, > 0xB9) | `#` (y `j`: BA–EF, que en combate se atraviesan) |
| escalera | **9A, 9B** | b03:AEE1: tile − 9A < 2 en los dos lados del jugador; lo alinea a múltiplo de 8 y consulta las escaleras extra (EC00) | `H` |
| agua | **B6, B7** (E511 = 1) y **B8, B9** (E511 = 2, más profunda [I]) | b03:B028; en el mundo 5, salas 12 y 13, con el Oar la B6 no hunde | `~` / `w` |
| fondo | el resto | — | `.` |

- **Correspondencia de coordenadas:** la y del jugador (E505) es la de los pies: está de pie sobre la fila r cuando y = 8·r + 32. Su caja de pies abarca de x − 8 a x + 7.
- **Validación** [O]: 91 caídas en 12 salas (castillo y mundos 1, 3, 5, 6 y 8). En todas, el jugador acaba apoyado en un tile que la cuadrícula marca como sólido.
- **Sin tile propio:**
  - la lava del mundo 8 no tiene clase propia: el daño debe de venir de los enemigos y generadores de esas salas [I];
  - los pinchos son enemigos (0x31).
- **Recuento en las 320 salas:** fondo 109 735 tiles, sólido 76 671, escalera 6 796, agua 2 406, agua profunda 3 432.

## Pendiente para completar la fase 3
- Dibujar lo que el juego superpone a la sala: puertas, carteles de dioses y el emblema del jefe.
- La animación de los tiles de agua y lava.
- La última sala de cada mundo (sala de dios) y la sala 94 del castillo, que no se alcanza por enlaces.
- Los enemigos de cada sala, los objetos que se recogen y los muros que se rompen.
- La salida 0xF5 del mundo 10 y la condición de la puerta 10 (E06C/E06D/E06E).
