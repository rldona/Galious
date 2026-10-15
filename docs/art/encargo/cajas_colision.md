# Cajas de colisión para el encargo de arte

Generado por `tools/extract/art_boxes.py` a partir de los datos extraídos del original (docs/items_enemies.md, «Cajas de colisión»; docs/boss_ai.md, «Cajas de los jefes»). **Lo que se ve es lo que golpea:** cada fotograma se dibuja en un lienzo con la proporción de su caja y el juego lo escala exactamente a ella. Un dibujo que sobresalga de la caja engaña al jugador; uno más pequeño deja aire que hace daño.

Medidas en píxeles del original. El arte se entrega a **8 texels por píxel**: un enemigo de 16 × 16 px se dibuja en 128 × 128 texels. Las cajas del original incluyen sus bordes; aquí van ya como píxeles cubiertos (ancho × alto).

## Jugador

| Caja | Desde los pies (x centro, y pies) | Píxeles | Texels |
|---|---|---|---|
| Contacto con enemigos y objetos | x -6 … x +6, y -14 … y -2 | 13 × 13 | 104 × 104 |
| Espada (a la derecha; a la izquierda, simétrica) | x +8 … x +18, y -7 … y -4 | 11 × 4 | 88 × 32 |
| Sprite del original | 16 × 16 con los pies en el borde de abajo | 16 × 16 | 128 × 128 |

- La caja del jugador no cambia con la postura (escalera, salto, espadazo). El dibujo puede ser más alto que la caja (yelmo, cresta) siempre que el cuerpo que «se toca» quede dentro de los 13 × 13 px.
- La hoja de la espada llega a x + 18 en el tick del golpe. La estocada golpea en el **primer** tick del espadazo: el fotograma «golpe» es el primero que se ve; la anticipación no cabe antes.
- Armas secundarias (cajas del original): flecha y flecha de cerámica 13 × 4 (en la rejilla de 8 px), fuego rodante 9 × 9, fuego 9 × 5, mina 13 × 5 y su explosión 33 × 33.

## Enemigos de las salas

53 tipos colocados en las salas (docs/enemy_ai.md, grupos G1–G4). Daño en unidades de vitalidad (la barra llena son 8).

| Tipo | Qué es | Familia visual | Vida | Daño | Caja inicial | Cajas (ancho×alto px) | Cambia | Lienzo mayor (texels) | Notas |
|---|---|---|---|---|---|---|---|---|---|
| 0x01 | bicho embestidor | escarabajo andante (cuernos) | 4 | 8 | 17 × 17 | 17×17 17×30 17×16 | sí | 136 × 240 | patrulla 00<->13 cada 8 f (sprite ^4 a la derecha, ^0C a la izquierda); embestida AC/C0 -> 01 (29 de alto, sube 13 px); A8 -> 00 |
| 0x02 | chispa que recorre superficies | gusano o babosa | 1 | 5 | 13 × 13 | 13×13 | no | 104 × 104 | caja fija |
| 0x03 | gota/babosa de techo | gusano o babosa | 1 | 1 | 15 × 7 | 15×7 13×11 11×10 | sí | 120 × 88 | quieta C4 -> 66 (6x14); avanza C8 -> 74, CC/D0 -> 77 |
| 0x04 | columna/pistón de tiles | trampa de piedra (pistón) | 1 | 2 | 9 × 8 | 9×8 | no | 72 × 64 | caja fija |
| 0x05 | serpiente que escupe | gusano o babosa | 3 | 7 | 16 × 17 | 17×17 10×6 16×17 | no | 136 × 136 | gancho B080: caja según sprite cada frame |
| 0x07 | oruga saltarina | escarabajo andante | 2 | 3 | 12 × 7 | 12×7 12×9 | no | 96 × 72 | anda 26<->27 cada 10 f (sprite ^4, caja ^1); en el salto conserva la caja |
| 0x0A | caballero | escarabajo andante (escudo) | 6 | 3 | 25 × 16 | 25×16 15×17 | sí | 200 × 136 | anda a la derecha B0 -> 4E y a la izquierda C0 -> 4F (24 de ancho); escalera A0/A8 -> 55 |
| 0x0B | pez/dardo rápido | dardo | 1 | 10 | 16 × 8 | 8×8 15×7 16×8 | sí | 128 × 64 | nada A0/3F <-> A4/05 (^3A cada 8 f); A8/05 <-> AC/3F; al girar A0 -> 3E (y A4 -> 04) |
| 0x0C | hombre-llama | llama o chorro | 3 | 7 | 7 × 9 | 16×17 7×9 13×17 10×14 9×11 | sí | 128 × 136 | gancho B067: crece A0 -> 51 (8x6), A4/A8 -> 52, AC -> 53, B0 -> 54; vuelo B4..C0 -> 2A/16 |
| 0x0E | murciélago | alado | 1 | 1 | 17 × 10 | 17×10 11×12 | sí | 136 × 96 | colgado A0 -> 67; se suelta AC -> 69; revolotea AC/69 <-> B0/0F (^66 cada 4-6 f) |
| 0x0F | bola que crece | bola | 6 | 5 | 5 × 5 | 17×17 5×5 9×9 13×13 | sí | 136 × 136 | crece por etapas (IX+1E 0..3): derecha C0/33, C8/35, D0/36, D8/00; izquierda A0/32, A8/34, B0/36, B8/00; anima solo el sprite |
| 0x10 | acechador que embiste y vuelve | trampa de piedra | 1 | 2 | 17 × 12 | 17×12 | no | 136 × 96 | caja fija |
| 0x11 | erizo que dispara púas | bola (pinchos) | 2 | 2 | 17 × 11 | 17×17 17×11 15×11 | sí | 136 × 136 | anda A4/B4 -> 58; descansa B0 -> 59 / C0 -> 5A alternando; dispara AC/BC -> 00 |
| 0x12 | armadillo / bola enrollada | bola (caparazón) | 5 | 4 | 17 × 16 | 5×5 17×16 16×17 | no | 136 × 136 | gancho B07B: emerge y rueda con cajas de 16 (13/16/2A); oculto con IX+2 bit 2 |
| 0x13 | espectro caminante que se desvanece y vuela | llama o chorro | 4 | 6 | 15 × 10 | 17×17 15×15 12×17 16×15 15×10 13×8 16×9 8×7 15×16 | sí | 136 × 136 | gancho B071: anda A0..B0 (2D/2E/1A/2F), se desvanece B4 -> 30 (6x7), vuela BC/C0 -> 15/31 |
| 0x14 | criatura que emerge del suelo, pasea y se hunde | trampa de piedra | 4 | 6 | 11 × 5 | 17×16 16×17 17×9 17×5 11×5 | sí | 136 × 136 | gancho B076: emerge A0 -> 38 (4 alto), A4 -> 1E (4), A8 -> 19 (8); pasea AC..B8 -> 16/13/2A/13 |
| 0x15 | guardián volador que orbita alrededor de un centro móvil | proyectil (orbe) | 40 | 10 | 33 × 26 | 33×26 33×31 | no | 264 × 248 | sprites 08/09 alternando cada 8 f -> 42/43 |
| 0x16 | aparición que embiste en línea recta y se teletransporta | espíritu | 24 | 8 | 17 × 30 | 17×30 | no | 136 × 240 | caja fija |
| 0x17 | centinela que patrulla y dispara al ver al jugador en su nivel | trampa de piedra | 5 | 4 | 17 × 16 | 17×15 17×16 | no | 136 × 128 | 13 <-> 09 con la animación (^1A) |
| 0x18 | rodador lento que solo gira en ciertos fotogramas | bola (pinchos) | 1 | 2 | 17 × 5 | 17×12 17×5 17×8 15×13 | sí | 136 × 104 | tabla 6CC4 por IX+1F: A0/1E (4 alto), A8/0B, AC/0B, A4/1F (7), B4/25 (12x14), B0/0B |
| 0x19 | murciélagos que se fusionan por parejas | alado | 1 | 1 | 6 × 6 | 17×13 11×14 11×8 9×10 6×6 4×6 | sí | 136 × 112 | nivel 0 D0/4A <-> 4B (5x5); nivel 1 C8/48 <-> 49; nivel 2 C0/46 <-> 47 (^1) |
| 0x1F | corredor saltarín | escarabajo andante | 3 | 4 | 17 × 17 | 17×17 17×16 | no | 136 × 136 | corre 00 <-> 13 (^13); salto A8/B4 -> 00 |
| 0x20 | embestidor invulnerable lento | escarabajo andante (cuernos) | indestructible | 3 | 17 × 9 | 17×9 | no | 136 × 72 | caja fija |
| 0x22 | escupefuego volador errante | llama o chorro | 24 | 8 | 17 × 15 | 17×15 | no | 136 × 120 | caja fija |
| 0x23 | estatua o bloque dormido que se activa al rodearlo | trampa de piedra | 64 | 0 | 15 × 25 | 15×25 | no | 120 × 200 | caja fija |
| 0x24 | saltador entre niveles que persigue al jugador | escarabajo andante | 3 | 3 | 17 × 17 | 17×17 17×15 | no | 136 × 136 | anda 00; tras saltar A8/B4 -> 09; al aterrizar 00 |
| 0x25 | armadillo o caracol que se enrosca al recibir golpes | escarabajo andante (esqueleto, escudo) | 8 | 3 | 25 × 16 | 17×12 17×9 17×15 25×16 | sí | 200 × 128 | anda AC -> 4E / B8 -> 4F (24 de ancho); enroscado tabla 9A61 por IX+19: 2 A0/4D, 1 A4/0B, 0 A8/19 |
| 0x26 | fantasma orbitante que dispara | espíritu | 10 | 3 | 14 × 17 | 14×17 | no | 112 × 136 | caja fija |
| 0x27 | ave/murciélago errante que se lanza en picado | alado | 12 | 8 | 11 × 5 | 17×17 11×5 | sí | 136 × 136 | dormida C4 -> 11 (4x10); despierta C8/CC -> 00 |
| 0x28 | insecto rebotador de 16 direcciones | alado | 1 | 1 | 9 × 17 | 15×15 9×17 17×9 | no | 136 × 136 | tabla 93CC (base C4) según la dirección: 62, 63, 0D, 64, 0C, 65 |
| 0x29 | burbuja invulnerable que rebota en línea recta | proyectil (orbe) | indestructible | 5 | 17 × 12 | 17×12 15×10 | no | 136 × 96 | 0E <-> 68 cada 8 f (^66) |
| 0x2A | aparición que se teletransporta entre plataformas | hechicero | 3 | 1 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x2B | trepamuros que recorre el contorno de las superficies | gusano o babosa | 3 | 10 | 17 × 16 | 17×16 | no | 136 × 128 | caja fija |
| 0x2C | caminante que se para hasta que desaparece su compañero | gusano o babosa | 4 | 2 | 17 × 12 | 17×12 15×15 | no | 136 × 120 | gancho B06C: A0/AC -> 0B, A4/A8 -> 0C, B0/B4 -> 0D |
| 0x2D | soldado que patrulla, salta huecos y dispara al jugador en su fila | escarabajo andante (lanza) | 2 | 3 | 15 × 15 | 17×17 15×15 | no | 136 × 136 | derecha A0/0C <-> A4/00, izquierda AC/0D <-> B0/00 (^0C/^0D cada 4 f) |
| 0x2E | brote/llama intermitente que crece y se esconde | brote | 2 | 2 | 12 × 7 | 12×7 12×9 12×13 12×16 | sí | 96 × 128 | tabla A0D1 por IX+1E: C4/26 (6 alto), C8/27 (8), CC/28 (12), D0/29 y D4/29 (15) y vuelta |
| 0x2F | bloque que imita los saltos del jugador | escarabajo andante | 5 | 1 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x31 | pinchos/estalagmita que emergen del suelo o del techo | pinchos | indestructible | 2 | 9 × 8 | 9×8 8×8 11×11 13×11 13×13 11×10 | sí | 104 × 104 | tabla 8C17 (base C4). Suelo: C8/73, CC/74, D0/75; techo: D8/77, DC/74, E0/75. C4/D4 no dan contacto (IX+2 sin bit 6); sprite 00 oculto |
| 0x32 | estatua/torreta que dispara proyectiles | trampa de piedra (flechas) | indestructible | 0 | 17 × 25 | 17×25 | no | 136 × 200 | caja fija |
| 0x34 | bloque/cabeza que sube y baja más rápido cuanto más cerca está el jugador | trampa de piedra | 4 | 6 | 9 × 8 | 9×8 | no | 72 × 64 | caja fija |
| 0x36 | orbes en cadena que saltan de un sitio a otro | bola (pinchos) | indestructible | 6 | 14 × 17 | 11×5 14×17 | sí | 112 × 136 | 12 <-> 11 cada 12 f (^3) |
| 0x3A | boca/cañón de pared que dispara ráfagas a la derecha | trampa de piedra (boca) | indestructible | 0 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x3C | hechicero que se teletransporta y dispara | hechicero | 7 | 5 | 17 × 16 | 17×16 | no | 136 × 128 | caja fija |
| 0x40 | espíritu grande que persigue lentamente y lanza proyectiles | espíritu | 35 | 12 | 27 × 32 | 27×32 | no | 216 × 256 | caja fija |
| 0x42 | monstruo del agua que emerge y dispara | pez | 10 | 20 | 23 × 16 | 23×16 | no | 184 × 128 | caja fija |
| 0x43 | criatura ancha que patrulla y suelta un rayo vertical | escarabajo andante (ancho, cuernos) | 10 | 10 | 33 × 15 | 33×15 | no | 264 × 120 | caja fija |
| 0x49 | insecto/mosca que se divide al ser golpeado | alado | 5 | 8 | 29 × 15 | 29×15 23×15 | sí | 232 × 120 | 6A <-> 6B (^1) |
| 0x4C | nube/criatura errante que suelta esporas cada 16 frames | proyectil (orbe) | 20 | 7 | 21 × 21 | 21×21 | no | 168 × 168 | caja fija |
| 0x4E | almas/fuegos fatuos que se reúnen en el centro | espíritu | 0 | 0 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x50 | boca/cañón de pared que dispara ráfagas a la izquierda | trampa de piedra (boca) | indestructible | 0 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x51 | trampa de flechas en la pared, dispara a la izquierda | trampa de piedra (flechas) | indestructible | 0 | 17 × 25 | 17×25 | no | 136 × 200 | caja fija |
| 0x64 | segmento trasero de oruga | gusano o babosa | 4 | 2 | 17 × 17 | 17×17 17×12 15×15 | no | 136 × 136 |  |
| 0x69 | cabezas flotantes en formación que disparan por turnos | espíritu (cabeza) | indestructible | 7 | 17 × 12 | 17×12 | no | 136 × 96 | caja fija |

## Generados periódicamente

10 tipos que salen de los generadores de las salas (bolas de fuego de la lava, peces, bolas rodantes…).

| Tipo | Qué es | Familia visual | Vida | Daño | Caja inicial | Cajas (ancho×alto px) | Cambia | Lienzo mayor (texels) | Notas |
|---|---|---|---|---|---|---|---|---|---|
| 0x08 | bola de fuego | llama o chorro (agua) | indestructible | 4 | 17 × 15 | 10×10 15×15 17×15 | no | 136 × 120 | gancho B085 (base C4): CC -> 0C, D0 -> 50 |
| 0x09 | bola que rebota | bola | indestructible | 3 | 17 × 16 | 17×16 | no | 136 × 128 | caja fija |
| 0x0D | pez saltador | pez | 1 | 1 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x1B | fragmento de la bola de fuego | llama o chorro (agua) | 1 | 1 | 15 × 7 | 15×7 | no | 120 × 56 | caja fija |
| 0x21 | caminante que cae | escarabajo andante | 2 | 2 | 17 × 11 | 17×11 15×11 | no | 136 × 88 | 58 <-> 59 a la derecha, 58 <-> 5A a la izquierda (^1 / ^2) |
| 0x30 | saltador de lava | escarabajo andante | 5 | 5 | 33 × 15 | 33×15 | no | 264 × 120 | caja fija |
| 0x35 | bola rodante | bola (piedra) | indestructible | 3 | 17 × 16 | 17×16 | no | 136 × 128 | caja fija |
| 0x38 | bola que sube al techo | gusano o babosa | 1 | 1 | 15 × 7 | 15×7 11×12 | sí | 120 × 96 | quieta C4 -> 66; cae C8 -> 67 |
| 0x3E | volador en zigzag | alado | 2 | 3 | 17 × 10 | 17×12 17×10 | no | 136 × 96 | 0F <-> 0E (^1) |
| 0x6A | salpicadura | llama o chorro (agua) | indestructible | 0 | 17 × 12 | 17×12 | no | 136 × 96 | caja fija |

## Hijos: proyectiles, botín y otros

44 tipos que crean otros enemigos (proyectiles, llamaradas, esporas…), el botín, el hada, la explosión y el bebé.

| Tipo | Qué es | Familia visual | Vida | Daño | Caja inicial | Cajas (ancho×alto px) | Cambia | Lienzo mayor (texels) | Notas |
|---|---|---|---|---|---|---|---|---|---|
| 0x06 | rayo |  | indestructible | 18 | 8 × 17 | 8×17 | no | 64 × 136 | caja calculada (lowerSegment). caja 10 con origen en IX+12/IX+13 (segmento inferior) salvo en el estado 4 |
| 0x1A | escupitajo de la serpiente 0x05 |  | 1 | 3 | 15 × 7 | 15×7 | no | 120 × 56 | caja fija |
| 0x1C | — |  | 1 | 2 | 9 × 8 | 9×8 | no | 72 × 64 | caja fija |
| 0x1D | púa del erizo 0x11 |  | indestructible | 1 | 15 × 7 | 15×7 | no | 120 × 56 | caja fija |
| 0x1E | bola del armadillo 0x12 |  | indestructible | 1 | 5 × 5 | 5×5 | no | 40 × 40 | caja fija |
| 0x33 | flecha de las torretas 0x32 y 0x51 |  | 1 | 1 | 15 × 4 | 15×4 | no | 120 × 32 | derecha C4 -> 5F, izquierda C8 -> 60 |
| 0x37 | murciélago grande | alado | 8 | 0 | 21 × 21 | 23×16 21×21 | no | 184 × 168 | 3D <-> 3C (^1) |
| 0x39 | — |  | 40 | 14 | 15 × 14 | 15×14 | no | 120 × 112 | caja fija |
| 0x3B | bola de las bocas de pared 0x3A y 0x50 |  | 1 | 2 | 13 × 13 | 13×13 | no | 104 × 104 | caja fija |
| 0x3D | bola del hechicero 0x3C |  | 1 | 2 | 9 × 9 | 9×9 | no | 72 × 72 | caja fija |
| 0x3F | disparo paralizante del soldado 0x2D |  | indestructible | 0 | 15 × 7 | 15×7 13×9 | no | 120 × 72 | A8 -> 05; con E624 B4 -> 21 |
| 0x41 | disparo en 8 direcciones del espíritu 0x40 |  | indestructible | 4 | 17 × 17 | 17×17 4×17 17×4 13×13 | sí | 136 × 136 | tabla 8DC5 por dirección: horizontal A4 -> 3A, vertical A8 -> 3B, diagonal C8/BC -> 75 |
| 0x44 | bola del espectro 0x13 |  | indestructible | 2 | 16 × 15 | 16×15 | no | 128 × 120 | caja fija |
| 0x45 | bola del guardián orbital 0x15 |  | indestructible | 4 | 7 × 7 | 7×7 | no | 56 × 56 | caja fija |
| 0x46 | rastro de la aparición 0x16 |  | indestructible | 2 | 7 × 7 | 7×7 | no | 56 × 56 | caja fija |
| 0x47 | bomba rodante del centinela 0x17 |  | indestructible | 2 | 9 × 11 | 21×21 33×33 9×11 11×9 | sí | 264 × 264 | proyectil C0/5C <-> C4/5D; explosión 0A -> 3D (20x20, x-8 y-8) y 0B -> 5B (32x32) |
| 0x48 | murcielaguito que sueltan 0x19 y 0x37 |  | indestructible | 1 | 6 × 6 | 6×6 | no | 48 × 48 | caja fija |
| 0x4A | llamarada del escupefuego 0x22 |  | indestructible | 2 | 5 × 5 | 5×5 | no | 40 × 40 | caja fija |
| 0x4B | bola del fantasma orbitante 0x26 |  | indestructible | 1 | 5 × 5 | 5×5 | no | 40 × 40 | caja fija |
| 0x4D | espora ondulante de 0x4C |  | indestructible | 1 | 9 × 11 | 9×11 | no | 72 × 88 | caja fija |
| 0x4F | hada |  | 0 | 0 | 17 × 17 | 17×17 17×16 | no | 136 × 136 | A4 -> 00; A8 -> 13 |
| 0x52 | moscas divididas | alado | 3 | 2 | 17 × 10 | 17×10 | no | 136 × 80 | 69 <-> 0F (^66) |
| 0x53 | moscas divididas | alado | 1 | 1 | 10 × 6 | 11×8 10×6 | no | 88 × 64 | 56 <-> 48 (^1E) |
| 0x54 | — |  | 5 | 1 | 17 × 17 | 17×17 17×12 17×5 17×8 15×13 | sí | 136 × 136 | como 0x18 (tabla 6CC4) |
| 0x55 | — |  | 1 | 1 | 17 × 17 | 17×17 17×12 17×5 17×8 15×13 | sí | 136 × 136 | como 0x18 (tabla 6CC4) |
| 0x56 | bola del espectro 0x13 |  | 1 | 2 | 7 × 9 | 7×9 | no | 56 × 72 | caja fija |
| 0x57 | — |  | indestructible | 6 | 17 × 17 | 17×17 | sí | 136 × 136 | caja calculada (row). sin tabla: dy 0, dx 0, alto 8, ancho 8*IX+1F (b03:B926) |
| 0x58 | — |  | 1 | 4 | 9 × 9 | 15×15 9×9 | sí | 120 × 120 | 6F (8x8) <-> 0D (14x14) (^62) |
| 0x59 | — |  | 1 | 1 | 7 × 7 | 7×7 | no | 56 × 56 | caja fija |
| 0x5A | — |  | 1 | 0 | 11 × 11 | 11×11 | no | 88 × 88 | caja fija |
| 0x5B | explosión de la muerte (efecto) | explosión | indestructible | 0 | 17 × 17 | 17×17 | no | 136 × 136 | caja calculada (none). explosión: IX+11 guarda el tipo muerto, no una caja; IX+2 = 80, sin contacto ni golpes |
| 0x5C | — |  | 5 | 0 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x5D | botín: flechas | explosión | 0 | 0 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x5E | botín: monedas | explosión | 0 | 0 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x5F | botín: llaves | explosión | 0 | 0 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x60 | chorro de agua del monstruo 0x42 |  | 4 | 2 | 17 × 17 | 17×17 | sí | 136 × 136 | caja calculada (columnDown). sin tabla: dy 0, dx 0, alto 8*((IX+9 & 7) - 1), ancho 7 (b03:B916) |
| 0x61 | bola del monstruo del agua 0x42 |  | indestructible | 2 | 5 × 5 | 5×5 | no | 40 × 40 | caja fija |
| 0x62 | rayo vertical del patrullero 0x43 |  | 4 | 8 | 17 × 17 | 17×17 | sí | 136 × 136 | caja calculada (columnUpDown). sin tabla: dx 0, alto 8*n, ancho 7 con n = IX+9 & 7; dy = -8*n si IX+15 bit 0, si no 0 (b03:B933) |
| 0x63 | — |  | indestructible | 0 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x65 | — |  | 1 | 4 | 11 × 17 | 11×17 | no | 88 × 136 | caja fija |
| 0x66 | — |  | indestructible | 8 | 9 × 9 | 9×9 | no | 72 × 72 | caja fija |
| 0x67 | — |  | 16 | 12 | 15 × 17 | 15×17 | no | 120 × 136 | caja fija |
| 0x68 | — |  | 1 | 10 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |
| 0x6B | bebé (final) |  | 0 | 0 | 17 × 17 | 17×17 | no | 136 × 136 | caja fija |

## Jefes

Tamaño del bloque de tiles de cada pose (lo que se dibuja) y sus dos cajas: la que **recibe las armas** y la que **hace daño al tocarla**, ambas desde la esquina superior izquierda del bloque. Los jefes se dibujan a tamaño del bloque: el lienzo es tiles × 8 px × 8 texels. El original cambia de pose por fotograma; aquí cada pose es una fila.

### Mundo 1: Bony Dragon (palabra YOMAR, vida 24, daño al tocarlo 8)


| Pose (sprite) | Parte | Bloque (tiles) | Dibujo (px) | Lienzo (texels) | Caja de armas (x, y, ancho×alto) | Caja de contacto (x, y, ancho×alto) |
|---|---|---|---|---|---|---|
| 0x0C | body | 10 × 6 | 80 × 48 | 640 × 384 | (4, 12) 17×33 | (4, 4) 73×41 |
| 0x0D | body | 10 × 6 | 80 × 48 | 640 × 384 | (4, 4) 17×33 | (4, 4) 73×41 |
| 0x0E | body | 10 × 7 | 80 × 56 | 640 × 448 | (4, 4) 17×33 | (4, 12) 73×41 |
| 0x0F | body | 10 × 6 | 80 × 48 | 640 × 384 | (4, 4) 17×33 | (4, 4) 73×41 |

### Mundo 2: Sea Anemone (palabra ELOHIM, vida 40, daño al tocarlo 12)


| Pose (sprite) | Parte | Bloque (tiles) | Dibujo (px) | Lienzo (texels) | Caja de armas (x, y, ancho×alto) | Caja de contacto (x, y, ancho×alto) |
|---|---|---|---|---|---|---|
| 0x10 | body | 6 × 6 | 48 × 48 | 384 × 384 | — | (8, 16) 33×33 |
| 0x11 | body | 6 × 6 | 48 × 48 | 384 × 384 | — | (8, 8) 33×41 |
| 0x12 | body | 6 × 6 | 48 × 48 | 384 × 384 | (16, 0) 17×9 | (8, 8) 33×41 |
| 0x13 | body | 6 × 6 | 48 × 48 | 384 × 384 | (16, 0) 17×9 | (8, 8) 33×41 |

### Mundo 3: Elusive Dragon (palabra HAHAKLA, vida 64, daño al tocarlo 12)


| Pose (sprite) | Parte | Bloque (tiles) | Dibujo (px) | Lienzo (texels) | Caja de armas (x, y, ancho×alto) | Caja de contacto (x, y, ancho×alto) |
|---|---|---|---|---|---|---|
| 0x06 | body | 16 × 3 | 128 × 24 | 1024 × 192 | — | — |
| 0x07 | body | 16 × 3 | 128 × 24 | 1024 × 192 | — | — |
| 0x08 | body | 16 × 3 | 128 × 24 | 1024 × 192 | — | — |
| 0x09 | body | 16 × 3 | 128 × 24 | 1024 × 192 | — | — |
| 0x0A | body | 16 × 3 | 128 × 24 | 1024 × 192 | — | — |
| 0x0B | body | 16 × 3 | 128 × 24 | 1024 × 192 | — | — |

### Mundo 4: Jump Slime (palabra BARECHET, vida 112, daño al tocarlo 16)


| Pose (sprite) | Parte | Bloque (tiles) | Dibujo (px) | Lienzo (texels) | Caja de armas (x, y, ancho×alto) | Caja de contacto (x, y, ancho×alto) |
|---|---|---|---|---|---|---|
| 0x2B | body | 10 × 5 | 80 × 40 | 640 × 320 | (16, 32) 49×9 | (8, 16) 65×9 · (0, 24) 81×17 |
| 0x2C | body | 10 × 5 | 80 × 40 | 640 × 320 | (24, 32) 33×9 | (20, 12) 41×13 · (8, 24) 65×17 |
| 0x2D | body | 10 × 5 | 80 × 40 | 640 × 320 | (24, 32) 25×9 | (32, 0) 33×17 · (12, 16) 49×25 |
| 0x2E | body | 10 × 5 | 80 × 40 | 640 × 320 | (32, 32) 25×9 | (16, 0) 33×17 · (20, 16) 49×25 |
| 0x2F | body | 10 × 5 | 80 × 40 | 640 × 320 | (40, 24) 13×17 | (40, 0) 41×25 · (20, 8) 33×33 |
| 0x30 | body | 10 × 5 | 80 × 40 | 640 × 320 | (28, 24) 13×17 | (0, 0) 41×25 · (28, 8) 33×33 |

### Mundo 5: Donra (palabra HEOTYMEO, vida 72, daño al tocarlo 16)


| Pose (sprite) | Parte | Bloque (tiles) | Dibujo (px) | Lienzo (texels) | Caja de armas (x, y, ancho×alto) | Caja de contacto (x, y, ancho×alto) |
|---|---|---|---|---|---|---|
| 0x1C | body | 10 × 4 | 80 × 32 | 640 × 256 | — | (24, 4) 85×25 |
| 0x1D | body | 10 × 4 | 80 × 32 | 640 × 256 | — | (24, 4) 85×25 |
| 0x1E | head | 2 × 4 | 16 × 32 | 128 × 256 | (0, 8) 25×9 | (0, 8) 25×9 |
| 0x1F | head | 2 × 4 | 16 × 32 | 128 × 256 | (0, 16) 25×9 | (0, 16) 25×9 |
| 0x20 | head | 2 × 4 | 16 × 32 | 128 × 256 | (0, 24) 25×9 | (0, 24) 25×9 |
| 0x21 | head | 2 × 4 | 16 × 32 | 128 × 256 | (0, 32) 25×9 | (0, 32) 25×9 |
| 0x22 | mouth | 2 × 1 | 16 × 8 | 128 × 64 | — | — |
| 0x23 | mouth | 2 × 1 | 16 × 8 | 128 × 64 | — | — |

### Mundo 6: Arlyane (palabra LEPHA, vida 56, daño al tocarlo 20)


| Pose (sprite) | Parte | Bloque (tiles) | Dibujo (px) | Lienzo (texels) | Caja de armas (x, y, ancho×alto) | Caja de contacto (x, y, ancho×alto) |
|---|---|---|---|---|---|---|
| 0x3A | body | 10 × 5 | 80 × 40 | 640 × 320 | (32, 12) 17×13 | (4, 4) 73×33 |
| 0x3B | body | 10 × 5 | 80 × 40 | 640 × 320 | (32, 12) 17×13 | (4, 4) 73×33 |
| 0x3C | body | 10 × 5 | 80 × 40 | 640 × 320 | (32, 12) 17×13 | (4, 4) 73×33 |

### Mundo 7: Big Lip (palabra NAWABRA, vida 64, daño al tocarlo 24)


| Pose (sprite) | Parte | Bloque (tiles) | Dibujo (px) | Lienzo (texels) | Caja de armas (x, y, ancho×alto) | Caja de contacto (x, y, ancho×alto) |
|---|---|---|---|---|---|---|
| 0x18 | body | 8 × 8 | 64 × 64 | 512 × 512 | — | (12, 20) 41×25 |
| 0x19 | body | 8 × 8 | 64 × 64 | 512 × 512 | (24, 32) 17×17 | (12, 4) 41×57 |
| 0x1A | body | 8 × 8 | 64 × 64 | 512 × 512 | (24, 32) 17×17 | (12, 16) 41×37 |
| 0x1B | body | 8 × 8 | 64 × 64 | 512 × 512 | (24, 32) 17×25 | (12, 12) 41×41 |

### Mundo 8: Monster (palabra ASCHER, vida 80, daño al tocarlo 32)


| Pose (sprite) | Parte | Bloque (tiles) | Dibujo (px) | Lienzo (texels) | Caja de armas (x, y, ancho×alto) | Caja de contacto (x, y, ancho×alto) |
|---|---|---|---|---|---|---|
| 0x24 | body | 6 × 3 | 48 × 24 | 384 × 192 | (4, 4) 41×21 | (4, 4) 41×21 |
| 0x25 | body | 6 × 3 | 48 × 24 | 384 × 192 | (4, 4) 41×21 | (4, 4) 41×21 |
| 0x26 | body | 6 × 3 | 48 × 24 | 384 × 192 | (4, 4) 41×21 | (4, 4) 41×21 |
| 0x27 | body | 6 × 3 | 48 × 24 | 384 × 192 | (4, 4) 41×21 | (4, 4) 41×21 |
| 0x28 | body | 6 × 3 | 48 × 24 | 384 × 192 | (4, 4) 41×21 | (4, 4) 41×21 |
| 0x29 | body | 6 × 3 | 48 × 24 | 384 × 192 | (4, 4) 41×21 | (4, 4) 41×21 |
| 0x2A | body | 4 × 4 | 32 × 32 | 256 × 256 | (4, 4) 25×29 | (4, 4) 25×29 |

### Mundo 9: King Dragon (palabra XYWOLEH, vida 80, daño al tocarlo 36)

- tres cajas de armas por pose (vale cualquiera).

| Pose (sprite) | Parte | Bloque (tiles) | Dibujo (px) | Lienzo (texels) | Caja de armas (x, y, ancho×alto) | Caja de contacto (x, y, ancho×alto) |
|---|---|---|---|---|---|---|
| 0x34 | body | 15 × 9 | 120 × 72 | 960 × 576 | (16, 16) 25×17 · (40, 0) 25×17 · (64, 8) 25×17 | (40, 0) 61×73 · (16, 16) 25×17 |
| 0x35 | body | 15 × 9 | 120 × 72 | 960 × 576 | (8, 16) 25×17 · (24, 32) 25×17 · (64, 0) 25×17 | (44, 4) 57×65 · (8, 16) 37×25 |
| 0x36 | body | 15 × 9 | 120 × 72 | 960 × 576 | (0, 48) 25×17 · (16, 8) 25×17 · (56, 0) 25×17 | (20, 4) 73×65 · (0, 48) 121×17 |

### Mundo 10: Galious (palabra HAMALECH, vida 248, daño al tocarlo 40)


| Pose (sprite) | Parte | Bloque (tiles) | Dibujo (px) | Lienzo (texels) | Caja de armas (x, y, ancho×alto) | Caja de contacto (x, y, ancho×alto) |
|---|---|---|---|---|---|---|
| 0x37 | body | 7 × 7 | 56 × 56 | 448 × 448 | (24, 40) 9×9 | (8, 0) 41×57 |
| 0x38 | body | 7 × 7 | 56 × 56 | 448 × 448 | (24, 40) 9×9 | (8, 0) 41×57 |

## Tabla de cajas del original (b14:9006)

Índice → (dx, dy) desde la posición del enemigo y ancho × alto cubiertos. Es la que citan las notas de arriba.

| i | dx, dy | px | i | dx, dy | px | i | dx, dy | px | i | dx, dy | px |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 00 | 0, 0 | 17×17 | 01 | 0, -13 | 17×30 | 02 | 7, 0 | 10×6 | 03 | 0, 0 | 11×11 |
| 04 | 1, 1 | 8×8 | 05 | 0, 0 | 15×7 | 06 | 0, 0 | 10×9 | 07 | 0, 0 | 10×10 |
| 08 | 7, 0 | 10×10 | 09 | 0, 0 | 17×15 | 0A | 0, 0 | 5×5 | 0B | 0, 5 | 17×12 |
| 0C | 2, 2 | 15×15 | 0D | 0, 2 | 15×15 | 0E | 0, 0 | 17×12 | 0F | 0, 0 | 17×10 |
| 10 | 0, 0 | 8×17 | 11 | 3, 0 | 11×5 | 12 | 1, 0 | 14×17 | 13 | 0, 1 | 17×16 |
| 14 | 0, 0 | 9×9 | 15 | 2, 0 | 12×17 | 16 | 1, 0 | 16×17 | 17 | 0, 0 | 12×17 |
| 18 | 3, 0 | 14×17 | 19 | 0, 8 | 17×9 | 1A | 0, 0 | 16×15 | 1B | 0, 9 | 9×8 |
| 1C | 0, 6 | 11×11 | 1D | 0, 3 | 14×14 | 1E | 0, 12 | 17×5 | 1F | 0, 0 | 17×8 |
| 20 | 4, 0 | 13×9 | 21 | 0, 0 | 13×9 | 22 | 0, 0 | 9×8 | 23 | 0, 0 | 14×14 |
| 24 | 2, 0 | 15×13 | 25 | 0, 0 | 15×13 | 26 | 2, 10 | 12×7 | 27 | 2, 8 | 12×9 |
| 28 | 2, 4 | 12×13 | 29 | 2, 1 | 12×16 | 2A | 0, 0 | 16×17 | 2B | 0, 10 | 17×7 |
| 2C | 0, 0 | 7×6 | 2D | 0, 0 | 15×10 | 2E | 0, 0 | 13×8 | 2F | 0, 0 | 16×9 |
| 30 | 0, 0 | 8×7 | 31 | 2, 0 | 15×16 | 32 | 0, 12 | 5×5 | 33 | 12, 12 | 5×5 |
| 34 | 0, 8 | 9×9 | 35 | 8, 8 | 9×9 | 36 | 0, 4 | 13×13 | 37 | 4, 4 | 13×13 |
| 38 | 3, 12 | 11×5 | 39 | 3, 0 | 27×32 | 3A | 0, 0 | 4×17 | 3B | 0, 0 | 17×4 |
| 3C | 5, 1 | 23×16 | 3D | 6, 6 | 21×21 | 3E | 1, 2 | 16×8 | 3F | 0, 2 | 16×8 |
| 40 | 2, 0 | 15×7 | 41 | 0, 2 | 33×15 | 42 | 0, 6 | 33×26 | 43 | 0, 2 | 33×31 |
| 44 | 0, 0 | 7×7 | 45 | 0, 5 | 33×23 | 46 | 0, 2 | 17×13 | 47 | 3, 1 | 11×14 |
| 48 | 0, 1 | 11×8 | 49 | 1, 0 | 9×10 | 4A | 0, 0 | 6×6 | 4B | 1, 0 | 4×6 |
| 4C | 0, 0 | 15×14 | 4D | 0, 2 | 17×15 | 4E | 0, 1 | 25×16 | 4F | -8, 1 | 25×16 |
| 50 | 1, 0 | 15×15 | 51 | 0, 0 | 7×9 | 52 | 2, 0 | 13×17 | 53 | 0, 0 | 10×14 |
| 54 | 0, 0 | 9×11 | 55 | 1, 0 | 15×17 | 56 | 0, 0 | 10×6 | 57 | 2, 0 | 14×8 |
| 58 | 0, 6 | 17×11 | 59 | 2, 6 | 15×11 | 5A | 0, 6 | 15×11 | 5B | 0, 0 | 33×33 |
| 5C | 0, 6 | 9×11 | 5D | 0, 8 | 11×9 | 5E | 0, 0 | 6×11 | 5F | 2, 0 | 15×4 |
| 60 | 0, 0 | 15×4 | 61 | 0, 0 | 13×13 | 62 | 4, 0 | 9×17 | 63 | 0, 4 | 17×9 |
| 64 | 0, 0 | 15×15 | 65 | 4, 0 | 15×15 | 66 | 1, 0 | 15×7 | 67 | 3, 0 | 11×12 |
| 68 | 1, 0 | 15×10 | 69 | 0, 1 | 17×10 | 6A | 2, 0 | 29×15 | 6B | 5, 2 | 23×15 |
| 6C | -2, 0 | 15×25 | 6D | 2, 3 | 13×13 | 6E | 2, 3 | 11×11 | 6F | 4, 4 | 9×9 |
| 70 | 5, 5 | 7×7 | 71 | 3, 0 | 11×17 | 72 | 4, 9 | 8×8 | 73 | 3, 6 | 11×11 |
| 74 | 2, 4 | 13×11 | 75 | 2, 2 | 13×13 | 76 | 4, 0 | 8×8 | 77 | 3, 0 | 11×10 |
| 78 | 6, 6 | 6×6 | 79 | 0, 0 | 17×25 |  |  |  |  |  |  |

