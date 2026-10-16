"""Los tajos del espadazo hacia arriba y hacia abajo de Hollow Knight (RemakeRules.HollowSlashes; PlayerController).

En Hollow Knight van en otro lienzo que el Caballero (no se pueden alinear con él como la estela de delante), así que
salen sueltos, recortados y centrados, a Assets/Art/Resources/Hollow/tajos/tajo_<arriba|abajo>_NN.png (fuera de git):
de Height px del original de alto, a 8 texels por píxel. PlayerController los pone en medio de la zona que golpea.
"""
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/tajos"
HEIGHT = 26          # px del original
FRAMES = {
    "arriba": ["up_slash_effect0000", "up_slash_effect0001"],
    "abajo": ["down_slash_effect0001", "down_slash_effect0002"],
}


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    sprites = src / "hollow-knight-tk2d/Knight/sprites"
    for name, frames in FRAMES.items():
        ims = [Image.open(sprites / f"{f}.png").convert("RGBA") for f in frames]
        # La misma caja para los dos fotogramas, para que no salte.
        boxes = [im.getchannel("A").point(lambda v: 255 if v > 8 else 0).getbbox() for im in ims]
        box = (min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))
        k = HEIGHT * 8 / (box[3] - box[1])
        for i, im in enumerate(ims):
            crop = im.crop(box)
            crop = crop.resize((round(crop.width * k), round(crop.height * k)), Image.LANCZOS)
            crop.save(OUT / f"tajo_{name}_{i:02d}.png")
    print(f"tajos: arriba y abajo → {OUT}")
