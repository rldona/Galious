"""Proyectiles de las armas del jugador con los de Hollow Knight (Visuals/HollowWeapons en Unity).

Cada arma lleva el proyectil de Hollow Knight que hace su icono (objetos.py): la flecha, el Espíritu vengativo (Ball);
la de cerámica, el Alma de sombra (Ball Lv2); los dos fuegos, la bola de fuego (Flameball, Idle); la mina, un glóbulo
(Generic Glob, Wobble). Salen a Assets/Art/Resources/Hollow/armas/armaN_NN.png (fuera de git): recortados con la misma
caja para todos los fotogramas, mirando a la derecha, centrados y de LENGTH px del original de largo (8 texels por px).
"""
import csv
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/armas"

# arma → (colección, clip, largo en px, mira a la izquierda en Hollow Knight). Ball y Ball Lv2 ya miran a la derecha
# (la cara delante y la estela detrás, a la izquierda): no se voltean.
WEAPONS = {
    1: ("Spells Anim", "Ball", 22, False),
    2: ("Spells Anim", "Ball Lv2", 26, False),
    3: ("Flameball Anim", "Idle", 14, True),
    4: ("Flameball Anim", "Idle", 14, True),
    5: ("Generic Glob Anim", "Wobble", 14, False),
}


def frames(src, col, clip):
    rows = csv.DictReader(open(src / "hollow-knight-tk2d" / col / "clip_frames.csv", encoding="utf-8"))
    out = []
    for r in rows:
        if r["clip"] == clip and r["archivo_png"] not in out:
            out.append(r["archivo_png"])
    return [src / "hollow-knight-tk2d" / col / f for f in out]


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    for wid, (col, clip, length, left) in WEAPONS.items():
        ims = [Image.open(p).convert("RGBA") for p in frames(src, col, clip)]
        boxes = [b for b in (im.getchannel("A").point(lambda v: 255 if v > 8 else 0).getbbox() for im in ims) if b]
        box = (min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))
        k = length * 8 / (box[2] - box[0])
        for i, im in enumerate(ims):
            crop = im.crop(box).resize((round((box[2] - box[0]) * k), round((box[3] - box[1]) * k)), Image.LANCZOS)
            if left:
                crop = crop.transpose(Image.FLIP_LEFT_RIGHT)
            crop.save(OUT / f"arma{wid}_{i:02d}.png")
    print(f"armas: {len(WEAPONS)} proyectiles → {OUT}")
