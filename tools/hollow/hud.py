"""Marcador de Hollow Knight para Maze of Knight (Visuals/HollowHud en Unity).

Sale a Assets/Art/Resources/Hollow/hud/ (fuera de git, como el resto del arte de Team Cherry):
  - marco.png: el marco del orbe de alma con su trazo (HUD Frame Idle), en su lienzo de 257 × 164;
  - alma_NN_FF.png: el mismo lienzo con el alma dentro del orbe, del 0 al 100 % (NN de 0 a 10) y el fotograma FF de su
    ola: la cara del Caballero hasta donde llega el líquido, como en Hollow Knight (en OnGUI no se puede recortar);
  - mascara_NN.png (llena, con su brillo), mascara_vacia.png y mascara_rota_NN.png, en su lienzo de 126 × 167;
  - geo.png, el geo del contador de monedas.
"""
import csv
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/hud"


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
    # Alma, como en Hollow Knight: la cara del Caballero (Soul Orb Glow: blanca con los ojos) se ve hasta donde llega el
    # líquido (Soulorb_fills_animation, Idle: un bloque blanco con la superficie ondulada, en el mismo lienzo que la
    # cara), que sube con el nivel; todo dentro del círculo de la cara, encajado en el hueco oscuro del marco (medido:
    # centro (72, 99), radio 58; la cara, de radio 56). 11 niveles × 6 fotogramas de la ola (alma_NN_FF).
    glow = frames(src, "HUD Anim", "Soul Orb Glow")[0]
    gb = glow.getchannel("A").point(lambda v: 255 if v > 128 else 0).getbbox()      # el círculo de la cara
    k = 112 / (gb[2] - gb[0])          # un poco menos que el hueco (116): se ve el borde del marco
    gw, gh = round(glow.width * k), round(glow.height * k)
    gcx, gcy = (gb[0] + gb[2]) / 2 * k, (gb[1] + gb[3]) / 2 * k
    ox, oy = round(72 - gcx), round(99 - gcy)
    glow_s = glow.resize((gw, gh), Image.LANCZOS)
    face_a = glow_s.getchannel("A")
    circle_h = (gb[3] - gb[1]) * k
    waves = [f.resize((gw, gh), Image.LANCZOS) for f in frames(src, "Soulorb_fills_animation", "Idle")]
    top_full = min(w.getchannel("A").point(lambda v: 255 if v > 40 else 0).getbbox()[1] for w in waves)
    for lvl in range(11):
        for fi, wave in enumerate(waves):
            layer = Image.new("RGBA", frame.size, (0, 0, 0, 0))
            if lvl > 0:
                # El bloque baja tanto como le falte: a 10, su borde de arriba en lo alto del círculo.
                drop = round((gb[1] * k - top_full) + (1 - lvl / 10) * circle_h) if lvl < 10 else round(gb[1] * k - top_full) - 4
                liquid = Image.new("L", (gw, gh), 0)
                liquid.paste(wave.getchannel("A"), (0, drop))
                # Debajo del bloque, lleno (el bloque no llega al fondo del círculo).
                wb = wave.getchannel("A").point(lambda v: 255 if v > 40 else 0).getbbox()
                if wb and drop + wb[3] < gh:
                    liquid.paste(255, (wb[0], drop + wb[3] - 2, wb[2], gh))
                a = ImageChops.multiply(liquid, face_a)
                face = glow_s.copy()
                face.putalpha(a)
                layer.paste(face, (ox, oy), face)
            layer.save(OUT / f"alma_{lvl:02d}_{fi:02d}.png")
    full = frames(src, "HUD Anim", "Health Idle")
    for i, f in enumerate(full[::3]):
        f.save(OUT / f"mascara_{i:02d}.png")
    frames(src, "HUD Anim", "Health Empty")[0].save(OUT / "mascara_vacia.png")
    for i, f in enumerate(frames(src, "HUD Anim", "Health Break")):
        f.save(OUT / f"mascara_rota_{i:02d}.png")
    frames(src, "HUD Anim", "Coin Idle")[0].save(OUT / "geo.png")
    print(f"marcador: marco, alma, máscaras y geo → {OUT}")
