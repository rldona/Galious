"""Enemigos y proyectiles de Hollow Knight para los de Maze of Galious (Visuals/HollowEnemyArt en Unity).

Cada enemigo del original lleva el dibujo de una criatura de Hollow Knight con un papel parecido, y cada proyectil, uno
de Hollow Knight que haga lo mismo. Sale a Assets/Art/Resources/Hollow/enemigos/ (los que andan: pivote en los pies) y
.../enemigos_centro/ (los que vuelan, cuelgan o vuelan como proyectil: pivote en el centro), fuera de git como el resto
del arte de Team Cherry:
  - <clave>_<accion>_NN.png, mirando a la derecha (los de Hollow Knight miran a la izquierda; los que no, faces="derecha"),
    a 8 texels por píxel;
  - enemigos/info.txt: qué tipos del original usan cada clave, su anclaje, cuánto puede pasar el cuerpo de su caja de
    contacto original (k, el mismo por arriba y por los lados), el alto y el ancho del cuerpo en reposo, si gira con su
    velocidad (proyectiles alargados), y los fps de cada clip.
Acciones: quieto y mover siempre; atacar (cuando lanza algo), girar y morir si la criatura las tiene. Si no se dan,
girar y morir se buscan solas ("Turn", "Death Air" + "Death Land", "Death"...). Cada acción usa su lienzo de tk2d y se
alinea por el centro del cuerpo, así que puede venir de otro lienzo que el reposo.
"""
import csv
from pathlib import Path

from PIL import Image, ImageFilter

REPO = Path(__file__).resolve().parents[2]
RES = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow"
ALPHA = 40

TURN = ["Turn", "turn", "TurnToIdle", "Turn To Idle", "TurnToFly", "Turn To Fly", "Turn Fly", "Mini Turn", "Side A Turn"]
DEATH = [["Death Air", "Death Land"], ["Death"], ["Death Fall"], ["Death Land"]]

# clave → tipos del original (enemy_ai.md), colección tk2d, anclaje ("pies" o "centro"), k (el cuerpo mide como mucho k
# veces su caja de contacto, por arriba y por los lados), clips de cada acción (un nombre o una lista que se encadena),
# "rot" (girar el dibujo al importarlo) y, para los proyectiles alargados, "spin": el ángulo al que apunta el dibujo
# (en el juego gira con la velocidad).
ENEMIES = {
    # ---------- castillo ----------
    "murcielago": dict(types=[0x0E], col="Buzzer", anchor="centro", k=1.5,     # Vengefly (elegido por el usuario)
                       clips={"quieto": "Idle", "mover": "Fly", "atacar": "Chase", "girar": "TurnToFly"}),
    "esqueleto": dict(types=[0x25], col="Zombie Basic1", anchor="pies", k=1.25,   # Husk
                      clips={"quieto": "Idle", "mover": "Walk", "atacar": ["Attack Anticipate", "Attack Lunge"]}),
    "caballero": dict(types=[0x0A], col="Zombie Shield", anchor="pies", k=1.25,   # Husk con escudo
                      clips={"quieto": "Idle", "mover": "Walk", "atacar": "Attack1 A"}),
    "brote": dict(types=[0x2E], col="Plant Trap Anim", anchor="pies", k=1.5,      # planta trampa
                  clips={"quieto": "Snap", "mover": "Snap"}),
    "rodador": dict(types=[0x18], col="Roller", anchor="pies", k=1.3,             # Baldur
                    clips={"quieto": "Idle", "mover": "Roll"}),
    "mosca": dict(types=[0x19, 0x37], col="Fly", anchor="centro", k=1.4,          # Gruzzer
                  clips={"quieto": "Fly", "mover": "Fly"}),
    "reptador": dict(types=[0x02, 0x2B], col="Crawler", anchor="pies", k=1.3,     # Crawlid
                     clips={"quieto": "idle", "mover": "walk"}),
    "erizo": dict(types=[0x11], col="Climber", anchor="pies", k=1.3,              # Tiktik
                  clips={"quieto": "Walk", "mover": "Walk", "atacar": "Stun"}),
    "espiritu": dict(types=[0x4E], col="Glow Fly", anchor="centro", k=2.0,        # Lumafly
                     clips={"quieto": "Bug", "mover": "Bug"}),
    "hada": dict(types=[0x4F], col="Health Bugs", anchor="centro", k=1.6,         # Lifeblood flapper
                 clips={"quieto": "Flapper Fly", "mover": "Flapper Fly"}),
    "fantasma": dict(types=[0x26], col="Mage Balloon Anim", anchor="centro", k=1.5,   # Folly
                     clips={"quieto": "Idle", "mover": "Chase", "girar": "TurnToChase"}),
    "hechicero": dict(types=[0x3C], col="Mage", anchor="pies", k=1.5,             # Soul Twister
                      clips={"quieto": "Idle", "mover": "Teleport In", "atacar": "Cast"}),
    "estatua": dict(types=[0x23], col="Moss Knight Anim", anchor="pies", k=1.4,   # Moss Knight
                    clips={"quieto": "Idle", "mover": "Walk", "atacar": "Slash Quick"}),
    "boca": dict(types=[0x3A, 0x50], col="Plant Turret", anchor="centro", k=1.4,  # Gulka
                 clips={"quieto": "Idle", "mover": "Idle", "atacar": "Shoot"}),
    "torreta": dict(types=[0x32, 0x51], col="Mawlek Turret", anchor="centro", k=1.4,
                    clips={"quieto": "Active", "mover": "Active"}),
    "pinchos": dict(types=[0x31], col="Colosseum Spike Anim", anchor="pies", k=1.0,
                    clips={"quieto": "Expand", "mover": "Expand"}),
    "bloque": dict(types=[0x34], col="Dynamic Terrain", anchor="centro", k=1.1,   # bloque que baja: roca que tiembla
                   clips={"quieto": "Idle", "mover": "Shake"}),
    "piston": dict(types=[0x04], col="Colosseum Plat Anim", anchor="centro", k=1.1,
                   clips={"quieto": "Expanded", "mover": "Expand"}),
    "monstruo_agua": dict(types=[0x42], col="Flipper", anchor="centro", k=1.4,    # Pilflip
                          clips={"quieto": "Side B Hop", "mover": "Side B Hop", "atacar": "Hop Start"}),
    "orbitador": dict(types=[0x15], col="Spider Flyer", anchor="centro", k=1.5,
                      clips={"quieto": "Idle", "mover": "Chase"}),

    # ---------- mundos ----------
    "acechador": dict(types=[0x10], col="Moss Charger", anchor="pies", k=1.3,     # Mosskin
                      clips={"quieto": "Run", "mover": "Run", "atacar": "Charge"}),
    "imitador": dict(types=[0x2F], col="Grub Mimic Anim", anchor="pies", k=1.3,
                     clips={"quieto": "Idle", "mover": "Run"}),
    "rebotador": dict(types=[0x28], col="Acid Flyer Anim", anchor="centro", k=1.5,    # Duranda
                      clips={"quieto": "Fly", "mover": "Fly"}),
    "embestidor": dict(types=[0x20], col="Mega Moss Charger Anim", anchor="pies", k=1.3,
                       clips={"quieto": "Charge", "mover": "Charge"}),
    "soldado": dict(types=[0x2D], col="Mantis v02", anchor="pies", k=1.4,         # guerrero mantis
                    clips={"quieto": "Idle", "mover": "Walk", "atacar": "Attack"}),
    "oruga": dict(types=[0x2C, 0x64], col="Centipede", anchor="pies", k=1.3,
                  clips={"quieto": "Walk", "mover": "Walk"}),
    "uh": dict(types=[0x2A], col="Mage Knight", anchor="pies", k=1.4,             # Soul Warrior
               clips={"quieto": "Idle", "mover": "Tele In"}),
    "saltador": dict(types=[0x24], col="Hopper", anchor="pies", k=1.4,
                     clips={"quieto": "Idle", "mover": "Jump"}),
    "babosa": dict(types=[0x03], col="Abyss Crawler Anim", anchor="pies", k=1.3,  # Shadow Creeper
                   clips={"quieto": "Walk", "mover": "Walk"}),
    "mosca_divisible": dict(types=[0x49, 0x52, 0x53], col="Blow Fly Anim", anchor="centro", k=1.4,   # Boofly
                            clips={"quieto": "Fly", "mover": "Fly"}),
    "emergente": dict(types=[0x14], col="Worm", anchor="pies", k=1.4,             # Goam
                      clips={"quieto": "Idle", "mover": "Burst"}),
    "burbuja": dict(types=[0x29], col="Jellyfish", anchor="centro", k=1.5,        # Ooma
                    clips={"quieto": "Idle", "mover": "Idle"}),
    "centinela": dict(types=[0x17], col="Spitter", anchor="centro", k=1.5,        # Aspid
                      clips={"quieto": "Fly", "mover": "Fly", "atacar": "Fire"}),
    "saltamontes": dict(types=[0x07], col="Grass Hopper ANIM", anchor="pies", k=1.4,
                        clips={"quieto": "Jump Antic", "mover": "Jump"}),
    "orbe": dict(types=[0x36], col="Blobble Anim", anchor="centro", k=1.4,
                 clips={"quieto": "Fly", "mover": "Fly"}),
    "errante": dict(types=[0x4C], col="Fungus Flier", anchor="centro", k=1.4,     # Fungling
                    clips={"quieto": "Float", "mover": "Float", "atacar": "Spit"}),
    "bola": dict(types=[0x0F], col="Inflater Anim", anchor="centro", k=1.3,       # Obble
                 clips={"quieto": "Inflate Idle", "mover": "Inflate Idle"}),
    "armadillo": dict(types=[0x12], col="Colosseum_Roller", anchor="pies", k=1.3,
                      clips={"quieto": "Idle", "mover": "Roll"}),
    "aparicion": dict(types=[0x16], col="Mantis Flyer", anchor="centro", k=1.5,   # Mantis Youth
                      clips={"quieto": "Rest", "mover": "Fly", "atacar": "Attack"}),
    "corredor": dict(types=[0x1F], col="Spider Mini Anim", anchor="pies", k=1.3,
                     clips={"quieto": "Run", "mover": "Run"}),
    "ave": dict(types=[0x27], col="Big Buzzer", anchor="centro", k=1.5,           # Vengefly King
                clips={"quieto": "Fly", "mover": "Swoop"}),
    "escupefuego": dict(types=[0x22], col="Super Spitter Anim", anchor="centro", k=1.5,   # Primal Aspid
                        clips={"quieto": "Idle", "mover": "Idle", "atacar": ["Attack Antic", "Attack"]}),
    "hombre_llama": dict(types=[0x0C, 0x13], col="Grimmkin Small Anim", anchor="centro", k=1.5,
                         clips={"quieto": "Idle", "mover": "Idle", "atacar": ["Throw Antic", "Throw"]}),
    "sombra": dict(types=[0x40], col="Hollow Shade", anchor="centro", k=1.4,
                   clips={"quieto": "Idle", "mover": "Fly", "atacar": "Cast"}),
    "cabeza": dict(types=[0x69], col="Ghost No Eyes Anim", anchor="centro", k=1.4,
                   clips={"quieto": "Head", "mover": "Head"}),
    "serpiente": dict(types=[0x05], col="Colosseum_Big_Worm", anchor="pies", k=1.3,
                      clips={"quieto": "Idle", "mover": "Walk", "atacar": "Attack"}),
    "bicho": dict(types=[0x01], col="Mushroom Brawler", anchor="pies", k=1.3,     # Shrumal Ogre
                  clips={"quieto": "Antic", "mover": "Run"}),
    "patrullero": dict(types=[0x43], col="Zombie Beam Miner Anim", anchor="pies", k=1.3,   # Crystal Hunter
                       clips={"quieto": "Idle", "mover": "Run", "atacar": ["Shoot Antic", "Shoot"]}),
    "escarabajo_naranja": dict(types=[0x21], col="Orange Scuttler", anchor="pies", k=1.3,
                               clips={"quieto": "Run", "mover": "Run"}),
    "vadeador": dict(types=[0x30], col="Mines Goomba", anchor="pies", k=1.3,
                     clips={"quieto": "Walk", "mover": "Walk"}),
    "trepador": dict(types=[0x38], col="Climber", anchor="pies", k=1.3,
                     clips={"quieto": "Walk", "mover": "Walk"}),
    "dardo": dict(types=[0x0B], col="Hatcher Baby", anchor="centro", k=1.4,
                  clips={"quieto": "Fly", "mover": "Fly"}),
    "pelota": dict(types=[0x09], col="Generic Glob Anim", anchor="centro", k=1.2,
                   clips={"quieto": "Wobble", "mover": "Wobble"}),
    "pez": dict(types=[0x0D], col="Flipper", anchor="centro", k=1.4,
                clips={"quieto": "Side B Hop", "mover": "Side B Hop"}),
    "piedra_rodante": dict(types=[0x35], col="Mushroom Roller _ Mini", anchor="pies", k=1.3,
                           clips={"quieto": "Idle", "mover": "Run"}),
    "tejedor": dict(types=[0x3E], col="Weaverling Anim", anchor="pies", k=1.3,
                    clips={"quieto": "Run", "mover": "Run", "atacar": "Attack"}),

    # ---------- agua ----------
    "geiser": dict(types=[0x08, 0x60], col="Water_effects", anchor="pies", k=1.2,
                   clips={"quieto": "Splash_out_of_water", "mover": "Splash_out_of_water"}),
    "gota": dict(types=[0x1B], col="WaterDrip", anchor="centro", k=1.3,
                 clips={"quieto": "Drip", "mover": "Drip"}),
    "salpicadura": dict(types=[0x6A], col="Water_effects", anchor="pies", k=1.2,
                        clips={"quieto": "Splash_into_water", "mover": "Splash_into_water"}),

    # ---------- proyectiles ----------
    "escupitajo": dict(types=[0x1A], col="Vomit Glob", anchor="centro", k=1.4,    # de la serpiente
                       clips={"quieto": "Idle", "mover": "Idle"}),
    "pua": dict(types=[0x1D, 0x33], col="Spine Projectile Anim", anchor="centro", k=1.3, spin=90,   # erizo y flechas
                clips={"quieto": "Fire", "mover": "Fire"}),
    "bolita_gris": dict(types=[0x1E], col="Generic Glob Anim", anchor="centro", k=1.3,   # del armadillo
                        clips={"quieto": "Fall", "mover": "Fall"}),
    "media_luna": dict(types=[0x3F], col="Grubberfly Beam Anim", anchor="centro", k=1.3, spin=0,   # soldado: paraliza
                       clips={"quieto": "Idle", "mover": "Idle"}),
    "gota_naranja": dict(types=[0x3B, 0x48], col="Orange Glob Anim", anchor="centro", k=1.3,   # bocas y murciélagos
                         clips={"quieto": "Fall", "mover": "Fall"}),
    "orbe_alma": dict(types=[0x3D], col="Mage", anchor="centro", k=1.4,           # orbe del Soul Twister
                      clips={"quieto": "Orb Idle", "mover": "Orb Idle"}),
    "bola_fuego": dict(types=[0x44, 0x56, 0x4A], col="Flameball Anim", anchor="centro", k=1.4,   # llamas
                       clips={"quieto": "Idle", "mover": "Idle"}),
    "bola_naranja": dict(types=[0x45, 0x46, 0x41], col="Projectiles", anchor="centro", k=1.3,
                         clips={"quieto": "Idle", "mover": "Idle"}),
    "orbe_rosa": dict(types=[0x4B], col="Laser Beam Anim", anchor="centro", k=1.3,    # del fantasma
                      clips={"quieto": "Ball Shoot", "mover": "Ball Shoot"}),
    "gota_agua": dict(types=[0x61], col="WaterDrip", anchor="centro", k=1.3, spin=270,
                      clips={"quieto": "Fall", "mover": "Fall"}),
    "bomba": dict(types=[0x47], col="Vomit Glob", anchor="centro", k=1.3,
                  clips={"quieto": "Idle", "mover": "Idle"}),
    "espora": dict(types=[0x4D], col="Spatters", anchor="centro", k=1.4,
                   clips={"quieto": "Impact Orange", "mover": "Impact Orange"}),
    "rayo": dict(types=[0x62], col="Laser Beam Anim", anchor="centro", k=1.1, rot=90,   # rayo vertical del patrullero
                 clips={"quieto": "Beam Shoot", "mover": "Beam Shoot"}),
    "relampago": dict(types=[0x06], col="Zap Cloud", anchor="centro", k=1.2,
                      clips={"quieto": "Zap", "mover": "Zap"}),

    # La sombra (RemakeRules.HollowDeath, tipo propio 0x1F1): donde murió el jugador, con su geo; la de Hollow Knight.
    "sombra_jugador": dict(types=[0x1F1], col="Hollow Shade", anchor="centro", k=1.4,
                   clips={"quieto": "Idle", "mover": "Fly", "atacar": ["Slash Antic", "Slash"], "morir": "Death"}),

    # ---------- jefes (tipo 0x100 + id del jefe) y lo que crean ----------
    # Bony Dragon (pasea, salta y lanza 3 huesos en parábola): el False Knight, que salta y aplasta.
    # Con RemakeRules.FalseKnightMace no lanza huesos: da con la maza delante y la bola hace daño donde cae (atacar,
    # con la maza en el suelo durante Attack Recover). El False Knight de Hollow Knight mira a la derecha (no se voltea).
    "jefe_bony_dragon": dict(types=[0x101], col="False Knight", anchor="pies", k=1.3, faces="derecha",
                             clips={"quieto": "Idle", "mover": "Run", "atacar": ["Attack Antic", "Attack", "Attack Recover"]}),
    # Elusive Dragon (asoma por los lados a 5 alturas, llamarada, se esconde): los Mantis Lords, que llegan por la pared.
    # Su caja de contacto es todo el cuello que sale de la pared: se apoya en la zona que recibe golpes (la cabeza).
    "jefe_elusive_dragon": dict(types=[0x102], col="Mantis Lords Anim", anchor="centro", k=1.5, on_weapon=True,
                                clips={"quieto": "Wall Ready", "mover": "Wall Ready", "atacar": ["Throw Antic", "Throw"]}),   # Wall Arrive es una estela borrosa
    # Arlyane (camina, te persigue, abanicos de disparos): el Traitor Lord, que lanza hoces.
    "jefe_arlyane": dict(types=[0x103], col="Traitor Lord Anim", anchor="pies", k=1.3,
                         clips={"quieto": "Idle", "mover": "Walk", "atacar": ["Sickle Throw Antic", "Sickle Throw Attack"]}),
    # Sea Anemone (fija, se abre y se cierra, surtidor de esporas): Uumuu.
    "jefe_sea_anemone": dict(types=[0x104], col="Mega Jellyfish Anim", anchor="centro", k=1.3,
                             clips={"quieto": "Idle", "mover": "Idle", "atacar": "Attack"}),
    # King Dragon (pasea y se para, llamaradas, nubes de tormenta): Grimm.
    "jefe_king_dragon": dict(types=[0x106], col="Grimm Anim", anchor="pies", k=1.3,
                             clips={"quieto": "Idle", "mover": "Idle", "atacar": "Cast"}),
    # Big Lip (ida y vuelta, saca la lengua, disparo en 8 direcciones): el Mawlek.
    "jefe_big_lip": dict(types=[0x107], col="Mini Mawlek Anim", anchor="pies", k=1.3,
                         clips={"quieto": "Rest", "mover": "Run Forward", "atacar": ["Shoot Antic", "Shoot"]}),
    # Monster (anda, cae por los bordes, bolas que rebotan): el Broken Vessel y sus globos de infección.
    "jefe_monster": dict(types=[0x108], col="Broken Vessel Anim", anchor="pies", k=1.3,
                         clips={"quieto": "Idle", "mover": "Walk", "atacar": "Shake Loop"}),
    # Galious (fijo, se teletransporta, orbes en órbita): la Radiance.
    "jefe_galious": dict(types=[0x109], col="Radiance Anim", anchor="centro", k=1.3,
                         clips={"quieto": "Idle", "mover": "Tele In", "atacar": "Antic"}),
    # Donra (vuela por una caja, la cabeza sigue tu altura, llamarada): Nosk alado.
    # Su caja de contacto es muy plana (unos 42 × 12 px): con más margen, para que se vea (unos 26 px de alto).
    "jefe_donra": dict(types=[0x10A], col="Nosk_Hornet", anchor="centro", k=2.2,
                       clips={"quieto": "Idle", "mover": "Fly", "atacar": ["Swoop Antic", "Swoop"]}),
    # Jump Slime (se arrastra y salta para aplastar): el Dung Defender, que rueda y salta.
    "jefe_jump_slime": dict(types=[0x10B], col="Dung Defender Anim", anchor="pies", k=1.3,
                            clips={"quieto": "Idle", "mover": "Roll"}),
    # Lo que crean los jefes.
    "hueso": dict(types=[0x54, 0x55], col="Mantis Lords Anim", anchor="centro", k=1.3, spin=0,   # Bony Dragon
                  clips={"quieto": "Lance", "mover": "Lance"}),
    "llamarada": dict(types=[0x57], col="Grimm Anim", anchor="centro", k=1.3,     # Elusive Dragon, King Dragon, Donra
                      clips={"quieto": "Firebat", "mover": "Firebat"}),
    "hoz": dict(types=[0x58], col="Mantis Lords Anim", anchor="centro", k=1.3,    # abanicos de Arlyane
                clips={"quieto": "Mantis Air Projectile", "mover": "Mantis Air Projectile"}),
    "medusita": dict(types=[0x59], col="Jellyfish", anchor="centro", k=1.4,       # esporas de Sea Anemone
                     clips={"quieto": "Lil Fly", "mover": "Lil Fly"}),
    "escupitajo_mawlek": dict(types=[0x65], col="Orange Glob Anim", anchor="centro", k=1.3,   # de Big Lip
                              clips={"quieto": "Fall", "mover": "Fall"}),
    "orbe_radiance": dict(types=[0x67], col="Laser Beam Anim", anchor="centro", k=1.3,   # orbes de Galious
                          clips={"quieto": "Ball Shoot", "mover": "Ball Shoot"}),
    "globo": dict(types=[0x68], col="Parasite Balloon Anim", anchor="centro", k=1.4,   # bolas de Monster
                  clips={"quieto": "Chase", "mover": "Chase"}),
    "chispa_invocacion": dict(types=[0x63], col="Light Effects", anchor="centro", k=1.5,
                              clips={"quieto": "Burst Effect", "mover": "Burst Effect"}),
    # Final: fantasmas de las piedras, piedras del bebé y el bebé (Pampas, un gusarapo de Hollow Knight).
    "fantasma_espiral": dict(types=[0x39], col="Ghost Slug Anim", anchor="centro", k=1.4,
                             clips={"quieto": "Idle", "mover": "Idle"}),
    "piedra_bebe": dict(types=[0x5C], col="Geo Rocks Green", anchor="pies", k=1.1,
                        clips={"quieto": "Idle 1", "mover": "Idle 1", "morir": "Broken 1"}),
    "bebe": dict(types=[0x6B], col="Grub2", anchor="pies", k=1.4,
                 clips={"quieto": "Idle", "mover": "Idle"}),
}


def opaque(im):
    return im.getchannel("A").point(lambda v: 255 if v > ALPHA else 0)


def body_box(im):
    """Caja del cuerpo: la silueta adelgazada 4 texels y vuelta a ensanchar (sin patas, alas finas, antenas ni armas)."""
    return opaque(im).filter(ImageFilter.MinFilter(9)).filter(ImageFilter.MaxFilter(9)).getbbox() or opaque(im).getbbox()


def body_center(im):
    a = opaque(im).filter(ImageFilter.MinFilter(9))
    if a.getbbox() is None:
        a = opaque(im)
    bb = a.getbbox()
    xs, ys = [], []
    for y in range(bb[1], bb[3]):
        for x in range(bb[0], bb[2]):
            if a.getpixel((x, y)):
                xs.append(x)
                ys.append(y)
    return sum(xs) / len(xs), sum(ys) / len(ys)


def rows_of(src, col):
    return list(csv.DictReader(open(src / "hollow-knight-tk2d" / col / "clip_frames.csv", encoding="utf-8")))


def frames(src, col, rows, names, rot):
    """Fotogramas (sin repetir y no vacíos) de uno o varios clips encadenados, y los fps del primero."""
    out, seen, fps = [], set(), None
    for clip in ([names] if isinstance(names, str) else names):
        rs = [r for r in rows if r["clip"] == clip]
        if not rs:
            raise SystemExit(f"No está el clip {clip!r} en {col}")
        fps = fps or float(rs[0]["fps"])
        for r in rs:
            p = src / "hollow-knight-tk2d" / col / r["archivo_png"]
            if p.stem in seen:
                continue
            seen.add(p.stem)
            im = Image.open(p).convert("RGBA")
            if rot:
                im = im.rotate(rot, expand=True)
            if opaque(im).getbbox():
                out.append(im)
    return out, fps


# Lo que no es una criatura (agua y proyectiles): sin girar ni morir buscados solos (serían los de otra criatura de su
# colección, como el orbe del Soul Twister).
NOT_CREATURES = {"geiser", "gota", "salpicadura", "escupitajo", "pua", "bolita_gris", "media_luna", "gota_naranja",
                 "orbe_alma", "bola_fuego", "bola_naranja", "orbe_rosa", "gota_agua", "bomba", "espora", "rayo", "relampago",
                 "hueso", "llamarada", "hoz", "medusita", "escupitajo_mawlek", "orbe_radiance", "chispa_invocacion",
                 "piedra_bebe", "bebe"}


def clips_for(key, spec, rows):
    names = {r["clip"] for r in rows}
    clips = dict(spec["clips"])
    if key in NOT_CREATURES:
        return clips
    if "girar" not in clips:
        t = next((n for n in TURN if n in names), None)
        if t:
            clips["girar"] = t
    if "morir" not in clips:
        d = next((d for d in DEATH if all(n in names for n in d)), None)
        if d:
            clips["morir"] = d
    return clips


def build(src):
    for sub in ("enemigos", "enemigos_centro"):
        (RES / sub).mkdir(parents=True, exist_ok=True)
        for old in (RES / sub).glob("*.png"):
            old.unlink()
    info = []
    for key, spec in ENEMIES.items():
        rows = rows_of(src, spec["col"])
        rot = spec.get("rot", 0)
        actions = {a: frames(src, spec["col"], rows, c, rot) for a, c in clips_for(key, spec, rows).items()}
        feet_anchor = spec["anchor"] == "pies"
        # Reposo: el primer fotograma de quieto (en su lienzo más común). Su cuerpo da el tamaño en el juego.
        qsizes = [f.size for f in actions["quieto"][0]]
        rest = next(f for f in actions["quieto"][0] if f.size == max(set(qsizes), key=qsizes.count))
        body = body_box(rest)
        ref_h, ref_w = body[3] - body[1], body[2] - body[0]
        folder = RES / ("enemigos" if feet_anchor else "enemigos_centro")
        made = []
        for a, (fr, fps) in actions.items():
            if not fr:
                continue
            # Cada acción, con su lienzo más común (otro lienzo de tk2d es otro ancla); alineada por el centro del cuerpo
            # de su primer fotograma y por sus pies (los que andan) o el centro del cuerpo (los que vuelan).
            sizes = [f.size for f in fr]
            canvas = max(set(sizes), key=sizes.count)
            fr = [f for f in fr if f.size == canvas]
            first = rest if rest.size == canvas else fr[0]
            cx, cy = body_center(first)
            if feet_anchor:
                cy = opaque(first).getbbox()[3]
            left = up = down = 0
            for f in fr:
                b = opaque(f).getbbox()
                left = max(left, cx - b[0], b[2] - cx)
                up = max(up, cy - b[1])
                down = max(down, b[3] - cy)
            half = int(left + 2)
            v = max(up, down) + 2
            box = (cx - half, cy - up - 2, cx + half, cy) if feet_anchor else (cx - half, cy - v, cx + half, cy + v)
            box = tuple(round(x) for x in box)
            for i, f in enumerate(fr):
                f = f.crop(box)
                if spec.get("faces") != "derecha":
                    f = f.transpose(Image.FLIP_LEFT_RIGHT)
                f.save(folder / f"{key}_{a}_{i:02d}.png")
            info.append(f"clip {key}_{a} {fps}")
            made.append(f"{a} {len(fr)}")
        types = ",".join(f"{t:02X}" for t in spec["types"])
        spin = spec.get("spin")
        info.append(f"enemigo {key} {types} {spec['anchor']} {spec['k']} {ref_h} {ref_w} {'-' if spin is None else spin} "
                    f"{'arma' if spec.get('on_weapon') else 'contacto'}")
        print(f"{key}: {spec['col']} ({', '.join(made)})")
    (RES / "enemigos" / "info.txt").write_text("\n".join(info) + "\n", encoding="utf-8")
    print(f"enemigos: {len(ENEMIES)} → {RES / 'enemigos'}")
