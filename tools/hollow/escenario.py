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
  - detalle: cosas pequeñas encima del suelo (bases de columna, huesos).
"""
from pathlib import Path

from PIL import Image

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
    print(f"escenario: {n} piezas de los Cruces Olvidados → {OUT}")
