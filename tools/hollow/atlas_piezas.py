"""Piezas sueltas de un atlas de Hollow Knight (los de hollow-knight-texturas/atlas_sprites vienen sin recortes).

Las piezas de un atlas están separadas por transparencia: se buscan sus manchas (lo opaco, en una rejilla reducida y
ensanchado un poco para que una pieza con huecos no se parta) y cada una da una caja. Las cajas salen ordenadas de arriba
abajo y de izquierda a derecha, así que el número de una pieza no cambia mientras no cambie el atlas.

Uso, para elegir piezas a mano:
    .venv/bin/python -I tools/hollow/atlas_piezas.py <atlas.png> <hoja.png>
escribe una hoja con cada pieza numerada y su caja; escenario.py guarda la caja de las elegidas (piece_from_atlas).
"""
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

MIN_SIDE = 24   # texels: lo más pequeño se ignora (motas, gotas sueltas)


def boxes(atlas):
    """Cajas (x0, y0, x1, y1) de las manchas del atlas, en texels."""
    alpha = atlas.getchannel("A")
    f = max(1, max(atlas.size) // 1024)
    small = alpha.resize((atlas.width // f, atlas.height // f), Image.BOX)
    a = small.point(lambda v: 255 if v > 90 else 0)   # sin las sombras suaves, que unen piezas vecinas
    w, h = a.size
    px = a.load()
    seen = bytearray(w * h)
    out = []
    for y in range(h):
        for x in range(w):
            if not px[x, y] or seen[y * w + x]:
                continue
            stack = [(x, y)]
            seen[y * w + x] = 1
            x0 = x1 = x
            y0 = y1 = y
            while stack:
                cx, cy = stack.pop()
                x0, x1, y0, y1 = min(x0, cx), max(x1, cx), min(y0, cy), max(y1, cy)
                for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                    if 0 <= nx < w and 0 <= ny < h and px[nx, ny] and not seen[ny * w + nx]:
                        seen[ny * w + nx] = 1
                        stack.append((nx, ny))
            b = (x0 * f, y0 * f, min(atlas.width, (x1 + 1) * f), min(atlas.height, (y1 + 1) * f))
            if b[2] - b[0] >= MIN_SIDE and b[3] - b[1] >= MIN_SIDE:
                out.append(b)
    out.sort(key=lambda b: (b[1] // 64, b[0]))
    return out


def piece(atlas, box):
    """La pieza de esa caja, sin lo que asome de las vecinas: solo su mancha más grande."""
    im = atlas.crop(box)
    a = im.getchannel("A").point(lambda v: 255 if v > 12 else 0)
    w, h = a.size
    px = a.load()
    seen = bytearray(w * h)
    best = None
    for y in range(0, h, 2):
        for x in range(0, w, 2):
            if not px[x, y] or seen[y * w + x]:
                continue
            stack, pts = [(x, y)], []
            seen[y * w + x] = 1
            while stack:
                cx, cy = stack.pop()
                pts.append((cx, cy))
                for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                    if 0 <= nx < w and 0 <= ny < h and px[nx, ny] and not seen[ny * w + nx]:
                        seen[ny * w + nx] = 1
                        stack.append((nx, ny))
            if best is None or len(pts) > len(best):
                best = pts
    if not best:
        return im
    mask = Image.new("L", im.size, 0)
    mp = mask.load()
    for x, y in best:
        mp[x, y] = 255
    # Los bordes suaves (alfa baja) que rodean la mancha, también.
    mask = mask.filter(ImageFilter.MaxFilter(5))
    clean = Image.new("RGBA", im.size, (0, 0, 0, 0))
    clean.paste(im, (0, 0), mask)
    bb = clean.getchannel("A").point(lambda v: 255 if v > 8 else 0).getbbox()
    return clean.crop(bb) if bb else clean


def sheet(atlas_path, out_path):
    atlas = Image.open(atlas_path).convert("RGBA")
    found = boxes(atlas)
    W, x, y, rowh = 2000, 0, 0, 0
    canvas = Image.new("RGBA", (W, 8000), (200, 120, 160, 255))
    d = ImageDraw.Draw(canvas)
    for i, b in enumerate(found):
        im = atlas.crop(b)
        k = min(1, 300 / max(im.size))
        im = im.resize((max(1, int(im.width * k)), max(1, int(im.height * k))))
        if x + max(im.width, 110) > W:
            x, y, rowh = 0, y + rowh + 16, 0
        canvas.alpha_composite(im, (x, y + 12))
        d.text((x, y), f"{i}", fill=(0, 0, 0))
        x += max(im.width, 110) + 8
        rowh = max(rowh, im.height)
        print(i, b)
    canvas.crop((0, 0, W, y + rowh + 16)).save(out_path)


if __name__ == "__main__":
    sheet(Path(sys.argv[1]), Path(sys.argv[2]))
