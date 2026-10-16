"""Menú principal y menú de pausa de Hollow Knight para Galious: The Hollow Kingdom (Visuals/HollowMainMenu y
HollowPauseMenu en Unity).

Sale a Assets/Art/Resources/Hollow/menu/ (fuera de git, como el resto del arte de Team Cherry): las filigranas de sus
menús, sin su logo ni el de Team Cherry (el título es el nuestro, escrito con su fuente):
  - flecha.png: la flecha con filigrana que señala la opción elegida (a la izquierda; la derecha es la misma volteada);
  - separador.png: la línea fina con un remate, bajo los títulos de las pantallas;
  - adorno_arriba.png / adorno_abajo.png: los adornos grandes con la cresta que enmarcan el título (Pause_fleurs);
  - remate.png: la cresta sola.
Y la fuente de Hollow Knight (Perpetua) en Assets/Art/Resources/Hollow/fuentes/Perpetua.ttf, que UiKit usa si está.
"""
import shutil
from pathlib import Path

from PIL import Image

import atlas_piezas

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/menu"
FONTS = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/fuentes"

# nombre → (atlas, caja), de las hojas numeradas de atlas_piezas.py.
PIECES = {
    "flecha": ("Menu.png", (364, 294, 441, 356)),
    "separador": ("Menu.png", (4, 833, 406, 883)),
    "adorno_arriba": ("Pause_fleurs.png", (348, 1986, 618, 2040)),
    "adorno_abajo": ("Pause_fleurs.png", (348, 1398, 618, 1452)),
    "remate": ("Pause_fleurs.png", (180, 412, 244, 500)),
}


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    atlases = {}
    for name, (atlas, box) in PIECES.items():
        if atlas not in atlases:
            atlases[atlas] = Image.open(src / "hollow-knight-texturas/atlas_sprites" / atlas).convert("RGBA")
        atlas_piezas.piece(atlases[atlas], box).save(OUT / f"{name}.png")
    # El adorno de arriba, del revés (Pause_fleurs solo trae el de abajo con la cresta hacia abajo).
    Image.open(OUT / "adorno_arriba.png").transpose(Image.FLIP_TOP_BOTTOM).save(OUT / "adorno_arriba.png")
    FONTS.mkdir(parents=True, exist_ok=True)
    shutil.copy(src / "hollow-knight-fuentes/Perpetua.ttf", FONTS / "Perpetua.ttf")
    print(f"menú: {len(PIECES)} filigranas y la fuente → {OUT}")
