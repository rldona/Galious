#!/usr/bin/env python3
"""Extrae de la ROM lo que puebla cada sala: enemigos, objetos y graficos de sprites.

Tablas (todas indexadas por mundo, E041, con una lista por sala; se salta sala-1 listas):
  - tipos de sprite cargados en la sala: 9737 (bancos 10-12), bytes terminados en 0.
    Para cada tipo, 7C72[tipo] apunta a entradas (hueco, puntero RLE) que se descomprimen
    en la tabla de patrones de sprites 1800 + hueco*8 (b00:5960, 59A6).
  - generadores de enemigos: B5CB (bancos 4-6, rutina b04:6000). Entradas de 2 bytes:
    t = tipo | bit7, p = posicion: y = p & F8, x = ((p & 7) << 5) | (bit7 << 4).
    La lista acaba con un byte cuyo valor & 7F es 0.
  - objetos de sala: A7BC (bancos 7-9, rutina b08:9D00). Entradas de 2 bytes:
    tipo, p: y = p & E0, x = (p & 1F) * 8; despacho por (tipo & 0F) a A013, A10E, ...
  - objeto recogible (uno por sala como mucho): B6FC (banco 9, b00:5DC9). Entradas de 3 bytes
    (sala, b1, b2): tipo = b1 & 3F, y = b2 & F8, x = ((b2 & 7) << 5) | (b1 bit7 << 4) | (b1 bit6 << 3).
    Si ya se recogio (E06F + tipo) no aparece. En RAM: E740 tipo, E742 y, E743 x.
  - piedras y escaleras extra: ver overlays.py.
Coordenadas en pixeles de pantalla (y incluye las 32 lineas del marcador).

Salida: extracted/entities/entities.json y sprites_<tipo>.png (catalogo de graficos).
"""
import json
from galious_rom import Rom, ROOT
from render_rooms import png
import overlays

SPRITE_TYPES, SPRITE_GFX = 0x9737, 0x7C72
SPAWNS, OBJECTS, ITEMS = 0xB5CB, 0xA7BC, 0xB6FC
OUT = ROOT / 'extracted' / 'entities'


def _room_ptr(rom, table, group, e041, room, size, end):
    p = rom.word(table + 2 * e041, group)
    for _ in range(room - 1):
        while not end(rom.byte(p, group)):
            p += size
        p += 1
    return p


def room_list(rom, table, group, e041, room, size, end):
    p = _room_ptr(rom, table, group, e041, room, size, end)
    out = []
    while not end(rom.byte(p, group)):
        out.append(tuple(rom.block(p, size, group)))
        p += size
    return out


def sprite_types(rom, e041, room):
    return [t for (t,) in room_list(rom, SPRITE_TYPES, 3, e041, room, 1, lambda b: b == 0)]


def spawns(rom, e041, room):
    out = []
    for t, p in room_list(rom, SPAWNS, 1, e041, room, 2, lambda b: b & 0x7F == 0):
        out.append({'type': t & 0x7F, 'x': ((p & 7) << 5) | ((t >> 7) << 4), 'y': p & 0xF8})
    return out


def objects(rom, e041, room):
    return [{'type': t, 'x': (p & 0x1F) * 8, 'y': p & 0xE0}
            for t, p in room_list(rom, OBJECTS, 2, e041, room, 2, lambda b: b == 0)]


def item(rom, e041, room):
    p = rom.word(ITEMS + 2 * e041, 2)
    while (r := rom.byte(p, 2)):
        if r == room:
            b1, b2 = rom.byte(p + 1, 2), rom.byte(p + 2, 2)
            return {'type': b1 & 0x3F, 'y': b2 & 0xF8,
                    'x': ((b2 & 7) << 5) | ((b1 >> 7) << 4) | (((b1 >> 6) & 1) << 3)}
        p += 3
    return None


def sprite_slots(rom, typ):
    """Huecos de patron de sprite (8x8) que carga un tipo, con sus bytes."""
    vram = bytearray(0x800)
    p = rom.word(SPRITE_GFX + 2 * typ, 3)
    slots = []
    while (slot := rom.byte(p, 3)):
        ptr = rom.word(p + 1, 3)
        dst = slot * 8
        start = dst
        while True:                      # RLE (b00:5C46) hacia la tabla de sprites
            n = rom.byte(ptr, 3); ptr += 1
            if n == 0:
                break
            if n == 0x80:
                ptr += 2
                break
            if n & 0x80:
                for _ in range(n & 0x7F):
                    vram[dst & 0x7FF] = rom.byte(ptr, 3); ptr += 1; dst += 1
            else:
                b = rom.byte(ptr, 3); ptr += 1
                for _ in range(n):
                    vram[dst & 0x7FF] = b; dst += 1
        slots.append((start // 8, (dst - start) // 8))
        p += 3
    return vram, slots


def sprite_sheet(rom, typ, path):
    """Pinta en blanco los sprites 16x16 que carga el tipo (4 patrones: arriba-izq, abajo-izq, arriba-der, abajo-der)."""
    vram, slots = sprite_slots(rom, typ)
    first = min(s for s, _ in slots)
    last = max(s + n for s, n in slots)
    first -= first % 4
    sprites = list(range(first, last, 4))
    W = max(1, len(sprites)) * 18
    rows = [[(32, 32, 32)] * W for _ in range(16)]
    for i, s in enumerate(sprites):
        for q, (dx, dy) in enumerate(((0, 0), (0, 8), (8, 0), (8, 8))):
            for y in range(8):
                b = vram[(s + q) * 8 + y]
                for x in range(8):
                    if b & (0x80 >> x):
                        rows[dy + y][i * 18 + dx + x] = (255, 255, 255)
    png(path, W, 16, rows)
    return slots


def main():
    rom = Rom()
    OUT.mkdir(parents=True, exist_ok=True)
    idx = json.loads((ROOT / 'extracted' / 'maps' / 'index.json').read_text())
    result, all_types = {}, set()
    for name, info in idx.items():
        e041 = info['e041']
        rooms = {}
        for room in range(1, info['table_rooms'] + 1):
            st, sp, ob = sprite_types(rom, e041, room), spawns(rom, e041, room), objects(rom, e041, room)
            all_types.update(st)
            rooms[room] = {'sprite_types': st, 'spawns': sp, 'objects': ob, 'item': item(rom, e041, room),
                           'rocks': overlays.rocks(rom, e041, room), 'ladders': overlays.ladders(rom, e041, room)}
        result[name] = rooms
        n_sp = sum(len(r['spawns']) for r in rooms.values())
        n_ob = sum(len(r['objects']) for r in rooms.values())
        n_it = sum(1 for r in rooms.values() if r['item'])
        n_ro = sum(len(r['rocks']) for r in rooms.values())
        print(f'{name:8s} generadores={n_sp:3d} objetos={n_ob:3d} recogibles={n_it:3d} piedras={n_ro:3d}')
    (OUT / 'entities.json').write_text(json.dumps(result))
    catalog = {}
    for t in sorted(all_types):
        catalog[t] = sprite_sheet(rom, t, OUT / f'sprites_{t:02x}.png')
    (OUT / 'sprite_slots.json').write_text(json.dumps({hex(k): v for k, v in catalog.items()}))
    print('tipos de sprite:', ' '.join(f'{t:02x}' for t in sorted(all_types)))


if __name__ == '__main__':
    main()
