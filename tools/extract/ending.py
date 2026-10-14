#!/usr/bin/env python3
"""Final del juego para el importador de Unity: extracted/unity/ending.json (docs/ending.md).

  ending.py   -> extracted/unity/ending.json   (lo llama unity_bundle.py)

Lo que pasa tras vencer a Galious: sala del jefe sin llave, sala 6 del mundo 10 con 10 piedras (una esconde al
bebe), y el estado 17 (sala que se deshace, escena del castillo, epilogo y creditos que suben, vuelta al titulo).
Constantes leidas de la ROM con su direccion; los tiempos medidos en openMSX van marcados (measured).

Fuentes [C] (grupo 0 = b01-b03, grupo 1 = b04-b06, grupo 4 = b13-b15):
  b00:4B8E-4BA7  estado 19, sub 2: en el mundo 10 (E041 = 11) no hay llave; E64B = 6 (salida derecha -> sala 6)
  b06:B5CB       generadores de la sala 6: 10 x 0x5C (piedra)
  b06:AABE       inicio de la piedra 0x5C: la primera tira F033 = (rnd & 3) + (rnd & 7); la piedra numero F033
                 (orden de la lista, desde 0) lleva IX+1F = 1 (bebe). Con F033 = 10 ninguna lo lleva
  b03:B69B       muerte de la piedra: crea 0x6B (bebe) si IX+1F, si no 0x39 (fantasma), sin explosion ni botin
  b06:A8DF/A90D  0x39: espiral alrededor de (y, x +- 10): angulo +8/256 y radio +0x80/256 px por tick
  b03:B1F7/B2AD  tocar 0x6B (solo si E518 = 0): tiles 7A 7B / 7C 7D en su sitio, E6A8 = puntero, E024 = 1 -> 17
  b00:49BA       estado 17 (9 subestados, 1 vez por frame), tabla 49BE
  b03:A05A/A06D  la sala se borra por anillos, de fuera adentro (respeta los 2x2 tiles del bebe, b03:A0E1)
  b15:B7C0/B847  escena del castillo: dibujo 20x14 tiles (b15:B989), banderas, bebe, bocadillos
  b01:6A1E/6ADC  epilogo: texto en b15:A84A, 1 fila de 8 px cada 8 pasos de 1 px; FB = parar musica,
                 FD n = esperar n pasos, FE = fila vacia, FF = fin (E02E = 0)
  b00:4A62       espera a que calle la musica (b00:50F5) -> b00:455F: borra E002 bit 6 y va al estado 0

Unidades: frames de video a 60 Hz. El estado 17 corre una vez por frame; el epilogo avanza un paso cada 2 pasadas
(E003 bit 0), pero la subida del texto carga la CPU y en openMSX cada paso dura 3 frames (medido).
Coordenadas de las piedras como en entities.json: pixeles de pantalla, con las 32 lineas del marcador en la y.

Campos de ending.json (JsonUtility: solo objetos y arrays):
  bossRoomAfterDefeat   sala del jefe tras el estado 19: sin llave, salidas, musica
  rockRoom              sala 6: rocks[] {index, x, y}, rock {type, hp, ...}, babyRule, baby, ghost
  screens[]             {id, state, substates[], durationFrames, measured, music, sounds[], texts[], notes}
  castleScene           fases de la escena del castillo
  epilogue              lines[] {kind: text | blank | wait | musicStop | end, col, text, rows, steps}, ritmo
  after                 que pasa al acabar
"""
import json
from galious_rom import Rom, ROOT
from entities import spawns

G0, G1, G4 = 0, 1, 4
E041_WORLD10, FINAL_ROOM, BOSS_ROOM = 11, 6, 5
ROCK, BABY, GHOST = 0x5C, 0x6B, 0x39
DESCRIPTOR, DAMAGE_TABLE, EXP_TABLE, LOOT_RULE = 0x67B6, 0x64E5, 0xBF27, 0x647B
EPILOGUE_PTR = 0x6ACB          # b01: LD HL,A84A (texto del epilogo, banco 15)
PROLOGUE_PTR = 0x6AC6          # b01: LD HL,AE28 (la misma rutina con la historia del titulo, b00:4369)


def font(b):
    """Letra de la fuente grande del epilogo (8x16): b = mitad de arriba; la de abajo es b - 1."""
    if b == 0:
        return ' '
    if 0x0B <= b <= 0x3D and b % 2:
        return chr(ord('A') + (b - 0x0B) // 2)
    # la fuente solo trae los digitos de '1987' (03 1, 05 7, 07 8, 09 9) y cuatro signos (45 = apostrofo, en la historia)
    return {0x03: '1', 0x05: '7', 0x07: '8', 0x09: '9', 0x3F: '.', 0x41: ',', 0x43: '©', 0x45: "'"}.get(b, f'{{{b:02X}}}')


def epilogue(rom, ptr):
    """Registros del texto: (col, n, n tiles) por fila de 8 px; cada linea de texto son dos filas (arriba y abajo)."""
    a, rows = ptr, []
    while True:
        b = rom.byte(a, G4)
        a += 1
        if b == 0xFB:
            rows.append({'kind': 'musicStop'})
        elif b == 0xFD:
            rows.append({'kind': 'wait', 'steps': rom.byte(a, G4)})
            a += 1
        elif b == 0xFE:
            rows.append({'kind': 'blank'})
        elif b == 0xFF:
            rows.append({'kind': 'end'})
            break
        else:
            n = rom.byte(a, G4)
            tiles = rom.block(a + 1, n, G4)
            a += 1 + n
            rows.append({'kind': 'row', 'col': b, 'tiles': tiles})
    # juntar las dos filas de cada linea (la de abajo usa los tiles - 1) y los huecos seguidos
    lines, i = [], 0
    while i < len(rows):
        r = rows[i]
        if r['kind'] == 'row':
            nxt = rows[i + 1]
            assert nxt['kind'] == 'row' and all(t2 == (t1 - 1 if t1 else 0) for t1, t2 in zip(r['tiles'], nxt['tiles']))
            lines.append({'kind': 'text', 'col': r['col'], 'text': ''.join(font(t) for t in r['tiles']), 'rows': 2,
                          'steps': 16})
            i += 2
        elif r['kind'] == 'blank':
            if lines and lines[-1]['kind'] == 'blank':
                lines[-1]['rows'] += 1
                lines[-1]['steps'] += 8
            else:
                lines.append({'kind': 'blank', 'col': 0, 'text': '', 'rows': 1, 'steps': 8})
            i += 1
        else:
            steps = r.get('steps', 0)
            lines.append({'kind': r['kind'], 'col': 0, 'text': '', 'rows': 0, 'steps': steps})
            i += 1
    return lines


def enemy(rom, t):
    d = rom.block(DESCRIPTOR + 5 * t, 5, G1)
    return {'type': t, 'hp': d[3], 'damage': rom.byte(DAMAGE_TABLE + t, G1), 'experience': rom.byte(EXP_TABLE + t, G0),
            'sprite': d[1]}


def build(rom):
    rocks = [{'index': i, 'x': s['x'], 'y': s['y']} for i, s in enumerate(spawns(rom, E041_WORLD10, FINAL_ROOM))]
    assert len(rocks) == 10 and all(s['type'] == ROCK for s in spawns(rom, E041_WORLD10, FINAL_ROOM))
    and_b, and_a = rom.byte(0xAAD0, G1), rom.byte(0xAAD6, G1)          # b06:AACF AND 3, AAD5 AND 7
    ghost = enemy(rom, GHOST)
    ghost.update({
        'lootRule': rom.byte(LOOT_RULE + GHOST, G1), 'lootWhenLastAlive': rom.byte(LOOT_RULE + GHOST, G1) == 3,
        'centerXOffset': rom.byte(0xA903, G1),        # b06:A902: centro x = x de la piedra +10 o -10 (rnd & 1)
        'centerYOffset': 0,
        'angleStepPerTick': rom.byte(0xA8F2, G1),     # b06:A8EF: 256 = vuelta entera -> 32 ticks por vuelta
        'radiusGrowthPerTick256': rom.byte(0xA8F6, G1),   # b06:A8F3: +0x80/256 = 0,5 px por tick
        'startRadius': 0, 'startAngle': 0,
        'despawnWhenOffscreen': True,
        'notes': 'espiral que se abre: y = cy + r*sin(a), x = cx + r*cos(a) (tabla b06:B374); desaparece al salir '
                 'de la zona visible (visto en y > 184). Mata: experiencia y botin de regla 3, nivel 3 (enemies.json). '
                 'No es el 0x28 (ectoplasma de la guia): el Helmet no protege.',
    })
    rock = enemy(rom, ROCK)
    rock.update({'hitSound': 0x1E, 'breakSound': 0x20, 'swingsToBreak': rock['hp'],
                 'notes': 'enemigo 0x5C (no es una piedra del castillo): sprite, caja fija 16x16 (caja 0), no hace '
                          'dano ni se mueve. Cada golpe de espada o arma resta 1 (2 con el objeto que suma dano, E094). '
                          'Al morir no da experiencia ni botin ni explosion: crea el bebe o un fantasma en su sitio.'})
    baby = enemy(rom, BABY)
    baby.update({'touchNeedsNotBlinking': True, 'anyCharacter': True,
                 'tilesOnTouch': [0x7A, 0x7B, 0x7C, 0x7D],
                 'notes': 'sprite quieto (actualizacion RET), no golpeable ni hace dano. Lo coge quien lo toque '
                          '(Popolon o Aphrodite; no se mira E501 ni el otro personaje) si no esta parpadeando '
                          '(E518 = 0). Al tocarlo: tiles 7A 7B / 7C 7D en su sitio, E024 = 1 -> estado 17.'})
    epi = epilogue(rom, rom.word(EPILOGUE_PTR, G0))
    data = {
        'source': 'ROM + openMSX (docs/ending.md)',
        'frameRate': 60,
        'bossRoomAfterDefeat': {
            'map': 'world10', 'room': BOSS_ROOM, 'state19Frames': 1 + 128, 'dropsKey': False,
            'finalBlowSound': 0x84, 'queuedMusic': 0x8B, 'queuedMusicDelayFramesMeasured': 156,
            'exitsAfterDefeat': {'up': -1, 'down': -1, 'left': -1, 'right': FINAL_ROOM},
            'notes': 'Estado 19 igual que en los otros mundos (el jefe se deshace en puntos 128 frames), pero sin '
                     'Great Key ni estado 20: E64B = 6 (b00:4BA2) y vuelta al estado 5. La sala sigue negra (sin '
                     'decoracion) y con las demas salidas cerradas; la izquierda, que llevaba a la sala 4, queda '
                     'cerrada. Se sale por la derecha del pasillo de abajo (filas 16-18) a la sala 6. La musica 0x8B '
                     '(la del golpe final, b03:BD48, en espera por E01D/E01E) sigue sonando.',
        },
        'rockRoom': {
            'map': 'world10', 'e041': E041_WORLD10, 'room': FINAL_ROOM, 'enteredFrom': BOSS_ROOM,
            'links': {'up': -1, 'down': -1, 'left': -1, 'right': -1},
            'spriteTypes': [GHOST],
            'rocks': rocks,
            'rock': rock,
            'babyRule': {
                'rollWhen': 'al iniciar la primera piedra, al cargar la sala (cada vez que se entra o se reaparece)',
                'roll': f'F033 = (rnd & {and_b}) + (rnd & {and_a}); rnd = (R >> 3 rotado) XOR E003 (b05:81F1)',
                'rangeMin': 0, 'rangeMax': and_b + and_a,
                'babyRockIndexIs': 'F033 (indice en rocks[], orden de la lista de la ROM)',
                'noBabyWhen': 10,
                'noBabyProbability': '1/32 con dados independientes y uniformes',
                'remakeSuggestion': 'elegir 0-9 uniforme; o reproducir la suma (0-3)+(0-7) con su sesgo hacia 3-7',
                'notes': 'Con F033 = 10 ninguna piedra tiene al bebe [F]: la sala no tiene salidas y solo se sale '
                         'perdiendo una vida, que reaparece en la sala 6 con las 10 piedras y una tirada nueva [O]. '
                         'Distribucion de la suma (sobre 32): 0:1 1:2 2:3 3:4 4:4 5:4 6:4 7:4 8:3 9:2 10:1.',
            },
            'baby': baby,
            'otherRocksRelease': ghost,
        },
        'castleScene': {
            'picture': {'tiles': 'b15:B989', 'wTiles': 20, 'hTiles': 14},
            'phases': [
                {'phase': 0, 'frames': rom.byte(0xB7C4, G4), 'notes': 'castillo con la bandera roja de Galious en el mastil; '
                 'un destello (sprite) parpadea junto a la bandera cada 16 frames (dos patrones)'},
                {'phase': 1, 'frames': 137, 'notes': 'la bandera roja baja 1 px cada 4 frames (y 0x2E -> 0x3F, 65 frames) y '
                 'se cambia por la bandera azul con la cruz, que sube igual hasta y 0x2E (72 frames)'},
                {'phase': 2, 'frames': 52, 'notes': 'el bebe (2 sprites, amarillo y blanco) sube 12 px entre Popolon y '
                 'Aphrodite, 1 px cada 4 frames (y 0x6B -> 0x5F)'},
                {'phase': 3, 'frames': 386, 'notes': 'Popolon y Aphrodite se mueven (dos tiles alternan cada 16 frames); tras '
                 '64 frames, cada 64 frames sale al azar un bocadillo: "NICE!" a la izquierda o "GREAT!" a la derecha '
                 '(uno a la vez). Acaba 510 frames despues del cambio de bandera (F02B = 255, un paso cada 2 frames)'},
            ],
            'balloons': ['NICE!', 'GREAT!'],
        },
        'epilogue': {
            'textPtr': 'b15:A84A', 'font': '8x16 (dos filas de tiles por linea)', 'screenCols': 32,
            'scrollPxPerStep': 1, 'stepFramesCode': 2, 'stepFramesMeasured': 3, 'rowSteps': 8,
            'waitStepFramesMeasured': 2,
            'lines': epi,
            'texts': [l['text'] for l in epi if l['kind'] == 'text'],
            'notes': 'El texto entra por abajo y sube. Las esperas (wait, 255 pasos = 510 frames) dejan quieta la '
                     'pantalla: con el epilogo completo, con el final del staff y con PRESENTED BY KONAMI. musicStop '
                     'para la musica (orden 0xFF) 13 filas antes del final.',
        },
        'screens': [
            {'id': 'touchBaby', 'state': 17, 'substates': [0, 1], 'durationFrames': rom.byte(0x49D6, G0),
             'measured': True, 'music': 0xFF, 'sounds': [], 'texts': [],
             'notes': 'orden 0xFF: calla musica y efectos. La sala se congela; piedras y fantasmas dejan de verse, el '
                      'jugador y el bebe (ya en tiles) se quedan. El marcador sigue.'},
            {'id': 'roomCollapse', 'state': 17, 'substates': [2], 'durationFrames': 101, 'measured': True,
             'music': 0, 'sounds': [], 'texts': [],
             'notes': f'{rom.byte(0xA06A, G0)} anillos de la sala de 32x20 tiles, uno cada {rom.byte(0xA073, G0)} frames, '
                      'de fuera adentro (rectangulo 32x20, 30x18 ... 14x2), se ponen a tile 0 (negro). Los 2x2 tiles '
                      'del bebe no se borran.'},
            {'id': 'blackPause', 'state': 17, 'substates': [3], 'durationFrames': rom.byte(0x49EC, G0),
             'measured': True, 'music': 0, 'sounds': [], 'texts': [],
             'notes': 'zona de juego negra con el jugador y el bebe; marcador visible'},
            {'id': 'castleLoad', 'state': 17, 'substates': [4], 'durationFrames': 19, 'measured': True,
             'music': rom.byte(0x4A17, G0), 'sounds': [], 'texts': [],
             'notes': 'borra la pantalla (sin marcador), dibuja el castillo con marco de hojas y empieza la musica 0x92'},
            {'id': 'castle', 'state': 17, 'substates': [5], 'durationFrames': 766, 'measured': True,
             'music': rom.byte(0x4A17, G0), 'sounds': [], 'texts': ['NICE!', 'GREAT!'],
             'notes': 'ver castleScene. Los bocadillos son dibujos de tiles (b15:BAA1, BAC1), no texto'},
            {'id': 'epilogueLoad', 'state': 17, 'substates': [6], 'durationFrames': 28, 'measured': True,
             'music': rom.byte(0x4A4A, G0), 'sounds': [], 'texts': [],
             'notes': 'pantalla negra, carga la fuente grande; empieza la musica 0x93 (60,3 s, vuelve a 9,1 s)'},
            {'id': 'epilogue', 'state': 17, 'substates': [7], 'durationFrames': 5417, 'measured': True,
             'music': rom.byte(0x4A4A, G0), 'sounds': [], 'texts': [l['text'] for l in epi if l['kind'] == 'text'],
             'notes': 'epilogo y creditos que suben (ver epilogue). La musica se corta 312 frames antes del final'},
            {'id': 'end', 'state': 17, 'substates': [8], 'durationFrames': 1, 'measured': True, 'music': 0,
             'sounds': [], 'texts': [],
             'notes': 'espera a que no suene musica y salta al estado 0 (logo de Konami), borrando E002 bit 6'},
        ],
        'after': {
            'nextState': 0, 'titleAfterFramesMeasured': 367, 'gameInProgressFlagCleared': True,
            'secondLoop': False, 'password': False, 'variations': False,
            'notes': 'Vuelve a la intro (logo de Konami, estado 0) y al titulo (estado 1); la demo sigue como siempre. '
                     'No hay segunda vuelta, ni contraseña final, ni puntuacion: una partida nueva empieza desde cero '
                     '(castillo, sala 1, puertas cerradas) [O]. El final no depende del tiempo, la experiencia, las '
                     'vidas ni del personaje activo: el estado 17 no lee nada de eso y el dibujo del castillo trae '
                     'siempre a Popolon y Aphrodite [C].',
        },
    }
    total = sum(s['durationFrames'] for s in data['screens'])
    data['totalFramesMeasured'] = total
    return data


def main():
    out = ROOT / 'extracted' / 'unity' / 'ending.json'
    out.parent.mkdir(parents=True, exist_ok=True)
    data = build(Rom())
    out.write_text(json.dumps(data, indent=1, ensure_ascii=False))
    print(f"final: {len(data['rockRoom']['rocks'])} piedras, {len(data['screens'])} pantallas, "
          f"{len(data['epilogue']['texts'])} lineas de texto, {data['totalFramesMeasured']} frames -> {out}")


if __name__ == '__main__':
    main()
