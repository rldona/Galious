#!/usr/bin/env python3
"""Clases de colision de los tiles y cuadricula de colision de cada sala.

Reglas leidas del codigo (el juego usa el numero de tile del mapa de nombres en RAM, ED00):
  - solido: tile >= F020, umbral que b00:554D pone a 0xBA, o a 0xF0 durante el combate con un jefe
    (E0D2 != 0). Lo comprueban b00:54FF-554C (jugador) y b00:4DC8 (enemigos, > 0xB9).
  - escalera: tiles 9A y 9B (b03:AEE1: tile - 9A < 2 en los dos lados del jugador).
  - agua: B6 y B7 (E511 = 1); lava: B8 y B9 (E511 = 2), en b03:B028. En el mundo 5, salas 12 y 13,
    con el Oar la B6 no hunde. En el mundo 8, con el Carpet (E087) las filas 12-19 pasan a F0 (b00:5575).
  - todo lo demas es fondo atravesable.
Incluye las superposiciones de la sala al cargar (piedras BA-BD, muros F0, estatuas F1-FC, columnas
de puertas C5-D2, escaleras extra 9A 9B), tal como las pinta overlays.py.

  collision.py            -> extracted/maps/collision/<mapa>.json (filas de texto por sala)
Leyenda: '#' solido, 'H' escalera, '~' agua (B6 B7), 'L' lava (B8 B9), '.' fondo,
'j' solido normal que es atravesable en combate de jefe (BA-EF).
"""
import json
from galious_rom import Rom, ROOT
from render_rooms import tilemap
import overlays

SOLID, SOLID_BOSS = 0xBA, 0xF0


def tile_class(t, boss=False):
    if t >= (SOLID_BOSS if boss else SOLID):
        return '#'
    if t in (0x9A, 0x9B):
        return 'H'
    if t in (0xB6, 0xB7):
        return '~'
    if t in (0xB8, 0xB9):
        return 'L'
    if t >= SOLID:          # solo en combate: BA-EF dejan de ser solidos
        return 'j'
    return '.'


def room_names(rom, e041, room, ent, castle):
    _, tm = tilemap(rom, e041, room)
    names = bytearray(128) + bytes(sum(tm, []))
    overlays.apply(names, overlays.stamps(rom, e041, room, ent, castle))
    return names[128:]                       # 20 filas x 32 columnas de la sala


def main():
    rom = Rom()
    ent = json.loads((ROOT / 'extracted' / 'entities' / 'entities.json').read_text())
    castle = json.loads((ROOT / 'extracted' / 'maps' / 'castle.json').read_text())
    idx = json.loads((ROOT / 'extracted' / 'maps' / 'index.json').read_text())
    out = ROOT / 'extracted' / 'maps' / 'collision'
    out.mkdir(parents=True, exist_ok=True)
    totals = {}
    for name, info in idx.items():
        e041 = info['e041']
        n = info['table_rooms'] - (0 if e041 in (1, 11) else 1)
        boss_room = rom.byte(0x5DFB + e041, None) if e041 > 1 else None
        rooms = {}
        for room in range(1, n + 1):
            names = room_names(rom, e041, room, ent[name][str(room)], castle)
            rows = [''.join(tile_class(names[32 * y + x]) for x in range(32)) for y in range(20)]
            rooms[room] = {'rows': rows}
            if room == boss_room:
                rooms[room]['rows_boss_fight'] = [''.join(tile_class(names[32 * y + x], True) for x in range(32))
                                                  for y in range(20)]
            for ch in ''.join(rows):
                totals[ch] = totals.get(ch, 0) + 1
        (out / f'{name}.json').write_text(json.dumps(rooms))
    print('tiles por clase en las 320 salas:', totals)


if __name__ == '__main__':
    main()
