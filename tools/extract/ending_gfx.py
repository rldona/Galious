#!/usr/bin/env python3
"""Gráficos del final para el importador de Unity, sacados de la VRAM (tools/openmsx/ending_vram.py).

  ending_gfx.py   -> extracted/unity/gfx/ending_castle.png y ending_font.png   (lo llama unity_bundle.py)

- ending_castle.png (256 × 192): la pantalla de la escena del castillo (estado 17, sub 5) con solo la capa de tiles
  (sin sprites: bandera, destello y bocadillos los dibuja EndingScreen). Modo GRAPHIC 2: nombres en 0x3800, patrones en
  0x2000 y colores en 0x0000 (R2 = 0x0E, R3 = 0x7F, R4 = 0x07), un juego por tercio.
- ending_font.png (8·96 × 16): la fuente grande del epílogo, un glifo de 8 × 16 por carácter ASCII de 32 a 127 (la
  mitad de arriba es el tile b y la de abajo b − 1; ending.py, font). Los que la fuente no trae quedan vacíos. Blanco
  sobre transparente, para teñirlo.
Paleta: la del MSX2 por defecto (render_rooms.py). Son gráficos de Konami: quedan en extracted/, que no se versiona.
"""
import json, struct, zlib
from galious_rom import ROOT
from render_rooms import PALETTE

SRC = ROOT / 'logs' / 'omsx' / 'ending'
OUT = ROOT / 'extracted' / 'unity' / 'gfx'


def png_rgba(path, w, h, px):
    raw = b''.join(b'\0' + bytes(c for p in px[y * w:(y + 1) * w] for c in p) for y in range(h))
    chunk = lambda t, d: struct.pack('>I', len(d)) + t + d + struct.pack('>I', zlib.crc32(t + d))
    path.write_bytes(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 6, 0, 0, 0))
                     + chunk(b'IDAT', zlib.compress(raw, 9)) + chunk(b'IEND', b''))


def load(name):
    d = json.loads((SRC / f'vram_{name}.json').read_text())
    return bytes.fromhex(d['vram']), d['regs']


def tables(regs):
    return regs[2] * 0x400, (regs[4] & 4) << 11, (regs[3] & 0x80) << 6   # nombres, patrones, colores


def tile_pixels(vram, regs, third, n, backdrop):
    _, pat, col = tables(regs)
    out = []
    for y in range(8):
        p = vram[pat + third * 0x800 + n * 8 + y]
        c = vram[col + third * 0x800 + n * 8 + y]
        fg, bg = c >> 4, c & 15
        for x in range(8):
            k = fg if p & (0x80 >> x) else bg
            out.append(PALETTE[k or backdrop] + (255,))
    return out


def castle():
    vram, regs = load('castle')
    names, _, _ = tables(regs)
    backdrop = regs[7] & 15
    px = [None] * (256 * 192)
    for row in range(24):
        for col in range(32):
            t = tile_pixels(vram, regs, row // 8, vram[names + row * 32 + col], backdrop)
            for y in range(8):
                for x in range(8):
                    px[(row * 8 + y) * 256 + col * 8 + x] = t[y * 8 + x]
    png_rgba(OUT / 'ending_castle.png', 256, 192, px)


def glyph_tile(ch):
    if 'A' <= ch <= 'Z':
        return 0x0B + 2 * (ord(ch) - ord('A'))
    return {'1': 0x03, '7': 0x05, '8': 0x07, '9': 0x09, '.': 0x3F, ',': 0x41, '©': 0x43, "'": 0x45}.get(ch)


def font():
    vram, regs = load('epilogue')
    _, pat, _ = tables(regs)
    # el tercio donde la fuente está cargada (la A, tile 0x0B, no vacía)
    third = next((t for t in range(3) if any(vram[pat + t * 0x800 + 0x0B * 8: pat + t * 0x800 + 0x0B * 8 + 8])), 0)
    w = 8 * 96
    px = [(0, 0, 0, 0)] * (w * 16)
    for i in range(96):
        b = glyph_tile(chr(32 + i))
        if b is None:
            continue
        for half, n in ((0, b), (1, b - 1)):
            for y in range(8):
                p = vram[pat + third * 0x800 + n * 8 + y]
                for x in range(8):
                    if p & (0x80 >> x):
                        px[(half * 8 + y) * w + i * 8 + x] = (255, 255, 255, 255)
    png_rgba(OUT / 'ending_font.png', w, 16, px)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    if not (SRC / 'vram_castle.json').exists():
        print('final: sin volcados de VRAM (tools/openmsx/ending_vram.py); no hay gráficos del final')
        return
    castle()
    font()
    print(f'final: castillo y fuente 8x16 -> {OUT}')


if __name__ == '__main__':
    main()
