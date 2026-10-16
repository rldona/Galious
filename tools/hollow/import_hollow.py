#!/usr/bin/env python3
"""Importa los protagonistas de Hollow Knight como Popolon (el Caballero) y Aphrodite (Hornet), y sus efectos (efectos.py).

Homenaje a Maze of Galious + Hollow Knight. El arte es de Team Cherry: se lee de la carpeta decodificada del usuario y
los PNG salen a Assets/Art/Resources/Hollow/ (fuera de git, como lo derivado de la ROM). No se versiona ni se reparte.

Uso: .venv/bin/python -I tools/hollow/import_hollow.py [carpeta "Hollow Knight Decodificado"]
Necesita Pillow (.venv/bin/pip install pillow).

Cada fotograma sale con el nombre que entiende ArtLibrary (<personaje>_<accion>_<NN>[_golpe].png) en un lienzo fijo por
personaje: los pies del personaje en reposo en el borde de abajo y su centro en el centro (el ArtImporter pone el pivote
abajo en el centro) a 8 texels por píxel del original, mirando a la derecha (los de Hollow Knight miran a la izquierda).
El Caballero va a ×1,375: su cuerpo sin los cuernos mide 16 px, lo que el sprite de Popolon del original (22 px con ellos).
"""
import csv
import sys
from pathlib import Path

from PIL import Image, ImageFilter

sys.path.insert(0, str(Path(__file__).resolve().parent))
import efectos  # noqa: E402
import enemigos  # noqa: E402
import objetos  # noqa: E402
import hud  # noqa: E402
import escenario  # noqa: E402
import liquidos  # noqa: E402
import menu  # noqa: E402
import bancos  # noqa: E402

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/personajes"
DEFAULT_SRC = Path.home() / "workspace/games/Hollow Knight Decodificado"
ALPHA = 40   # opacidad mínima para medir
# Acciones que se centran fotograma a fotograma por su centro de masas en vez de por las piernas del reposo: en la
# escalera el motor alinea al jugador con su centro y el dibujo tiene que quedar en medio (la pose de Hornet agarrada
# a la pared tiene el cuerpo ladeado).
CENTERED = {"escalera"}

# Acciones de CharacterArt: quieto, correr, girar, salto_preparar, salto_subir, salto_caer, escalera, estocada (con el
# fotograma "golpe"), golpe (recibido), muerte; y estela (el tajo, encima del cuerpo, en el mismo lienzo).
# Cada acción: (colección, clip, filtro de fichero o lista de ficheros, fps, índice del fotograma "golpe" o None).
CHARACTERS = {
    "popolon": {
        "collection": "Knight",
        "rest": "idle_still_020000",
        # Sin los cuernos (34 de sus 128 texels en reposo), el cuerpo mide lo que el sprite de Popolon del original:
        # 16 px. Con los cuernos, 22 px (antes iba a 1,0 y se veía más pequeño que el original).
        "scale": 1.375,
        "actions": {
            "quieto": ("Knight", "Idle", None, 12, None),
            "correr": ("Knight", "Run", None, 12, None),     # 6 fotogramas de arranque y el bucle
            "girar": ("Knight", "Turn", None, 20, None),
            "salto_preparar": ("Knight", "Land", ["land0001"], 12, None),
            "salto_subir": ("Knight", "Airborne", lambda f: f.startswith("jump"), 16, None),
            "salto_caer": ("Knight", "Fall", None, 12, None),   # 3 de arranque y el bucle
            # Hollow Knight no tiene escaleras: el Caballero de espaldas, dando pasitos, de cuando entra por una puerta.
            "escalera": ("Knight", "Enter", [f"into_door{n:04d}" for n in range(2, 10)], 12, None),
            "estocada": ("Knight", "Slash", ["slash_left_longer0000", "slash_left_longer0002", "slash_left_longer0003",
                                             "slash_left_longer0004", "slash_left_longer0005"], 24, 1),
            "estela": ("Knight", "SlashEffect", ["slashes_effect0001", "slashes_effect0002"], 24, None),
            "golpe": ("Knight", "Recoil", None, 24, None),
            "muerte": ("Knight", "Death", None, 10, None),
            # Nadando en la superficie del agua (RemakeRules.HollowSwim). Hornet no nada en Hollow Knight: sin estos, corre
            # o se queda quieta flotando.
            "nadar": ("Knight", "Surface Swim", None, 12, None),
            "nadar_quieto": ("Knight", "Surface Idle", lambda f: f.startswith("water_surface_idle"), 12, None),
            # En el banco (RemakeRules.HollowBenches): sentarse, sentado, levantarse y despertar al reaparecer.
            "sentarse": ("Knight", "Sit", None, 12, None),
            "sentado": ("Knight", "Sit Idle", None, 12, None),
            "levantarse": ("Knight", "Wake", None, 16, None),          # se rehace abajo: del sentado al de pie
            "despertar": ("Knight", "Wake", None, 12, None),           # se rehace abajo: dormido en el banco y sentarse
        },
        "explicit": {
            "levantarse": ("Knight", ["sit0003", "sit0004", "land0000", "land0001", "land0002"], 16, None),
            "despertar": ("Knight", ["wake0001", "wake0002", "wake0003", "wake0004", "wake0004", "wake0004", "sit0003",
                                     "sit0002"], 10, None),
        },
    },
    "aphrodite": {
        "collection": "Hornet Boss Anim",
        "rest": "idle0000",
        # Hornet mide 213 texels en reposo y el Caballero 128 (1,66 veces más). Con su proporción (37 px) era demasiado grande
        # para las salas de Galious y con 24 px parecía de su tamaño: a 1,125 mide 240 texels (30 px), 1,36 veces el Caballero.
        "scale": 1.125,
        "actions": {
            "quieto": ("Hornet Boss Anim", "Idle", None, 12, None),
            "correr": ("Hornet Boss Anim", "Run", None, 12, None),
            "girar": ("Hornet Anim", "Turn", None, 18, None),
            "salto_preparar": ("Hornet Boss Anim", "Jump Antic", ["hard_land0001", "jump0000"], 12, None),
            "salto_subir": ("Hornet Boss Anim", "Jump", ["jump0001", "jump0002", "jump0003", "jump0004", "jump0005"], 12, None),
            "salto_caer": ("Hornet Boss Anim", "Fall", None, 12, None),
            "escalera": ("Hornet Boss Anim", "Wall Impact", None, 12, None),   # se rehace abajo, en ida y vuelta
            "estocada": ("Hornet Boss Anim", "G Dash Antic Q", None, 24, None),   # se rehace abajo con la acometida
            "golpe": ("Hornet Boss Anim", "Stun", ["stun_ground0000", "stun_ground0001", "stun_ground0002"], 18, None),
            "muerte": ("Hornet Boss Anim", "Death Air", None, 10, None),
        },
        # La estocada de Hornet: el final de la preparación, la acometida con la aguja (golpe) y la recogida.
        "explicit": {
            # Escalera: agarrada a la pared (Wall Impact), con las piernas colgando. Sus 4 fotogramas cambian poco: en ida
            # y vuelta, para que se note al subir (avanzan con la altura, como en el original).
            "escalera": ("Hornet Boss Anim", ["wall_impact0000", "wall_impact0001", "wall_impact0002", "wall_impact0003",
                                              "wall_impact0002", "wall_impact0001"], 12, None),
            "estocada": ("Hornet Boss Anim", ["hornet_dash0005", "hornet_dash0006", "hornet_dash0007", "hornet_dash0008",
                                              "hornet_dash0010", "hornet_dash0012"], 24, 2),
        },
    },
}


def rows_for(src, collection):
    return list(csv.DictReader(open(src / "hollow-knight-tk2d" / collection / "clip_frames.csv", encoding="utf-8")))


def loop_start(src, collection, clip, frames):
    """Fotograma de nuestra lista donde empieza el bucle del clip (clips.csv solo lo trae para el Caballero), o 0."""
    path = src / "hollow-knight-sprites" / "clips.csv"
    if not path.exists():
        return 0
    for r in csv.DictReader(open(path, encoding="utf-8")):
        if r["clip"] == clip and r["colecciones"] == collection and r["wrap_mode"] == "LoopSection":
            rows = [x for x in rows_for(src, collection) if x["clip"] == clip]
            first = Path(rows[int(r["loop_start"])]["archivo_png"]).stem
            names = [n for n, _ in frames]
            return names.index(first) if first in names else 0
    return 0


def frames_for(src, collection, clip, pick):
    rows = [r for r in rows_for(src, collection) if r["clip"] == clip]
    if not rows:
        raise SystemExit(f"No está el clip {clip!r} en {collection}")
    files, seen = [], set()
    for r in rows:
        name = Path(r["archivo_png"]).stem
        if name in seen:            # Hollow Knight repite fotogramas para alargarlos; aquí manda el fps
            continue
        seen.add(name)
        files.append((name, src / "hollow-knight-tk2d" / collection / r["archivo_png"]))
    if isinstance(pick, list):
        byname = dict(files)
        return [(n, byname[n]) for n in pick]
    if callable(pick):
        return [(n, p) for n, p in files if pick(n)]
    return files


def opaque_bbox(im):
    return im.getchannel("A").point(lambda a: 255 if a > ALPHA else 0).getbbox()


def mass_x(im):
    """Centro del cuerpo: el de masas de la silueta adelgazada 4 texels por lado, que borra lo fino (la aguja de Hornet,
    el aguijón del Caballero, las piernas) y deja la máscara y la capa."""
    a = im.getchannel("A").point(lambda v: 255 if v > ALPHA else 0).filter(ImageFilter.MinFilter(9))
    bb = a.getbbox()
    xs = [x for y in range(bb[1], bb[3]) for x in range(bb[0], bb[2]) if a.getpixel((x, y))]
    return sum(xs) / len(xs)


def anchor(im):
    """Pies (fila de abajo) y centro de las piernas en la pose de reposo.

    Las piernas son lo que toca el suelo cerca del centro de masas del dibujo: así no cuenta la punta de la aguja de
    Hornet, que en reposo apoya en el suelo a su izquierda (con ella, Hornet salía 8 px corrida de su caja).
    """
    bb = opaque_bbox(im)
    feet = bb[3]
    a = im.getchannel("A")
    xs = [x for y in range(bb[1], bb[3]) for x in range(bb[0], bb[2]) if a.getpixel((x, y)) > ALPHA]
    mass = sum(xs) / len(xs)
    cols = [x for x in range(im.width) if abs(x - mass) <= 30 and any(a.getpixel((x, y)) > ALPHA for y in range(feet - 12, feet))]
    return (min(cols) + max(cols)) / 2, feet, feet - bb[1]


def build(src, name, spec):
    col = spec["collection"]
    rest = src / "hollow-knight-tk2d" / col / "sprites" / f"{spec['rest']}.png"
    rest_im = Image.open(rest).convert("RGBA")
    ax, feet, height = anchor(rest_im)
    print(f"{name}: centro de las piernas x = {ax:.1f}, pies y = {feet} (lienzo {rest_im.size[0]}×{rest_im.size[1]})")
    canvas = rest_im.size
    s = spec["scale"]

    plan, loops = {}, {}
    for action, (c, clip, pick, fps, golpe) in spec["actions"].items():
        if action in spec.get("explicit", {}):
            c, names, fps, golpe = spec["explicit"][action]
            byname = {Path(r["archivo_png"]).stem: src / "hollow-knight-tk2d" / c / r["archivo_png"] for r in rows_for(src, c)}
            frames = [(n, byname[n]) for n in names]
        else:
            frames = frames_for(src, c, clip, pick)
        loops[action] = 0 if action in spec.get("explicit", {}) else loop_start(src, c, clip, frames)
        plan[action] = (frames, golpe)

    # Lienzo común: el mismo para todos los fotogramas, simétrico alrededor del centro, con los pies abajo.
    imgs = {}
    left = up = 0
    for action, (frames, _) in plan.items():
        for n, p in frames:
            im = Image.open(p).convert("RGBA")
            if im.size != canvas:
                raise SystemExit(f"{p.name}: lienzo {im.size}, se esperaba {canvas} (otro ancla)")
            imgs[p] = im
            bb = opaque_bbox(im)
            if bb:
                left = max(left, ax - bb[0], bb[2] - ax)
                up = max(up, feet - bb[1])
    half = int((left * s) + 4)
    top = int(up * s + 4)
    w, h = half * 2, top

    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob(f"{name}_*.png"):
        old.unlink()
    count = 0
    for action, (frames, golpe) in plan.items():
        for i, (n, p) in enumerate(frames):
            im = imgs[p]
            # Recorte en coordenadas del original: centro ax, pies en 'feet' (lo que quede por debajo se pierde).
            cx = mass_x(im) if action in CENTERED else ax
            box = (cx - half / s, feet - top / s, cx + half / s, feet)
            crop = im.crop(tuple(round(v) for v in box))
            if s != 1.0:
                crop = crop.resize((w, h), Image.LANCZOS)
            else:
                crop = crop.resize((w, h), Image.NEAREST) if crop.size != (w, h) else crop
            crop = crop.transpose(Image.FLIP_LEFT_RIGHT)
            tag = "_golpe" if golpe == i else ""
            crop.save(OUT / f"{name}_{action}_{i:02d}{tag}.png")
            count += 1
    print(f"{name}: {count} fotogramas en {len(plan)} acciones, lienzo {w}×{h} texels "
          f"({w / 8:.0f}×{h / 8:.0f} px), reposo {height * s:.0f} texels de alto")
    fps = {a: f for a, ((_, _, _, f, _)) in spec["actions"].items()} | {a: v[2] for a, v in spec.get("explicit", {}).items()}
    return {a: (fps[a], loops[a]) for a in plan}


def main(argv):
    src = Path(argv[0]) if argv else DEFAULT_SRC
    if not (src / "hollow-knight-tk2d").is_dir():
        raise SystemExit(f"No encuentro hollow-knight-tk2d en {src}")
    fps = {}
    for name, spec in CHARACTERS.items():
        for action, info in build(src, name, spec).items():
            fps[f"{name}_{action}"] = info
    # Velocidad de cada clip y primer fotograma de su bucle, para CharacterArt (ArtLibrary no lo saca del nombre).
    lines = [f"{k} {f} {loop}" for k, (f, loop) in sorted(fps.items())]
    (OUT / "fps.txt").write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"→ {OUT}")
    efectos.build(src)
    enemigos.build(src)
    objetos.build(src)
    hud.build(src)
    escenario.build(src)
    liquidos.build(src)
    menu.build(src)
    bancos.build(src)


if __name__ == "__main__":
    main(sys.argv[1:])
