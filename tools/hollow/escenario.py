"""Escenario de Hollow Knight para las salas de Maze of Galious (Visuals/HollowScenery en Unity).

Las salas de Hollow Knight están montadas con piezas sueltas (rocas, bordes de suelo, techos con estalactitas, paredes,
arcos y estatuas de fondo) colocadas encima de una colisión invisible. Aquí se cogen las de los Cruces Olvidados
(hollow-knight-crossroads/atlas_Crossroads, ya recortadas; piezas.csv dice cuántas veces se usan) y se guardan a la
escala del Caballero (×1,375: 1 px de Hollow Knight = 1,375 texels, 8 texels por píxel del original), clasificadas por
su papel. HollowScenery las coloca sobre la colisión de cada sala del original: el sólido se pinta negro, como el
interior de la roca en Hollow Knight, y las piezas visten sus bordes.

Un kit por zona de Hollow Knight (KITS; cada mundo del original lleva una, la tabla está en Visuals/HollowScenery):
sale a Assets/Art/Resources/Hollow/escenario/<zona>/<papel>__<pieza>.png, fuera de git como el resto del arte de Team
Cherry. Las piezas salen de hollow-knight-crossroads/atlas_<atlas>/ (ya recortadas) o, para las zonas que no están ahí,
de su caja en un atlas sin recortar de hollow-knight-texturas/atlas_sprites (las cajas salen de la hoja numerada de
atlas_piezas.py; se guarda la mancha más grande de la caja).
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

import atlas_piezas

Image.MAX_IMAGE_PIXELS = None   # las salas enteras de Hollow Knight pasan de 90 megapíxeles

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/escenario"
SCALE = 1.375   # la del Caballero
KIT = "hollow-knight-crossroads"

GG = "GG_Waterway.png"          # el del Palacio de los dioses, con las piezas de los Canales
SLUICE = "Water_Sluice.png"
HIVE = "Hive.png"


def A(atlas, box, name):
    """Una pieza por su caja en un atlas sin recortar. Con atlas None, 'box' es una pieza recortada con otro nombre (la
    misma dos veces sale el doble)."""
    return (atlas, box, name)


# zona → papel → piezas (atlas/nombre, o A(atlas, caja, nombre)). Las de pared con _l/_r se usan cada una en su lado; las demás se voltean.
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
    # Mundo 2 (arrecife): los Canales Reales. Escombros por suelo, un borde que gotea por techo, columnas y muros de
    # ladrillo de la esclusa por pared; rejas redondas de desagüe, tuberías, cadenas y el gran arco al fondo.
    "canales": {
        "suelo": [A(GG, (1166, 388, 1482, 476), "escombros_a"), A(GG, (372, 496, 632, 588), "escombros_b"),
                  "atlas_Crossroads/highway_01_layered_0003_Layer-0", "atlas_Crossroads/highway_01_layered_0001_Layer-4"],
        "techo": ["atlas_Crossroads/roof_03", A(None, "atlas_Crossroads/roof_03", "roof_03_b"), A(GG, (988, 98, 1306, 176), "borde_goteando")],
        "estalactita": [A(GG, (1464, 114, 1528, 552), "gota_larga")],
        "pared_izq": [A(SLUICE, (416, 250, 492, 512), "ladrillo_columna_a"), A(SLUICE, (498, 250, 572, 512), "ladrillo_columna_b")],
        "pared_der": [A(SLUICE, (218, 252, 412, 512), "ladrillo_muro")],
        "masa": ["atlas_Crossroads/Mines_Layered_0020_a", A(GG, (736, 50, 990, 362), "desague_a"),
                 A(GG, (1160, 592, 1382, 898), "desague_b")],
        "fondo": [A("waterways.png", (10, 98, 704, 502), "arco"), A("waterways.png", (523, 36, 1003, 331), "reja_media"),
                  A(GG, (736, 50, 990, 362), "desague_a"), A(GG, (1160, 592, 1382, 898), "desague_b"),
                  A(GG, (1844, 472, 2030, 552), "tuberia"), A(GG, (120, 6, 414, 498), "cadenas")],
        "pp_abajo": [A(GG, (1166, 388, 1482, 476), "escombros_a"),
                     A(GG, (372, 496, 632, 588), "escombros_b")],
        "pp_arriba": [A(GG, (120, 6, 414, 498), "cadenas"), A(GG, (930, 264, 1052, 752), "tubos"),
                      A(GG, (988, 98, 1306, 176), "borde_goteando")],
        "detalle": [A(GG, (1568, 850, 1696, 984), "tarro"), A(GG, (1704, 890, 1832, 1022), "barril"),
                    A(GG, (640, 450, 750, 566), "piedras")],
    },
    # Mundo 3 (guarida ámbar): la Colmena. Bordes con miel por suelo, goterones de miel por techo, columnas y paneles
    # hexagonales por pared; marcos hexagonales, columnas de cristal y manchas de luz ámbar al fondo.
    "colmena": {
        "suelo": [A(HIVE, (1196, 568, 1664, 680), "borde_a"), A(HIVE, (1220, 680, 1708, 796), "borde_b"),
                  A(HIVE, (2268, 1260, 2588, 1312), "borde_c"), A(HIVE, (2424, 1656, 2744, 1712), "borde_d"),
                  A(HIVE, (2924, 1784, 3196, 1816), "borde_e"), A(HIVE, (2564, 1992, 2940, 2048), "borde_f")],
        "techo": [A(HIVE, (248, 1052, 824, 1208), "miel_goteando"), A(HIVE, (2196, 1884, 2564, 1964), "miel_a"),
                  A(HIVE, (2196, 1968, 2564, 2048), "miel_b"), A(HIVE, (1204, 1120, 1460, 1268), "goteron")],
        "estalactita": [A(HIVE, (700, 1184, 988, 1704), "miel_larga"), A(HIVE, (940, 1528, 1232, 2048), "miel_negra"),
                        A(HIVE, (980, 992, 1116, 1404), "chorro")],
        "pared_izq": [A(HIVE, (616, 696, 812, 1048), "hexagonos"), A(HIVE, (2092, 1704, 2188, 2044), "columna_hex"),
                      A(HIVE, (1528, 1052, 1728, 1748), "columna_miel"), A(HIVE, (1236, 1652, 1432, 1892), "hex_roto")],
        "masa": [A(HIVE, (1152, 1292, 1504, 1520), "panal_a"), A(HIVE, (388, 1740, 920, 2000), "panal_b"),
                 A(HIVE, (172, 812, 488, 1048), "panal_roto")],
        "fondo": [A(HIVE, (200, 248, 420, 572), "marco_hex_a"), A(HIVE, (992, 488, 1188, 776), "marco_hex_b"),
                  A(HIVE, (1556, 4, 1708, 268), "cristal_a"), A(HIVE, (1880, 860, 2024, 1116), "cristal_b"),
                  A(HIVE, (1944, 1788, 2088, 2044), "cristal_c"), A(HIVE, (12, 1364, 324, 2036), "vitrina"),
                  A(HIVE, (312, 1224, 700, 1616), "luz_a"), A(HIVE, (1184, 796, 1420, 1008), "luz_b"),
                  A(HIVE, (296, 576, 612, 816), "paneles")],
        "pp_abajo": [A(HIVE, (1180, 1752, 1672, 2048), "ladera"), A(HIVE, (584, 208, 868, 348), "bloque"),
                     A(HIVE, (2412, 1068, 2612, 1132), "losa")],
        "pp_arriba": [A(HIVE, (1668, 492, 1880, 684), "colgajo"), A(HIVE, (940, 1528, 1232, 2048), "miel_negra"),
                      A(HIVE, (1204, 1120, 1460, 1268), "goteron")],
        # Los trozos de miel brillan como un objeto: uno solo, entre piedras.
        "detalle": [A(HIVE, (2472, 1552, 2592, 1652), "trozo_miel"), A(HIVE, (2592, 1568, 2740, 1652), "piedra"),
                    A(None, A(HIVE, (2592, 1568, 2740, 1652), "x"), "piedra_b")],
    },
    # Mundo 4 (cristal violeta): los Picos de Cristal. Piedras y tablones de mina por suelo, paredes de roca con puntales
    # de madera (cada una con su versión para el otro lado), cristales violetas, arcos de roca, ruedas, cestas y andamios
    # al fondo; cristales oscuros como siluetas delante.
    "cristal": {
        "suelo": ["otras/mine_floor_01", "otras/mine_loose_stones", "otras/mines_floor_rocks_0003_Layer-1",
                  "otras/mines_metal_thin_floors_0001_a", "otras/mines_metal_thin_floors_0004_a"],
        "techo": ["atlas_Crossroads/roof_03", "otras/Mines_Layered_0080_a", "otras/Mines_Layered_0081_a"],
        "estalactita": ["atlas_Crossroads/roof_FG_rock_08_0000_06", "atlas_Crossroads/roof_FG_rock_08_0005_01",
                        "otras/mines_floating_crystals"],
        "pared_izq": ["otras/mine_wall_01", "otras/mine_wall_02", "otras/mine_wall_03"],
        "pared_der": ["otras/mine_wall_01_r", "otras/mine_wall_02_r", "otras/mine_wall_03_r"],
        "masa": ["otras/mines_additions_02_0004_a", "otras/Mines_Layered_0090_a", "otras/cryst_cave_0004_Layer-6"],
        "fondo": ["otras/Mines_Layered_0087_c", "otras/Mines_Layered_0088_c", "otras/Mines_Layered_0089_c",
                  "otras/cryst_cave_0002_Layer-3", "otras/mine_spike_fg_clear", "otras/mine_strut_07_BG",
                  "otras/mine_wood_back", "otras/mines_additions_03_0001_a", "otras/mines_flip_platform_BG"],
        "pp_abajo": ["otras/Mines_Layered_0088_c", "otras/Mines_Layered_0089_c", "otras/mines_quake_floor"],
        "pp_arriba": ["atlas_Crossroads/roof_FG_rock_08_0004_02", "atlas_Crossroads/roof_FG_rock_07_0003_03"],
        "detalle": ["otras/mine_crystal_08", "otras/mines_additions_02_0010_a"],   # sin los rosas brillantes de la cima
    },
}


def load_piece(src, name):
    if isinstance(name, tuple) and name[0] is None:
        return load_piece(src, name[1])
    elif isinstance(name, tuple):
        atlas, box, _ = name
        return atlas_piezas.piece(Image.open(src / "hollow-knight-texturas/atlas_sprites" / atlas).convert("RGBA"), box)
    # Texturas sueltas (hollow-knight-texturas/otras: las de los Picos de Cristal, por ejemplo).
    p = src / "hollow-knight-texturas" / f"{name}.png" if name.startswith("otras/") else src / KIT / f"{name}.png"
    if not p.exists():
        raise SystemExit(f"No está la pieza {p}")
    return Image.open(p).convert("RGBA")


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
                im = load_piece(src, name)
                bb = im.getchannel("A").point(lambda a: 255 if a > 8 else 0).getbbox()
                if bb:
                    im = im.crop(bb)
                im = im.resize((max(1, round(im.width * SCALE)), max(1, round(im.height * SCALE))), Image.LANCZOS)
                label = name[2] if isinstance(name, tuple) else Path(name).name
                im.save(out / f"{role}__{label}.png")
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
    "canales": ("Waterways", [("Waterways_01", 8774, 566), ("Waterways_01", 8675, 1647), ("Waterways_01", 7698, 1966),
                              ("Waterways_09", 1514, 605), ("Waterways_09", 1127, 634), ("Waterways_04", 5157, 2865),
                              ("Waterways_04b", 4825, 2305), ("Waterways_04b", 7010, 1069), ("Waterways_04b", 2302, 2144),
                              ("Waterways_04", 6137, 2619)]),
    "colmena": ("Hive", [("Hive_02", 6884, 907), ("Hive_04", 6754, 2039), ("Hive_01", 4856, 2613), ("Hive_01", 5597, 2346),
                         ("Hive_05", 3319, 2429), ("Hive_05", 5095, 2548), ("Hive_05", 5424, 2625),
                         ("Hive_03_c", 3777, 2249), ("Hive_05", 4081, 2061), ("Hive_01", 6491, 3662)]),
    "cristal": ("Mines", [("Mines_02", 6027, 2711), ("Mines_06", 7727, 997), ("Mines_30", 5899, 937), ("Mines_31", 5297, 1999),
                          ("Mines_17", 1276, 783), ("Mines_10", 7115, 1127), ("Mines_13", 8706, 1822),
                          ("Mines_01", 374, 1263), ("Mines_18", 1654, 1338), ("Mines_29", 1830, 3322)]),
}


def build_far(src, name, out):
    """Los trozos de FAR de esa zona, del tamaño de una sala del original a la escala del Caballero, desenfocados."""
    zone, picks = FAR.get(name, (None, []))
    for n, (room, x, y) in enumerate(picks):
        p = src / "hollow-knight-mundos" / zone / room / f"{room}_plana.jpg"
        im = Image.open(p).convert("RGB").crop((x, y, x + FAR_W, y + FAR_H)).resize(FAR_OUT, Image.LANCZOS)
        im.filter(ImageFilter.GaussianBlur(2)).save(out / f"lejano__{n:02d}.png")
    return len(picks)
