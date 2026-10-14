# IA de los enemigos (fase 2)

Leyenda: **[C]** leído en el código · **[I]** interpretación · **[O]** observado en openMSX.

**Método.**
- `tools/extract/enemy_ai.py` desensambla, para cada uno de los 53 tipos que aparecen en salas, su rutina de **inicio** (b04:66B4) y la de **actualización** (b06:B472).
- Cuatro agentes leyeron los desensamblados por grupos y escribieron una ficha por tipo, contrastándola con lo medido en el emulador (`items_enemies.md`, "Comportamiento observado").
- Las fichas se han revisado por encima, pero no se han comprobado una a una en el emulador.

**Avisos sobre las fichas:**
- **Nombres.** Son descriptivos, deducidos del comportamiento, no los del juego. Los de la guía de GameFAQs se recogen en `items_enemies.md`: Uh (0x2A), ectoplasma (0x28), nubes blancas (0x31), disparos de bambú (0x2E) y bolas de fuego (0x08). El 0x25, que las fichas llaman "armadillo", es por sus sprites el **esqueleto**.
- **Fallo ya corregido.** En las "notas previas" de cada grupo se menciona que los `.asm` se cortaban en los `RET` condicionales. Ya está arreglado en `enemy_ai.py` y los `.asm` se han regenerado. Los agentes trabajaron con un desensamblado propio corregido.
- **Campos del hueco corregidos respecto a la primera versión:**
  - la velocidad y está en IX+0C/0D;
  - IX+16/17 es la aceleración (gravedad);
  - IX+11 es un atributo del sprite que cambia con la animación;
  - un tile ≥ 0xBA es sólido.
- **Variable sin identificar.** E045 escala la velocidad de los ataques dirigidos; los valores de las fichas suponen E045 = 0. Quizá sea la dificultad.

## Comprobado en openMSX [O]

Los tres tipos que en la primera medición salían "quietos" sí se mueven como dice el código. La primera medición fallaba por dos razones:
- duraba solo 6 s y caía entera en la fase oculta;
- el medidor contaba como enemigo nuevo cualquier salto de más de 24 px.

- **0x12**, en el mundo 6, sala 5 (breakpoints en sus estados):
  - estado 0: oculto unos 4,5 s, con IX+8 bajando 1 cada 2 frames;
  - estado 1: emerge durante unos 3 s;
  - estado 2: rueda (x de 112 a 97 en 4 s).

  El sentido en que rueda lo decide b04:70FF comparando la **Y del jugador con la X del enemigo**. Parece un fallo del original [C].
- **0x14**, en el mundo 4, sala 3, con sus tres huecos:
  - oculto 4–5 s;
  - emerge en dos fases, de 36 y 20 frames;
  - pasea unos 5 s recorriendo la plataforma (por ejemplo, x de 15 a 113);
  - se hunde (estados 4 y 5) y repite.

  Los tiempos son el doble de los valores del código, 128–191 ticks.
- **0x2A**, en el mundo 2, sala 4: alterna oculto y visible (IX+2 = 00 ↔ F0) y reaparece en otro sitio, con x entre 48 y 176 e y entre 40 y 168. Se teletransporta.
- **Confirmado:** la IA de cada enemigo se ejecuta **uno de cada dos frames** (contado con breakpoints: 30 ejecuciones por segundo). Hay que tenerlo en cuenta al pasar a la remasterización las velocidades y los tiempos de las fichas, que están en ticks de IA.

## Daño por contacto [C][O]

Tabla **b04:64E5 + tipo**, leída en b03:B39A (el 0x19 usa b03:B3AE según su tamaño: 1, 2, 4, 8; con el Hat Helmet, E088, se divide entre 2 salvo si es 1). Comprobado con el esqueleto: quita 3 [O].

0x01: 8 · 0x02: 5 · 0x03: 1 · 0x04: 2 · 0x05: 7 · 0x07: 3 · 0x0A: 3 · 0x0B: 10 · 0x0C: 7 · 0x0E: 1 · 0x0F: 5 · 0x10: 2 · 0x11: 2 · 0x12: 4 · 0x13: 6 · 0x14: 6 · 0x15: 10 · 0x16: 8 · 0x17: 4 · 0x18: 2 · 0x19: 1 · 0x1F: 4 · 0x20: 3 · 0x22: 8 · 0x23: 0 · 0x24: 3 · 0x25: 3 · 0x26: 3 · 0x27: 8 · 0x28: 1 · 0x29: 5 · 0x2A: 1 · 0x2B: 10 · 0x2C: 2 · 0x2D: 3 · 0x2E: 2 · 0x2F: 1 · 0x31: 2 · 0x32: 0 · 0x34: 6 · 0x36: 6 · 0x3A: 0 · 0x3C: 5 · 0x40: 12 · 0x42: 20 · 0x43: 10 · 0x49: 8 · 0x4C: 7 · 0x4E: 0 · 0x50: 0 · 0x51: 0 · 0x64: 2 · 0x69: 7

## IA de enemigos — grupo G1 (0x01 0x02 0x03 0x04 0x05 0x07 0x0A 0x0B 0x0C 0x0E 0x0F 0x10 0x11 0x12)

### Notas previas (afectan a todas las fichas)

- **Los .asm de extracted/ai están incompletos.** `tools/extract/enemy_ai.py` corta el trazado en cualquier `RET cc`,
  porque z80dis escribe "RET nz" sin coma y la condición `',' not in text` lo trata como un RET incondicional.
  Por eso faltan muchas ramas (p. ej. 01 tras 7A49, 02 tras 6D1A, 03 tras AC86 y ACBF, 04 casi entero). Las rutinas se
  han vuelto a desensamblar en el scratchpad (copia corregida del trazador, `g1/*.asm`, más desensamblado lineal
  de los auxiliares). [C]
- **Velocidad y = +0C/+0D, no +16/+17.** El integrador de b06:B0ED suma (+0C/+0D) a (+4/+5)=y y (+0E/+0F) a
  (+6/+7)=x [C]. B137/B133/B124/B129 escriben en +0C/+0D [C]. B145 escribe en +16/+17, y B14C hace
  `+0C/0D += +16/17`, así que +16/+17 es una **aceleración (gravedad)**, no la velocidad [C]. Para los tipos 0x0A, 0x0D,
  0x24, 0x25 y 0x2D, B0ED solo suma el byte alto +0D a y (la parte fraccionaria se ignora, así que la velocidad y
  es entera) [C].
- **+11 es la caja de colisión, no la recompensa.** El valor inicial de +11 es el 5.º byte del descriptor, y la IA
  lo reescribe con cada fotograma de sprite (B0DD hace XOR con B; B08F/B067/B07B/B080 lo leen de tablas indexadas por
  (+0A−0xA0)/4) [C]. b03:B954 lo usa como índice en la tabla b14:9006 + 4·(+11) = (dy, dx, alto, ancho) para el
  contacto con el jugador [C]. Por ejemplo, la entrada 0x4E del esqueleto 0x25 es (1, 0, 15, 24), que cuadra con lo
  medido en `player.md` [O]. Abajo se le llama "attr". El botín se decide **por tipo** (647B[tipo], 64E5[tipo]; ver
  `items_enemies.md`, «Muerte y botín»). Las «clase / categoría / tier / valor» de las fichas de abajo se calcularon
  indexando con +11 y son erróneas; cada ficha lleva la corrección «botín real».
- Sólidos: 4DA1 convierte (H=x, L=y) en una dirección del mapa de tiles ED00+(y/8)*32+x/8; 4DC8 y 6D49 devuelven
  carry si el tile es > 0xB9, y 7C5E/9D2D comparan con ≥ 0xBA. Es decir, **tile ≥ 0xBA = sólido** [C].
- Auxiliares comunes [C]:
  - 6BEE: tile del suelo delante, en (x+16 o x, y+16); carry = hay suelo.
  - 6C04: tiles en la fila superior a la de 6BEE (pared delante) o en el borde de pantalla (x<8 a la izquierda, x≥0xF0 a la derecha); carry = bloqueado.
  - 6C25: carry si |jugador_y−16−y| ≤ 4, es decir, el jugador está en el mismo piso [I: el jugador mide 32 px y el enemigo 16].
  - B15F: invierte vx. B0D6: temporizador de animación (+8; con E de recarga y D/B como máscaras XOR de +0A/+11).
  - B17F: igual que B0D6, pero cada 8 frames de E604.
  - 7083: +0A = A (sprite).
  - 6551: crea un enemigo hijo (A = tipo, E = x, D = y, HL→F014 [I: vx del hijo], B→E624 [I: dirección]); llama a 65E3, que
    respeta el límite E620/E635.
- E604 es un contador de frames distinto de E003 y se usa para animar [C]. 7CA9 pone +1D=1 si jugador_x ≥ x [C].
  74A8, en cambio, pone +1D=1 si jugador_x < x (jugador a la izquierda) y devuelve carry si |dx| < 0x41 [C].
  Es decir, el significado de +1D depende de la rutina.
- Retorno empujado: algunos tipos hacen `PUSH HL` con B067, B07B o B080 antes de la máquina de estados. Esa rutina se
  ejecuta al salir de la IA y recalcula +11 desde una tabla según el sprite [C].

---

#### 0x01 — "bicho embestidor" (camina, se para al ver al jugador y embiste)
- **Vida / recompensa:** hp 4; reward_class 0, categoría 101, tier 4. Descriptor a2 a0 08 04 00. **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 2 [C].
- **Inicio:**
  - vx = ±1.5 px/f (0x180), con signo aleatorio según el bit 0 de R [C].
  - Invierte los bits 0-1 de +9 [C].
  - Sprite: +0A/+12 = A0/B0 si va a la derecha y C4/B4 si va a la izquierda. +13=6, +11=0 [C].
- **Comportamiento:**
  - Fase 0 (patrulla):
    - Cada frame, 7A5F compara la y del jugador. Si jugador_y = y−16, pone +1C=1 [I: mismo piso]. Si jugador_y = y+16, pone +1C=2 [I: el jugador está un piso por debajo]. En los dos casos pasa a fase 1 [C].
    - Si no, camina a 1.5 px/f y da la vuelta si no hay suelo delante o hay pared/borde (en ese caso pone +1E=1) [C].
    - Anima con XOR 0x04 del sprite y XOR 0x13 de attr cada 8 frames (E604) [C].
  - Fase 1 (aviso):
    - Al entrar se para (RES 4), pone el sprite A8 y +15=30 [C].
    - Durante 30 frames anima con XOR 0x14 [C].
    - Al acabar: guarda la x del jugador en +1F, apunta al jugador (74A8: +1D=1 si está a la izquierda), pone vx=±1.5 hacia él, vuelve a moverse y pasa a fase 2 [C].
  - Fase 2 (embestida):
    - Ejecuta el mismo paso de caminar (con giro en bordes y paredes) [C].
    - Sigue mientras no haya rebasado la x guardada en +1F [C].
    - Al rebasarla o al chocar: si +1C≠1, vuelve a fase 0 [C]. Si +1C=1 y el tile en (x+12 o x+4, y−16) es sólido, ataca [C]. Si no, vuelve a fase 0 [C].
  - Ataque:
    - Pone +8=15, el sprite AC (derecha) o C0 (izquierda) y attr=1, y se para [C].
    - Llama a 7A76: si y−jugador_y = 16 y 0 ≤ jugador_x−x < 16, copia E503→E514 y hace farcall b03:B389 (no se sigue) [C] [I: golpe al jugador].
    - Pide el sonido 0x21, pasa a fase 3 e invierte de nuevo los bits de +9 [C].
  - Fase 3: espera 15 frames. Después restaura +9, vuelve a moverse y pasa a fase 0 [C].
- **Ataques / hijos:** no crea hijos. Sonido 0x21 al atacar. El daño es directo vía b03:B389 [I].
- **Comparación con lo observado:** encaja. step_x 1.51 ≈ 1.5 px/f, y moving 0.5 cuadra con las pausas de 30 y 15 frames [I].
- **Dudas:**
  - No se entiende por qué el ataque (7A09-7A18) exige un tile sólido en y−16 sobre el propio enemigo [I].
  - No se ha seguido b03:B389, ni E503/E514.
  - El significado de +9 (XOR 3) y de +12/+13 no está claro.

#### 0x02 — "chispa que recorre superficies" (se pega a suelos, paredes y techos)
- **Vida / recompensa:** hp 1; reward_class 0x75, categoría 10, tier 18. **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 1 [C].
- **Inicio:**
  - La dirección inicial viene en +1 (fase 0-7) desde la creación (no se ve) [C].
  - 6D7F copia 7 tablas indexadas por fase en +14..+1A (tablas en 6DC6..6DF6, 8 bytes cada una) [C].
  - Pone +1B=1 [C].
  - Velocidad según la fase [C]:
    - Fases 0-3 (horizontal): vx = +1 (fase par) o −1 (fase impar), vy=0.
    - Fases 4-7 (vertical): vy = +1 (par, baja) o −1 (impar, sube), vx=0.
  - +1C=17, +8=2 [C].
  - +1D = 80..127 aleatorio (0x50 + rnd&0x2F) [C].
- **Comportamiento:**
  - Anima con XOR 4 de +0A cada 2 frames [C].
  - Las tablas de la fase dan dos sondas (+14,+15) y (+16,+17), que son desplazamientos dx,dy de tile, y tres fases destino (+18, +19, +1A) [C]. Por ejemplo, en la fase 0 (derecha, apoyado en el suelo) [C]:
    - sonda 1 = (x+16, y+15), delante;
    - sonda 2 = (x, y+16), debajo;
    - fases destino 5 (sube), 6 (baja) y 1 (vuelta).
  - Si la sonda 1 es sólida (pared delante), pasa a la fase +18: gira para trepar por ella [C].
  - Si la sonda 2 no es sólida (se acabó la superficie), pasa a la fase +19: dobla la esquina [C]. Durante los 17 frames siguientes a un cambio de fase (+1B=1, +1C) no comprueba la sonda 2 [C].
  - Si +1B=0, +1D va bajando; al llegar a 0 se renueva (80..127) y pasa a la fase +1A, la contraria (media vuelta) [C].
  - Límites de pantalla: x<8 o x≥0xF0 (horizontal), y<0x18 o y≥0xB0 (vertical). Al tocarlos pasa a la fase +1A [C].
  - En conjunto, sigue el contorno de los bloques a 1 px/f, como el "spark" clásico [I].
- **Ataques / hijos:** ninguno.
- **Comparación con lo observado:**
  - Lo observado ("vuela", con algo de dy y y_range 24) encaja con un movimiento que recorre superficies [I].
  - step_x 0.73 < 1. Puede deberse a los frames de movimiento vertical [I].
- **Dudas:**
  - No se ve de dónde sale la fase inicial (+1).
  - Rutina 4083 (read_byte_table) [C, leída].

#### 0x03 — "gota/babosa de techo" (anda colgada del techo a tirones)
- **Vida / recompensa:** hp 1; reward_class 5, categoría 0, tier 7. **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 0 [C].
- **Inicio:**
  - Elige al azar entre fase 0 (activa) y fase 0xFF (dormida). En la fase 0xFF desactiva el movimiento [C].
  - +8=0x18, attr=0x66, +14=0 [C].
  - vx=+1 px/f en ambas ramas: las dos cargan 0x100, probablemente un bug [C].
- **Comportamiento:**
  - Fase 0xFF (dormida): despierta (pasa a fase 0) cuando y = jugador_y−24 y |jugador_x−6−x| < 32 [C].
  - Fase 0 (avanza):
    - Cada frame da la vuelta si el tile en (x+16 o x, y−2) NO es sólido, es decir, si se acaba el techo [C] [I: anda pegada al techo]. También la da si hay pared o borde delante (6BEE+6C04) [C].
    - Ciclo de +14 [C]:
      - frames 0-7: quieta, sprite CC/D0;
      - frames 8-23: se mueve a 1 px/f, sprite C8, attr 74 (16 px por ciclo);
      - frames 24-27: quieta;
      - frame 28: sprite C4, attr 66, +8=24, sonido 0x0B, pasa a fase 1.
  - Fase 1: espera 24 frames. Después pone +14=0, vuelve a moverse y pasa a fase 0 [C].
- **Ataques / hijos:** ninguno. Sonido 0x0B en cada pausa [C].
- **Comparación con lo observado:**
  - Mueve el 31 % del ciclo, que es de 52 frames [C]. Lo observado (moving 0.19, speed 0.19) es menor; puede deberse a las instancias dormidas [I].
  - toward_player 0.27 encaja con no apuntar nunca al jugador [I].
- **Dudas:**
  - En AC6F-AC77 el sprite CC/D0 se elige con la condición invertida respecto a AC49 [C].
  - No está claro el significado de la condición y = jugador_y−24.

#### 0x04 — "columna/pistón de tiles" (sin sprite; baja y sube del techo)
- **Vida / recompensa:** hp 1; reward_class 0x22, categoría 3, tier 8. Descriptor 40 00 00, sin sprite. **Botín real:** regla 2 (4.ª y 6.ª), nivel 0 [C].
- **Inicio:**
  - +0A=0x12, +14=0 (longitud) y movimiento desactivado [C].
  - +12/+13 = dirección del tile en (x, y) [C].
  - +0C=15 (recarga del temporizador) y +8=15 [C].
- **Comportamiento:** el paso es de 8 px cada 15 frames [C].
  - Fase 0 (extender): baja y en 8 px y suma 1 a +14 [C].
    - Se detiene si y ≥ 0xB0 o si el tile en (x, y+8) o el de al lado (x+8) es sólido [C].
    - Al detenerse pasa a fase 1 (salvo en fase 3) [C].
  - Fase 1 (retraer): sube y en 8 px y resta 1 a +14 hasta 0. Después vuelve a fase 0 (DEC +1) [C].
    - Las fases 0 y 1 forman un pistón continuo [I].
  - Fase 2 (latente): espera a que el jugador esté entre 0 y 24 px por debajo de y. Entonces pasa a fase 3, con +0A=0x12 y +8=15 [C].
  - Fase 3 (persigue en vertical): si jugador_y ≥ y, se extiende; si jugador_y < y, se retrae. No cambia de fase [C].
  - El dibujo de los tiles no está en esta rutina [C]. Lo hará el bucle común u otra rutina, usando +12/+13 [I].
- **Ataques / hijos:** ninguno.
- **Comparación con lo observado:** encaja con "quieto", con speed_y 0.13 y y_range 16: 8 px cada 15 frames ≈ 0.53 px/f mientras se mueve [I].
- **Dudas:**
  - No se sabe quién elige la fase inicial (0 o 2).
  - **Tiles** [O] (barrido de tiles, castillo 117 y 128): la columna se dibuja con 99 (base) y 9C 9D B4 B5, todos por debajo de 0xBA, así que **no es sólida**: solo hace daño.
  - Aquí +0C se usa como recarga y no como vy. No afecta porque el movimiento está desactivado [C].
- **Remake:** la y del enemigo es la cabeza, así que la caja 22 (b14:9006) solo cubre la cabeza; la columna de encima solo se dibuja. La fase latente (2/3) no se usa hasta saber quién la elige.

#### 0x05 — "serpiente que escupe"
- **Vida / recompensa:** hp 3; reward_class 0x2A, categoría 3, tier 1. **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 2 [C].
- **Inicio:** +15=120. vx=±1 px/f aleatorio. Sprite A0 (derecha) o B0 (izquierda) [C].
- **Comportamiento:** tiene B080 como retorno, que recalcula attr [C].
  - Fase 0 (patrulla) [C]:
    - Camina a 1 px/f y da la vuelta en bordes de plataforma, paredes y bordes de pantalla.
    - Anima cada 8 frames (A0↔A4 o B0↔B4).
    - Cada 120 frames: se para y pone el sprite A8/B8, +15=10, fase 1.
    - Si el jugador está en su piso (6C25), pasa a fase 2 con +8=120 y se para.
  - Fase 1: a los 10 frames dispara y vuelve a fase 0 con +15=120 [C].
  - Fase 2 (acecho, 120 frames) [C]:
    - Se gira hacia el jugador cambiando el signo de vx. El movimiento sigue desactivado, así que solo se orienta.
    - Mantiene el sprite de ataque.
    - Dispara cuando +8&0x1F = 0: 3 disparos, en +8=96, 64 y 32.
  - Fase 3: tras la fase 2, +8=60. Camina 60 frames sin mirar al jugador y vuelve a fase 0 [C].
- **Ataques / hijos:** crea el tipo 0x1A en (x, y+4) con HL=±0x0400, es decir, ±4 px/f hacia donde mira [C] [I: HL es la vx del hijo]. No hay sonido.
- **Comparación con lo observado:** encaja. Hijos 0x1A observados ×3; camina a 1 px/f con moving 0.47 [I].
- **Dudas:** no se ha verificado cómo usa el hijo F014 (65E3 y siguientes no se han seguido).

#### 0x07 — "oruga saltarina" (salta huecos en su mismo piso)
- **Vida / recompensa:** hp 2; reward_class 0x26, categoría 3, tier 3. **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 1 [C].
- **Inicio:** vx=±0.5 px/f (0x80) con signo aleatorio. +16 = y inicial (altura de "casa"). +8=10 [C].
- **Comportamiento:**
  - Fase 0 (camina) [C]:
    - Anima cada 10 frames (sprite XOR 4, attr XOR 1).
    - Si hay suelo delante, gira solo si hay pared o borde.
    - Al borde de una plataforma mira 48 px más allá: a la derecha (x+48) si x < 0xB8, a la izquierda (x−45) si x ≥ 0x38. Si no se cumple, gira.
    - Para saltar exige:
      - suelo sólido en (tx, y+16) y (tx+8, y+16);
      - los 4 tiles del cuerpo libres en el destino (96FE).
    - Si se cumplen, salta: vy = −2.125 (0xFDE0), vx = ±1.375 (0x160), fase 1. Si no, gira.
  - Fase 1 (aire) [C]:
    - Sprite A4 y gravedad manual: vy += 0x22 (≈0.133 px/f²) cada frame.
    - Cuando y supera la y de casa (+16), aterriza: y = casa, vy=0, vx=±0.5, fase 0.
    - Unos 32 frames de vuelo, ≈44 px y ≈17 px de altura [I].
- **Ataques / hijos:** ninguno.
- **Comparación con lo observado:** encaja ("cae entre plataformas", y_range 21 ≈ 17 px de salto + sprite) [I].
- **Dudas:** ninguna importante.

#### 0x0A — "caballero" (castillo; camina, salta huecos, sube y baja pisos, usa escaleras)
- **Vida / recompensa:** hp 6; reward_class 0x4E, categoría 4, tier 0. **Botín real:** regla 1 (2.ª y 5.ª), nivel 1 [C].
- **Inicio:**
  - Pone el global E62E=1 y +2 bit 1 [C].
  - +8 = rnd&0x7F (0-127) [C].
  - +1D por 7CA9 (1 = jugador a la derecha) [C].
  - Gravedad +16/17 = 0x0050 y +18=5 [C].
  - vx = ±0.4375 px/f (0x70), negativa si +1D=1. Es decir, empieza alejándose del jugador [C].
  - Sprite B0 (vx+) o C0 (vx−, con +2 bit 0 = espejo [I]). +12/+09/+11/+13 desde 98F9 [C].
- **Comportamiento:** el integrador usa vy entera, solo +0D (ver notas) [C].
  - Paso de caminar 7B68 [C]:
    - Anima con E604 bit 2.
    - Si hay suelo delante y ni pared ni borde (x<16 o x≥0xD8), sigue.
    - Si el tile sobre el suelo de delante es 0xD2, marca +14=1. Si después se acaba el suelo con +14=1, coloca x = (EA63)−16 o +8 y aborta el frame [I: plataforma o puerta especial].
    - Ante otro obstáculo, si +1E=0 intenta saltar el hueco (9A74 + 9CB0, con destino a +48/−32 px y comprobación de suelo y cuerpo libre). Si no puede, gira: invierte +2 bit 0 y +1D y resta 1 a +18.
  - Fase 0 (patrulla) [C]:
    - Cuando +8 llega a 0, 7C84 calcula la relación vertical +1E: 0 mismo piso, 1 jugador más arriba, 2 más abajo. Pone +18=6 y pasa a fase 1.
    - Si antes +18 llega a 0 (5 giros), elige +1E al azar entre 0 y 2 y pasa a fase 1.
  - Fase 1 (busca cambiar de piso), cada frame al 50 % [C]:
    - (a) 9A67: salto a otro piso. Si +1E=1 (arriba) se ofrecen +32/−16 px en x y y−16. Si +1E=2 (abajo), +40/−24 px y y+16. Comprueba la plataforma de destino con la tabla 9C68.
    - (b) 7CB7: escalera. Busca tiles 0x9A/0x9B en (x+4, x+12) a la altura y+15 (subir) o y+24 (bajar) [I: escalera].
      - Si la hay, centra x, pone vy=−1 (sube, sprite A0) o +1 (baja, y+=9, sprite A8), attr 0x55 y vx=0, y pasa a fase 3.
    - Si nada funciona, camina [C].
    - Con +18<5 y par, rota +1E (7BE9). Si es impar, vuelve a fase 0 con un nuevo +8 aleatorio, vy=0, +18=5 y vx=±0.4375 [C].
  - Saltos (9988) [C]:
    - Pone la fase 2.
    - vy = 0xFBB0 si +1E es impar (salto alto, −5 px/f efectivo), o 0xFE30 (−2 px/f) en otro caso.
    - vx=±2 px/f según +1D. Sprite B8/C8.
  - Fase 2 (aire) [C]:
    - vy += 0x50 por frame, con tope +6 px/f.
    - Mientras sube no comprueba nada.
    - Al bajar y encontrar suelo en (x+4 o x+12, y+16): alinea y a 8 px y vuelve a fase 0 (nuevo +8, vx 0.4375).
  - Fase 3 (escalera) [C]:
    - Anima cada 8 frames.
    - Si y está fuera de 0x20..0xAF, o encuentra un tile sólido en su propia posición, invierte el sentido (sprite XOR 8, −vy).
    - Al pisar suelo en otro tile no sólido, se alinea y sale caminando hacia +1D.
- **Ataques / hijos:** ninguno; solo daño por contacto [I]. No hay sonido.
- **Comparación con lo observado:** encaja con "camina (cae entre plataformas)", moving 0.23, y_range 32 [I]. La vx de 0.4375 casa con step_x ≈ 0.95 si el movimiento se mide por frames con avance [I].
- **Dudas:**
  - Significado de E62E, EA63 y del tile 0xD2.
  - 9CA3/9D1C se han leído solo por encima: las condiciones exactas de destino del salto están sin verificar.
  - Por qué empieza mirando en sentido contrario al jugador.
  - Los tiles 0x9A/0x9B como escalera es una suposición.

#### 0x0B — "pez/dardo rápido" (corre a 4 px/f a ráfagas)
- **Vida / recompensa:** hp 1; reward_class 0x3E, categoría 3, tier 3. **Botín real:** regla 1 (2.ª y 5.ª), nivel 3 [C].
- **Inicio:**
  - +8=8 [C].
  - Dirección aleatoria [C]:
    - vx=+4 px/f, sprite A0, attr 0x3F;
    - o vx=−4 px/f, sprite A8, attr 0x05.
  - +15 = rnd&0x2F (0-47) y +14 = +15+0x50 (80-127) [C].
- **Comportamiento:**
  - Fase 0 [C]:
    - Se mueve y solo gira ante pared o borde de pantalla: el resultado de suelo de 6BEE se ignora, así que no se detiene en los bordes de plataforma.
    - Anima cada 8 frames (sprite XOR 4, attr XOR 0x3A).
    - Tras +14 frames se para y pasa a fase 1.
  - Fase 1: espera +15 frames y vuelve a fase 0 con nuevos temporizadores aleatorios [C]. Si +15 era 0, la espera es de 256 frames, porque DEC 0 da 255 [I].
- **Ataques / hijos:** ninguno.
- **Comparación con lo observado:** encaja. step_x 4.0; speed 1.74 ≈ 4 × moving 0.43 [I].
- **Dudas:** no se sabe si cae en los huecos. No tiene gravedad propia, así que probablemente vive en suelo continuo o en el agua [I].

#### 0x0C — "hombre-llama" (crece, camina disparando, se convierte en fuego volador y vuelve a posarse)
La rutina se comparte con 0x13 (en 9F0B dispara 0x44 si es 0x13 y 0x56 en otro caso) [C].
- **Vida / recompensa:** hp 3; reward_class 0x51, categoría 4, tier 0. **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 2 [C].
- **Inicio:**
  - +17=10, +8=8, +18=8, +15=1, +16=10, +13=4, +12=0 [C].
  - +1B = rnd&0x4F + 0x30 (48..127) como temporizador de disparo [C].
- **Comportamiento:** retorno por B067 (attr según sprite) [C]. Si +1A≠0, está en modo vuelo; si no, salta por tabla según la fase [C].
  - Fase 0 (aparece/crece) [C]:
    - Cada 10 frames el sprite avanza por la tabla 9F37 (B0, AC, A8, A4).
    - Al 4.º paso: vx=+1 px/f (al azar invertida), sprite B8 o C0 con máscara de animación 0x0C/0x7C, +17=0, fase 1.
  - Fase 1 (camina 256 frames) [C]:
    - Va a 1 px/f, gira en bordes y paredes y anima cada 8 frames.
    - Cuando +1B llega a 0, recarga 48..127 y, si el jugador está delante (en la dirección de vx), crea el tipo 0x56 en (x+8, y+8). Si está detrás, reintenta al frame siguiente (+1B=1).
    - Al terminar +17: vx=0 y fase 2.
  - Fase 2 (encoge) [C]:
    - Cada 10 frames recorre la tabla 9F3B (A4, A8, AC, B0).
    - Al final: sprite B0, máscara 0x1C, +17=48, +1A=1 (vuelo), fase 0.
  - Modo vuelo (7DBD) [C]:
    - Vuela a 2 px/f en una de 16 direcciones aleatorias (tablas seno/coseno en 7E49/7E69, ×2).
    - Cambia de dirección cada 16..31 frames.
    - Rebota en y 0x28/0xB0 y en x 8/0xE8.
  - Aterrizaje [C]:
    - Tras 48 frames comprueba suelo sólido en (x, y+16) y (x+8, y+16) y cuerpo libre.
    - Si se cumple, se para (vx=vy=0), pone +1A=0 y +14=0 y vuelve a la fase 0 (crecer). Si no, reintenta cada frame.
- **Ataques / hijos:** tipo 0x56 (0x44 si el padre es 0x13) en (x+8, y+8). HL (→F014) vale E003 por arrastre de 81F1, es decir, basura [C] [I: el hijo se orientará solo]. No hay sonido.
- **Comparación con lo observado:** encaja ("camina", 1 px/f, hijos 0x56 ×2). El vuelo no aparece en lo observado [I].
- **Dudas:**
  - Si el hijo 0x56 apunta solo.
  - 9EC6 hace XOR de +0B con +12=0, lo que no tiene efecto para 0x0C.

#### 0x0E — "murciélago" (cuelga del techo, se descuelga y revolotea)
- **Vida / recompensa:** hp 1; reward_class 0x69, categoría 4, tier 7. **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 0 [C].
- **Inicio:**
  - +8=4, +18=4, +16=11 [C].
  - +17 = 96..127 y +15 = 16..31 [C].
  - Pone el global E63E=1 [C].
  - Si hay techo sólido en (x, y−1) y (x+8, y−1) y el cuerpo está libre, sprite A0 y +19=1 (empieza colgado) [C].
- **Comportamiento:**
  - Fase 0 → 1. Si +19 bit 0, se cuelga ya [C].
  - Fase 1 (busca techo) [C]:
    - Vuela en una dirección aleatoria entre 16, a 2 px/f, cambiando cada 16..31 frames y rebotando en los límites.
    - Se cuelga cuando encuentra techo encima.
    - Al colgarse: +8=80, sprite A0, attr 0x67, se para, fase 2.
  - Fase 2 (colgado): 80 frames [C]. Después sprite AC, attr 0x69, vy=+1 px/f, +14=10, fase 3.
  - Fase 3 (se suelta): 10 frames cayendo a 1 px/f con animación cada 6 frames [C]. Después +17 = 96..127 y fase 4.
  - Fase 4: revolotea de forma aleatoria (igual que en el vuelo de 0x0C) durante +17 frames, y vuelve a fase 1 para buscar techo [C].
- **Ataques / hijos:** ninguno. No persigue al jugador [C].
- **Comparación con lo observado:** encaja con "vuela", y_range 118 y toward_player 0.51 (aleatorio) [I].
- **Dudas:** significado del global E63E (A103 lo pone a 1 en el inicio).

#### 0x0F — "bola que crece" (camina; crece por etapas)
- **Vida / recompensa:** hp 6; reward_class 0x32, categoría 4, tier 0. **Botín real:** regla 1 (2.ª y 5.ª), nivel 1 [C].
- **Inicio:** vx=±1 px/f aleatorio [C]. Sprite y attr desde la tabla 73AA (derecha) o 73B2 (izquierda), indexada por la etapa +1E [C]:
  - etapas 0-3 a la derecha: C0, C8, D0, D8;
  - etapas 0-3 a la izquierda: A0, A8, B0, B8.

  +8=4. El valor de B (0x33/0x32) se calcula y se descarta [C].
- **Comportamiento:**
  - Camina a 1 px/f y gira en bordes, paredes y pantalla [C].
  - Anima cada 4 frames (XOR 4) [C].
  - Si +1C≠0, lo pone a 0 y sube la etapa +1E (máximo 3) [C].
  - Nada en la IA pone +1C [C]. Lo pone el código de impacto b03:B54D (ver «Quién escribe IX+1C»): la bola crece con cada golpe que no la mata [C].
- **Ataques / hijos:** ninguno.
- **Comparación con lo observado:** encaja (camina a 1 px/f, moving 0.5, turns_per_s alto) [I]. moving 0.5 puede deberse a que la x se integra a medias [I].
- **Dudas:** quién escribe +1C (buscar escrituras a IX+1C fuera de la IA).

#### 0x10 — "acechador que embiste y vuelve" (oculto hasta que el jugador entra en su piso)
- **Vida / recompensa:** hp 1; reward_class 0x0B, categoría 1, tier 10. **Botín real:** regla 1 (2.ª y 5.ª), nivel 0 [C].
- **Inicio:** se para y guarda la x de casa en +8 [C].
- **Comportamiento:**
  - Fase 0: quieto hasta que el jugador está en su piso (6C25). Entonces se mueve, pone el sprite A0 y vx = ±2 px/f hacia el jugador, y pasa a fase 1 [C].
  - Fase 1 (embiste) [C]:
    - Anima cada 4 frames con la tabla 7245 (A0, A4, A8, A4).
    - No se detiene en el jugador: sigue hasta un borde de plataforma, una pared o el borde de pantalla.
    - Al llegar, vx = 1 px/f en sentido contrario y pasa a fase 2.
  - Fase 2 (regresa) [C]:
    - Anima cada 8 frames y no comprueba paredes.
    - Cuando x = casa: fase 0, parado, sprite A0.
- **Ataques / hijos:** ninguno.
- **Comparación con lo observado:** encaja ("quieto" en general, "camina" cerca del jugador; step_x 1.39 sale de la mezcla de 2 y 1 px/f) [I].
- **Dudas:** ninguna importante.

#### 0x11 — "erizo que dispara púas"
- **Vida / recompensa:** hp 2; reward_class 0x58, categoría 4, tier 4. **Botín real:** regla 1 (2.ª y 5.ª), nivel 0 [C].
- **Inicio:** +15=120. vx=±2 px/f aleatorio. attr 0x58. Sprite A4 (derecha) o B4 (izquierda) [C].
- **Comportamiento:**
  - Fase 0 (camina a 2 px/f) [C]:
    - Gira en bordes, paredes y pantalla.
    - Anima cada 8 frames (XOR 0x0C).
    - Si no está en enfriamiento (+17=0) y el jugador está a menos de 65 px en horizontal (74A8), pasa a fase 3. Este disparo no comprueba el piso del jugador.
    - Al pasar a fase 3: +8=60, se para, sprite AC (jugador a la derecha) o BC (a la izquierda), attr 0.
    - El enfriamiento +17/+18 dura 40 frames tras cada disparo.
    - A los 120 frames: +15=90, se para, fase 1.
  - Fase 1 (descanso de 90 frames, parpadeando) [C]:
    - Alterna el sprite B0/C0 con attr 59/5A cada 8 frames (E604 bit 3).
    - Al final: +15=30, se orienta al jugador (solo el sprite) y pasa a fase 2.
  - Fase 2: espera 30 frames y vuelve a fase 0 con +15=120, moviéndose con la misma vx de antes [C].
  - Fase 3 (dispara) [C]:
    - Dura 60 frames.
    - Cuando +8&0x1F = 0 (en +8=32, un único disparo): +18=40, +17=1, sonido 0x10 (9154).
    - Crea el tipo 0x1D en (x, y+8) con HL=±4 px/f hacia el jugador.
    - Al acabar vuelve a fase 0.
- **Ataques / hijos:** tipo 0x1D a 4 px/f, con sonido 0x10 [C].
- **Comparación con lo observado:** encaja (step_x 2.0; hijos 0x1D ×2) [I].
- **Dudas:** ninguna importante. 7973 solo pone attr=0.

#### 0x12 — "armadillo / bola enrollada" (oculto, emerge, rueda disparando y se vuelve a esconder)
- **Vida / recompensa:** hp 5; reward_class 0x13, categoría 0, tier 6. **Botín real:** regla 1 (2.ª y 5.ª), nivel 1 [C].
- **Inicio:** +8 = 96..159 aleatorio. Se para. Pone +2 bit 2 [C] [I: oculto/intangible].
- **Comportamiento:** retorno B07B (attr según sprite) [C].
  - Fase 0 (oculto): espera +8 frames. Después +1F=0, +8=30 y fase 1 [C].
  - Fase 1 (emerge) [C]:
    - Cada 30 frames sube +1F; el sprite lo da la tabla 70ED (1→A8, 2→A4).
    - En +1F=3:
      - vuelve a moverse;
      - +8 = 5..20 (se descuenta cada 32 frames, así que da de 160 a 640 frames de vida activa);
      - vx = ±1 px/f, sprite B0 o B8;
      - +1F=10 y +15 = 128..191;
      - borra +2 bit 2 y pasa a fase 2.
    - Para la dirección compara **jugador_y con x** del enemigo (7102, E505 frente a IX+7), probablemente un bug: el resultado es casi arbitrario [C].
  - Fase 2 (rueda) [C]:
    - Va a 1 px/f, gira en bordes y paredes y anima cada 10 frames (B0/B4 o B8/BC).
    - Cuando +15 llega a 0 se para y pasa a fase 3 con +15=30.
    - Cada 32 frames (E604&0x1F = 0) resta 1 a +8. Al llegar a 0: fase 4, +1F=3, +8=30, se para y pone +2 bit 2.
  - Fase 3 (pausa): 30 frames [C]. Al acabar vuelve a moverse, fase 2, nuevo +15, y crea el tipo 0x1E en (x, y) con vx ±2 px/f según el sentido de la marcha.
  - Fase 4 (se esconde) [C]:
    - Cada 30 frames baja +1F (sprites A4, A8, AC).
    - Al bajar de 0: fase 0 y vuelve a ejecutar el inicio.
- **Ataques / hijos:** tipo 0x1E a ±2 px/f tras cada parada de rodar [C]. No hay sonido.
- **Comparación con lo observado:** **no encaja del todo.** Se observó "quieto" (0 movimiento, sin hijos en 718 frames). Lo esperable son ~190-250 frames oculto/emergiendo y después rodar. Puede ser que la observación coincidiera con instancias ocultas o que otra condición (¿+2 bit 2?, ¿sala?) las bloquee [I].
- **Dudas:**
  - Por qué no se vio moverse.
  - El significado exacto de +2 bit 2.
  - Confirmar que 7102 compara de verdad jugador_y con x (el código es inequívoco: `LD A,(E505) / CP (IX+7)`) [C].

---

### Tabla resumen

| Tipo | Nombre | Vel. | Esquema | Hijos / sonido | ¿Encaja con observado? |
|---|---|---|---|---|---|
| 0x01 | bicho embestidor | 1.5 px/f | patrulla → aviso 30f → embiste hasta la x del jugador → golpe (15f) | b03:B389 (¿daño?), sonido 0x21 | sí |
| 0x02 | chispa recorre-superficies | 1 px/f | 8 fases (4 dir × apoyo), sondas de tiles, media vuelta cada 80-127f | — | sí ("vuela") |
| 0x03 | gota/babosa de techo | 1 px/f | dormida o activa; avanza 16 px cada 52f colgada del techo | sonido 0x0B | aprox. |
| 0x04 | pistón de tiles (sin sprite) | 8 px/15f | extiende/retrae; o latente → sigue la y del jugador | — | sí |
| 0x05 | serpiente que escupe | 1 px/f | patrulla, dispara cada 120f; acecha 120f si el jugador está en su piso | 0x1A a ±4 px/f | sí |
| 0x07 | oruga saltarina | 0.5 px/f | camina; salta huecos de ~48 px (vy −2.125, g 0.133) | — | sí |
| 0x0A | caballero | 0.44 px/f (salto 2) | patrulla; busca el piso del jugador: saltos alto/bajo, escaleras 0x9A/9B | — | sí |
| 0x0B | pez/dardo | 4 px/f | ráfagas de 80-127f y pausas de 0-47f | — | sí |
| 0x0C | hombre-llama (comp. con 0x13) | 1 px/f, vuelo 2 px/f | crece → camina 256f disparando → encoge → vuela ≥48f → aterriza | 0x56 (0x13: 0x44) | sí |
| 0x0E | murciélago | 2 px/f | colgado 80f → cae 10f → revolotea 96-127f → busca techo | — | sí |
| 0x0F | bola que crece | 1 px/f | camina; etapa +1E sube cuando +1C≠0 (externo) | — | sí |
| 0x10 | acechador que embiste | 2 / 1 px/f | quieto → embiste al ver al jugador en su piso → vuelve a casa | — | sí |
| 0x11 | erizo que dispara | 2 px/f | camina 120f / descansa 90+30f; dispara si |dx|<65 | 0x1D a ±4 px/f, sonido 0x10 | sí |
| 0x12 | armadillo enrollado | 1 px/f | oculto → emerge → rueda (paradas con disparo) → se esconde | 0x1E a ±2 px/f | **no** (observado quieto) |

### Dudas más importantes
1. **Bug del extractor** (`enemy_ai.py`): corta en `RET cc` y deja fuera ramas enteras de casi todos los .asm. Hay que regenerarlos tras arreglar la condición, por ejemplo con `len(text.split()) == 1`.
2. **Campos mal etiquetados en el contexto:** la vy real es +0C/+0D (B0ED); +16/+17 es aceleración (B14C); y +11 es un atributo de sprite, no la recompensa.
3. **0x12** no se movió en la observación, aunque el código hace que ruede y dispare 0x1E. Además, 7102 compara jugador_y con la x del enemigo (bug original o mala lectura del campo).
4. Rutinas y variables no seguidas:
   - farcall b03:B389 (golpe de 0x01) y E503/E514;
   - quién escribe +1C de 0x0F (¿crece al ser golpeada?);
   - globales E62E y E63E;
   - EA63 y el tile 0xD2 en el caballero 0x0A;
   - cómo usan los hijos F014/E624 (HL basura en 0x0C).
5. Condiciones raras:
   - el ataque de 0x01 exige un tile sólido en y−16;
   - 0x03 despierta con y = jugador_y−24 y siempre arranca con vx=+1;
   - 0x0A empieza alejándose del jugador.

## IA de enemigos — grupo G2 (tipos 0x13 0x14 0x15 0x16 0x17 0x18 0x19 0x1F 0x20 0x22 0x23 0x24 0x25)

### Notas previas (afectan a todos los tipos)

- **Los .asm de `extracted/ai/` están truncados.** `tools/extract/enemy_ai.py` trata `RET nz/z/nc/c` (sin coma) como
  RET incondicional y deja de desensamblar justo después, así que falta casi todo lo que hay tras cada
  `DEC (IX+n) / RET nz`. Para este análisis usé una copia corregida en el scratchpad (`g2/dis.py`, que sigue
  los CALL hasta profundidad 2 y no entra en 6551/65DA/65E3), sin tocar el proyecto. [C]
- **Velocidades.** El bucle común (b06:B407) llama a B0ED si IX+2 bit 4 está activo [C]. B0ED suma
  **IX+0C/0D a y (IX+4/5)** y **IX+0E/0F a x (IX+6/7)** [C]. Así que **IX+0C/0D es la velocidad y** (B137 la escribe),
  e **IX+16/17 es una aceleración**: B145 la escribe y B14C la suma a IX+0C/0D, como la gravedad [C]. El contexto
  dice "+16/17 velocidad y", pero el código no lo confirma. Para los tipos 0x24, 0x25, 0x0D, 0x2D y 0x0A, B0ED solo suma el
  **byte alto** de vy a y, sin la fracción (B11F→B117) [C].
- El bucle común **destruye el enemigo** si y ≥ 0xB8 o x ≥ 0xF9 (B452 → B41C) [C].
- La creación (b04:6640) pone IX+2 = F0 o F8, así que los bits 4-7 están activos por defecto [C]. Lectura de los bits:
  bit 7 = visible (0x16 lo hace parpadear) [I], bit 4 = aplicar velocidad [C], bits 5/6 = colisión con el jugador o
  con el arma [I].
- `406E` (4DC8): mira el tile en (H=x, L=y) del mapa ED00 y da C si el tile es mayor que 0xB9 (sólido) [C]. `4071`: el tile
  siguiente (x+8) [C]. `4083`: A = (HL+A) [C]. `6551`/`65DA`: crear un enemigo con A = tipo, D = y, E = x y B pasado a
  E624; 6551 guarda además HL en F014 [C]. Los dos acaban en 65E3, que **sí** respeta el límite E620 < E635 y el de sprites E621/E636 [C] (corregido: antes decía que no).
- Rutinas de borde que comparten varios tipos [C]:
  - `6BEE`: suelo delante. Mira el tile en (x+16 si va a la derecha o x si va a la izquierda, y+16) y da C si hay suelo.
  - `6C04`: pared delante. Mira el tile de encima (y+8) y da C si es sólido. Si no lo es, da C cuando x < 8 (yendo
    a la izquierda) o x ≥ 0xF0 (yendo a la derecha).
  - `6C25`: mismo nivel. Da C si |(jugador_y − 16) − y| ≤ 4.
- `B18F/B195` + `B223/B262`: velocidad dirigida a un punto. El módulo M es parámetro + 8·E045. Resultado: vx, vy ≈ (M/32)·(cos, sen)
  px/frame, con una tabla atan grosera en B274 y la tabla de seno en B374 [C]. E045 es una global que no he
  identificado [I].
- `7E25`: velocidad en 16 direcciones (tablas 7E49 y 7E69, de 22,5° en 22,5°) multiplicada por B px/frame [C].
- IX+11 cambia con el fotograma del sprite (B0D6 le hace XOR B, y B071/B076 lo leen de tablas indexadas por IX+0A) [C].
  No parece una recompensa: más bien un segundo atributo de dibujo (color o forma) [I].
- E604 es un contador global que se usa para el ritmo de las animaciones (AND 7, AND 1F) [C]. No sé si es el mismo que E003 [I].

---

#### 0x13 — espectro caminante que se desvanece y vuela (comparte código con 0x0C)
- **Vida / recompensa:** 4 / descriptor `81 a0 0f 04 2d` (clase 45). **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 2 [C].
- **Inicio (A3E2):** +17 = 10 (temporizador), +08 = 8, +15 = 1, +13 = 3 (número de fotogramas de aparición),
  +12 = 5 (máscara XOR de IX+0B), +16 = 10, +18 = 4 (+16 y +18 no se leen en la actualización) [C].
  +1B = (aleatorio AND 4F) + 0x30, es decir 48..63 o 112..127 frames hasta el primer disparo [C].
- **Comportamiento** (A401; al volver ejecuta B071, que fija IX+11 según el sprite) [C]:
  - Si **+1A ≠ 0** está en **modo vuelo** (9E54) [C]:
    - Rebota en el rectángulo y∈[0x28,0xB0), x∈[8,0xE8) [C].
    - Cambia de rumbo cada 16..31 frames, a una de 16 direcciones aleatorias a **2 px/frame** (7DBD→7E25 con B = 2) [C].
    - Anima con los sprites B0 y AC, alternando cada 8 frames [C].
    - Al agotar +17 (48 frames, y luego cada frame) busca dónde aterrizar: los tiles (x, y+16) y (x+8, y+16) deben ser
      sólidos y los de (x / x+8, y+7 / y+15) libres (9E5D, 96FE) [C].
    - Si los encuentra: se detiene, +1A = 0 y vuelve a la fase 0. Si no, +17 = 1 y lo reintenta en el frame siguiente [C].
  - **Fase 0, aparición (9DD8):** cada 10 frames pone los sprites A0, A4 y A8 (tabla A427). Al cuarto tick invierte IX+0B
    (XOR +12), pone vx = **1 px/frame** con un 50 % de probabilidad de ir a la izquierda, elige el sprite de andar
    (BC a la derecha, C4 a la izquierda; alterna con XOR 4 cada 8 frames), pone +17 = 0 y pasa a la fase 1 [C].
  - **Fase 1, caminar (9DFE):** dura 256 frames (+17 empieza en 0 y se decrementa) [C]. Da la vuelta si delante no hay suelo, hay
    pared o llega al borde (6BEE/6C04) [C]. Cuando se agota: invierte IX+0B, se detiene y pasa a la fase 2 [C].
  - **Fase 2, desvanecerse (9E2D):** repite los sprites A0, A4 y A8 cada 10 frames. Luego sprite B0, +17 = 48, +1A = 1
    (modo vuelo) y fase 0 [C].
- **Ataques / hijos:**
  - En la fase 1, cada vez que +1B llega a 0 (se recarga con 48..127 aleatorio) dispara **solo si mira hacia el
    jugador** (compara jugador_x con x y el signo de vx) [C]. Crea el **tipo 0x44** en (y+8, x+8) [C].
  - Si no mira hacia el jugador, pone +1B = 1 y vuelve a probar en el frame siguiente [C].
  - El mismo código crea el tipo 0x56 si el enemigo no es 0x13 (es decir, si es 0x0C) [C].
  - Sin sonidos [C].
- **Comparación con lo observado:** encaja. Hijos 0x44 ✓. Camina a 1 px/frame con pausas de animación, lo que da una media de 0,4 ✓.
  No se observó el modo vuelo (speed_y = 0): con 359 frames por instancia apenas da tiempo a llegar a él [I].
- **Dudas:**
  - Qué significan +16 y +18, que se escriben en el inicio pero no se leen aquí.
  - El efecto visual de XOR IX+0B con 5 (¿color?) [I].
  - En 6C04 las dos ramas cargan BC = FFE0 (C04/C0D); parece una rama muerta [C].

#### 0x14 — criatura que emerge del suelo, pasea y se hunde
- **Vida / recompensa:** 4 / `81 a0 0e 04 38`. **Botín real:** regla 1 (2.ª y 5.ª), nivel 2 [C].
- **Inicio (7249):** borra los bits 4-7 de IX+2 (invisible, sin colisión, quieto) [C][I]. +08 = 128..191 aleatorio [C].
- **Comportamiento** (7255; al volver ejecuta B076, que pone IX+11 según el sprite) [C]:
  - **Fase 0:** espera +08 frames. Luego se hace visible (bit 7), sprite A0, **sonido 8** y +08 = 6 [C].
  - **Fase 1, emerger:** cada 6 frames pone los sprites A0, A4 y A8 (tabla 72A1). Al terminar activa los bits 5, 6 y 7 de
    IX+2, pone IX+0B = 9 y elige una dirección al azar: sprite AC con vx = +1 o sprite B4 con vx = −1 px/frame [C].
  - **Fase 2:** espera 10 frames. Luego +08 = 128..191 y activa el movimiento (bit 4) [C].
  - **Fase 3, pasear:** anda a **1 px/frame** durante +08 frames [C]. Da la vuelta sin suelo, con pared o en el borde [C].
    Anima cada 10 frames (B0/AC a la derecha, B8/B4 a la izquierda) [C]. Al acabar se detiene y espera 10 frames [C].
  - **Fase 4:** sprite A8, IX+0B = 14, **sonido 8** [C].
  - **Fase 5, hundirse:** cada 6 frames pone los sprites A8, A4 y A0. Después vuelve al inicio: oculto y espera otros
    128..191 frames (bucle) [C].
- **Ataques / hijos:** ninguno [C]. Sonido 8 al emerger y al hundirse [C].
- **Comparación con lo observado:** **no encaja del todo.** Lo observado dice "quieto" (moving 0,03), pero el código
  pasea a 1 px/frame durante 128..191 frames de cada ciclo de unos 300..400 [C].
  Posibles causas [I]: el trazador no sigue al enemigo mientras está oculto, o en esas salas está encerrado.
- **Dudas:** el significado exacto de los bits 5 y 6 de IX+2. La causa de la discrepancia con lo observado.

#### 0x15 — guardián volador que orbita alrededor de un centro móvil (jefe menor)
- **Vida / recompensa:** 40 / `c4 08 05 28 42`. **Botín real:** regla 3 (última viva), nivel 3 [C].
- **Inicio (8E05):** sin aplicar velocidad (RES 4) [C]. Centro inicial (+17, +19) = (y 0x60, x 0x80) [C].
  Ángulo +1A = 0, velocidad angular +1B = 1, +1C = 60, +1D = 0 [C]. +08 = 128..255, +1E = 60 ("enfado") [C].
  Primer disparo +15 = 32..95 [C]. Coloca el sprite directamente en la órbita [C].
- **Comportamiento** [C]:
  - **Órbita (8EB2):**
    - Cada frame el ángulo +1A aumenta en +1B (0..255 = vuelta completa) [C].
    - La posición es y = cy ± 31·sen y x = cx ± 31·cos (tabla B374 desplazada >>3): un círculo de **radio 31 px** [C].
    - Cada 60 frames la velocidad angular +1B sube o baja 1. Oscila en diente de sierra triangular entre −8 y +8, con
      un periodo de unos 1920 frames, así que la órbita acelera, se para y cambia de sentido [C].
  - **Fase 0:** solo órbita durante +08 frames (128..255) [C].
  - **Fase 1:** +08 = 48..111 frames. El **centro** se desplaza con la velocidad calculada en la transición [C]:
    - Mientras +1E ≠ 0: dirección aleatoria, sesgada hacia abajo si cy < 0x40 y hacia arriba si cy ≥ 0x80, con módulo 0x10.
    - Cuando +1E = 0: **dirección hacia el jugador**, con módulo 0x10 + 8·E045 (≈ 0,5 px/frame si E045 = 0).
    - El centro queda limitado a cy∈[0x20,0x80] y cx∈[0x20,0xC0] [C].
    - Si la velocidad angular es 0, el centro no se mueve (8EF0) [C].
    - Al acabar vuelve a la fase 0 [C].
  - **+1E** baja 1 cada 32 frames: tras unos 1920 frames (unos 32 s) el centro empieza a perseguir al jugador [C].
  - Animación: sprites 08 y 09 alternando cada 8 frames (E604 bit 3), con IX+11 = 42/43 [C].
- **Ataques / hijos:** cada 32..95 frames crea el **tipo 0x45** en (y+8, x+8) [C]. Sin sonidos [C].
- **Comparación con lo observado:** encaja. "Vuela" ✓, hijos 0x45 ✓, vx y vy ≈ 0,5 ✓ (por la componente orbital) [I].
- **Dudas:** qué es E045 (un factor global de velocidad [I]).

#### 0x16 — aparición que embiste en línea recta y se teletransporta
- **Vida / recompensa:** 24 / `c4 00 02 18 01`. **Botín real:** regla 3 (última viva), nivel 2 [C].
- **Inicio (8C1F):** +08 = 30, sin aplicar velocidad, sprite 0 si el jugador está a la derecha y 2 si está a la
  izquierda [C].
- **Comportamiento** [C]:
  - **Fase 0, aparecer:** 30 frames **parpadeando** (bit 7 de IX+2 alterna cada 2 frames) [C].
    Luego se hace visible, activa el movimiento y calcula la **velocidad hacia el jugador** con módulo 0x40
    (≈ **2 px/frame**, más 8·E045) [C]. Fija el sprite según el signo de vx [C].
  - **Fase 1, embestida:** va en línea recta. No mira tiles; atraviesa paredes [I por ausencia]. Anima con el bit 0 del sprite
    cada 4 frames [C].
    Si sale del área x∈[0x10,0xE0] o y∈[0x20,0xA0], se detiene y pasa a la fase 2 [C].
  - **Fase 2:** 30 frames parpadeando. Luego se vuelve invisible y pasa a la fase 3 [C].
  - **Fase 3:** 30 frames invisible. Luego se **teletransporta** a x = 48..175 aleatorio e y = (E604 AND 7F) + 0x20 y
    vuelve a la fase 0 [C].
- **Ataques / hijos:** en la fase 3, cada 10 frames crea el **tipo 0x46** en su posición, que es el punto donde salió del
  área. Son 2 por ciclo [C]. Sin sonidos [C].
- **Comparación con lo observado:** encaja. toward_player 0,96 ✓ (embiste hacia el jugador) y 2 hijos 0x46 ✓.
- **Dudas:** si mientras es invisible o parpadea sigue haciendo daño (no toca los bits 5 y 6) [I].

#### 0x17 — centinela que patrulla y dispara al ver al jugador en su nivel
- **Vida / recompensa:** 5 / `81 c8 0a 05 13`. **Botín real:** regla 2 (4.ª y 6.ª), nivel 1 [C].
- **Inicio (951C):** vx = ±**2 px/frame** al azar [C]. Sprite C8 a la derecha o D0 a la izquierda; +14 = 0 a la
  derecha y 1 a la izquierda [C]. IX+11 = 0x13 [C].
- **Comportamiento** [C]:
  - **Fase 0:** patrulla. Anima cada 8 frames (sprite XOR 4, IX+11 XOR 1A) y da la vuelta sin suelo, con pared o en el borde.
    Si el jugador está **en su mismo nivel** (|dy| ≤ 4, 6C25): se detiene, se **gira hacia el jugador**, pone +15 = 30 y
    pasa a la fase 1 [C].
  - **Fase 1:** espera 30 frames. Luego crea el **tipo 0x47** en (y, x) con B = +14 (dirección) y pone +15 = 120 [C].
  - **Fase 2:** espera 120 frames. Luego retoma el movimiento con su vx original; si se había girado, el sprite vuelve a
    mirar en la dirección de vx [C].
- **Ataques / hijos:** 0x47 (proyectil horizontal [I]), uno por avistamiento y como mucho uno cada unos 150 frames [C].
- **Comparación con lo observado:** encaja. Paso de 2 px ✓, moving 0,25 por las paradas largas ✓, hijo 0x47 ✓.
- **Dudas:** ninguna importante.

#### 0x18 — rodador lento que solo gira en ciertos fotogramas
- **Vida / recompensa:** 1 / `81 a0 09 01 1e`. **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 0 [C].
- **Inicio (6C50):** +14 = dirección al azar (1 = izquierda), vx = ±**0,5 px/frame** [C].
- **Comportamiento** (6C5D) [C]:
  - Siempre, cada 4 frames: avanza el fotograma +1F en un ciclo de 7 (0..6). Va hacia delante si +14 = 0 y hacia atrás si
    +14 = 1 [C]. Sprite e IX+11 de la tabla 6CC4: A0, A8, AC, A4, B4, B0, A0 [C].
  - **Fase 0:** se mueve. Ante falta de suelo, pared o borde:
    - Si el fotograma es 0 o 6, gira en el acto (invierte +14 y vx) [C].
    - Si no, se **detiene** (RES 4) y pasa a la fase 1 [C].
  - **Fase 1:** sigue animando quieto hasta que el fotograma sea 0 o 6. Entonces gira y retoma el movimiento [C].
- **Ataques / hijos:** ninguno [C].
- **Comparación con lo observado:** encaja. Paso de 1 px a 0,5 px/frame y moving 0,22 por las paradas ✓.
- **Dudas:** ninguna.

#### 0x19 — murciélagos que se fusionan por parejas (pequeño → mediano → grande → 0x37)
- **Vida / recompensa:** 1 (nivel 0); 2 (nivel 1); 4 (nivel 2) / `81 d0 0b 01 4a`. **Botín real:** regla 3 (última viva), nivel 0 [C].
- **Inicio (863D):** +19 = 128 (tiempo hasta intentar fusionarse), +08 = 4, +1C = 80 (primer disparo) [C].
  +1B = nivel = E62F (global) [C].
  - Si el nivel es 0: identificador de pareja +18 = (E62C++) >> 1, así que cada dos creados comparten pareja [C].
  - Si el nivel es 1: sprite C8, IX+11 = 48, vida 2. Si el nivel es 2: sprite C0, IX+11 = 46, vida 4 [C].
  - En ambos casos (nivel 1 o 2) empieza en la fase 6, con +18 = el B que pasó el padre [C].
- **Comportamiento** (868F) [C]:
  - **Deambular** (8145, compartido con 0x22): dirección aleatoria entre 16 a **1 px/frame** durante 3..31 frames [C].
    - Si entra en la franja derecha o inferior, se redirige hacia el centro: elige una de 3 direcciones según el
      cuadrante (tablas 819E y 81A6) y la mantiene 24 frames [C].
    - La franja es x∈[0xC8,0xF0) o y∈[0x8F,0xDF); si IX+9 bit 2 está activo, x≥0xB8 o y∈[0x7F,0xDF) [C].
    - No mira tiles [C].
  - Si tiene pareja (+18 bit 7 = 0), +19 baja cada frame. Al llegar a 0 pasa a la **fase 4** [C].
  - **Fase 4:** busca otro 0x19 con el mismo +18 [C].
    - Si lo encuentra: vuela hacia él con módulo 0x80 (≈ **4 px/frame**), pasa a la fase 5 y pone E62D = 1 [C].
    - Si no: vuelve a la fase 0 y se marca sin pareja (+18 bit 7) [C].
  - **Contacto:** la rutina global AFE8 (activa mientras E62D ≠ 0) pone +1D = 1 cuando |dy| < 18 y |dx| < 18 con la
    pareja [C].
  - **Fase 5:**
    - El primero de la pareja con +1D activo que aún ve a su compañero se elimina [C].
    - El segundo, que ya no lo ve: **sonido 11**, se elimina y sube el nivel (+1B++ → E62F) [C].
      Crea un **0x19 del nivel siguiente**, o un **0x37** si el nivel llega a 3, en su posición y con B = su +18 [C].
    - Si la pareja desaparece sin contacto (+1D = 0), vuelve a la fase 0 sin pareja [C].
  - **Fase 6** (recién fusionado): espera a que no quede ningún 0x19 en la fase 5. Entonces +18 >>= 1 (se emparejan los
    fusionados), fase 0 y E62D = 0 [C].
  - Animación: cada 4 frames, sprite XOR 4. En el nivel 3, cada 8 frames, sprite XOR 1 [C].
- **Ataques / hijos:** cada 64 frames, pero **solo si E62F ≠ 0** (ya hubo alguna fusión en la sala), crea el **tipo 0x48**
  en (y+16, x+16) [C]. La fusión crea 0x19 o 0x37 [C].
- **Comparación con lo observado:** encaja. "Vuela" ✓ y no se observaron hijos ✓: sin fusiones E62F = 0 y no dispara.
- **Dudas:**
  - No hay límites izquierdo ni superior: al salir por la izquierda o por arriba, x o y pasan de 0 a 0xFF y B452 lo
    destruye. ¿Es intencionado? [C][I]
  - Quién pone E62F y E62C a 0 al entrar en la sala (no lo he seguido).
  - 9F3F (otro tipo, probablemente 0x37) también incrementa E62F.

#### 0x1F — corredor saltarín (salta huecos y sube o baja de plataforma)
- **Vida / recompensa:** 3 / `81 a0 02 03 00`. **Botín real:** regla 1 (2.ª y 5.ª), nivel 1 [C].
- **Inicio (942F):** +08 = 0..127, +1D = dirección al azar, gravedad +16/17 = 0x0100 (**1 px/frame²**) [C].
  Sprite A0 con vx = **+3,625 px/frame**, o sprite AC con vx = −3,625 e IX+11 = 0x13 [C].
- **Comportamiento** [C]:
  - **Fase 0, correr:** corre a 3,625 px/frame durante +08 frames, animando cada 8 [C]. Ante falta de suelo, pared o borde:
    - Si +1E = 0 y el tile de delante a la altura de la cabeza está libre (9A77), comprueba si hay dónde aterrizar
      al mismo nivel (dy +16) a 44 px por la derecha o 30 px por la izquierda (9CB0) [C]. Si lo hay, **salta el hueco** [C].
    - Si no, da la vuelta y +18 baja 1 [C].
    - Al acabar +08: tipo de salto +1E = 0, 1 o 2 al azar, +18 = 6 y fase 1 [C].
  - **Fase 1, buscar salto:** cada frame sondea según +1E (tabla 9ADF) [C]:
    - Tipo 1 (subir): un hueco libre en (dy −16; dx +32 a la derecha o −16 a la izquierda) y una plataforma a
      dy −16 / dx +64 o −48 (tabla 9C68) [C].
    - Tipo 2 (bajar): un hueco en (dy +16; dx +40 o −24) y una plataforma a dy +48, con hueco libre justo delante [C].
    - Si el sondeo va bien, salta. Si no, sigue corriendo; cada giro baja +18 [C].
    - Con +18 = 4 y 2 cambia el tipo de salto (+1E = (+1E+1) mod 3); con +18 = FF vuelve a la fase 0 [C].
  - **Salto (949F):** vy = **−9 px/frame** si +1E es impar (subir) y −4 en los demás casos [C]. vx de la tabla 9510:
    ±3,19 en los tipos 0 y 2; +3,375 o −3,25 en el tipo 1 [C]. Sprite A8 o B4, IX+11 = 0 y fase 2 [C].
  - **Fase 2, en el aire:** gravedad de 1 px/frame², con vy de caída limitada a 6 px/frame [C].
    Al tocar suelo bajo los pies (99C9) ajusta y a múltiplo de 8, pone vy = 0 y vuelve a la fase 0 con un +08 nuevo [C].
- **Ataques / hijos:** ninguno. Sin sonidos [C].
- **Comparación con lo observado:** encaja. Paso de 3,64 px ✓ (3,625) y moving 0,5 ✓.
- **Dudas:**
  - No seguí 9D1C ni 9D2D por completo (comprobaciones extra de tiles en la zona de aterrizaje) [I].
  - **Remake:** el sondeo (para 0x0A, 0x1F, 0x25 y 0x2D) mira el hueco de 9ADF y la plataforma de 9C68 como arriba. Sin datos: la x de la plataforma al bajar (se usa la del hueco) y 9D1C/9D2D (se pide que el cuerpo quepa) [I].
  - En la fase 2 se comprueba el aterrizaje incluso mientras sube (94D0 ignora el resultado de 9B1D) [C].

#### 0x20 — embestidor invulnerable lento
- **Vida / recompensa:** invulnerable (FE) / `81 a0 01 fe 19`. **Botín real:** regla 1 (2.ª y 5.ª), nivel 1 [C].
- **Inicio (93D4):** vx = ±**0,25 px/frame** al azar [C].
- **Comportamiento (93E2)** [C]:
  - Ante falta de suelo, pared o borde: da la vuelta y **vuelve a 0,25 px/frame** [C].
  - Con el camino libre: si |jugador_x − x| < 80, el jugador está **delante** (en el sentido de la marcha) y en su mismo
    nivel (|dy| ≤ 4), acelera a **1 px/frame** en esa dirección [C].
  - Mantiene esa velocidad hasta el siguiente obstáculo; nunca se gira hacia un jugador que tenga detrás [C].
- **Ataques / hijos:** ninguno [C].
- **Comparación con lo observado:** encaja razonablemente. speed 0,13 y toward 0,1: casi siempre va a 0,25 px/frame,
  que es 1 px cada 4 frames [I].
- **Dudas:** ninguna.

#### 0x22 — escupefuego volador errante
- **Vida / recompensa:** 24 / `81 c8 03 18 09`. **Botín real:** regla 3 (última viva), nivel 2 [C].
- **Inicio (80C0):** +08 = 64, +15 = 4 [C].
- **Comportamiento (80C9)** [C]:
  - Cada 64 frames alterna el sprite entre C4 y C8 (XOR 0C). Al pasar a C8 lanza el **sonido 15** (una vez cada 128 frames) [C].
  - Cada 4 frames (+15): si el sprite tiene el bit 3 (fase C8, "boca abierta" [I]), crea el **tipo 0x4A** en (y, x+8) [C].
    Son hasta 16 por ráfaga de 64 frames, limitados por E635 [C][I].
  - En ese 4.º frame no llama a la rutina de deambular, aunque la velocidad se sigue aplicando [C].
  - El resto de frames **deambula** como 0x19 (8145): 16 direcciones a 1 px/frame durante 3..31 frames, con
    redirección al centro en la franja derecha o inferior [C].
- **Ataques / hijos:** 0x4A en ráfagas; sonido 15 [C].
- **Comparación con lo observado:** encaja. "Vuela" ✓, unos 0,37 px/frame ✓ y 14 hijos 0x4A ✓.
- **Dudas:** como en 0x19, no hay límites izquierdo ni superior.

#### 0x23 — estatua o bloque dormido que se activa al rodearlo
- **Vida / recompensa:** 64 / `9a a0 0e 40 6c`. **Botín real:** regla 4 (nunca) [C].
- **Inicio (A198):** +16 = y+8 (nivel de referencia), +14 = x−13 y +15 = x+23 (zonas de disparo) [C].
  +12 = A4 y +13 = 0E: segundo sprite o color [I]. Borra los bits 4-7 de IX+2: invisible e inactivo [C][I].
- **Comportamiento (A1B8)** [C]:
  - **Fase 0, dormido:**
    - Si jugador_y∈[+16, +16+16] activa el bit 0 de +17 [C].
    - Si además jugador_x∈[x−16, x−11] activa el bit 1; si jugador_x∈[x+20, x+25] activa el bit 2 [C].
    - Los bits se **acumulan**. Cuando +17 = 7 (el jugador ha pasado por los dos lados a su altura) despierta [C].
    - Al despertar: IX+2 |= F0 y **vx = ±0,25 px/frame hacia el lado donde está el jugador**; pasa a la fase 1 [C].
  - **Fase 1:** avanza. Mira el suelo en (x+10 o x, y+24) porque mide 24 px de alto, y la pared, y gira al borde (6C04) [C].
    Ante un obstáculo invierte +18, se detiene y pasa a la fase 2 [C].
  - **Fase 2:** espera a que el jugador esté en su nivel y **al otro lado** (el que se aleja del obstáculo). Entonces
    reanuda el avance hacia él a 0,25 px/frame [C].
- **Ataques / hijos:** ninguno; hace daño solo por contacto [I]. Sin sonidos [C].
- **Comparación con lo observado:** encaja ("quieto"): en la traza no llegó a despertar [I].
- **Dudas:** para qué sirven +12 y +13 (probablemente el dibujo de dos sprites). Por qué hace falta pasar por los dos lados [I].

#### 0x24 — saltador entre niveles que persigue al jugador
- **Vida / recompensa:** 3 / `81 a0 09 03 00`. **Botín real:** regla 2 (4.ª y 6.ª), nivel 1 [C].
- **Inicio (9B30):** +08 = byte aleatorio, nivel +19 = ((y AND E0) >> 5) − 1 [C]. Los niveles 0..4 corresponden a las
  alturas de suelo 56, 88, 120, 152 y 184 [I]. +1D al azar, vx = ±**2 px/frame**, sprite A0 o AC [C].
  En vuelo solo se aplica a y el byte alto de vy [C].
- **Comportamiento (9B54)** [C]:
  - **Fase 0, pasear:** anda a 2 px/frame, gira sin suelo, con pared o en el borde y anima cada 8 frames [C].
    - Cada 32 frames elige un salto aleatorio C = 0..4 (0 = ninguno), restringido por el nivel (9D47) [C]:
      en el nivel 0 no sube; en el nivel 1 no sube dos; en el 3 no baja dos; en el 4 no baja [C].
    - Si el jugador no está haciendo nada (**E500 = 0**) y el aterrizaje es válido (9CA3), salta [C].
    - Al acabar +08: fase 1 con un +08 aleatorio [C].
  - **Fase 1, perseguir:** se alterna en bloques de 32 frames (bit 5 de +08) [C]:
    - Bloque con el bit 5 activo: solo pasea [C].
    - Bloque con el bit 5 a 0: cada 8 frames **se gira hacia el jugador** y elige el salto según la diferencia
      vertical (9C6C) [C]: 0 si está a la misma altura; 1 o 3 si el jugador está arriba (1 si dy ≤ 32); 2 o 4 si está abajo.
      Si E500 = 0 y el aterrizaje es válido, salta [C].
    - Al acabar +08 vuelve a la fase 0 [C].
  - **Comprobación de aterrizaje (9CA3):** plataforma sólida en dy −16, +48, −48 o +80 y dx +56 (derecha) o −40
    (izquierda), con y'∈[0x28,0xB9) y x'∈[0x10,0xF0) [C].
  - **Saltos (9BB1)** [C]:

    | Tipo | Efecto en el nivel | vy | Gravedad | vx |
    |---|---|---|---|---|
    | 1 | sube 1 | −6 | 0,5 | ±2 |
    | 2 | baja 1 (y+2) | −3 | 0,5 | ±2 |
    | 3 | sube 2 | −6,31 | 0,3125 | ±1,5 |
    | 4 | baja 2 | −2,75 | 0,25 | ±1 |

    Tras saltar: sprite A8 o B4, IX+11 = 9, +1A = y mínima del nivel de destino (tabla 9C63: 28 48 68 88 A8) y fase 2 [C].
  - **Fase 2, en el aire:** gravedad con vy de caída limitada a 6 px/frame. No aterriza hasta que y ≥ +1A y haya suelo
    bajo los pies. Al aterrizar ajusta y, vuelve a la fase 0, IX+11 = 0 y vx = ±2 [C].
- **Ataques / hijos:** ninguno [C].
- **Comparación con lo observado:** encaja. Paso de 2 px ✓ y moving 0,5 ✓.
- **Dudas:** el significado exacto de E500 = 0 (¿jugador quieto o andando?). No seguí 9D1C ni 9D2D en detalle.

#### 0x25 — armadillo o caracol que se enrosca al recibir golpes (dos sprites, cae entre plataformas)
- **Medido en openMSX** [O]: el segundo sprite va **hacia donde mira** (castillo, sala 9, mirando a la derecha: sprites en x y x + 16; sala 83, mirando a la izquierda: en x y x − 16). Se dibuja aunque quede sobre un muro: en la sala 9 nace medio metido en la pared. El contacto con el jugador abarca los dos sprites.
- **Vida / recompensa:** 8 (se restaura) / `82 ac 0f 08 4e`. **Botín real:** regla 2 (4.ª y 6.ª), nivel 1 [C].
- **Inicio (98C0):** activa el bit 1 de IX+2 (objeto de dos sprites [I]) [C]. +08 = 0..127 y **mira al jugador** (7CA9) [C].
  Gravedad 0x50 (0,3125 px/frame²), +1A = 32..159 [C]. vx = ±**1,125 px/frame** hacia el jugador [C].
  Sprites: a la derecha AC con segundo B4, IX+9 = AA e IX+11 = 4E; a la izquierda B8 con segundo C0, IX+9 = B2,
  IX+11 = 4F y bit 0 de IX+2 [C]. IX+13 = 0F [C].
  En vuelo solo se aplica a y el byte alto de vy [C].
- **Comportamiento (9908)** [C]:
  - **Fase 0, andar:** anima alternando los sprites cada 4 frames (E604 bit 2) [C].
    - Mira el suelo y la pared delante. Un tile **0xD2** a media altura **no** cuenta como pared y pone +14 = 1 [C].
      Los bordes son x∈[0x10,0xD8) [C].
    - Ante un obstáculo, si +1E = 0, intenta saltar el hueco (aterrizaje a dy +16, dx +48 o −32). Si no puede, da la vuelta
      (invierte el bit 0 de IX+2 y +1D) [C].
    - Al acabar +08: tipo de salto 0..2 al azar y fase 1, igual que 0x1F [C].
    - Al acabar +1A: se detiene y pasa a la fase 6 [C].
  - **Fase 1, buscar salto:** igual que 0x1F (sondeos 9A67 y 9CA3 con dx +64/−48) [C].
  - **Salto (9988):** vy = −4,31 si +1E es impar (subir) y −1,81 en los demás casos; vx = ±2; fase 2 [C].
  - **Fase 2, en el aire:** gravedad 0,3125, vy limitada a 6. Al aterrizar vuelve a la fase 0 (SET 4, SET 6, vx = ±1,125) [C].
  - **Fase 6, pausa:** quieto +1A frames (32..159). Luego vuelve a andar [C].
  - **Golpe** (+1C ≠ 0 en las fases 0, 1 o 6, 9960) [C]:
    - Borra los bits 4 y 6 de IX+2 (se para), hace IX+9 XOR 3, lanza el **sonido 0x1F** y se **enrosca un paso**:
      +19 pasa de 3 a 2 con el sprite A0 (tabla 9A61) y +1B = 60; pasa a la fase 3 [C].
    - **Fase 3:** cada golpe nuevo avanza un paso más (sprite A4 y luego A8), con el sonido 0x1F [C].
      Con +19 = 0 pasa a la fase 4. Si pasan 60 frames sin golpes, pasa a la fase 5 [C].
    - **Fase 4:** 60 frames enroscado del todo [C].
    - **Fase 5:** se desenrosca un paso cada 8 frames. Al llegar a +19 = 3 **restaura la vida a 8** y vuelve a andar [C].
- **Ataques / hijos:** no crea nada en este código [C]. Sonido 0x1F al recibir un golpe [C].
- **Comparación con lo observado:** encaja. Paso de 1,16 ✓ (1,125) y "cae entre plataformas" ✓ (saltos de tipo 2 y caídas).
  El hijo observado **0x3D** no sale de este código: probablemente lo crea el código común al morir o al recibir el
  golpe [I].
- **Dudas:**
  - **Quién pone IX+1C (el aviso de golpe)**: no lo he encontrado en los bancos 4-6; estará en la rutina de daño común [I].
  - El significado del tile 0xD2 y de +14 [I].
  - Si el bit 6 de IX+2 (que se borra al enroscarse) es "hace daño al contacto" [I].

---

### Tabla resumen

| Tipo | Nombre | Vida | Movimiento | Velocidades (px/frame) | Ataque / hijos | ¿Encaja con lo observado? |
|---|---|---|---|---|---|---|
| 0x13 | espectro caminante / volador | 4 | aparece, anda 256 f, se desvanece, vuela al azar y reaparece en el suelo | andar 1; volar 2 (16 dir.) | 0x44 al azar (48..127 f) solo si mira al jugador | sí (no se vio el vuelo) |
| 0x14 | emerge del suelo | 4 | oculto 128..191 f, emerge, pasea 128..191 f y se hunde | 1 | ninguno; sonido 8 | **no** (se observó quieto) |
| 0x15 | guardián orbital | 40 | círculo de radio 31 alrededor de un centro que deriva; tras ~1920 f el centro persigue al jugador | centro ≈ 0,5; vel. angular −8..+8 | 0x45 cada 32..95 f | sí |
| 0x16 | aparición embestidora | 24 | parpadea, embiste recto hacia el jugador, sale del área y se teletransporta | 2 (+E045) | 2 × 0x46 en el punto de salida | sí |
| 0x17 | centinela tirador | 5 | patrulla; se para si el jugador está en su nivel | 2 | 0x47 tras 30 f; luego pausa 120 f | sí |
| 0x18 | rodador | 1 | patrulla; solo gira en los fotogramas 0 o 6 de su animación | 0,5 | ninguno | sí |
| 0x19 | murciélagos que se fusionan | 1 / 2 / 4 | deambula; a los 128 f se lanza contra su pareja y se fusionan (→ 0x37 al 3.er nivel) | 1; fusión 4 | 0x48 cada 64 f tras la primera fusión | sí |
| 0x1F | corredor saltarín | 3 | corre, salta huecos, sube o baja de plataforma | 3,625; salto vy −9/−4, g 1 | ninguno | sí |
| 0x20 | embestidor invulnerable | inv. | patrulla lenta; embiste si el jugador está delante a < 80 px en su nivel | 0,25 → 1 | ninguno | sí |
| 0x22 | escupefuego errante | 24 | deambula como 0x19 | 1 | ráfagas de 0x4A cada 4 f durante 64 f; sonido 15 | sí |
| 0x23 | estatua dormida | 64 | despierta cuando el jugador pasa por los dos lados a su altura; avanza lento hacia él | 0,25 | ninguno | sí (no despertó) |
| 0x24 | saltador entre niveles | 3 | pasea; salta 1 o 2 niveles al azar o hacia el nivel del jugador | 2; saltos vy −6…−2,75 | ninguno | sí |
| 0x25 | armadillo que se enrosca | 8 | anda, salta y cae, pausas; al recibir golpes se enrosca y luego recupera la vida | 1,125; salto ±2 | ninguno (0x3D observado sin origen en este código); sonido 0x1F | sí |

### Dudas más importantes
1. **Bug del extractor:** `enemy_ai.py` corta el desensamblado en los `RET cc`. Todos los `extracted/ai/*.asm` están
   incompletos y habría que regenerarlos con el arreglo (detectar la condición sin coma).
2. **Campos del hueco:** IX+0C/0D es la velocidad y, e IX+16/17 una aceleración (gravedad), no la velocidad y como dice el
   contexto. Para 0x24, 0x25, 0x0D, 0x2D y 0x0A solo se aplica a y el byte alto de vy. IX+11 cambia con cada fotograma del
   sprite, así que no parece una recompensa.
3. **Bits de IX+2:** se deduce que el bit 7 es visible y el bit 4 mover. Los bits 5 y 6 parecen colisión o daño, pero no está
   confirmado.
4. **0x25:** no he localizado quién pone IX+1C (el aviso de golpe). Tampoco qué crea el hijo 0x3D.
5. **0x14:** el código dice que pasea a 1 px/frame, pero se observó quieto. ¿Se trazan los enemigos ocultos?
6. **E045** (factor de velocidad en los apuntados de 0x15, 0x16 y 0x19) y **E500** (condición de salto de 0x24): sin identificar.
7. **0x19 y 0x22:** solo se redirigen en los bordes derecho e inferior. Si salen por la izquierda o por arriba, B452 los destruye.

## IA de enemigos — grupo G3 (0x26 0x27 0x28 0x29 0x2A 0x2B 0x2C 0x2D 0x2E 0x2F 0x31 0x32 0x34)

### Notas previas (afectan a todas las fichas)

- **Los .asm de extracted/ai están incompletos.** `enemy_ai.py` deja de seguir el flujo en cualquier `RET cc`
  (la comprobación `op in ('RET','RETI') and ',' not in text` también se cumple con `RET nz`, que no lleva coma),
  así que falta el código posterior a cada retorno condicional (p. ej. 0x26 A4C5-A4E3, 0x27 8171-8174, 8A1B-8A2F).
  He vuelto a desensamblar con una copia corregida en scratchpad/g3/dis.py (sin tocar el proyecto). Todo lo de
  abajo sale de ese desensamblado completo.
- Rutinas comunes que he seguido además de las del contexto [C]:
  - **B0ED** (mover): suma IX+0C/0D a y (IX+4/5) e IX+0E/0F a x (IX+6/7). Por tanto **IX+0C/0D = velocidad y**
    (8.8) e **IX+0E/0F = velocidad x**. Para los tipos 0x24, 0x25, 0x0D, **0x2D** y 0x0A solo suma el byte alto
    de vy (sin fracción). IX+16/17 (B145) se usa como **aceleración y** (B14C: vy += IX+16/17), no como velocidad.
  - **B137** vy = DE · **B13E** vx = DE · **B133** vx = DE y vy = HL · **B129/B12E** vy=0 / vx=0 ·
    **B15F** vx = -vx · **B16F** vy = -vy.
  - **B0D6** animación: DEC IX+8; al llegar a 0, IX+8 = E, IX+0A ^= D, IX+11 ^= B. **B0DD** = solo la parte XOR.
  - **4083** (banco 0, 4155): A = (HL+A) (tabla de bytes). **406B** (4DCF): HL = H·E (8x8 bits).
    **4056** (4DA1): HL = ED00 + (y/8)·32 + x/8 (H=x, L=y) → dirección del tile en el mapa de la sala.
    **406E** (4DC8): igual + `CP 0xB9` con el tile → **carry si tile ≥ 0xBA (sólido)** [I: sólido].
  - **7E25**: velocidad en una de 16 direcciones (A = dir, B = multiplicador): vx = 7E49[dir]·B, vy = 7E69[dir]·B.
    Tabla: dir 0 = derecha, 4 = arriba, 8 = izquierda, 12 = abajo; módulos 1.0 / 0.86 / 0.71 / 0.5 px/frame.
  - **6551**: crea hijo de tipo A en (y = D, x = E) con vx = HL, vy = 0 (vía 65E3; F014 → IX+0E/0F del hijo).
  - **81FB/B18F + B233/B1A3**: velocidad dirigida a un punto (ángulo cuantizado por tabla B274 sobre |dy|,|dx|,
    seno en B374). Módulo ≈ E609/32 px/frame con E609 = E045·8 + base [C]; E045 sin identificar.
  - 65E3 pone a cero IX+12..IX+1F al crear el hueco y pone IX+2 = F0 (o F8) [C].
- **IX+11 no parece ser solo la recompensa**: en 0x28, 0x29, 0x2D, 0x2E y 0x31 el código lo cambia con cada
  frame de animación (tablas indexadas por el sprite IX+0A, o XOR en B0D6/B0DD) [C]. [I] Probablemente es un
  dato del sprite (color/patrón de la 2.ª capa) que además se usa como clase de recompensa.
- **IX+2 bits 4-7** [I]: bit 4 = el bucle aplica la velocidad; bits 5-7 se encienden/apagan juntos al
  "aparecer"/"desaparecer" (0x2A, 0x2E, 0x31): F0 = activo, &0F = oculto/intangible; bit 7 solo = visible pero
  inofensivo; bit 6 = daña al jugador (0x32 lo apaga en el inicio).
- **'moving': 0.5** en casi todos los tipos observados [I]: sugiere que cada enemigo se actualiza un frame de cada
  dos; por eso las velocidades observadas son ~la mitad de las del código.

---

#### 0x26 — fantasma orbitante que dispara (castillo)
- **Vida / recompensa:** 10 / clase 18 (0x12), cat. 1, valor 4. **Botín real:** regla 3 (última viva), nivel 1 [C].
- **Inicio:** RES 4 IX+2 → el bucle común **no** aplica la velocidad; la posición la calcula la IA [C].
  IX+0A = CC (sprite); centro de la órbita IX+14 = x, IX+15 = y; IX+18 = 6 (paso angular); IX+1A = 0x30;
  IX+1F = 8; IX+12 = (rand & 0x2F) + 0x10 → 16..63 frames hasta el primer disparo; IX+1B = 0x10 (no se usa en
  esta IA) [C]. Radio IX+16/17 = 0 y ángulo IX+19 = 0 por el borrado de 65E3 [C].
- **Comportamiento:**
  - Animación: cada 8 frames IX+0A ^= 4 (CC↔C8) [C].
  - Órbita (A92F): ángulo IX+19 += 6 por frame (vuelta completa ≈ 43 frames); y = cy + r·sin(áng),
    x = cx + r·sin(áng ± 0x40) (coseno; el signo depende de IX+1E bit 0, que queda a 0 → +0x40) [C].
    Radio r = byte alto de IX+16/17 [C]. → círculo alrededor del punto de aparición [I].
  - Radio "respira" (IX+1 = fase): fase 0 crece 0x10/frame (1/16 px) hasta 40 px, se queda en 40 px durante
    48 frames (IX+1A) y pasa a fase 1; fase 1 decrece 1/16 px/frame hasta 5 px, se queda 48 frames y vuelve a
    fase 0 [C]. Crecer 5→40 px tarda 560 frames [C].
  - Deriva del centro (A4E4): cada vez que IX+1F llega a 0 (se recarga con 8) suma (IX+0F, IX+0D) como bytes al
    centro y lo limita: cx < 0x40 → 0x30, cx ≥ 0xC0 → 0xD0, cy < 0x48 → 0x50, cy ≥ 0x80 → 0x88 [C].
  - **Código muerto** [C+I]: IX+1F se decrementa dos veces por frame (A45B y A4E4); como siempre empieza el frame
    con valor par (8,6,4,2), el DEC de A45B nunca da 0 y nunca se elige la dirección aleatoria de la tabla A558
    ((+1,0),(0,−1),(−1,0),(0,+1)). La deriva usa entonces los valores iniciales de IX+0D/0F (los que deje el
    creador en F012/F014; en sala probablemente 0 → centro fijo) y se aplica cada 4 frames, no cada 8.
- **Ataques / hijos:** IX+12 llega a 0 → se recarga con 16..63 y crea **tipo 0x4B** en (y+8, x+8) [C]. El 0x4B
  en su inicio se apunta al jugador (B18F con base 0x60 → ≈3 px/frame + E045·0.25) y pide el sonido 0x0F [C].
- **Comparación con lo observado:** encaja: 'vuela', hijos 0x4B ×3 en 359 frames (media 40 frames ≈ 16..63) [I].
  y_range 19: con r creciendo 1/16 px por actualización y a media frecuencia, en 359 frames r ≈ 11 px [I].
- **Dudas:** de dónde salen IX+0D/IX+0F iniciales en una aparición de sala; si la deriva aleatoria muerta es un bug
  original; IX+1B = 0x10 sin uso aquí.

#### 0x27 — ave/murciélago errante que se lanza en picado (world07)
- **Vida / recompensa:** 12 / clase 17 (0x11), cat. 1, valor 2. **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 2 [C].
- **Inicio:** IX+8 = 4 (temporizador de animación); IX+1F = rand & 15 [C]. Fase IX+1 = 0, IX+1A = 0 [C].
- **Comportamiento:**
  - Cada 64 frames (E003 & 0x3F = 0) pide el sonido 6 [C].
  - Sprite: si IX+0A = C4 (sprite del descriptor) no se anima hasta que IX+1C bit 0 = 1; entonces pone IX+0A = C8
    e IX+11 = 0 y ya se anima cada 4 frames (IX+0A ^= 4: C8↔CC) [C]. IX+1C no lo escribe esta IA → [I] lo pone
    otro código (¿despertar/aparecer?).
  - IX+1F cuenta atrás; a 0 → nuevo IX+1F = rand & 15 (0 → 256 frames) e IX+1A ^= 1 [C]. IX+1A = 1 → solo
    deambular; IX+1A = 0 → además puede atacar [C].
  - **Ataque** (solo con IX+1A = 0): si el jugador está en la ventana y−0x30 < jy ≤ y+0x40 y x−0x38 < jx ≤ x+0x38
    y el enemigo tiene y ≥ 0x30 (y+0x58 ≥ 0x88) [C]:
    - Fase 4 (aviso, 32 frames, IX+1B): vx = 0, vy = +3 px/frame y cada frame vy += 0.5 px/frame² hacia la y de
      partida (IX+19) → rebote/oscilación vertical alrededor de su altura [C].
    - Fase 5 (picado): guarda la posición del jugador en IX+1D/IX+1E y fija una velocidad hacia ella con módulo
      ≈ 4 px/frame (+0.25 por cada unidad de E045) [C]. Termina (fase 0, IX+1A = 1 → vuelve a deambular) al llegar
      a ±0x14 px del punto guardado o si y queda fuera de [0x20, 0xA8) o x fuera de [8, 0xF0) [C].
  - **Deambular** (IX+1 = 0..3):
    - 0: dirección aleatoria de 16 (rand & 15) a 1 px/frame; IX+17 = 3..31 frames; pasa a 1 [C].
    - 1: comprueba límites (81B2) y cuenta IX+17; a 0 → vuelve a 0 (nueva dirección) [C].
      81B2 solo detecta x ∈ [0xC8, 0xF0) o y ∈ [0x8F, 0xDF) (con IX+9 bit 2: x ∈ [0xB8,0xF0), y ∈ [0x7F,0xDF))
      [C]; al detectarlo calcula el cuadrante (IX+16 = 2·(y ≥ 0x70) + (x ≥ 0x80)) y pasa a 2.
    - 2: elige al azar una de 3 direcciones hacia el centro (tabla 819E: cuadrante sup-izq 13/14/15,
      sup-der 9/10/11, inf-izq 3/2/1, inf-der 5/6/7) a 1 px/frame durante 24 frames → fase 3 [C].
    - 3: a los 24 frames pasa a fase 1 con IX+17 = 0, así que el DEC siguiente da 255 frames más en la misma
      dirección salvo que vuelva a tocar un límite [C].
- **Ataques / hijos:** ninguno; el daño es por contacto en el picado [I]. Sonido 6 periódico [C].
- **Comparación con lo observado:** 'vuela', 0.35/0.33 px/frame de media (≈ 1 px/frame a media frecuencia y con
  pausas), y_range 51 [I]. toward_player 0.35: el picado es poco frecuente [I]. Encaja.
- **Resuelto:** IX+1C lo pone el código de impacto b03:B54D (el 0x27 está en su lista): despierta con el primer golpe [C].
- **Dudas:** 81B2 no comprueba los bordes izquierdo/superior (¿bug o la sala no lo
  necesita?); E045 (¿dificultad/vuelta?).

#### 0x28 — insecto rebotador de 16 direcciones (world01, world07)
- **Vida / recompensa:** 1 / clase 98 (0x62), cat. 4, valor 8. Inmune con objeto 0x0F. **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 0 [C].
- **Inicio:** IX+15 = (rand & 0x3F) + 7 → 7..70 frames; IX+18 = dirección rand & 15; velocidad 7E25 con
  multiplicador 3 → 3 px/frame en la dirección elegida [C]. Sprite IX+0A = 939C[dir] (8 orientaciones:
  CC D0 D0 D0 C4 DC DC DC D8 E0 E0 E0 C8 D4 D4 D4) e IX+11 = 93CC[(IX+0A−C4)/4] [C].
- **Comportamiento:**
  - Rebote (7DDD): bajando con y ≥ 0xB0 o subiendo con y < 0x28 → vy = −vy, IX+1D = 1; hacia la derecha con
    x ≥ 0xE8 o hacia la izquierda con x < 8 → vx = −vx, IX+1C = 1; cualquiera de los dos pone IX+15 = 16 [C].
    Rebota solo contra los bordes de pantalla, no contra tiles [C].
  - Tras rebotar ajusta la dirección para el sprite: espejo horizontal 93AC, espejo vertical 93BC [C].
  - IX+15 llega a 0 → nueva dirección aleatoria y nuevo tiempo 7..70 frames (16 frames tras un rebote) [C].
  - Sprite e IX+11 se recalculan cada frame según la dirección [C].
- **Ataques / hijos:** ninguno [C].
- **Comparación con lo observado:** 'vuela', y_range 138, step_x 2.04 ≈ 3 px/frame · |cos| medio [I]; encaja.
- **Dudas:** las entradas 4 y 12 de 93AC y 0/8 de 93BC no son espejos correctos, pero no se usan (esa dirección no
  tiene componente en ese eje) [I].

#### 0x29 — burbuja invulnerable que rebota en línea recta (world04)
- **Vida / recompensa:** invulnerable (FF) / clase 14 (0x0E), cat. 0, valor 1. **Botín real:** regla 4 (nunca) [C].
- **Inicio:** E627 = 1 (bandera global) [C]; **recoloca el enemigo al azar**: x = (rand & 0x7F) + 0x40
  (64..191), y = (rand & 0x3F) + 0x40 (64..127) [C]; IX+1F = dirección rand & 15; IX+1E = 1; IX+8 = 1 [C].
- **Comportamiento:**
  - Cada 8 frames (E604 & 7 = 0): IX+0A ^= 0x0C e IX+11 ^= 0x66 (animación) [C].
  - Arranque: IX+8 llega a 0 → IX+1D = 1; con IX+1D = 1 e IX+1E = 1 fija vx/vy en la dirección IX+1F a
    1 px/frame (7E25, B = 1), pone IX+1D = IX+1E = 0 e IX+8 = 16 [C]. IX+1E no se vuelve a poner a 1 en esta IA,
    así que la dirección se elige una sola vez [C].
  - Rebota en los bordes: y < 0x20 subiendo o y ≥ 0xB0 bajando → vy = −vy; x < 8 hacia la izquierda o x ≥ 0xE0
    hacia la derecha → vx = −vx [C].
- **Ataques / hijos:** ninguno [C].
- **Comparación con lo observado:** 'vuela', turns_per_s 0.05 (casi no gira), velocidades 0.21/0.43 → trayectoria
  recta con rebotes a ~1 px/frame a media frecuencia [I]; encaja.
- **Dudas:** significado de E627 y de E604 (¿segundo contador de frames?); si otro código vuelve a poner IX+1E = 1
  (p. ej. al recibir un golpe, ya que es invulnerable).

#### 0x2A — aparición que se teletransporta entre plataformas (world02)
- **Vida / recompensa:** 3 / clase 0, cat. 101, valor 4. Inmune con objeto 0x0E. **Botín real:** regla 3 (última viva), nivel 0 [C].
- **Inicio:** es la misma rutina que el tick de actualización (entra en 969A) [C].
- **Comportamiento** (cada 16 frames, IX+8):
  - Si IX+2 bit 7 = 1 → IX+2 &= 0x0F (se apaga: [I] invisible e intangible) [C].
  - Busca sitio: y aleatoria de la tabla 9720 {0x28,0x48,0x68,0x88,0xA8} (índice (rand&3)+(rand&1), sesgado al
    centro) y x aleatoria en {0x30,0x50,0x70,0x90,0xB0} (repite el sorteo hasta que cae en rango) [C].
  - El sitio vale si los dos tiles de debajo (x, y+16) y (x+8, y+16) son sólidos y los cuatro tiles del cuerpo
    (x,y), (x+8,y), (x,y+8), (x+8,y+8) son libres [C]. Si vale: se teletransporta (x, y) e IX+2 |= 0xF0 (se
    enciende) [C]. Si no vale: se queda apagado hasta el siguiente tick [C].
  - En la práctica: cada 16 frames salta a otro punto sobre una plataforma; si el sorteo falla, desaparece 16
    frames [I]. No usa velocidad [C].
- **Ataques / hijos:** ninguno [C].
- **Comparación con lo observado:** 'quieto', velocidad 0. No encaja literalmente; [I] la medición probablemente
  ignora saltos de posición grandes o no siguió al enemigo entre teletransportes.
- **Dudas:** semántica exacta de IX+2 bits 4-7; si el trazado de enemies.json filtra teletransportes.

#### 0x2B — trepamuros que recorre el contorno de las superficies (world08)
- **Vida / recompensa:** 3 / clase 19 (0x13), cat. 0, valor 6. **Botín real:** regla 3 (última viva), nivel 3 [C].
- **Inicio:** IX+1 = 0 (dirección: derecha con suelo debajo); IX+1C = 0x11; IX+1D = 0; IX+1F = 1;
  IX+0A = IX+8 = C4; IX+1E = C8; carga las sondas de la dirección (6D7F) y velocidad 1 px/frame [C].
- **Comportamiento:**
  - IX+1 = dirección 0..7: 0/2 derecha, 1/3 izquierda, 4/6 abajo, 5/7 arriba, a 1 px/frame [C]. Para cada
    dirección, 6D7F copia de la tabla 88FD (8 bytes por campo) [C]: IX+14/15 = sonda delantera, IX+16/17 = sonda
    lateral (superficie a la que se agarra), IX+18 = dirección al chocar de frente, IX+19 = dirección al perder la
    superficie. Superficie lateral: 0 y 1 suelo, 2 y 3 techo, 4 y 5 pared derecha, 6 y 7 pared izquierda [C].
  - Cada frame: si toca el borde de pantalla en su dirección (x ≥ 0xF0 / x < 8 / y ≥ 0xA8 / y < 0x20) → gira a
    IX+18 e IX+1D = 1 [C].
  - Sonda delantera (2 puntos) sólida → gira a IX+18 (sube por la pared / pasa al techo…) e IX+1D = 0 [C].
  - Tras cada giro (IX+1B = 1) no mira la sonda lateral durante IX+1C frames (17 la primera vez, 16 después) [C].
  - Sonda lateral libre (se acaba la superficie) y IX+1D = 0 → gira a IX+19 (dobla la esquina) [C]. Si IX+1D = 1
    (último giro por borde de pantalla) no dobla esquinas hasta el siguiente choque frontal [C].
  - Animación cada 6 frames entre IX+8 e IX+1E: C4/C8 con vx > 0, CC/D0 con vx < 0 [C].
- **Ataques / hijos:** ninguno [C].
- **Comparación con lo observado:** 'vuela' (se mueve en x e y), 0.35/0.16 px/frame, toward_player 0.27 (no
  persigue) [I]; encaja.
- **Dudas:** IX+1A = 0x0F (copiado de la tabla) no se usa aquí.

#### 0x2C — caminante que se para hasta que desaparece su compañero (world02)
- **Vida / recompensa:** 4 / clase 11 (0x0B), cat. 1, valor 10. **Botín real:** regla 4 (nunca) [C].
- **Inicio:** IX+14 = E637 (índice de instancia; E637++) → estado compartido en E638[IX+14] [C].
  IX+15 = 16..31 frames de espera inicial; RES 4 IX+2 (quieto); IX+1D = 0x1A, IX+1E = 8, IX+1F = 0x20;
  vx = +1 px/frame; sprites IX+16..19 = A0 A4 A8 AC (derecha) o B0 B4 B8 BC (izquierda); IX+0A = IX+16 [C].
- **Comportamiento** (tras la espera inicial, cada frame; al final siempre IX+11 = B0B0[(IX+0A−A0)/4]) [C]:
  - Estado E638[i] = 0 (caminar): activa el movimiento y da un ciclo de paso (AA1C). Al terminar el ciclo
    (IX+1B vuelve a 0) E638[i] ^= 1 → estado 1 [C].
  - Ciclo de paso (AA1C): si no hay suelo delante (x+16 ó x, y+16), hay pared en (…, y+8) o llega a x < 8 /
    x ≥ 0xF0 → da la vuelta [C]. IX+1B cuenta frames: < IX+1E sprite 2.º, < IX+1D sprite 3.º, ≥ IX+1D se para
    (RES 4) con el 4.º sprite; = IX+1F → IX+1B = 0 y 1.er sprite [C]. Normal: anda 26 de cada 32 frames [C].
  - Estado 1: parado (RES 4) [C]. Solo sale si el hueco **siguiente** (IX+0x20) está vacío (tipo 0) [C].
  - Hueco siguiente vacío (en estado 0 o 1) → modo rápido: IX+1A = 1, vx = ±2 px/frame, ciclo IX+1D = 13,
    IX+1E = 6, IX+1F = 17, E638[i] = 2 → camina siempre (estado ≥ 2) [C].
  - [I] Va en pareja con el enemigo del hueco siguiente: da un paso y se queda quieto; al morir el compañero se
    enfurece y corre.
- **Ataques / hijos:** ninguno [C].
- **Comparación con lo observado:** 'camina', moving 0.21, turns 0.08: casi siempre parado → encaja con quedarse
  en el estado 1 mientras vive el compañero [I].
- **Dudas:** la rama "si el tipo no es 0x2C mira el hueco anterior (IX−0x20)" es inalcanzable con esta tabla (solo
  0x2C usa esta rutina) [C]; quién ocupa el hueco siguiente en las salas; si E637 se reinicia por sala.

#### 0x2D — soldado que patrulla, salta huecos y dispara al jugador en su fila (world02)
- **Vida / recompensa:** 2 / clase 12 (0x0C), cat. 0, valor 7. **Botín real:** regla 1 (2.ª y 5.ª), nivel 1 [C].
- **Inicio:** IX+8 = R & 0x7F (tiempo hasta buscar salto); IX+1D = R & 1 (1 = izquierda); vx = ±1.5 px/frame;
  aceleración y IX+16/17 = 0.25 px/frame²; sprite A0 / IX+11 = 0x0C (derecha) o AC / 0x0D (izquierda) [C].
- **Comportamiento** (IX+1 vía tabla de saltos) [C]:
  - **0 patrulla:** animación cada 8 frames (IX+0A ^= 4 ó 0x1C, IX+11 ^= 0x0C/0x0D). Si no hay suelo delante,
    hay pared o borde → intenta saltar el hueco (9889: si IX+1E = 0 y el tile de delante a la altura del cuerpo
    está libre, comprueba aterrizaje a ±36 px, tabla 9AE6) → salto (fase 2); si no, da la vuelta (vx = −vx,
    IX+1D ^= 1, IX+18−−). Con el camino libre: si E500 ≠ 6 y jy − 16 = y (jugador en su misma fila) → fase 5.
    IX+8 llega a 0 → fase 1 con IX+1E = R&3 (3→0) e IX+18 = 6.
  - **5 apuntar:** se para (RES 4), se gira hacia el jugador; a los 30 frames → fase 3, sprite A4/B0, crea
    **tipo 0x3F** en (y, x) con vx = ±4 px/frame en su dirección y pide el sonido 5.
  - **3 tras disparar** (60 frames): si E500 = 6 → **carga** hacia el jugador a ±4 px/frame (fase 4); si se acaban
    los 60 frames → vuelve a patrullar (IX+8 = R&0x7F, vx = ±1.5).
  - **4 carga:** animación cada 4 frames; al quedarse sin suelo, chocar o llegar al borde da la vuelta y vuelve a
    patrullar.
  - **1 buscar salto:** cada frame prueba saltar a otro nivel según IX+1E (tabla 9ADF: 1 → punto a (−16, +32/−16),
    2 → (+16, +40/−24), más el chequeo 9CA3); si puede → fase 2. Si no, patrulla como en 0; cada dos vueltas
    cambia IX+1E (1→2→0); al llegar IX+18 a FF (7 vueltas) se rinde y vuelve a 0.
  - **2 en el aire:** vy inicial −4.5 px/frame (IX+1E impar) o −2.5 (par); vy += 0.25 por frame, cae como máximo
    a 6 px/frame; al tocar sólido bajo los pies (x+4 ó x+12, y+16) ajusta y a múltiplo de 8 y vuelve a patrullar.
    Para este tipo B0ED solo aplica el byte alto de vy [C].
- **Ataques / hijos:** 0x3F (proyectil horizontal ±4 px/frame que muere al tocar un tile sólido; sprite B4 si se
  dispara a la izquierda) [C]. Sonido 5 al disparar [C].
- **Comparación con lo observado:** 'camina', 0.51 px/frame, toward_player 0.62, hijos 0x3F ×2 → encaja [I].
  y_range 0: no se le vio saltar [I].
- **Dudas:** E500 = 6 (¿jugador agachado/atacando/en escalera?); detalle de 9CA3/9C68 (comprobación de aterrizaje)
  sin seguir a fondo; IX+11 cambia con la animación.

#### 0x2E — brote/llama intermitente que crece y se esconde (castillo)
- **Vida / recompensa:** 2 / clase 38 (0x26), cat. 3, valor 3. Inmune con objeto 0x17. **Botín real:** regla 0 (1.ª y 3.ª muerte), nivel 0 [C].
- **Inicio:** IX+8 = rand (1..256 frames de espera); IX+1 = 0; IX+1E = FF; IX+2 &= 0x0F (oculto) [C].
- **Comportamiento:**
  - Fase 0: espera IX+8 → fase 1 y primer paso [C].
  - Cada paso: IX+2 |= E0; IX+1E++; sprite e IX+11 de la tabla A0D1 (C4/26, C8/27, CC/28, D0/29, D4/29, D0/29,
    CC/28, C8/27, C4/26) [C]. Pasos 0, 4 y 8 duran 16 frames, el resto 4 [C]. En los pasos 0 y 8 IX+2 &= 0x8F
    (solo bit 7: [I] visible sin dañar); en 1..7 bits 5-7 activos ([I] daña) [C].
  - Paso 9 → reinicia (oculto, nueva espera aleatoria) [C]. Ciclo visible: 16+4·3+16+4·3+16 = 72 frames [C].
  - No se mueve [C].
- **Ataques / hijos:** ninguno en el código [C].
- **Comparación con lo observado:** 'quieto' → encaja. El hijo 0x1D observado no lo crea esta IA [C]; [I] será de
  otro enemigo de la misma sala.
- **Dudas:** semántica exacta de los bits de IX+2.

#### 0x2F — bloque que imita los saltos del jugador (world01, world03)
- **Vida / recompensa:** 5 / clase 0, cat. 101, valor 4. **Botín real:** regla 2 (4.ª y 6.ª), nivel 0 [C].
- **Inicio:** IX+1 = 0 si E515 = 0 o E513 bit 7 = 1; si no, IX+1 = E515+1. IX+1F = y (altura de reposo);
  vx = 0 [C].
- **Comportamiento** (E512/E513, E515 y E50A son variables del jugador; [I] E512/13 = vy del jugador, E515 = 1
  mientras está en el aire o saltando):
  - Si E515 ≠ 1: si vy < 0 la invierte; fase 0; cuando y ≥ IX+1F se recoloca en IX+1F y vy = 0 [C] → vuelve a su
    altura [I].
  - E515 = 1, fase 0: cuando E513 bit 7 = 1 (jugador subiendo) → sonido 3 y fase 1 [C].
  - Fase 1: si E50A ≠ 0 copia vy = E512/E513 (la del jugador); si E50A = 0 → vy = 0 [C]. Choca con el techo
    (4074 con DE = 0x000F) → y = (y & F8) + 8, vy = 0, fase 2; y < 8 → fase 2; si E513 = 0 → fase 3 [C].
  - Fase 2: espera E513 = 0 → fase 3 [C].
  - Fase 3: vuelve a copiar la vy del jugador; al llegar a y ≥ IX+1F se recoloca, fase 0 (y si el jugador ya sube
    otra vez, fase 1) [C].
  - Resultado [I]: sube y baja a la vez que el jugador salta, desde su altura de reposo, sin moverse en x.
- **Ataques / hijos:** ninguno; sonido 3 al empezar a subir [C].
- **Comparación con lo observado:** 'quieto' → compatible si el jugador no saltó durante la captura [I].
- **Dudas:** significado exacto de E50A, E512/E513 y E515; rutina 4074 (54FF, colisión con dos tiles) no seguida
  del todo.

#### 0x31 — pinchos/estalagmita que emergen del suelo o del techo (castillo)
- **Vida / recompensa:** invulnerable (FF) / clase 27 (0x1B), cat. 4, valor 1. Inmune con objeto 0x22. **Botín real:** regla 4 (nunca) [C].
- **Inicio:** IX+2 &= 0x0F (oculto); IX+18 = 3; IX+8 = rand (1..256 frames) [C].
- **Comportamiento:**
  - Fase 0: espera IX+8; mira el tile (x, y+16): si es sólido IX+17 = 1 → tabla 8C05 (00 C4 C8 CC D0 CC C8 C4 00);
    si no → tabla 8C0E (00 D4 D8 DC E0 DC D8 D4 00) [C]. [I] Crece desde el suelo o cuelga del techo. Pasa a
    fase 1.
  - Fase 1: 8 etapas (IX+16 = 0..7) de 32 frames; en cada una alterna cada 3 frames entre el sprite de la etapa y
    el de la siguiente [C]. Sprite 00 → IX+2 &= 0x0F (oculto); C4/D4 → solo bit 7 ([I] visible sin dañar); el
    resto → IX+2 |= C0 ([I] daña) [C]. IX+11 = tabla 8C17 según el sprite [C].
  - Tras la etapa 7 → oculto, fase 2: 32 frames y vuelve a fase 1 (el valor aleatorio de IX+8 en 8B81 se
    sobrescribe con 0x20) [C]. Ciclo fijo de 288 frames tras el desfase inicial aleatorio [C].
- **Ataques / hijos:** ninguno [C].
- **Comparación con lo observado:** 'quieto' → encaja.
- **Dudas:** semántica de los bits de IX+2.

#### 0x32 — estatua/torreta que dispara proyectiles (castillo; compartida con 0x51)
- **Vida / recompensa:** invulnerable (FE) / clase 121 (0x79), cat. 5, valor 101. **Botín real:** regla 4 (nunca) [C].
- **Inicio:** IX+8 = 30; IX+15 = 30; RES 6 IX+2 ([I] no hace daño por contacto) [C].
- **Comportamiento:**
  - Fase 0: cada 30 frames comprueba la zona; si el jugador está dentro → fase 1 [C].
  - Zona [C]: 1 ≤ jy − y ≤ 32 y, para 0x32, d = x − jx en [16, 48) o [64, 96) (jugador a la izquierda); para
    0x51, d = jx − x en [40, 72) o [88, 120) (jugador a la derecha).
  - Fase 1: si el jugador sale de la zona → fase 0 (IX+8 = 30). Si sigue dentro: IX+15 cuenta; a 0 → nuevo IX+15
    = (R & 0x1F) + 1 (1..32 frames) y dispara [C].
- **Ataques / hijos:** **tipo 0x33**; 0x32 lo crea en (y+4, x+16) con vx = +4 px/frame; 0x51 en (y+4, x) con
  vx = −4 px/frame [C]. 0x33 pide el sonido 0x10 al nacer, usa el sprite C8 si va a la izquierda y muere al tocar un
  tile sólido [C].
- **Comparación con lo observado:** 'quieto' → encaja; no se registraron hijos (el jugador no entraría en la zona)
  [I].
- **Dudas:** **dispara hacia el lado contrario al jugador** (0x32 comprueba el jugador a la izquierda y dispara a
  la derecha; 0x51 al revés) [C]. ¿Zonas "de rebote", sprite/coordenadas espejadas o error de lectura del signo
  de E507? Merece comprobarlo en el emulador.

#### 0x34 — bloque/cabeza que sube y baja más rápido cuanto más cerca está el jugador (castillo)
- **Vida / recompensa:** 4 / clase 34 (0x22), cat. 3, valor 8. **Botín real:** regla 1 (2.ª y 5.ª), nivel 2 [C].
- **Inicio:** vy = +1 px/frame y ejecuta ya una actualización (entra en AAFA) [C].
- **Comportamiento** (solo eje y; vx no se toca) [C]:
  - Velocidad = término x + término y. Término x por |jx − x|: ≥ 64 → 1.0; 40..63 → 1.5; 24..39 → 2.0;
    < 24 → 2.5 px/frame. Término y por |jy − y|: ≥ 128 → 1.0; 96..127 → 1.5; 64..95 → 2.0; 32..63 → 2.5;
    < 32 → 3.0 px/frame. Total 2.0 a 5.5 px/frame [C].
  - IX+14 bit 0 = sentido (1 = arriba). Mira el tile (x, y+8) bajando o (x, y) subiendo; si es sólido invierte vy
    y IX+14 [C].
  - Si y < 0x20: IX+14 = 0 (hacia abajo) y vy = solo el término x (1.0..2.5) [C].
- **Ataques / hijos:** ninguno; daño por contacto [I].
- **Comparación con lo observado:** 'vertical', speed_x 0, speed_y 1.88 de media (≈ 3.8 por actualización a media
  frecuencia) → encaja [I].
- **Dudas:** la sonda a y+8 al bajar (y no a y+16) hace que penetre medio tile en el suelo [I]; ¿el sprite es de
  8 px de alto o está pensado así?

---

### Tabla resumen

| Tipo | Nombre | Movimiento | Reacción al jugador | Hijos / sonido | ¿Encaja con lo observado? |
|---|---|---|---|---|---|
| 0x26 | fantasma orbitante | órbita (6/256 de vuelta por frame), radio 5↔40 px que respira; centro casi fijo | ninguna (dispara a ciegas) | 0x4B apuntado cada 16..63 fr | sí |
| 0x27 | ave en picado | 16 direcciones a 1 px/fr, corrige hacia el centro | ventana ±0x38 x, −0x30/+0x40 y → aviso 32 fr + picado ~4 px/fr | sonido 6 cada 64 fr | sí |
| 0x28 | insecto rebotador | 16 direcciones a 3 px/fr, rebota en bordes, cambia cada 7..70 fr | ninguna | — | sí |
| 0x29 | burbuja invulnerable | aparece al azar, línea recta 1 px/fr con rebotes | ninguna | — | sí |
| 0x2A | aparición teletransportadora | salta cada 16 fr a un hueco sobre plataforma | ninguna | — | no ('quieto'; ¿teletransportes filtrados?) |
| 0x2B | trepamuros | recorre suelo/paredes/techo a 1 px/fr | ninguna | — | sí |
| 0x2C | caminante emparejado | 1 ciclo de paso y se para; ±2 px/fr si el hueco siguiente se vacía | ninguna | — | sí (casi siempre parado) |
| 0x2D | soldado saltador | patrulla ±1.5 px/fr, salta huecos y entre niveles | misma fila → para 30 fr y dispara; E500 = 6 → carga a 4 px/fr | 0x3F ±4 px/fr, sonido 5 | sí |
| 0x2E | brote intermitente | fijo; ciclo de 9 sprites (72 fr) tras espera aleatoria | ninguna | — (0x1D observado no es suyo) | sí |
| 0x2F | imitador de saltos | sube y baja copiando la vy del jugador | salta cuando salta el jugador | sonido 3 | compatible |
| 0x31 | pinchos emergentes | fijo; 8 etapas de 32 fr + 32 oculto (desde suelo o techo) | ninguna | — | sí |
| 0x32 (=0x51) | torreta | fija | zona a 1..32 px por debajo y franjas de x → dispara cada 1..32 fr | 0x33 ±4 px/fr (sonido 0x10) | sí (sin disparos vistos) |
| 0x34 | bloque vertical | solo y, rebota en tiles, 2.0..5.5 px/fr | más rápido cuanto más cerca está el jugador | — | sí |

## IA de enemigos — grupo G4 (0x36 0x3A 0x3C 0x40 0x42 0x43 0x49 0x4C 0x4E 0x50 0x51 0x64 0x69)

### Notas previas (válidas para todo el grupo)

- **Los .asm de extracted/ai están incompletos.** `enemy_ai.py` trata `RET nz`/`RET c`… como RET
  incondicional (la prueba `',' not in text` falla porque «RET nz» no lleva coma) y corta el flujo
  tras cada retorno condicional. Por eso, p. ej., 3a.asm termina en `9169 RET nz` sin la lógica de
  detección. He re-desensamblado todo con un trazador corregido (copia en scratchpad/g4/dis.py, sin
  tocar el proyecto). También interpreta como `.word` código que sigue a la tabla de saltos (AE65 en
  0x3C) [C].
- **Velocidad y = IX+0C/0D, no IX+16/17.** b06:B0ED (llamada por B407 si IX+2 bit 4) suma IX+0C/0D
  a IX+4/5 (y con subpíxel en +4) e IX+0E/0F a IX+6/7 (x) [C]. B133 pone vx = DE y **vy (+0C) = HL**;
  B124 pone ambas a 0; B137 pone vy = DE. B145 escribe +16/17, y B14C suma +16/17 a +0C/0D: +16/17
  es una **aceleración y** (gravedad) para quien llame a B14C, no la velocidad [C]. El contexto y
  docs/items_enemies.md lo describen mal.
- **IX+2 nibble alto**: al crear el enemigo vale F0 o F8 (b04:6645) [C]. El bit 4 = «aplicar velocidad»
  (B411) [C]. Varios tipos borran/ponen el nibble entero (96EE/96F5) para desaparecer y quedar
  quietos; el bit 7 parece «visible/colisionable» [I].
- **Apuntar al jugador**: B18F/B195 (A = rapidez S) apunta al punto (jugador_y−16, jugador_x−8); 81FD
  apunta a (H, L). Usan la tabla de ángulos b06:B274 (16×16) y la de senos B374 (64 valores, máx 0xFF).
  Módulo ≈ (S + 8·E045)/32 px/frame [C] (4DCF es un producto 8×8 y B262 divide entre 32). E045 no
  identificado; con E045 = 0: S = 0x10 → 0,5; 0x40 → 2; 0x60 → 3; 0x80 → 4; 0xC0 → 6 px/frame.
- **Direcciones de 16 rumbos** (7E25, B = multiplicador): tablas 7E49 (vx) y 7E69 (vy): rumbo 0 = derecha,
  4 = arriba, 8 = izquierda, 12 = abajo, módulo 1,0 px/frame × B [C].
- **Crear hijos**: 6551 (A = tipo, D = y, E = x, HL = vx, vy = 0, B → E624 = parámetro) y 65DA (igual
  con velocidades 0) [C]. Varios tipos llaman a 6551 sin cargar HL: la vx es basura, pero el inicio
  del hijo la sobrescribe en todos los casos vistos [C].
- **4DC8** (vec_4dc8, H = x, L = y): lee el tile en ED00 + (y/8)·32 + x/8 y deja carry si el tile es
  > 0xB9 (sólido, [I]) [C].
- **B0D6/B0DD** (animación): cuenta IX+8 y al llegar a 0 hace IX+0A ^= D e **IX+11 ^= B**. B06C
  recalcula IX+11 desde el fotograma (tabla B0B0). IX+11 («recompensa» en el descriptor) cambia con el
  fotograma en varios tipos [C]; puede que sea también la caja de colisión [I].
- E604 se usa como contador global de frames (AND 7, AND 15…) [I]. E620 = enemigos vivos [C]
  (B42D lo decrementa al borrar). Un enemigo se borra si y ≥ 0xB8 o x ≥ 0xF9 (B452) [C].

---

#### 0x36 — orbes en cadena que saltan de un sitio a otro
- **Vida / recompensa:** invulnerable (FE); clase 18 (cat. 1, nivel 4). Mundo 5, 16 generadores. **Botín real:** regla 4 (nunca) [C].
- **Inicio (8A39):** IX+2 bit 2 = 1; IX+8 = 1..4 (desfase aleatorio de la animación); IX+18/19 =
  posición inicial y/x («casa») [C]. vx = 0, vy = +2,0 px/f (B133 con DE = 0, HL = 0x200) [C].
  IX+14 = índice de la instancia (contador global E633, que se incrementa); IX+0B = índice+2 [C].
  Si índice+2 = 10 (la **9.ª instancia**, índice 8): velocidades a 0 y estado 1 [C]. E634 = 8 [C].
- **Comportamiento** (update 8A7A). Cada frame, B0D6 con DE = 0x0C04, B = 3: cada 12 frames
  IX+0A ^= 4 e IX+11 ^= 3 [C].
  - Estado 0 (en reposo): muelle vertical; vy += +0,5 px/f² si y ≤ casa_y, −0,5 si y > casa_y
    (821C suma a +0C/0D) [C]. Con vy inicial 2 → oscila unos ±5 px alrededor de la casa, periodo
    ≈ 16 frames [I].
  - Estado 1 (dura 1 frame): elige un índice aleatorio 0..7 distinto de E634; E634 = ese índice;
    estado 2; busca entre los **8 primeros huecos** la 0x36 con IX+14 = ese índice (8B14) y copia su
    casa a IX+15 (y) / IX+16 (x) como destino; apunta hacia allí a rapidez 0xC0 (≈ 6 px/f con E045 = 0)
    [C].
  - Estado 2 (vuelo en línea recta): al entrar en la caja destino (y ∈ (dy−24, dy+24], x ∈ (dx−18,
    dx+18]) → sonido 0x0C, estado 0, vy = +2, vx = 0, posición = destino exacto, casa = destino; y la
    0x36 que vivía en ese destino (índice E634) pasa a estado 1 [C].
  - Resultado: siempre hay una sola en vuelo; la que llega «empuja» a la ocupante, que sale hacia otra
    casa al azar. Una cadena de relevos que no termina nunca [I].
- **Ataques / hijos:** ninguno. Sonido 0x0C al aterrizar [C].
- **Comparación con lo observado:** «vuela», 9 instancias (= índices 0..8), y_range 136 con poca velocidad
  media (0,81): encaja con saltos rápidos y raros entre posiciones fijas, con reposo oscilante [I].
- **Dudas:** E633 (contador de instancias) no se pone a 0 aquí, se supone que al entrar en la sala.
  Si hay menos de 9 instancias, ninguna llega a estado 1 y todas se quedan oscilando [C]. Si el índice
  elegido no existe, 8B14 deja IX = E900 (hueco 8) y se leen datos ajenos [C]. La casa de la 9.ª
  instancia nunca es destino (sólo 0..7) [C]. E045 sin identificar.

#### 0x3A — boca/cañón de pared que dispara ráfagas a la derecha
(rutinas compartidas con 0x50; inicio 9159, update 9160)
- **Vida / recompensa:** invulnerable (FE); clase 0 (cat. 101). Castillo, 3 generadores. **Botín real:** regla 4 (nunca) [C].
- **Inicio:** IX+8 = 60; IX+2 bit 6 = 0 [C].
- **Comportamiento:**
  - Estado 0 (cerrado): DEC IX+8; sólo comprueba cuando llega a 0. Si falla, IX+8 da la vuelta y la
    siguiente comprobación es **256 frames después** [C]. Condición: el jugador está debajo
    (0 < jugador_y − y ≤ 32) **y a la derecha** (x < jugador_x), sin límite de distancia en x [C].
    Si se cumple: estado 1, IX+0A++ (fotograma «abierto»), IX+8 = 120 [C].
  - Estado 1 (abierto, 120 frames): cada 8 frames de E604, si el jugador sigue a la derecha (sin
    mirar y) dispara 0x3B en (y+4, x+16) [C]. Al acabar: estado 0, IX+8 = 120, IX+0A−− [C]. Como mucho
    15 disparos por apertura [I].
- **Ataques / hijos:** 0x3B: inicio 91E0, apunta al jugador desde una y desplazada al azar
  (y + (E604 & 31) − 8) a rapidez 0x60 (≈ 3 px/f), sonido 0x12; su update es un RET (va en línea
  recta) [C].
- **Comparación con lo observado:** «quieto», sin hijos: encaja si el jugador nunca entró en la franja
  de activación durante la traza [I].
- **Dudas:** E604 no confirmado como contador de frames. Significado de IX+2 bit 6.

#### 0x3C — hechicero que se teletransporta y dispara
- **Vida / recompensa:** 7; clase 19 (cat. 0, nivel 6). Castillo. **Botín real:** regla 3 (última viva), nivel 1 [C].
- **Inicio (AE4C):** IX+16 = 32 (cadencia de disparo), IX+17 = 6 (animación), IX+8 = 64 [C].
- **Comportamiento** (AE59, tabla de saltos con 3 casos):
  - Estado 0 (visible, 64 frames): cada 6 frames IX+0A ^= 4. Cada 32 frames (IX+16) dispara. Al
    acabarse IX+8: IX+8 = 8, estado 1 [C].
  - Estado 1 (parpadeo, 8 frames): alterna cada frame el nibble alto de IX+2 (F0 ↔ 0). Al final lo
    deja a 0 (invisible e inmóvil [I]) y pasa a estado 2 [C].
  - Estado 2 (teletransporte): 96AB elige una fila al azar de la tabla 9720 (y = 0x28, 0x48, 0x68,
    0x88, 0xA8; índice = (r&3)+(r&1), así que las filas centrales salen más) y una columna x ∈ {0x30,
    0x50, 0x70, 0x90, 0xB0} [C]. Acepta si los dos tiles bajo los pies (y+16, x y x+8) son sólidos y
    los 4 tiles del cuerpo (16×16) están libres; si no, lo vuelve a intentar al frame siguiente [C].
    Al acertar: posición nueva, nibble alto = F0, IX+8 = 64, estado 0 [C].
  - Ciclo ≈ 64 + 8 + ≥1 frames; unos 2 disparos por aparición [I].
- **Ataques / hijos:** 0x3D en (y+8, x+8): inicio AEC4, apunta al jugador a rapidez 0x40 (≈ 2 px/f),
  sonido 0x0F, sin update (línea recta) [C].
- **Comparación con lo observado:** «quieto», hijos 0x3D ×5: encaja (no tiene velocidad, sólo se
  teletransporta) [C/I].
- **Dudas:** qué hace exactamente cada bit del nibble alto de IX+2 (visible, colisión, movimiento).

#### 0x40 — espíritu grande que persigue lentamente y lanza proyectiles
- **Vida / recompensa:** 35; clase 57 (cat. 3, nivel 14). Mundo 8, 1 generador. **Botín real:** regla 3 (última viva), nivel 3 [C].
- **Inicio (8D04):** IX+15 = 120; 8D61; IX+8 = 60; apunta al jugador a rapidez 0x10 (≈ 0,5 px/f) [C].
- **Comportamiento** (8D0D). Cada frame, 8D61: IX+0A = 4 ó 5 según E604 bit 3 (animación de 8 frames);
  IX+12 = y+10, IX+13 = x+13 [C].
  - Estado 0 (persigue, 120 frames): si sale de la caja x ∈ [0x10, 0xE0], y ∈ [0x20, 0xA0], invierte vx y
    vy [C]. Si no, cada 60 frames vuelve a apuntar al jugador a 0,5 px/f [C]. Al acabar IX+15:
    estado 1, IX+2 bit 4 = 0 (se para), IX+15 = 30 [C].
  - Estado 1 (parado, 30 frames): al acabar, estado 0, bit 4 = 1, IX+15 = 120 y **crea un 0x41** en
    (y, x) [C].
  - Atraviesa paredes: no mira tiles [C].
- **Ataques / hijos:** 0x41 cada ≈150 frames. Inicio 8D7F: usa B1DB (no seguido) con DE = 0x0808 para
  sacar un rumbo, que redondea a 8 direcciones; de la tabla 8DC5 saca fotograma, vy, vx, desplazamiento
  de posición e IX+11; sonido 0x19; update RET [C].
- **Comparación con lo observado:** toward_player 1,0 encaja. Lo observado (speed 0,21, «camina»,
  y_range 10) es más lento y más plano que 0,5 px/f en diagonal: puede que en la traza E045 o el
  ángulo lo reduzcan, o que el jugador estuviera a su altura [I].
- **Dudas:** para qué sirven IX+12/13 (y+10, x+13); ¿punto de colisión o de origen de disparo? B1DB.

#### 0x42 — monstruo del agua que emerge y dispara
- **Vida / recompensa:** 10; clase 60 (cat. 3, nivel 5). Castillo. **Botín real:** regla 3 (última viva), nivel 3 [C].
- **Inicio (838D, también se reusa al reiniciar):** E603 = 1; vx = vy = 0; IX+16 = 0xA0; E628 = 0;
  x = tabla 83BE[r&3] = {0x30, 0x60, 0x80, 0x90}; vy = −0,5 px/f; nibble alto de IX+2 = 0
  (oculto e inmóvil [I]); IX+14 = 32 [C].
- **Comportamiento** (83C2):
  - Estado 0 (oculto): si el jugador está en el agua (E511 = 1, véase player.md), reinicia IX+14 = 32
    y espera. Si no, cuenta 32 frames; luego nibble alto = F0 (aparece y se mueve) y estado 1 [C].
  - Estado 1 (sube a 0,5 px/f): al llegar a y = 0x94 → IX+14 = 1, E628 = 0, se para, estado 2, y crea
    **0x60** en y = 0xA0, x = jugador_x + 8 o − 16 (+8 si jugador_x < 0x30, −16 si ≥ 0xC8; si no, al
    azar) [C].
  - Estado 2 (fuera): si E628 ≠ 0 (el 0x60 terminó) o el jugador entra en el agua → vy = +0,5,
    estado 3 [C]. Si no, y sólo si jugador_y < 0x94: cada 32 frames (el primero en 1 frame) crea
    **0x61** en (y, x+16) [C].
  - Estado 3 (se hunde): al llegar a y = 0xA0, IX+4 = 0, estado 0 y vuelve a inicio (x nueva) [C].
- **Ataques / hijos:** 0x61: apunta al jugador a ≈ 2 px/f, sonido 0x0F [C]. 0x60: chorro o columna de
  agua; sube a 4 px/f creciendo por bloques de 16 px (IX+9 hasta &7 = 7), baja a 4 px/f, pone E628 = 1
  al pasar por y = 0x90 y se borra en y = 0xA0; sonido 0x17 [C].
- **Comparación con lo observado:** «vertical», y_range 12 (= 0xA0 − 0x94) e hijos 0x61: encaja [C].
- **Dudas:** E603 (lo ponen 0x42 y 0x43). IX+16 = 0xA0 no se usa en esta rutina. ¿Alguien llama a B14C?

#### 0x43 — criatura ancha que patrulla y suelta un rayo vertical
- **Vida / recompensa:** 10; clase 65 (cat. 4, nivel 4). Mundo 9. **Botín real:** regla 3 (última viva), nivel 3 [C].
- **Inicio (84D5):** E603 = 1; IX+14 = 1; IX+12 = 0xD8, IX+13 = 3 (¿segundo sprite?) [I]; 8623 [C].
- **8623 (empezar a andar):** `LD DE,0x200 / CALL 81F1 / AND 1 / LD D,0xFE`: el aleatorio no se usa y
  vx = **−2 px/f siempre** (izquierda) [C]. Luego comprueba el borde (8630).
- **Comportamiento** (84E8):
  - Estado 0 (patrulla): cada frame IX+0A ^= 4 e IX+12 ^= 4 (IX+14 vuelve a 1) [C]. Si el jugador
    está a la derecha a ≤ 32 px (x < jx ≤ x+32), **sin mirar y**: se para (vx = vy = 0), estado 1 y
    crea **0x62** en (y, x+14) [C]. Si no: invierte vx si x ∉ [6, 0xE4); y 804C/806F: invierte si
    no hay suelo delante (tile en y+16, x+32 hacia la derecha o x hacia la izquierda), si hay pared
    (tile en y+8, x+24 o x−8) o si toca el borde (x < 0x10 yendo a la izquierda, x ≥ 0xE0 a la derecha)
    [C].
  - Estado 1: espera hasta que E620 = 1 (que sólo quede él vivo, así que el 0x62 ha desaparecido);
    entonces estado 0 y 8623 (otra vez hacia la izquierda) [C].
- **Ataques / hijos:** 0x62: inicio 8544; columna o rayo vertical a 4 px/f hacia arriba, o hacia abajo
  si el jugador está más abajo (o y < 0x30); se para en y < 0x20 o y ≥ 0xB0 y crece por segmentos;
  sonido 0x17 [C].
- **Comparación con lo observado:** «camina», step_x 2,0, speed 1,0 (50 % del tiempo en movimiento):
  encaja con 2 px/f. No se observaron hijos [C/I].
- **Dudas:** el `AND 1` sin uso en 862B: ¿falta un salto o es un bug del original? Sólo detecta al
  jugador por la derecha. Si hay otros enemigos en la sala, E620 nunca baja a 1 y se queda parado
  para siempre [I].

#### 0x49 — insecto/mosca que se divide al ser golpeado
- **Vida / recompensa:** 5; clase 106 (cat. 4, nivel 0). Mundo 3. **Botín real:** regla 4 (nunca) [C].
- **Inicio (A0E3):** IX+12 = 0xBC, IX+13 = 1 (segundo sprite [I]), IX+14 = 1, IX+2 bit 5 = 0, IX+15 = 1,
  vx = vy = 0, E63E = 1 [C].
- **Comportamiento** (A109, compartido con 0x52/0x53):
  - Cada 8 frames: IX+0A ^= 0x0C, IX+11 ^= 1, IX+12 ^= 0x7C (aleteo); cada 16 frames, sonido 0x0A [C].
  - Rebota en la caja y ∈ [0x20, 0xB0), x ∈ [8, 0xE0) invirtiendo la componente (930E) [C].
  - Cada 16..31 frames (IX+15 = 16 + r&15) toma uno de 16 rumbos al azar a 2 px/f [C].
  - Tras el primer frame pone IX+2 bit 5 (se puede golpear [I]).
  - Si bit 5 está puesto e IX+1C ≠ 0: crea dos 0x52 en su posición (con E624 = 1 y 0) y se borra; un
    0x52 hace lo mismo con dos 0x53; el 0x53 no se divide [C].
- **Ataques / hijos:** 0x52 ×2, después 0x53 ×2. Sonido 0x0A periódico [C].
- **Comparación con lo observado:** «vuela», step 1,41, 1 giro/s, y_range 120: encaja con 2 px/f y
  cambios de rumbo cada 0,3–0,5 s [I].
- **Resuelto:** IX+1C lo escribe b03:B54D (ver «Quién escribe IX+1C»), así que se divide al recibir un golpe [C].
- **Dudas (antiguas):** quién escribe IX+1C. No hay ningún `LD (IX+1C)` fuera de la IA, así que será el código
  de daño con otro direccionamiento [I]. La interpretación «se divide al recibir un golpe» depende de
  eso.

#### 0x4C — nube/criatura errante que suelta esporas cada 16 frames
- **Vida / recompensa:** 20; clase 61 (cat. 4, nivel 2). Mundo 5. **Botín real:** regla 3 (última viva), nivel 2 [C].
- **Inicio (8FCA):** IX+15 = 16, IX+8 = 8 [C].
- **Comportamiento** (8FD3): cada 8 frames IX+0A ^= 1. Cada 16 frames (IX+15) crea **0x4D** en
  (y+16, x+16) y no ejecuta el movimiento ese frame [C]. Si no, el deambulador común 8145
  (compartido con 0x4E/0x4F):
  - Est. 0: rumbo aleatorio de 16, a 1 px/f; IX+17 = 3..31 frames; estado 1 [C].
  - Est. 1: cuenta IX+17 (al acabar, vuelve al estado 0) y comprueba la caja 81B2. Sprite pequeño
    (IX+9 bit 2 = 0): fuera si x < 0x10, x ≥ 0xD8 o y ∈ [0x8F, 0xDF). Grande: fuera si x ∈ [0xB8, 0xF0)
    o y ∈ [0x7F, 0xDF) [C]. Si sale: cuadrante IX+16 (0..3 según y ≥ 0x70, x ≥ 0x80), IX+17 = 2,
    estado 2 [C].
  - Est. 2: estado 3, IX+17 = 24 y uno de 3 rumbos hacia el centro (tabla 819E: {13,14,15}, {9,10,11},
    {3,2,1}, {5,6,7}) a 1 px/f [C].
  - Est. 3: tras 24 frames, estado 1 con IX+17 = 0, así que no cambia de rumbo hasta 255 frames
    después o hasta volver a salirse [C].
- **Ataques / hijos:** 0x4D: inicio 8FF7 y update 900B, ondula cambiando de rumbo cada 8 frames a
  1 px/f (tabla 9064) [C].
- **Comparación con lo observado:** «vuela», step 0,73, hijos 0x4D ×11: encaja [C/I].
- **Dudas:** la caja no tiene límite izquierdo ni superior efectivos (dependen del desbordamiento) [C].

#### 0x4E — almas/fuegos fatuos que se reúnen en el centro
- **Vida / recompensa:** vida 0 (descriptor); IX+2 &= 0x9F (bits 5 y 6 a 0 → ¿no golpeable?) [C/I]. **Botín real:** regla 4 (nunca) [C].
  Castillo, 36 generadores.
- **Inicio (822A):** IX+1A = 2, IX+0A = 0xA0, IX+18 = 112, IX+19 = r&15 [C].
- **Comportamiento** (8247): cada 2 frames, IX+0B = tabla 82E4[IX+19] (ciclo de colores de 21 pasos)
  [C].
  - Estados 0–3: el deambulador 8145 (como 0x4C, 1 px/f) [C]. Además, si el jugador está en un punto
    concreto de la sala: x ∈ [0xC0, 0xCF], y = 0x58 en la sala 0x6E; x ∈ [0x60, 0x6F], y = 0x58 en
    la 0x19; x ∈ [0x60, 0x6F], y = 0x78 en las demás. Ahí cuenta IX+18 (112 frames); si sale, se
    reinicia [C]. Al llegar a 0: la primera vez (E63D = 0) suena 0x83 y llama a 4086 (→5D12, no
    seguido) con A = 0x88, y pone E63D = 1. Después, estado 4 y vuela hacia (0x70, 0x80) a rapidez
    0x80 (≈ 4 px/f) [C].
  - Estado 4: al entrar en y ∈ [0x66, 0x7A), x ∈ [0x76, 0x8A) se borra. Si E620 = 0 (era el último),
    crea **0x4F** en (0x70, 0x80) [C].
- **Ataques / hijos:** 0x4F (entidad grande que también deambula con 8145) [C].
- **Comparación con lo observado:** «vuela», 12 instancias, step 0,7: encaja con 1 px/f [C/I].
- **Dudas:** 4086/5D12 (¿música o evento?). Papel de 0x4F (jefe, puerta, objeto). Con otros enemigos
  vivos, el 0x4F no aparece.

#### 0x50 — boca/cañón de pared que dispara ráfagas a la izquierda
(misma rutina que 0x3A: 9159/9160)
- **Vida / recompensa:** invulnerable (FE); clase 0 (cat. 101). Castillo. Sin hoja de sprites **Botín real:** regla 4 (nunca) [C].
  (descriptor 00 02 00 → sin bit 7 en d0, sin sprite [I]).
- **Inicio / comportamiento:** como 0x3A, salvo dos cosas: la condición en x se invierte (CCF): el
  jugador debe estar a la izquierda o en la misma x (x ≥ jugador_x); y el 0x3B sale en (y+4, x+0) en
  lugar de x+16 [C].
- **Ataques / hijos:** 0x3B (≈ 3 px/f apuntado, sonido 0x12) [C].
- **Comparación con lo observado:** «quieto»; encaja [I].
- **Dudas:** las mismas que 0x3A. Al no tener sprite, el cambio de IX+0A (abrir/cerrar) no se ve;
  será parte del decorado [I].

#### 0x51 — trampa de flechas en la pared, dispara a la izquierda
(rutinas compartidas con 0x32: inicio 9078, update 9084)
- **Vida / recompensa:** invulnerable (FE); clase 121. Castillo. Sin sprite (descriptor 00 05 00) [I]. **Botín real:** regla 4 (nunca) [C].
- **Inicio:** IX+8 = 30, IX+15 = 30, IX+2 bit 6 = 0 [C].
- **Comportamiento:**
  - Estado 0: DEC IX+8; comprueba sólo al llegar a 0 (si falla, la siguiente vez es 256 frames
    después) [C]. Condición: 0 < jugador_y − y ≤ 32 y d = x − jugador_x ≥ 0 (jugador a la izquierda),
    con d ∈ [16, 48) o d ∈ [64, 96). Para 0x32 es d' = jugador_x − x, en [40, 72) o [88, 120) [C].
    Si se cumple: estado 1 [C].
  - Estado 1: repite la comprobación cada frame; si falla, estado 0 con IX+8 = 30 [C]. Mientras se
    cumpla, DEC IX+15; al llegar a 0, IX+15 = 1 + (R & 31) (1..32 frames) y crea **0x33** en (y+4, x)
    con vx = −4 px/f (0x32: en (y+4, x+16) con vx = +4) [C].
- **Ataques / hijos:** 0x33 (vx ±4 px/f, que fija esta rutina) [C].
- **Comparación con lo observado:** «quieto», sin hijos: encaja si el jugador no pasó por las franjas
  [I].
- **Dudas:** las franjas con hueco (d ∈ [48, 64) no dispara) quizá correspondan a la geometría de la
  sala [I]. No he mirado el update de 0x33.

#### 0x64 — segmento trasero de oruga (pareja de 0x2C)
- **Vida / recompensa:** 4; clase 0 (cat. 101). Mundo 2. **Botín real:** regla 4 (nunca) [C].
- **Inicio (AAA3):** IX+2 bit 4 = 0. Copia IX+14 (índice de grupo) e IX+15 (retardo inicial) del
  **hueco anterior** (IX−0x0C, IX−0x0B), que es la cabeza 0x2C. Esta tomó el índice de E637 (que
  incrementa) y un retardo de 16..31 [C]. Después, A97F: IX+1D = 26, IX+1E = 8, IX+1F = 32 (umbrales
  de animación: **IX+1D no es la dirección en este tipo**), vx = +1 px/f, fotogramas IX+16..19 = A0,
  A4, A8, AC (derecha) o B0..BC (izquierda), IX+0A = IX+16 [C].
- **Comportamiento** (A99D): espera el retardo IX+15 y después actúa cada frame. Estado de grupo
  compartido en E638[IX+14] [C]:
  - Grupo = 1 (turno del segmento): se mueve (bit 4 = 1) con el paso «oruga» AA1C. Al terminar el
    ciclo (IX+1B vuelve a 0), E638 ^= 1 → turno de la cabeza [C]. (La cabeza 0x2C hace lo contrario:
    anda con grupo = 0 y pasa a 1 [C].)
  - Grupo = 0: quieto (bit 4 = 0) [C].
  - Grupo ≥ 2: anda siempre [C].
  - Si el compañero ha desaparecido (el tipo del hueco anterior es 0) o IX+1A bit 0 = 1: modo solo,
    con IX+1A = 1, umbrales 13/6/17, vx = ±2 px/f y E638 = 2 [C].
  - Paso AA1C: invierte vx (y cambia el juego de fotogramas) si no hay suelo delante (tile y+16, x+16
    o x), si hay pared (tile y+8) o si x < 8 / x ≥ 0xF0. Contador IX+1B: 1..7 → fotograma +17;
    8..25 → +18; 26..31 → +19 y parado (bit 4 = 0); 32 → 0 y fotograma +16. En modo normal se mueve
    26 de cada 32 frames [C].
  - Después, B06C: IX+11 = tabla B0B0[(IX+0A − 0xA0)/4] [C].
- **Ataques / hijos:** ninguno [C].
- **Comparación con lo observado:** «camina», moving 0,16, step 1,0: encaja con 1 px/f, alternando
  turnos con la cabeza [C/I].
- **Dudas:** E638..: tabla de estados por grupo; ¿quién la pone a 0 al entrar en la sala? Si el
  generador no deja la cabeza justo en el hueco anterior, la herencia de IX−0x0C falla [I].

#### 0x69 — cabezas flotantes en formación que disparan por turnos
- **Vida / recompensa:** invulnerable (FF); clase 14 (cat. 0, nivel 1). Mundo 9, 21 generadores. **Botín real:** regla 4 (nunca) [C].
- **Inicio (74BB):** si E041 = 10 y E042 = 0x19 (mundo 9, sala 0x19), IX+1D = 1 (fuerza el modo
  apuntado) [C]. IX+2 bit 2 = 1; IX+15 = índice (contador E62A, que se incrementa). Si es la primera
  (líder, índice 0): E62B = 0, IX+18 = 0 (modo), IX+16 = 40, IX+19 = número de turnos (1..7) [C].
  Todas: IX+1B = y de casa, IX+8 = 8, vy = ±0,5, vx = ±0,125 al azar [C].
- **Comportamiento** (7511):
  - Cada 8 frames: IX+0A ^= 0x7C, IX+11 ^= 1 [C].
  - Invierte vx si x < 0x10 o x ≥ 0xD8. Muelle vertical: vy ±1/16 px/f² hacia la casa, unos ±2 px
    [C/I].
  - Fase global E62B = 0 (sólo actúa el líder): cuenta IX+16 (40 al principio, después 16). Al llegar
    a 0: E62B = 1, IX+17 = número de cabezas, DEC IX+19. Si llega a 0, cambia de modo: alterna
    IX+18 ^= 1, o se fuerza a IX+1D si no es 0, y recarga IX+19 (1..7 en modo 0, 1..3 en modo 1) [C].
    - Modo 0: elige una tiradora al azar entre los índices 0..3 (r&3; si ≥ número de cabezas, la 0).
      E629 = ese índice; a ella le pone IX+16 = 24 e IX+1A = dirección al azar 0..9 [C].
    - Modo 1: de los índices 0..3, elige la más cercana en x al jugador y le calcula IX+1A con la tabla
      de umbrales 7626 (según jugador_x − x) [C].
  - Fase E62B = 1 (sólo actúa la tiradora, IX+15 = E629): 24 frames de destello (IX+0B ^= 0x0C cada
    frame). Después, IX+0B = 15, E62B = 0, IX+16 del líder = 16, y crea **0x06** en
    (y+10, x+8 − 77CA[…]) con E624 = IX+1A (dirección) [C].
- **Ataques / hijos:** 0x06 (inicio 7679): proyectil con 10 direcciones (tabla 77FB). Cerca de los
  bordes (x < 0x27 o ≥ 0xD5) cambia la dirección ±5; sonido 0x07 [C].
- **Comparación con lo observado:** «vuela», y_range 7, speed 0,06/0,16, hijos 0x06 ×4: encaja con
  la deriva lenta y el muelle [C/I].
- **Remake:** el modo forzado de la sala 0x19 del mundo 9 está implementado (al cambiar de modo, siempre el apuntado). Falta la tabla de umbrales 7626: el reparto 0..4 / 5..9 según la distancia es una aproximación [I].
- **Dudas:** 75AD con el jugador a la derecha da índices 0..4 y luego sigue leyendo más allá de la
  tabla (bytes de código en 7630), así que el reparto 0..4 / 5..9 por lado no está claro [C]. Sólo
  pueden disparar los índices 0..3. E629, E62A y E62B no se reinician aquí.

---

### Tabla resumen

| Tipo | Nombre | Vida | Movimiento | Disparo / hijos | Encaja con lo observado |
|---|---|---|---|---|---|
| 0x36 | orbes en cadena que saltan | inv. | muelle vertical ±5 px; relevo: una vuela a ≈6 px/f a la casa de otra (0..7) | — (sonido 0x0C) | sí |
| 0x3A | boca de pared (dispara a la derecha) | inv. | fija | 0x3B apuntado ≈3 px/f cada 8 frames durante 120 si el jugador está 0..32 px debajo y a la derecha | sí (no activada) |
| 0x3C | hechicero que se teletransporta | 7 | 64 visible → 8 parpadeo → teletransporte a suelo libre al azar | 0x3D apuntado ≈2 px/f cada 32 frames | sí |
| 0x40 | espíritu perseguidor | 35 | apunta al jugador a 0,5 px/f (cada 60 frames), atraviesa paredes, rebota en caja; para 30 cada 120 | 0x41 (8 direcciones) al reanudar | parcial (más lento en la traza) |
| 0x42 | monstruo del agua | 10 | sale del agua en 4 x fijas, sube/baja 12 px a 0,5 px/f; oculto si el jugador está en el agua | 0x60 (chorro junto al jugador) + 0x61 apuntado cada 32 frames | sí |
| 0x43 | patrullero ancho | 10 | anda a 2 px/f (empieza siempre a la izquierda), gira en bordes, paredes y fin de plataforma | 0x62 (rayo vertical 4 px/f) si el jugador está ≤32 px a la derecha; espera a quedar solo | sí |
| 0x49 | mosca que se divide | 5 | 16 rumbos al azar a 2 px/f cada 16..31 frames, rebota en la caja | se divide en 2×0x52 (→ 2×0x53) con IX+1C ≠ 0 | sí |
| 0x4C | errante que suelta esporas | 20 | deambulador 1 px/f (16 rumbos, vuelve al centro) | 0x4D ondulante cada 16 frames | sí |
| 0x4E | almas que se reúnen | 0 | deambulador 1 px/f; si el jugador espera 112 frames en un punto, vuelan al centro a 4 px/f | al juntarse todas → 0x4F; sonido 0x83 | sí |
| 0x50 | boca de pared (dispara a la izquierda) | inv. | fija (sin sprite) | como 0x3A, con el jugador a la izquierda | sí (no activada) |
| 0x51 | trampa de flechas a la izquierda | inv. | fija (sin sprite) | 0x33 a −4 px/f cada 1..32 frames si el jugador está en franjas de x (16–48, 64–96 px) y 0..32 px debajo | sí (no activada) |
| 0x64 | cola de oruga (con 0x2C) | 4 | 1 px/f a pasos (26/32 frames), turnos alternos con la cabeza; solo: 2 px/f continuo | — | sí |
| 0x69 | cabezas flotantes en formación | inv. | deriva x 0,125 px/f + muelle vertical ±2 px | por turnos: una destella 24 frames y lanza 0x06 (al azar o apuntado a la más cercana) | sí |

### Dudas principales
1. extracted/ai/*.asm está truncado en cada RET condicional (fallo de enemy_ai.py); conviene
   regenerarlo.
2. La velocidad y es IX+0C/0D; IX+16/17 es aceleración (B14C). Hay que corregir el contexto y
   docs/items_enemies.md.
3. E045 escala todas las velocidades «apuntadas» ((S + 8·E045)/32 px/f) y no está identificado.
4. 0x49: quién escribe IX+1C (el disparador de la división).
5. 0x43: el `AND 1` sin uso en 8629/862B fija vx = −2 siempre; ¿bug del original?
6. 0x69: el cálculo de dirección 75AD con el jugador a la derecha lee más allá de la tabla 7626.
7. Contadores globales que no se reinician en estas rutinas: E633/E634 (0x36), E62A/E62B/E629 (0x69),
   E637/E638.. (0x2C/0x64), E63D (0x4E), E603 (0x42/0x43).

## IA de enemigos — hijos (proyectiles y enemigos creados)

(0x06 0x1A 0x1D 0x1E 0x33 0x37 0x3B 0x3D 0x3F 0x41 0x44 0x45 0x46 0x47 0x48 0x4A 0x4B 0x4D 0x4F 0x52 0x53 0x56 0x60 0x61 0x62)

### Notas previas

- Fuente: `extracted/ai/<tt>.asm` (regenerados, ya sin el corte en `RET cc`) más desensamblado lineal de los auxiliares.
  Entradas reales: tabla de inicio b04:66B4[t−1] y de actualización b06:B472[t−1] [C]. Velocidades y tiempos **por tick
  de IA** (1 tick = 2 frames), aunque se escriba «px/f».
- **Descriptor** (b04:67B6 + 5·t): byte 0 = d0 (IX+9), byte 1 = sprite, byte 2 = color, **byte 3 = vida** (FE/FF =
  invulnerable), byte 4 = IX+11 [C]. **d0 & 7 = número de sprites hardware** que ocupa: al borrar el enemigo, B431 resta
  ese número de E621 [C]. d0 = 0xC0|n en los tipos «columna» (0x06, 0x60, 0x62): n segmentos de 16×16 apilados que
  la IA hace crecer o encoger con INC/DEC IX+9 [C]. 0x37 y la explosión de 0x47 usan 4 sprites (32×32 [I]); el hada
  0x4F, 2 [C/I].
- Creación: 6551 (A = tipo, D = y, E = x, **HL → vx**, vy = 0, B → E624) y 65DA (igual con velocidad 0) [C].
- **IX+3 bit 6** (lo pone la rutina común 6C42, que usan casi todos los proyectiles): al tocar al jugador, tras aplicar
  el daño por contacto, el proyectil **se destruye** (b03:B285 → 4257) [C]. Sin ese bit, sigue su camino.
- Apuntados (B18F/B195): apuntan desde su propia (y, x) al punto (jugador_y − 16, jugador_x − 8), con módulo
  (S + 8·E045)/32 px/tick: S = 0x40 → 2, 0x60 → 3, 0x80 → 4 [C]. El rumbo se cuantiza con la tabla atan B274 [C].
  La velocidad no cambia después: **línea recta, atraviesa paredes** y muere al salir de pantalla (y ≥ 0xB8 o
  x ≥ 0xF9; salir por arriba o por la izquierda también da la vuelta a ≥ 0xB8/0xF9) [C].
- Proyectiles horizontales (0x1A, 0x1D, 0x1E, 0x33, 0x3F; update común **98A7**): cada tick miran el tile en
  (x + 16 si vx ≥ 0, x − 1 si vx < 0; y) y se destruyen si es sólido (≥ 0xBA) [C]. La vx la pone el padre por HL.
- **Daño por contacto** (b04:64E5 + tipo) [C]: 0x06: 18 · 0x1A: 3 · 0x1D: 1 · 0x1E: 1 · 0x33: 1 · 0x37: 0 · 0x3B: 2 ·
  0x3D: 2 · 0x3F: 0 (efecto especial, ver ficha) · 0x41: 4 · 0x44: 2 · 0x45: 4 · 0x46: 2 · 0x47: 2 · 0x48: 1 · 0x4A: 2 ·
  0x4B: 1 · 0x4D: 1 · 0x4F: 0 (es el hada) · 0x52: 2 · 0x53: 1 · 0x56: 2 · 0x60: 2 · 0x61: 2 · 0x62: 8.
- **Quién escribe IX+1C («me han golpeado»)** [C]: el código de impacto del arma, **b03:B54D** (IY = arma, IX =
  enemigo). Llama a B592, que da Z si el tipo es 0x0F, 0x25, 0x27, 0x49, 0x52 o 0x54, y entonces B5A7 hace
  `LD (IX+1C),1`. Después resta 1 a la vida (IX+10), o 2 si E094 ≠ 0; si llega a 0, el enemigo muere (B604). El arma
  de tipo 5 entra por B5B1 (mismo aviso, resta 2 o 4). Si el enemigo tiene IX+2 bit 2 (p. ej. 0x06), salta a AE68
  sin quitar vida [C]. **0x53 no está en la lista**: por eso no se divide.
- **E045** [C/I]: no hay ninguna escritura directa (ni `LD (E045),A`, ni vía HL/IX/IY con esa dirección). Se pone a 0 en
  el borrado de E016–E0FE al empezar partida (b00:4AC3 y 4CB0). Se lee en 9 sitios (bancos 4–6). Además de en los
  apuntados, b04:6320 lo usa como índice de una tabla {3,3,3,4,4,5…} y calcula 0x70 − 2·E045. Es un **nivel de
  dificultad** que en la práctica vale 0 [I]. Quizá lo escriba un LDIR de bloque o la carga de contraseña; no lo he
  encontrado.

---

#### 0x06 — rayo (lo lanzan las cabezas 0x69)
- **Vida:** invulnerable (FF); además IX+2 bit 2 = 1, así que el arma rebota (AE68) [C]. Contacto 18. Sonido 0x07 al nacer.
- **No es un proyectil con velocidad:** vx = vy = 0 (B124). Es una **cadena de segmentos de 16×16** que baja en zigzag.
  El renderizador (b01:717B) dibuja IX+16 segmentos desde (y, x) hacia abajo, cada uno 16 px más abajo, con los
  patrones IX+18.. y el color IX+15. Deja en IX+12/13 la posición del **último segmento** [C].
- **Trazado:** los patrones salen de un flujo de bytes en **77FB** (A0, A4, A8, AC, B0 = 5 formas de rayo). La
  «dirección» E624 (0..9) solo elige el punto de arranque, 77FB + 8·dir. Entre una forma s y la siguiente s', la x se
  desplaza salida[s] − entrada[s'], con salida 77C5 = {1, 6, 10, 10, 14} y entrada 77CA = {6, 7, 0, 15, 4} [C].
  Deriva horizontal acumulada en los primeros 8 segmentos [C]: dir 0: −40 · 1: −36 · 2: −23 · 3: −21 ·
  4: −4 (casi vertical) · 5: +24 · 6: +14 · 7: +26 · 8: +54 · 9: ≈ +55. La 9 lee más allá de la tabla (78 4B..,
  bytes de código) a partir del 9.º segmento.
- **Corrección en los bordes** (inicio): si x < 0x27 y dir < 5 → dir + 5; si x ≥ 0xD5 y dir ≥ 5 → dir − 5; es decir,
  junto a un borde se fuerza la deriva hacia dentro [C/I].
- **Ciclo** [C]:
  - Est. 0: crece 1 segmento por tick, de 1 a 6, con la cabeza fija en el origen.
  - Est. 1: toda la cadena **baja 16 px por tick** (y += 16, x según el zigzag, puntero +1) hasta que el último segmento
    (IX+12) llega a y ≥ 0x90.
  - Est. 2: encoge desde arriba, 1 segmento por tick (y += 16, d0−−). Al quedar uno, mira el tile en (IX+13, 0xB8):
    - si **no** es sólido, desaparece;
    - si es sólido, se convierte en un **chispazo**: sprite B4, color 8, y = 0xA8, sonido 0x1C, durante 48 ticks
      (sprite ^ 0x0C cada 4); luego se borra.
  - Color cíclico: cada tick, IX+15 = 82E4[i], con 21 pasos (la misma tabla que 0x4E) [C].
- **Colisión:** b03:B2A0/B51D usan para el 0x06 una caja con origen en IX+12/13, es decir, el segmento inferior [C].
  [I] Solo hace daño la punta del rayo.

#### 0x1A — escupitajo de la serpiente 0x05
- **Vida** 1. Contacto 3. Sonido 0x0E al nacer. 0x05 lo crea en (y+4, x) con vx = ±4 px/tick (HL) y B = 0 (derecha) o
  1 (izquierda) [C].
- **Inicio:** IX+8 = 32. Si E624 ≠ 0 (va a la izquierda): sprite BC e IX+3 bit 6 [C]. **Peculiaridad:** el que va a la
  derecha no tiene el bit 6, así que **no desaparece al tocar al jugador** [C].
- **Movimiento:** recto a 4 px/tick. Muere a los **32 ticks** (128 px) o al chocar con un tile sólido (98A7) [C].

#### 0x1D — púa del erizo 0x11
- **Vida:** inv. (FF). Contacto 1. Inicio 6C42 (desaparece al tocarte). Sin sonido propio (el 0x11 pide el 0x10) [C].
- Horizontal a ±4 px/tick (HL del padre). Muere contra un tile sólido o al salir de pantalla [C].

#### 0x1E — bola del armadillo 0x12
- **Vida:** inv. (FF). Contacto 1. Sonido 0x0F. Horizontal a ±2 px/tick. Muere contra sólido o al salir [C].

#### 0x33 — flecha de las torretas 0x32 y 0x51
- **Vida** 1. Contacto 1. Sonido 0x10. Si vx < 0: sprite C8 y attr 0x60 [C].
- Horizontal a ±4 px/tick. Muere contra sólido o al salir [C].

#### 0x3F — disparo paralizante del soldado 0x2D
- **Vida:** inv. (FF). Contacto 0. Si E624 ≠ 0: sprite B4, attr 0x21 (izquierda) [C]. Horizontal a ±4 px/tick; muere contra
  sólido o al salir [C].
- **Al tocar al jugador** (b03:B2E2) [C]: se destruye y, en lugar del daño normal, pone **E500 = 6 con E50F = 60**
  (estado especial del jugador, 60 frames). Si el jugador está en E500 = 3, solo hace el retroceso (E500 = 4) sin
  daño. [I] Es un estado de parálisis: el 0x2D **carga** contra el jugador precisamente cuando E500 = 6.
  `items_enemies.md` lo describía como «restaura algo»; no es así.

#### 0x3B — bola de las bocas de pared 0x3A y 0x50
- **Vida** 1. Contacto 2. Sonido 0x12.
- **Inicio:** apunta a rapidez 0x60 (**3 px/tick**) como si saliera de y' = y + (E604 & 31) − 8, es decir, de −8 a +23 px
  respecto a su y real, lo que añade una dispersión vertical. Update: RET (recto) [C].

#### 0x3D — bola del hechicero 0x3C
- **Vida** 1. Contacto 2. Sonido 0x0F. Apuntado a **2 px/tick** (S = 0x40), recto [C].

#### 0x41 — disparo en 8 direcciones del espíritu 0x40
- **Vida:** inv. (FF). Contacto 4. Sonido 0x19. Update: RET [C].
- **Inicio:** calcula el ángulo hacia el jugador desde (y+8, x+8) (B1DB) y lo redondea a 1 de 8: ((ángulo + 0x10) >> 5) & 7.
  Cada entrada de la tabla **8DC5** (8 bytes) da sprite, vy, vx, el desplazamiento de salida (dy, dx) y el attr [C]:

  | i | sprite | (vx, vy) px/tick | salida (dy, dx) |
  |---|---|---|---|
  | 0 | A4 | (+4, 0) → | (+13, +16) |
  | 1 | C8 | (+4, +4) ↘ | (+13, +16) |
  | 2 | A8 | (0, +4) ↓ | (+13, +16) |
  | 3 | BC | (−4, +4) ↙ | (+13, 0) |
  | 4 | A4 | (−4, 0) ← | (+13, 0) |
  | 5 | C8 | (−4, −4) ↖ | (−3, 0) |
  | 6 | A8 | (0, −4) ↑ | (−3, +16) |
  | 7 | BC | (+4, −4) ↗ | (−3, +16) |

  Las diagonales van a 4 px por eje (5,66 px/tick) [C]. [I] El sentido de giro de los índices sale de la tabla
  (y hacia abajo); no lo he comprobado en el emulador.

#### 0x44 / 0x56 — bola del espectro 0x13 (0x44) y del hombre-llama 0x0C (0x56)
- **Vida:** 0x44 inv. (FF); 0x56 vida 1. Contacto 2. Sonido 0x0F (0x44) o 0x0E (0x56). Sprites B4 / A0 [C].
- Apuntados a **2 px/tick** (S = 0x40), rectos. La vx basura que pasa el padre se sobrescribe [C].

#### 0x45 — bola del guardián orbital 0x15
- **Vida:** inv. (FF). Contacto 4. Sonido 0x0E. Apuntado a **2 px/tick**, recto [C].

#### 0x46 — rastro de la aparición 0x16
- **Vida:** inv. (FF). Contacto 2. Sonido 0x0F. Apuntado a **4 px/tick** (S = 0x80, B18D), recto [C].

#### 0x47 — bomba rodante del centinela 0x17
- **Vida:** inv. (FF). Contacto 2. Durante el rodado, d0 = C4 ^ 45 = 0x81 (1 sprite). E624 = dirección del padre
  (bit 0 = 1 → izquierda). IX+1A = **jugador_x en el momento del disparo** (objetivo) [C].
- **Est. 0, rodar:** cada tick recalcula vx según la distancia |x − objetivo| (**frena al acercarse**) [C]:
  ≥ 132: 3,5 px/tick · 116–131: 3,0 · 96–115: 2,5 · 72–95: 2,0 · 40–71: 1,5 · < 40: 1,0. La animación (sprite ^ 4) va
  cada 3, 6, 9, 12, 15 o 18 ticks, respectivamente. Se para cuando:
  - pasa el objetivo (x ≥ objetivo yendo a la derecha, x < objetivo yendo a la izquierda);
  - no hay suelo delante (6BEE);
  - hay pared delante o llega al borde de pantalla (6C04).
- **Est. 1, mecha:** quieta (bit 4 = 0) durante 32..63 ticks al azar [C].
- **Est. 2, explosión:** d0 = C4 (4 sprites), x −= 8, y −= 8, patrón 0x0A, color 6, attr 0x3D, sonido 0x18. A los 8 ticks,
  patrón 0x0B y attr 0x5B; a los 16 ticks se borra [C]. [I] Explosión de 32×32 centrada en la bomba.

#### 0x48 — murcielaguito que sueltan 0x19 y 0x37
- **Vida:** inv. (FF). Contacto 1. Sprite D8. Inicio 6C42.
- **Movimiento:** vagabundeo: rumbo al azar entre 16 a **1 px/tick**, nuevo rumbo cada **16 ticks** (87BA). No mira tiles;
  muere al salir de pantalla o al tocarte [C].

#### 0x4A — llamarada del escupefuego 0x22
- **Vida:** inv. (FF). Contacto 2. Sprite CC. Inicio 6C42. El sonido 15 lo pide el padre [C].
- **Inicio:** vy = −4 px/tick (hacia arriba); vx = ±{0, 2, 4, 3}[r & 3] px/tick, con signo al azar. Gravedad
  IX+14 = 0x40 → **vy += 0,25 px/tick²** (el cálculo aleatorio de 812D siempre da 0x40) [C].
- **Movimiento:** parábola. Sube unos 32 px en 16 ticks y luego cae; atraviesa paredes y muere al salir por abajo
  (y ≥ 0xB8) [C].

#### 0x4B — bola del fantasma orbitante 0x26
- **Vida:** inv. (FF). Contacto 1. Sonido 0x0F. Apuntado a **3 px/tick** (S = 0x60), recto [C].

#### 0x4D — espora ondulante de 0x4C
- **Vida:** inv. (FF). Contacto 1. Color 1. Inicio 6C42 [C].
- **Movimiento:** cada 8 ticks, rumbo = 9064[base + k] a **1 px/tick** (7E25). La tabla 9064 es (i + 1) & 15, así que
  rumbo = (base + k + 1) & 15. base = r & 15 se fija al nacer, y k oscila 1, 2, 3, 4, 3, 2, 1, 0, 1… [C].
  Resultado: **ondula ±2 rumbos (±45°) alrededor del rumbo base + 3**, con un periodo de 64 ticks. Animación: cada 4
  ticks, sprite = C0 | ((sprite + 4) & 15), es decir, C0 → C4 → C8 → CC [C]. Atraviesa paredes [C].

#### 0x4F — hada (la crean las almas 0x4E al reunirse)
- **Vida 0** (descriptor `82 a4 0f 00 00`); al nacer, IX+2 bits 5 y 6 = 0 (intocable). Velocidad 0. Nace en
  (0x70, 0x80) [C].
- **Fase 0 (aparición, 80 ticks):** quieta, sprite A4 parpadeando (^ 0x0C cada 2 ticks) [C].
- **Al acabar:** attr 0x13, **IX+2 bit 6 = 1** (ya se puede tocar), d0 = C2 (2 sprites), patrón 0, color 8, música
  **0x89** (4080) [C].
- **Fase 1:** revolotea con el deambulador común 8145, a 1 px/tick, rumbo al azar de 16 cada 3..31 ticks y vuelta al
  centro si sale de la caja (caja pequeña, porque d0 bit 2 = 0). Patrón 0 y color 8 si va a la derecha; patrón 2 y
  color 9 si va a la izquierda; el patrón alterna ^ 1 cada 2 ticks [C].
- **Al tocarla** (b03:B2FE): se destruye y llama a 612C con A = vitalidad máxima del personaje (E053 Aphrodite /
  E057 Popolon). Eso suma la vitalidad máxima al incremento pendiente (E05E/E061): **rellena la vitalidad**. Sonido
  0x29 [C]. Coincide con lo observado en `entities.md` («Hadas») [O]. No muere por tiempo [C].

#### 0x37 — murciélago grande (fusión final de 0x19)
- **Vida 8**. Descriptor `c4 0e 0e 08 3d`: 4 sprites (32×32 [I]), patrón 0x0E, color 14. **Contacto 0** en la tabla
  64E5: la tabla por niveles B3AE {1, 2, 4, 8} solo se aplica al tipo 0x19, así que el 0x37 solo empuja (E500 = 4,
  sin restar vitalidad) [C]. [I] Puede ser un descuido del original.
- **Inicio (8681):** nivel IX+1B = 3, IX+8 = 4, primer disparo a los 48 ticks (IX+1C = 0x30), estado 0, **IX+18 bit 7 = 1
  (sin pareja)**, así que nunca intenta fusionarse. La B que pasa el padre se ignora [C].
- **Comportamiento (868F, compartido con 0x19):**
  - Deambula con 8145 a 1 px/tick (rumbo de 16 al azar cada 3..31 ticks), con la caja grande porque d0 bit 2 = 1: si
    x ∈ [0xB8, 0xF0) o y ∈ [0x7F, 0xDF), vuelve hacia el centro 24 ticks [C].
  - Animación: patrón ^ 1 cada 8 ticks [C].
  - Cada **64 ticks** (el primero a los 48) crea un **0x48** en (y+16, x+16). E62F vale ya 3, así que siempre dispara [C].
- No atraviesa ninguna lógica de tiles; muere solo por vida [C].

#### 0x52 / 0x53 — moscas divididas (de 0x49)
- **Vida:** 0x52 = 3, 0x53 = 1. Contacto 2 / 1. Sprites AC / A4 [C].
- **Nacimiento:** al recibir un golpe que no lo mata, el 0x49 pone IX+1C = 1. En su siguiente update crea **dos 0x52** en
  su misma (y, x) (E624 = 1 y 0, que no se usan) y se borra. Un 0x52 golpeado hace lo mismo con **dos 0x53** [C].
- **Inicio (A0F1):** IX+14 = 8 → **8 ticks sin poder ser golpeado** (IX+2 bit 5 = 0; el 0x49 solo tiene 1).
  IX+15 = 1 → elige rumbo en el primer update. Velocidad 0. E63E = 1 [C].
- **Comportamiento (A109):**
  - Rebota en la caja y ∈ [0x20, 0xB0), x ∈ [8, 0xE0) invirtiendo vy o vx (930E) [C].
  - Cada 16..31 ticks, rumbo de 16 al azar a **2 px/tick** (las dos hermanas lo eligen por separado) [C].
  - Aleteo cada 8 ticks: 0x52 hace sprite ^ 0x1C y attr ^ 0x66; 0x53 hace sprite ^ 0x0C y attr ^ 0x1E [C].
  - Sin sonido: el 0x0A periódico es solo del 0x49 [C].
- **División:** el 0x52 está en la lista de B592, así que se divide al primer golpe que no lo mata (vida 3). El 0x53 no
  está y muere con 1 de daño [C]. Total por cada 0x49: 1 → 2 → 4 moscas.

#### 0x60 — chorro de agua del monstruo 0x42
- **Vida 4**. Contacto 2. Descriptor `c7 d8 0a 04 00`. El 0x42 lo crea en y = 0xA0, x ≈ jugador_x ± (ver su ficha) [C].
- **Inicio:** d0 &= 0xFA → C2 (2 segmentos), IX+16 = 0xA0 (base), vy = **−4 px/tick**, vx = 0, sonido 0x17 [C].
- **Ciclo** [C]:
  - Est. 0: sube hasta y = 0x90 (4 ticks); d0 = C3.
  - Est. 1: cada 16 px de subida (y & 15 = 0) añade un segmento, hasta C7 (7). Con 7 segmentos y y = 0x40, invierte:
    vy = +4.
  - Est. 2: baja a 4 px/tick quitando un segmento cada 16 px; al pasar por y = 0x90 pone **E628 = 1**, que avisa al
    0x42; en y = 0xA0 se borra.
  - Ida y vuelta: 0xA0 → 0x40 → 0xA0, es decir, 24 + 24 ticks.
- **Dibujo** (b01:70C6): sprite base D0 (color 4) en y = 0xA0, cabeza en y (sprite D8) y segmentos D4 entre ambos. Columna de
  16 px de ancho y hasta 112 px de alto. La caja de colisión crece con el número de segmentos (B916) [C].

#### 0x61 — bola del monstruo del agua 0x42
- **Vida:** inv. (FF). Contacto 2. Sonido 0x0F. Apuntado a **2 px/tick**, recto [C].

#### 0x62 — rayo vertical del patrullero 0x43
- **Vida 4**. **Contacto 8**. Descriptor `86 c4 08 04 00`. Nace en (y, x+14) [C].
- **Inicio:** d0 = 0x81 y sin mover. Elige el sentido [C]:
  - **abajo** (y += 12, sprite CC, vy = +4, IX+15 = 1) si y < 0x30, o si y < 0xA0 y jugador_y ≥ y;
  - **arriba** en otro caso (y −= 4, sprite C4, vy = −4).
- **Ciclo** [C]:
  - Est. 0: sonido 0x17, d0 = C2, color 12, IX+16 = y de origen; empieza a moverse.
  - Est. 1: se alarga a 4 px/tick y añade un segmento cada 4 ticks (16 px), hasta 4 más (d0 hasta C6). Para si llega al
    límite: y ≥ 0xB0 bajando o y < 0x20 subiendo.
  - Est. 2: quieto **16 ticks**.
  - Est. 3: invierte vy y se recoge hacia el origen a 4 px/tick quitando un segmento cada 4 ticks; al llegar a 0
    segmentos se borra.
- **Dibujo:** base C8 en el origen, cabeza C4/CC y segmentos C8 hacia el origen (+16 o −16 según IX+15). La caja crece
  con los segmentos y se desplaza hacia arriba si el rayo va hacia abajo (B933) [C].
- Mientras exista, el 0x43 espera (E620 = 1) [C].

---

### Tabla resumen

| Tipo | Padre | Vida | Contacto | Movimiento | Muere | Sonido |
|---|---|---|---|---|---|---|
| 0x06 | 0x69 | inv. | 18 | rayo de 16×16 en zigzag: crece a 6 segmentos y baja 16 px/tick; deriva según dir 0..9 (77FB) | al encoger; chispazo de 48 ticks si hay suelo en y = 0xB8 | 0x07; 0x1C |
| 0x1A | 0x05 | 1 | 3 | ±4 px/tick horizontal | 32 ticks, tile sólido; a la derecha no desaparece al tocarte | 0x0E |
| 0x1D | 0x11 | inv. | 1 | ±4 horizontal | sólido / pantalla / jugador | (padre 0x10) |
| 0x1E | 0x12 | inv. | 1 | ±2 horizontal | sólido / pantalla / jugador | 0x0F |
| 0x33 | 0x32, 0x51 | 1 | 1 | ±4 horizontal | sólido / pantalla / jugador | 0x10 |
| 0x37 | 0x19 (fusión) | 8 | 0 | deambula 1 px/tick (caja grande) | vida | — |
| 0x3B | 0x3A, 0x50 | 1 | 2 | apuntado 3 con dispersión vertical | pantalla / jugador | 0x12 |
| 0x3D | 0x3C | 1 | 2 | apuntado 2 | pantalla / jugador | 0x0F |
| 0x3F | 0x2D | inv. | 0 | ±4 horizontal | sólido / pantalla / jugador → E500 = 6 durante 60 frames (parálisis [I]) | (padre 5) |
| 0x41 | 0x40 | inv. | 4 | 8 direcciones, 4 px por eje (8DC5) | pantalla / jugador | 0x19 |
| 0x44 | 0x13 | inv. | 2 | apuntado 2 | pantalla / jugador | 0x0F |
| 0x45 | 0x15 | inv. | 4 | apuntado 2 | pantalla / jugador | 0x0E |
| 0x46 | 0x16 | inv. | 2 | apuntado 4 | pantalla / jugador | 0x0F |
| 0x47 | 0x17 | inv. | 2 | rueda hacia la x del jugador, de 3,5 a 1 px/tick frenando | mecha 32..63 ticks → explosión 32×32 de 16 ticks | 0x18 |
| 0x48 | 0x19, 0x37 | inv. | 1 | 16 rumbos al azar a 1 px/tick, cambia cada 16 ticks | pantalla / jugador | — |
| 0x4A | 0x22 | inv. | 2 | parábola: vy −4, g = 0,25, vx ∈ {0, ±2, ±3, ±4} | pantalla / jugador | (padre 15) |
| 0x4B | 0x26 | inv. | 1 | apuntado 3 | pantalla / jugador | 0x0F |
| 0x4D | 0x4C | inv. | 1 | 1 px/tick ondulando ±45° (cambio cada 8 ticks) | pantalla / jugador | — |
| 0x4F | 0x4E | 0 | — | 80 ticks quieta; luego deambula 1 px/tick | al tocarla: rellena la vitalidad | 0x89; 0x29 |
| 0x52 | 0x49 | 3 | 2 | 16 rumbos a 2 px/tick, rebota en la caja | golpe → 2 × 0x53 | — |
| 0x53 | 0x52 | 1 | 1 | igual | vida | — |
| 0x56 | 0x0C | 1 | 2 | apuntado 2 | pantalla / jugador | 0x0E |
| 0x60 | 0x42 | 4 | 2 | columna de agua: sube de 0xA0 a 0x40 y baja a 4 px/tick (2..7 segmentos) | al volver a 0xA0 (E628 = 1) | 0x17 |
| 0x61 | 0x42 | inv. | 2 | apuntado 2 | pantalla / jugador | 0x0F |
| 0x62 | 0x43 | 4 | 8 | rayo vertical hacia el jugador: se alarga a 4 px/tick hasta 6 segmentos, 16 ticks quieto, se recoge | al recogerse | 0x17 |

### Dudas
1. E045: no he encontrado quién lo escribe. Con 0 en la práctica, las velocidades de arriba son las reales.
2. 0x37: daño por contacto 0 (tabla 64E5). ¿Querían usar la tabla por niveles de 0x19 (8)?
3. 0x1A hacia la derecha no desaparece al tocar al jugador (falta el bit 6). ¿Fallo del original?
4. 0x3F: el significado exacto de E500 = 6 y E50F (parálisis o aturdimiento) está por comprobar en el emulador.
5. 0x06 dir 9 lee bytes de código más allá de 77FB+0x50 a partir del 9.º segmento.
6. 0x41: la correspondencia entre el índice 0..7 y el ángulo de B1DB se ha deducido de la tabla, no se ha medido.

## IA de enemigos — generados periódicamente

(0x08 0x09 0x0D 0x21 0x30 0x35 0x38 0x3E, y sus hijos 0x1B y 0x6A). Generadores, salas y tiempos de aparición: `entities.md`, "Generadores periódicos por sala".

### Notas previas

- Unidades **por tick de IA** (1 tick = 2 frames). Velocidades en 8.8; px/tick = valor/256.
- Orden de cada tick (b06:B3C5) [C]:
  1. update del tipo;
  2. si IX+2 bit 4 = 1, el integrador B0ED suma vy a y y vx a x;
  3. se borra el enemigo si y ≥ 0xB8 o x ≥ 0xF9. Salir por arriba o por la izquierda también cuenta, porque la coordenada da la vuelta.

  Cuando una ficha dice «se para», es que pone IX+2 bit 4 = 0.
- **Gravedad** = IX+16/17, que B14C suma a vy, pero solo cuando la IA lo llama. El 0x3E usa IX+16/17 como variables propias, no como gravedad [C].
- **Suelo, pared y techo:** un tile ≥ 0xBA es sólido [C].
- **Animaciones:**
  - E604 es el contador de ticks del bucle (b00:52F5) [I];
  - 7062 anima con el sprite entre C0 y D0 cada 4 ticks;
  - B17F anima cada 8 ticks.
- **Inicio y update** en b04:66B4[t−1] y b06:B472[t−1]. Vida = byte 3 del descriptor b04:67B6 + 5·t (FE/FF = invulnerable). Contacto = b04:64E5 + t [C].
- **Hijos** (6551): están sujetos al mismo límite E620 < E635 que los generados.

| Tipo | Descriptor | Vida | Contacto | Inicio / update |
|---|---|---|---|---|
| 0x08 | 82 cc 09 ff 4d (2 sprites) | inv. (FF) | 4 | 6DFE / 6E32 |
| 0x09 | 81 c4 0e fe 13 | inv. (FE) | 3 | 700D / 7020 |
| 0x0D | 81 c8 09 01 00 | 1 | 1 | 784B / 78CD |
| 0x21 | 81 a0 0b 02 58 | 2 | 2 | 7E89 / 7EB5 |
| 0x30 | 8a c4 09 05 41 (2 sprites, 32 px de ancho) | 5 | 5 | 7F2E / 7F72 |
| 0x35 | 81 c4 0e fe 13 (el mismo aspecto que 0x09) | inv. (FE) | 3 | AB88 / ABA7 |
| 0x38 | 81 c4 08 01 66 | 1 | 1 | A7A8 / A7C3 |
| 0x3E | 81 c4 0a 02 0f | 2 | 3 | A283 / A2A4 |
| 0x1B (hijo de 0x08) | 81 c8 0a 01 05 | 1 | 1 | 6EC2 / 6E7D |
| 0x6A (salpicadura) | 81 c4 0f ff 0b | inv. (FF) | 0 | 7937 / 6C47 |

---

#### 0x09 — bola que rebota (E650: castillo 58, 71, 113, 114)
- **Nace** en (x de la tanda, y = 0) [C]:
  - gravedad +0,5 px/tick² (IX+16 = 0x0080);
  - vx = ±1 px/tick, con el signo al azar (R bit 0);
  - vy = 0.
- **Cada tick** [C]:
  - Anima (7062).
  - Solo si **y ≥ IX+8** (que empieza en 0), mira el suelo con 7087: el tile en (x, y+16) o en (x+8, y+16) es sólido y el de encima, (·, y+8), está libre. Si se cumple, rebota: IX+8 = y + 16, vy = **−4**, vx = ±1 con un signo nuevo al azar, y **sonido 0x09**.
  - Si no rebota: vy += 0,5, con tope en **6 px/tick** (la parte alta de vy no pasa de 6).
- **Consecuencia** [C/I]: tras un rebote sube unos 18 px en 8 ticks y vuelve a caer. No vuelve a mirar el suelo hasta bajar 16 px por debajo del punto del rebote. Así que **rebota una vez en cada piso y lo atraviesa** al caer de nuevo.
  - No mira paredes: las atraviesa.
  - Muere al salir por abajo o por un lado.
  - No se puede matar.

#### 0x38 — bola que sube al techo (E662: castillo 94, 95, 102)
- **Nace** en (x de la tanda, y = 0xB0). Inicio [C]:
  - estado 1;
  - IX+1E = 0xA1 (altura a partir de la que mira el techo);
  - gravedad **−0,5** (hacia arriba);
  - vx = ±1 al azar.
- **Estado 1, subir** [C]:
  - Solo si y < IX+1E, mira el techo con 7087: tile sólido en (x, y) o en (x+8, y), con el de debajo libre. Al tocarlo:
    - y = (y & F8) + 8, IX+1E = y − 0x20;
    - vx = ±1 con un signo nuevo;
    - **se para** (bit 4 = 0), con sprite C4 y attr 0x66;
    - pasa al estado 0 con IX+8 = 60;
    - **sonido 0x0B**.
  - Velocidad: si vy ≥ 0, vy −= 0,5. Si vy es negativa, la parte alta se fija en −6, así que vy vale **−5,5 px/tick** desde el 2.º tick. Es la versión invertida del tope de 0x09, pero mal hecha: el tope actúa en cuanto la velocidad es negativa [C].
- **Estado 0, colgado** [C]: quieto durante **60 ticks** pegado al techo.
  - Luego vuelve a moverse, con sprite C8, attr 0x67 y **vy = +4**, y pasa al estado 1.
  - Con la gravedad −0,5 cae unos 18 px en 8 ticks y vuelve a subir a 5,5 px/tick.
  - No mira el techo hasta estar 32 px por encima del anterior, así que **atraviesa ese techo** y se pega al siguiente.
  - Acaba saliendo por arriba (y da la vuelta a ≥ 0xB8) y se borra.

#### 0x08 — bola de fuego (E653: lava; castillo 144–147, mundo 8: 15–19, mundo 9: 3–5, mundo 10: 2–3)
- Es la que no daña con el **Pitcher** (`items_enemies.md`, objeto 0x24).
- **Inicio (6DFE)** [C]:
  - x = (E604 & 0x7F) + 0x40, es decir **64..191**: ignora la x del generador. La y es la del generador: 0xA0, o 0xB0 en el mundo 8.
  - Altura objetivo IX+8 = (R & 0x3F) + 0x30 = **48..111**.
  - vy = **−4**.
  - Crea una salpicadura 0x6A en (x, y) con B = 1, con **sonido 0x0D**.
  - Sprite: par CC/D4 o D0/D8 según E604 bit 3. Es un dibujo de 2 sprites (IX+0A e IX+12).
- **Estado 0, subir:** sube a 4 px/tick (recto, sin gravedad) hasta y < IX+8. Entonces **se para**, pone IX+15 = 60 y pasa al estado 1 [C].
- **Estado 1, flotar 60 ticks:** y += 1 o y −= 1 según E604 bit 2, así que oscila ±1 px con un periodo de 8 ticks [C].
- **Al acabar, estalla** [C]:
  - crea **8 fragmentos 0x1B** en su (y, x), con B = 7..0 (una dirección cada uno);
  - se borra;
  - sonido 0x0E.
- Cada fragmento es un intento de creación aparte: con muchos enemigos en pantalla pueden faltar algunos [C].

#### 0x1B — fragmento de la bola de fuego (hijo de 0x08)
- **Inicio (6EC2)** [C]:
  - (vx, vy, gravedad) salen de la tabla 6F13 + 6·dir;
  - IX+3 bit 6 = 1: desaparece al tocar al jugador.

  | dir | vx | vy | gravedad (px/tick²) | muere en y ≥ (mundos ≤ 8 y castillo) | ídem (mundos 9 y 10, E041 ≥ 10) |
  |---|---|---|---|---|---|
  | 0 | +0,25 | −4 | 0,125 | 0xA8 | 0xB0 |
  | 1 | +0,75 | −3,5 | 0,133 | 0xA0 | 0xA8 |
  | 2 | +1,25 | −3 | 0,141 | 0x90 | 0xA4 |
  | 3 | +2 | −2 | 0,125 | 0x78 | 0xA0 |
  | 4 | −2 | −2 | 0,125 | 0x78 | 0xA0 |
  | 5 | −1,25 | −3 | 0,141 | 0x90 | 0xA4 |
  | 6 | −0,75 | −3,5 | 0,133 | 0xA0 | 0xA8 |
  | 7 | −0,25 | −4 | 0,125 | 0xA8 | 0xB0 |

- **Cada tick (6E7D)** [C]:
  - Primero, solo en cuatro salas, se borra al llegar a un borde, sin salpicadura (6E98):
    - castillo 147 y mundo 9, sala 5: x ≥ 0xD8;
    - castillo 144 y mundo 8, sala 15: x < 0x20.
  - Después aplica la gravedad.
  - Al llegar a la y límite se borra y crea una **salpicadura 0x6A** con B = 1 en su posición, con sonido 0x0D.
  - No mira tiles. Abanico parabólico de 8 trozos.

#### 0x6A — salpicadura (efecto)
- Contacto 0, invulnerable. **Quieta** (bit 4 = 0) **10 ticks** y se borra [C].
- Aspecto según B (E624) [C]:
  - 0: el del descriptor (sprite C4, color F);
  - 1: color 9;
  - 2: sprite C0.
- La crean el 0x08 (B = 1), el 0x1B (B = 1), el 0x0D (B = 0, en y = 0x90) y el 0x30 (B = 2, en (0x90, x+4)).
- La rutina común 78B7 que la crea pide el sonido 0x24 si el padre es 0x30 y el 0x0D en los demás casos [C].

#### 0x0D — pez saltador (E659: castillo 137–140)
- **Inicio (784B)** [C]:
  - **Lado:** IX+14 = R & 1. Con 0 salta hacia la derecha (vx > 0, sprite C8); con 1, hacia la izquierda (sprite D0).
  - **50 % (R bit 2 = 0), junto al jugador:**
    - x = jugador_x − 16 si salta a la derecha, o jugador_x + 16 si salta a la izquierda: nace detrás del jugador y salta hacia él;
    - fuerza k según la altura del jugador: jugador_y < 0x3D → 0; < 0x5D → 1; < 0x7D → 2; si no, 3.
  - **50 %, al azar:** x = R y k = R & 3.
  - **Límite de x según la sala** (78F2, con B del generador = sala − 136): 137: [0x20, 0xE0] · 138 y 139: [0, 0xE0] · 140: [0, 0xD0]. Después:
    - si x < mín + 0x30, salta a la derecha;
    - si x ≥ máx − 0x18, a la izquierda;
    - si no, se queda el lado sorteado.
  - y = **0x90**. vx = ±tabla 7927[k], vy = tabla 792F[k], gravedad **+0,5**.
  - Salpicadura 0x6A (B = 0) en (0x90, x) y **sonido 0x0D**.

  | k | vx (px/tick) | vy inicial | altura del salto | ticks en el aire | avance en x |
  |---|---|---|---|---|---|
  | 0 | 0,664 | −6,5 | 42 px (cima en y = 0x66) | 27 | ≈ 17 px |
  | 1 | 0,762 | −8,5 | 72 px (0x48) | 35 | ≈ 26 px |
  | 2 | 0,887 | −10 | 100 px (0x2C) | 41 | ≈ 35 px |
  | 3 | 1,141 | −11,5 | 132 px (0x0C) | 47 | ≈ 52 px |

- **Cada tick (78CD)** [C]:
  - vy += 0,5.
  - Para la y, el integrador solo suma la **parte entera** de vy (tipo 0x0D en B0ED). Con eso, la bajada termina exactamente en y = 0x90 (simulado).
  - Si vy ≥ 0 e y = 0x90: salpicadura (B = 0), sonido 0x0D y se borra.
  - Cuando vy = 0 exacto (en la cima), sprite + 4.
  - No mira tiles.
- [I] Con el jugador arriba (y pequeña), el salto es el **más bajo**. Es lo que dice el código, aunque parezca al revés.

#### 0x21 — caminante que cae (E65C)
- **Inicio (7E89)** [C]:
  - x = ((r − 16) & 0xDF) + 16, con r = (R/8) XOR E003. Sale en las bandas **16–47, 80–111, 144–175 o 208–239**.
  - y = 0, la del generador.
  - IX+14 = 0 si x < 0x80 (va a la derecha, vx = +1, sprite A0); si no, IX+14 = 1 (vx = −1, sprite A8). Es decir, **anda hacia el centro**.
- **Estado 0, andar a 1 px/tick** [C]:
  - Anima cada 8 ticks.
  - **Suelo:** mira (x+2, y+16) y (x+13, y+16) (7F0E; con y ≥ 0xB0 no hay suelo). Si los dos están libres: vx = 0, vy = **+2**, estado 1.
  - **Pared:** si el tile en (x+14, y+8) yendo a la derecha, o en (x+1, y+8) yendo a la izquierda, es sólido, da la vuelta (vx = −vx, attr 0x58).
  - No da la vuelta en los bordes de pantalla: sale y se borra.
- **Estado 1, caer a 2 px/tick constantes** (sin gravedad) [C]: al encontrar suelo, y &= F8, vy = 0, vx = ±1 según IX+14, y vuelve al estado 0.
- Desde y = 0 cae hasta el primer suelo bajo su x. Muere al caer por abajo o salir por un lado. Vida 2.

#### 0x35 — bola rodante (E665: castillo 47, 59; mundo 8: 2, 14)
- **Inicio (AB88)** [C]:
  - x como 0x21 (las mismas cuatro bandas); y = 0.
  - **vx = +1,5 si x < 0x80, −1,5 si no** (hacia el centro).
  - vy = 0, gravedad **+0,25**.
- **Cada tick** [C]:
  - Anima (7062).
  - **Pared:** si el tile en (x+16, y+8) yendo a la derecha, o en (x, y+8) yendo a la izquierda, es sólido, vx = −vx. Pasa también en el aire. En el borde de pantalla (x < 8 a la izquierda, x ≥ 0xF0 a la derecha) **no** rebota y se sale.
  - **Estado par, cayendo:** si el tile en (x+8, y+16) es sólido: y &= F8, vy = 0 (salvo si y ≥ 0xB0), **sonido 0x09**, estado 1. Si no, vy += 0,25, **sin tope**.
  - **Estado impar, rodando a 1,5 px/tick:** si falta el suelo bajo su centro (x+8, y+16), vuelve a caer con gravedad 0,25.
- Invulnerable; muere al salir de la pantalla.

#### 0x30 — saltador de lava (E65F: mundo 5, salas 7–10)
- **Inicio (7F2E)** [C]:
  - Altura objetivo k = R & 3 → y = tabla 80BC[k] = **0x24, 0x44, 0x64 o 0x84**.
  - x = (r & 0xBF) + 0x20, con r = (R/8) XOR E003: bandas **32–95 o 160–223**. y = 0x90.
  - IX+14 = 0 si x < 0x80 (mira a la derecha); si no, 1.
  - vy = **−4**, gravedad +0,5 (solo se aplica en los estados 1 y 4).
  - Contador de vueltas IX+18 = 2.
  - Salpicadura 0x6A (B = 2) en (0x90, x+4) y **sonido 0x24**.
  - Mide 32 px de ancho (2 sprites).
- **Estados** [C]:
  - **0:** sube a 4 px/tick hasta y = objetivo (exacto). Tarda 3, 11, 19 o 27 ticks.
  - **1:** arco de 16 ticks con gravedad 0,5: vy de −4 a +4. Sube unos 14 px más y acaba 4 px por debajo.
  - **2, caer a +4 constantes:**
    - Si y ≥ 0x90 → salpicadura y se borra.
    - Busca suelo en 5 columnas (x, x+8, …, x+32): algún tile sólido en la fila y+16 y ninguno en la fila y+8 (8087).
    - Al encontrarlo: y &= F8, vy = 0, IX+1A = x (punto de aterrizaje), vx = **±2** según IX+14, y pasa al estado 3.
  - **3, andar a 2 px/tick.** Da la vuelta (vx = −vx, IX+14 ^= 1, IX+18 −= 1) si pasa una de estas cosas:
    - no hay suelo delante: (x+32, y+16) a la derecha o (x, y+16) a la izquierda;
    - hay pared en (x+24, y+8) a la derecha o en (x−8, y+8) a la izquierda;
    - llega al borde: x ≥ 0xE0 a la derecha o x < 0x10 a la izquierda.

    Con IX+18 = 0 (ya dio 2 vueltas), al volver a pasar por IX+1A salta. También salta si da una tercera vuelta (IX+18 = FF). El salto es vy = −4, vx = 0 y estado 4.
  - **4:** el mismo arco de 16 ticks que el estado 1.
  - **5:** cae a +4 constantes **atravesando todo** hasta y ≥ 0x90; ahí hace la salpicadura y se borra.

#### 0x3E — volador en zigzag (E668: mundo 6, salas 2, 3, 4, 7, 8, 17, 18)
- **Inicio (A283)** [C]:
  - Ignora la posición del generador. **x = 0xB0 si E669 ≥ 5** (los 5 primeros de 10); si no, **x = 0x40**. y = **0x28**.
  - vy = +2. Anima cada 6 ticks (sprite ^ IX+1B, con IX+1B = 12, y attr ^ 1).
- **Movimiento** [C]: es determinista, sin aleatorio y sin mirar tiles.
  - **Estado 0:**
    - k = IX+18 & 7 (sube 1 en cada paso);
    - doble = A3D2[k] = {0,0,1,0,0,0,0,1};
    - fila = A3DA[k] = {3,1,2,3,1,3,0,1};
    - objetivo y = A3CE[fila] = {0x3C, 0x5C, 0x7C, 0x9C} si va hacia abajo, o A3CA[fila] = {0x2C, 0x4C, 0x6C, 0x8C} si va hacia arriba (IX+15).
    - Si x ≥ 0xC8 yendo a la derecha, o x < 0x28 yendo a la izquierda, cambia de sentido: sprite C4 o CC, IX+1B = 12 o 0x1C, attr 0x0F.
  - **Estado 1:** se mueve en vertical a ±2 px/tick hasta y = objetivo (exacto). Entonces vy = 0 y vx = ±1.
  - **Estado 2, ondulación de 14 ticks** a 1 px/tick en x:
    - en cada tick, y += d[i] con i = 0..13 y d = A3BA = {3,3,2,1,1,0,0,0, 0,0,0,−1,−1,−2}, es decir, baja 10 px y sube 4;
    - todo invertido si va hacia arriba;
    - con «doble» = 1, al llegar a i = 13 invierte el sentido vertical y repite desde i = 2.

    Al acabar invierte el sentido vertical (IX+15 ^= 1), vx = 0, vy = ±2 y vuelve al estado 0.
- **Trayectoria simulada desde x = 0x40** (tick: x, y):
  - 59: (0x40, 0x9C), 74: (0x4F, 0xA2), 117: (0x4F, 0x4C), 132: (0x5E, 0x46), 159: (0x5E, 0x7C), 186: (0x79, 0x82)…;
  - llega a x = 0xD0 hacia el tick 458 y vuelve.
- **Lo que se ve:** barre la pantalla de lado a lado entre x ≈ 0x22 y 0xD0, con subidas y bajadas en vertical a 2 px/tick entre las filas 0x26 y 0xA2, y una ondulación en cada parada.
- **No sale de pantalla** (simulado más de 2000 ticks): vive hasta que lo matan (vida 2). El generador pone un máximo de 5 o 10 en total.

### Tabla resumen

| Tipo | Gen. | Vida | Contacto | Movimiento | Muere | Hijos / sonido |
|---|---|---|---|---|---|---|
| 0x09 | E650 | inv. | 3 | cae (g 0,5, tope 6) y rebota con vy −4 una vez por piso; vx ±1 | sale de pantalla | sonido 0x09 al rebotar |
| 0x38 | E662 | 1 | 1 | sube a 5,5; se pega al techo 60 ticks; cae con vy +4 y sigue subiendo | sale por arriba | 0x0B al pegarse |
| 0x08 | E653 | inv. | 4 | sube a 4 hasta y = 48..111; flota 60 ticks (±1 px) | estalla | 8 × 0x1B; 0x6A al nacer (0x0D); 0x0E al estallar |
| 0x1B | (0x08) | 1 | 1 | parábola (tabla de 8 direcciones) | y límite (0x6A) o al tocar al jugador | 0x6A; 0x0D |
| 0x6A | — | inv. | 0 | quieta | 10 ticks | — |
| 0x0D | E659 | 1 | 1 | salto parabólico desde y = 0x90 (g 0,5; 4 fuerzas) | al volver a y = 0x90 | 0x6A; 0x0D |
| 0x21 | E65C | 2 | 2 | cae a 2 y anda a 1; gira en paredes | sale de pantalla | — |
| 0x35 | E665 | inv. | 3 | cae (g 0,25) y rueda a 1,5; rebota en paredes | sale de pantalla | 0x09 al aterrizar |
| 0x30 | E65F | 5 | 5 | sube a 4, arco, cae a 4 a una plataforma, anda a 2 (2 vueltas), salta y cae a la lava | y ≥ 0x90 | 0x6A; 0x24 |
| 0x3E | E668 | 2 | 3 | zigzag determinista: vertical a 2, ondulación a 1 en x, cambia de sentido en 0x28/0xC8 | solo por vida | — |

### Dudas
1. 0x38: el tope de velocidad deja vy fija en −5,5 en cuanto se vuelve negativa. Parece un descuido (el 0x09 lo hace bien), pero es lo que hace el código.
2. 0x0D: la fuerza más baja corresponde al jugador más arriba. Habría que comprobar en openMSX (castillo 137–140) si es así.
3. 0x09 y 0x38 atraviesan el piso o el techo en el que acaban de rebotar o colgarse (IX+8 / IX+1E). Comprobar en el emulador si es lo que se ve.
4. E604 se trata como contador de ticks (se incrementa en b00:52F5, dentro del bucle) [I].
