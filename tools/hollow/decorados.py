"""Decorados de las salas que el escenario de Hollow Knight no pinta (Visuals/HollowDecor en Unity).

En el original son baldosas del fondo; el escenario del remake solo dibuja la colisión, así que se ponen como sprites,
de texturas sueltas de Hollow Knight (hollow-knight-texturas/otras), con los pies abajo en el centro y a 8 texels por
píxel del original. Salen a Assets/Art/Resources/Hollow/decorados/ (fuera de git):
  - lapida: las lápidas de cada mundo (una tumba de los sueños);
  - lapida_palabra: la que tiene la palabra del jefe, que se lee con la lupa (una tablilla de Hollow Knight);
  - puerta_tienda: la entrada a las tiendas de los santuarios del castillo, una boca de cueva iluminada por dentro (la
    cueva de los mineros de los Picos de Cristal; lo pidió el usuario: se entra en ellas como en una cueva). Cambia con
    la zona de Hollow Knight del mapa (HollowScenery.Looks): puerta_tienda_<zona> si la hay; si no, la de piedras;
  - puerta_tienda_cristal: la de la zona de cristal, la cueva rosada de los mineros;
  - puerta_santuario: la de los otros santuarios (Demeter, pistas, la Muerte): la puerta iluminada de los Creyentes;
  - puerta_mundo y puerta_mundo_<mundo>: el marco de las puertas de los mundos en el castillo, una fachada de Hollow
    Knight que destaque en el escenario (elegidas por el usuario), con el sello de la puerta de jefe en el hueco
    (objetos.py, puerta_mundo_cerrada/media). Cada una según el mundo al que lleva: la cúpula de los Creyentes por
    defecto (la que el usuario prefería), el arco de Sin Ojos al Abismo (7) y al Nido Profundo (8; la de la guarida de
    la Tejedora no convenció) y el Templo del Huevo Negro a los Confines (9, el último);
  - estandarte: a cada lado de la fachada de los mundos, el estandarte rojo con dibujo de la troupe de Grimm (pieza del
    atlas Red_Banners, que viene sin recortar; lo pidió el usuario);
  - arco y columna: el marco de antes, para quien no tenga puerta_mundo.
"""
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/decorados"

# nombre → (textura, medida en px del original, "ancho" o "alto"[, caja en el atlas]); "atlas/…" sale de atlas_sprites
PIECES = {
    "lapida": ("dream_graves_0000_4.png", 18, "ancho"),
    "lapida_palabra": ("Lore_0017_city_01.png", 26, "ancho"),
    "puerta_tienda": ("cd_miner_cave.png", 40, "ancho"),
    "puerta_tienda_cristal": ("cd_miner_cave2.png", 30, "ancho"),
    "puerta_santuario": ("Shrine_of_Believers_Ext_door.png", 30, "alto"),
    "puerta_mundo": ("Shrine_of_Believers_Ext.png", 64, "ancho"),
    "puerta_mundo_7": ("No_eyes_temple_0000_6.png", 52, "ancho"),
    "puerta_mundo_8": ("No_eyes_temple_0000_6.png", 52, "ancho"),
    "puerta_mundo_9": ("temple_ext_01.png", 58, "ancho"),
    "estandarte": ("atlas/Red_Banners.png", 34, "alto", (240, 142, 483, 960)),
    "arco": ("big_arch_full.png", 56, "ancho"),
    "columna": ("Ruins_gate_0000_a.png", 40, "alto"),
}


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    for name, (tex, size, side, *box) in PIECES.items():
        folder = "atlas_sprites" if tex.startswith("atlas/") else "otras"
        im = Image.open(src / "hollow-knight-texturas" / folder / tex.removeprefix("atlas/")).convert("RGBA")
        if box:
            im = im.crop(box[0])
        bb = im.getchannel("A").point(lambda a: 255 if a > 8 else 0).getbbox()
        if bb:
            im = im.crop(bb)
        k = size * 8 / (im.width if side == "ancho" else im.height)
        im.resize((max(1, round(im.width * k)), max(1, round(im.height * k))), Image.LANCZOS).save(OUT / f"{name}.png")
    print(f"decorados: {len(PIECES)} piezas → {OUT}")
