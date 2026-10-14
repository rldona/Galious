#!/usr/bin/env python3
"""Dibujo de los jefes para el importador de Unity, sacado de la VRAM del combate (tools/openmsx/boss_vram.py).

  boss_gfx.py   -> extracted/unity/gfx/boss_w<k>.png y bosses_gfx.json   (lo llama unity_bundle.py)

El jefe se dibuja con tiles del fondo (docs/boss_ai.md, "Cómo se dibuja el jefe"). Sus tiles son las celdas de la tabla
de nombres que cambian respecto a la sala normal (render_rooms.tilemap) en la zona de juego (filas 4-23); el resto de la
sala, en el combate, tiene la decoración en negro. Se guarda el rectángulo que las contiene, con las demás celdas
transparentes, y su esquina respecto a la posición lógica del jefe ajustada a la rejilla de 8 px (E707 & ~7,
E705 & ~7), como se dibuja en el original. Solo cuenta el grupo de celdas
unidas más cercano al jefe.
[I] Un solo fotograma por jefe (el del volcado); el original cambia de nombres de tile con cada fotograma.
bosses_gfx.json: {"bosses": [{"world", "file", "dx", "dy", "w", "h"}]} en píxeles.
"""
import json
from galious_rom import Rom, ROOT
from render_rooms import tilemap, PALETTE
from ending_gfx import png_rgba, tables

SRC = ROOT / 'logs' / 'omsx' / 'bosses'
OUT = ROOT / 'extracted' / 'unity' / 'gfx'


def cluster(cells, c0, r0):
    """Las celdas unidas (8 vecinos) al grupo más cercano a la posición del jefe: fuera quedan cambios sueltos."""
    left, groups = set(cells), []
    while left:
        stack, g = [left.pop()], set()
        while stack:
            r, c = stack.pop()
            g.add((r, c))
            for dr in (-1, 0, 1):
                for dc in (-1, 0, 1):
                    if (r + dr, c + dc) in left:
                        left.remove((r + dr, c + dc))
                        stack.append((r + dr, c + dc))
        groups.append(g)
    near = lambda g: min(abs(r - r0) + abs(c - c0) for r, c in g)
    return sorted(min(groups, key=lambda g: (near(g), -len(g)))) if groups else []


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    rom = Rom()
    out = []
    bj = json.loads((ROOT / 'extracted' / 'unity' / 'bosses.json').read_text())
    sizes = {b['world']: (b['sprites'][0]['w'], b['sprites'][0]['h']) for b in bj['bosses'] if b.get('sprites')}
    for k in range(1, 11):
        p = SRC / f'vram_w{k}.json'
        if not p.exists():
            continue
        d = json.loads(p.read_text())
        vram, regs = bytes.fromhex(d['vram']), d['regs']
        names, pat, col = tables(regs)
        _, tm = tilemap(rom, k + 1, d['E042'])
        cells = [(r, c) for r in range(20) for c in range(32) if vram[names + (r + 4) * 32 + c] != tm[r][c]
                 and vram[names + (r + 4) * 32 + c] < 0xF0]
        # solo dentro del primer fotograma del jefe (bosses.json): en el mundo 3 los suelos de la sala también cambian
        size = sizes.get(k)
        if size:
            x0, y0 = d['E707'] // 8, (d['E705'] - 32) // 8
            cells = [(r, c) for r, c in cells if x0 <= c < x0 + (size[0] + 7) // 8 and y0 <= r < y0 + (size[1] + 7) // 8]
        cells = cluster(cells, d['E707'] // 8, (d['E705'] - 32) // 8)
        if not cells:
            continue
        r0, r1 = min(r for r, _ in cells), max(r for r, _ in cells)
        c0, c1 = min(c for _, c in cells), max(c for _, c in cells)
        w, h = (c1 - c0 + 1) * 8, (r1 - r0 + 1) * 8
        px = [(0, 0, 0, 0)] * (w * h)
        for r, c in cells:
            n, third = vram[names + (r + 4) * 32 + c], (r + 4) // 8
            for y in range(8):
                bits = vram[pat + third * 0x800 + n * 8 + y]
                colr = vram[col + third * 0x800 + n * 8 + y]
                for x in range(8):
                    k2 = colr >> 4 if bits & (0x80 >> x) else colr & 15
                    if k2:
                        px[((r - r0) * 8 + y) * w + (c - c0) * 8 + x] = PALETTE[k2] + (255,)
        name = f'boss_w{k}.png'
        png_rgba(OUT / name, w, h, px)
        out.append({'world': k, 'file': name[:-4], 'dx': c0 * 8 - (d['E707'] & ~7), 'dy': 32 + r0 * 8 - (d['E705'] & ~7), 'w': w, 'h': h})
    (OUT / 'bosses_gfx.json').write_text(json.dumps({'bosses': out}, indent=1))
    print(f'jefes: {len(out)} dibujos -> {OUT}')


if __name__ == '__main__':
    main()
