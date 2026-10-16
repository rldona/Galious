"""Enemigos de Hollow Knight para los de Maze of Galious (Visuals/HollowEnemyArt en Unity).

Cada enemigo del original lleva el dibujo de una criatura de Hollow Knight con un papel parecido. Sale a
Assets/Art/Resources/Hollow/enemigos/ (los que andan: pivote en los pies) y .../enemigos_centro/ (los que vuelan o
cuelgan: pivote en el centro), fuera de git como el resto del arte de Team Cherry:
  - <clave>_<accion>_NN.png, mirando a la derecha (los de Hollow Knight miran a la izquierda), a 8 texels por píxel;
  - enemigos/info.txt: qué tipos del original usan cada clave, su anclaje, cuánto más grande que su caja se dibujan y
    la altura de referencia (la de su pose de reposo), y los fps de cada clip.
Acciones: quieto y mover (y las que se añadan). Si un tipo no está, el juego sigue con su dibujo o su cuadro.
"""
import csv
from pathlib import Path

from PIL import Image, ImageFilter

REPO = Path(__file__).resolve().parents[2]
RES = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow"
ALPHA = 40

# clave → tipos del original (enemy_ai.md), colección tk2d, anclaje ("pies" o "centro"), cuánto más alto que su caja
# (la pose de reposo), y clip de Hollow Knight para cada acción.
ENEMIES = {
    # Murciélago (colgado del techo, se descuelga y revolotea hacia ti): el Vengefly, el volador que persigue al
    # Caballero desde el principio de Hollow Knight (elegido por el usuario el 16 de octubre de 2026).
    "murcielago": dict(types=[0x0E], col="Buzzer", anchor="centro", k=1.5,
                       clips={"quieto": "Idle", "mover": "Fly"}),
    # Esqueleto (patrulla y salta entre plataformas): el Husk que deambula.
    "esqueleto": dict(types=[0x25], col="Zombie Basic1", anchor="pies", k=1.25,
                      clips={"quieto": "Idle", "mover": "Walk"}),
    # Caballero (patrulla con escudo y busca tu piso): el Husk con escudo.
    "caballero": dict(types=[0x0A], col="Zombie Shield", anchor="pies", k=1.25,
                      clips={"quieto": "Idle", "mover": "Walk"}),
    # Brote (aparece y desaparece): la planta trampa, cerrada (en espera está escondida bajo la hierba).
    "brote": dict(types=[0x2E], col="Plant Trap Anim", anchor="pies", k=1.5,
                  clips={"quieto": "Snap", "mover": "Snap"}),
    # Rodador: el Baldur, que rueda.
    "rodador": dict(types=[0x18], col="Roller", anchor="pies", k=1.3,
                    clips={"quieto": "Idle", "mover": "Roll"}),
    # Murciélagos que se fusionan (pequeño → mediano → grande): el Gruzzer.
    "mosca": dict(types=[0x19, 0x37], col="Fly", anchor="centro", k=1.4,
                  clips={"quieto": "Fly", "mover": "Fly"}),
    # Bicho que se arrastra: el Crawlid.
    "reptador": dict(types=[0x02, 0x2B], col="Crawler", anchor="pies", k=1.3,
                     clips={"quieto": "idle", "mover": "walk"}),
    # Erizo (dispara púas si estás cerca): el Tiktik, con su lomo de púas.
    "erizo": dict(types=[0x11], col="Climber", anchor="pies", k=1.3,
                  clips={"quieto": "Walk", "mover": "Walk"}),
    # Espíritu del polvo de hada: la Lumafly.
    "espiritu": dict(types=[0x4E], col="Glow Fly", anchor="centro", k=2.0,
                     clips={"quieto": "Bug", "mover": "Bug"}),
    # Fantasma orbitante que dispara: el Folly.
    "fantasma": dict(types=[0x26], col="Mage Balloon Anim", anchor="centro", k=1.5,
                     clips={"quieto": "Idle", "mover": "Chase"}),
    # Hechicero (se teletransporta y dispara): el Soul Twister.
    "hechicero": dict(types=[0x3C], col="Mage", anchor="pies", k=1.5,
                      clips={"quieto": "Idle", "mover": "Idle"}),
    # Estatua dormida que despierta: el Moss Knight, que duerme entre la maleza.
    "estatua": dict(types=[0x23], col="Moss Knight Anim", anchor="pies", k=1.4,
                    clips={"quieto": "Idle", "mover": "Walk"}),
    # Bocas de pared y estatuas que disparan (invulnerables): el Gulka y la torreta del Mawlek.
    "boca": dict(types=[0x3A, 0x50], col="Plant Turret", anchor="centro", k=1.4,
                 clips={"quieto": "Idle", "mover": "Shoot"}),
    "torreta": dict(types=[0x32, 0x51], col="Mawlek Turret", anchor="centro", k=1.4,
                    clips={"quieto": "Active", "mover": "Active"}),
    # Pinchos que salen del suelo: los del Coliseo.
    "pinchos": dict(types=[0x31], col="Colosseum Spike Anim", anchor="pies", k=1.0,
                    clips={"quieto": "Expand", "mover": "Expand"}),
}


def opaque(im):
    return im.getchannel("A").point(lambda v: 255 if v > ALPHA else 0)


def body_x(im):
    """Centro del cuerpo: el de masas de la silueta adelgazada (sin patas, antenas ni armas finas)."""
    a = opaque(im).filter(ImageFilter.MinFilter(9))
    bb = a.getbbox() or opaque(im).getbbox()
    if a.getbbox() is None:
        a = opaque(im)
    xs = [x for y in range(bb[1], bb[3]) for x in range(bb[0], bb[2]) if a.getpixel((x, y))]
    ys = [y for y in range(bb[1], bb[3]) for x in range(bb[0], bb[2]) if a.getpixel((x, y))]
    return sum(xs) / len(xs), sum(ys) / len(ys)


def frames(src, col, clip):
    rows = [r for r in csv.DictReader(open(src / "hollow-knight-tk2d" / col / "clip_frames.csv", encoding="utf-8")) if r["clip"] == clip]
    if not rows:
        raise SystemExit(f"No está el clip {clip!r} en {col}")
    out, seen = [], set()
    for r in rows:
        p = src / "hollow-knight-tk2d" / col / r["archivo_png"]
        if p.stem in seen:
            continue
        seen.add(p.stem)
        out.append(Image.open(p).convert("RGBA"))
    return out, float(rows[0]["fps"])


def build(src):
    for sub in ("enemigos", "enemigos_centro"):
        (RES / sub).mkdir(parents=True, exist_ok=True)
        for old in (RES / sub).glob("*.png"):
            old.unlink()
    info = []
    for key, spec in ENEMIES.items():
        clips = {a: frames(src, spec["col"], c) for a, c in spec["clips"].items()}
        rest = clips["quieto"][0][0]
        canvas = rest.size
        bb = opaque(rest).getbbox()
        cx, cy = body_x(rest)
        feet = bb[3]
        ref_h = bb[3] - bb[1]
        # Lienzo común: el mismo para todos los fotogramas (los de otro lienzo de tk2d no cuadran y se quitan).
        left = up = down = 0
        for a, (fr, _) in clips.items():
            keep = [f for f in fr if f.size == canvas]
            if len(keep) < len(fr):
                print(f"  {key}_{a}: {len(fr) - len(keep)} fotogramas con otro lienzo, fuera")
            clips[a] = (keep or fr[:1], clips[a][1])
            for f in keep:
                b = opaque(f).getbbox()
                if not b:
                    continue
                left = max(left, cx - b[0], b[2] - cx)
                up = max(up, (feet if spec["anchor"] == "pies" else cy) - b[1])
                down = max(down, b[3] - cy)
        half = int(left + 2)
        if spec["anchor"] == "pies":
            box = (cx - half, feet - up - 2, cx + half, feet)
            folder = RES / "enemigos"
        else:
            v = int(max(up, down) + 2)
            box = (cx - half, cy - v, cx + half, cy + v)
            folder = RES / "enemigos_centro"
        box = tuple(round(b) for b in box)
        for a, (fr, fps) in clips.items():
            for i, f in enumerate(fr):
                f.crop(box).transpose(Image.FLIP_LEFT_RIGHT).save(folder / f"{key}_{a}_{i:02d}.png")
            info.append(f"clip {key}_{a} {fps}")
        types = ",".join(f"{t:02X}" for t in spec["types"])
        info.append(f"enemigo {key} {types} {spec['anchor']} {spec['k']} {ref_h}")
        print(f"{key}: {spec['col']} ({', '.join(f'{a} {len(f)}' for a, (f, _) in clips.items())}), lienzo {box[2] - box[0]}×{box[3] - box[1]}")
    (RES / "enemigos" / "info.txt").write_text("\n".join(info) + "\n", encoding="utf-8")
    print(f"enemigos: {len(ENEMIES)} → {RES / 'enemigos'}")
