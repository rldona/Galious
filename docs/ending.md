# Final del juego (tras Galious)

Leyenda como en `state_machine.md`: **[C]** código · **[O]** observado en openMSX · **[F]** forzado a mano · **[I]** interpretación · **[G]** guía.
Datos para Unity: `extracted/unity/ending.json` (`tools/extract/ending.py`, lo llama `unity_bundle.py`). Capturas: `logs/omsx/ending/` (no se versionan).
Tiempos en frames de vídeo a 60 Hz. El estado 17 corre una vez por frame [C][O].

**Cómo se midió** [O*]: savestate `w10_boss_fight_auto`, vida de Galious (E710) a 1 y Cross (E07A) a 1, dos flechas al núcleo (y del jugador 0x75, x 0x5C), a la derecha por el pasillo de abajo, romper piedras con la espada y tocar al bebé. Savestates `end_room6` (recién entrado en la sala 6) y `end_baby` (bebé a la vista).

## 1. La sala del jefe tras la derrota [C][O]

- Estado 19 como en los demás mundos (`boss_ai.md`, «Derrota»): sonido 0x84, el jefe se deshace en puntos durante 128 frames y queda derrotado.
- En el mundo 10 (E041 = 11) **no hay Great Key ni estado 20**. b00:4BA2 pone E64B = 6 y vuelve al estado 5.
- La sala sigue negra (sin decoración) y con las salidas que cerró el estado 15. Solo se abre la derecha, a la **sala 6**. Se sale por el pasillo de abajo (filas 16–18), que también comunica con la sala 4 por la izquierda, pero esa salida sigue cerrada.
- Música: el golpe final deja 0x8B en espera (b03:BD48 → b00:5D12, E01D/E01E). Empieza ~156 frames después y sigue sonando en la sala 6: al cambiar de sala no hay petición nueva [O].

## 2. La sala de las piedras (mundo 10, sala 6)

**La sala** [C][O]
- Es una sala normal del mapa: disposición 6 de la tabla de b08:9AF6, con 5 pisos de ladrillo y escaleras que los cruzan. Se entra por abajo a la izquierda (filas 16–18).
- Sus 4 enlaces son FF: no tiene salidas. Por eso el extractor creía que la tabla del mundo 10 acababa en la sala 5.
- Generadores (b06:B5CB): **10 enemigos 0x5C**, las «piedras». Están en (x, y) de pantalla, con el marcador en la y, en este orden (índice 0–9):
  (32,40) (176,40) (64,72) (144,72) (32,104) (144,104) (208,104) (96,136) (176,136) (144,168).
- Exportada como `world10.json`, sala 6 (`afterBossRight` = 6 en la sala 5).

**Las piedras (0x5C)** [C][O]
- Son sprites rosas quietos, de caja fija 16×16 (caja 0). No hacen daño.
- Tienen **5 de vida**. Cada golpe de espada o de arma resta 1 (sonido 0x1E): son 5 espadazos. Con el objeto E094 restan 2.
- Al romperse: sonido 0x20, sin explosión, sin experiencia y sin botín. En su sitio nace el bebé (0x6B) o un fantasma (0x39) (b03:B69B).

**Qué piedra tiene al bebé** [C][O][F]
- Se decide al cargar la sala, es decir, al entrar o al reaparecer. Lo hace el inicio de la primera piedra (b06:AABE):
  - F033 = (rnd & 3) + (rnd & 7), con rnd = R del Z80 rotado y XOR E003 (b05:81F1);
  - la piedra número F033 en el orden de la lista lleva IX+1F = 1.
- Si F033 = 10, **ninguna piedra tiene al bebé** [C][F]:
  - la sala no tiene salida, y solo se sale perdiendo una vida;
  - al reaparecer, el jugador vuelve a la sala 6 con las 10 piedras y una tirada nueva [O].
- Reparto de la suma (sobre 32): 0:1, 1:2, 2:3, 3–7:4 cada uno, 8:3, 9:2, 10:1. Medido: F033 = 4, 9 y 3 en tres entradas [O].
- [G] La guía dice «al azar»: cierto, pero con sesgo hacia las piedras 3–7. Hay 1/32 de no tener bebé.

**Las demás piedras sueltan un fantasma (0x39)** [C][O]
- Es una bola blanca. Hay uno por piedra vacía, así que son hasta 9 (o 10 si no hay bebé).
- Movimiento: espiral que se abre desde la piedra.
  - Centro: (y, x ± 10), con el lado al azar.
  - El ángulo sube 8/256 de vuelta por tick (32 ticks por vuelta) y el radio sube 0,5 px por tick, desde 0 (b06:A90D, seno de b06:B374).
  - Desaparece al salir de la zona visible. El de la fila de abajo se fue a los ~38 ticks [O].
- Combate: vida 40 y daño por contacto 14. Da 8 de experiencia, y botín de regla 3, nivel 3 (`enemies.json`).
- **No es el 0x28** de la guía («ectoplasma»): el Helmet no le protege [C].

**El bebé (0x6B) y el final** [C][O]
- Es un sprite quieto (patrón 0x17, amarillo). No se puede golpear ni hace daño.
- Lo puede tocar **cualquiera de los dos personajes**, con una sola condición: no estar parpadeando (E518 = 0). No se mira E501 ni si el otro personaje vive (b03:B1F7).
- Al tocarlo (b03:B2AD):
  - lo dibuja con los tiles 7A 7B / 7C 7D en su sitio y guarda el puntero en E6A8;
  - pone E024 = 1, y el estado 5 salta al **17**.

## 3. Estado 17: la secuencia final [C][O]

| Sub | Qué se ve | Frames | Música / sonido |
|---|---|---|---|
| 0–1 | la sala se congela; piedras y fantasmas dejan de verse; el jugador, el bebé y el marcador siguen | 40 | orden 0xFF: silencio |
| 2 | la sala se borra en 10 anillos de tiles, de fuera adentro (32×20, 30×18 … 14×2), uno cada 10 frames; los 2×2 tiles del bebé se respetan (b03:A06D) | 101 | — |
| 3 | negro con el jugador y el bebé; marcador visible | 120 | — |
| 4 | carga: pantalla sin marcador, castillo de 20×14 tiles (b15:B989) en un marco de hojas | ~19 | **0x92** (14,4 s) |
| 5 | escena del castillo (ver abajo) | 766 | 0x92 |
| 6 | carga: pantalla negra y fuente grande | ~28 | **0x93** (60,3 s, vuelve a 9,1 s) |
| 7 | epílogo y créditos que suben (ver abajo) | 5417 [O] | 0x93; se corta 312 frames antes del final (orden FB → 0xFF) |
| 8 | espera a que no suene nada → estado 0 | <1 | — |

En total son unos 6490 frames (108 s) desde que se toca al bebé.

**Escena del castillo** (b15:B7C0 inicio, B847 por frame; fase en F031) [C][O]:
- Fase 0, 191 frames: el castillo de Galious con su bandera roja. Junto a la bandera parpadea un destello (dos patrones, cada 16 frames).
- Fase 1, 137 frames:
  - la bandera roja baja 1 px cada 4 frames (y 0x2E → 0x3F, 65 frames);
  - después se cambia por la **bandera azul con la cruz**, que sube igual (72 frames).
- Fase 2, 52 frames: el bebé (2 sprites) sube 12 px entre Popolon y Aphrodite, 1 px cada 4 frames.
- Fase 3, 386 frames:
  - Popolon y Aphrodite se animan: dos tiles alternan cada 16 frames;
  - a partir de los 64 frames, cada 64 frames sale al azar un bocadillo, **«NICE!»** a la izquierda o **«GREAT!»** a la derecha, uno cada vez;
  - los bocadillos son dibujos de tiles (b15:BAA1, BAC1);
  - acaba 510 frames después del cambio de bandera.

**Epílogo** (b01:6A1E, 6ADC; texto en b15:A84A) [C][O]
- Fuente de 8×16: cada línea son dos filas de tiles. Las letras van de 0x0B a 0x3D (de 2 en 2; la mitad de abajo es el tile − 1). Además: `.` 3F, `,` 41, `©` 43, y solo los dígitos de 1987 (03 05 07 09).
- El texto entra por abajo y sube **1 px por paso**: una fila de 8 px cada 8 pasos.
  - El código da un paso cada 2 frames.
  - Medido: **3 frames por paso** mientras sube, por la carga de la VRAM.
  - Las esperas FD 255 duran 510 frames (2 por paso).
- Órdenes del texto: FE fila vacía, FD n esperar n pasos, FB parar la música, FF fin (E02E = 0).
- Texto (el estado 17 usa siempre este, en inglés, también con BIOS japonesa: b00:4A41 → b01:6ACA):
  - «EPILOGUE / THANKS TO YOU,THE BABY WAS / RESCUED SAFELY.THE KINGDOM / TOOK BACK PEACE AND VIGOR. / POPOLON WAS AWARDED THE / CROWN FOR HIS ACHIEVEMENT.IT / WAS DECIDED THAT HE PROTECT / THE KINGDOM WITH APHRODITE / AS HIS WIFE FOR THE REST OF / THEIR LIVES.THIS LEGEND WILL / BE TOLD AGAIN WHEN THEY ARE / APT TO LOSE BELIEF IN LOVE.»;
  - **pausa de 510 frames**;
  - **STAFF**: MAIN PROGRAMMER, SUB PROGRAMMER, GRAPHIC DESIGNER, SOUND CREATOR, PLANNER AND DIRECTOR (los nombres están en `ending.json`);
  - «WE PROMISE TO CONTINUE / SENDING ROMANTIC GAME / MESSAGES. / THANK YOU VERY MUCH / FOR YOUR JOINING IN / THIS STORY.»;
  - **pausa**;
  - «PRESENTED BY KONAMI / © 1987 KONAMI»;
  - **pausa**;
  - la música se para, suben 13 filas vacías y acaba.
- La misma rutina, con el texto de b15:AE28, cuenta la historia del principio en el título (b00:4369).

## 4. Después [C][O]

- b00:455F borra el bit 6 de E002 (no hay partida en curso) y pasa al estado 0: el logo de Konami. A los ~367 frames llega el título (estado 1) y luego la demo, como siempre.
- **No hay segunda vuelta**, ni contraseña final, ni puntuación. Una partida nueva empieza desde cero: castillo, sala 1, 1 vida y puertas cerradas [O].
- **Sin variaciones**: el estado 17 no lee el tiempo, la experiencia, las vidas ni el personaje [C]. El dibujo del castillo trae siempre a Popolon y a Aphrodite, aunque uno haya muerto [C][I].

## Para el remake

- Mantener la regla de una piedra con el bebé de entre 10 y fantasmas en las demás. El caso «sin bebé» (1/32) parece un descuido: conviene elegir una piedra al azar entre 0 y 9 (opción en `RemakeRules` si se implementa).
- Las duraciones de la tabla valen tal cual. Para el epílogo hay dos opciones: el ritmo del código (2 frames por px, 4058 frames) o el medido (3 frames por px, ≈ 5322–5417 frames).
