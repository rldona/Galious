"""Objetos de Maze of Galious con iconos de Hollow Knight (amuletos e inventario).

Cada uno de los 47 objetos del original lleva el icono de un amuleto u objeto de Hollow Knight que haga algo parecido.
Sale a Assets/Art/Resources/Hollow/objetos_sala/objetoXX.png (XX, el tipo en hexadecimal), fuera de git como el resto
del arte de Team Cherry, ajustado sin deformar a un hueco de 16 × 16 px del original (128 × 128 texels, a 8 por píxel)
y con el pivote abajo en el centro (lo pone el ArtImporter por la carpeta), como los objetos de las salas.
"""
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/objetos_sala"
SIZE = 128   # texels: 16 px del original

# tipo → icono de Hollow Knight (nombre de fichero, se busca en la carpeta de inventario) y por qué.
ITEMS = {
    0x01: ("Inv_0025_spell_fireball_01", "Arrow: Vengeful Spirit, el proyectil básico"),
    0x02: ("Inv_0025_spell_fireball_01_level2", "Ceramic Arrow: Shade Soul, el proyectil mejorado"),
    0x03: ("Inv_0026_spell_quake_01", "Rolling Fire: Desolate Dive, fuego por el suelo"),
    0x04: ("Inv_0026_spell_quake_01_level2", "Fire: Descending Dark"),
    0x05: ("charm_dung_def", "Mine: Defender's Crest, la nube que daña alrededor"),
    0x06: ("dream_nail_0000_4", "Magnifying Glass: el aguijón onírico, que lee lo oculto"),
    0x07: ("_0002_charm_glass_attack_up", "Holy Water: Fragile Strength, más daño al jefe"),
    0x08: ("Inv_0015_dash-cloak", "Cloak: la capa de polilla"),
    0x09: ("charm_magic_cost_down", "Magic Rod: Spell Twister, munición sin gastar"),
    0x0A: ("inv_item_map_quill_combined", "Map: el mapa y la pluma de Cornifer"),
    0x0B: ("inv_white_brand", "Cross: el sello del Rey, sin él no se vence a Galious"),
    0x0C: ("Elegant_Key", "Great Key: la llave elegante, abre la puerta del mundo"),
    0x0D: ("inv_item__0012_hallownest-seal", "Necklace: el sello de Hallownest"),
    0x0E: ("inv_item__0010_Kings_idol", "Crown: el ídolo del Rey"),
    0x0F: ("HP_UI_010007", "Helmet: una máscara"),
    0x10: ("items__0001_acid-armour", "Oar: la Lágrima de Isma, para andar sobre el agua"),
    0x11: ("charm_grimm_sprint_master", "Shoes: Sprintmaster"),
    0x12: ("inv_item__0011_arcane-egg", "Decorative Doll: el huevo arcano"),
    0x13: ("charm_sprite_03", "Hoi: Wayward Compass, para no perder el norte"),
    0x14: ("charm_grub_mid", "Bell: Grubsong, una canción"),
    0x15: ("dream_gate_inv_icon", "Angel Ring: la Puerta del sueño, volver a un sitio"),
    0x16: ("Lumafly_Lantern0000", "Candle: el farol de Lumafly, para las salas oscuras"),
    0x17: ("charm_hive", "Armor: Hiveblood"),
    0x18: ("items__0004_shade_cloak", "Carpet: la capa de sombra"),
    0x19: ("_0007_charm_blue_health_large", "Hat Helmet: Lifeblood Core, aguantar más"),
    0x1A: ("inv_item__0004_quill", "Lamp: la pluma, que marca el mapa"),
    0x1B: ("_0003_charm_glass_geo", "Vase: Fragile Greed, más ganancia"),
    0x1C: ("_0008_charm_nail_damage_up", "Pendant: Heavy Blow, romper paredes"),
    0x1D: ("charm_fluke", "Earrings: Flukenest, varios proyectiles a la vez"),
    0x1E: ("charm_grimm_silkweaver", "Bracelet: Weaversong, varias crías a la vez"),
    0x1F: ("char_mantis", "Ring: Mark of Pride, golpes más fuertes contra las piedras"),
    0x20: ("inv_item__0013_wanderers-journal", "Bible: el diario del errante"),
    0x21: ("Inv_0024_spell_scream_01", "Harp: Howling Wraiths, un sonido que lo rompe todo"),
    0x22: ("charm_white_full", "Triangle: el amuleto blanco"),
    0x23: ("inv_Waterways_Key", "Conch Shell: la llave de las alcantarillas"),
    0x24: ("inv_item__0008_jar_col_map", "Pitcher: el tarro"),
    0x25: ("nail_upgrade_0002_sharpened_nail", "Saber: el aguijón afilado"),
    0x26: ("Inv_0021_inv_dash_strike_skill", "Dagger: un arte del aguijón"),
    0x27: ("items__0002_emperor-wings", "Wing: las alas de monarca"),
    0x28: ("charm_blocker", "Copper Shield: Baldur Shell"),
    0x29: ("inv_rancid_egg", "Bread & Water: comida"),
    0x2A: ("inv_item__0009_ore", "Salt: el mineral pálido"),
    0x2B: ("_0006_charm_stalwart_shell", "Silver Shield: Stalwart Shell"),
    0x2C: ("charm_grimm_markoth_shield", "Gold Shield: Dreamshield"),
    0x2D: ("Inventory_soul_vessel_full", "Quiver: el recipiente de alma (munición)"),
    0x2E: ("InventoryCoin", "Coins: geo"),
    0x2F: ("inv_item__00014_graveyard_key", "Keys: una llave sencilla"),
}


# Piezas de la sala (tk2d: colección, clip, fotograma), ajustadas sin deformar a su hueco en texels (8 por píxel), con
# los pies abajo en el centro:
#   - piedra rompible (2 × 2 casillas): la roca de geo, que también se golpea hasta romperla y esconde cosas;
#   - puerta con llave (columna de 1 × 3 que se abre hacia arriba): la verja del Archivo; se guardan sus 3, 2 y 1 casillas de
#     abajo, que son lo que se ve mientras sube;
#   - puerta giratoria (1 × 3): la puerta de hueso;
#   - puerta de mundo (el hueco de 2 × 2 entre las jambas): la puerta de jefe, entera (cerrada) y rompiéndose (a medio abrir).
PIECES = {
    "roca": ("Geo Rocks Extra", "Idle 1", 0, (128, 128)),
    "puerta_llave": ("Battle Gate Archive Anim", "Closed", 0, (64, 192)),   # la de peaje era casi negra: no se veía
    "puerta_giratoria": ("Switches and gates", "Bone Gate Closed", 0, (64, 192)),
    "puerta_mundo_cerrada": ("Boss Door Anim", "Idle", 0, (128, 128)),
    "puerta_mundo_media": ("Boss Door Anim", "Break", 4, (128, 128)),
}


def piece(src, col, clip, frame, box):
    import csv
    rows = [r for r in csv.DictReader(open(src / "hollow-knight-tk2d" / col / "clip_frames.csv", encoding="utf-8")) if r["clip"] == clip]
    im = Image.open(src / "hollow-knight-tk2d" / col / rows[min(frame, len(rows) - 1)]["archivo_png"]).convert("RGBA")
    im = im.crop(im.getchannel("A").point(lambda a: 255 if a > 20 else 0).getbbox())
    s = min(box[0] / im.width, box[1] / im.height)
    im = im.resize((max(1, round(im.width * s)), max(1, round(im.height * s))), Image.LANCZOS)
    # En un lienzo del tamaño del hueco, abajo en el centro (así el pivote de los pies es el del hueco).
    canvas = Image.new("RGBA", box, (0, 0, 0, 0))
    canvas.paste(im, ((box[0] - im.width) // 2, box[1] - im.height), im)
    return canvas


def find(src, name):
    for base in (src / "hollow-knight-inventario", src / "hollow-knight-texturas"):
        hits = list(base.rglob(f"{name}.png"))
        if hits:
            return hits[0]
    raise SystemExit(f"No encuentro el icono {name}.png")


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    for t, (name, _) in ITEMS.items():
        im = Image.open(find(src, name)).convert("RGBA")
        bb = im.getchannel("A").point(lambda a: 255 if a > 20 else 0).getbbox()
        if bb:
            im = im.crop(bb)
        s = SIZE / max(im.size)
        im = im.resize((max(1, round(im.width * s)), max(1, round(im.height * s))), Image.LANCZOS)
        im.save(OUT / f"objeto{t:02x}.png")
    for name, (col, clip, frame, box) in PIECES.items():
        im = piece(src, col, clip, frame, box)
        if name == "puerta_llave":
            # Lo que se ve de la puerta mientras sube: sus h casillas de abajo.
            for h in (1, 2, 3):
                im.crop((0, box[1] - 64 * h, box[0], box[1])).save(OUT / f"{name}_{h}.png")
        else:
            im.save(OUT / f"{name}.png")
    print(f"objetos: {len(ITEMS)} y {len(PIECES)} piezas de sala → {OUT}")
