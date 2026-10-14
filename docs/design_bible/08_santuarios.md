# 8. Santuarios y economía

Fuentes: [entities.md](../entities.md) («Santuarios y tiendas»), [state_machine.md](../state_machine.md) (estados 14 y 16). Datos para Unity: `extracted/unity/shrines.json` (`tools/extract/shrines.py`, que documenta cada campo). Capturas: `logs/omsx/shrines/`.

## Entrar y salir

**Castillo (estado 16)** [C][O]
- Hay que **mantener ↓** con el jugador en y = godY + 16…17 y x = godX + 6…10. Es decir, de pie sobre la casilla de 16×16 del dios, centrado ±2 px. No vale en knockback ni en otros estados especiales (E500 = 4…7).
- En 9 de las 11 salas la casilla del dios está tapada por una **piedra**. Hasta romperla no se puede estar ahí. Las salas 72 y 143 no tienen piedra.
- La Muerte (sala 87) exige además la **Salt** (objeto 0x2A, E099).
- **Animación de entrada:** el juego centra al jugador en x = godX + 8 y suena 0x82. El jugador se hunde 16 px en el suelo en unos 64 frames (~1 s) y aparece la pantalla del dios, con la música 0x8D.
- **Salida:** pulsar ↓ (pulsación nueva). Se redibuja la sala y el jugador sube 16 px (~1 s) hasta godY + 16, en el mismo sitio. Vuelve la música 0x88.
- Cada visita empieza de cero.

**Mundos (estado 14, «sala especial»)** [C][O]
- Es la última sala de cada mundo, del 1 al 9; el 10 no tiene. No tiene mapa propio: al cargarla se pasa directamente a la pantalla del dios.
- Se llega **subiendo por una escalera** de la fila 0 (columnas 15–16, x = 128) de la sala de abajo. Al bajar de y = 40 cambia de sala hacia arriba.
- En los mundos 1 y 7 la escalera siempre está. En los demás es una **escalera oculta**: aparece al matar a los enemigos marcados de una sala (tabla de abajo).
- **Salida:** pulsar ↓ cambia de sala hacia abajo. Apareces en la sala de abajo en x = 128, y = 40, encima de la escalera, con la música de mundo 0x8A.
- No hay animación ni sonido de entrada.

## La pantalla del dios [C][O]

Rejilla de 32×20 tiles de la zona de juego:
- **Arriba:** nombre del dios centrado en la fila 1 y, debajo, su retrato de 2×2 tiles en la fila 2, columna 15.
- **Ventana de texto:** filas 4–10, columnas 4–27. El mensaje empieza en la fila 5, columna 6, con 20 columnas por línea. Se escribe entero de golpe, sin efecto de máquina de escribir.
- **Tienda:** tres huecos con el icono del objeto (2×2) en la fila 7, columnas 9, 15 y 21. El precio va debajo, en la fila 9, con dos cifras.
- **Ventana inferior:** filas 11–15, con la **mano** que señala el hueco elegido.
- El jugador se dibuja quieto abajo, en el centro (x = 0x80, y = 0xAF), y debajo de él hay una escalera dibujada.
- **Controles:**
  - **← →** mueven la mano entre los huecos libres. Salta los vacíos y los ya comprados en esta visita, da la vuelta en los extremos y suena 0x2C.
  - **Espacio** compra o elige.
  - **↓** sale.
- Toda la pantalla avanza **cada frame** (60 Hz). Los tiempos de abajo están en frames.

## Tiendas [C][O]

Tabla b02:9253 + 2·tipo: tres pares (precio en BCD, objeto).

| Sala | Tipo | Dios | Hueco 0 | Hueco 1 | Hueco 2 |
|---|---|---|---|---|---|
| castillo 115 | 2 | Hefesto | Mine 30 | llave 5 | carcaj 20 |
| castillo 143 | 4 | Ares | carcaj 20 | Copper Shield 30 | Magnifying Glass 20 |
| castillo 65 | 5 | Ares | carcaj 20 | Silver Shield 40 | llave 5 |
| castillo 5 | 6 | Hefesto | Harp 30 | carcaj 20 | llave 5 |
| castillo 55 | 7 | Hefesto | carcaj 20 | Conch Shell 25 | llave 5 |
| castillo 4 | 9 | Ares | carcaj 20 | Gold Shield 50 | llave 5 |
| castillo 10 | 10 | Ares | carcaj 20 | Hat Helmet 50 | llave 5 |
| castillo 32 | 11 | Demeter | carcaj 20 | Necklace 25 | llave 5 |
| mundo 5, sala 18 | 12 | Demeter | — | Oar 30 | — |
| mundo 8, sala 21 | 13 | Demeter | — | Carpet 30 | — |

Al entrar, el mensaje 5 dice "YOU CAN BUY SOME OF MY TREASURE." y la mano empieza en el primer hueco con objeto.

Al pulsar espacio:
1. **Escudos de plata y de oro.** Solo se venden si E097 + 0x2A ≥ objeto, es decir, si el escudo actual es ≥ 1 para el de plata o ≥ 2 para el de oro. Si no, suena 0x2F y no pasa nada más: ni mensaje ni cobro.
2. **Monedas.** Si E048 < precio, sale "COINS ARE NOT ENOUGH." y suena 0x2F. El mensaje sigue hasta que se mueve la mano, que vuelve a poner el 5.
3. Si se puede comprar, **cobra** (BCD, actualiza la HUD) y aplica la compra:
   - **Carcaj (0x2D):** +100 flechas.
   - **Llave (0x2F):** +1 llave. Las dos cantidades tienen tope en 999.
   - **Escudos:** E097 pasa a 1 (cobre), 2 (plata) o 3 (oro). Ojo: comprar el de cobre teniendo uno mejor lo **rebaja** a 1 [C].
   - **Otros objetos:** su flag (E06F + objeto) pasa a 1.
4. Suena **0x2D**, el **icono desaparece** (el precio se queda) y la mano salta a otro hueco libre. Si no queda ninguno, espacio ya no hace nada.

Lo que cambia con cada visita [C][O]:
- Lo vendido solo se marca durante esa visita (E6CC–E6CE se ponen a 0 al entrar). Al volver, **está todo otra vez**, así que carcaj y llaves son ilimitados entrando y saliendo.
- **No se comprueba si ya tienes el objeto.** El Necklace sale a la venta otra vez después de comprarlo [O], y recomprarlo cobra otra vez [C].
- Ninguna tienda pide condiciones: la «condición» de las tiendas de los mundos 5 y 8 es llegar a ellas por la escalera oculta.

## Santuarios sin tienda [C][O]

**Demeter, contraseña (castillo 72, tipo 1)**
- Mensaje 0x0C "YOU WANT THE SECRET CODE…?" con SÍ/NO.
- **SÍ:** calcula la contraseña de 45 caracteres (b02:95A0) y la escribe en el lugar del mensaje, con "DID YOU GET THEM? YES" debajo. Con espacio: "SEE YOU AGAIN.".
- **NO:** "RESCUE THE BABY AS QUICKLY AS POSSIBLE.".
- Para el remake se sustituye por guardar (capítulo 11).

**La Muerte, resurrección (castillo 87, tipo 3; para entrar hace falta la Salt)**
- Si el otro personaje (índice E501 xor 1) tiene **0 vidas** (E058/E059) y le queda **crédito de resurrección** (E05A/E05B; empieza en 1 para cada uno), y además tienes **≥ 20 llaves, ≥ 100 monedas y ≥ 100 flechas**, dice "GIVE ME {flechas}x100, {monedas}x100 AND {llaves}x20…" con SÍ/NO.
  - **SÍ:**
    - cobra las tres cantidades;
    - el otro personaje recupera 1 vida, la vitalidad al máximo y la experiencia a 0, y gasta su crédito;
    - suena 0x30;
    - baja su sprite 80 px (1 px cada 2 frames) y sale "YOUR LOVER HAS REVIVED…";
    - a los 180 frames **sales solo**. Mientras dura, ↓ no hace nada.
  - **NO:** "THEN, YOU MUST FIGHT ALL BY YOURSELF…" y sales solo a los 180 frames.
- En cualquier otro caso (el otro vive, no hay crédito o no llega el dinero) solo dice el mensaje 0 ("IF YOUR LOVER IS KILLED, COME BACK HERE WITH…"), y se sale con ↓.
- En la práctica, **una resurrección por personaje**. Con el bonus de cartucho en el segundo slot (F0F8 bit 0) el crédito empieza en 99. Tras una contraseña, el crédito es 0 o 1 (un bit).

**Asclepio (castillo 54, tipo 8)**
- Solo dice "SEE A FAIRY TO RESTORE YOUR VITALITY.". **No cura**: la cura la dan las hadas.

**Santuarios de pista en los mundos (estado 14)**
- Tienen un solo mensaje (tabla b02:9308[tipo − 14]):

| Mundo | Sala | Tipo | Dios | Escalera (sala de abajo) | Mensaje |
|---|---|---|---|---|---|
| 1 | 15 | 14 | Demeter | sala 14, siempre visible | TAKE A MINE WITH YOU WHEN YOU GO TO WORLD 4. |
| 2 | 15 | 15 | Atenea | sala 7, oculta (marcados de la 7) | HIT THE WALL WITH YOURE SWORD IF YOU WANT TO GO TO THE OPPOSITE SHORE. |
| 3 | 16 | 16 | Atenea | sala 12, oculta (marcados de la 12) | APHRODITE. DO NOT BE AFRAID OF WATER. |
| 4 | 17 | 17 | Demeter | sala 15, oculta (marcados de la sala 8) | THE PURE WATER IS INSIDE THE WALL. |
| 5 | 18 | 12 | Demeter | sala 16, oculta (marcados de la sala 3) | **tienda**: Oar 30 |
| 6 | 19 | 18 | Atenea | sala 11, oculta (marcados de la 11) | HIT THE WALL WHEN YOU ARE HALFWAY OF THE STAIRS. |
| 7 | 20 | 19 | Demeter | sala 9, siempre visible | FIND THE SPELL WRITTEN ON A STONE MONUMENT UNDER THE WATER. |
| 8 | 21 | 13 | Demeter | sala 5, oculta (marcados de la sala 8) | **tienda**: Carpet 30 |
| 9 | 27 | 20 | Demeter | sala 18, oculta (marcados de la 18) | A GATE WHICH LEARDS TO GALIOUS WILL APPEAR SOMEWHERE IN THE CASTLE. |

- La sala especial no tiene jefe ni Great Key: es solo esto.
- Los textos se copian tal cual, con las erratas del original (YOURE, LEARDS). El mensaje 0x11 ("…BREAD AND WATER…") está en la ROM pero ningún santuario lo usa.
- Los textos se eligen según la BIOS: japonés con id, inglés con id + 0x20.

## Sonidos y música

| Qué | Id |
|---|---|
| Música de santuario | 0x8D |
| Al volver: castillo / mundo | 0x88 / 0x8A |
| Entrada en el castillo | 0x82 |
| Mover la mano | 0x2C |
| Comprar | 0x2D |
| Sin monedas o escudo fuera de orden | 0x2F |
| Resurrección | 0x30 |

## Economía

- **Ingresos:** monedas bajo piedras (25 en el castillo, +10 cada una) y recompensas de enemigos [C].
- **Gasto total:** la guía calcula **360 monedas** para comprarlo todo [G]. La resurrección cuesta 100 monedas, 100 flechas y 20 llaves.
- **Lo que se repone:** flechas (munición de todas las armas) y llaves (cada puerta gasta una). Las dos se compran sin límite: 20 monedas por 100 flechas y 5 por llave.

## Contraseña

Demeter da una contraseña de 45 caracteres que guarda el estado. Se introduce con L en la pantalla de título (estado 18) [G][C]. Existen contraseñas especiales y trucos [G]:
- **ZEUS** en pausa: continuar con F5;
- con otro cartucho de Konami en el segundo slot, ventajas (F0F8: 99 resurrecciones y 100 flechas, monedas y llaves [C]).

## Notas para el remake

- **Guardado:** sustituir la contraseña por guardado en bancos o estatuas (estilo HK), conservando a Demeter (castillo 72) como punto de guardado.
- **Desbloquear la sala especial de cada mundo:** quitar el bloqueo y conectar la escalera de la fila 0 de la sala de abajo (oculta hasta su evento).
- **Opcional (RemakeRules):** ocultar objetos ya poseídos en las tiendas y no rebajar el escudo. Por defecto, comportamiento original.
- **Economía:** mantener la tensión entre flechas, monedas y llaves: es el sistema de recursos del juego.
