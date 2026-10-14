#!/usr/bin/env python3
"""Monta un mapa visual por mundo con las capturas de cada sala (requiere Pillow).

Coloca la zona de juego de cada captura (256x160, sin el marcador) en la celda que le da
extracted/maps/<mundo>.json. Las capturas salen de tools/openmsx/scan_rooms.py, por eso
estos PNG contienen graficos originales: son para uso local, no para redistribuir.

  stitch_maps.py [escala]     -> extracted/maps/<mundo>_visual.png
"""
import json, sys
from PIL import Image, ImageDraw
from galious_rom import ROOT

MAPS = ROOT / 'extracted' / 'maps'
SHOTS = ROOT / 'logs' / 'omsx' / 'rooms'
STATE = {'castle': 'galious_start', **{f'world{k:02d}': f'world{k:02d}_entry' for k in range(1, 11)}}
PLAY = (32, 56, 288, 216)   # zona de juego dentro de la captura 320x240
GAP = 4


def stitch(name, scale):
    w = json.loads((MAPS / f'{name}.json').read_text())
    cells = {tuple(v['pos']): int(r) for r, v in w['rooms'].items()}
    xs = [p[0] for p in cells]; ys = [p[1] for p in cells]
    cw, ch = int(256 * scale), int(160 * scale)
    W = (max(xs) - min(xs) + 1) * (cw + GAP) + GAP
    H = (max(ys) - min(ys) + 1) * (ch + GAP) + GAP
    img = Image.new('RGB', (W, H), (24, 24, 24))
    d = ImageDraw.Draw(img)
    missing = []
    for (x, y), r in cells.items():
        px = GAP + (x - min(xs)) * (cw + GAP)
        py = GAP + (y - min(ys)) * (ch + GAP)
        shot = SHOTS / f'{STATE[name]}_{r:03d}.png'
        if shot.exists():
            img.paste(Image.open(shot).convert('RGB').crop(PLAY).resize((cw, ch)), (px, py))
        else:
            missing.append(r)
            d.rectangle((px, py, px + cw, py + ch), outline=(120, 120, 120))
        d.rectangle((px, py, px + 22, py + 11), fill=(0, 0, 0))
        d.text((px + 2, py), str(r), fill=(255, 255, 0))
    img.save(MAPS / f'{name}_visual.png')
    return missing


if __name__ == '__main__':
    scale = float(sys.argv[1]) if len(sys.argv) > 1 else 0.5
    for name in STATE:
        if (MAPS / f'{name}.json').exists():
            miss = stitch(name, scale)
            print(name, 'sin captura:', miss if miss else 'ninguna')
