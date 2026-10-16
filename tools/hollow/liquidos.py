"""Agua y lava de Hollow Knight para Maze of Knight (Visuals/HollowLiquids en Unity).

Sale a Assets/Art/Resources/Hollow/liquidos/, fuera de git como el resto del arte de Team Cherry, a la escala del Caballero
(×1,375), en blanco o en su color (HollowLiquids los tiñe: azul el agua, naranja la lava):
  - superficie_NN.png: la línea ondulada de la superficie del agua (Water_effects, Ripple Water; 1 de cada 2 de sus 70);
  - salpicadura_NN.png: al entrar o salir del líquido (Splash_into_water);
  - gota_NN.png: la gota que cae del techo sobre el agua (WaterDrip, Drip) y gota_cae.png, la gota cayendo;
  - burbuja.png y burbuja_estalla_NN.png: las burbujas que suben y estallan en la superficie (bubble_burst_ss);
  - ascua.png, humo.png: lo que sube de la lava (ember_particle, smoke_cloud_orange);
  - globulo_a/b/c.png: los glóbulos naranjas de la infección de los Cruces, flotando en la lava (Hollow Knight no tiene
    lava: el usuario pidió su agua en naranja y la infección, las dos cosas);
  - vapor_NN.png: el chorro de vapor del ácido (acid_burst_ss), para la lava;
  - niebla.png: la niebla que flota sobre el agua (water_fog).
"""
import csv
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/liquidos"
SCALE = 1.375


def save(im, name):
    bb = im.getchannel("A").point(lambda a: 255 if a > 4 else 0).getbbox()
    if bb:
        im = im.crop(bb)
    im = im.resize((max(1, round(im.width * SCALE)), max(1, round(im.height * SCALE))), Image.LANCZOS)
    im.save(OUT / f"{name}.png")


def clip(src, col, name):
    base = src / "hollow-knight-tk2d" / col
    rows = [r for r in csv.DictReader(open(base / "clip_frames.csv", encoding="utf-8")) if r["clip"] == name]
    if not rows:
        raise SystemExit(f"No está el clip {name!r} en {col}")
    return [Image.open(base / r["archivo_png"]).convert("RGBA") for r in rows]


def save_clip(frames, name, common=True):
    """Fotogramas con un recorte común (la animación no baila)."""
    if common:
        boxes = [f.getchannel("A").point(lambda a: 255 if a > 4 else 0).getbbox() for f in frames]
        boxes = [b for b in boxes if b]
        box = (min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))
        frames = [f.crop(box) for f in frames]
    for i, f in enumerate(frames):
        f = f.resize((max(1, round(f.width * SCALE)), max(1, round(f.height * SCALE))), Image.LANCZOS)
        f.save(OUT / f"{name}_{i:02d}.png")


def grid(im, cols, rows):
    w, h = im.width // cols, im.height // rows
    return [im.crop((c * w, r * h, c * w + w, r * h + h)) for r in range(rows) for c in range(cols)]


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    fx = src / "hollow-knight-texturas/efectos"
    # La superficie: un recorte común con su ancho entero (las piezas se ponen una tras otra).
    ripple = clip(src, "Water_effects", "Ripple Water")[::2]
    save_clip(ripple, "superficie")
    save_clip(clip(src, "Water_effects", "Splash_into_water"), "salpicadura")
    save_clip(clip(src, "WaterDrip", "Drip"), "gota")
    save(clip(src, "WaterDrip", "Fall")[0], "gota_cae")
    burst = grid(Image.open(fx / "bubble_burst_ss.png").convert("RGBA"), 4, 2)
    save(burst[3], "burbuja")
    save_clip([burst[4], burst[5]], "burbuja_estalla", common=False)
    save(Image.open(fx / "ember_particle.png").convert("RGBA"), "ascua")
    save(Image.open(fx / "smoke_cloud_orange.png").convert("RGBA"), "humo")
    save(Image.open(fx / "water_fog.png").convert("RGBA").rotate(90, expand=True), "niebla")
    save_clip(grid(Image.open(fx / "acid_burst_ss.png").convert("RGBA"), 4, 2)[:4], "vapor")
    inf = src / "hollow-knight-crossroads/atlas_Infected"
    for n, f in (("globulo_a", "large_blob_030000"), ("globulo_b", "small_blob_010000"), ("globulo_c", "small_blob_020000")):
        save(Image.open(inf / f"{f}.png").convert("RGBA"), n)
    print(f"líquidos: superficie, salpicadura, gotas, burbujas, ascuas, humo, vapor y glóbulos → {OUT}")
