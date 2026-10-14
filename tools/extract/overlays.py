#!/usr/bin/env python3
"""Lo que el juego dibuja encima de la disposicion de bloques al cargar una sala (b01:71D9).

Fuentes en la ROM (coordenadas de pantalla en pixeles; fila de tile = y/8, con el marcador):
  - piedras (solo castillo): B8A2 (banco 9), 1 byte por piedra: y = (b & E0) | 8,
    x = (b & 1F) * 8; sello 2x2 BA BB / BC BD. Cada una tiene un bit persistente (rota o no).
  - escaleras extra: A6EB (banco 9), pares (mundo, sala); datos de 5 bytes en A705 + 5*k:
    y, x, alto, largo en pixeles, modo (1 se retira al subir, 2 al bajar, 3 siempre; b03:A608);
    columna de alto filas con 9A 9B.
  - puertas de mundo (castillo): ver world_maps.py; sello 4x3.
  - estatuas: generadores 0x50/0x51 (entities.py); sello 2x3.
  - objetos de sala A7BC (entities.py): columna 1x3 o hueco, segun tipo.
  - salas de dioses: el cartel 2x2 (BA..BD o, para Demeter-contrasena, 8D 93 / 8E 94).
  - eventos de sala (room_events.py): bloques de muro 4x3 que siguen cerrados al empezar.

  overlays.py --check   compara el mapa de nombres previsto con la VRAM real (logs/omsx/vram_*.tsv)
"""
import json, sys
from galious_rom import Rom, ROOT

ROCKS, LADDER_ROOMS, LADDER_DATA = 0xB8A2, 0xA6EB, 0xA705

DOOR = ((0x5A, 0x5A, 0x5A, 0x5A), (0x5A, 0x5B, 0x5B, 0x5A), (0x5A, 0x5B, 0x5B, 0x5A))
ROCK = ((0xBA, 0xBB), (0xBC, 0xBD))
SIGN_DEMETER = ((0x8D, 0x93), (0x8E, 0x94))
# Generadores que se dibujan con tiles (estatuas, etc.); sellos medidos en la VRAM
SPAWN_TILES = {0x51: ((0xF1, 0xF4), (0xF2, 0xF5), (0xF3, 0xF6)),      # estatua mirando a la derecha
               0x32: ((0xFA, 0xF7), (0xFB, 0xF8), (0xFC, 0xF9)),      # estatua que dispara flechas
               0x50: ((0xF1, 0xF2), (0xF5, 0xF3), (0xF0, 0xF0)),
               0x3A: ((0xF8, 0xF7), (0xF9, 0xFB)),
               0x04: ((0x99,),)}
HOLE = tuple((0xF0,) * 4 for _ in range(3))                          # bloque de muro 4x3 (F0 = ladrillo)
EMBLEMS = 0x9BE2      # banco 8: puntero por mundo (2*E041) a un emblema de 8x8 tiles (b00:55CE)
ITEM_BOX = ((0x76, 0x77), (0x78, 0x79))
OBJECTS = {1: ((0xC5,), (0xC5,), (0xC5,)), 2: ((0xC6,), (0xC7,), (0xC8,)),
           3: ((0xCF,), (0xCF,), (0xCF,)), 4: ((0xD0,), (0xD1,), (0xD2,)),
           0x13: ((0xCF,), (0xCF,), (0xCF,)), 0x07: HOLE, 0x17: HOLE, 0x27: HOLE, 0x37: HOLE}


def rocks(rom, e041, room):
    if e041 != 1:
        return []
    p = ROCKS
    for _ in range(room - 1):
        while rom.byte(p, 2):
            p += 1
        p += 1
    out = []
    while (b := rom.byte(p, 2)):
        out.append({'x': (b & 0x1F) * 8, 'y': (b & 0xE0) | 8})
        p += 1
    return out


def ladders(rom, e041, room):
    out = []
    for k in range(13):          # 13 entradas; lo que sigue ya no es la tabla
        w, r = rom.byte(LADDER_ROOMS + 2 * k, 2), rom.byte(LADDER_ROOMS + 2 * k + 1, 2)
        if (w, r) == (e041, room):
            y, x, h, a, b = rom.block(LADDER_DATA + 5 * k, 5, 2)
            out.append({'x': x, 'y': y, 'height': h, 'flags': [a, b]})
    return out


def stamps(rom, e041, room, ent, castle):
    """Lista de (columna, fila, sello) a pintar sobre el mapa de nombres de 32x24."""
    out = []
    # el objeto recogible va debajo: en el castillo suele estar tapado por una piedra
    it = ent.get('item')
    if it and it['type'] >= 7:      # b01:6C56: 7..2C cofre generico; >= 2D tiles propios
        t = it['type']
        stamp = ITEM_BOX if t < 0x2D else (((t - 0x2D) * 4 + 0x60, (t - 0x2D) * 4 + 0x61),
                                          ((t - 0x2D) * 4 + 0x62, (t - 0x2D) * 4 + 0x63))
        out.append((it['x'] // 8, it['y'] // 8, stamp))
    elif it and 1 <= it['type'] <= 4:   # armas secundarias: un destello (tile 4E) una fila mas abajo
        out.append((it['x'] // 8, it['y'] // 8 + 1, ((0x4E, 0x00),)))
    for r in rocks(rom, e041, room):
        out.append((r['x'] // 8, r['y'] // 8, ROCK))
    for l in ladders(rom, e041, room):
        out.append((l['x'] // 8, l['y'] // 8, tuple((0x9A, 0x9B) for _ in range(l['height']))))
    if e041 == 1:
        d = castle['doors'].get(str(room))
        if d:
            out.append((d['x'] // 8, d['y'] // 8, DOOR))
        g = castle['gods'].get(str(room))
        if g:
            out.append((g['x'] // 8, g['y'] // 8, SIGN_DEMETER if g['type'] == 1 else ROCK))
    for s in ent['spawns']:
        if s['type'] in SPAWN_TILES:
            out.append((s['x'] // 8, s['y'] // 8, SPAWN_TILES[s['type']]))
    if e041 > 1 and room == rom.byte(0x5DFB + e041):   # sala del jefe sin derrotar (E615 = 1)
        ptr = rom.word(EMBLEMS + 2 * e041, 2)
        emblem = tuple(tuple(rom.block(ptr + 8 * j, 8, 2)) for j in range(8))
        out.append((12, 10 if e041 == 7 else 8, emblem))
    for o in ent['objects']:
        if o['type'] in OBJECTS:
            out.append((o['x'] // 8, o['y'] // 8, OBJECTS[o['type']]))
    world = 'castle' if e041 == 1 else f'world{e041 - 1:02d}'
    for ev in room_events()['on_load']:     # estado inicial: el evento todavia no ha ocurrido
        if ev['kind'] == 'muro 4x3' and (ev['world'], ev['room']) == (world, room):
            out.append((ev['pos']['col'], ev['pos']['row'] + 4, HOLE))
    return out


_EVENTS = None


def room_events():
    global _EVENTS
    if _EVENTS is None:
        p = ROOT / 'extracted' / 'entities' / 'room_events.json'
        _EVENTS = json.loads(p.read_text()) if p.exists() else {'on_load': []}
    return _EVENTS


def apply(names, st):
    for cx, cy, stamp in st:
        for j, row in enumerate(stamp):
            for i, t in enumerate(row):
                if 0 <= cx + i < 32 and 0 <= cy + j < 24:
                    names[(cy + j) * 32 + cx + i] = t
    return names


def check():
    from render_rooms import tilemap
    rom = Rom()
    idx = json.loads((ROOT / 'extracted' / 'maps' / 'index.json').read_text())
    ent = json.loads((ROOT / 'extracted' / 'entities' / 'entities.json').read_text())
    castle = json.loads((ROOT / 'extracted' / 'maps' / 'castle.json').read_text())
    state = {'castle': 'galious_start', **{f'world{k:02d}': f'world{k:02d}_entry' for k in range(1, 11)}}
    total = exact = 0
    for name, info in idx.items():
        e041 = info['e041']
        n = info['table_rooms'] - (0 if e041 in (1, 11) else 1)
        bad = []
        for line in open(ROOT / 'logs' / 'omsx' / f'vram_{state[name]}.tsv'):
            room, _, cur, nt, _ = line.rstrip('\n').split('\t')
            room = int(room)
            if int(cur) != room or room > n:
                continue
            nt = bytes.fromhex(nt)
            _, tm = tilemap(rom, e041, room)
            names = bytearray(nt[:128]) + bytes(sum(tm, []))
            apply(names, stamps(rom, e041, room, ent[name][str(room)], castle))
            diff = sum(a != b for a, b in zip(names[128:], nt[128:]))
            total += 1
            exact += diff == 0
            if diff:
                bad.append((room, diff))
        print(f'{name:8s} salas exactas {n - len(bad)}/{n}  con diferencias: {bad[:12]}')
    print(f'total: {exact}/{total} salas con el mapa de nombres identico a la VRAM')


if __name__ == '__main__':
    if '--check' in sys.argv:
        check()
