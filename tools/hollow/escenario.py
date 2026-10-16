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
ABYSS = "Abyss.png"
DEEP = "Deepnest.png"
DUNES, WYRM, OUTLOOK = "Dunes.png", "Wyrm.png", "Outlook.png"


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
    # Mundo 5 (ruinas hundidas): la Ciudad de las Lágrimas. Plataformas adornadas y suelos de casa, arcos de puente
    # colgando, muros finos y pilares oscuros; ventanales, edificios, agujas, la estatua del vigilante y estanterías al
    # fondo; escombros y cojines como siluetas delante.
    "ciudad": {
        "suelo": ["otras/ruind_plat_float_01", "otras/ruind_plat_float_01_wide", "otras/ruind_plat_float_01_superwide",
                  "otras/ruinhouse_floor", "otras/ruins_additions_0035_a", "otras/ruins_additions_0036_a"],
        "techo": ["otras/ruind_bridge_roof_arch_01", "otras/ruind_bridge_base_arch_02", "otras/ruind_bridge_base_01",
                  "atlas_Crossroads/roof_03"],
        "estalactita": ["otras/ruins_additions_0038_a", "atlas_Crossroads/roof_FG_rock_08_0000_06"],
        "pared_izq": ["otras/ruind_wall_02", "otras/ruind_wall_03", "otras/ruins_dark pillar"],
        "masa": ["otras/ruin_house_brick", "otras/ruin house brick2", "otras/ruins_bg_building_mid"],
        "fondo": ["otras/ruind_window_01", "otras/ruind_window_03", "otras/ruind_window_04", "otras/ruind_dressing_window_01",
                  "otras/ruins_bg_building_mid", "otras/ruins_resting_extra_bits_0084_Layer-186",
                  "otras/ruins_resting_extra_bits_0098_Layer-198", "otras/ruins_fancy_shelf_0000_2",
                  "otras/ruins_breakable_watcher_statue_0004_watcher_statue_01", "otras/ruind_arch_04"],
        "pp_abajo": ["otras/ruins_resting_extra_bits_0044_Layer-146", "otras/ruins_resting_extra_bits_0045_Layer-147",
                     "otras/ruins_red_FG_cushions_0000_v", "otras/ruins_red_FG_cushions_0001_v"],
        "pp_arriba": ["otras/ruins_additions_0002_a", "otras/ruins_additions_0019_a", "otras/ruind_bridge_roof_arch_01"],
        "detalle": ["otras/ruins_breakable_watcher_statue_0000_watcher_statue_debris_01", "otras/ruin_add_pillarend",
                    "otras/ruins_breakable_watcher_statue_0003_watcher_statue_base"],
    },
    # Mundo 6 (bosque de hongos): los Páramos Fúngicos. Setas de puntos por suelo y techo, mascaras de hongo y muros que
    # se rompen por pared; setas grandes, tallos y montones de setas al fondo y como siluetas.
    "fungicos": {
        "suelo": ["atlas_Fung/fungd_mush_01_0001_a", "otras/fungus_crubmle_floor", "otras/shroom_charm_plinth_0001_floor",
                  "otras/ancient_wall_pieces_0000_a_mushroom"],
        "techo": ["atlas_Fung/fung_roof_mush_01_0001_a", "atlas_Fung/fung_new_mush_layered_03_0006_a",
                  "atlas_Fung/fung_new_mush_layered_03_0003_a"],
        "estalactita": ["otras/fung_pod_01", "otras/fung_pod_02", "otras/fung_mask_03"],
        "pared_izq": ["otras/break_wall_fung", "otras/fung_break_wall_02", "otras/fung_new_0014_Layer-335"],
        "masa": ["atlas_Fung/fung_new_0007_m", "atlas_Fung/fung_new_0005_m", "otras/mr_mush_riddle_room_0003_Layer-8"],
        "fondo": ["atlas_Fung/fung_new_0002_m", "atlas_Fung/fung_new_0004_m", "atlas_Fung/fung_new_0010_m",
                  "otras/ruin_mush_BG_single", "otras/fung_mush_01", "otras/mr_mush_riddle_room_0002_Layer-7",
                  "atlas_Fung/fung_new_mush_layered_03_0004_a", "otras/fung_mask_02"],
        "pp_abajo": ["atlas_Fung/fung_new_0005_m", "atlas_Fung/fung_new_0010_m", "otras/fung_mask_04"],
        "pp_arriba": ["atlas_Fung/fung_roof_mush_01_0001_a", "otras/fung_mask_02", "otras/giraffe_masks_0001_fung_new_0133_Layer-236"],
        "detalle": ["atlas_Fung/mush_010000", "atlas_Fung/mush_020000", "atlas_Fung/mush_050000",
                    "atlas_Fung/fungd_mush_09_0004_a"],
    },
    # Mundo 7 (abismo luminoso): el Abismo. Bordes de concha por suelo, cortinas negras que gotean por techo, columnas
    # de concha por pared; caparazones, conchas en espiral y la reja del faro al fondo; mascaras de los recipientes
    # vacíos tiradas por el suelo.
    "abismo": {
        "suelo": [A(ABYSS, (2288, 880, 2556, 996), "borde_a"), A(ABYSS, (2508, 996, 2756, 1060), "borde_b"),
                  A(ABYSS, (2836, 1136, 3084, 1200), "borde_c"), A(ABYSS, (1156, 1908, 1552, 2048), "borde_d"),
                  A(ABYSS, (2332, 1988, 2588, 2048), "borde_e")],
        "techo": [A(ABYSS, (2220, 648, 2304, 904), "cortina_a"), A(ABYSS, (2660, 1056, 2744, 1308), "cortina_b"),
                  A(ABYSS, (3044, 1792, 3132, 2048), "cortina_c"), A(ABYSS, (620, 1676, 708, 1940), "cortina_d")],
        "estalactita": [A(ABYSS, (1504, 1204, 1588, 1452), "goteo_a"), A(ABYSS, (3144, 1780, 3228, 2048), "goteo_b"),
                        A(ABYSS, (1864, 448, 1944, 704), "goteo_c")],
        "pared_izq": [A(ABYSS, (2088, 444, 2216, 700), "columna_a"), A(ABYSS, (2820, 968, 2908, 1132), "columna_b"),
                      A(ABYSS, (2924, 1792, 3040, 2048), "columna_c"), A(ABYSS, (852, 1244, 1152, 2048), "columna_d")],
        "masa": [A(ABYSS, (4, 660, 784, 1028), "caparazon_a"), A(ABYSS, (1144, 1484, 1688, 1868), "caparazon_b"),
                 A(ABYSS, (1748, 700, 2004, 964), "flor")],
        "fondo": [A(ABYSS, (560, 172, 820, 432), "espiral_a"), A(ABYSS, (816, 260, 1096, 476), "espiral_b"),
                  A(ABYSS, (0, 380, 316, 652), "concha"), A(ABYSS, (788, 484, 1100, 920), "caparazon_c"),
                  A(ABYSS, (0, 1620, 224, 1972), "reja"), A(ABYSS, (2068, 1668, 2360, 1848), "cupula"),
                  A(ABYSS, (1204, 1248, 1500, 1480), "cascara")],
        "pp_abajo": [A(ABYSS, (1288, 180, 1576, 376), "roca"), A(ABYSS, (3232, 1924, 3520, 2048), "loma"),
                     A(ABYSS, (2068, 1668, 2360, 1848), "cupula")],
        "pp_arriba": [A(ABYSS, (2604, 1316, 2796, 1500), "garra"), A(ABYSS, (2492, 1372, 2636, 1628), "puas"),
                      A(ABYSS, (2660, 1056, 2744, 1308), "cortina_b")],
        # Las máscaras blancas de los recipientes parecen caballeros: una, entre conchas pequeñas.
        "detalle": [A(ABYSS, (856, 124, 972, 256), "mascara"), A(ABYSS, (2268, 536, 2412, 620), "concha_a"),
                    A(ABYSS, (2608, 892, 2724, 992), "concha_b"), A(ABYSS, (1448, 1112, 1552, 1196), "concha_c"),
                    A(ABYSS, (716, 1232, 828, 1340), "concha_d"), A(ABYSS, (3044, 1616, 3184, 1720), "concha_e")],
    },
    # Mundo 8 (magma; Hollow Knight no tiene volcán): el Nido Profundo, lo más oscuro y peligroso. Bordes de roca y de
    # capullos por suelo, bordes que gotean y telarañas por techo, garras y colmillos colgando, capullos y raíces por
    # pared; ovillos, telarañas y la puerta de la Ciudad de las Arañas al fondo; setas blancas sobre el suelo.
    "nido": {
        "suelo": [A(DEEP, (844, 40, 1168, 140), "borde_roca"), A(DEEP, (0, 168, 356, 276), "capullos_a"),
                  A(DEEP, (544, 1168, 936, 1288), "capullos_b")],
        "techo": [A(DEEP, (436, 1296, 928, 1464), "borde_goteando"), A(DEEP, (1660, 232, 1816, 480), "ovillo_colgando"),
                  "atlas_Crossroads/roof_03"],   # la telaraña blanca, en todos los techos, era demasiado
        "estalactita": [A(DEEP, (880, 260, 1024, 600), "garra_a"), A(DEEP, (580, 760, 724, 1172), "garra_b"),
                        A(DEEP, (1556, 888, 1692, 1292), "garra_c"), A(DEEP, (1348, 1688, 1488, 2276), "colmillo")],
        "pared_izq": [A(DEEP, (1512, 320, 1680, 588), "capullos_pared"), A(DEEP, (920, 1408, 1056, 1980), "raices")],
        "masa": [A(DEEP, (464, 416, 736, 600), "nido_a"), A(DEEP, (1028, 404, 1308, 548), "nido_b"),
                 A(DEEP, (0, 1080, 364, 1364), "racimo")],
        "fondo": [A(DEEP, (1508, 1616, 1660, 1956), "puerta"), A(DEEP, (1336, 460, 1520, 692), "anillo"),
                  A(DEEP, (1204, 984, 1460, 1208), "ovillo"), A(DEEP, (1140, 552, 1344, 788), "capullo"),
                  A(DEEP, (160, 648, 408, 932), "telarana_a"), A(DEEP, (8, 612, 128, 1048), "telarana_b"),
                  A(DEEP, (1748, 1736, 1940, 1976), "telarana_c"), A(DEEP, (872, 604, 1096, 860), "racimo_b")],
        "pp_abajo": [A(DEEP, (668, 112, 948, 304), "loma"), A(DEEP, (248, 1664, 532, 1952), "bulto"),
                     A(DEEP, (0, 1836, 424, 2116), "caparazon")],
        "pp_arriba": [A(DEEP, (0, 368, 288, 600), "colgajo"), A(DEEP, (1256, 1400, 1432, 1744), "ovillo_grande"),
                      A(DEEP, (880, 260, 1024, 600), "garra_a")],
        "detalle": [A(DEEP, (1820, 52, 2004, 232), "setas_a"), A(DEEP, (1896, 924, 1960, 1096), "seta"),
                    A(DEEP, (1836, 1284, 1968, 1468), "setas_b"), A(DEEP, (2016, 1816, 2076, 1896), "setas_c")],
    },
    # Mundo 9 (osario): los Confines del Reino, la ceniza blanca sobre los huesos del Wyrm. Dunas de ceniza por suelo,
    # costillas y tendones del Wyrm por techo y pared, colmillos y púas de hueso colgando y en el fondo, pilares y arcos
    # del mirador; hierba de ceniza blanca encima del suelo.
    "confines": {
        "suelo": [A(DUNES, (390, 75, 896, 153), "duna_a"), A(DUNES, (0, 196, 495, 311), "duna_b"),
                  A(DUNES, (6, 97, 437, 190), "duna_c"), A(WYRM, (0, 2604, 436, 2696), "tendon")],
        "techo": ["atlas_Crossroads/roof_03", A(WYRM, (1364, 1860, 1968, 2084), "carne"), A(WYRM, (1240, 1624, 1512, 1824), "hueso_a")],
        "estalactita": ["otras/bone_spike_04", "otras/bone_spike_06", "otras/bone_deep_0028_Layer-152", "otras/bone_deep_0029_Layer-153"],
        "pared_izq": ["otras/east_wall_break_0002_1", "otras/bone_deep_0018_Layer-142", "otras/ruind_wall_edge_01"],
        "masa": [A(WYRM, (440, 604, 908, 1208), "costilla"), A(WYRM, (308, 2524, 1372, 3272), "lomo"),
                 "otras/colosseum_EXT_0004_col_04"],
        "fondo": [A(OUTLOOK, (645, 28, 748, 517), "pilar_a"), A(OUTLOOK, (207, 243, 336, 701), "pilar_b"),
                  A(OUTLOOK, (431, 223, 638, 496), "arco_a"), A(OUTLOOK, (347, 406, 566, 679), "arco_b"),
                  "otras/bone_deep_0027_Layer-151", "otras/bone_deep_0034_Layer-121", "otras/colosseum_EXT_0002_col_06",
                  "otras/colosseum_EXT_0007_col_01", "otras/white_ash_scenery_0021_22"],
        "pp_abajo": [A(DUNES, (312, 267, 1023, 427), "duna_grande"), A(DUNES, (0, 332, 610, 512), "loma"),
                     "otras/FG_bone_pile"],
        "pp_arriba": [A(WYRM, (0, 2712, 456, 3196), "colgajo"), A(WYRM, (976, 3688, 1348, 4092), "garras"),
                      "otras/colosseum_EXT_0003_col_05"],
        "detalle": ["otras/ash_grass1000", "otras/ash_grass_02000", "otras/ash_grass_03000", "otras/bone_spike_03"],
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
                label = (name[2] if isinstance(name, tuple) else Path(name).name).replace(" ", "_")
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
    "ciudad": ("Ruins1", [("Ruins1_09", 277, 1214), ("Ruins1_23", 2616, 2730), ("Ruins1_24", 749, 1448),
                          ("Ruins1_27", 5916, 2708), ("Ruins1_24", 664, 812), ("Ruins1_18", 7433, 1679),
                          ("Ruins1_06", 6011, 2096), ("Ruins1_01", 1219, 540), ("Ruins1_18", 9694, 1493),
                          ("Ruins1_03", 3160, 4513)]),
    "fungicos": ("Fungus2", [("Fungus2_19", 1769, 840), ("Fungus2_06", 1481, 4731), ("Fungus2_09", 1048, 268),
                             ("Fungus2_10", 3927, 3400), ("Fungus2_28", 4893, 1017), ("Fungus2_20", 2148, 1081),
                             ("Fungus2_11", 1824, 3489), ("Fungus2_14", 6723, 888), ("Fungus2_18", 2774, 3610),
                             ("Fungus2_23", 3344, 2587)]),
    "abismo": ("Abyss", [("Abyss_02", 7966, 1347), ("Abyss_02", 8751, 1764), ("Abyss_03", 3681, 819),
                         ("Abyss_03_b", 2380, 498), ("Abyss_21", 5691, 749), ("Abyss_03_c", 3947, 1463),
                         ("Abyss_03_c", 3792, 1263), ("Abyss_22", 1089, 931), ("Abyss_18", 5243, 996),
                         ("Abyss_Lighthouse_room", 707, 91)]),
    "nido": ("Deepnest", [("Deepnest_40", 4870, 3844), ("Deepnest_10", 1543, 3674), ("Deepnest_02", 520, 250),
                          ("Deepnest_Spider_Town", 260, 8349), ("Deepnest_34", 7599, 2277), ("Deepnest_44", 5072, 3224),
                          ("Deepnest_38", 3922, 1287), ("Deepnest_01b", 2267, 3988), ("Deepnest_41", 783, 1082),
                          ("Deepnest_14", 4167, 2094)]),
    "confines": ("Deepnest_East", [("Deepnest_East_12", 2212, 1459), ("Deepnest_East_12", 6272, 1548),
                                   ("Deepnest_East_14", 4545, 6204), ("Deepnest_East_04", 1525, 1338),
                                   ("Deepnest_East_06", 5763, 344), ("Deepnest_East_06", 8761, 523),
                                   ("Deepnest_East_11", 4977, 3192), ("Deepnest_East_03", 2789, 1954),
                                   ("Deepnest_East_08", 7097, 1367), ("Deepnest_East_18", 5982, 4160)]),
}


def build_far(src, name, out):
    """Los trozos de FAR de esa zona, del tamaño de una sala del original a la escala del Caballero, desenfocados."""
    zone, picks = FAR.get(name, (None, []))
    for n, (room, x, y) in enumerate(picks):
        p = src / "hollow-knight-mundos" / zone / room / f"{room}_plana.jpg"
        im = Image.open(p).convert("RGB").crop((x, y, x + FAR_W, y + FAR_H)).resize(FAR_OUT, Image.LANCZOS)
        im.filter(ImageFilter.GaussianBlur(2)).save(out / f"lejano__{n:02d}.png")
    return len(picks)
