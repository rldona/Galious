"""Los dioses de los santuarios con personajes de Hollow Knight (ShrineScreen en Unity), en la pantalla del santuario.

Cada dios lo hace un personaje de Hollow Knight que encaja con él, con su animación de reposo:
  - Hefesto, el herrero: el Forjador de Aguijones;
  - Ares, la tienda de armas: Sly, el viejo maestro del aguijón;
  - Deméter, la que guarda la partida y vende: Iselda, la tendera;
  - Atenea, las pistas: el Anciano Hu;
  - Asclepio, la de la vitalidad: Jiji, la Confesora;
  - la Muerte: Grimm.
Salen a Assets/Art/Resources/Hollow/dioses/<dios>_NN.png (fuera de git, como el resto del arte de Team Cherry): los
fotogramas recortados con la misma caja, de modo que los pies quedan en el mismo sitio y no tiembla.
"""
import csv
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/dioses"

# dios (como en Shrines.json) → (colección tk2d, clip de reposo)
GODS = {
    "Hephaestus": ("Nailsmith", "Idle"),
    "Ares": ("Sly Anim", "Idle"),
    "Demeter": ("Iselda Anim", "Idle"),
    "Athena": ("Ghost Elder Hu Anim", "Idle"),
    "Asclepius": ("Jiji Anim", "Idle"),
    "Death": ("Grimm NPC Anim", "Idle"),
}
MAX_FRAMES = 24


def frames(src, col, clip):
    rows = csv.DictReader(open(src / "hollow-knight-tk2d" / col / "clip_frames.csv", encoding="utf-8"))
    out = []
    for r in rows:
        if r["clip"] == clip and r["archivo_png"] not in out:
            out.append(r["archivo_png"])
    return [src / "hollow-knight-tk2d" / col / f for f in out][:MAX_FRAMES]


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    n = 0
    for god, (col, clip) in GODS.items():
        ims = [Image.open(p).convert("RGBA") for p in frames(src, col, clip)]
        boxes = [b for b in (im.getchannel("A").point(lambda v: 255 if v > 8 else 0).getbbox() for im in ims) if b]
        if not boxes:
            continue
        box = (min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))
        for i, im in enumerate(ims):
            im.crop(box).save(OUT / f"{god.lower()}_{i:02d}.png")
            n += 1
    print(f"dioses: {len(GODS)} dioses, {n} fotogramas → {OUT}")
