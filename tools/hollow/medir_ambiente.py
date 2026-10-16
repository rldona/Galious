"""Mide el color de cada zona de Hollow Knight en las imágenes de sus salas (hollow-knight-mundos/<zona>/<sala>/
<sala>_perspectiva.jpg: cada escena reconstruida desde su fichero de nivel, con los tintes de sus capas) y el de nuestras
salas sin gradación (capturas de -ambienteNeutro -capturaSalas), y propone la gradación de cada mundo para Visuals/
HollowAmbience: el filtro de color que lleva el tono medio de las nuestras al de la zona (sin cambiar el brillo) y la
saturación que iguala la suya.

Uso:
    .venv/bin/python -I tools/hollow/medir_ambiente.py <carpeta de Hollow Knight decodificado> <carpeta de capturas>

Solo cuenta lo que no es casi negro (la roca maciza de las dos es negra). Las capturas de las nuestras se recortan
arriba (el marcador).
"""
import colorsys
import sys
import warnings
from pathlib import Path

from PIL import Image

Image.MAX_IMAGE_PIXELS = None          # las salas de Hollow Knight miden más de 100 millones de píxeles
warnings.filterwarnings("ignore", category=DeprecationWarning)

# Mundo (0 = castillo) → carpetas de zona de Hollow Knight (las de HollowScenery.Looks)
ZONES = {
    0: ["Crossroads"], 1: ["Fungus1"], 2: ["Waterways"], 3: ["Hive"], 4: ["Mines"], 5: ["Ruins1", "Ruins2"],
    6: ["Fungus2"], 7: ["Abyss"], 8: ["Deepnest"], 9: ["Deepnest_East"], 10: ["White_Palace"],
}
DARK = 0.07       # valor (HSV) por debajo del cual el píxel no cuenta
MAX_ROOMS = 40


def stats(images, crop_top=0.0):
    n = 0
    r = g = b = s = v = 0.0
    for path in images:
        im = Image.open(path).convert("RGB")
        if crop_top:
            im = im.crop((0, int(im.height * crop_top), im.width, im.height))
        im.thumbnail((480, 480))
        for pr, pg, pb in im.getdata():
            hh, ss, vv = colorsys.rgb_to_hsv(pr / 255, pg / 255, pb / 255)
            if vv < DARK:
                continue
            n += 1
            r += pr / 255; g += pg / 255; b += pb / 255; s += ss; v += vv
    if n == 0:
        return None
    return (r / n, g / n, b / n, s / n, v / n, n)


def main(hk, captures):
    hk, captures = Path(hk), Path(captures)
    print("mundo | zona               | HK r g b  sat  val | nuestro r g b  sat  val | filtro r g b   saturación")
    for w, zones in ZONES.items():
        rooms = []
        for z in zones:
            rooms += sorted((hk / "hollow-knight-mundos" / z).glob("*/*_perspectiva.jpg"))
        rooms = rooms[:MAX_ROOMS]
        ours = [p for p in sorted(captures.glob(f"sala_m{w:02d}_*.png"))
                if not any(k in p.name for k in ("colision", "nadar", "lava", "puerta"))]
        a, o = stats(rooms), stats(ours, crop_top=0.14)
        if not a or not o:
            print(f"{w:5} | {'/'.join(zones):18} | sin datos ({len(rooms)} salas de HK, {len(ours)} capturas)")
            continue
        # Filtro: el tono de la zona sobre el nuestro, normalizado para no cambiar la luminancia.
        lum = lambda c: 0.2126 * c[0] + 0.7152 * c[1] + 0.0722 * c[2]
        ta = [c / max(1e-4, lum(a)) for c in a[:3]]
        to = [c / max(1e-4, lum(o)) for c in o[:3]]
        f = [x / max(1e-4, y) for x, y in zip(ta, to)]
        k = lum(f)
        f = [min(1.35, max(0.65, x / k)) for x in f]
        sat = max(-60, min(40, round((a[3] / max(1e-4, o[3]) - 1) * 100)))
        print(f"{w:5} | {'/'.join(zones):18} | {a[0]:.2f} {a[1]:.2f} {a[2]:.2f} {a[3]:.2f} {a[4]:.2f} | "
              f"{o[0]:.2f} {o[1]:.2f} {o[2]:.2f} {o[3]:.2f} {o[4]:.2f} | {f[0]:.2f} {f[1]:.2f} {f[2]:.2f}   {sat:+d}")


if __name__ == "__main__":
    main(*sys.argv[1:3])
