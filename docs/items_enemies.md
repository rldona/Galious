# Objetos recogibles y enemigos (fase 3)

Leyenda como en `state_machine.md`: **[C]** código · **[O]** observado en openMSX · **[I]** interpretación.

```
python3 tools/extract/entities.py   # posiciones (entities.json) y sprites por tipo
python3 tools/extract/catalog.py    # extracted/entities/enemies.json e items.json
```

Iconos de los objetos: `extracted/entities/item_icons.png` (tipos 0x07–0x2C en filas de 10, empezando por 0x07). Sprites de los enemigos: `sprites_<tipo>.png`.

## Objetos recogibles

**Icono** [C]: b00:58E1 descomprime 32 bytes de patrones (6E34[tipo]) y de colores (6E80[tipo]), del banco 10, en los tiles 0x76–0x79 (0x23B0). Por eso el "cofre" que se ve en la sala es el icono real del objeto. Los tipos ≥ 0x2D usan sus propios tiles (0x60 + 4·(t − 0x2D)); los < 7 no tienen icono de tiles.

**Armas secundarias (tipos 1–4)** [O*]: no tienen icono. En la sala se ven como un destello de un tile (0x4E, una fila por debajo de su posición), y tres de las cuatro están debajo de una piedra. Están en el castillo: 0x01 en la sala 121, 0x02 en la 71, 0x03 en la 79 (a la vista) y 0x04 en la 147. Al recogerlas se activa E06F + tipo (E070–E073). Forzando las cuatro flags, el menú de objetos (F1) muestra **flecha, flecha doble, fuego rodante y fuego**, y la elegida pasa al recuadro del marcador (E510 = arma elegida). Al empezar partida, E070 y E510 valen 1 (b01:7DA4) [C].

**Al recogerlo** (b03:B701) [C]:
- E06F + tipo = 1 (lo tienes) y se activa el bit persistente "recogido en esta sala". Suena 0x2D, salvo la llave.
- 0x07, 0x08, 0x09 y 0x0A son **objetos de mundo**: activan los bits 5, 6, 7 y 4 de E061 + mundo, y al entrar en cada mundo se copian a sus flags (b00:462E–4642). Hay uno de cada en cada mundo.
- 0x0C, **llave**: desbloquea la puerta del mundo siguiente (bit 1 de E062 + mundo) y dentro de un mundo dispara la fanfarria (estado 20) [C][O].
- 0x2B y 0x2C guardan 2 o 3 en E097, el **nivel de escudo**. E097 es a la vez la flag del objeto 0x28, que es el nivel 1 [C].
- 0x2D, 0x2E y 0x2F son **recursos**: suman +50, +10 y +2 a los contadores E046, E048 y E04A (BCD, tope 999), que son las tres filas del marcador (b00:4D5A, 4D7B, 4D82). Por los iconos serían flechas, monedas y llaves. Que E046 son flechas está comprobado: cada disparo de flecha resta 1 [O*].

**Qué hace cada objeto.** El icono está descrito a ojo desde `item_icons.png`. El efecto sale de buscar en todo el código quién lee su flag (E06F + tipo) y, donde se indica, de probarlo en openMSX forzando la flag.
- **[O*]** probado en el emulador.
- **[C]** leído en el código, sin probar.
- **?** sin descifrar.

| Tipo | Icono | Efecto | Código |
|---|---|---|---|
| 0x01–0x04 | (sin icono) | armas secundarias: flecha, flecha doble, fuego rodante, fuego (M) [O*] | E510 |
| 0x05, 0x06 | (sin icono) | armas secundarias 5 y 6: casos propios al disparar; la 6 activa E517. No aparecen en ninguna sala [C] | b03:A72B, AD46 |
| 0x07–0x0A | frasco, capa, vara, pergamino | objetos de mundo: bits 5, 6, 7 y 4 de E061 + mundo [C] | b00:462E |
| 0x0B | cruz | interviene en el combate con el jefe (estado del jefe E70A ≥ 0x12/0x19): con la cruz se ejecuta b03:B96D en su posición [C] ? | b03:BBA8 |
| 0x0C | llave | abre la puerta del mundo siguiente; fanfarria [C][O] | b03:B701 |
| 0x0D | collar azul | Popolon lo necesita para la puerta 7 [C] | b02:9A41 |
| 0x0E, 0x0F, 0x17, 0x22, 0x24 | corona, guante, figura verde, triángulo, frasco verde | inmunidad al contacto con los enemigos 0x2A, 0x28, 0x2E, 0x31 y 0x08 [C] | b03:B1F7 |
| 0x10 | pluma roja | en el mundo 5, salas 12 y 13, el agua (tile B6) no te hunde: activa E52A en vez de "en el agua" [C] | b03:B037 |
| 0x11 | bota | **sube y baja escaleras un 25 % más rápido** (0,62 px/frame frente a 0,5); el salto y el paso no cambian [O*] | b03:A5FE, A6B6 |
| 0x12 | figura verde | hace aparecer el **puente** del mundo 2, sala 11 [O*] | b08:9F87 |
| 0x13 | capa gris | sin ella, **en el mundo 8 izquierda y derecha van al revés** (b03:A8BE intercambia los bits) [O*]. Te lo da un dios (b02:9FA7) | b03:A8A6 |
| 0x14 | campana | en el castillo, **suena (0x28) al entrar en ciertas salas**, según tus flags de progreso (tablas b00:BE4F/BE63) [C]. Pista | b00:5D24 |
| 0x15 | aro dorado | en el menú de objetos, **Return te devuelve al castillo** (sala 72), también desde un mundo [O*] | b02:9AC9 |
| 0x16 | vela | sin ella, la sala 6 del mundo 5 está **a oscuras** (solo se ve el fondo) [O*]. Te la da un dios | b00:55F7 |
| 0x18 | pañuelo rojo | **apaga los generadores periódicos** de enemigos de varias salas de los mundos 7–10 (b04:6200); en el mundo 8 también cambia tiles (b00:5575) [C] | b04:6200 |
| 0x19 | casco | **recibes la mitad de daño** [C] | b03:B3BF |
| 0x1A | lámpara | en los mundos coloca un sprite indicador desde una tabla por mundo; quizá en el recuadro del marcador [C][I] | b01:668F |
| 0x1B | jarrón | **duplica la experiencia** que ganas [C]. Te lo da un dios | b03:B62E |
| 0x1C | collar amarillo | con él, la espada interactúa con una lista de objetos de la sala (EA20) y suena 0x1E [C] ? | b09:A668 |
| 0x1D, 0x1E | pendientes, piedra roja | nadie lee su flag directamente ? | — |
| 0x1F | anillo | **Aphrodite rompe piedras igual que Popolon**: 5 por golpe en vez de 1 (de 15) [O*] | b03:B841 |
| 0x20 | libro | **CTRL congela a los enemigos ~5 s** (E0D5/E0D6), hasta 16 veces (E531) [O*] | b02:9BB6 |
| 0x21 | arpa | **un golpe de espada rompe todas las piedras de la sala** [O*] | b01:6398 |
| 0x23 | cuerno | Popolon lo necesita para la puerta 5 [C] | b02:9A33 |
| 0x25 | daga | la espada quita 1 de vida extra por golpe [C] | b03:B580 |
| 0x26 | espada | te la dan al limpiar ciertas salas del castillo e introducir una secuencia de 4 direcciones (b03:A003, tabla A056). Con ella, escribir **UMBRELLA** activa algo (b02:9C0B → b03:B5EA) [C] ? | b02:9BEE |
| 0x27 | pluma blanca | en el menú de objetos, **pulsar el número de un mundo abierto te lleva a la sala de su puerta** [O*] | b02:9A99 |
| 0x28 | objeto rojo | primer nivel de escudo: E097 es su flag. 0x2B y 0x2C la suben a 2 y 3 [C] | b03:B74B |
| 0x29 | copa | con ella, los proyectiles que llegan a la altura del jugador por delante se bloquean (b09:A345) [C] ? | b09:A345 |
| 0x2A | montón gris | necesario para entrar en la sala de dios de tipo 3 (castillo, sala 87) [C] | b02:9A84 |
| 0x2B, 0x2C | escudo gris, escudo dorado | escudo de nivel 2 y 3 (E097) [C] | b03:B74B |
| 0x2D–0x2F | (tiles propios) | +50 flechas, +10 monedas, +2 llaves (E046/E048/E04A) [C][O] | b03:B701 |

Las inmunidades salen de b03:B1F7–B22D, la colisión entre jugador y enemigo: si tienes el objeto, se sale sin hacer daño.

**Dónde están**: 0x2D, 0x2E y 0x2F aparecen en 13, 25 y 14 salas del castillo, casi siempre bajo piedras. 0x07–0x0A, una vez por mundo. 0x0C, en el castillo y en cada mundo (el jefe la suelta, véase `state_machine.md`). El resto son únicos y están en el castillo. La lista completa está en `items.json`.

## Enemigos

**Creación** (b04:6000 → 65DA) [C]: si no se ha llegado al límite (E620 < E635), ocupa un hueco de 0x20 bytes en **E800 + 0x20·n** y copia el descriptor del tipo, de 5 bytes, desde **b04:67B6 + 5·tipo**:

| Campo | Destino | Significado |
|---|---|---|
| d0 | IX+9 | flags (bit 7: tiene sprite) [I] |
| d1 | IX+0A | atributos o patrón de sprite [I] |
| d2 | IX+0B | ¿tamaño o caja de colisión? [I] |
| d3 | IX+10 | **vida**; FE/FF = no se puede dañar [C][O] |
| d4 | IX+11 | clase de recompensa: 647B[d4] da la categoría (4 = nada) y 64E5[d4] el nivel de botín [C]; en algunos tipos cambia durante la partida [O] |

Estructura en RAM [O]: +0 tipo, +5 Y, +7 X, +9..+0B descriptor, +10 vida, +11 recompensa. Comprobado con esqueletos (vida 8), murciélagos (1), 0x18 (1) y estatuas (254).

Después, una tabla de saltos por tipo (b04:66B4) lleva a la rutina de inicialización de cada enemigo, y de ahí sale su comportamiento.

**Daño y muerte** [C]:
- Cada golpe resta 1 a IX+10 (o 2, según el ataque), +1 con el objeto 0x25 (b03:B57A–B5E0).
- En 0, b03:B604 lo mata. b04:6A3B y 6A77 deciden el botín: un objeto 0x5D, 0x5E o 0x5F según un número aleatorio (`LD A,R`) y el nivel, en cantidades de 10/20/30/40, 1/4/7/10 o 1/1/2/2 (tabla 6A6B). Al tocar ese objeto se suma al contador correspondiente (b03:B2CB).
- Con los jefes manda además la tabla de vulnerabilidades por arma (ver `state_machine.md`, «Jefes»).

**Objetos especiales que se tratan como enemigos** [C]: 0x3F restaura algo (E50F = 0x3C, ¿el hada?). Tocar 0x6B dibuja tiles 7A… y activa E024 → estado 17.

**Resumen** (`enemies.json`):
- 53 tipos con generador.
- La mayoría aparece en un solo mundo: cada mundo tiene su propio bestiario. El murciélago 0x0E está en 8 mundos y suma 294 generadores.
- Los más duros: 0x23 (vida 64), 0x15 (40), 0x40 (35), 0x16 y 0x22 (24), 0x4C (20).
- No se pueden dañar: 0x20, 0x29, 0x31, 0x32, 0x36, 0x3A, 0x50, 0x51 y 0x69.

## Comportamiento observado [O]

```
python3 tools/openmsx/trace_enemies.py [--near] [tipos]   # graba 360 frames (6 s) por tipo
python3 tools/openmsx/analyze_enemies.py                  # summary.json + trayectorias <tipo>_paths.png
python3 tools/extract/catalog.py                          # lo incorpora a enemies.json
```

**Método.** Para cada tipo se carga la primera sala donde aparece, con el jugador invencible y quieto en la entrada. Se graban dentro de openMSX, frame a frame (`after frame`), los 16 huecos de E800. Con `--near` el jugador se coloca a 24 px del enemigo, para ver si reacciona a su cercanía. Las trayectorias se pintan sobre la sala renderizada desde la ROM (`logs/omsx/enemies/<tipo>_paths.png`).

**Columnas.**
- Velocidades: px por frame, de media sobre todos los frames.
- *Hacia el jugador*: fracción de frames en movimiento en que va hacia el jugador; ~0,5 equivale a azar.
- *Hijos*: tipos que nacen pegados a él (proyectiles, objetos que suelta).
- Las grabaciones son de 6 s y de una sola sala, así que son una primera aproximación.

| Tipo | Vida | Patrón | Vel. X | Vel. Y | Hacia el jugador | Hijos | Generadores | Mundos | Sala grabada |
|---|---|---|---|---|---|---|---|---|---|
| 0x01 | 4 | camina | 0.75 | 0.0 | 0.55 |  | 7 | 1 | world09:11 |
| 0x02 | 1 | vuela | 0.37 | 0.13 | 0.58 |  | 5 | 1 | castle:7 |
| 0x03 | 1 | camina | 0.19 | 0.0 | 0.27 |  | 10 | 1 | world03:3 |
| 0x04 | 1 | quieto | 0.0 | 0.13 | 0.42 |  | 7 | 1 | castle:117 |
| 0x05 | 3 | camina | 0.47 | 0.0 | 0.51 | 0x1a ×3 | 11 | 1 | world09:14 |
| 0x07 | 2 | camina (cae entre plataformas) | 0.3 | 0.07 | 0.22 |  | 21 | 1 | world05:2 |
| 0x0a | 6 | camina (cae entre plataformas) | 0.22 | 0.03 | 0.41 |  | 54 | 1 | castle:20 |
| 0x0b | 1 | camina | 1.74 | 0.0 | 0.53 |  | 5 | 1 | world05:12 |
| 0x0c | 3 | camina | 0.37 | 0.0 | 0.44 | 0x56 ×2 | 20 | 1 | world08:6 |
| 0x0e | 1 | vuela | 0.21 | 0.37 | 0.51 |  | 294 | 8 | castle:13 |
| 0x0f | 6 | camina | 0.5 | 0.0 | 0.39 |  | 24 | 1 | world06:9 |
| 0x10 | 1 | quieto → cerca del jugador: camina | 0.0 | 0.0 | 0.0 |  | 23 | 1 | world01:4 |
| 0x11 | 2 | camina | 0.67 | 0.0 | 0.48 | 0x1d ×2 | 25 | 1 | castle:5 |
| 0x12 | 5 | quieto | 0.0 | 0.0 | 0.0 |  | 9 | 1 | world06:5 |
| 0x13 | 4 | camina | 0.4 | 0.0 | 0.47 | 0x44 ×4 | 12 | 1 | world08:3 |
| 0x14 | 4 | quieto | 0.03 | 0.0 | 0.17 |  | 15 | 1 | world04:3 |
| 0x15 | 40 | vuela | 0.52 | 0.5 | 0.61 | 0x45 ×2 | 1 | 1 | world09:26 |
| 0x16 | 24 | vuela | 0.32 | 0.24 | 0.96 | 0x46 ×2 | 1 | 1 | world06:11 |
| 0x17 | 5 | camina | 0.5 | 0.0 | 0.53 | 0x47 ×1 | 10 | 1 | world04:2 |
| 0x18 | 1 | camina | 0.22 | 0.0 | 0.62 |  | 41 | 1 | castle:3 |
| 0x19 | 1 | vuela | 0.35 | 0.36 | 0.55 |  | 40 | 1 | castle:18 |
| 0x1f | 3 | camina | 1.81 | 0.0 | 0.48 |  | 12 | 1 | world07:7 |
| 0x20 | — | camina | 0.13 | 0.0 | 0.1 |  | 12 | 1 | world02:2 |
| 0x22 | 24 | vuela | 0.37 | 0.32 | 0.58 | 0x4a ×14 | 2 | 1 | world07:16 |
| 0x23 | 64 | quieto | 0.0 | 0.0 | 0.0 |  | 7 | 2 | castle:2 |
| 0x24 | 3 | camina | 1.0 | 0.0 | 0.45 |  | 13 | 1 | world03:6 |
| 0x25 | 8 | camina (cae entre plataformas) | 0.48 | 0.03 | 0.54 | 0x3d ×1 | 62 | 1 | castle:6 |
| 0x26 | 10 | vuela | 0.22 | 0.21 | 0.5 | 0x4b ×3 | 3 | 1 | castle:4 |
| 0x27 | 12 | vuela | 0.35 | 0.33 | 0.35 |  | 4 | 1 | world07:4 |
| 0x28 | 1 | vuela | 1.02 | 0.89 | 0.6 |  | 27 | 2 | world01:6 |
| 0x29 | — | vuela | 0.21 | 0.43 | 0.4 |  | 14 | 1 | world04:5 |
| 0x2a | 3 | quieto | 0.0 | 0.0 | 0.0 |  | 5 | 1 | world02:4 |
| 0x2b | 3 | vuela | 0.35 | 0.16 | 0.27 |  | 9 | 1 | world08:4 |
| 0x2c | 4 | camina | 0.21 | 0.0 | 0.07 |  | 7 | 1 | world02:10 |
| 0x2d | 2 | camina | 0.51 | 0.0 | 0.62 | 0x3f ×2 | 12 | 1 | world02:5 |
| 0x2e | 2 | quieto | 0.0 | 0.0 | 0.0 | 0x1d ×1 | 58 | 1 | castle:5 |
| 0x2f | 5 | quieto | 0.0 | 0.0 | 0.0 |  | 26 | 2 | world01:2 |
| 0x31 | — | quieto | 0.0 | 0.0 | 0.0 |  | 45 | 1 | castle:49 |
| 0x32 | — | quieto | 0.0 | 0.0 | 0.0 |  | 8 | 1 | castle:9 |
| 0x34 | 4 | vertical | 0.0 | 1.88 | 0.5 |  | 38 | 1 | castle:17 |
| 0x36 | — | vuela | 0.23 | 0.81 | 0.49 |  | 16 | 1 | world05:5 |
| 0x3a | — | quieto | 0.0 | 0.0 | 0.0 |  | 3 | 1 | castle:3 |
| 0x3c | 7 | quieto | 0.0 | 0.0 | 0.0 | 0x3d ×5 | 2 | 1 | castle:6 |
| 0x40 | 35 | camina (cae entre plataformas) | 0.21 | 0.03 | 1.0 | 0x41 ×1 | 1 | 1 | world08:20 |
| 0x42 | 10 | vertical | 0.0 | 0.1 | 0.67 | 0x61 ×3 | 3 | 1 | castle:141 |
| 0x43 | 10 | camina | 1.0 | 0.0 | 0.5 |  | 4 | 1 | world09:6 |
| 0x49 | 5 | vuela | 0.71 | 0.54 | 0.47 |  | 1 | 1 | world03:14 |
| 0x4c | 20 | vuela | 0.34 | 0.34 | 0.43 | 0x4d ×11 | 2 | 1 | world05:6 |
| 0x4e | 0 | vuela | 0.33 | 0.33 | 0.46 |  | 36 | 1 | castle:25 |
| 0x50 | — | quieto | 0.0 | 0.0 | 0.0 |  | 4 | 1 | castle:3 |
| 0x51 | — | quieto | 0.0 | 0.0 | 0.0 |  | 5 | 1 | castle:9 |
| 0x64 | 4 | camina | 0.16 | 0.0 | 0.11 |  | 7 | 1 | world02:10 |
| 0x69 | — | vuela | 0.06 | 0.16 | 0.44 | 0x06 ×4 | 21 | 1 | world09:7 |

**Lo más útil para el diseño** (con trayectorias y sprites):
- **Caminantes que dan la vuelta en los bordes:** esqueleto 0x25, que además salta entre plataformas con una parábola, caballero 0x0A, 0x01, 0x0F, 0x11 y 0x43. Los más rápidos son 0x0B y 0x1F (~1,8 px/frame).
- **Voladores:** murciélago 0x0E (zigzag de tramos rectos), 0x28 (rápido, ~1 px/frame en X e Y; el objeto 0x0F protege de él), 0x49, 0x4E, 0x19 y 0x27.
- **Persiguen:** 0x16 (se acerca el 96 % del tiempo, a saltos, y dispara 0x46) y 0x40 (100 %, vida 35, suelta 0x41).
- **Tiradores:** 0x3C (estatua: flechas 0x3D en línea recta, 5 en 6 s), 0x22 (14 proyectiles 0x4A), 0x4C (11 de 0x4D), serpiente 0x05 (0x1A), 0x0C (0x56), 0x13 (0x44), 0x15 (0x45).
- **Emboscada:** 0x10 se queda quieto hasta que el jugador se acerca y entonces camina. 0x14 sale del suelo: sus sprites van de montículo a criatura y su estado cambia sin desplazarse.
- **Peligros fijos o animados:** llamas 0x31 (invulnerables; el objeto 0x22 protege de ellas), cabeza 0x2A (el objeto 0x0E protege de ella), 0x2E (suelta 0x1D; el objeto 0x17 protege de él), estatuas 0x32, 0x3A, 0x50 y 0x51.
- **Barrera:** 0x23, columna con vida 64 que no se mueve.
- **Vertical:** 0x34 sube y baja (~1,9 px/frame).
- **Sin actividad en 6 s:** 0x2F, 0x12 y 0x2A (este se mueve un poco). Pueden depender de algo que la grabación no provoca.

## Pendiente
- Confirmar cada comportamiento en el código (rutinas de inicialización en b04:66B4 y de actualización por tipo) y medir con grabaciones más largas y con el jugador moviéndose.
- Nombres de objetos más allá de la descripción visual. Cómo se disparan las armas secundarias y qué daño hacen.
- Objetos con **?** en la tabla: 0x0B, 0x1C, 0x1D, 0x1E, 0x26 (UMBRELLA), 0x29, y las armas 5 y 6.
