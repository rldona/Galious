#!/usr/bin/env python3
"""Bucle de atraccion para el importador de Unity: extracted/unity/attract.json (docs/attract.md).

  attract.py   -> extracted/unity/attract.json   (lo llama unity_bundle.py)

Estados 0 (logo de Konami), 1 (titulo), 2 (demo jugada o historia) y 3 (inicio de partida). Constantes leidas de la
ROM con su direccion; los tiempos medidos en openMSX (tools/openmsx/attract_measure.py) van marcados (measured).

Fuentes [C] (grupo 0 = b01-b03, grupo 4 = b13-b15):
  b00:4277       game_frame: con E000 < 3 apila attract_check_start (b00:4BFA) como vuelta
  b00:4BFA       teclas nuevas (b00:4C75: flechas, espacio, M o joystick; E012 = flanco). Estado 1: espacio/M (0x30) ->
                 E002 = 0x40, estado 3. Estados 0 y 2: cualquiera -> estado 1 (titulo desde el principio, musica 0);
                 desde la demo cuenta como terminada (E025++, y si era demo jugada, E023 ^= 1)
  b00:42B8       estado 0: sub 0 init (VDP, b01:6957 logo de 21x6 tiles en fila 8 col 7, patrones RLE b09:BCEF);
                 sub 1 cada 2 frames b01:6985 colorea una linea de pixeles (blanco) de las 6 filas: 48 lineas;
                 sub 2 espera 256 frames (E004 = 0) y b00:42F1 monta el titulo -> estado 1
  b00:430C       estado 1: L (b02:98E5, SNSMAT fila 4 bit 1) -> estado 18; b01:6215 init, b01:6251 por frame:
                 F02B = 48 frames; destello (b14:9407) por la espada +2 px/frame de x 0x60 a 0x98; estrella (b14:93F7 /
                 93FF) 5 pasos de 16 frames (E003 & 15 = 0); 120 frames (F02C) -> E019 = 0 -> estado 2
  b01:7D51       textos del titulo (direccion VRAM, bytes, FE / FF); b01:7D71 texto del estado 3
  b00:433C       estado 2 (tabla 4340): sub 0 barrido (b00:4D1E, 32 columnas) y, segun E025 bit 0, demo (b01:7D82) o
                 historia (b01:6A1E + 6AC5, musica 0x86); sub 1 demo (b01:7E22 entrada, 7DE8, 7EDD F5 = musica on/off);
                 sub 2 historia (b01:6ADC); sub 3 espera a que calle la musica. Al acabar: E025++, E023 ^= 1 (demo) -> 0
  b01:7DD9       E041/E042 de cada demo (palabra por E023); b01:7DE3 E033 (musica: se para tras E033 pasos de 32 ticks
                 de E604); b15:BAFD punteros a las entradas de cada demo: pares (ticks, teclas), teclas FF = fin
  b00:4CB0       inicio de partida (tambien de la demo): borra E016-E0FE y E200-F03E, E041 = E042 = 1, E501 = 1,
                 E051-E05B desde b00:4CE6, b02:99B3 (E06E = R & 3, E06D = b02:99C4[E06E])
  b00:528E       posicion inicial en la sala: b09:B106[E041] -> byte[E042]: y = (b & F0) + 8, x = (b & 0F) * 16
  b00:43F0       estado 3: sub 0 musica 0x87, E004 = 45; sub 1 parpadeo (visible si E004 & 4 = 0); sub 2 b00:4CB0
  b15:AE28       texto de la historia (misma rutina y fuente 8x16 que el epilogo, ver ending.py)

Unidades: frames de video a 60 Hz. Los estados 0, 1, 3 y la historia corren una vez por frame. La demo corre en el motor
del juego: sus entradas cuentan TICKS de juego (un paso de la logica, ~2 frames; medido 2,2 frames por tick).
Texto: col/row en tiles de 8 px de la pantalla de 32x24 (sin el marcador: el titulo no lo tiene).
Posiciones del jugador: E507 (x) / E505 (y), las mismas unidades que player.md.

Campos de attract.json (JsonUtility: solo objetos y arrays):
  cycle            orden de las pantallas y reglas de teclas (keys[])
  screens[]        {id, state, substates[], durationFrames, measured, music, texts[] {text, x, y, blink}, notes}
                   x, y = columna y fila (tiles); blink = parpadea
  title            logo, sprites del destello y tiempos de las fases del titulo
  konamiLogo       logo del estado 0 (posicion, revelado por lineas)
  storyLines       como ending.json epilogue.lines {kind: text | blank | wait | musicStop | end, col, text, rows, steps}
  story            ritmo y tiempos de la historia
  demo             la primera demo (= demos[0])
  demos[]          {index, map, e041, room, x, y, spawnByte, character, inputs[] {frames, keys, left, right, up, down,
                   space, m}, durationTicks, durationFramesMeasured, musicStopTicks (E033), roomsVisitedMeasured[],
                   startState {...}}; inputs[].frames = ticks de juego que se mantiene esa combinacion
  gameStart        estado 3 y estado inicial de la partida (sala, posicion, personaje, vitalidad, vidas, contadores)
"""
import json
from galious_rom import Rom, ROOT
import ending

G0, G4 = 0, 4
TITLE_TEXT, START_TEXT = 0x7D51, 0x7D71           # b01
DEMO_ROOMS, DEMO_MUSIC_STEPS, DEMO_INPUTS = 0x7DD9, 0x7DE3, 0xBAFD   # b01, b01, b15
STORY_PTR = 0x6AC6                                # b01: LD HL,AE28
INIT_TABLE = 0x4CE6                               # b00: E051..E05B al empezar
SPAWN_TABLE = 0xB106                              # b09 (grupo 2, A000)
KEY_BITS = ['up', 'down', 'left', 'right', 'space', 'm']   # bits 0-5 de E008/E009 (b00:4C75)


def title_char(b):
    """Fuente pequena del titulo: 00 espacio, 10-19 digitos, 1A (c), 21-3A A-Z."""
    if b == 0:
        return ' '
    if 0x10 <= b <= 0x19:
        return str(b - 0x10)
    if 0x21 <= b <= 0x3A:
        return chr(ord('A') + b - 0x21)
    return {0x1A: '©'}.get(b, f'{{{b:02X}}}')


def vram_texts(rom, a, blink=False):
    """Registros (direccion VRAM de la tabla de nombres 3800, bytes..., FE siguiente / FF fin), b00:5C25."""
    out = []
    while True:
        addr = rom.word(a, G0)
        a += 2
        s = ''
        while (b := rom.byte(a, G0)) not in (0xFE, 0xFF):
            s += title_char(b)
            a += 1
        a += 1
        off = addr - 0x3800
        lead = len(s) - len(s.lstrip(' '))
        out.append({'text': s.strip(' '), 'x': off % 32 + lead, 'y': off // 32, 'blink': blink})
        if b == 0xFF:
            return out


def demo_inputs(rom, k):
    p = rom.word(DEMO_INPUTS + 2 * k, G4)
    out = []
    while True:
        n, keys = rom.byte(p, G4), rom.byte(p + 1, G4)
        p += 2
        e = {'frames': n or 256, 'keys': keys}
        e.update({name: bool(keys != 0xFF and keys >> i & 1) for i, name in enumerate(KEY_BITS)})
        out.append(e)
        if keys == 0xFF:
            return out


def spawn(rom, e041, room):
    b = rom.byte(rom.word(SPAWN_TABLE + 2 * e041, 2) + room, 2)
    return b, ((b & 0x0F) << 4, (b & 0xF0) + 8)


def init_state(rom):
    v = rom.block(INIT_TABLE, 11)      # E051..E05B
    bcd = lambda x: (x >> 4) * 10 + (x & 15)
    return {
        'character': 'Popolon', 'e501': 1,
        'aphrodite': {'vitality': v[1], 'vitalityMax': v[2], 'experience': 0, 'experienceMax': v[0],
                      'lives': bcd(v[7]), 'resurrections': v[9]},
        'popolon': {'vitality': v[5], 'vitalityMax': v[6], 'experience': 0, 'experienceMax': v[4],
                    'lives': bcd(v[8]), 'resurrections': v[10]},
        'arrows': 0, 'coins': 0, 'keys': 0, 'subWeapon': 0, 'items': [],
        'doorsOpen': False,
        'world10Door': {'e06eRoll': 'R & 3 (registro R del Z80: al azar)',
                        'roomByRoll': list(rom.block(0x99C4, 4, G0))},
        'notes': 'b00:4CB0 borra E016-E0FE y E200-F03E (objetos, puertas, contadores, flags) y copia E051-E05B de '
                 'b00:4CE6. Vidas de reserva BCD: E058 = 1 (Aphrodite), E059 = 0 (Popolon): la partida tiene 2 vidas '
                 'en total (la que se juega + 1). Resurrecciones = credito de la Muerte (E05A/E05B). Con otro '
                 'cartucho de Konami en el segundo slot (F0F8, b00:4CF1) hay extras: no se reproduce.',
    }


def build(rom):
    title_texts = vram_texts(rom, TITLE_TEXT)
    start_texts = vram_texts(rom, START_TEXT, blink=True)
    story_lines = ending.epilogue(rom, rom.word(STORY_PTR, G0))
    sprites = lambda a, n: [{'y': rom.byte(a + 4 * i, G4), 'x': rom.byte(a + 4 * i + 1, G4),
                             'pattern': rom.byte(a + 4 * i + 2, G4), 'color': rom.byte(a + 4 * i + 3, G4)}
                            for i in range(n)]
    st = init_state(rom)
    demos = []
    measured = {0: {'frames': 2440, 'ticks': 1108, 'musicStopTick': 1026,
                    'rooms': [146, 135, 126, 113, 101, 100, 99, 112, 111, 110, 109]},
                1: {'frames': 2743, 'ticks': 1293, 'musicStopTick': 1140,
                    'rooms': [1, 107, 120, 121, 132, 133, 143, 133]}}
    for k in range(2):
        e041, room = rom.byte(DEMO_ROOMS + 2 * k, G0), rom.byte(DEMO_ROOMS + 2 * k + 1, G0)
        b, (x, y) = spawn(rom, e041, room)
        inputs = demo_inputs(rom, k)
        ticks = sum(e['frames'] for e in inputs[:-1]) + 1
        m = measured[k]
        start = json.loads(json.dumps(st))
        start.update({'arrows': 100, 'subWeapon': 1, 'items': [1],
                      'notes': 'como una partida nueva (b00:4CB0), pero con el Arrow (E070 = 1), elegido (E510 = 1) y '
                               '100 flechas (E046 = 0x0100 BCD), b01:7DA1. Al acabar la demo se borran los tres.'})
        demos.append({
            'index': k, 'map': 'castle', 'e041': e041, 'room': room, 'x': x, 'y': y, 'spawnByte': b,
            'character': 'Popolon', 'startState': start,
            'music': 0x88, 'musicStopSteps': rom.byte(DEMO_MUSIC_STEPS + k, G0),
            'musicStopTickMeasured': m['musicStopTick'] if m['musicStopTick'] is not None else -1,
            'inputs': inputs, 'inputCount': len(inputs),
            'durationTicks': ticks, 'durationFrames': m['frames'], 'durationFramesMeasured': m['frames'],
            'roomsVisitedMeasured': m['rooms'],
            'notes': 'Demo jugada con el motor real: cada tick b01:7E22 mete la combinacion de teclas como si viniera '
                     'del teclado (E009 mantenidas, E008 flancos). Una entrada dura "frames" ticks (contando el tick en '
                     'que se carga); la ultima (teclas FF) acaba la demo en el tick en que se lee. Marcador normal '
                     '(CASTLE), sin texto de DEMO. Enemigos, IA y azar son los del juego: el azar (R del Z80 xor E003) '
                     'no se siembra, asi que en otra maquina la demo podria torcerse.',
        })

    data = {
        'source': 'ROM + openMSX (docs/attract.md)',
        'frameRate': 60,
        'cycle': {
            'order': ['konamiLogo', 'title', 'demo0', 'konamiLogo', 'title', 'story', 'konamiLogo', 'title', 'demo1',
                      'konamiLogo', 'title', 'story'],
            'notes': 'E025 cuenta las vueltas del estado 2: par = demo jugada (E023 elige la 0 o la 1 y cambia al '
                     'acabar), impar = historia. Al encender E023 = E025 = 0. Se repite cada 4 vueltas.',
            'keys': [
                {'state': 0, 'keys': 'flechas, espacio, M (o joystick); tecla nueva', 'goTo': 1,
                 'notes': 'titulo desde el principio (sin pasar por el barrido); no cuenta vuelta'},
                {'state': 1, 'keys': 'espacio o M (boton A o B)', 'goTo': 3,
                 'notes': 'E002 = 0x40 (partida en curso). Las flechas no hacen nada'},
                {'state': 1, 'keys': 'L mantenida (SNSMAT fila 4 bit 1, cada frame del titulo)', 'goTo': 18,
                 'notes': 'pantalla de contraseña; se mira antes que el espacio'},
                {'state': 2, 'keys': 'flechas, espacio, M; tecla nueva', 'goTo': 1,
                 'notes': 'vuelve al titulo (musica 0) y la vuelta cuenta como hecha: E025++ y, si era demo, E023 ^= 1'},
                {'state': 2, 'keys': 'F5 (fila 7 bit 1), por flanco, solo en la demo jugada', 'goTo': 2,
                 'notes': 'E00D ^= 1: apaga la musica de la demo (peticion 0) o la vuelve a pedir si E033 > 0. Los '
                          'efectos suenan siempre. Tambien vale para la demo siguiente (E00D no se borra)'},
            ],
        },
        'konamiLogo': {
            'col': 7, 'row': 8, 'wTiles': 21, 'hTiles': 6, 'patterns': 'RLE b09:BCEF', 'color': 15,
            'backdropColor': 4, 'revealLines': 48, 'framesPerLine': 2,
            'notes': 'logo de Konami en blanco sobre fondo azul oscuro (VDP R7 = 0xE4). Se colorea de arriba abajo, una '
                     'linea de pixeles cada 2 frames (las 21 columnas a la vez): 6 filas x 8 lineas = 96 frames. '
                     'Despues queda quieto 256 frames. Sin musica (peticion 0 = silencio total).',
        },
        'title': {
            'logo': {'tiles': 'b14:933F', 'col': 9, 'row': 1, 'wTiles': 14, 'hTiles': 12,
                     'notes': 'escudo con THE MAZE OF GALIOUS y KNIGHTMARE II (dibujo de tiles); fondo negro (R7 = 0xE0)'},
            'logoSprites': sprites(0x93E7, 4),
            'glint': {'sprite': sprites(0x9407, 1)[0], 'xFrom': 0x60, 'xTo': 0x98, 'pxPerFrame': 2,
                      'patternStepAtX': [0x6E, 0x7C, 0x8A], 'patternStep': 4,
                      'notes': 'destello que recorre la espada: aparece a los 48 frames, avanza 2 px por frame '
                               '(28 frames) y cambia de patron (+4) en x 0x6E, 0x7C y 0x8A; al llegar a 0x98 se oculta'},
            'star': {'frames': [sprites(0x93F7, 2), sprites(0x93FF, 2), sprites(0x93FF, 2), sprites(0x93F7, 2)],
                     'framesEach': 16,
                     'notes': 'estrella en la punta (x 0xA0, y 0x3F): 4 dibujos de 16 frames, empezando cuando '
                              'E003 & 15 = 0 (0-15 frames de espera); el 5.o paso la oculta. y = 0xE0 = oculto'},
            'phasesFrames': [{'id': 'still', 'frames': 48}, {'id': 'glint', 'frames': 28},
                             {'id': 'star', 'frames': 67, 'notes': '64 + alineacion a E003 & 15 (0-15)'},
                             {'id': 'wait', 'frames': 120}],
        },
        'screens': [
            {'id': 'konamiLogo', 'state': 0, 'substates': [0, 1, 2], 'durationFrames': 366, 'measured': True,
             'music': 0, 'texts': [],
             'notes': 'sub 0: 1 frame (init, silencio); sub 1: 96 frames (revelado); sub 2: 256 frames quieto + ~10 de '
                      'carga del titulo. 353 en codigo, 366 medido'},
            {'id': 'title', 'state': 1, 'substates': [0], 'durationFrames': 264, 'measured': True, 'music': 0,
             'texts': title_texts,
             'notes': 'sin musica; nada parpadea. 48 quieto + 28 destello + 64-79 estrella + 120 quieto: 264-279 '
                      'frames (medidos 264, 268, 272)'},
            {'id': 'wipe', 'state': 2, 'substates': [0], 'durationFrames': 55, 'measured': True, 'music': 0,
             'texts': [],
             'notes': 'barrido: cada frame borra una columna (filas 4-23) de izquierda a derecha, 32 frames; luego '
                      'carga la sala de la demo o la historia. 55 frames antes de la demo, 61 antes de la historia'},
            {'id': 'demo', 'state': 2, 'substates': [1], 'durationFrames': 2440, 'measured': True, 'music': 0x88,
             'texts': [],
             'notes': 'ver demos[]: demo 0 1107 ticks (2440 frames), demo 1 1292 ticks (2743 frames). Marcador normal'},
            {'id': 'story', 'state': 2, 'substates': [2, 3], 'durationFrames': 1710, 'measured': True, 'music': 0x86,
             'texts': [],
             'notes': 'historia que sube (storyLines, story). La orden FB del texto para la musica y el sub 3 acaba '
                      'en el acto'},
            {'id': 'gameStart', 'state': 3, 'substates': [0, 1, 2], 'durationFrames': 49, 'measured': True,
             'music': 0x87, 'texts': [title_texts[0]] + start_texts,
             'notes': 'el titulo se queda (sin animar) y PLAY START sustituye a PUSH SPACE KEY en la fila 17. '
                      'Parpadeo: E004 de 44 a 1, visible si E004 & 4 = 0 (1 frame oculto, luego 4 visibles / 4 ocultos). '
                      'Sub 0: 1 frame, sub 1: 45, sub 2: 1 + ~3 de borrado. Luego estado 4 (barrido 32 + carga) y '
                      'estado 5: 104 frames medidos de la tecla al juego'},
        ],
        'storyLines': story_lines,
        'story': {
            'textPtr': 'b15:AE28', 'font': '8x16 (la del epilogo)', 'screenCols': 32, 'music': 0x86,
            'scrollPxPerStep': 1, 'stepFramesCode': 2, 'stepFramesMeasured': 3, 'rowSteps': 8,
            'waitFramesMeasured': 513, 'durationFramesMeasured': 1710,
            'texts': [l['text'] for l in story_lines if l['kind'] == 'text'],
            'notes': 'pantalla negra. El texto entra por abajo y sube 1 px por paso (192 px: STORY queda arriba y '
                     'LATER... abajo), espera 255 pasos (513 frames), sigue subiendo 10 filas, para la musica (FB) y '
                     'sube 14 filas mas hasta salir. Medido: 384 pasos de 3 frames + 513 = 1710 frames. Solo hay '
                     'texto en ingles.',
        },
        'demo': demos[0],
        'demos': demos,
        'gameStart': {
            'blinkText': start_texts[0], 'blinkFrames': 45, 'music': 0x87,
            'keyToPlayFramesMeasured': 104,
            'map': 'castle', 'e041': 1, 'room': 1, 'x': spawn(rom, 1, 1)[1][0], 'y': spawn(rom, 1, 1)[1][1],
            'spawnByte': spawn(rom, 1, 1)[0], 'state': st,
            'notes': 'espacio/M en el titulo -> estado 3 (PLAY START, musica 0x87 del castillo que sigue en el juego) '
                     '-> estado 4 (barrido y carga de la sala) -> estado 5. Popolon en la sala 1 del castillo, en el '
                     'suelo, mirando a la derecha (E503 = 0).',
        },
    }
    return data


def main():
    out = ROOT / 'extracted' / 'unity' / 'attract.json'
    out.parent.mkdir(parents=True, exist_ok=True)
    data = build(Rom())
    out.write_text(json.dumps(data, indent=1, ensure_ascii=False))
    print(f"atraccion: {len(data['screens'])} pantallas, {len(data['storyLines'])} lineas de historia, "
          f"demos de {[d['inputCount'] for d in data['demos']]} entradas -> {out}")


if __name__ == '__main__':
    main()
