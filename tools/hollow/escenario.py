"""Escenario de Hollow Knight para las salas de Maze of Galious (Visuals/HollowScenery en Unity).

Las salas de Hollow Knight están montadas con piezas sueltas (rocas, bordes de suelo, techos con estalactitas, paredes,
arcos y estatuas de fondo) colocadas encima de una colisión invisible. Aquí se cogen las de los Cruces Olvidados
(hollow-knight-crossroads/atlas_Crossroads, ya recortadas; piezas.csv dice cuántas veces se usan) y se guardan a la
escala del Caballero (×1,375: 1 px de Hollow Knight = 1,375 texels, 8 texels por píxel del original), clasificadas por
su papel. HollowScenery las coloca sobre la colisión de cada sala del original: el sólido se pinta negro, como el
interior de la roca en Hollow Knight, y las piezas visten sus bordes.

Sale a Assets/Art/Resources/Hollow/escenario/<papel>__<pieza>.png, fuera de git como el resto del arte de Team Cherry.
Papeles:
  - suelo: el borde de arriba de un sólido (donde se pisa);
  - techo: el borde de abajo (lo que cuelga);
  - estalactita: algo más largo que cuelga de un techo, de vez en cuando;
  - pared_izq / pared_der: el lado de un sólido con aire a la izquierda / a la derecha;
  - masa: rocas grandes para dentro de los macizos anchos (se ven por los bordes);
  - fondo: arcos, estatuas y rocas lejanas, detrás de todo y apagados;
  - detalle: cosas pequeñas encima del suelo (bases de columna, huesos);
  - pp_abajo / pp_arriba: primer plano, por delante de los personajes, en negro (lo pinta así HollowScenery), asomando
    desde el borde de abajo o colgando del de arriba: la profundidad de Team Cherry (siluetas negras delante del juego).
Y el fondo más lejano: lejano__<zona>_NN.png, trozos de salas enteras de Hollow Knight de la zona (hollow-knight-mundos,
ya pintadas) del tamaño de una sala del original a la escala del Caballero, desenfocados; HollowScenery los apaga.
"""
from pathlib import Path

from PIL import Image, ImageFilter

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/escenario"
SCALE = 1.375   # la del Caballero
KIT = "hollow-knight-crossroads"

# papel → piezas (atlas/nombre). Las de pared con _l/_r se usan cada una en su lado; las demás se voltean.
PIECES = {
    "suelo": ["atlas_Crossroads/floor_layered_0000_04", "atlas_Crossroads/floor_layered_0001_03",
              "atlas_Crossroads/floor_layered_0002_02", "atlas_Crossroads/floor_layered_0003_01",
              "atlas_Crossroads/highway_01_layered_0003_Layer-0", "atlas_Crossroads/highway_01_layered_0001_Layer-4"],
    "techo": ["atlas_Crossroads/roof_03"],
    "estalactita": ["atlas_Crossroads/roof_FG_rock_08_0000_06", "atlas_Crossroads/roof_FG_rock_08_0005_01",
                    "atlas_Crossroads/roof_FG_rock_07_0005_01", "atlas_Crossroads/roof_FG_rock_08_0001_05"],
    "pared_izq": ["atlas_Crossroads/wall_07_0000_04", "atlas_Crossroads/wall_07_0002_02", "atlas_Crossroads/wall_01",
                  "atlas_Crossroads/wall_black_l"],
    "pared_der": ["atlas_Crossroads/wall_black_r"],
    "masa": ["atlas_Crossroads/cd_FG_rock_20", "atlas_Crossroads/Mines_Layered_0020_a",
             "atlas_Crossroads/Mines_Layered_0021_a", "atlas_Crossroads/roof_FG_rock_07_0002_04"],
    "fondo": ["atlas_Crossroads/cd_archway_01", "atlas_Crossroads/cd_archway_02", "atlas_Crossroads/cd_archway_03",
              "atlas_Crossroads/statue_02", "atlas_Crossroads/statue_06", "atlas_Crossroads/statue_07",
              "atlas_Crossroads/cd_pillar_01", "atlas_Crossroads/tut_BG_set_01_0000_05",
              "atlas_Crossroads/tut_BG_set_02_0004_02", "atlas_Crossroads/tut_BG_set_02_0000_06",
              "atlas_Crossroads_Eggs/BG_egg_07_0000_Layer-1", "atlas_Crossroads/cd_spikes_BG"],
    "pp_abajo": ["atlas_Crossroads/floor_FG_rock_03_0004_01", "atlas_Crossroads/floor_FG_rock_03_0003_02",
                 "atlas_Crossroads/floor_FG_rock_03_0002_03", "atlas_Crossroads/floor_FG_rock_03_0001_04"],
    "pp_arriba": ["atlas_Crossroads/roof_FG_rock_08_0004_02", "atlas_Crossroads/roof_FG_rock_08_0002_04",
                  "atlas_Crossroads/roof_FG_rock_07_0003_03", "atlas_Crossroads/roof_FG_rock_07_0001_05",
                  "atlas_Crossroads/roof_FG_rock_09_0001_01"],
    "detalle": ["atlas_Crossroads/cd_archbase_01", "atlas_Crossroads/BG_bone_01", "atlas_Crossroads/BG_bone_03",
                "atlas_Crossroads/BG_bone_05"],
}


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    n = 0
    for role, names in PIECES.items():
        for name in names:
            p = src / KIT / f"{name}.png"
            if not p.exists():
                raise SystemExit(f"No está la pieza {p}")
            im = Image.open(p).convert("RGBA")
            bb = im.getchannel("A").point(lambda a: 255 if a > 8 else 0).getbbox()
            if bb:
                im = im.crop(bb)
            im = im.resize((max(1, round(im.width * SCALE)), max(1, round(im.height * SCALE))), Image.LANCZOS)
            im.save(OUT / f"{role}__{Path(name).name}.png")
            n += 1
    far = build_far(src, "Crossroads", "cruces")
    print(f"escenario: {n} piezas de los Cruces Olvidados y {far} fondos lejanos → {OUT}")


# Una sala del original (256 × 160 px) a la escala del Caballero: 256 · 8 / 1,375 ≈ 1490 px de Hollow Knight.
FAR_W, FAR_H = 1490, 931
FAR_OUT = (512, 320)


# Fondos lejanos elegidos a mano (sala de Hollow Knight, esquina del trozo en su versión plana), de una hoja de candidatos:
# cuevas con arcos, niebla y columnas, sin personajes, larvas, ácido ni primeros planos enormes.
FAR = {
    "cruces": ("Crossroads", [("Crossroads_13", 2693, 2302), ("Crossroads_39", 3483, 822), ("Crossroads_10", 2428, 3672),
                              ("Crossroads_01", 1495, 1819), ("Crossroads_43", 2075, 421), ("Crossroads_12", 668, 357),
                              ("Crossroads_31", 1579, 548), ("Crossroads_40", 2352, 1657), ("Crossroads_16", 1622, 543),
                              ("Crossroads_48", 676, 1162)]),
}


def build_far(src, zone, name):
    """Los trozos de FAR de esa zona, del tamaño de una sala del original a la escala del Caballero, desenfocados."""
    n = 0
    for room, x, y in FAR[name][1]:
        p = src / "hollow-knight-mundos" / zone / room / f"{room}_plana.jpg"
        im = Image.open(p).convert("RGB").crop((x, y, x + FAR_W, y + FAR_H)).resize(FAR_OUT, Image.LANCZOS)
        im.filter(ImageFilter.GaussianBlur(2)).save(OUT / f"lejano__{name}_{n:02d}.png")
        n += 1
    return n
