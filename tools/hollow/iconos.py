"""Los iconos de los objetos del original redibujados al estilo de Hollow Knight (lo pidió el usuario: los amuletos son
los objetos de Galious, no los amuletos de Hollow Knight).

Lee de la ROM del usuario (rom/*.rom, como el resto de tools/extract) el icono de 16 × 16 de cada objeto del 0x07 al
0x2C: la rutina b00:58E1 descomprime 32 bytes de patrones (puntero en 6E34 + 2·tipo) y 32 de colores (6E80 + 2·tipo),
del banco 10, con el RLE de b00:5C46 (el de vram_rle sin dirección), en 4 tiles de 8 × 8 (arriba a la izquierda, arriba
a la derecha, abajo a la izquierda, abajo a la derecha) con los colores de la paleta TMS9918 (el fondo blanco, transparente).

Y lo redibuja a 128 × 128 (7 por píxel y un píxel de margen para el contorno, en el hueco de un objeto) como los de
Hollow Knight:
  - la forma de cada color, agrandada y suavizada (sin escalones de píxel);
  - colores apagados y fríos, con algo del tono original (el jarrón sigue siendo rojizo);
  - luz de arriba (degradado y un brillo en el borde de arriba);
  - líneas de tinta entre colores y un contorno negro grueso alrededor.
Sale a Assets/Art/Resources/Hollow/objetos_sala/objetoXX.png (fuera de git: viene de la ROM), encima de los de
objetos.py; las armas (1–6, sin icono en el original) y los recursos (0x2D–0x2F) siguen con los de Hollow Knight.
"""
import sys
from pathlib import Path

from PIL import Image, ImageChops, ImageFilter

REPO = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(REPO / "tools/extract"))
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/objetos_sala"
K = 7                  # texels por píxel del original (16 px y uno de margen a cada lado: 126, en 128)
PAD = 1
SIZE = (16 + 2 * PAD) * K
GROUP = 3              # grupo de bancos con el banco 10 en 6000

PALETTE = [None, (0, 0, 0), (33, 200, 66), (94, 220, 120), (84, 85, 237), (125, 118, 252), (212, 82, 77),
           (66, 235, 245), (252, 85, 84), (255, 121, 120), (212, 193, 84), (230, 206, 128), (33, 176, 59),
           (201, 91, 186), (204, 204, 204), (255, 255, 255)]
INK = (14, 15, 20)


def rle(rom, p, n=32):
    out = []
    while len(out) < n:
        b = rom.byte(p, GROUP)
        p += 1
        if b == 0:
            break
        if b & 0x80:
            for _ in range(b & 0x7F):
                out.append(rom.byte(p, GROUP))
                p += 1
        else:
            out += [rom.byte(p, GROUP)] * b
            p += 1
    return (out + [0] * n)[:n]


def original(rom, t):
    """16 × 16 índices de color (0: transparente)."""
    pat = rle(rom, rom.word(0x6E34 + 2 * t, GROUP))
    col = rle(rom, rom.word(0x6E80 + 2 * t, GROUP))
    px = [[0] * 16 for _ in range(16)]
    for tile in range(4):
        tx, ty = (tile % 2) * 8, (tile // 2) * 8
        for row in range(8):
            pb, cb = pat[tile * 8 + row], col[tile * 8 + row]
            for bit in range(8):
                c = (cb >> 4) if pb & (0x80 >> bit) else (cb & 15)
                px[ty + row][tx + bit] = 0 if c == 15 and not pb & (0x80 >> bit) else c
    return px


def hollow_tone(rgb):
    """Un color del MSX llevado a la paleta de Hollow Knight: apagado, frío y con algo de su tono."""
    r, g, b = rgb
    lum = 0.3 * r + 0.59 * g + 0.11 * b
    base = (lum * 0.92 + 18, lum * 0.95 + 20, lum * 1.0 + 30)          # gris azulado según su luz
    mix = 0.38
    return tuple(max(0, min(255, int(base[i] * (1 - mix) + (r, g, b)[i] * mix))) for i in range(3))


def smooth_mask(cells, color):
    """La forma de un color agrandada K veces y suavizada."""
    n = 16 + 2 * PAD
    m = Image.new("L", (n, n))
    m.putdata([255 if 0 <= y - PAD < 16 and 0 <= x - PAD < 16 and cells[y - PAD][x - PAD] == color else 0
               for y in range(n) for x in range(n)])
    # Desenfoque y umbral bajo: los escalones se redondean sin perder las líneas de un píxel (cadenas, cuerdas).
    m = m.resize((SIZE, SIZE), Image.NEAREST).filter(ImageFilter.GaussianBlur(K * 0.42))
    return m.point(lambda v: 255 if v > 84 else 0).filter(ImageFilter.GaussianBlur(0.7))


def redraw(cells):
    colors = sorted({c for row in cells for c in row if c})
    body = Image.new("RGBA", (SIZE, SIZE))
    sil = Image.new("L", (SIZE, SIZE))
    masks = {}
    for c in colors:
        m = smooth_mask(cells, c)
        masks[c] = m
        layer = Image.new("RGBA", (SIZE, SIZE), hollow_tone(PALETTE[c]) + (255,))
        body.paste(layer, (0, 0), m)
        sil = ImageChops.lighter(sil, m)
    # Luz de arriba: un degradado que oscurece hacia abajo.
    grad = Image.linear_gradient("L").resize((SIZE, SIZE))                 # 0 arriba, 255 abajo
    shade = Image.new("RGBA", (SIZE, SIZE), (10, 12, 22, 255))
    body = Image.composite(shade, body, grad.point(lambda v: int(v * 0.42)))
    # Brillo en el borde de arriba de cada forma: la forma menos ella misma bajada unos texels.
    shifted = ImageChops.offset(sil, 0, 3)
    rim = ImageChops.subtract(sil, shifted).filter(ImageFilter.GaussianBlur(1.2))
    body = Image.composite(Image.new("RGBA", (SIZE, SIZE), (245, 245, 250, 255)), body, rim.point(lambda v: int(v * 0.35)))
    body.putalpha(sil)
    # Tinta entre colores: el borde de cada forma, fino.
    lines = Image.new("L", (SIZE, SIZE))
    for m in masks.values():
        edge = ImageChops.subtract(m.filter(ImageFilter.MaxFilter(3)), m.filter(ImageFilter.MinFilter(3)))
        lines = ImageChops.lighter(lines, edge)
    lines = ImageChops.multiply(lines, sil.filter(ImageFilter.MinFilter(5)))
    body = Image.composite(Image.new("RGBA", (SIZE, SIZE), INK + (255,)), body, lines.point(lambda v: int(v * 0.85)))
    # Contorno negro grueso por fuera.
    out = Image.new("RGBA", (SIZE, SIZE))
    outline = sil.filter(ImageFilter.MaxFilter(9)).filter(ImageFilter.GaussianBlur(1))
    out.paste(Image.new("RGBA", (SIZE, SIZE), INK + (255,)), (0, 0), outline)
    out.alpha_composite(body)
    return out


def build(src=None):
    from galious_rom import Rom
    try:
        rom = Rom()
    except (StopIteration, FileNotFoundError):
        print("iconos: sin la ROM en rom/, se quedan los de Hollow Knight")
        return
    OUT.mkdir(parents=True, exist_ok=True)
    for t in range(0x07, 0x2D):
        im = Image.new("RGBA", (128, 128))
        im.alpha_composite(redraw(original(rom, t)), ((128 - SIZE) // 2, 128 - SIZE))
        im.save(OUT / f"objeto{t:02X}.png")
    print(f"iconos: {0x2D - 0x07} objetos del original al estilo de Hollow Knight → {OUT}")


if __name__ == "__main__":
    build()
