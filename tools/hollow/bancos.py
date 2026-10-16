"""Bancos de Hollow Knight para Galious: The Hollow Kingdom (RemakeRules.HollowBenches, Visuals en GameBootstrap).

Sale a Assets/Art/Resources/Hollow/bancos/<zona>.png (fuera de git, como el resto del arte de Team Cherry), uno por zona
de escenario (las de escenario.py), a la escala del Caballero (×1,375) y con el pivote abajo en el centro (la carpeta
está en las de pies del ArtImporter: el banco se apoya en el suelo).
"""
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/bancos"
SCALE = 1.375

# zona → banco de Hollow Knight (texturas sueltas de hollow-knight-texturas/otras).
BENCHES = {
    "cruces": "bone_bench",            # el banco de hueso de los Cruces, el de siempre
    "greenpath": "slug_shrine_bench",  # piedra con hojas
    "canales": "pay_bench0012",        # el de hierro forjado
    "colmena": "bone_bench",
    "cristal": "bone_bench",
    "ciudad": "pay_bench0012",
    "fungicos": "fung_bench_new",
    "abismo": "bone_bench",
    "nido": "spider_town_bench",
    "confines": "bone_bench",
    "palacio": "GG_bench_metal_0000_2",  # el dorado
}


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    for zone, name in BENCHES.items():
        im = Image.open(src / "hollow-knight-texturas/otras" / f"{name}.png").convert("RGBA")
        bb = im.getchannel("A").point(lambda a: 255 if a > 8 else 0).getbbox()
        if bb:
            im = im.crop(bb)
        im = im.resize((max(1, round(im.width * SCALE)), max(1, round(im.height * SCALE))), Image.LANCZOS)
        im.save(OUT / f"{zone}.png")
    print(f"bancos: {len(BENCHES)} zonas → {OUT}")
