# IA de los jefes (fase 2)

Leyenda: **[C]** código · **[I]** interpretación · **[O]** observado en openMSX · **[G]** guía de GameFAQs.

## Resumen [C][O]

- **Invocación:** al escribir la palabra en la sala del jefe (estado 15, b00:4985), el número de jefe sale de la tabla b00:5E05[E041] y se guarda en E0D3 y luego en E700. **No coincide con el número de mundo**; el jefe 5 no se usa.

| Mundo | Jefe (E700) | Nombre [G] | Vida (b01:7F97) | Debilidad (b03:BD0A) |
|---|---|---|---|---|
| 1 | 1 | Bony Dragon | 24 | 0x21: espada + flecha |
| 2 | 4 | Sea Anemone | 40 | 0x21: espada + flecha (solo abierta) |
| 3 | 2 | Elusive Dragon | 64 | 0x09: espada + fuego |
| 4 | 11 | Jump Slime | 112 | 0x03: espada + mina |
| 5 | 10 | Donra | 72 | 0x21: espada + flecha (solo en la cabeza) |
| 6 | 3 | Arlyane | 56 | 0x29: espada + fuego + flecha |
| 7 | 7 | Big Lip | 64 | 0x11: espada + flecha de cerámica (solo boca o lengua) |
| 8 | 8 | Monster | 80 | 0x05: espada + fuego rodante |
| 9 | 6 | King Dragon | 80 | 0x11: espada + flecha de cerámica |
| 10 | 9 | Galious | 248 | 0x3F: todas, **pero solo con la Cross** (b03:BBA8 lee E07A; es la única lectura de esa flag en la ROM) |

- **Vida:** con el Holy Water (E076) se divide entre 2 (b01:7F63).
- **Bits de BD0A:** bit 0 espada, 1 mina, 2 fuego rodante, 3 fuego, 4 flecha de cerámica, 5 flecha. El proyectil lleva el número del arma elegida (E510).
- **Daño:** la mina resta 2 y lo demás 1. Si el arma no vale, suena 0x1D. Además, cada jefe tiene su propio test de colisión (tabla b03:BB7A).
- **Las debilidades coinciden con los consejos de la guía en los 10 jefes.**
- **Ritmo** [O]: todo el bucle del juego (jefe y enemigos) corre unos 26–30 ciclos por segundo, uno cada dos frames. Breakpoint en la rutina del Bony Dragon: 26 por segundo. IA de enemigos: 416, es decir, 16 huecos × 26. Los tiempos en "t" o "frames" de las fichas son ciclos del juego.
- **Código:** las tablas de inicio y actualización están en b01:7F82 y b01:7F40, y el código de cada jefe en el banco 2. Para moverlos se usan las ayudas de velocidad del banco 6 (b01:7FA3–7FCD). Desensamblado: `tools/extract/boss_ai.py` → `extracted/ai/boss_NN.asm`.

**Fichas.** Las escribieron dos agentes a partir del desensamblado (re-trazado por ellos, porque la primera versión de la herramienta trataba como código las tablas de `CALL 4140`; ya está corregido). Solo se han comprobado en el emulador el número y la vida del jefe 1 y el ritmo de actualización.

## IA de los jefes — grupo A (jefes 1, 2, 3, 4 y 6) + tabla de vulnerabilidades

Leyenda: **[C]** leído en el código · **[I]** interpretación · **[G]** guía de GameFAQs.
Correspondencia jefe ↔ mundo (corrección del coordinador, b00:4985 / tabla 5E05[E041]): jefe 1 = mundo 1 (Bony Dragon),
jefe 2 = mundo 3 (Elusive Dragon), jefe 3 = mundo 6 (Arlyane), jefe 4 = mundo 2 (Sea Anemone), jefe 6 = mundo 9 (King Dragon).

Método: los `.asm` de `extracted/ai/boss_*.asm` llevan ruido (desensamblan las tablas de salto de 4140 como código
y mezclan trozos del banco 6 con direcciones del banco 2). Se ha vuelto a desensamblar con un trazador propio
(grupo 0, siguiendo las tablas de `CALL 4140`) en el scratchpad (`disb.py`, `runb.py`, `linb.py`).
"tick" = una ejecución de la rutina de actualización. Velocidades en px/tick (8.8: 0x0100 = 1 px).

### Notas comunes [C salvo indicación]

- **Despachador.** b01:7EFC según E0D2: 1 → 7FD0 (E0D2++, E640=0x41, 5CC4 con 0x90); 2 → 7FDB (cuenta atrás de E640,
  cada 8 ticks dibuja algo con la tabla 8003; al acabar espera E620=0 y E0D2++); 3 → 7F56 (E0D2++, E700=E0D3,
  vida = 7F97[E700] (/2 con E076), IX+1=0, IX+2=F0 y llama al inicio de la tabla 7F82); ≥4 → cada tick:
  b06:B3E7/B3F7 (apagan los bits 5/4 de IX+3 cada 16/32 ticks: parpadeo de golpe [I]), si E0D5 (libro) ≠ 0 no hace
  nada más; si no, actualización (tabla 7F40[E700−1]) y, si IX+2 bit 4, integra la velocidad (7F2F → b06:B0ED).
- **¿1 de cada 2 frames?** 7EFC se llama desde el bucle de juego b00:53ED **sin comprobar paridad**; el bucle de
  enemigos (b06:B3C5, llamado en b00:53C6, mismo bucle) tampoco la comprueba [C]. Así que el jefe se actualiza
  **al mismo ritmo que los enemigos** (30 veces/s medido para estos), es decir, la mitad de ritmo se debe al bucle
  de juego entero y no a un filtro propio [I]. Los tiempos de abajo están en ticks de IA.
- B0ED suma vy (+0C/0D) a y (+4/5) y vx (+0E/0F) a x (+6/7); para E700 = 0x0A (jefe 10) solo suma el byte alto [C]
  (la lista de tipos 0x24, 0x25, 0x0D, 0x2D, 0x0A es la de enemigos, pero se aplica también al jefe 10 por coincidencia [I]).
- **IX+2 bit 5 = el jefe se puede golpear**: b03:BB5D sale si E702 bit 5 = 0 [C]. F0 y E0 lo tienen puesto.
- Crear hijo: `CALL 407D` → 424C → b04:6551 con A = tipo, E = x, D = y, B → E624 (parámetro del hijo) [C].
- Sonidos: 4059 y 4E1F son la misma rutina (`sound_request`; 4059 es `JP 4E1F`) [C].
- 8100 = `LD A,R; AND 7F; ADD 50` → 80..207 [C]. R no es un buen aleatorio [I].

---

#### Jefe 1 — Bony Dragon (mundo 1)
- **Vida:** 24 (12 con Holy Water) [C].
- **Inicio (8013):** y = 0x68, IX+1E = 0x68 (suelo), x = 0x98; IX+1D = 80..207 (tiempo hasta saltar);
  IX+1F = 16..47 (`R&1F+10`, tiempo hasta lanzar huesos); aceleración y = 0x0080 (0,5 px/tick²);
  vx = ±0,5 hacia el jugador (negativa si jugador_x < x); sprite 0x0C. IX+1 = 0, IX+2 = F0 [C].
- **Comportamiento (IX+1):**
  - **Estado 0 (pasea):** cada tick DEC +1D; al llegar a 0 → estado 1 [C].
    - 810E: y = 0x68, sprite 0x0C. Si va a la derecha y E604 bit 4 = 0 (16 de cada 32 ticks): sprite 0x0E e y = 0x60;
      si va a la izquierda y E604 bit 4 = 0: sprite 0x0D [C]. (Animación de andar, con un cambio de altura de 8 px
      al ir a la derecha [I].)
    - DEC +1F; a 0 → lanza huesos (80DC) y recarga +1F = 16..47 [C].
    - 8136: rebota entre x = 0x92 (yendo a la izquierda, x < 0x92) y x = 0xA0 (yendo a la derecha, x ≥ 0xA0)
      invirtiendo vx [C]. Es decir, apenas se mueve: 14 px de recorrido a 0,5 px/tick [C].
  - **Estado 1 (salto):** al entrar: sprite 0x0D, vx = 0, vy = −7 px/tick (0xF900), IX+18 = altura objetivo
    `((R&3)+3)·16+5` = 0x35, 0x45, 0x55 o 0x65 [C].
    - Cada tick: cada 4 ticks sprite ^= 2 (aleteo), cada 16 ticks sonido 0x0A, vy += 0,5 [C].
    - Subiendo (vy < 0): si y < IX+18, invierte vy (empieza a caer con la misma rapidez) y lanza huesos [C].
      Con vy = −7 y g = 0,5, la cima natural queda unos 45 px más arriba (y ≈ 0x3A) [I: cálculo], así que con
      objetivo 0x35 no rebota ni lanza; con 0x45/0x55/0x65 corta el salto y lanza en el aire [I].
    - Bajando: al llegar a y ≥ IX+1E (0x68): +1D = 80..207, sprite 0x0C, IX+8 = 60, estado 2, se para (RES 4),
      vy = 0, sonido 0x21 [C].
  - **Estado 2 (aterriza):** espera 60 ticks; después estado 0, vuelve a moverse (SET 4), IX+19++ y elige vx = ±0,5:
    hacia el jugador 3 de cada 4 veces, y la 4.ª (IX+19 & 3 = 0) en sentido aleatorio (bit 0 de R) [C].
- **Ataques / hijos:** 80DC crea **3 huesos (0x54)** en (x, y) con B = 2, 1, 0 y suena 0x11 [C]. 0x54 (b04:6EED,
  tabla 6F43 por E624): vx = −2,25 / −1,19 / −0,125; vy = −4 / −3,75 / −3,5; gravedad 0,1875 [C]. **Siempre hacia
  la izquierda** (el jefe está a la derecha de la sala) [C]. Si el hueso tiene IX+1C ≠ 0 se parte en dos 0x55
  (vx −1,19 y −0,125) y desaparece [C]; quién pone +1C no se ha seguido [I: el impacto con el suelo o con un arma].
- **Debilidad:** BD0A[1] = 0x21 → **flecha** y espada [C]. Siempre se le puede golpear (IX+2 = F0 fijo) [C].
- **Contraste con la guía:** "usar flechas" [G] ✓ con BD0A [C].
- **Dudas:** quién pone IX+1C del hueso 0x54 (b06:ACE8); qué es exactamente el sprite 0x0E con y = 0x60 (810E).

#### Jefe 2 — Elusive Dragon (mundo 3)
- **Vida:** 64 (32) [C].
- **Inicio (8279, también reutilizado en cada reaparición):** IX+1 = 5; elige IX+1D (posición) según
  `(IX+19 >> 2) & 3` (IX+19 = nº de apariciones; no se pone a 0 en el inicio, se supone 0 por la RAM limpia [I]):
  - caso 0: banda de altura del jugador (8364: jugador_y < 0x40 → 0, < 0x60 → 1, < 0x80 → 2, < 0xA0 → 3, si no 4)
    y lado = bit 0 de E503 [C] (E503 = orientación del jugador [I]).
  - caso 1: banda − 1 + tabla 8349[R&7] (0,2,1,0,1,0,2,1), limitada a 1..4 (0→1, −1→0, ≥5→4); lado derecho si
    jugador_x ≥ 0x80 [C].
  - caso 2: altura 0 (jugador_y < 0x70) o 4; lado derecho si jugador_x < 0x80 [C].
  - caso 3: aleatorio 0..15 [C].
  Así, las apariciones 1-4 usan el caso 0, las 5-8 el 1, etc., y se repite cada 16 [C].
  - IX+1D bit 0 = 1 → **lado derecho**: x = 0xF8, sprite 6, vx = −2 px/tick, IX+15 = 1 [C].
  - bit 0 = 0 → **lado izquierdo**: x = 0, sprite 8, vx = +2 (no se usa: RES 4), IX+9 bit 5 = 1, IX+0B = 1 [C].
  - y = tabla 8341[IX+1D >> 1] = 0x20, 0x40, 0x60, 0x80, 0xA0, 0x20, 0xA0, 0x40 [C].
  - E61B = 1: arranca la carga por pasos de b00:587B (7 pasos a la derecha, hasta 0x2C a la izquierda) [C]
    [I: sube gráficos/tiles del cuerpo].
- **Comportamiento (IX+1, tabla 8176):**
  - **5 (preparar):** llama a 587B cada tick hasta E61B = 0; entonces IX+2 = F0 (derecha, se mueve) o E0
    (izquierda, quieto: crece con tiles [I]) y estado 0 [C].
  - **0 (asoma):** sprite ^= 1 cada 8 ticks. Lado izquierdo: IX+0B++ cada 4 ticks (cuello de 8 px por paso, 837A:
    punta = x + 8·IX+0B) [C]. Lado derecho: avanza a −2 px/tick; la longitud es IX+15 [C].
    - Si el jugador está delante (0 ≤ jugador_y − y < 25 y 0 ≤ distancia horizontal a la cabeza − 8 < 104) y la
      longitud ≥ 12: cada tick sortea R&3 → 0 sigue, 1 o 3 ataca (estado 1), 2 se retira (estado 3) [C].
    - Si no, al llegar la longitud a 16 → ataca (estado 1) [C].
  - **1 (abre la boca):** se para, sprite 10 (der.) / 11 (izq.), 30 ticks; luego estado 2, IX+8 = 96 y crea la
    llamarada 0x57 en (cabeza: izq. punta+8 / der. (x&F8)−16, y+8) con B = 0 (izq.) o 1 (der.) [C].
  - **2 (escupe):** 96 ticks; luego sprite 8 (izq.) / 6 (der., vuelve a moverse), invierte vx, estado 3 [C].
  - **3 (se retira):** izq.: IX+0B−− cada 4 ticks hasta < 2; der.: +2 px/tick hasta x ≥ 0xF8. Al terminar:
    IX+2 = 0 (**invisible e invulnerable**), IX+8 = 30, estado 4 [C].
  - **4:** IX+19++ y vuelve a elegir sitio (8279) → estado 5 [C]. El IX+8 = 30 de antes no se usa [C].
- **Ataques / hijos:** llamarada 0x57 (b06:AD51), sonido 0x1C al crearla [C]. Con B = 1 vive 12 ticks y avanza
  8 px/tick hacia la izquierda (≈ 96 px); con B = 0 no mueve x, solo avanza su sprite cada tick [C]
  [I: la del lado izquierdo crece con tiles hacia la derecha].
- **Debilidad:** BD0A[2] = 0x09 → **fuego** y espada [C]. Solo mientras está visible (IX+2 bit 5) [C]; la
  colisión es especial (b03:BB90 → BC15/BDD7), con anchura según IX+0B o IX+15 [C].
- **Contraste con la guía:** "atacar con fuego" [G] ✓ [C]. "Elusive": aparece por un lado u otro a distintas
  alturas y se esconde [C].
- **Dudas:** **en el lado derecho nada del código del jefe incrementa IX+15** (solo 8305 lo pone a 1), y sin embargo
  se compara con 12 y 16; tiene que actualizarlo otra rutina (¿el dibujo o b03:BD70?) [duda; sin ella el dragón
  derecho no pararía]. Significado exacto de E503 (caso 0). Qué hace 587B (b00) en cada paso.

#### Jefe 3 — Arlyane (mundo 6)
- **Vida:** 56 (28) [C].
- **Inicio (8384):** IX+1F = 31..158 (`(R&7F+50)&7F + 1F`) [C]; y = 0x90, x = 0x60, sprite 0x3A; IX+8 = R&1F;
  vx = ±1 px/tick al azar (R&1 = 0 → derecha) [C].
- **Comportamiento:**
  - **Estado 0 (camina):** límites x ∈ [0x29, 0x8C]: al pasarlos se recoloca y se da la vuelta (8436) [C].
    DEC +1F; a 0 → estado 1 [C]. Cada 8 ticks anima: sprite 0x3A + (IX+19&3, 3→1) = 3A, 3B, 3C, 3B [C].
    - Jugador a su altura o por debajo (jugador_y ≥ 0x90): cada 32 ticks va a la derecha si jugador_x − 8 ≥
      (x&F8) + 0x28, si no a la izquierda [C] (lo persigue tomando como referencia x + 40 [I: el centro del jefe]).
    - Jugador por encima: DEC IX+8; a 0 → IX+8 = R&1F y sentido aleatorio [C] (si R&1F = 0 tarda 256 ticks [I]).
  - **Estado 1 (dispara):** al entrar se para y pone sprite 0x3C; IX+1F = 15, 31, 47 o 63 (`((R&3)+1)·16 & 30 + F`) [C].
    Cada tick DEC +1F; cuando +1F & 0x0F = 0 (y ≠ 0) lanza una ráfaga; a 0 vuelve a estado 0, se mueve y
    recarga +1F = 31..158 [C]. Resultado: 0, 1, 2 o 3 ráfagas (para 15, 31, 47, 63) separadas 16 ticks [I: cálculo].
- **Ataques / hijos:** cada ráfaga = **6 proyectiles 0x58** en (x+0x20, y+0x10), B = 5..0, sonido 0x12 [C]. 0x58
  (b06:ADB2): apuntado al jugador (B1DB con DE = 0808) más un desvío (E624−3)·8 + (R&F − 8) en unidades de ángulo
  de la tabla [C]: abanico de 6 tiros centrado en el jugador con algo de dispersión [I]. Rapidez ≈ (0x70 + 8·E045)/32
  ≈ 3,5 px/tick con E045 = 0 [C de enemy_ai.md + I].
- **Debilidad:** BD0A[3] = 0x29 → **flecha, fuego** y espada [C]. Siempre golpeable [C].
- **Contraste con la guía:** "Aphrodite (3 disparos), fuego" [G]: fuego ✓ [C]; además admite flecha [C]. "3 disparos"
  podría corresponder al máximo de 3 ráfagas [I].
- **Dudas:** escala de los ángulos de B1DB/B1FF (cuánto abre el abanico).

#### Jefe 4 — Sea Anemone (mundo 2)
- **Vida:** 40 (20) [C].
- **Inicio (846E):** quieto (RES 4), y = 0x48, x = 0x68, IX+8 = 80..207, sprite 0x10 [C].
- **Comportamiento:**
  - **Estado 0 (cerrada):** DEC IX+8; cuando IX+8 < 20 pone sprite 0x11 (aviso [I]); a 0 → IX+8 = 80..207,
    sprite 0x12, estado 1 [C].
  - **Estado 1 (abierta, dispara):** DEC IX+8; cada 4 ticks alterna sprite 0x12 ↔ 0x13; cada vez que pasa a 0x13
    (cada 8 ticks) lanza 1 proyectil [C] → unas 10-26 esporas por apertura [I: cálculo]. A 0: sprite 0x11,
    IX+8 = 20, estado 2 [C].
  - **Estado 2 (cerrándose):** 20 ticks; luego estado 0, IX+8 = 80..207, sprite 0x10 [C].
- **Ataques / hijos:** 0x59 en (x+0x10, y−8) con B = R&15, sonido 0x12 [C]. 0x59 (b06:AE03): vy = tabla AE2C[B>>1]
  (−4,5 / −8 / −7,5 / −7 / −6,5 / −6 / −5,5 / −5), |vx| = tabla AE3C[B>>1] (3, 0, 2, 3, 1, 4, 3, 2) con el signo
  del bit 0 de B, gravedad 0,5 [C]: surtidor hacia arriba que cae en parábola [I].
- **Debilidad:** BD0A[4] = 0x21 → **flecha** y espada [C]. Además el test de colisión del jefe 4 (b03:BB96) solo
  acepta el golpe si el sprite IX+0A ≥ 0x12, es decir, **solo cuando está abierta (estado 1)** [C]. (La explosión de
  mina usa otra tabla, BC33, sin esa condición, pero la mina no está en BD0A[4] [C].)
- **Contraste con la guía:** "disparar al pin verde" [G] ✓: flecha [C] y solo cuando se abre [C] (el "pin" será la
  parte que se ve abierta [I]).
- **Dudas:** si la espada pasa por la misma tabla BB73 (y por tanto también exige que esté abierta) [I: parece que sí,
  BD55 → BB5D].

#### Jefe 6 — King Dragon (mundo 9)
- **Vida:** 80 (40) [C].
- **Inicio (895C):** IX+2 = F0, y = 0x70, x = 0x58, sprite 0x34; IX+16 = 0x10 (aquí es un temporizador, no la
  aceleración); vy = 0; IX+1B = 0; IX+17 = 0; vx = +0,5 px/tick (0x0080); IX+19 = 2; IX+1A = 0x30 [C].
  Crea **4 nubes 0x69** en y = 0x28, x = 0x40, 0x60, 0x80, 0xA0 [C] (0x69 no se puede dañar, items_enemies.md).
- **Comportamiento (cada tick, antes de la máquina de estados):**
  - **Cabeza (89F5):** DEC IX+16; a 0 → IX+16 = ((R>>>3) XOR E003)&1F + 3 (3..34 ticks) y cambia de pose: si estaba en
    0x35 pasa a 0x34 o 0x36 al azar; si estaba en 0x34/0x36 vuelve a 0x35. IX+17 = sprite − 0x34 (0, 1, 2) [C]
    [I: cabeza arriba / centro / abajo].
  - **Ciclo andar/parar (8A22):** cada 48 ticks (IX+1A) alterna IX+1B. Con IX+1B = 0 se mueve (SET 4); con 1 se para
    (RES 4) y DEC IX+19: a 0 → IX+19 = 16, IX+16 = 24 (mantiene la pose) y **escupe** [C]. Primer disparo 2 ticks
    después de la primera parada y luego cada 16 ticks mientras está parado (≈ 3 por parada) [I: cálculo; IX+19 no
    se reinicia entre paradas].
  - **Estado 0:** si IX+1C ≠ 0 (golpe válido, lo pone b03:BD03 solo para el jefe 6) → IX+1C = 0, vx = 0, IX+18 = 32,
    estado 1 [C]. Si no, cuando x & 7 = 0 y x ∉ [0x50, 0x78) invierte vx (CPL del byte alto: +0,5 ↔ −0,5) [C]:
    pasea entre x ≈ 0x48 y 0x78 [I]. (Estando parado en un borde lo invertiría cada tick [I].)
  - **Estado 1 (retroceso por golpe, 32 ticks):** vx = +0,5 si x < 0x78, si no 0 (lo empuja a la derecha mientras
    el ciclo esté en fase de moverse) [C]. Al acabar: estado 0, vx = −0,5 [C].
- **Ataques / hijos:** llamarada **0x57** (la misma del jefe 2) con B = 2 en (x + dx, y + dy) según la pose
  (tabla 8A6E: pose 0 → (−8, +0x18), 1 → (−0x10, +0x18), 2 → (−0x18, +0x38)) [C]. Con B = 2 vive 8 ticks y avanza
  8 px/tick a la izquierda (≈ 64 px); sonido 0x1C [C].
- **Debilidad:** BD0A[6] = 0x11 → **flecha de cerámica** y espada [C]. Siempre golpeable (F0) [C]; la colisión usa
  3 cajas (b03:BBBF: tablas BBDB, BBE8, BBF5 por sprite) [C] [I: cabeza y cuerpo]. La explosión de mina no tiene
  test para este jefe (BC33[6] = 0000) [C].
- **Contraste con la guía:** "flechas de cerámica desde la escalera, cuatro nubes de tormenta encima" [G]: cerámica ✓,
  4 nubes 0x69 ✓ [C]. Que convenga la escalera encaja con que la llamarada sale a la izquierda a la altura de la
  cabeza [I].
- **Dudas:** comportamiento de las nubes 0x69 (b04:74BB / b06:7511; tienen un caso especial si E041 = 10 y sala 0x19,
  que sería la sala del jefe [I]). Cajas exactas de BBDB-BBF5.

---

### Daño al jefe y tabla de vulnerabilidades b03:BD0A

#### Flujo [C]
- **Proyectiles del jugador** (b03:BB24): recorre 6 huecos E540 + 0x20·n. Sale si el jugador está en la acción 5 o si
  E02C ≠ 0 (jefe ya derrotado). Toma huecos con tipo 1..5 e IY+2 bit 0. **El tipo del proyectil (IY+0) es el arma
  elegida E510** (b03:A740 lo copia al disparar): 1 flecha, 2 flecha de cerámica, 3 fuego rodante, 4 fuego, 5 mina.
- BB5D: exige IX+2 bit 5 (golpeable) y las dos pruebas de solapamiento B528/B53D; luego un test por jefe (tabla
  BB7A; para la mina que ya explotó, IY+2 bit 2, tabla BC33):
  1 genérico (BD70) · 2 especial dragón (BC15 + BDD7) · 3 genérico · 4 **solo si sprite ≥ 0x12** · 5 nada ·
  6 tres cajas (BBBF) · 7 **solo si sprite ≥ 0x19** · 8 genérico · 9 **solo si E07A ≠ 0** · 10 especial (BC02, con
  IX+19 como sprite) · 11 genérico.
- Si hay contacto → BC75: mina (tipo 5) → BD15: si no había explotado, la hace explotar (BD4D: AE4B + sonido 0x27),
  SET 4 IX+3, comprueba arma con C = 5 y, si vale, **resta 2**. Otro tipo: (tipo 2 llama además a B5AC; los demás a
  AD76, probablemente para destruir el proyectil [I]) y entra en BC90 con A = 0, C = tipo.
- **Espada** (b03:BD55): si E51B ≠ 0 (lo pone A761 al empezar el golpe de espada) → B8FB, BB5D; si toca, E51B = 0
  y BC90 con A = 1.
- **BC90:** BCC1 (¿arma permitida?) → si no, retorna; si sí, sonido 0x27, **resta 1** a E710; a 0 → BD32.
- **BCC1:** elige la máscara B: A ≠ 0 (espada) → 0x01; si no, por C: 1 → 0x20, 2 → 0x10, 3 → 0x04, 4 → 0x08,
  otro (5 = mina) → 0x02. Hace `AND` con BD0A[E700−1]. Si es 0: con mina no hace nada; con otra arma suena 0x1D
  (rebote) [I]. Si vale y E700 = 6, pone IX+1C = 1 (aviso de golpe para el King Dragon).
- **BC9F (tras cada golpe):** E1C5 = 0 si vida > máx/4, 1 si máx/8 < vida ≤ máx/4, 2 si vida ≤ máx/8 (máx = 7F97,
  sin dividir por Holy Water) [C] [I: grado de daño, quizá color o música].
- **BD32 (derrotado):** E1C5 = 0, E02C = 1, sonido 0x84, y `JP 5D12` con A = 0x91, o 0x8B si E041 = 11 (mundo 10,
  Galious) [C] [I: música de victoria distinta].

#### Bits de BD0A [C]
| bit | máscara | arma |
|---|---|---|
| 0 | 0x01 | espada |
| 1 | 0x02 | mina (tipo 5) |
| 2 | 0x04 | fuego rodante (tipo 3) |
| 3 | 0x08 | fuego (tipo 4) |
| 4 | 0x10 | flecha de cerámica (tipo 2) |
| 5 | 0x20 | flecha (tipo 1) |

El tipo 2 es la flecha de **cerámica** (lo confirman King Dragon y Big Lip, para los que la guía pide cerámica):
`docs/player.md` la llamaba "flecha doble"; ya está corregido.

#### Byte de cada jefe (BD0A = 21 09 29 21 00 11 11 05 3F 21 03) [C]
| jefe | nombre (mundo) | byte | armas | guía [G] |
|---|---|---|---|---|
| 1 | Bony Dragon (1) | 0x21 | flecha, espada | flechas ✓ |
| 2 | Elusive Dragon (3) | 0x09 | fuego, espada | fuego ✓ |
| 3 | Arlyane (6) | 0x29 | flecha, fuego, espada | fuego ✓ |
| 4 | Sea Anemone (2) | 0x21 | flecha, espada (solo abierta) | flecha al pin ✓ |
| 5 | (sin uso) | 0x00 | ninguna | — |
| 6 | King Dragon (9) | 0x11 | cerámica, espada | cerámica ✓ |
| 7 | Big Lip (7) | 0x11 | cerámica, espada (solo sprite ≥ 0x19) | cerámica a la lengua ✓ |
| 8 | Monster (8) | 0x05 | fuego rodante, espada | fuego rodante ✓ |
| 9 | Galious (10) | 0x3F | todas (solo con E07A ≠ 0) | flechas + fuego rodante, hace falta la cruz ✓ [I: E07A = cruz] |
| 10 | Donra (5) | 0x21 | flecha, espada | — |
| 11 | Jump Slime (4) | 0x03 | mina, espada | minas ✓ |

La **espada** está permitida para todos los jefes (bit 0 siempre a 1) [C]; que en la práctica sirva depende de poder
alcanzarlos y de los tests de colisión [I].

---

### Tabla resumen

| jefe | nombre | vida | patrón | ataque | debilidad (BD0A) |
|---|---|---|---|---|---|
| 1 | Bony Dragon | 24 | pasea 14 px a 0,5 px/t; cada 80-207 t salta (vy −7, g 0,5), 60 t de pausa al caer | 3 huesos 0x54 en parábola, siempre a la izquierda, cada 16-47 t y en la cima del salto | 0x21 flecha + espada |
| 2 | Elusive Dragon | 64 | asoma por la izquierda (tiles) o la derecha (2 px/t) a 5 alturas, se esconde invulnerable y reaparece en otro sitio | llamarada 0x57 tras 30 t con la boca abierta | 0x09 fuego + espada |
| 3 | Arlyane | 56 | camina a 1 px/t en x 0x29-0x8C, sigue al jugador si está abajo | 0-3 ráfagas de 6 tiros 0x58 en abanico al jugador (~3,5 px/t) | 0x29 flecha + fuego + espada |
| 4 | Sea Anemone | 40 | fija; cerrada 80-207 t, abierta 80-207 t, 20 t cerrándose | una espora 0x59 cada 8 t mientras está abierta | 0x21 flecha + espada, solo abierta |
| 6 | King Dragon | 80 | 48 t paseando (±0,5 px/t, x 0x48-0x78) / 48 t parado; cabeza en 3 poses; retrocede 32 t al recibir un golpe | llamarada 0x57 (8 t, a la izquierda) cada 16 t mientras está parado; 4 nubes 0x69 al empezar | 0x11 cerámica + espada |

## IA de los jefes 7, 8, 9, 10 y 11 (grupo B)

Correspondencia (corrección del coordinador, confirmada en b00:4979-4985: `E0D2/E0D3 = 1 / 5E05[E041]`, tabla
`19 05 01 04 02 0b 0a 03 07 08 06 09`): jefe 7 = mundo 7 (Big Lip), 8 = mundo 8 (Monster), 9 = mundo 10 (Galious),
10 = mundo 5 (Donra), 11 = mundo 4 (Jump Slime) [C]. b01:7F56 hace `E0D2++ ; E700 = E0D3` [C].

**Aviso sobre `extracted/ai/boss_NN.asm`:** están incompletos. Las tablas de saltos que siguen a `CALL 4140` salen
desensambladas como instrucciones, y las ayudas de b06 (B137/B13E/B15F) se leyeron del banco equivocado (grupo 0) [C].
Para estas fichas he desensamblado los rangos completos de b02 (grupo 0) y las ayudas de b06 en el grupo 1.

### Notas comunes (aplican a todas las fichas)

- **Frecuencia:** b00:53ED llama a b01:7EFC una vez por pasada del bucle principal. Ni 7EFC ni 7F38 miran la paridad del
  frame (a diferencia de los enemigos normales) [C]. → [I] la IA del jefe corre **cada frame**; los hijos son enemigos
  normales (huecos E800+) y siguen la regla de 1 de cada 2.
- Antes de la IA, b06:B3E7/B3F7 borran los bits 5 y 4 de IX+3 cuando el contador de frames E604 es múltiplo de 16/32
  [C]. El bit 4 lo pone un golpe de mina (b03:BD1C) [C].
- **Integración (b06:B0ED):** y += vy y x += vx en 8.8, pero para los tipos 0x24, 0x25, 0x0D, 0x2D y **0x0A** solo se
  suma el byte alto de vy. Como E700 = 10 (0x0A) para Donra, **a Donra se le aplica esa regla**: su vy se mueve en
  píxeles enteros (IX+0D) [C]. [I] Seguramente es casual (la comprobación es para tipos de enemigo).
- **Golpe de un arma del jugador (IY = proyectil, b03:BB24…):** hace falta que IX+2 bit 5 esté a 1 (b03:BB5D) [C];
  luego se usa una prueba de choque por jefe (tabla b03:BB7A, índice E700−1):
  1 BBAD · 2 BB90 · 3 BBAD · 4 BB96 · 5 — · 6 BBBF · **7 BB9F** · **8 BBAD** · **9 BBA8** · **10 BC02** · **11 BBAD** [C].
  BBAD es la caja normal, sacada de b02:91EC[E700] por el sprite E70A [C]. Con una mina explotando (tipo 5 con
  IY+2 bit 2) se usa otra tabla, b03:BC33 (7 BC4E, 8 BBAD vía BC69, 9 BC69, 10 BC57, 11 BC60) [C].
- **Tabla de armas (b03:BCC1/BD0A):** espada (E51B, b03:BD55) → bit 0; proyectil tipo 1 → bit 5 (0x20), tipo 2 → bit 4,
  tipo 3 → bit 2, tipo 4 → bit 3, tipo 5 → bit 1 [C]. [I] Por el orden del menú (items_enemies.md) los tipos 1-5 son
  flecha, flecha doble/de cerámica, fuego rodante, fuego y mina. Las cinco fichas de abajo encajan con la guía con esta
  correspondencia, así que la considero bastante segura (aun así sigue siendo [I]).
  BD0A: 7 = 0x11, 8 = 0x05, 9 = 0x3F, 10 = 0x21, 11 = 0x03 [C].
  Si el arma no vale: sonido 0x1D, salvo con la mina, que no suena [C]. Si vale: sonido 0x27 y E710 −1; la mina resta 2
  (b03:BD27-BD2C) [C]. Al llegar a 0: sonido 0x84, E02C = 1 y música 0x91 (o **0x8B si E041 = 11, es decir, Galious**,
  b03:BD3F) [C]. E1C5 = 0/1/2 según la vida quede por encima de 1/4, entre 1/8 y 1/4, o por debajo de 1/8 de la máxima
  [C]. [I] Es el indicador de daño.
- Creación de hijos: b00:407D → 424C → b04:6551 con A = tipo, E = x, D = y, B = parámetro [C].

---

#### Jefe 7 — Big Lip (mundo 7; guía: NAWABRA → Big Lip)
- **Vida:** 64 (32 con Holy Water) [C].
- **Inicio (b02:8627):** IX+2 &= F0 (queda F0: se mueve y se le puede golpear), x = 0x68, y = 0x40, sprite E70A = 0x18,
  +24 = 0x30 (ancho para el borde derecho), +1E = +1F = 1, +30 = 0x10 (temporizador de disparo), +23 = 0xB0
  (temporizador de lengua) [C]. Fase 0, sin hijos [C].
- **Comportamiento (b02:864E):**
  - **Antes de la máquina de estados**, una animación en curso (+20 bit 0) cuenta +21 hacia atrás. Con la lengua
    (+22 = 3) el sprite pasa por 0x1A (32 f) → 0x1B (32 f) → 0x1A (32 f), luego vuelve a 0x18 y se pone IX+2 bit 4
    (se mueve otra vez) [C]. Si +22 = 0 (disparo) dura +21 frames y vuelve a 0x18 [C].
  - Si **E500 = 4**, salta a 8765, que inicia la lengua: +20 = 1, +22 = 3, +21 = 32, sprite 0x1A y **se para**
    (IX+2 bit 4 = 0). Lo hace cada frame mientras siga E500 = 4 [C]. Quien pone E500 = 4 es b03:BA7C, una rutina que
    usa E70A/E707 [C]. → [I] es el retroceso del jugador al tocar al jefe, así que Big Lip saca la lengua cuando te golpea.
  - Fase 0 (86A4): si +1E bit 0 y rnd&1 → dirección E71C = rnd&1, +1B = 8, +1F = 0, vx = +1 px/f (E71C = 0) o
    −1 px/f, fase 1. Si no → fase 2 [C].
  - Fase 1 (86C1), movimiento horizontal: los primeros 8 frames no mira los bordes. Después rebota (invierte vx) si
    x < 0x18 yendo a la izquierda o x + 0x30 ≥ 0xE0 yendo a la derecha [C]. Al volver a x = 0x68 se para:
    +1F = 1, vx = 0, fase 2 [C]. [I] Cada pasada es un viaje de ida y vuelta desde el centro: unos 144 frames.
  - Fase 2 (86E0): si +1F bit 0 y rnd&1 → cambia E71D, +1B = 8, +1E = 0, vy = +1 (E71D = 0) o −1 px/f, fase 3.
    Si no → fase 0 [C].
  - Fase 3 (86FD), movimiento vertical entre y = 0x40 y 0x70: rebota fuera de ese rango. Al llegar exactamente a
    0x40 o 0x70 se para: +1E = 1, vy = 0, fase 0 [C].
  - Disparo y lengua solo **mientras se mueve** (fases 1/3, en los frames que no termina el tramo) y si no hay lengua
    en curso (8720) [C]:
    - Cada 16 frames (+30): sprite 0x19, +20 = 1, +21 = 24, crea un **0x65** en (x+0x18, y+0x18) con B = 0 y
      sonido 0x11 [C]. Como +21 = 24 > 16, [I] mientras se mueve dispara cada 16 frames y se queda con el sprite 0x19
      todo el tiempo.
    - En los demás frames, con probabilidad 1/2 decrementa +23. Al llegar a 0 (unos 2 × 176 ≈ 350 frames de
      movimiento), +23 = 0xB0 y lengua de 96 frames, parado [C].
- **Ataques / hijos:** 0x65 (b06: init A560, update A55F/74BA = RET), proyectil recto [C]. Su dirección sale de una
  rejilla 3×3: x del jugador a la izquierda del jefe / entre x y x+0x40 / a la derecha, por y del jugador por encima /
  entre y e y+0x40 / por debajo. vx/vy de las tablas b06:A5B3/A5C5 [C]: ±2 px por actualización en los ejes y ±1,41 en
  las diagonales (8 direcciones; la casilla central dispara hacia abajo) [C]. Sprite de b06:A5D7 [C]. Sonido 0x11 al
  disparar [C].
- **Debilidad:** BD0A = 0x11 → espada + tipo 2 (flecha de cerámica [I]) [C]. Además la prueba b03:BB9F solo deja
  golpear con E70A ≥ 0x19, es decir, con la boca abierta disparando (0x19) o con la lengua fuera (0x1A/0x1B) [C].
- **Contraste con la guía:** «flechas de cerámica a la lengua». Encaja con el arma [C+I]. El código también deja
  golpear con el sprite 0x19 (disparando), no solo con la lengua [C].
- **Dudas:** qué es exactamente E500 = 4 (b03:BA6E-BA81). Que 0x19 sea "boca abierta" y 0x1A/0x1B "lengua" sale solo
  de la secuencia, sin ver los sprites. Tras rebotar, fase 3 da un viaje de más de 0x40 a 0x38 antes de pararse en 0x40.

#### Jefe 8 — Monster (mundo 8; guía: ASCHER → Monster)
- **Vida:** 80 [C].
- **Inicio (b02:8A7A):** IX+2 = F0, x = 0x68, y = 0x20, sprite 0x28, +18 = 0x30, +19 = 4, +1A = 15, +1C = 0x80,
  +1E = 10, vx = vy = 0 [C]. Fase 0, sin hijos [C].
- **Comportamiento (b02:8AA4, tabla 8AB0: 8AC0, 8B14, 8B3A, 8B96, 8BAC, 8BBE, 8BE6, 8C00):**
  - **Cada frame, antes (8C8C):** +1C cuenta 128 frames. Después mira cada frame si vy (bytes alto + bajo) = 0. Si
    es así: guarda la fase en E71B y el sprite en E71F, se para (bit 4 = 0), +1D = 48, sprite 0x24, **fase 7** y
    +1C = 0 [C].
  - **Fase 7 (8C00), ataque, 48 frames:** con +1D = 0x20, sprite 0x27. Con +1D en 0x11-0x15 (5 frames), crea un
    **0x68** por frame en (x&F8 + E71E, y+4), con E71E alternando 10 ↔ 0x16 [C]. Con +1D < 0x10, sprite 0x24, y con
    +1D = 9, sonido 0x1B [C]. Al final recupera la fase y el sprite guardados, bit 4 = 1 y +1C = 0x80 [C].
  - Fase 0 (8AC0), quieto: 48 frames (+18), cambiando el sprite cada 15. Después: sprite 0x25, fase 1, guarda
    E717 = x del jugador y **E716 = (y del jugador & E0) − 8** (piso de destino) [C]. vx = ±0,5 px/f: con x ≥ 0x98 a
    la izquierda, con x < 0x38 a la derecha y si no hacia el lado de E717 respecto a 0x80 [C].
  - Fase 1 (8B14): anda (el sprite alterna cada 4 frames). En x = 0x38 o 0x98 (borde de plataforma) pasa a fase 2.
    Si ya está abajo (y = 0xA0) → salto (8B6C). Si no, se desplaza 8 en el sprite, vx = 0, sprite 0x2A y **cae a
    4 px/f** (vy = 0x0400) [C].
  - Fase 2 (8B3A): al llegar a y = 0x98 se para (vy = 0). Si el jugador (E717) está en su mitad de pantalla → salto.
    Si no, anda hacia el otro borde a 0,5 px/f (fase 3) [C].
  - Fase 3 (8B96): anda hasta x = 0x38/0x98 → salto [C].
  - Salto (8B6C): se para, sprite 0x24, vy = −2 px/f, 8 frames de espera (fase 4) [C]. Si ya está en el piso de
    destino (y − 8 = E716), va directo a la fase 6 [C].
  - Fase 4 (8BAC): 8 frames → sprite 0x2A, se mueve y **sube a 2 px/f** (fase 5) [C].
  - Fase 5 (8BBE): sube hasta y = E716 → vy = 0, sprite 0x25, +18 = 80, vx = ±0,5 hacia E717, fase 6 [C].
  - Fase 6 (8BE6): anda 80 frames → sprite 0x28, vx = 0, +18 = 48, fase 0 [C].
  - [I] Resumen: baja por los bordes, sube volando hasta el piso donde estaba el jugador, anda hacia él y cada
    ~128 frames, cuando no se mueve en vertical, se para y escupe 5 bolas.
- **Ataques / hijos:** 0x68 (b06: init 9F3F, update 9F6B) [C]. Al crearse toma una diagonal de b06:9F66 según un
  contador global E62F&3, rotando arriba-izquierda, arriba-derecha, abajo-derecha y abajo-izquierda, con 4 px por
  actualización por eje (solo bytes altos) [C]. Espera 8 actualizaciones quieto, se alinea a la rejilla, se mueve
  **rebotando en las paredes** (invierte vx/vy al chocar con tiles, 9FD8/A011) y desaparece a las 192 actualizaciones
  [C]. Sonido 0x1B en el ataque [C].
- **Debilidad:** BD0A = 0x05 → espada + tipo 3 (fuego rodante [I]) [C]. Caja de choque normal (BBAD), siempre
  vulnerable (IX+2 bit 5 no se borra) [C].
- **Contraste con la guía:** «subirse a una plataforma encima y usar fuego rodante». Encaja con el arma [C+I]. Lo de
  la plataforma tiene sentido [I] porque desde ahí no te alcanza en horizontal y el fuego rodante cae sobre él.
- **Dudas:** si E716 sale 0x98 (jugador abajo del todo), al subir de 2 en 2 desde 0x90 nunca coincide (8BBE); habría
  que ver qué valores de y puede tener el jugador. Los desplazamientos de ±8 de 8CBB/8CCC parecen un cambio de sprite
  (0x2A) [I]. El número exacto de bolas depende de que el hueco de enemigo esté libre (b04:6551).

#### Jefe 9 — Galious (mundo 10; guía: HAMALECH → Galious)
- **Vida:** 248 (124 con Holy Water), la mayor [C].
- **Inicio (b02:8CDD):** x = 0x60, y = 0x40, sprite 0x37, +17 = 0x30, +8 = 0xFA (250), +33 = +34 = 1, centro de las
  órbitas +1D = 0x74, +1E = 0x58 [C]. **Crea 8 hijos 0x67** en (0x60, 0x40) con ángulos de b02:8ED7
  `00 40 80 C0 20 60 A0 E0` (cada 45°) [C]. IX+2 |= E0 [C].
- **Hijos 0x67 (b06: init A5E0, update A5F3):** al crearse, radio +17 = 0x32 (50 px), ángulo +1A = E71A, centro
  (+14, +15) = (E71D, E71E) [C]. Su comportamiento lo manda **E71B (IX+1B de Galious)**, o el modo 3 si el hijo tiene
  +1F bit 0 (lanzado) [C]:
  - Modo 0 (A60B): órbita. El radio cambia en E71C/256 por actualización: E71C = 0x70 lo hace crecer (+0,44) hasta
    0x32, y E71C = 0x90 decrecer (−0,44) hasta 0x18 (24 px). Al llegar a 0x32 pone E71F = 0 [C]. Se ocultan fuera de
    y 8-0xB8 (A756) [C].
  - Modo 1 (A644): ocultos (IX+2 &= 0F) [C].
  - Modo 2 (A647): vuelven al nuevo centro (E71D/E71E) [C].
  - Modo 4 (A6D1): lanzamiento. Galious lanza 4 hijos seguidos (huecos E800-E860 o E880-E8E0, según +24/+25 = 0x00
    o 0x80, al azar). A cada uno le da una dirección de 16 posibles, empezando por +20 = rnd&15 y restando 4 (90°)
    cada vez, con la velocidad de b04:7E25 (B = 8) [C].
  - Modo 3 (A65A), lanzado: espera a que su ángulo de órbita coincida con el guardado (E72A+i), sale en línea recta y
    tras 9 pasos invierte vx y vy. Al paso 17 vuelve a la órbita (+1F = 0) [C]. [I] Es un bumerán: sale y vuelve.
- **Comportamiento (b02:8D2D, tabla 8D33: 8D3D, 8D65, 8D8B, 8DC1, 8DF7):**
  - Fase 0 (8D3D): E71B = 0 (órbita) [C].
    - Si **no queda ningún hijo** (8E6D mira los 8 huecos E800-E8E0): sprite 0x37 y solo cuenta +8 [C].
    - Si quedan, cada frame (8E97) anima +33. Si no hay un ataque en curso (+23 = 0), mira si el jugador está a menos
      de 0x1E en x del centro (+1D) y a menos de 0x1E en y (jy−16 contra +1E) [C]. Si es así, +1C = 0x90 (las bolas
      se cierran) y +1F = 1. Si no, +1C = 0x70 (se abren) [C].
    - Con +1F = 0 y sin ataque, cuenta +17 (48 frames) → **fase 4** (ataque) [C]. Mientras haya ataque en curso, espera
      a que vuelvan los 4 lanzados (8E3E) y entonces +23 = 0 [C]. En los demás casos cuenta +8 (250 frames) → **fase 1**
      (teletransporte) [C].
  - Fase 4 (8DF7): +17 = 48, elige la mitad de hijos y la dirección al azar, +22 = 4, **E71B = 4** (lanzar),
    sprite 0x38 durante 32 frames (+34), +23 = 1 (ataque en curso), limpia E726-E731 y vuelve a fase 0 [C].
  - Fase 1 (8D65): espera a que no haya bolas lanzadas fuera. Luego +8 = 63, E71B = 1 (hijos ocultos) y
    **IX+2 &= 0F (invisible e invulnerable**: se borra el bit 5) → fase 2 [C].
  - Fase 2 (8D8B): tras 63 frames aparece en una de 8 posiciones al azar (b02:8EDF, (x,y)): (60,20) (80,20) (60,40)
    (70,40) (A0,40) (30,60) (40,60) (B0,60). Centro = (x+0x14, y+0x18), E71B = 2, fase 3 [C].
  - Fase 3 (8DC1): **parpadeo**: cada 3 frames alterna visible y vulnerable (IX+2 |= A0) con oculto (&= 0F). Lo hace
    24 veces (72 frames) y luego +17 = 48, +8 = 250, fase 0, IX+2 |= E0 [C].
- **La cruz (E07A = flag del objeto 0x0B):** la prueba de choque de Galious es **b03:BBA8**: `LD A,(E07A) ; OR A ;
  RET Z`. Sin la cruz devuelve Z/NC, así que ni el proyectil ni la espada (b03:BD61 usa la misma 5D) cuentan como
  golpe: **no suena 0x1D ni resta vida**. Con la cruz sigue a la caja normal BBAD [C]. Es el único sitio de toda la ROM
  que lee E07A (búsqueda de `7A E0`) [C]. → La cruz es **imprescindible para dañarlo**, no para que aparezca [C].
- **Debilidad:** BD0A = 0x3F → espada y las 5 armas (todos los bits 0-5) [C]. Solo es vulnerable con IX+2 bit 5, que
  se borra mientras se teletransporta y parpadea [C].
- **Contraste con la guía:** «anular las bolas con flechas y luego fuego rodante; hace falta la cruz». Lo de la cruz
  encaja [C]. Las bolas son los 8 hijos 0x67, y si se destruyen todos Galious deja de atacar (fase 0 solo cuenta para
  teletransportarse) [C]. [I] Pero el código le deja golpear con cualquier arma, no solo con el fuego rodante.
  [I] Con las bolas cerradas a 24 px cuando te acercas, protegen el centro, y eso explica que haya que eliminarlas antes.
- **Dudas:** la resistencia y el daño de los hijos 0x67 (lo decide el sistema de enemigos, no lo he mirado). El cálculo
  de posición de la órbita (b06:A92F) y la velocidad exacta de b04:7E25 con B = 8. Si el modo 4 (E71B = 4) dura solo
  un frame antes de que la fase 0 lo ponga a 0, ¿se pierde el lanzamiento cuando los hijos se actualizan en el frame
  impar? Los flags de 8E3E al volver (8D5B `RET NZ`) dependen del último `ADD` de 4136 y no los he resuelto. El estado
  de E718 al empezar (índice de 8ED7) se da por 0.

#### Jefe 10 — Donra (mundo 5; guía: HEOTYMEO → Donra)
- **Vida:** 72 [C].
- **Inicio (b02:84F0):** IX+9 bit 6 = 1, +0B = 0, IX+2 = F0, x = 0x70, y = 0x60, sprite del cuerpo E70A = 0x1C,
  +18 = 0x23, cabeza E719 = 0x1E, posición de la cabeza E71C = 1, +1A = 3, +1D = 8, +1B = 1, +1E = 4, E71F = y;
  **vy = +4, vx = −4 px/f** [C].
- **Comportamiento (b02:8535):**
  - Cada frame (85BB): el sprite E70A cambia el bit 0 cada 4 frames si sube (vy < 0) y cada 10 si no [C].
    [I] Bate las alas más rápido al subir.
  - Fase 0:
    - Límites verticales (8586): si y + vy ≥ 0x78 → vy = −1. Si < 0x40 → vy = +2 (bytes altos; B0ED solo suma el
      alto para el tipo 0x0A) [C]. → Baja a 2 px/f y sube a 1 px/f entre y = 0x40 y 0x78 [C].
    - Límites horizontales (85A4): si x + vx ≥ 0x88 → vx = −1 px/f. Si < 0x40 → +1 px/f [C]. El −4 inicial solo dura
      hasta llegar a x < 0x40 [C].
    - **Cabeza** (cada 8 frames, +1A): compara y + 8·E71C con la y del jugador y mueve la cabeza un paso hacia él.
      E719 va de 0x1E a 0x21 y E71C = E719 − 0x1D (1-4) [C]. [I] La cabeza sube y baja a lo largo del cuerpo siguiendo
      al jugador.
    - Cuando +1E llega a 0 (4 frames la primera vez, luego 8-39 al azar) → fase 1: se para (bit 4 = 0), +18 = 0x22
      ([I] sprite de boca abierta), +1E = 16 [C].
  - Fase 1 (85E1): a los 16 frames crea un **0x57** en (x − 0x10, y + {08,10,18,20}[E71C−1]), a la altura de la
    cabeza, con B = 2. +1E = 32 y fase 2 [C].
  - Fase 2 (860C): a los 32 frames se mueve otra vez, +1E = 8 + (rnd&0x1F), +18 = 0x23 y fase 0 [C].
- **Ataques / hijos:** 0x57 (b06: init AD51, update AD8C): sonido 0x1C al crearse. Con parámetro ≠ 0, vive 8
  actualizaciones y en cada una avanza 8 px **hacia la izquierda** (x −= 8) y cambia de sprite [C]. [I] Es una llamarada
  horizontal de ~64 px que sale de la boca, siempre hacia la izquierda (no apunta al jugador en x) [C].
- **Debilidad:** BD0A = 0x21 → espada + tipo 1 (flecha [I]) [C]. Prueba de choque b03:BC02: pone un momento
  E70A = E719 (sprite de la cabeza) y usa su caja [C]. → **solo se le golpea en la cabeza** [C].
- **Contraste con la guía:** «disparar flechas a la cabeza, cuidado con su fuego». Encaja del todo [C+I].
- **Dudas:** la semántica de E624 en 0x57 (supongo que es el parámetro B) [I]. IX+9 bit 6 y +18 (0x22/0x23) parecen
  atributos o el segundo sprite [I]. Que el fuego vaya siempre a la izquierda supone que Donra está en el lado derecho
  de la sala.

#### Jefe 11 — Jump Slime (mundo 4; guía: BARECHET → Jump Slime)
- **Qué es:** no es una segunda forma de Galious. E700 = 11 lo pone b00:4985, que copia 5E05[E041] a E0D3, y luego
  b01:7F56 lo pasa a E700. Con E041 = 5 (mundo 4), 5E05[5] = 0x0B [C]. Es el jefe del mundo 4 [C].
- **Vida:** 112 [C].
- **Inicio (b02:87E6):** IX+2 |= F0, x = 0x80, y = 0x70 y **suelo E71B = 0x70**, sprite 0x2B, +18 = 8, +24 = 0x40
  (ancho), vx = ±0,5 px/f al azar (bit 0 de R) [C]. Fase 0, sin hijos [C].
- **Comportamiento (b02:8815, tabla 881B: 8825, 885F, 8883, 88BA, 890E):**
  - Fase 0 (8825), se arrastra:
    - El sprite alterna 0x2B y 0x2C cada 8 frames. Rebota en x < 0x18 o x + 0x40 ≥ 0xE0 [C].
    - Si va a la derecha y la x del jugador ≤ x + 0x70, o va a la izquierda y la x del jugador > x − 0x20, entonces con
      probabilidad 3/4 por frame: sprite 0x2D (derecha) o 0x2E (izquierda), copiado en E71D, +18 = 8 y fase 1 [C].
      [I] En la práctica salta enseguida en cuanto el jugador está cerca o detrás.
  - Fase 1 (885F): se para 8 frames, sprite 0x2F (derecha) o 0x30 (izquierda), copiado en E71C, y se mueve otra vez
    a ±0,5 (fase 2, 8 frames) [C]. [I] Se agacha para coger impulso.
  - Fase 2 (8883), **salto**: vx = ±1,5 px/f (0x180) y ay = +0,25 px/f² (0x40). vy = −4,5 px/f con probabilidad 3/4
    o **−6 px/f con 1/4** (rnd&3 = 1). +19 = 0, fase 3 [C].
  - Fase 3 (88BA), en el aire:
    - vy += ay. En los bordes vx = 0 (no rebota) [C].
    - Los primeros 32 frames, sprite 0x2B y no comprueba el aterrizaje. Después, sprite E71C ^ 0x1F [C].
    - Al pasar y > E71B (0x70): y = 0x70, vx = vy = 0, sprite E71D ^ 3, +8 = 32, **sonido 0x1A** y fase 4 [C].
    - [I] Por la física, el salto normal sube ~40 px y dura 36 frames (~54 px en horizontal). El alto sube ~72 px y
      dura 48 frames (~72 px en horizontal).
  - Fase 4 (890E): 32 frames quieto en el suelo. Luego, cada 4 aterrizajes (+1E) la dirección es al azar; si no, va
    hacia el jugador (comparando con x, el borde izquierdo de un sprite de 64 px). vx = ±0,5 px/f y fase 0 [C].
- **Ataques / hijos:** ninguno. No crea proyectiles; el único sonido es 0x1A al aterrizar [C]. [I] Hace daño por
  contacto.
- **Debilidad:** BD0A = 0x03 → espada + tipo 5 (mina [I]) [C]. Caja normal BBAD, siempre vulnerable [C]. Con la mina
  explotando usa la prueba alternativa b03:BC60 (BB04 + BA4C) [C].
- **Contraste con la guía:** «poner 3 minas cada vez que salta». Encaja: solo le hacen daño la mina y la espada, la
  mina resta 2 y siempre aterriza en y = 0x70, en el suelo donde están las minas [C+I].
- **Dudas:** los sprites 0x2B-0x30 los he interpretado solo por la secuencia. El cambio E71C ^ 0x1F en la caída
  intercambia 0x2F y 0x30 (¿mira al revés al bajar?). La condición asimétrica de la fase 0 (+0x70 a la derecha, −0x20 a
  la izquierda) quizá sea respecto al borde izquierdo del sprite.

---

### Tabla resumen

| Jefe (E700) | Mundo | Nombre | Vida | Patrón | Hijos | Debilidad (BD0A + prueba de choque) |
|---|---|---|---|---|---|---|
| 7 | 7 | Big Lip | 64 | Ida y vuelta horizontal desde x = 0x68 y vertical entre y = 0x40 y 0x70 a 1 px/f; lengua 96 f parado (cada ~350 f de movimiento o al golpearte, E500 = 4) | 0x65 cada 16 f mientras se mueve, apuntado en 8 direcciones a 2 px; sonido 0x11 | 0x11: espada + flecha de cerámica [I]; solo con sprite ≥ 0x19 (boca abierta o lengua), b03:BB9F |
| 8 | 8 | Monster | 80 | Anda a 0,5, cae a 4 px/f por los bordes, sube a 2 px/f al piso del jugador; cada ~128 f, parado 48 f | 5 × 0x68: bolas diagonales que rebotan en las paredes, 192 act.; sonido 0x1B | 0x05: espada + fuego rodante [I]; siempre vulnerable |
| 9 | 10 | Galious | 248 | Fijo; teletransporte cada ~250 f a 8 posiciones (63 f oculto + 72 f parpadeando, invulnerable); ataque cada 48 f | 8 × 0x67 en órbita (radio 24-50; se cierran si te acercas); lanza 4 en bumerán a 90° | 0x3F: todas las armas, **pero b03:BBA8 exige la cruz (E07A ≠ 0)**; sin ella los golpes no hacen nada ni suenan |
| 10 | 5 | Donra | 72 | Vuela en x 0x40-0x88 a 1 px/f y en y 0x40-0x78 (baja a 2, sube a 1); la cabeza sigue la y del jugador | 0x57: llamarada a la izquierda desde la cabeza (8 × 8 px) cada 8-39 f + 48 f; sonido 0x1C | 0x21: espada + flecha [I]; **solo en la cabeza** (b03:BC02 usa el sprite E719) |
| 11 | 4 | Jump Slime | 112 | Se arrastra a 0,5; salta enseguida si estás cerca: vx 1,5, vy −4,5 (3/4) o −6 (1/4), g = 0,25; 32 f en el suelo | Ninguno; sonido 0x1A al aterrizar | 0x03: espada + mina [I] (la mina resta 2); siempre vulnerable |
