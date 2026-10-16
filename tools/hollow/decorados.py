"""Decorados de las salas que el escenario de Hollow Knight no pinta (Visuals/HollowDecor en Unity).

En el original son baldosas del fondo; el escenario del remake solo dibuja la colisión, así que se ponen como sprites,
de texturas sueltas de Hollow Knight (hollow-knight-texturas/otras), con los pies abajo en el centro y a 8 texels por
píxel del original. Salen a Assets/Art/Resources/Hollow/decorados/ (fuera de git):
  - lapida: las lápidas de cada mundo (una tumba de los sueños);
  - lapida_palabra: la que tiene la palabra del jefe, que se lee con la lupa (una tablilla de Hollow Knight);
  - puerta_tienda: la entrada a las tiendas de los santuarios del castillo (la puerta de la tienda de Iselda);
  - puerta_santuario: la de los otros santuarios (Demeter, pistas, la Muerte): la puerta iluminada de los Creyentes;
  - arco y columna: el marco de las puertas de los mundos en el castillo.
"""
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/decorados"

# nombre → (textura, medida en px del original, "ancho" o "alto")
PIECES = {
    "lapida": ("dream_graves_0000_4.png", 18, "ancho"),
    "lapida_palabra": ("Lore_0017_city_01.png", 26, "ancho"),
    "puerta_tienda": ("antique dealer door.png", 30, "alto"),
    "puerta_santuario": ("Shrine_of_Believers_Ext_door.png", 30, "alto"),
    "arco": ("big_arch_full.png", 56, "ancho"),
    "columna": ("Ruins_gate_0000_a.png", 40, "alto"),
}


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    for name, (tex, size, side) in PIECES.items():
        im = Image.open(src / "hollow-knight-texturas/otras" / tex).convert("RGBA")
        bb = im.getchannel("A").point(lambda a: 255 if a > 8 else 0).getbbox()
        if bb:
            im = im.crop(bb)
        k = size * 8 / (im.width if side == "ancho" else im.height)
        im.resize((max(1, round(im.width * k)), max(1, round(im.height * k))), Image.LANCZOS).save(OUT / f"{name}.png")
    print(f"decorados: {len(PIECES)} piezas → {OUT}")
