#!/usr/bin/env python3
"""Santuarios de los dioses: tiendas, contraseña, resurrección y pistas (docs/design_bible/08_santuarios.md).

  shrines.py   -> extracted/unity/shrines.json  (lo llama unity_bundle.py)

Fuentes en la ROM [C] (grupo de bancos 0 = b01-b03, grupo 4 = b13-b15):
  b02:9253 + 2*tipo  tienda de cada tipo de sala de dios (E690): 3 pares (precio BCD, objeto); objeto 0 = hueco vacio,
                     FF = opcion SI/NO de la Muerte
  b02:954C[tipo]     dios: 0 Muerte, 1 Hefesto, 2 Asclepio, 3 Demeter, 4 Ares, 5 Atenea (nombre = mensaje 0x18 + dios)
  b02:9308[tipo-14]  mensaje de los santuarios de pista (tipos 14-20); el tipo 8 (Asclepio) dice el mensaje 1
  b02:8F0A / 8F14    sala especial de cada mundo (E041 - 2 = mundo - 1) y su tipo; se pone en E690 al cargar la sala
  b15:B09E           tabla de mensajes: puntero -> (direccion en el bufer de nombres ED80, bytes..., FF). FE = linea
                     siguiente. Ingles = id + 0x20 si (0x002B) & 0x0F != 0 (BIOS no japonesa); japones = id.
  b02:8F43           logica por tipo (8F72 Muerte, 90C0 contraseña, 9126 tienda, 92E8 pista)
  b02:9A76 / 9A5F    entrada desde el castillo; b00:488E (estado 14) y 48C5 (estado 16)

Campos de shrines.json (JsonUtility: solo arrays de objetos):
  rules     reglas globales: sonidos, cantidades, topes, tiempos (frames a 60 Hz: la pantalla de dios corre cada frame)
  layout    pantalla del santuario en tiles de 8 px dentro de la zona de juego (32 x 20, fila 0 = y 32 de pantalla)
  messages  todos los mensajes: id (japones; el ingles es id + 0x20), texto ingles con '\\n' entre lineas y los iconos
            {arrows} {coins} {keys}, fila/columna donde se escribe, used = si algun santuario lo usa
  shrines   uno por sala de dios:
    map, room, type (E690), state (16 castillo / 14 sala especial de mundo), god, kind (shop, password, resurrection,
    hint), entry {...}, exit {...}, items [{slot, type, name, price, amount, rule, needsShield, setsShield}],
    messageIds y messages (texto ingles en el orden en que salen)
  entry (castillo, estado 16): pulsar abajo (tecla mantenida E009 bit 1) con el jugador (y = fila de arriba del tile que
    pisa, x = centro) en y = godY + 16..17 y x = godX + 6..10, sin estar en E500 = 4..7. needsItem = 0x2A (Salt) en la
    Muerte. rockOnTop = hay una piedra del castillo en (godX, godY): tapa el sitio y hay que romperla antes.
  entry (mundo, estado 14): subir por la escalera de la fila 0 de fromRoom (columnas ladderCol, ladderCol+1) hasta
    y < 40: el cambio de sala hacia arriba lleva a la sala especial. hiddenLadder = la escalera solo aparece tras el
    evento openedBy (room_events.json).
  exit: abajo (pulsacion nueva, E008 bit 1). Castillo: reaparece en la misma sala, en x = godX + 8, y = godY + 16.
    Mundo: cambio de sala hacia abajo (E01A = 2) a toRoom, con x = 128 e y = 40 (encima de la escalera).

Todo sale de la ROM del usuario: extracted/ no se versiona.
"""
import json
from galious_rom import Rom, ROOT
from items_export import NAMES

G2, G15 = 0, 4                     # grupos de bancos de b02 y b15
SHOP_TABLE, GOD_TABLE, HINT_TABLE = 0x9253, 0x954C, 0x9308
WORLD_ROOM, WORLD_TYPE = 0x8F0A, 0x8F14
MSG_TABLE, EN = 0xB09E, 0x20
GOD_NAMES = ['Death', 'Hephaestus', 'Asclepius', 'Demeter', 'Ares', 'Athena']
ICONS = {0x54: '{arrows}', 0x56: '{coins}', 0x58: '{keys}'}          # iconos de 2 tiles (0x55, 0x57, 0x59 = mitad derecha)
PUNCT = {0x00: ' ', 0x26: 'x', 0x27: '!', 0x29: '?', 0x2A: '.', 0x2B: ','}
QUIVER, KEY, SALT = 0x2D, 0x2F, 0x2A
SHIELDS = {0x28: (0, 1), 0x2B: (1, 2), 0x2C: (2, 3)}               # objeto -> (nivel E097 necesario, nivel que deja)
# mensajes (id japones) que dice cada clase de santuario, en orden (b02:8F72, 90C0, 9126)
FLOW = {
    'shop': [0x05, 0x12],
    'password': [0x0C, 0x0D, 0x0F, 0x0E, 0x10],
    'resurrection': [0x00, 0x02, 0x0D, 0x03, 0x04],
}


def char(b):
    if 1 <= b <= 10:
        return str(b - 1)
    if 0x0B <= b <= 0x24:
        return chr(ord('A') + b - 0x0B)
    return PUNCT.get(b)


def message(rom, mid):
    p = rom.word(MSG_TABLE + 2 * mid, G15)
    dest = rom.word(p, G15)
    a, text = p + 2, ''
    while (b := rom.byte(a, G15)) != 0xFF:
        a += 1
        if b == 0xFE:
            text += '\n'
        elif b in ICONS:
            text += ICONS[b]
        elif b - 1 in ICONS:
            pass
        else:
            c = char(b)
            text += c if c is not None else f'{{{b:02X}}}'
    off = dest - 0xED80
    return {'id': mid, 'text': text.rstrip(' '), 'row': off // 32, 'col': off % 32}


def bcd(b):
    return (b >> 4) * 10 + (b & 15)


def kind_of(t):
    if t == 1:
        return 'password'
    if t == 3:
        return 'resurrection'
    if t == 8 or t >= 14:
        return 'hint'
    return 'shop'


def shop_items(rom, t):
    p = rom.word(SHOP_TABLE + 2 * t, G2)
    out = []
    for slot in range(3):
        price, item = rom.byte(p + 2 * slot, G2), rom.byte(p + 2 * slot + 1, G2)
        if item in (0, 0xFF):
            continue
        need, sets = SHIELDS.get(item, (0, 0))
        rule = 'quiver' if item == QUIVER else 'key' if item == KEY else 'shield' if item in SHIELDS else 'flag'
        out.append({'slot': slot, 'type': item, 'name': NAMES.get(item, '?'), 'price': bcd(price),
                    'amount': 100 if item == QUIVER else 1, 'rule': rule, 'needsShield': need, 'setsShield': sets})
    return out


def messages_for(rom, t, kind):
    if kind == 'hint':
        return [1] if t == 8 else [rom.byte(HINT_TABLE + t - 14, G2)]
    return FLOW[kind]


def world_entry(maps, events, k, shrine_room):
    rooms = {r['id']: r for r in maps['rooms']}
    below = next(r for r in maps['rooms'] if r['up'] == shrine_room and not r.get('shrine'))
    ev = next((e for e in events if e['world'] == f'world{k:02d}' and e['room'] == below['id']
               and e['kind'] == 'escalera' and e['pos']['row'] == 0), None)
    if ev:
        col, opened = ev['pos']['col'], ev['opened_by']
    else:
        row0 = below['collision'][0]
        col, opened = row0.index('HH'), ''
    return {'how': 'ladderUp', 'fromRoom': below['id'], 'ladderCol': col, 'x': 8 * col + 8, 'exitBelowY': 40,
            'hiddenLadder': ev is not None, 'openedBy': opened}, rooms[shrine_room]['down']


def build(rom):
    msgs = [message(rom, m) for m in range(0x14)]
    en = [message(rom, m + EN) for m in range(0x14)]
    for m, e in zip(msgs, en):
        m['text'], m['row'], m['col'] = e['text'], e['row'], e['col']
    castle = json.loads((ROOT / 'extracted' / 'maps' / 'castle.json').read_text())
    unity_castle = json.loads((ROOT / 'extracted' / 'unity' / 'castle.json').read_text())
    rocks = {r['id']: {(k['x'], k['y']) for k in r['rocks']} for r in unity_castle['rooms']}
    events = json.loads((ROOT / 'extracted' / 'entities' / 'room_events.json').read_text())['on_load']
    shrines = []

    def add(mapname, room, t, state, entry, exit_):
        kind = kind_of(t)
        ids = messages_for(rom, t, kind)
        god = GOD_NAMES[rom.byte(GOD_TABLE + t, G2)]
        shrines.append({'map': mapname, 'room': room, 'type': t, 'state': state, 'god': god, 'kind': kind,
                        'entry': entry, 'exit': exit_, 'items': shop_items(rom, t) if kind == 'shop' else [],
                        'messageIds': ids, 'messages': [msgs[i]['text'] for i in ids]})

    for room, g in sorted(castle['gods'].items(), key=lambda kv: kv[1]['type']):
        room, t = int(room), g['type']
        entry = {'how': 'down', 'godX': g['x'], 'godY': g['y'], 'xMin': g['x'] + 6, 'xMax': g['x'] + 10,
                 'yMin': g['y'] + 16, 'yMax': g['y'] + 17, 'needsItem': SALT if t == 3 else 0,
                 'rockOnTop': (g['x'], g['y']) in rocks.get(room, set())}
        add('castle', room, t, 16, entry, {'key': 'down', 'toRoom': room, 'x': g['x'] + 8, 'y': g['y'] + 16})
    for k in range(1, 10):                                  # mundo k: E041 = k + 1, indice k - 1
        room, t = rom.byte(WORLD_ROOM + k - 1, G2), rom.byte(WORLD_TYPE + k - 1, G2)
        maps = json.loads((ROOT / 'extracted' / 'unity' / f'world{k:02d}.json').read_text())
        entry, down = world_entry(maps, events, k, room)
        add(f'world{k:02d}', room, t, 14, entry, {'key': 'down', 'toRoom': down, 'x': 128, 'y': 40})
    used = {i for s in shrines for i in s['messageIds']}
    for m in msgs:
        m['used'] = m['id'] in used
    rules = {
        'musicShrine': 0x8D, 'musicAfterCastleShrine': 0x88, 'musicAfterWorldShrine': 0x8A,
        'soundEnterCastleShrine': 0x82, 'soundCursor': 0x2C, 'soundBuy': 0x2D, 'soundRefused': 0x2F,
        'soundResurrection': 0x30,
        'quiverArrows': 100, 'keyAmount': 1, 'counterCap': 999, 'pricesBcdTwoDigits': True,
        'sinkPixels': 16, 'sinkFrames': 64,
        'resurrectionArrows': 100, 'resurrectionCoins': 100, 'resurrectionKeys': 20,
        'resurrectionCreditsStart': 1, 'resurrectionCreditsCartridgeBonus': 99,
        'loverDescendPixels': 80, 'loverDescendFramesPerPixel': 2, 'deathAutoExitFrames': 180,
        'playerScreenY': 0xAF, 'playerScreenX': 0x80,
        'languageOffsetEnglish': EN,
    }
    layout = {
        'godNameRow': 1, 'portraitRow': 2, 'portraitCol': 15,
        'textBox': {'top': 4, 'bottom': 10, 'left': 4, 'right': 27}, 'cursorBox': {'top': 11, 'bottom': 15, 'left': 4, 'right': 27},
        'textRow': 5, 'textCol': 6, 'textWidth': 20,
        'itemRow': 7, 'itemCols': [9, 15, 21], 'priceRow': 9, 'priceCols': [9, 15, 21],
    }
    return {'rules': rules, 'layout': layout, 'messages': msgs, 'shrines': shrines}


def main():
    data = build(Rom())
    out = ROOT / 'extracted' / 'unity' / 'shrines.json'
    out.write_text(json.dumps(data, ensure_ascii=False, indent=1))
    print(f'santuarios: {len(data["shrines"])} -> {out.relative_to(ROOT)}')


if __name__ == '__main__':
    main()
