"""Marcador de Hollow Knight para Maze of Knight (Visuals/HollowHud en Unity).

Sale a Assets/Art/Resources/Hollow/hud/ (fuera de git, como el resto del arte de Team Cherry):
  - marco.png: el marco del orbe de alma con su trazo (HUD Frame Idle), en su lienzo de 257 × 164;
  - alma_00…10.png: el mismo lienzo con el alma dentro del orbe, del 0 al 100 % (la experiencia hacia el siguiente
    nivel), recortada al círculo del orbe (en OnGUI no se puede recortar en círculo);
  - mascara_NN.png (llena, con su brillo), mascara_vacia.png y mascara_rota_NN.png, en su lienzo de 126 × 167;
  - geo.png, el geo del contador de monedas.
"""
import csv
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/hud"
# El círculo del orbe dentro del marco (medido en HUD Frame Idle: lo oscuro va de x 11 a 130; centro hacia (71, 92)).
ORB = (17, 38, 125, 146)


def frames(src, col, clip):
    base = src / "hollow-knight-tk2d" / col
    rows = [r for r in csv.DictReader(open(base / "clip_frames.csv", encoding="utf-8")) if r["clip"] == clip]
    if not rows:
        raise SystemExit(f"No está el clip {clip!r} en {col}")
    seen, out = set(), []
    for r in rows:
        if r["archivo_png"] in seen:
            continue
        seen.add(r["archivo_png"])
        out.append(Image.open(base / r["archivo_png"]).convert("RGBA"))
    return out


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    frame = frames(src, "HUD Anim", "HUD Frame Idle")[0]
    frame.save(OUT / "marco.png")
    # Alma: el líquido de Hollow Knight escalado al orbe y recortado a su círculo, hasta la altura de cada nivel.
    liquid = frames(src, "Soulorb_fills_animation", "Idle")[0]
    liquid = liquid.crop(liquid.getbbox())
    w, h = ORB[2] - ORB[0], ORB[3] - ORB[1]
    liquid = liquid.resize((w, h), Image.LANCZOS)
    circle = Image.new("L", frame.size, 0)
    ImageDraw.Draw(circle).ellipse(ORB, fill=255)
    for k in range(11):
        # El líquido entero (con su superficie ondulada arriba), subido hasta el nivel y recortado al círculo.
        top = ORB[3] - round(h * k / 10)
        layer = Image.new("RGBA", frame.size, (0, 0, 0, 0))
        if k > 0:
            layer.paste(liquid, (ORB[0], top))
        layer.putalpha(ImageChops.multiply(layer.getchannel("A"), circle))
        layer.save(OUT / f"alma_{k:02d}.png")
    full = frames(src, "HUD Anim", "Health Idle")
    for i, f in enumerate(full[::3]):
        f.save(OUT / f"mascara_{i:02d}.png")
    frames(src, "HUD Anim", "Health Empty")[0].save(OUT / "mascara_vacia.png")
    for i, f in enumerate(frames(src, "HUD Anim", "Health Break")):
        f.save(OUT / f"mascara_rota_{i:02d}.png")
    frames(src, "HUD Anim", "Coin Idle")[0].save(OUT / "geo.png")
    print(f"marcador: marco, alma, máscaras y geo → {OUT}")
