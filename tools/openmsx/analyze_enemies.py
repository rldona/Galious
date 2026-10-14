#!/usr/bin/env python3
"""Analiza las grabaciones de trace_enemies.py y resume el movimiento de cada tipo.

Sigue cada hueco de RAM (+0 tipo, +5 Y, +7 X); un salto de mas de 24 px o un cambio de tipo
cuenta como un enemigo nuevo. Metricas por tipo (agregadas sobre sus instancias):
  vx/vy medias cuando se mueve (px/frame), giros por segundo, patron vertical,
  'acerca': fraccion de frames en que se mueve hacia el jugador,
  hijos: tipos que aparecen en huecos nuevos cerca de el (proyectiles, objetos soltados).
Salida: logs/omsx/enemies/summary.json y <tipo>_paths.png (trayectorias sobre la sala).
"""
import gzip, json, os, sys
from collections import Counter, defaultdict
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'extract'))
from galious_rom import Rom, ROOT
from render_rooms import tilemap, render, PALETTE, png
from tilesets import tileset
import overlays

DIR = ROOT / 'logs' / 'omsx' / 'enemies'
COLORS = [(255, 230, 0), (0, 255, 255), (255, 80, 255), (120, 255, 120), (255, 140, 0), (255, 255, 255)]


def tracks(frames):
    """Devuelve lista de (tipo, [(f, x, y)]) por instancia."""
    out, cur = [], {}
    for f, (_, _, slots) in enumerate(frames):
        for i, s in enumerate(slots):
            t, y, x = int(s[0:2], 16), int(s[10:12], 16), int(s[14:16], 16)
            prev = cur.get(i)
            if t == 0:
                if prev:
                    out.append(prev); cur.pop(i)
                continue
            if prev and (prev[0] != t or abs(prev[1][-1][1] - x) > 24 or abs(prev[1][-1][2] - y) > 24):
                out.append(prev); prev = None
            if not prev:
                prev = cur[i] = (t, [], f)
            prev[1].append((f, x, y))
    out += cur.values()
    return out


def sign(v):
    return (v > 0) - (v < 0)


def stats(trs, frames, t):
    mine = [tr for tr in trs if tr[0] == t and len(tr[1]) > 10]
    if not mine:
        return None
    vx, vy, moving, toward, turns, n, ys, ddy_pos = [], [], 0, 0, 0, 0, [], 0
    for _, pts, _ in mine:
        last = 0
        for (f0, x0, y0), (f1, x1, y1) in zip(pts, pts[1:]):
            dx, dy = x1 - x0, y1 - y0
            n += 1
            if dx or dy:
                moving += 1
                vx.append(abs(dx)); vy.append(abs(dy))
                px, py = frames[f1][0], frames[f1][1]
                toward += (dx and sign(dx) == sign(px - x0)) or (not dx and dy and sign(dy) == sign(py - y0))
            if dx:
                if last and sign(dx) != last:
                    turns += 1
                last = sign(dx)
        yv = [p[2] for p in pts]
        ys.append(max(yv) - min(yv))
        dys = [b[2] - a[2] for a, b in zip(pts, pts[1:])]
        ddy_pos += sum(1 for a, b in zip(dys, dys[1:]) if b > a > 0)
    yr = max(ys)
    mean = lambda v: round(sum(v) / len(v), 2) if v else 0
    sx, sy = round(sum(vx) / max(n, 1), 2), round(sum(vy) / max(n, 1), 2)   # px/frame en media
    fdy = sum(1 for v in vy if v) / max(n, 1)                               # frames con movimiento vertical
    if moving / max(n, 1) < 0.05:
        pattern = 'quieto'
    elif fdy < 0.1 and sx > 0.05:
        pattern = 'camina' + (' (cae entre plataformas)' if yr > 8 else '')
    elif sx < 0.05:
        pattern = 'vertical'
    elif ddy_pos > 3 and fdy < 0.6:
        pattern = 'salta (aceleracion vertical)'
    else:
        pattern = 'vuela'
    return {'instances': len(mine), 'frames': n, 'moving': round(moving / max(n, 1), 2),
            'speed_x': sx, 'speed_y': sy, 'step_x': mean(vx), 'y_range': yr, 'frames_with_dy': round(fdy, 2),
            'turns_per_s': round(turns / max(n, 1) * 60, 2),
            'toward_player': round(toward / max(moving, 1), 2), 'pattern': pattern}


def children(trs, t):
    """Tipos que nacen (no en el frame 0) a menos de 24 px de una instancia del tipo t."""
    out = Counter()
    mine = [tr for tr in trs if tr[0] == t]
    for ct, pts, f0 in trs:
        if ct == t or f0 == 0:
            continue
        for _, mp, _ in mine:
            near = [p for p in mp if p[0] == f0 or p[0] == f0 - 1]
            if near and abs(near[0][1] - pts[0][1]) < 24 and abs(near[0][2] - pts[0][2]) < 24:
                out[ct] += 1
                break
    return dict(out)


def paths_png(rom, data, trs, ent, castle, path):
    e041 = 1 if data['world'] == 'castle' else int(data['world'][5:]) + 1
    room = data['room']
    _, tm = tilemap(rom, e041, room)
    names = bytearray(128) + bytes(sum(tm, []))
    overlays.apply(names, overlays.stamps(rom, e041, room, ent[data['world']][str(room)], castle))
    tm = [list(names[128 + 32 * y:160 + 32 * y]) for y in range(20)]
    img = [[tuple(c // 3 for c in PALETTE[i]) for i in row] for row in render(tm, *tileset(rom, e041, room))]
    kinds = {}
    for t, pts, _ in trs:
        col = kinds.setdefault(t, COLORS[len(kinds) % len(COLORS)])
        for _, x, y in pts:
            for dx in (6, 7, 8, 9):
                for dy in (6, 7, 8, 9):
                    yy, xx = y - 32 + dy, x + dx
                    if 0 <= yy < 160 and 0 <= xx < 256:
                        img[yy][xx] = col
    png(path, 256, 160, img)
    return {f'{t:#04x}': c for t, c in kinds.items()}


def main():
    rom = Rom()
    ent = json.loads((ROOT / 'extracted' / 'entities' / 'entities.json').read_text())
    castle = json.loads((ROOT / 'extracted' / 'maps' / 'castle.json').read_text())
    summary = {}
    for fn in sorted(os.listdir(DIR)):
        if not fn.endswith('.json.gz'):
            continue
        with gzip.open(DIR / fn, 'rt') as f:
            data = json.load(f)
        t, frames = data['type'], data['frames']
        trs = tracks(frames)
        st = stats(trs, frames, t)
        tag = f'{t:02x}' + ('_near' if data.get('near') else '')
        legend = paths_png(rom, data, trs, ent, castle, DIR / f'{tag}_paths.png')
        summary[('near ' if data.get('near') else '') + f'{t:#04x}'] = {'room': f"{data['world']}:{data['room']}", 'movement': st,
                                'children': {f'{k:#04x}': v for k, v in children(trs, t).items()},
                                'path_colors': legend}
        m = st or {}
        t_label = tag
        print(f"{t_label:7s} {data['world']}:{data['room']:<4} {m.get('pattern','(no se mueve o no aparece)'):34s} "
              f"vel_x={m.get('speed_x')} vel_y={m.get('speed_y')} giros/s={m.get('turns_per_s')} hacia_jugador={m.get('toward_player')} "
              f"hijos={summary[('near ' if data.get('near') else '') + f'{t:#04x}']['children']}")
    (DIR / 'summary.json').write_text(json.dumps(summary, indent=1))


if __name__ == '__main__':
    main()
