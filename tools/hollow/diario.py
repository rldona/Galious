"""Diario del cazador de Galious: The Hollow Kingdom (Hunter's Journal de Hollow Knight; Visuals/HuntersJournal en Unity).

Cada criatura del juego lleva ya el dibujo de una de Hollow Knight (enemigos.py); su entrada del diario es la de esa
criatura en el diario de Hollow Knight (hollow-knight-diario: retrato, nombre, descripción y notas del cazador en
castellano). Las que no tienen entrada en Hollow Knight (la Lumélula común, el escarabajo naranja…) llevan un texto
nuestro y, de retrato, su dibujo en reposo. Los tipos que comparten criatura comparten entrada.

Sale a Assets/Art/Resources/Hollow/diario/ (fuera de git, como el resto del arte y los textos de Team Cherry):
  - <CLAVE>.png: el retrato, ajustado a 512 px de lado como mucho;
  - diario.json: {"entries": [{key, name, desc, notes, boss, types: [...]}]}, en el orden del juego (castillo y mundos,
    jefes al final), que lee Unity con JsonUtility;
  - simbolo.png: el símbolo del cazador; selector.png: la marca de la entrada elegida.
"""
import csv
import json
from pathlib import Path

from PIL import Image

import enemigos

REPO = Path(__file__).resolve().parents[2]
RES = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow"
OUT = RES / "diario"
MAX = 512

# clave de enemigos.py → clave del diario de Hollow Knight (None: texto propio, abajo).
JOURNAL = {
    "murcielago": "BUZZER", "esqueleto": "ZOM_RUNNER", "caballero": "ZOM_SHIELD", "brote": "SNAPPER_TRAP",
    "rodador": "ROLLER", "mosca": "BOUNCER", "reptador": "CRAWLER", "erizo": "CLIMBER", "trepador": "CLIMBER",
    "espiritu": None, "hada": "HEALTH_SCUTTLER", "fantasma": "MAGE_BALLOON", "hechicero": "MAGE",
    "estatua": "MOSS_KNIGHT", "boca": "PLANT_SHOOTER", "torreta": "MAWLEK_TURRET", "monstruo_agua": "FLIP_HOPPER",
    "pez": "FLIP_HOPPER", "orbitador": "SPIDER_FLYER", "acechador": "MOSS_CHARGER", "imitador": "GRUB_MIMIC",
    "rebotador": "ACID_FLYER", "embestidor": "MEGA_MOSS_CHARGER", "soldado": "MANTIS", "oruga": "BIG_CENTIPEDE",
    "uh": "MAGE_KNIGHT", "saltador": "HOPPER", "babosa": "ABYSS_CRAWLER", "mosca_divisible": "BIG_FAT_FLYER",
    "emergente": "WORM", "burbuja": "JELLYFISH", "centinela": "SPITTER", "saltamontes": "GRASS_HOPPER",
    "orbe": "BLOBBLE", "errante": "FUNGOON_BABY", "bola": "BLOB_FLYER", "armadillo": "COL_ROLLER",
    "aparicion": "MANTIS_FLYER_CHILD", "corredor": "MINI_SPIDER", "ave": "BIGBUZZER", "escupefuego": "SUPER_SPITTER",
    "hombre_llama": "FLAMEBEARER_SMALL", "sombra": "HOLLOW_SHADE", "cabeza": "GHOST_NOEYES", "serpiente": "COL_WORM",
    "bicho": "MUSH_BRAWLER", "patrullero": "ZOM_BEAM_MINER", "escarabajo_naranja": None, "vadeador": "MINES_CRAWLER",
    "dardo": "HATCHLING", "pelota": None, "piedra_rodante": "MUSH_ROLLER", "tejedor": None,
    # jefes
    "jefe_bony_dragon": "FALSEKNIGHT", "jefe_sea_anemone": "MEGA_JELLYFISH", "jefe_elusive_dragon": "MANTIS_LORD",
    "jefe_jump_slime": "DUNG_DEFENDER", "jefe_donra": "MIMIC_SPIDER", "jefe_arlyane": "TRAITOR_LORD",
    "jefe_big_lip": "MAWLEK", "jefe_monster": "INFECTED_KNIGHT", "jefe_king_dragon": "GRIMM",
    "jefe_galious": "FINAL_BOSS",
}

# Las que no están en el diario de Hollow Knight: nombre, descripción y notas propias.
OWN = {
    "espiritu": ("Lumélula", "Insecto luminoso que vaga por las salas del castillo.",
                 "Inofensiva a la vista, pero su luz quema al tocarla. No hay gloria en cazarla; sí en no tropezar con ella."),
    "escarabajo_naranja": ("Escarabajo naranja", "Corretea por el suelo de un lado a otro sin mirar por dónde va.",
                           "Rápido y sin cerebro. Espéralo quieto con el aguijón listo."),
    "pelota": ("Glóbulo", "Bola de infección que rebota por las salas.",
               "Rebota sin pensar. Aprende su ritmo y golpéala en el aire."),
    "tejedor": ("Tejedorcillo", "Pequeño tejedor que avanza a saltitos por el suelo.",
                "Débil por sí solo; los nidos que guarda son lo peligroso."),
}

# Orden del diario: el de la tabla de enemigos.py (que va más o menos de los primeros a los últimos) y los jefes al final.


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    rows = {r["clave"]: r for r in csv.DictReader(open(src / "hollow-knight-diario/diario.csv", encoding="utf-8"))}
    entries, seen = [], {}
    keys = [k for k in enemigos.ENEMIES if k in JOURNAL and not k.startswith("jefe_")]
    keys += [k for k in enemigos.ENEMIES if k in JOURNAL and k.startswith("jefe_")]
    for key in keys:
        types = list(enemigos.ENEMIES[key]["types"])
        jk = JOURNAL[key]
        ek = jk or key.upper()
        if ek in seen:                         # otra criatura con la misma entrada: suma sus tipos
            seen[ek]["types"] += types
            continue
        if jk:
            r = rows[jk]
            name, desc, notes = r["nombre_es"].strip(), r["desc_es"].strip(), r["notas_es"].strip()
            im = Image.open(src / "hollow-knight-diario" / r["png"]).convert("RGBA")
        else:
            name, desc, notes = OWN[key]
            sub = "enemigos" if enemigos.ENEMIES[key]["anchor"] == "pies" else "enemigos_centro"
            im = Image.open(RES / sub / f"{key}_quieto_00.png").convert("RGBA")
        bb = im.getchannel("A").point(lambda a: 255 if a > 8 else 0).getbbox()
        if bb:
            im = im.crop(bb)
        k = min(1.0, MAX / max(im.size))
        if k < 1:
            im = im.resize((round(im.width * k), round(im.height * k)), Image.LANCZOS)
        im.save(OUT / f"{ek}.png")
        e = dict(key=ek, name=name, desc=desc, notes=notes, boss=key.startswith("jefe_"), types=types)
        seen[ek] = e
        entries.append(e)
    (OUT / "diario.json").write_text(json.dumps({"entries": entries}, ensure_ascii=False, indent=1), encoding="utf-8")
    for name, path in (("simbolo", "hunter_symbol.png"), ("selector", "selector.png")):
        Image.open(src / "hollow-knight-inventario/Journal" / path).convert("RGBA").save(OUT / f"{name}.png")
    print(f"diario: {len(entries)} entradas → {OUT}")
