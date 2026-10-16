"""Efectos de Hollow Knight para Maze of Knight: chispas, impactos, polvo y grietas (Visuals/HollowFx en Unity).

Salen a Assets/Art/Resources/Hollow/efectos/ (fuera de git, como el resto del arte de Team Cherry):
  - animaciones ya recortadas en la carpeta tk2d: <nombre>_NN.png y su velocidad en fps.txt;
  - piezas sueltas del atlas de impactos de los enemigos (EnemyHitEffects), que viene sin recortes: se separan por
    manchas (lo opaco, ensanchado 4 texels, conectado) y cada pieza se recorta solo con su mancha, sin las vecinas.
A 8 texels por píxel del original, como los personajes; el tamaño en pantalla lo pone HollowFx.
"""
import csv
from pathlib import Path

from PIL import Image, ImageFilter

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/efectos"

# Animaciones: nombre → (colección tk2d, clip).
CLIPS = {
    "chispas": ("Sparks Anim", "Sparks"),          # chispas de metal (el escudo que para un golpe)
    "grieta": ("Crack Effect Anim", "Crack"),      # rayo blanco (el jugador herido)
    "polvo_salto": ("Dust", "Jump Puff"),          # polvo hacia arriba (puertas, piedras rotas)
    "polvo_pared": ("Dust", "Wall Puff"),          # nube de polvo (golpe a una piedra)
    "polvo_dash": ("Dust", "Dash Puff"),           # nube alargada (piedra rota)
}

# Piezas del atlas EnemyHitEffects: nombre → caja aproximada (x0, y0, x1, y1) de su mancha en el atlas de 1024 × 2048.
PIECES = {
    "destello": (176, 1778, 534, 2042),            # el gran destello en estrella del impacto
    "destello_raya": (646, 824, 834, 966),         # destello pequeño con su raya
    "raya": (174, 1370, 826, 1408),                # la raya larga del tajo
    "abanico_a": (176, 1630, 482, 1756),           # abanico de chispas hacia un lado
    "abanico_b": (580, 1630, 870, 1756),           # y hacia el otro
    "gotas": (778, 1234, 876, 1344),               # gotas blancas
}


def blobs(alpha):
    """Manchas del atlas: lo opaco ensanchado 4 texels, en una rejilla de 2 texels. Devuelve [(caja, puntos)]."""
    a = alpha.point(lambda v: 255 if v > 20 else 0).filter(ImageFilter.MaxFilter(9))
    w, h = a.size
    px = a.load()
    seen, out = set(), []
    for y in range(0, h, 2):
        for x in range(0, w, 2):
            if not px[x, y] or (x, y) in seen:
                continue
            stack, pts = [(x, y)], []
            seen.add((x, y))
            while stack:
                cx, cy = stack.pop()
                pts.append((cx, cy))
                for nx, ny in ((cx + 2, cy), (cx - 2, cy), (cx, cy + 2), (cx, cy - 2)):
                    if 0 <= nx < w and 0 <= ny < h and px[nx, ny] and (nx, ny) not in seen:
                        seen.add((nx, ny))
                        stack.append((nx, ny))
            xs, ys = [p[0] for p in pts], [p[1] for p in pts]
            out.append(((min(xs), min(ys), max(xs) + 2, max(ys) + 2), set(pts)))
    return out


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    fps = {}
    tk2d = src / "hollow-knight-tk2d"
    for name, (col, clip) in CLIPS.items():
        rows = [r for r in csv.DictReader(open(tk2d / col / "clip_frames.csv", encoding="utf-8")) if r["clip"] == clip]
        if not rows:
            raise SystemExit(f"No está el clip {clip!r} en {col}")
        frames = [Image.open(tk2d / col / r["archivo_png"]).convert("RGBA") for r in rows]
        # Un recorte común a todos los fotogramas, centrado en el lienzo de tk2d (su pivote).
        boxes = [f.getbbox() for f in frames if f.getbbox()]
        w, h = frames[0].size
        half_w = max(max(w / 2 - b[0], b[2] - w / 2) for b in boxes)
        half_h = max(max(h / 2 - b[1], b[3] - h / 2) for b in boxes)
        crop = tuple(round(v) for v in (w / 2 - half_w, h / 2 - half_h, w / 2 + half_w, h / 2 + half_h))
        for i, f in enumerate(frames):
            f.crop(crop).save(OUT / f"{name}_{i:02d}.png")
        fps[name] = float(rows[0]["fps"])
    atlas = Image.open(src / "hollow-knight-texturas/atlas_tk2d/EnemyHitEffects_atlas0.png").convert("RGBA")
    found = blobs(atlas.getchannel("A"))
    for name, want in PIECES.items():
        box, pts = min(found, key=lambda b: sum(abs(p - q) for p, q in zip(b[0], want)))
        if sum(abs(p - q) for p, q in zip(box, want)) > 24:
            raise SystemExit(f"No encuentro la pieza {name} en el atlas (la más parecida: {box})")
        piece = atlas.crop(box)
        mask = Image.new("L", piece.size, 0)
        mp = mask.load()
        for y in range(piece.height):
            for x in range(piece.width):
                if ((box[0] + x) & ~1, (box[1] + y) & ~1) in pts:
                    mp[x, y] = 255
        clean = Image.new("RGBA", piece.size, (0, 0, 0, 0))
        clean.paste(piece, (0, 0), mask)
        clean.save(OUT / f"{name}.png")
    (OUT / "fps.txt").write_text("".join(f"{k} {v}\n" for k, v in sorted(fps.items())), encoding="utf-8")
    print(f"efectos: {len(CLIPS)} animaciones y {len(PIECES)} piezas del atlas → {OUT}")
