#!/usr/bin/env python3
"""Extrae de la ROM el mapa de salas del castillo y de los 10 mundos.

Fuentes (ver docs/world_maps.md):
  - vecinos: tabla de punteros por mundo en A9EF (bancos 7-9), 4 bytes por sala
    (arriba, abajo, izquierda, derecha; FF = pared; >= F6 = salida al castillo; F5 = salida
    especial del mundo 10, sin identificar)
  - puertas de mundo (solo castillo): salas en 9990, posicion (Y, X) en 9997
  - salas de dioses (solo castillo): tabla en 9DDA (sala, tipo, Y, X), terminada en 0
  - sala del jefe: tabla en 5DFB indexada por mundo
Las salas se colocan en una cuadricula recorriendo los enlaces desde la sala 1 (entrada).

Salida: extracted/maps/<mundo>.json y .svg, y extracted/maps/index.json
"""
import json
from collections import deque
from galious_rom import Rom, ROOT

LINKS_GROUP = 2          # bancos 7, 8, 9
LINKS_TABLE = 0xA9EF
DOOR_ROOMS, DOOR_POS = 0x9990, 0x9997   # banco 2 (grupo 0)
GOD_TABLE = 0x9DDA                       # banco 2
BOSS_ROOM = 0x5DFB
BOSS_WORDS = 0x9B4F      # 3 bytes por mundo: puntero, longitud (indexado por E041)
GODS = {1: 'Demeter (contraseña)', 2: 'Hefesto (tienda)', 3: '? (necesita E099)',
        5: 'Ares (tienda)', 6: 'Hefesto (tienda)', 7: 'Hefesto (tienda)',
        8: 'Asclepio (pista)', 9: 'Ares (tienda)', 10: 'Ares (tienda)', 11: 'Demeter (tienda)'}
DIRS = (('up', 0, -1), ('down', 0, 1), ('left', -1, 0), ('right', 1, 0))
OUT = ROOT / 'extracted' / 'maps'


def world_name(e041):
    return 'castle' if e041 == 1 else f'world{e041 - 1:02d}'


def extract(rom):
    ptrs = [rom.word(LINKS_TABLE + 2 * w, LINKS_GROUP) for w in range(12)]
    worlds = {}
    for e041 in range(1, 12):
        # Las salas van de 1 a N: la entrada 0 no se usa y la N cae donde empieza la tabla
        # del mundo siguiente (cuya entrada 0 tampoco se usa). El ultimo mundo acaba en la
        # primera entrada todo FF.
        start = ptrs[e041]
        if e041 < 11:
            count = (ptrs[e041 + 1] - start) // 4
        else:
            count = 1
            while rom.block(start + 4 * (count + 1), 4, LINKS_GROUP) != b'\xff' * 4:
                count += 1
        links = {}

        def get(room):
            if room not in links:
                b = rom.block(start + 4 * room, 4, LINKS_GROUP)
                links[room] = dict(zip(('up', 'down', 'left', 'right'), b))
            return links[room]

        # recorrido desde la entrada; en el ultimo mundo no sabemos el tamano de la tabla
        rooms = range(1, count + 1)
        for r in rooms:
            get(r)
        pos, grid, conflicts = {1: (0, 0)}, {(0, 0): 1}, []
        q = deque([1])
        while q:
            r = q.popleft()
            x, y = pos[r]
            for d, dx, dy in DIRS:
                n = get(r)[d]
                if n == 0xFF or n >= 0xF5:
                    continue
                if n > count:
                    conflicts.append(f'{r}.{d} -> {n} fuera de la tabla')
                    continue
                p = (x + dx, y + dy)
                if n in pos:
                    if pos[n] != p:
                        conflicts.append(f'{r}.{d} -> {n}: ya colocada en {pos[n]}, aqui seria {p}')
                    continue
                if p in grid:
                    conflicts.append(f'{r}.{d} -> {n}: celda {p} ocupada por {grid[p]}')
                    continue
                pos[n], grid[p] = p, n
                q.append(n)
        exits = [(r, d, hex(get(r)[d])) for r in pos for d, _, _ in DIRS if 0xF5 <= get(r)[d] < 0xFF]
        boss = rom.byte(BOSS_ROOM + e041) if e041 >= 2 else None
        word = None
        if e041 >= 2:
            p, n = rom.word(BOSS_WORDS + 3 * e041, 0), rom.byte(BOSS_WORDS + 3 * e041 + 2, 0)
            word = rom.block(p, n, 0).decode()   # tabla en el banco 2 (grupo 0)
        worlds[world_name(e041)] = {
            'e041': e041, 'table': hex(start), 'table_rooms': count,
            'reachable': len(pos), 'unreachable': sorted(set(rooms) - set(pos)),
            'rooms': {r: {'pos': pos[r], 'links': get(r)} for r in sorted(pos)},
            'exits_to_castle': exits, 'boss_room': boss, 'boss_word': word,
            'conflicts': conflicts,
        }
    castle = worlds['castle']
    castle['doors'] = {}
    for c in range(1, 10):
        room = rom.byte(DOOR_ROOMS + c - 1, 0)
        yx = rom.word(DOOR_POS + 2 * c, 0)
        castle['doors'][room] = {'world': c, 'y': yx & 0xFF, 'x': yx >> 8}
    castle['gods'] = {}
    a = GOD_TABLE
    while rom.byte(a, 0):
        room, typ, y, x = rom.block(a, 4, 0)
        castle['gods'][room] = {'type': typ, 'who': GODS.get(typ, '?'), 'y': y, 'x': x}
        a += 4
    return worlds


def svg(name, w):
    cells = {tuple(v['pos']): r for r, v in w['rooms'].items()}
    xs = [p[0] for p in cells]; ys = [p[1] for p in cells]
    x0, y0 = min(xs), min(ys)
    S, M = 44, 20
    W = max((max(xs) - x0 + 1) * S + 2 * M, 460)
    H = (max(ys) - y0 + 1) * S + 2 * M + 24
    doors, gods = w.get('doors', {}), w.get('gods', {})
    out = [f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" font-family="monospace">',
           f'<rect width="{W}" height="{H}" fill="#111"/>',
           f'<text x="{M}" y="16" fill="#ddd" font-size="13">{name}: {w["reachable"]} salas'
           + (f' · jefe en sala {w["boss_room"]} ({w["boss_word"]})' if w['boss_room'] is not None else '')
           + '</text>']
    for (x, y), r in cells.items():
        px, py = M + (x - x0) * S, M + 24 + (y - y0) * S
        links = w['rooms'][r]['links']
        fill = '#345'
        if r == 1: fill = '#2a6'
        if r == w['boss_room']: fill = '#a33'
        if r in doors: fill = '#a82'
        if r in gods: fill = '#649'
        out.append(f'<rect x="{px + 4}" y="{py + 4}" width="{S - 8}" height="{S - 8}" fill="{fill}" stroke="#9ab"/>')
        label = str(r) + (f' P{doors[r]["world"]}' if r in doors else '') + (f' D{gods[r]["type"]}' if r in gods else '')
        out.append(f'<text x="{px + S / 2}" y="{py + S / 2 + 4}" fill="#fff" font-size="10" text-anchor="middle">{label}</text>')
        for d, dx, dy in DIRS:
            n = links[d]
            if n == 0xFF:
                continue
            cx, cy = px + S / 2, py + S / 2
            ex, ey = cx + dx * S / 2, cy + dy * S / 2
            col = '#ff0' if n >= 0xF5 else '#9ab'
            out.append(f'<line x1="{cx + dx * (S / 2 - 4)}" y1="{cy + dy * (S / 2 - 4)}" x2="{ex}" y2="{ey}" stroke="{col}" stroke-width="3"/>')
    out.append('</svg>')
    return '\n'.join(out)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    worlds = extract(Rom())
    index = {}
    for name, w in worlds.items():
        (OUT / f'{name}.json').write_text(json.dumps(w, indent=1, default=str))
        (OUT / f'{name}.svg').write_text(svg(name, w))
        index[name] = {k: w[k] for k in ('e041', 'table_rooms', 'reachable', 'unreachable', 'boss_room',
                                         'boss_word', 'exits_to_castle', 'conflicts')}
        print(f"{name:8s} tabla={w['table_rooms']} alcanzables={w['reachable']} "
              f"inalcanzables={w['unreachable']} jefe={w['boss_room']} {w['boss_word'] or ''} "
              f"salidas={w['exits_to_castle']} conflictos={len(w['conflicts'])}")
        for c in w['conflicts']:
            print('   ', c)
    (OUT / 'index.json').write_text(json.dumps(index, indent=1, default=str))


if __name__ == '__main__':
    main()
