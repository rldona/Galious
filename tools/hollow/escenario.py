"""Escenario de Hollow Knight para las salas de Maze of Galious (Visuals/HollowScenery en Unity).

Las salas de Hollow Knight están montadas con piezas sueltas (rocas, bordes de suelo, techos con estalactitas, paredes,
arcos y estatuas de fondo) colocadas encima de una colisión invisible. Aquí se cogen las de los Cruces Olvidados
(hollow-knight-crossroads/atlas_Crossroads, ya recortadas; piezas.csv dice cuántas veces se usan) y se guardan a la
escala del Caballero (×1,375: 1 px de Hollow Knight = 1,375 texels, 8 texels por píxel del original), clasificadas por
su papel. HollowScenery las coloca sobre la colisión de cada sala del original: el sólido se pinta negro, como el
interior de la roca en Hollow Knight, y las piezas visten sus bordes.

Un kit por zona de Hollow Knight (KITS; cada mundo del original lleva una, la tabla está en Visuals/HollowScenery):
sale a Assets/Art/Resources/Hollow/escenario/<zona>/<papel>__<pieza>.png, fuera de git como el resto del arte de Team
Cherry. Las piezas salen de hollow-knight-crossroads/atlas_<atlas>/ (ya recortadas).
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
Y el fondo más lejano: lejano__NN.png, trozos de salas enteras de Hollow Knight de la zona (hollow-knight-mundos,
ya pintadas) del tamaño de una sala del original a la escala del Caballero, desenfocados; HollowScenery los apaga.
"""
from pathlib import Path

from PIL import Image, ImageFilter

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/escenario"
SCALE = 1.375   # la del Caballero
KIT = "hollow-knight-crossroads"

# zona → papel → piezas (atlas/nombre). Las de pared con _l/_r se usan cada una en su lado; las demás se voltean.
KITS = {
    # Castillo: los Cruces Olvidados.
    "cruces": {
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
    },
    # Mundo 1 (cueva verde agua): Greenpath. Suelos de musgo y plataformas con goteo, enredaderas colgando, arbustos con
    # luces y raíces en arco al fondo, hierba encima del suelo. No tiene trozos de pared: sus rocas con musgo.
    "greenpath": {
        "suelo": ["atlas_Green/fung_floor1_0000_Layer-1", "atlas_Green/fung_floor1_0001_Layer-2",
                  "atlas_Green/fung_floor1_0002_Layer-0", "atlas_Green/fung_FG_03"],
        "techo": ["atlas_Green/fungd_roof_vine_01_0000_a", "atlas_Green/fungd_roof_vine_01_0002_a",
                  "atlas_Green/fungd_roof_vine_01_0003_a", "atlas_Green/fungd_vine_02", "atlas_Green/fung_vine_cut000"],
        "estalactita": ["atlas_Green/fungd_vine_01_0000_a", "atlas_Green/fungd_vine_01_0001_a",
                        "atlas_Green/fungd_vine_02_0000_Layer-1", "atlas_Green/fungd_vine_02_0001_Layer-4",
                        "atlas_Green/fungd_vine_02_0003_Layer-2"],
        "pared_izq": ["atlas_Green/fungd_floor_rock_03_0001_a", "atlas_Green/fungd_floor_rock_03_0002_a"],
        "masa": ["atlas_Green/fung_bg_rocks_full_01", "atlas_Green/fung_new_0088_a", "atlas_Green/fung_new_0089_a",
                 "atlas_Green/fung_mid_bush"],
        "fondo": ["atlas_Green/BG_bush_set", "atlas_Green/fung_new_0127_02", "atlas_Green/fung_new_0128_01",
                  "atlas_Green/fung_new_0133_Layer-236", "atlas_Green/fung_new_0065_Layer-293",
                  "atlas_Green/fung_new_0071_Layer-277", "atlas_Green/fung_root_04", "atlas_Green/fung_root_05",
                  "atlas_Green/fungd_vine_03"],
        "pp_abajo": ["atlas_Green/fung_new_0127_02", "atlas_Green/fung_new_0128_01", "atlas_Green/fung_new_0132_Layer-237",
                     "atlas_Green/fung_new_0086_a"],
        "pp_arriba": ["atlas_Green/fung_FG_hanging_01", "atlas_Green/fungd_vine_02", "atlas_Green/fungd_roof_vine_01_0004_a"],
        "detalle": ["atlas_Green/grass_01_idle0000", "atlas_Green/grass_03_idle0000", "atlas_Green/simple_grass0000"],
    },
}


def build(src):
    total = 0
    for zone, roles in KITS.items():
        out = OUT / zone
        out.mkdir(parents=True, exist_ok=True)
        for old in out.glob("*.png"):
            old.unlink()
        n = 0
        for role, names in roles.items():
            for name in names:
                p = src / KIT / f"{name}.png"
                if not p.exists():
                    raise SystemExit(f"No está la pieza {p}")
                im = Image.open(p).convert("RGBA")
                bb = im.getchannel("A").point(lambda a: 255 if a > 8 else 0).getbbox()
                if bb:
                    im = im.crop(bb)
                im = im.resize((max(1, round(im.width * SCALE)), max(1, round(im.height * SCALE))), Image.LANCZOS)
                im.save(out / f"{role}__{Path(name).name}.png")
                n += 1
        far = build_far(src, zone, out)
        print(f"escenario {zone}: {n} piezas y {far} fondos lejanos")
        total += n
    print(f"escenario: {len(KITS)} zonas → {OUT}")


# Una sala del original (256 × 160 px) a la escala del Caballero: 256 · 8 / 1,375 ≈ 1490 px de Hollow Knight.
FAR_W, FAR_H = 1490, 931
FAR_OUT = (512, 320)


# Fondos lejanos elegidos a mano (carpeta de la zona en hollow-knight-mundos, sala, esquina del trozo en su versión plana),
# de una hoja de candidatos: sin personajes, larvas, ácido ni primeros planos enormes.
FAR = {
    "cruces": ("Crossroads", [("Crossroads_13", 2693, 2302), ("Crossroads_39", 3483, 822), ("Crossroads_10", 2428, 3672),
                              ("Crossroads_01", 1495, 1819), ("Crossroads_43", 2075, 421), ("Crossroads_12", 668, 357),
                              ("Crossroads_31", 1579, 548), ("Crossroads_40", 2352, 1657), ("Crossroads_16", 1622, 543),
                              ("Crossroads_48", 676, 1162)]),
    "greenpath": ("Fungus1", [("Fungus1_36", 493, 2333), ("Fungus1_20_v02", 2211, 900), ("Fungus1_02", 1860, 3484),
                              ("Fungus1_25", 923, 611), ("Fungus1_23", 7719, 568), ("Fungus1_03", 3186, 873),
                              ("Fungus1_24", 2183, 2475), ("Fungus1_29", 2914, 1930), ("Fungus1_32", 1677, 1793),
                              ("Fungus1_06", 871, 707)]),
}


def build_far(src, name, out):
    """Los trozos de FAR de esa zona, del tamaño de una sala del original a la escala del Caballero, desenfocados."""
    zone, picks = FAR.get(name, (None, []))
    for n, (room, x, y) in enumerate(picks):
        p = src / "hollow-knight-mundos" / zone / room / f"{room}_plana.jpg"
        im = Image.open(p).convert("RGB").crop((x, y, x + FAR_W, y + FAR_H)).resize(FAR_OUT, Image.LANCZOS)
        im.filter(ImageFilter.GaussianBlur(2)).save(out / f"lejano__{n:02d}.png")
    return len(picks)
