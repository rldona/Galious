"""Generadores periodicos de enemigos por sala (b04:6186-62DA al cargar, b04:62DB-647B en cada tick).

Datos transcritos a mano del codigo (docs/entities.md, "Generadores periodicos por sala"; fichas en
docs/enemy_ai.md, "IA de enemigos - generados periodicamente"). Todo con E045 = 0, el valor real.
Los temporizadores cuentan **ticks** (una pasada del bucle principal = 2 frames), no frames.

periodic_for(e041, room) devuelve la lista que unity_bundle.py guarda en rooms[].periodic. Campos:
  gen           RAM del generador en el original (solo referencia): "E650", "E653"...
  type          tipo de enemigo creado (b04:65DA)
  firstDelay    ticks desde que se entra en la sala hasta el primer intento de creacion (tick 1 = primera
                pasada del bucle en la sala). Si periodRandom no esta vacio, el primero tambien es aleatorio
  period        ticks entre dos intentos seguidos (dentro de una tanda, si batch > 0)
  periodRandom  [] o [min, max]: cada espera es un entero al azar en ese rango, ambos incluidos (E653)
  batch         0 = sin tandas; n = intentos por tanda, separados por `period`
  batchGap      ticks entre el ultimo intento de una tanda y el primero de la siguiente
  maxTotal      0 = sin fin; n = el generador se apaga tras n intentos (E668)
  x, y          posicion efectiva de nacimiento. x = -1 si la decide la rutina de inicio del tipo
  xSequence     [] o x de cada intento, en orden (ciclica por tanda si batch > 0; uno por intento si maxTotal)
  randomX       [] o rangos de x al azar en pares planos [a0, b0, a1, b1, ...], ambos incluidos
  xRule         "fixed" | "sequence" | "random" | "leap" (0x0D: junto al jugador o al azar, ver ficha)
  xClamp        [] o [min, max] de x (solo "leap": limites de la sala)
  param         valor que el generador pasa en B (E624). 0x0D: 1..4 segun la sala
  pauseInLiquid true: el temporizador no avanza mientras el jugador esta en agua o lava (E511 != 0)
  disabledByItem 0 o id de objeto: si se tiene al entrar en la sala, el generador no se arma (0x18 Carpet)
Comun a todos: un intento falla sin reintentar si ya hay E635 enemigos (16) o no caben sus sprites (E636);
el temporizador sigue igual. Todos se paran mientras el libro congela (E0D5 != 0).
"""

CASTLE = 1          # E041: 1 = castillo, n + 1 = mundo n

BATCH_X = [0x60, 0xC0, 0x40]       # E651 = 3 -> 2, 1, 0 -> tabla 6336/6426 [E651 & 3] = {40, C0, 60, A0}
RANDOM_X_21 = [16, 47, 80, 111, 144, 175, 208, 239]   # ((r - 16) & 0xDF) + 16
LEAP_CLAMP = {137: [0x20, 0xE0], 138: [0x00, 0xE0], 139: [0x00, 0xE0], 140: [0x00, 0xD0]}   # tabla 791F
E668_COUNT = {2: 5, 3: 10, 4: 5, 7: 10, 8: 10, 17: 10, 18: 5}                             # b04:61A8


def _gen(gen, typ, **kw):
    g = {'gen': gen, 'type': typ, 'firstDelay': 145, 'period': 145, 'periodRandom': [], 'batch': 0,
         'batchGap': 0, 'maxTotal': 0, 'x': -1, 'y': 0, 'xSequence': [], 'randomX': [], 'xRule': 'random',
         'xClamp': [], 'param': 0, 'pauseInLiquid': False, 'disabledByItem': 0}
    g.update(kw)
    return g


def periodic_for(e041, room):
    out = []
    # E650 (b04:61E4 / 62F9): tipo 0x09 desde arriba, tandas de 3 (113, +32, +32; luego 113 hasta la siguiente)
    if e041 == CASTLE and room in (58, 71, 113, 114):
        out.append(_gen('E650', 0x09, firstDelay=113, period=32, batch=3, batchGap=113, y=0x00,
                        xSequence=BATCH_X, xRule='sequence'))
    # E653 (b04:6200 / 634A): tipo 0x08 desde la lava; espera 1 + (48..111); el Carpet lo desactiva
    if (e041 == CASTLE and 144 <= room <= 147 or e041 == 9 and 15 <= room <= 19 or
            e041 == 10 and 3 <= room <= 5 or e041 == 11 and 2 <= room <= 3):
        out.append(_gen('E653', 0x08, firstDelay=0, period=0, periodRandom=[49, 112],
                        y=0xB0 if e041 == 9 else 0xA0, randomX=[64, 191], disabledByItem=0x18))
    # E659 (b04:6239 / 6374): tipo 0x0D, pez que salta del agua; parado con el jugador en agua/lava
    if e041 == CASTLE and 137 <= room <= 140:
        out.append(_gen('E659', 0x0D, y=0x90, xRule='leap', xClamp=LEAP_CLAMP[room], param=room - 136,
                        pauseInLiquid=True))
    # E65C (b04:6249 / 639D): tipo 0x21 que cae desde arriba y camina
    if (e041 == CASTLE and room in (10, 17, 28, 56, 73, 80, 122, 131, 132) or e041 == 7 and room == 8 or
            e041 == 5 and room in (10, 12)):
        out.append(_gen('E65C', 0x21, y=0x00, randomX=RANDOM_X_21))
    # E65F (b04:6291 / 63C3): tipo 0x30 que salta de la lava a una plataforma
    if e041 == 6 and 7 <= room <= 10:
        out.append(_gen('E65F', 0x30, y=0x90, randomX=[32, 95, 160, 223]))
    # E662 (b04:62A4 / 63E9): tipo 0x38 desde abajo, tandas de 3 como E650
    if e041 == CASTLE and room in (94, 95, 102):
        out.append(_gen('E662', 0x38, firstDelay=113, period=32, batch=3, batchGap=113, y=0xB0,
                        xSequence=BATCH_X, xRule='sequence'))
    # E665 (b04:62B9 / 643A): tipo 0x35, bola que cae desde arriba y rueda
    if e041 == CASTLE and room in (47, 59) or e041 == 9 and room in (2, 14):
        out.append(_gen('E665', 0x35, y=0x00, randomX=RANDOM_X_21))
    # E668 (b04:61A8 / 6460): tipo 0x3E, 5 o 10 en total cada 16 ticks; x del inicio (b06:A283)
    if e041 == 7 and room in E668_COUNT:
        n = E668_COUNT[room]
        out.append(_gen('E668', 0x3E, firstDelay=16, period=16, maxTotal=n, x=-1, y=0x28,
                        xSequence=[0xB0 if n - 1 - i >= 5 else 0x40 for i in range(n)], xRule='sequence'))
    for g in out:
        if g['xRule'] in ('fixed', 'sequence') and len(set(g['xSequence'])) == 1:
            g['x'] = g['xSequence'][0]
    return out
