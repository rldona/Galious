# Objetos recogibles y enemigos (fase 3)

Leyenda como en `state_machine.md`: **[C]** código · **[O]** observado en openMSX · **[I]** interpretación.

```
python3 tools/extract/entities.py   # posiciones (entities.json) y sprites por tipo
python3 tools/extract/catalog.py    # extracted/entities/enemies.json e items.json
```

Iconos de los objetos: `extracted/entities/item_icons.png` (tipos 0x07–0x2C en filas de 10, empezando por 0x07). Sprites de los enemigos: `sprites_<tipo>.png`.

## Objetos recogibles

**Icono** [C]: b00:58E1 descomprime 32 bytes de patrones (6E34[tipo]) y de colores (6E80[tipo]), del banco 10, en los tiles 0x76–0x79 (0x23B0). Por eso el "cofre" que se ve en la sala es el icono real del objeto. Los tipos ≥ 0x2D usan sus propios tiles (0x60 + 4·(t − 0x2D)); los < 7 no tienen icono de tiles.

**Armas secundarias (tipos 1–4)** [O*]: no tienen icono. En la sala se ven como un destello de un tile (0x4E, una fila por debajo de su posición), y tres de las cuatro están debajo de una piedra. Están en el castillo: 0x01 en la sala 121, 0x02 en la 71, 0x03 en la 79 (a la vista) y 0x04 en la 147. Al recogerlas se activa E06F + tipo (E070–E073). Forzando las cuatro flags, el menú de objetos (F1) muestra **flecha, flecha de cerámica, fuego rodante y fuego**, y la elegida pasa al recuadro del marcador (E510 = arma elegida). Al empezar partida, E070 y E510 valen 1 (b01:7DA4) [C].

**Al recogerlo** (b03:B701) [C]:
- E06F + tipo = 1 (lo tienes) y se activa el bit persistente "recogido en esta sala". Suena 0x2D, salvo la llave.
- 0x07, 0x08, 0x09 y 0x0A son **objetos de mundo**: activan los bits 5, 6, 7 y 4 de E061 + mundo, y al entrar en cada mundo se copian a sus flags (b00:462E–4642). Hay uno de cada en cada mundo.
- 0x0C, **llave**: desbloquea la puerta del mundo siguiente (bit 1 de E062 + mundo) y dentro de un mundo dispara la fanfarria (estado 20) [C][O].
- 0x2B y 0x2C guardan 2 o 3 en E097, el **nivel de escudo**. E097 es a la vez la flag del objeto 0x28, que es el nivel 1 [C].
- 0x2D, 0x2E y 0x2F son **recursos**: suman +50, +10 y +2 a los contadores E046, E048 y E04A (BCD, tope 999), que son las tres filas del marcador (b00:4D5A, 4D7B, 4D82). Por los iconos serían flechas, monedas y llaves. Que E046 son flechas está comprobado: cada disparo de flecha resta 1 [O*].

**Qué hace cada objeto.** Los nombres son los de la guía de GameFAQs de Vahan Nisanian (versión 1.3, 2023), que sigue los manuales. Cada efecto lleva una de estas marcas:
- **[O*]** probado en openMSX forzando la flag;
- **[C]** leído en el código;
- **[G]** solo lo dice la guía, sin contrastar todavía.

El efecto en código sale de buscar quién lee la flag del objeto (E06F + tipo).

| Tipo | Nombre | Efecto | Código |
|---|---|---|---|
| 0x01 | Arrow | flecha (M; gasta flechas, E046) [O*] | E510 |
| 0x02 | Ceramic Arrow | atraviesa enemigos y es más lenta [G] | |
| 0x03 | Rolling Fire | recorre la plataforma del jugador [G] | |
| 0x04 | Fire | corre por el suelo y cae al de abajo [G] | |
| 0x05 | Mine | mina: daño alto, también a ti. Popolon pone 2, Aphrodite 3. Se compra a Hefesto [G]. Tiene caso propio al dispararla [C] | b03:A72B |
| 0x06 | Magnifying Glass | con M delante de una lápida, revela la palabra del jefe [G]. Activa E517 [C] | b03:AD46 |
| 0x07–0x0A | Holy Water, Cloak, Magic Rod, Map | uno de cada en cada mundo [C]. Según la guía: daño doble al jefe, daño del jefe a la mitad, munición infinita contra el jefe y mapa del mundo en el menú [G]. Magic Rod comprobado: con E078 y E0D2 ≠ 0 los disparos no gastan flechas (b03:AA8E) [C] | b00:462E |
| 0x0B | Cross | necesaria para vencer a Galious [G]. Interviene en el combate con el jefe (b03:BBA8) [C] | b03:BBA8 |
| 0x0C | Great Key | abre la puerta del mundo siguiente [C][O]. Sube 32 la vitalidad máxima, hasta 192 [G] | b03:B701 |
| 0x0D | Necklace | **Popolon** lo necesita para entrar en el mundo 7 [C] | b02:9A41 |
| 0x0E | Crown | inmune al contacto con 0x2A, los Uh del mundo 2 [C][G] | b03:B1F7 |
| 0x0F | Helmet | inmune al contacto con 0x28, el ectoplasma [C][G] | b03:B1F7 |
| 0x10 | Oar | andar sobre el agua en el mundo 5, salas 12 y 13 [C][G] | b03:B037 |
| 0x11 | Shoes | escaleras un 25 % más rápido [O*]. La guía dice "andan más rápido", pero en mi medida el paso no cambia | b03:A6B6 |
| 0x12 | Decorative Doll | hace aparecer el puente del mundo 2 [O*] | b08:9F87 |
| 0x13 | Hoi | sin él, izquierda y derecha van al revés en el mundo 8 [O*]. Aparece al matar a todos en una sala del castillo [G] | b03:A8A6 |
| 0x14 | Bell | suena al acercarte a la puerta del mundo de la llave que llevas [G]; código por progreso [C] | b00:5D24 |
| 0x15 | Angel Ring | F1 + Return: vuelves al santuario de Demeter, que es la sala 72 [O*] | b02:9AC9 |
| 0x16 | Candle | ilumina salas oscuras del mundo 5 [O*] | b00:55F7 |
| 0x17 | Armor | inmune a 0x2E, los disparos de bambú [C][G] | b03:B1F7 |
| 0x18 | Carpet | convierte en piedra la lava del mundo 8 [G]. Rellena tiles con F0 (b00:5575) y apaga generadores de enemigos (b04:6200) [C] | b04:6200 |
| 0x19 | Hat Helmet | recibes la mitad de daño [C] | b03:B3BF |
| 0x1A | Lamp | marca en el mapa la sala del jefe [G]. Sprite indicador por mundo [C] | b01:668F |
| 0x1B | Vase | duplica la experiencia que ganas [C]. Aparece tras recuperar el mundo 4 con una secuencia de 9 acciones de espada y salto (E536 = 9) [G][C] | b03:B62E, b02:9FC1 |
| 0x1C | Pendant | **Aphrodite puede romper las paredes que suenan**; sin él, solo Popolon [O*]. Así se consigue el muñeco del mundo 2 | b09:A668 |
| 0x1D | Earrings | Popolon dispara 3 flechas a la vez (no los fuegos) [C][O] | b03:A99E (E08C) |
| 0x1E | Bracelet | Popolon pone 3 minas [C][O] | b03:A9B7 (E08D) |
| 0x1F | Ring | Aphrodite rompe piedras con 3 golpes en vez de 15 [O*] | b03:B841 |
| 0x20 | Bible | CTRL congela a los enemigos, como mucho 16 veces [O*]. Yo medí unos 5 s; la guía dice 15 | b02:9BB6 |
| 0x21 | Harp | un golpe de espada rompe todas las piedras [O*] | b01:6398 |
| 0x22 | Triangle | inmune a 0x31, las nubes blancas o vapores [C][G] | b03:B1F7 |
| 0x23 | Conch Shell | **Aphrodite** la necesita para entrar en el mundo 5 [C] | b02:9A33 |
| 0x24 | Pitcher | inmune a 0x08, las bolas de fuego [C][G] | b03:B1F7 |
| 0x25 | Saber | la espada quita más vida [C]. Aparece al matar a los murciélagos de una sala del castillo después de recuperar el mundo 6 (E068 bit 2 + E61C) [C][G] | b03:A003 |
| 0x26 | Dagger | escribir UMBRELLA mata a los murciélagos de la pantalla [G]. Aparece al introducir derecha, abajo, izquierda y arriba sobre el ascensor horizontal (tabla A056) [C][G] | b03:A01A |
| 0x27 | Wing | en el menú (F1), el número de un mundo te lleva a su puerta [O*] | b02:9A99 |
| 0x28, 0x2B, 0x2C | Copper, Silver y Gold Shield | detienen disparos de demonios menores, medios y todos. Nivel en E097: 1, 2 y 3 [C][G] | b03:B74B |
| 0x29 | Bread & Water | Aphrodite cruza las puertas giratorias [G] (b09:A345 compara E098 con un objeto a la altura del jugador [C]) | b09:A345 |
| 0x2A | Salt | necesaria para entrar en el santuario de la Muerte (sala de dios de tipo 3) [C][G] | b02:9A84 |
| 0x2D–0x2F | carcaj, moneda, llave | +50 flechas, +10 monedas, +2 llaves [C][O] | b03:B701 |

**Mecánicas que describe la guía y aún no están localizadas en la ROM:**
- **Asclepio:** cura. La Muerte (resurrección) solo está localizada en parte. Tiendas y hadas: ver `entities.md`.
- **Experiencia:** llenar la barra repone la vitalidad.
- **Callejones que se cierran a tu espalda.**
- **Piedras que aparecen** si esperas 15 s en el borde, en el mundo 5.
- **Puertas giratorias** que Popolon cruza siempre y Aphrodite solo con Bread & Water.
- **Contraseñas especiales** y extras con otro cartucho de Konami en el segundo slot.

Las inmunidades salen de b03:B1F7–B22D, la colisión entre jugador y enemigo: si tienes el objeto, se sale sin hacer daño.

**Dónde están**: 0x2D, 0x2E y 0x2F aparecen en 13, 25 y 14 salas del castillo, casi siempre bajo piedras. 0x07–0x0A, una vez por mundo. 0x0C, en el castillo y en cada mundo (el jefe la suelta, véase `state_machine.md`). El resto son únicos y están en el castillo. La lista completa está en `items.json`.

## Enemigos

**Creación** (b04:6000 → 65DA) [C]: si no se ha llegado al límite (E620 < E635), ocupa un hueco de 0x20 bytes en **E800 + 0x20·n** y copia el descriptor del tipo, de 5 bytes, desde **b04:67B6 + 5·tipo**:

| Campo | Destino | Significado |
|---|---|---|
| d0 | IX+9 | flags (bit 7: tiene sprite) [I] |
| d1 | IX+0A | atributos o patrón de sprite [I] |
| d2 | IX+0B | ¿tamaño? En los objetos de tiles (d0 bit 6) elige cómo se dibujan (tabla de b01:6F2C; 7 = bloque 2×2 de tiles d1…d1+3) [C] |
| d3 | IX+10 | **vida**; FE/FF = no se puede dañar [C][O] |
| d4 | IX+11 | **caja de colisión**: índice en la tabla b14:9006 + 4·IX+11 (dy, dx, alto, ancho), que usa el contacto con el jugador (b03:B954). La IA la cambia con cada fotograma [C][O]. No es la recompensa: el botín se decide por tipo (ver «Botín») [C] |

Estructura en RAM [O]: +0 tipo, +5 Y, +7 X, +9..+0B descriptor, +10 vida, +11 caja de colisión. Comprobado con esqueletos (vida 8), murciélagos (1), 0x18 (1) y estatuas (254).

Después, una tabla de saltos por tipo (b04:66B4) lleva a la rutina de inicialización de cada enemigo, y de ahí sale su comportamiento.

**Daño y muerte** [C]:
- Cada golpe resta 1 a IX+10 (o 2, según el ataque), +1 con el objeto 0x25 (b03:B57A–B5E0).
- En 0, b03:B604 lo mata (ver «Muerte y botín»).
- Con los jefes manda además la tabla de vulnerabilidades por arma (ver `state_machine.md`, «Jefes»).

**Objetos especiales que se tratan como enemigos** [C]: 0x3F es la bala del soldado 0x2D: al tocarla paraliza al jugador 60 ticks (E500 = 6, E50F = 0x3C; b03:B2E2). El hada es 0x4F. Tocar 0x6B dibuja tiles 7A… y activa E024 → estado 17.

### Muerte y botín [C]

Todo se decide por el **tipo** del enemigo (IX+0), no por IX+11: b03:B63C guarda el tipo en E6C7, la explosión 0x5B lo copia en su IX+11 al nacer (b04:6A0D) y lo devuelve a E6C7 al acabar (6A3B). Las tablas b04:647B y b04:64E5 se indexan con ese tipo.

**Al morir** (b03:B604):
1. Sonido 0x20.
2. **Experiencia**: b03:BF27[tipo] (×2 con el Vase, E08A), sumada al byte alto de E05C/E05D (E501 = 0) o E05F/E060 (E501 ≠ 0) por b01:6158. El 0x19 usa {1, 2, 4, 4} según su tamaño (IX+1B). Valores por tipo en la tabla de abajo.
3. Si estaba marcado (IX+2 bit 3), resta 1 a E622 y, al llegar a 0, lanza el evento de sala (ver `entities.md`, «matar a los marcados»). Se marca al nacer si la sala tiene E610 = 1 y su tipo está en b04:6584.
4. Borra el hueco (E620 − 1).
5. Según el tipo:
   - **Normal:** crea la explosión 0x5B en su (y, x).
   - **0x15, 0x16, 0x40, 0x42, 0x4C** (los grandes): no crea explosión. Pone E63F = 0x41 y E6C0 = (y, x). Después b04:69D2 crea cuatro 0x5B, una cada 16 llamadas, en (y+8, x+8), (y, x+16), (y+16, x) e (y+16, x+16). Cada una hace su propia tirada de botín.
   - **0x43:** antes borra todos sus rayos 0x62.
   - **0x5C:** crea 0x39 o 0x6B (según IX+1F), sin explosión ni botín.

**Explosión 0x5B**: 8 fotogramas de 3 ticks (24 ticks de IA, ≈ 0,8 s). No se mueve y se anima también con el libro (E0D5). Al acabar se borra y decide el botín (6A3B):

1. **Regla** = 647B[tipo]. b06:B57A anota hasta **2 tipos distintos por sala** en E6C2/E6C3, con un contador de muertes en E6C4/E6C5. Si el tipo no está anotado y ya hay dos, no hay botín. La regla 4 no anota nada. La zona E600–F03F se pone a 0 al entrar en cada sala (b00:525E, desde los estados 4, 8, 10 y 13, y al salir del menú de objetos), así que todo cuenta **por visita a la sala**.

   | Regla | Hay tirada cuando… |
   |---|---|
   | 0 | es la 1.ª o la 3.ª muerte de ese tipo |
   | 1 | es la 2.ª o la 5.ª |
   | 2 | es la 4.ª o la 6.ª |
   | 3 | no queda ningún otro enemigo vivo de ese tipo en los 16 huecos (cada vez que ocurre) |
   | 4 | nunca |

   Más allá de la 6.ª muerte, las reglas 0–2 no vuelven a dar nada (el contador es de un byte).
2. **Nivel** según 64E5[tipo], que es también el daño por contacto: 0–2 → 0, 3–5 → 1, 6–8 → 2, ≥ 9 → 3 (b06:B562).
3. **Tirada** (6A77): v = (R + E604) AND 15. R es el registro de refresco del Z80, así que v sale prácticamente uniforme en 0–15 [I]. Umbrales en 6A9E + 3·nivel:

   | Nivel | Umbrales | Monedas (0x5E) | Flechas (0x5D) | Llaves (0x5F) | Nada |
   |---|---|---|---|---|---|
   | 0 | 08 0A 0B | v < 8: 8/16 | 8–9: 2/16 | 10: 1/16 | 11–15: 5/16 |
   | 1 | 09 0B 0C | v < 9: 9/16 | 9–10: 2/16 | 11: 1/16 | 12–15: 4/16 |
   | 2 | 0A 0D 0F | v < 10: 10/16 | 10–12: 3/16 | 13–14: 2/16 | 15: 1/16 |
   | 3 | 0A 0D 0F | igual que el nivel 2 | | | |

4. **Cantidad** (tabla 6A6B + 4·(objeto − 0x5D) + nivel, BCD):

   | Objeto | Nivel 0 | 1 | 2 | 3 | Contador |
   |---|---|---|---|---|---|
   | 0x5D flechas | 10 | 20 | 30 | 40 | E046 |
   | 0x5E monedas | 1 | 4 | 7 | 10 | E048 |
   | 0x5F llaves | 1 | 1 | 2 | 2 | E04A |

5. Lo crea con b04:6551 en **(y AND 0xF8, x AND 0xF8)** de la explosión, es decir, en la celda de 8 px de la esquina superior izquierda del enemigo. La cantidad va en IX+18. Como cualquier enemigo, no nace si la sala está llena (E620 ≥ E635), y en ese caso el botín se pierde.

**El objeto 0x5D/0x5E/0x5F** (descriptor `40 60|64|68 07 00 00`):
- Se dibuja con **tiles de fondo, no con un sprite**: un bloque de 2×2 tiles (d1…d1+3: 60–63 flechas, 64–67 monedas y 68–6B llaves, los mismos que los recursos 0x2D–0x2F) en el mapa ED00, en la dirección que guarda en IX+12/13 (b01:6F73). Monedas y flechas se pintan en una pasada y llaves y el resto en la otra, alternando con E630.
- **Quieto**: su actualización es un `RET` y no tiene el bit 4 de IX+2 (C0), así que no cae, no rebota, no parpadea y **no caduca**. Si el enemigo muere en el aire o sobre un foso, el objeto se queda flotando ahí. Desaparece al recogerlo o al salir de la sala, y mientras tanto ocupa un hueco (cuenta en E620).
- **Recogerlo** (b03:B1ED → B2CB): caja del jugador de 12×12 desde (y − 14, x − 6) (b03:B8A0), contra la caja 0 del objeto: dy 0, dx 0, 16×16. Se toca si −12 ≤ (y_j − 14) − y_o ≤ 16 y −12 ≤ (x_j − 6) − x_o ≤ 16, es decir, y_j ∈ [y_o + 2, y_o + 30] y x_j ∈ [x_o − 6, x_o + 22]. Funciona aunque el jugador esté parpadeando (no mira E518). Se coge uno por frame (E61F).
- Al cogerlo: se borra, suena 0x2D y suma la cantidad en BCD con b00:4D5A/4D7B/4D82 al contador E046/E048/E04A. El **tope es 999**: si el byte alto llega a 0x10, queda en 09 99. Después se redibuja la fila del marcador.

**Hijos y proyectiles**: casi todos tienen la regla 4. Mueren por la misma vía (B604) y sí tienen botín 0x0D (regla 1, nivel 0), 0x1C (2, 0), 0x21 (1, 0), 0x30 (2, 1), 0x38 (2, 0), 0x39 (3, 3), 0x3E (3, 1) y 0x53 (3, 0). Lo que desaparece por otro camino (fuera de la pantalla, al chocar, por `4257` directo) no pasa por la explosión y no da nada.

**Botín por tipo de las salas** (regla, nivel; * = no se puede matar, así que nunca da nada). También en `extracted/unity/enemies.json`:

| Regla | Nivel | Tipos |
|---|---|---|
| 0 (1.ª y 3.ª) | 0 | 03 0E 18 28 2E |
| 0 | 1 | 02 07 |
| 0 | 2 | 01 05 0C 13 27 |
| 1 (2.ª y 5.ª) | 0 | 10 11 |
| 1 | 1 | 0A 0F 12 1F 2D (20*) |
| 1 | 2 | 14 34 |
| 1 | 3 | 0B |
| 2 (4.ª y 6.ª) | 0 | 04 2F |
| 2 | 1 | 17 24 25 |
| 3 (última viva) | 0 | 19 2A |
| 3 | 1 | 26 3C |
| 3 | 2 | 16 22 4C (16 y 4C: cuatro tiradas) |
| 3 | 3 | 15 2B 40 42 43 (15, 40 y 42: cuatro tiradas) |
| 4 (nunca) | — | 23 2C 49 64 (29* 31* 32* 36* 3A* 4E* 50* 51* 69*) |

Experiencia (BF27) de los tipos de las salas: 01:6 02:3 03:1 04:1 05:5 07:2 0A:4 0B:4 0C:4 0E:1 0F:4 10:1 11:2 12:3 13:3 14:4 15:8 16:6 17:3 18:1 19:1–4 1F:4 22:6 23:4 24:2 25:4 26:3 27:5 28:2 2A:1 2B:4 2C:2 2D:2 2E:1 2F:1 34:1 3C:4 40:8 42:4 43:8 49:3 4C:5 64:2; el resto, 0.

**Dudas**: la cadencia de b04:69D2, las cuatro explosiones de los grandes (llamadas o frames). Si el jugador mata a otro enemigo mientras explotan los grandes, E6C7 cambia y las explosiones siguientes tiran con ese otro tipo [C, caso raro]. Falta confirmarlo todo en openMSX [O].

**Resumen** (`enemies.json`):
- 53 tipos con generador.
- La mayoría aparece en un solo mundo: cada mundo tiene su propio bestiario. El murciélago 0x0E está en 8 mundos y suma 294 generadores.
- Los más duros: 0x23 (vida 64), 0x15 (40), 0x40 (35), 0x16 y 0x22 (24), 0x4C (20).
- No se pueden dañar: 0x20, 0x29, 0x31, 0x32, 0x36, 0x3A, 0x50, 0x51 y 0x69.

## Cajas de colisión

Todo sale del código (b03:B1C1, B492, B6C1, B8A0–B9AF; b06:B067–B0EC) [C]. Datos en `extracted/unity/enemies.json` (`tools/extract/enemy_boxes.py`).

### La prueba de solape [C]

Una sola rutina, b03:B96D, para contacto, espada y armas. Recibe la caja del atacante (E, D = y, x de la esquina; E′, D′ = alto, ancho) y la del enemigo (b03:B954: tabla **b14:9006 + 4·IX+11** = dy, dx, alto, ancho, con origen en (IX+5 + dy, IX+7 + dx); dy y dx con signo). Antes suma alto y ancho de las dos más 1 (b03:B23E/B4F4). Con aritmética de 8 bits, hay toque si

- y_A ≤ y_B + alto_B **y** y_B ≤ y_A + alto_A, y lo mismo en x.

Los **bordes son inclusivos**: el «alto» y el «ancho» de la tabla son la distancia entre el primer y el último píxel, así que una caja ocupa (alto + 1) × (ancho + 1) píxeles. Dos cajas que solo comparten la última fila o columna ya se tocan. Para el remake: rectángulo semiabierto [y, y + alto + 1) × [x, x + ancho + 1).

### Jugador, espada y armas [C]

Coordenadas del jugador: y = pies (E505), x = centro (E507). Rangos inclusivos.

| Caja | Origen (y, x) | alto × ancho | Píxeles que cubre | Rutina |
|---|---|---|---|---|
| Contacto con enemigos y recogida de objetos | (y − 14, x − 6) | 12 × 12 | y − 14 … y − 2, x − 6 … x + 6 | b03:B8A0 |
| Espada a la derecha (E503 = 0) | (y − 7, x + 8) | 3 × 10 | y − 7 … y − 4, x + 8 … x + 18 | b03:B8FB |
| Espada a la izquierda (E503 ≠ 0) | (y − 7, x − 18) | 3 × 10 | y − 7 … y − 4, x − 18 … x − 8 | b03:B8FB |

- La caja del jugador **no cambia con la postura**: es la misma en la escalera, en el salto y al golpear. No hay agachado.
- **Contacto** (b03:B1C1, cada frame): recorre los huecos con IX+2 bit 6. Se para en el primer toque (E61F). No mira a nadie si el jugador es invulnerable (E518 ≠ 0), salvo los objetos 0x5D–0x5F, que se recogen igual. Tampoco hay contacto con 0x08, 0x2A, 0x28, 0x2E y 0x31 si están puestos E093, E07D, E07E, E086 y E091 (objetos protectores).
- **Espada** (b03:B6C1): solo mientras E51B ≠ 0 (lo pone b03:A761 al empezar el espadazo). Prueba los 16 huecos con IX+2 bit 5. En el primer toque, E51B = 0: **un golpe por espadazo**. Con IX+2 bit 1 (escudo), el golpe solo vale si (IX+2 bit 0) = E503. Si no, se pierde sin daño. Con IX+2 bit 2 suena el «clinc» (b03:AE88) y no hace daño. La misma caja sirve contra las piedras (EC30, b03:B877) y contra el jefe (b03:BD55).
- **Armas secundarias** (b03:B492 → B8B2): huecos E540 + 0x20·n (6), tipo IY+0 = 1–5 [I: el número de arma de `player.md`] con IY+2 bit 0. Origen (IY+5, IY+7):

| Tipo | Origen | alto × ancho | Filtro |
|---|---|---|---|
| 1 flecha | ((y AND F8) + 2, (x AND F8) + 2) | 3 × 12 | — |
| 2 flecha de cerámica | igual | 3 × 12 | no toca enemigos con IX+3 bit 5 (b03:B53D) |
| 3 fuego rodante | (y, x) | 8 × 8 | — |
| 4 fuego | (y, x) | 4 × 8 | — |
| 5 mina | (y, x + 2) | 4 × 12 | — |
| 5 mina explotada (IY+2 bit 2) | (y − 16, x) | 32 × 32 | no toca enemigos con IX+3 bit 4 (b03:B528) |

[I] En la comprobación de la espada, b03:B528/B53D leen IY, que queda en E600 tras el bucle de armas. Esos filtros no se aplican a la espada salvo que E600 valga 2 o 5.

### Cajas que no salen de la tabla [C]

b03:B23E y B4F4 calculan la caja en estos casos:

- **0x60** (`columnDown`, chorro): dy 0, dx 0, alto 8·((IX+9 AND 7) − 1), ancho 7 (B916).
- **0x57** (`row`): dy 0, dx 0, alto 8, ancho 8·IX+1F (B926).
- **0x62** (`columnUpDown`, rayo): n = IX+9 AND 7, alto 8·n, ancho 7, dx 0 y dy = −8·n si IX+15 bit 0 (B933).
- **0x06** (`lowerSegment`, rayo de 0x69): caja de tabla (10), pero con origen en (IX+12, IX+13), salvo en el estado 4 (B2A0/B51D).
- **0x5B** (explosión): IX+11 guarda el **tipo** del enemigo muerto, no una caja. Tiene IX+2 = 80, así que no hay contacto ni golpes.

### Cómo cambia IX+11 [C]

- **Valor inicial**: el 5.º byte del descriptor.
- **XOR por animación**: b06:B0D6/B0DD (y B17F/B183, cada 8 o 4 frames) hacen IX+0A ^= D e IX+11 ^= B a la vez. Por eso la caja va emparejada con el sprite. Con B = 0 la caja no cambia.
- **Ganchos**: algunas IA dejan en la pila b06:B067/B06C/B071/B076/B07B/B080/B085. Al volver, IX+11 = tabla[(IX+0A − A0)/4] (B085: base C4). Las tablas son B0A7, B0B0, B0B6, B0C1, B0C8, B09F y B0D0. 0x28 y 0x31 usan B088 con sus propias tablas (93CC y 8C17, base C4).
- **Tablas de palabras** (sprite, caja) que lee b06:736F: 0x0F (73AA/73B2), 0x18/0x54/0x55 (6CC4), 0x2E (A0D1) y 0x25 (9A61). 0x41 elige caja según la dirección (8DC5).
- **Escrituras directas**: ver la columna «Estados → caja».

En `enemies.json`, cada tipo trae:

- `box` (inicial) y `boxes` (todas las posibles);
- `boxFrames` = [{sprite, box}]. Con ganchos y tablas es exacto. Con XOR es la pareja nominal [I: tras un cambio de estado la fase sprite/caja puede cruzarse; 0x07 conserva la caja en el salto];
- `boxSizeChanges`, `boxSpecial` y `boxNote`.

También van la tabla (`boxes` como [dy, dx, alto, ancho] y `boxRects` como objetos), `playerContactBox`, `swordBoxRight`/`swordBoxLeft` y `weaponBoxes`.

### Contraste con el remake

- **Contacto**: el remake usa x − 8 … x + 7, y − 16 … y − 1 (16 × 16). El original usa x − 6 … x + 6, y − 14 … y − 2 (13 × 13 px). Con el esqueleto 0x25 (caja 4E: dy 1, dx 0, 15 × 24), el daño empieza cuando x + 6 ≥ IX+7. La medida en openMSX daba «x + 7 toca el borde izquierdo», un píxel de diferencia [O pendiente de repetir con la caja exacta].
- **Espada**: el alcance en x coincide a la derecha (x + 8 … x + 18). A la izquierda el remake va un píxel más allá (x − 19 … x − 9 frente a x − 18 … x − 8). En altura, el original cubre solo y − 7 … y − 4; el remake, y − 12 … y − 3.

### Tabla b14:9006 (0x7A cajas; a partir de 91EE empiezan los punteros de los jefes, b03:BD73)

Valores en decimal. dy y dx tienen signo.

| i | dy dx alto ancho | i | dy dx alto ancho | i | dy dx alto ancho | i | dy dx alto ancho |
|---|---|---|---|---|---|---|---|
| 00 | 0 0 16 16 | 01 | -13 0 29 16 | 02 | 0 7 5 9 | 03 | 0 0 10 10 |
| 04 | 1 1 7 7 | 05 | 0 0 6 14 | 06 | 0 0 8 9 | 07 | 0 0 9 9 |
| 08 | 0 7 9 9 | 09 | 0 0 14 16 | 0A | 0 0 4 4 | 0B | 5 0 11 16 |
| 0C | 2 2 14 14 | 0D | 2 0 14 14 | 0E | 0 0 11 16 | 0F | 0 0 9 16 |
| 10 | 0 0 16 7 | 11 | 0 3 4 10 | 12 | 0 1 16 13 | 13 | 1 0 15 16 |
| 14 | 0 0 8 8 | 15 | 0 2 16 11 | 16 | 0 1 16 15 | 17 | 0 0 16 11 |
| 18 | 0 3 16 13 | 19 | 8 0 8 16 | 1A | 0 0 14 15 | 1B | 9 0 7 8 |
| 1C | 6 0 10 10 | 1D | 3 0 13 13 | 1E | 12 0 4 16 | 1F | 0 0 7 16 |
| 20 | 0 4 8 12 | 21 | 0 0 8 12 | 22 | 0 0 7 8 | 23 | 0 0 13 13 |
| 24 | 0 2 12 14 | 25 | 0 0 12 14 | 26 | 10 2 6 11 | 27 | 8 2 8 11 |
| 28 | 4 2 12 11 | 29 | 1 2 15 11 | 2A | 0 0 16 15 | 2B | 10 0 6 16 |
| 2C | 0 0 5 6 | 2D | 0 0 9 14 | 2E | 0 0 7 12 | 2F | 0 0 8 15 |
| 30 | 0 0 6 7 | 31 | 0 2 15 14 | 32 | 12 0 4 4 | 33 | 12 12 4 4 |
| 34 | 8 0 8 8 | 35 | 8 8 8 8 | 36 | 4 0 12 12 | 37 | 4 4 12 12 |
| 38 | 12 3 4 10 | 39 | 0 3 31 26 | 3A | 0 0 16 3 | 3B | 0 0 3 16 |
| 3C | 1 5 15 22 | 3D | 6 6 20 20 | 3E | 2 1 7 15 | 3F | 2 0 7 15 |
| 40 | 0 2 6 14 | 41 | 2 0 14 32 | 42 | 6 0 25 32 | 43 | 2 0 30 32 |
| 44 | 0 0 6 6 | 45 | 5 0 22 32 | 46 | 2 0 12 16 | 47 | 1 3 13 10 |
| 48 | 1 0 7 10 | 49 | 0 1 9 8 | 4A | 0 0 5 5 | 4B | 0 1 5 3 |
| 4C | 0 0 13 14 | 4D | 2 0 14 16 | 4E | 1 0 15 24 | 4F | 1 -8 15 24 |
| 50 | 0 1 14 14 | 51 | 0 0 8 6 | 52 | 0 2 16 12 | 53 | 0 0 13 9 |
| 54 | 0 0 10 8 | 55 | 0 1 16 14 | 56 | 0 0 5 9 | 57 | 0 2 7 13 |
| 58 | 6 0 10 16 | 59 | 6 2 10 14 | 5A | 6 0 10 14 | 5B | 0 0 32 32 |
| 5C | 6 0 10 8 | 5D | 8 0 8 10 | 5E | 0 0 10 5 | 5F | 0 2 3 14 |
| 60 | 0 0 3 14 | 61 | 0 0 12 12 | 62 | 0 4 16 8 | 63 | 4 0 8 16 |
| 64 | 0 0 14 14 | 65 | 0 4 14 14 | 66 | 0 1 6 14 | 67 | 0 3 11 10 |
| 68 | 0 1 9 14 | 69 | 1 0 9 16 | 6A | 0 2 14 28 | 6B | 2 5 14 22 |
| 6C | 0 -2 24 14 | 6D | 3 2 12 12 | 6E | 3 2 10 10 | 6F | 4 4 8 8 |
| 70 | 5 5 6 6 | 71 | 0 3 16 10 | 72 | 9 4 7 7 | 73 | 6 3 10 10 |
| 74 | 4 2 10 12 | 75 | 2 2 12 12 | 76 | 0 4 7 7 | 77 | 0 3 9 10 |
| 78 | 6 6 5 5 | 79 | 0 0 24 16 |  |  |  |  |

### Cajas por tipo

«Inicial» = índice (alto × ancho de la tabla). En las notas, «sprite → caja». «Cambia» marca un cambio de tamaño apreciable.

| Tipo | Inicial | Cajas | Cambia | Estados → caja |
|---|---|---|---|---|
| 01 | 00 (16×16) | 00 01 13 | **sí** | patrulla 00↔13 cada 8 f (sprite ^4 a la derecha, ^0C a la izquierda); embestida AC/C0 → 01 (29 de alto, sube 13 px); A8 → 00 |
| 02 | 75 (12×12) | 75 | no | caja fija |
| 03 | 05 (6×14) | 05 66 74 77 | **sí** | quieta C4 → 66 (6x14); avanza C8 → 74, CC/D0 → 77 |
| 04 | 22 (7×8) | 22 | no | caja fija |
| 05 | 2A (16×15) | 00 02 16 2A 56 | no | gancho B080: caja según sprite cada frame |
| 06 (hijo) | 10 (16×7) | 10 | no | `lowerSegment`: caja 10 con origen en IX+12/IX+13 (segmento inferior) salvo en el estado 4 |
| 07 | 26 (6×11) | 26 27 | no | anda 26↔27 cada 10 f (sprite ^4, caja ^1); en el salto conserva la caja |
| 08 (hijo) | 4D (14×16) | 07 0C 4D 50 | no | gancho B085 (base C4): CC → 0C, D0 → 50 |
| 09 (hijo) | 13 (15×16) | 13 | no | caja fija |
| 0A | 4E (15×24) | 4E 4F 55 | **sí** | anda a la derecha B0 → 4E y a la izquierda C0 → 4F (24 de ancho); escalera A0/A8 → 55 |
| 0B | 3E (7×15) | 04 05 3E 3F | **sí** | nada A0/3F ↔ A4/05 (^3A cada 8 f); A8/05 ↔ AC/3F; al girar A0 → 3E (y A4 → 04) |
| 0C | 51 (8×6) | 16 2A 51 52 53 54 | **sí** | gancho B067: crece A0 → 51 (8x6), A4/A8 → 52, AC → 53, B0 → 54; vuelo B4..C0 → 2A/16 |
| 0D (hijo) | 00 (16×16) | 00 | no | caja fija |
| 0E | 69 (9×16) | 0F 67 69 | **sí** | colgado A0 → 67; se suelta AC → 69; revolotea AC/69 ↔ B0/0F (^66 cada 4-6 f) |
| 0F | 32 (4×4) | 00 32 33 34 35 36 | **sí** | crece por etapas (IX+1E 0..3): derecha C0/33, C8/35, D0/36, D8/00; izquierda A0/32, A8/34, B0/36, B8/00; anima solo el sprite |
| 10 | 0B (11×16) | 0B | no | caja fija |
| 11 | 58 (10×16) | 00 58 59 5A | **sí** | anda A4/B4 → 58; descansa B0 → 59 / C0 → 5A alternando; dispara AC/BC → 00 |
| 12 | 13 (15×16) | 0A 13 16 2A | no | gancho B07B: emerge y rueda con cajas de 16 (13/16/2A); oculto con IX+2 bit 2 |
| 13 | 2D (9×14) | 00 0C 15 1A 2D 2E 2F 30 31 | **sí** | gancho B071: anda A0..B0 (2D/2E/1A/2F), se desvanece B4 → 30 (6x7), vuela BC/C0 → 15/31 |
| 14 | 38 (4×10) | 13 16 19 1E 2A 38 | **sí** | gancho B076: emerge A0 → 38 (4 alto), A4 → 1E (4), A8 → 19 (8); pasea AC..B8 → 16/13/2A/13 |
| 15 | 42 (25×32) | 42 43 | no | sprites 08/09 alternando cada 8 f → 42/43 |
| 16 | 01 (29×16) | 01 | no | caja fija |
| 17 | 13 (15×16) | 09 13 | no | 13 ↔ 09 con la animación (^1A) |
| 18 | 1E (4×16) | 0B 1E 1F 25 | **sí** | tabla 6CC4 por IX+1F: A0/1E (4 alto), A8/0B, AC/0B, A4/1F (7), B4/25 (12x14), B0/0B |
| 19 | 4A (5×5) | 46 47 48 49 4A 4B | **sí** | nivel 0 D0/4A ↔ 4B (5x5); nivel 1 C8/48 ↔ 49; nivel 2 C0/46 ↔ 47 (^1) |
| 1A (hijo) | 05 (6×14) | 05 | no | caja fija |
| 1B (hijo) | 05 (6×14) | 05 | no | caja fija |
| 1C (hijo) | 22 (7×8) | 22 | no | caja fija |
| 1D (hijo) | 05 (6×14) | 05 | no | caja fija |
| 1E (hijo) | 0A (4×4) | 0A | no | caja fija |
| 1F | 00 (16×16) | 00 13 | no | corre 00 ↔ 13 (^13); salto A8/B4 → 00 |
| 20 | 19 (8×16) | 19 | no | caja fija |
| 21 (hijo) | 58 (10×16) | 58 59 5A | no | 58 ↔ 59 a la derecha, 58 ↔ 5A a la izquierda (^1 / ^2) |
| 22 | 09 (14×16) | 09 | no | caja fija |
| 23 | 6C (24×14) | 6C | no | caja fija |
| 24 | 00 (16×16) | 00 09 | no | anda 00; tras saltar A8/B4 → 09; al aterrizar 00 |
| 25 | 4E (15×24) | 0B 19 4D 4E 4F | **sí** | anda AC → 4E / B8 → 4F (24 de ancho); enroscado tabla 9A61 por IX+19: 2 A0/4D, 1 A4/0B, 0 A8/19 |
| 26 | 12 (16×13) | 12 | no | caja fija |
| 27 | 11 (4×10) | 00 11 | **sí** | dormida C4 → 11 (4x10); despierta C8/CC → 00 |
| 28 | 62 (16×8) | 0C 0D 62 63 64 65 | no | tabla 93CC (base C4) según la dirección: 62, 63, 0D, 64, 0C, 65 |
| 29 | 0E (11×16) | 0E 68 | no | 0E ↔ 68 cada 8 f (^66) |
| 2A | 00 (16×16) | 00 | no | caja fija |
| 2B | 13 (15×16) | 13 | no | caja fija |
| 2C | 0B (11×16) | 0B 0C 0D | no | gancho B06C: A0/AC → 0B, A4/A8 → 0C, B0/B4 → 0D |
| 2D | 0C (14×14) | 00 0C 0D | no | derecha A0/0C ↔ A4/00, izquierda AC/0D ↔ B0/00 (^0C/^0D cada 4 f) |
| 2E | 26 (6×11) | 26 27 28 29 | **sí** | tabla A0D1 por IX+1E: C4/26 (6 alto), C8/27 (8), CC/28 (12), D0/29 y D4/29 (15) y vuelta |
| 2F | 00 (16×16) | 00 | no | caja fija |
| 30 (hijo) | 41 (14×32) | 41 | no | caja fija |
| 31 | 1B (7×8) | 1B 72 73 74 75 76 77 | **sí** | tabla 8C17 (base C4). Suelo: C8/73, CC/74, D0/75; techo: D8/77, DC/74, E0/75. C4/D4 no dan contacto (IX+2 sin bit 6); sprite 00 oculto |
| 32 | 79 (24×16) | 79 | no | caja fija |
| 33 (hijo) | 5F (3×14) | 5F 60 | no | derecha C4 → 5F, izquierda C8 → 60 |
| 34 | 22 (7×8) | 22 | no | caja fija |
| 35 (hijo) | 13 (15×16) | 13 | no | caja fija |
| 36 | 12 (16×13) | 11 12 | **sí** | 12 ↔ 11 cada 12 f (^3) |
| 37 (hijo) | 3D (20×20) | 3C 3D | no | 3D ↔ 3C (^1) |
| 38 (hijo) | 66 (6×14) | 66 67 | **sí** | quieta C4 → 66; cae C8 → 67 |
| 39 (hijo) | 4C (13×14) | 4C | no | caja fija |
| 3A | 00 (16×16) | 00 | no | caja fija |
| 3B (hijo) | 61 (12×12) | 61 | no | caja fija |
| 3C | 13 (15×16) | 13 | no | caja fija |
| 3D (hijo) | 14 (8×8) | 14 | no | caja fija |
| 3E (hijo) | 0F (9×16) | 0E 0F | no | 0F ↔ 0E (^1) |
| 3F (hijo) | 05 (6×14) | 05 21 | no | A8 → 05; con E624 B4 → 21 |
| 40 | 39 (31×26) | 39 | no | caja fija |
| 41 (hijo) | 00 (16×16) | 00 3A 3B 75 | **sí** | tabla 8DC5 por dirección: horizontal A4 → 3A, vertical A8 → 3B, diagonal C8/BC → 75 |
| 42 | 3C (15×22) | 3C | no | caja fija |
| 43 | 41 (14×32) | 41 | no | caja fija |
| 44 (hijo) | 1A (14×15) | 1A | no | caja fija |
| 45 (hijo) | 44 (6×6) | 44 | no | caja fija |
| 46 (hijo) | 44 (6×6) | 44 | no | caja fija |
| 47 (hijo) | 5C (10×8) | 3D 5B 5C 5D | **sí** | proyectil C0/5C ↔ C4/5D; explosión 0A → 3D (20x20, x-8 y-8) y 0B → 5B (32x32) |
| 48 (hijo) | 4A (5×5) | 4A | no | caja fija |
| 49 | 6A (14×28) | 6A 6B | **sí** | 6A ↔ 6B (^1) |
| 4A (hijo) | 0A (4×4) | 0A | no | caja fija |
| 4B (hijo) | 0A (4×4) | 0A | no | caja fija |
| 4C | 3D (20×20) | 3D | no | caja fija |
| 4D (hijo) | 54 (10×8) | 54 | no | caja fija |
| 4E | 00 (16×16) | 00 | no | caja fija |
| 4F (hijo) | 00 (16×16) | 00 13 | no | A4 → 00; A8 → 13 |
| 50 | 00 (16×16) | 00 | no | caja fija |
| 51 | 79 (24×16) | 79 | no | caja fija |
| 52 (hijo) | 69 (9×16) | 0F 69 | no | 69 ↔ 0F (^66) |
| 53 (hijo) | 56 (5×9) | 48 56 | no | 56 ↔ 48 (^1E) |
| 54 (hijo) | 00 (16×16) | 00 0B 1E 1F 25 | **sí** | como 0x18 (tabla 6CC4) |
| 55 (hijo) | 00 (16×16) | 00 0B 1E 1F 25 | **sí** | como 0x18 (tabla 6CC4) |
| 56 (hijo) | 51 (8×6) | 51 | no | caja fija |
| 57 (hijo) | 00 (16×16) | 00 | **sí** | `row`: sin tabla: dy 0, dx 0, alto 8, ancho 8*IX+1F (b03:B926) |
| 58 (hijo) | 6F (8×8) | 0D 6F | **sí** | 6F (8x8) ↔ 0D (14x14) (^62) |
| 59 (hijo) | 70 (6×6) | 70 | no | caja fija |
| 5A (hijo) | 6E (10×10) | 6E | no | caja fija |
| 5B (hijo) | 00 (16×16) |  | no | explosión: IX+11 guarda el tipo muerto, no una caja; IX+2 = 80, sin contacto ni golpes |
| 5C (hijo) | 00 (16×16) | 00 | no | caja fija |
| 5D (hijo) | 00 (16×16) | 00 | no | caja fija |
| 5E (hijo) | 00 (16×16) | 00 | no | caja fija |
| 5F (hijo) | 00 (16×16) | 00 | no | caja fija |
| 60 (hijo) | 00 (16×16) | 00 | **sí** | `columnDown`: sin tabla: dy 0, dx 0, alto 8*((IX+9 & 7) - 1), ancho 7 (b03:B916) |
| 61 (hijo) | 0A (4×4) | 0A | no | caja fija |
| 62 (hijo) | 00 (16×16) | 00 | **sí** | `columnUpDown`: sin tabla: dx 0, alto 8*n, ancho 7 con n = IX+9 & 7; dy = -8*n si IX+15 bit 0, si no 0 (b03:B933) |
| 63 (hijo) | 00 (16×16) | 00 | no | caja fija |
| 64 | 00 (16×16) | 00 0B 0C 0D | no |  |
| 65 (hijo) | 71 (16×10) | 71 | no | caja fija |
| 66 (hijo) | 6F (8×8) | 6F | no | caja fija |
| 67 (hijo) | 55 (16×14) | 55 | no | caja fija |
| 68 (hijo) | 00 (16×16) | 00 | no | caja fija |
| 69 | 0E (11×16) | 0E | no | caja fija |
| 6A (hijo) | 0B (11×16) | 0B | no | caja fija |
| 6B (hijo) | 00 (16×16) | 00 | no | caja fija |

## Dónde está la IA [C]

- **Creación** (b04:65DA): copia el descriptor y llama a la rutina de **inicio** del tipo, en la tabla b04:66B4 (índice = tipo − 1, 107 entradas).
- **Cada frame**, el bucle principal (b00:53C9) llama a b06:B3C5, que recorre los huecos E800 y para cada enemigo hace:
  - B3F7 y B3E7: parpadeo y temporizadores en IX+3;
  - **B45E**: la rutina de **actualización** del tipo, en la tabla b06:B472. No se llama si el libro está congelando (E0D5), salvo para el tipo 0x5B;
  - B407: aplica la velocidad y borra al enemigo si sale de la pantalla (y ≥ 0xB8 o x ≥ 0xF9). La velocidad es 8.8: IX+0E/0F en x e IX+0C/0D en y, que se suman a la posición en b06:B0ED/B11B. IX+16/17 es una aceleración vertical que B14C suma a la velocidad y.
  - IX+11 empieza con el valor del descriptor, pero la IA la reescribe como atributo del sprite.
  - La IA de cada enemigo se ejecuta uno de cada dos frames: 30 veces por segundo, contado con breakpoints [O]. Por eso las velocidades medidas en el emulador son más o menos la mitad de las del código.
- Diez rutinas de actualización se comparten entre tipos, por ejemplo 0x32/0x51, 0x3A/0x50 y 0x1D/0x1E/0x33/0x3F.
- `tools/extract/enemy_ai.py` desensambla el inicio y la actualización de cada tipo en `extracted/ai/<tipo>.asm`, siguiendo el flujo. Necesita `z80dis`.

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
- Contrastar en la ROM lo marcado [G] y las mecánicas de la guía que aún no están localizadas.
