#!/usr/bin/env python3
"""Renderiza todas las salas desde la ROM: disposicion (bancos 7-9) + juego de tiles (bancos 10-12).

  render_rooms.py              -> extracted/rooms/:
                                    <mundo>/<sala>.png   cada sala (256x160)
                                    <mundo>_map.png      el mundo entero a mitad de escala
                                    tiles_<juego>.png    juegos de tiles (16x16 tiles)
                                    rooms.json           disposicion de cada sala (8x5 bloques)
                                    blocks.json          bloques: 4x4 tiles cada uno
                                    <mundo>_entities.png el mundo con enemigos (primer sprite de cada
                                                         generador, en amarillo) y objetos recogibles
                                                         (recuadro magenta)
  render_rooms.py --compare    -> ademas compara cada sala con su captura del emulador (Pillow)

Incluye lo que el juego superpone a la sala (overlays.py): puertas, piedras, escaleras extra,
estatuas, objetos, cofres, pasadizos y el emblema del jefe, en el estado de una partida nueva.
Limitaciones: no anima los tiles (agua: 0xB6, 0xB8), no dibuja los sprites, ni los pasadizos que
abren eventos programados (b01:735F), ni la ultima sala de cada mundo (sala de dios, estado 14).

Sin dependencias: el PNG se escribe a mano. Las imagenes contienen graficos de Konami:
solo para uso local.
"""
import json, struct, sys, zlib
from galious_rom import Rom, ROOT
from tilesets import castle_zone, tileset
import overlays

LAYOUT_GROUP = 2                 # bancos 7, 8, 9
LAYOUT_TABLE, BLOCKS = 0x6ABE, 0x6000
SPECIAL = {(6, 6): 0x8DAE}       # (E041, sala) con disposicion fija (b00:55FD)
# Paleta por defecto del MSX2 (la BIOS la carga imitando al TMS9918), 3 bits por canal, con los
# niveles que usa openMSX. El color 0 es transparente: se pinta negro.
_L = (0, 39, 82, 115, 155, 186, 224, 255)
PALETTE = [tuple(_L[v] for v in rgb) for rgb in (
    (0, 0, 0), (0, 0, 0), (1, 6, 1), (3, 7, 3), (1, 1, 7), (2, 3, 7), (5, 1, 1), (2, 6, 7),
    (7, 1, 1), (7, 3, 3), (6, 6, 1), (6, 6, 4), (1, 4, 1), (6, 2, 5), (5, 5, 5), (7, 7, 7))]
OUT = ROOT / 'extracted' / 'rooms'


def png(path, w, h, rows):
    raw = b''.join(b'\0' + bytes(c for px in row for c in px) for row in rows)
    chunk = lambda t, d: struct.pack('>I', len(d)) + t + d + struct.pack('>I', zlib.crc32(t + d))
    path.write_bytes(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 2, 0, 0, 0))
                     + chunk(b'IDAT', zlib.compress(raw, 9)) + chunk(b'IEND', b''))


def tilemap(rom, e041, room):
    lay_ptr = SPECIAL.get((e041, room)) or rom.word(LAYOUT_TABLE + 2 * e041, LAYOUT_GROUP) + 40 * (room - 1)
    lay = rom.block(lay_ptr, 40, LAYOUT_GROUP)
    tm = [[0] * 32 for _ in range(20)]
    for i, blk in enumerate(lay):
        tiles = rom.block(BLOCKS + 16 * blk, 16, LAYOUT_GROUP)
        for j, t in enumerate(tiles):
            tm[(i // 8) * 4 + j // 4][(i % 8) * 4 + j % 4] = t
    return lay, tm


def render(tm, pats, cols):
    """Devuelve 160 filas de 256 indices de color."""
    img = []
    for ty in range(20):
        for y in range(8):
            row = []
            for tx in range(32):
                t = tm[ty][tx]
                p, c = pats[t * 8 + y], cols[t * 8 + y]
                fg, bg = c >> 4, c & 15
                row += [fg if p & (0x80 >> x) else bg for x in range(8)]
            img.append(row)
    return img


def nearest(rgb):
    return min(range(1, 16), key=lambda i: sum((a - b) ** 2 for a, b in zip(PALETTE[i], rgb)))


def compare(img, shot_path):
    """Fraccion de pixeles iguales contra la zona de juego de la captura (necesita Pillow)."""
    from PIL import Image
    im = Image.open(shot_path).convert('RGB').crop((32, 56, 288, 216))
    px = im.load()
    cache, same = {}, 0
    for y in range(160):
        for x in range(256):
            rgb = px[x, y]
            if rgb not in cache:
                cache[rgb] = nearest(rgb)
            same += cache[rgb] == (img[y][x] or 1)
    return same / (256 * 160)


def tileset_png(path, pats, cols):
    rows = []
    for ty in range(16):
        for y in range(8):
            row = []
            for tx in range(16):
                t = ty * 16 + tx
                p, c = pats[t * 8 + y], cols[t * 8 + y]
                row += [PALETTE[c >> 4] if p & (0x80 >> x) else PALETTE[c & 15] for x in range(8)]
            rows.append(row)
    png(path, 128, 128, rows)


def world_map(path, rooms_pos, images, rgb=False):
    """Compone el mundo a mitad de escala (128x80 por sala) con un borde de 2 px."""
    xs = [p[0] for p in rooms_pos.values()]; ys = [p[1] for p in rooms_pos.values()]
    cw, ch, gap = 128, 80, 2
    W = (max(xs) - min(xs) + 1) * (cw + gap) + gap
    H = (max(ys) - min(ys) + 1) * (ch + gap) + gap
    canvas = [[(24, 24, 24)] * W for _ in range(H)]
    for room, (x, y) in rooms_pos.items():
        img = images.get(room)
        if not img:
            continue
        ox, oy = gap + (x - min(xs)) * (cw + gap), gap + (y - min(ys)) * (ch + gap)
        for yy in range(ch):
            src = img[yy * 2]
            canvas[oy + yy][ox:ox + cw] = [src[xx * 2] if rgb else PALETTE[src[xx * 2]] for xx in range(cw)]
    png(path, W, H, canvas)


def annotate(img, ent, rom, icons):
    """Copia de la sala (indices de color) con los generadores y el objeto marcados."""
    from entities import sprite_slots
    out = [[PALETTE[c] for c in row] for row in img]

    def put(x, y, rgb):
        if 0 <= x < 256 and 0 <= y < 160:
            out[y][x] = rgb
    for s in ent['spawns']:
        t = s['type']
        if t not in icons:
            try:
                vram, slots = sprite_slots(rom, t)
                first = min(a for a, _ in slots) // 4 * 4
                icons[t] = [[bool(vram[(first + q) * 8 + y] & (0x80 >> x)) for q, x in
                             ((0, xx) if xx < 8 else (2, xx - 8) for xx in range(16))] for y in range(8)] + \
                           [[bool(vram[(first + q) * 8 + y] & (0x80 >> x)) for q, x in
                             ((1, xx) if xx < 8 else (3, xx - 8) for xx in range(16))] for y in range(8)]
            except Exception:
                icons[t] = None
        x0, y0 = s['x'], s['y'] - 32
        if icons[t]:
            for y in range(16):
                for x in range(16):
                    if icons[t][y][x]:
                        put(x0 + x, y0 + y, (255, 230, 0))
        else:
            for k in range(16):
                put(x0 + k, y0, (255, 230, 0)); put(x0 + k, y0 + 15, (255, 230, 0))
    it = ent.get('item')
    if it:
        x0, y0 = it['x'], it['y'] - 32
        for k in range(16):
            for a, b in ((x0 + k, y0), (x0 + k, y0 + 15), (x0, y0 + k), (x0 + 15, y0 + k)):
                put(a, b, (255, 0, 255))
    return out


def main(do_compare):
    rom = Rom()
    idx = json.loads((ROOT / 'extracted' / 'maps' / 'index.json').read_text())
    state = {'castle': 'galious_start', **{f'world{k:02d}': f'world{k:02d}_entry' for k in range(1, 11)}}
    report = {}
    layouts = {}
    ent = json.loads((ROOT / 'extracted' / 'entities' / 'entities.json').read_text())
    castle = json.loads((ROOT / 'extracted' / 'maps' / 'castle.json').read_text())
    for name, info in idx.items():
        e041 = info['e041']
        n = info['table_rooms'] - (0 if e041 in (1, 11) else 1)   # la ultima sala de un mundo es especial
        d = OUT / name
        d.mkdir(parents=True, exist_ok=True)
        cache = {}
        scores = []
        images, annotated, icons = {}, {}, {}
        for room in range(1, n + 1):
            if e041 == 1:
                key = f'castle_zone{castle_zone(rom, room)}'
            else:
                key = 'worlds'
            if key not in cache:
                cache[key] = tileset(rom, e041, room)
                tileset_png(OUT / f'tiles_{key}.png', *cache[key])
            ts = cache[key]
            lay, tm = tilemap(rom, e041, room)
            layouts.setdefault(name, {})[room] = {'tileset': key, 'blocks': list(lay)}
            names = bytearray(128) + bytes(sum(tm, []))
            overlays.apply(names, overlays.stamps(rom, e041, room, ent[name][str(room)], castle))
            tm = [list(names[128 + 32 * y:160 + 32 * y]) for y in range(20)]
            img = render(tm, *ts)
            images[room] = img
            annotated[room] = annotate(img, ent[name][str(room)], rom, icons)
            png(d / f'{room:03d}.png', 256, 160, [[PALETTE[c] for c in row] for row in img])
            if do_compare:
                shot = ROOT / 'logs' / 'omsx' / 'rooms' / f'{state[name]}_{room:03d}.png'
                if shot.exists():
                    scores.append((room, round(compare(img, shot), 3)))
        pos = {int(r): tuple(v['pos']) for r, v in
               json.loads((ROOT / 'extracted' / 'maps' / f'{name}.json').read_text())['rooms'].items()}
        world_map(OUT / f'{name}_map.png', pos, images)
        world_map(OUT / f'{name}_entities.png', pos, annotated, rgb=True)
        if scores:
            worst = sorted(scores, key=lambda s: s[1])[:5]
            avg = sum(s for _, s in scores) / len(scores)
            report[name] = {'rooms': len(scores), 'mean': round(avg, 3), 'worst': worst}
            print(f'{name:8s} salas={len(scores)} parecido medio={avg:.3f} peores={worst}', flush=True)
        else:
            print(f'{name:8s} {n} salas renderizadas', flush=True)
    (OUT / 'rooms.json').write_text(json.dumps(layouts))
    (OUT / 'blocks.json').write_text(json.dumps(
        [list(rom.block(BLOCKS + 16 * b, 16, LAYOUT_GROUP)) for b in range(256)]))
    if report:
        (OUT / 'compare.json').write_text(json.dumps(report, indent=1))


if __name__ == '__main__':
    main('--compare' in sys.argv)
