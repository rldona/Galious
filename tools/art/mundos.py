"""Mundos 1 a 10: tiles, cuatro planos y decoración por tema, con la paleta de WorldPalette.cs.

Cada mundo es un tema: material del terreno, motivos de los planos de fondo y tres piezas de decoración (una luz, algo
que cuelga, algo que se apoya en el suelo). Mismos nombres y tamaños que el castillo (castillo.py), con el prefijo
mundoNN. Añadir un mundo = una entrada en THEMES con sus funciones.
"""
import math
import random

from castillo import BG_TEXELS, H, W, column, stone_blocks
from svgkit import INK, emit, noise_filter, svg

# dominante / lejano / medio / macizo / borde / niebla / acento / agua, de WorldPalette.cs
PALETTES = {
    1: dict(name="cueva", sky_top="#081619", sky_bottom="#1d4646", far="#2f6a68", mid="#173839", ground="#1b2a2c", rim="#8ee6d6", fog="#4fa39a", accent="#a6fff0", liquid="#2f8f8a"),
    2: dict(name="arrecife", sky_top="#08142a", sky_bottom="#1c4a6b", far="#2d6f90", mid="#153953", ground="#182634", rim="#99dcff", fog="#4a8fb5", accent="#b3f2ff", liquid="#2a78b8"),
    3: dict(name="guarida", sky_top="#1f1108", sky_bottom="#583213", far="#8a5521", mid="#452810", ground="#2b1b10", rim="#ffd27f", fog="#c48a3e", accent="#ffb347", liquid="#5a7f8f"),
    4: dict(name="cristal", sky_top="#130a22", sky_bottom="#3d2465", far="#6b4aa0", mid="#32205a", ground="#221833", rim="#ecd2ff", fog="#9a7ad0", accent="#f2dcff", liquid="#6a5ad0"),
    5: dict(name="ruinas", sky_top="#0a1220", sky_bottom="#22385c", far="#3b5a88", mid="#1d2d4a", ground="#1a2233", rim="#b0caff", fog="#5d7fb5", accent="#d0e0ff", liquid="#2d5fa8"),
    6: dict(name="hongos", sky_top="#09170c", sky_bottom="#23452a", far="#3f7546", mid="#1e3b24", ground="#18251a", rim="#bdf2a6", fog="#6aa86a", accent="#f2ff9e", liquid="#3a8f6a"),
    7: dict(name="abismo", sky_top="#03071a", sky_bottom="#0f1d45", far="#1d3470", mid="#0d1a3a", ground="#0e1424", rim="#7fb5ff", fog="#2a4f9a", accent="#66ffe0", liquid="#1d5fb0"),
    8: dict(name="magma", sky_top="#1c0505", sky_bottom="#5a170c", far="#8f2a12", mid="#3d0e08", ground="#241010", rim="#ffa266", fog="#f09a7a", accent="#ffb060", liquid="#3f6fa0"),
    9: dict(name="osario", sky_top="#171513", sky_bottom="#4a4540", far="#7a7268", mid="#3a3530", ground="#25221f", rim="#f2e9d8", fog="#9e9588", accent="#fff2d2", liquid="#4f7f9f"),
    10: dict(name="trono", sky_top="#120308", sky_bottom="#46101f", far="#7a1f38", mid="#380c1a", ground="#220b12", rim="#ff94b4", fog="#a3304f", accent="#ff6b8f", liquid="#5a3f9f"),
}


def mix(a, b, t):
    """Mezcla dos colores hexadecimales."""
    a, b = a.lstrip("#"), b.lstrip("#")
    r = [round(int(a[i:i + 2], 16) * (1 - t) + int(b[i:i + 2], 16) * t) for i in (0, 2, 4)]
    return "#%02x%02x%02x" % tuple(max(0, min(255, v)) for v in r)


def rgb01(hexcolor):
    h = hexcolor.lstrip("#")
    return tuple(int(h[i:i + 2], 16) / 255 for i in (0, 2, 4))


# =====================================================================================================================
# tiles: cada material recibe (seed, base, light, dark, mortar, grain_color)
# =====================================================================================================================

def rock_tile(seed, base, light, dark, mortar, grain_color):
    """Roca irregular (cueva, abismo, magma): lajas desiguales que encajan al repetir el tile."""
    rng = random.Random(seed)
    out = [f'<rect width="8" height="8" fill="{mortar}"/>']
    ys = [0, 2.6, 5.3, 8]
    for k in range(3):
        y0, y1 = ys[k], ys[k + 1]
        splits = sorted({0, 8} | {round(rng.uniform(1.5, 6.5), 1) for _ in range(rng.randint(1, 2))})
        for a, b in zip(splits, splits[1:]):
            inset = 0.3
            pts = [(a + inset, y0 + inset), (b - inset, y0 + inset + rng.uniform(-0.2, 0.2)), (b - inset + rng.uniform(-0.2, 0.1), y1 - inset), (a + inset + rng.uniform(-0.1, 0.2), y1 - inset)]
            d = "M" + " L".join(f"{x:.2f} {y:.2f}" for x, y in pts) + " Z"
            out.append(f'<path d="{d}" fill="{base}"/>')
            out.append(f'<path d="M{pts[0][0]:.2f} {pts[0][1]:.2f} L{pts[1][0]:.2f} {pts[1][1]:.2f}" stroke="{light}" stroke-width="0.4" opacity=".6"/>')
            out.append(f'<path d="M{pts[3][0]:.2f} {pts[3][1]:.2f} L{pts[2][0]:.2f} {pts[2][1]:.2f}" stroke="{dark}" stroke-width="0.5" opacity=".7"/>')
    x, y = rng.uniform(1, 7), rng.uniform(1, 7)
    out.append(f'<path d="M{x:.2f} {y:.2f} l{rng.uniform(-0.8, 0.8):.2f} {rng.uniform(0.4, 1.2):.2f} l{rng.uniform(-0.6, 0.6):.2f} {rng.uniform(0.3, 0.9):.2f}" stroke="{mortar}" stroke-width="0.22" fill="none" stroke-linecap="round"/>')
    out.append('<rect width="8" height="8" filter="url(#grain)" opacity=".4"/>')
    return svg(8, 8, "".join(out), noise_filter("grain", freq=0.9, octaves=3, seed=seed, alpha=0.5, color=grain_color))


def coral_tile(seed, base, light, dark, mortar, grain_color):
    """Caliza porosa (arrecife) u hueso con cuencas (osario): bloque con agujeros."""
    rng = random.Random(seed)
    out = [f'<rect width="8" height="8" fill="{base}"/>']
    out.append(f'<path d="M0 0.3 L8 0.3" stroke="{light}" stroke-width="0.5" opacity=".5"/><path d="M0 7.7 L8 7.7" stroke="{dark}" stroke-width="0.6" opacity=".7"/>')
    for _ in range(4):
        x, y, r = rng.uniform(1, 7), rng.uniform(1, 7), rng.uniform(0.35, 0.8)
        out.append(f'<ellipse cx="{x:.2f}" cy="{y:.2f}" rx="{r:.2f}" ry="{r * 0.8:.2f}" fill="{mortar}"/>')
        out.append(f'<path d="M{x - r:.2f} {y + r * 0.5:.2f} A{r:.2f} {r * 0.8:.2f} 0 0 0 {x + r:.2f} {y + r * 0.5:.2f}" stroke="{light}" stroke-width="0.2" fill="none" opacity=".6"/>')
    out.append('<rect width="8" height="8" filter="url(#grain)" opacity=".45"/>')
    return svg(8, 8, "".join(out), noise_filter("grain", freq=0.7, octaves=3, seed=seed, alpha=0.5, color=grain_color))


def block_tile(seed, base, light, dark, mortar, grain_color):
    """Sillares (ruinas, trono): los bloques del castillo con otro color."""
    body = stone_blocks(seed, base, light, dark, mortar, grain=0.35)
    return svg(8, 8, body, noise_filter("grain", freq=0.9, octaves=3, seed=seed, alpha=0.5, color=grain_color))


def bone_tile(seed, base, light, dark, mortar, grain_color):
    """Guarida: costillas apiladas en horizontal, de hueso oscuro."""
    rng = random.Random(seed)
    out = [f'<rect width="8" height="8" fill="{mortar}"/>']
    for k in range(3):
        y = 0.4 + k * 2.65
        out.append(f'<rect x="-0.5" y="{y:.2f}" width="9" height="1.9" rx="0.9" fill="{base}"/>')
        out.append(f'<path d="M0 {y + 0.5:.2f} L8 {y + 0.5:.2f}" stroke="{light}" stroke-width="0.35" opacity=".7"/>')
        out.append(f'<path d="M0 {y + 1.6:.2f} L8 {y + 1.6:.2f}" stroke="{dark}" stroke-width="0.35" opacity=".8"/>')
        x = rng.uniform(1, 7)
        out.append(f'<path d="M{x:.2f} {y + 0.2:.2f} l0.3 0.7 l-0.4 0.8" stroke="{dark}" stroke-width="0.2" fill="none"/>')
    out.append('<rect width="8" height="8" filter="url(#grain)" opacity=".35"/>')
    return svg(8, 8, "".join(out), noise_filter("grain", freq=0.8, octaves=3, seed=seed, alpha=0.45, color=grain_color))


def crystal_tile(seed, base, light, dark, mortar, grain_color):
    """Cristal: facetas triangulares que encajan en el tile."""
    rng = random.Random(seed)
    out = [f'<rect width="8" height="8" fill="{base}"/>']
    pts = [(0, 0), (8, 0), (8, 8), (0, 8), (4 + rng.uniform(-1, 1), 4 + rng.uniform(-1, 1)), (2, 2 + rng.uniform(-0.5, 0.5)), (6, 6 + rng.uniform(-0.5, 0.5))]
    tris = [(0, 1, 4), (1, 2, 4), (2, 3, 4), (3, 0, 4), (0, 5, 4), (2, 6, 4)]
    for k, (a, b, c) in enumerate(tris):
        fill = [base, light, dark, mix(base, light, 0.3), mix(base, dark, 0.5), light][k % 6]
        d = f"M{pts[a][0]:.2f} {pts[a][1]:.2f} L{pts[b][0]:.2f} {pts[b][1]:.2f} L{pts[c][0]:.2f} {pts[c][1]:.2f} Z"
        out.append(f'<path d="{d}" fill="{fill}" opacity=".85" stroke="{mortar}" stroke-width="0.18"/>')
    out.append(f'<path d="M1 1.5 L2.5 0.6" stroke="#ffffff" stroke-width="0.3" opacity=".6"/>')
    out.append('<rect width="8" height="8" filter="url(#grain)" opacity=".2"/>')
    return svg(8, 8, "".join(out), noise_filter("grain", freq=0.9, octaves=2, seed=seed, alpha=0.4, color=grain_color))


def wood_tile(seed, base, light, dark, mortar, grain_color):
    """Bosque de hongos: madera y raíz con vetas horizontales."""
    rng = random.Random(seed)
    out = [f'<rect width="8" height="8" fill="{base}"/>']
    for k in range(5):
        y = 0.8 + k * 1.6 + rng.uniform(-0.3, 0.3)
        out.append(f'<path d="M0 {y:.2f} Q2 {y + rng.uniform(-0.4, 0.4):.2f} 4 {y:.2f} T8 {y:.2f}" stroke="{dark}" stroke-width="0.35" fill="none" opacity=".8"/>')
    out.append(f'<ellipse cx="{rng.uniform(2, 6):.2f}" cy="{rng.uniform(2, 6):.2f}" rx="0.9" ry="0.5" fill="{dark}"/><ellipse cx="{rng.uniform(2, 6):.2f}" cy="{rng.uniform(2, 6):.2f}" rx="0.5" ry="0.3" fill="{light}" opacity=".7"/>')
    out.append(f'<path d="M0 0.3 L8 0.3" stroke="{light}" stroke-width="0.5" opacity=".4"/>')
    out.append('<rect width="8" height="8" filter="url(#grain)" opacity=".4"/>')
    return svg(8, 8, "".join(out), noise_filter("grain", freq=0.9, octaves=3, seed=seed, alpha=0.45, color=grain_color))


def rim_tile(rim, accent_bits):
    defs = ('<linearGradient id="rimg" x1="0" y1="0" x2="0" y2="1">'
            f'<stop offset="0" stop-color="{rim}" stop-opacity="1"/><stop offset="0.18" stop-color="{rim}" stop-opacity="0.85"/>'
            f'<stop offset="0.45" stop-color="{rim}" stop-opacity="0.15"/><stop offset="1" stop-color="{rim}" stop-opacity="0"/></linearGradient>')
    return svg(8, 8, '<rect width="8" height="3.2" fill="url(#rimg)"/><rect width="8" height="0.55" fill="#ffffff" opacity=".8"/>' + accent_bits, defs)


def ladder_tile(side, rail, rim):
    rail_x = 1.2 if side == "izq" else 8 - 2.4
    body = f'<rect x="{rail_x}" y="0" width="1.2" height="8" fill="{rail}" stroke="{INK}" stroke-width="0.3"/>'
    body += f'<rect x="{rail_x + 0.25}" y="0" width="0.3" height="8" fill="{rim}" opacity=".7"/>'
    x0, x1 = (rail_x + 1.2, 8) if side == "izq" else (0, rail_x)
    for y in (1.6, 5.6):
        body += f'<rect x="{x0:.2f}" y="{y}" width="{x1 - x0:.2f}" height="1.1" fill="{rail}" stroke="{INK}" stroke-width="0.3"/>'
    return svg(8, 8, body)


def water_tile(liquid, seed):
    defs = noise_filter("grain", freq=0.5, octaves=2, seed=seed, alpha=0.35, color=(0.6, 0.8, 1.0))
    body = f'<rect width="8" height="8" fill="{liquid}" opacity=".55"/><rect width="8" height="8" filter="url(#grain)" opacity=".5"/>'
    body += '<path d="M0 1.2 q2 -0.8 4 0 t4 0" stroke="#bfe3ff" stroke-width="0.3" fill="none" opacity=".8"/><path d="M0 0.3 L8 0.3" stroke="#ffffff" stroke-width="0.35" opacity=".9"/>'
    return svg(8, 8, body, defs)


def lava_tile(seed):
    defs = noise_filter("grain", freq=0.6, octaves=3, seed=seed, alpha=0.9, color=(1.0, 0.85, 0.4))
    body = '<rect width="8" height="8" fill="#ff7320"/><rect width="8" height="8" filter="url(#grain)" opacity=".55"/><path d="M0 0.4 L8 0.4" stroke="#fff0c0" stroke-width="0.5" opacity=".9"/>'
    return svg(8, 8, body, defs)


# =====================================================================================================================
# motivos de los planos
# =====================================================================================================================

def stalactite(x, y_top, length, width, fill):
    return f'<path d="M{x - width / 2:.1f} {y_top:.1f} Q{x - width * 0.1:.1f} {y_top + length * 0.6:.1f} {x:.1f} {y_top + length:.1f} Q{x + width * 0.1:.1f} {y_top + length * 0.6:.1f} {x + width / 2:.1f} {y_top:.1f} Z" fill="{fill}"/>'


def stalagmite(x, y_base, height, width, fill):
    return f'<path d="M{x - width / 2:.1f} {y_base:.1f} Q{x - width * 0.1:.1f} {y_base - height * 0.6:.1f} {x:.1f} {y_base - height:.1f} Q{x + width * 0.1:.1f} {y_base - height * 0.6:.1f} {x + width / 2:.1f} {y_base:.1f} Z" fill="{fill}"/>'


def coral_branch(x, y_base, height, fill, seed, depth=3):
    rng = random.Random(seed)
    out = []
    def branch(x0, y0, length, angle, d, width):
        x1, y1 = x0 + length * math.sin(angle), y0 - length * math.cos(angle)
        out.append(f'<path d="M{x0:.1f} {y0:.1f} L{x1:.1f} {y1:.1f}" stroke="{fill}" stroke-width="{width:.1f}" stroke-linecap="round" fill="none"/>')
        if d == 0:
            out.append(f'<circle cx="{x1:.1f}" cy="{y1:.1f}" r="{width * 0.7:.1f}" fill="{fill}"/>')
            return
        for _ in range(rng.randint(2, 3)):
            branch(x1, y1, length * rng.uniform(0.55, 0.75), angle + rng.uniform(-0.9, 0.9), d - 1, width * 0.7)
    branch(x, y_base, height * 0.4, rng.uniform(-0.2, 0.2), depth, height * 0.08)
    return "".join(out)


def kelp(x, y_base, height, fill, seed, width=3):
    pts = [(x + math.sin(k / 6 * 5 + seed) * height * 0.08, y_base - height * k / 6) for k in range(7)]
    d = f"M{pts[0][0]:.1f} {pts[0][1]:.1f} " + " ".join(f"L{px:.1f} {py:.1f}" for px, py in pts[1:])
    out = [f'<path d="{d}" stroke="{fill}" stroke-width="{width}" stroke-linecap="round" fill="none"/>']
    for k, (px, py) in enumerate(pts[1:]):
        side = 1 if k % 2 == 0 else -1
        out.append(f'<ellipse cx="{px + side * width * 1.6:.1f}" cy="{py:.1f}" rx="{width * 1.8:.1f}" ry="{width * 0.7:.1f}" fill="{fill}" transform="rotate({side * 25} {px + side * width * 1.6:.1f} {py:.1f})"/>')
    return "".join(out)


def rib_arch(x, y_base, span, height, fill, thick=6):
    """Costilla: un arco apuntado de hueso."""
    r = span / 2
    return (f'<path d="M{x - r:.1f} {y_base:.1f} Q{x - r * 0.9:.1f} {y_base - height:.1f} {x:.1f} {y_base - height - 4:.1f} '
            f'Q{x + r * 0.9:.1f} {y_base - height:.1f} {x + r:.1f} {y_base:.1f}" stroke="{fill}" stroke-width="{thick}" fill="none" stroke-linecap="round"/>')


def spine(x, y_base, height, fill, seed):
    rng = random.Random(seed)
    out = []
    for k in range(int(height / 7)):
        y = y_base - k * 7
        w = 8 - k * 0.35
        out.append(f'<ellipse cx="{x + math.sin(k * 0.4 + seed) * 4:.1f}" cy="{y:.1f}" rx="{w:.1f}" ry="3" fill="{fill}"/>')
        out.append(f'<ellipse cx="{x + math.sin(k * 0.4 + seed) * 4:.1f}" cy="{y - 3.5:.1f}" rx="{w * 0.5:.1f}" ry="2" fill="{fill}"/>')
    return "".join(out)


def crystal(x, y_base, height, width, fill, light, lean=0.0):
    tip = (x + lean * height, y_base - height)
    return (f'<path d="M{x - width / 2:.1f} {y_base:.1f} L{x - width * 0.35 + lean * height * 0.9:.1f} {y_base - height * 0.9:.1f} L{tip[0]:.1f} {tip[1]:.1f} '
            f'L{x + width * 0.35 + lean * height * 0.9:.1f} {y_base - height * 0.9:.1f} L{x + width / 2:.1f} {y_base:.1f} Z" fill="{fill}"/>'
            f'<path d="M{x - width * 0.2:.1f} {y_base:.1f} L{x - width * 0.1 + lean * height * 0.9:.1f} {y_base - height * 0.9:.1f} L{tip[0]:.1f} {tip[1]:.1f} Z" fill="{light}" opacity=".6"/>')


def crystal_cluster(x, y_base, size, fill, light, seed, up=True):
    rng = random.Random(seed)
    out = []
    sgn = 1 if up else -1
    for k in range(5):
        h = size * rng.uniform(0.5, 1.0) * sgn
        out.append(crystal(x + (k - 2) * size * 0.22, y_base, h, size * 0.22, fill, light, rng.uniform(-0.25, 0.25)))
    return "".join(out)


def mushroom(x, y_base, height, fill, cap, seed):
    rng = random.Random(seed)
    stem_w = height * 0.14
    cap_w = height * rng.uniform(0.7, 1.0)
    out = [f'<path d="M{x - stem_w / 2:.1f} {y_base:.1f} Q{x - stem_w:.1f} {y_base - height * 0.5:.1f} {x - stem_w * 0.6:.1f} {y_base - height * 0.82:.1f} L{x + stem_w * 0.6:.1f} {y_base - height * 0.82:.1f} Q{x + stem_w:.1f} {y_base - height * 0.5:.1f} {x + stem_w / 2:.1f} {y_base:.1f} Z" fill="{fill}"/>']
    out.append(f'<path d="M{x - cap_w / 2:.1f} {y_base - height * 0.78:.1f} Q{x:.1f} {y_base - height * 1.25:.1f} {x + cap_w / 2:.1f} {y_base - height * 0.78:.1f} Q{x:.1f} {y_base - height * 0.68:.1f} {x - cap_w / 2:.1f} {y_base - height * 0.78:.1f} Z" fill="{cap}"/>')
    for k in range(3):
        out.append(f'<circle cx="{x + (k - 1) * cap_w * 0.22 + rng.uniform(-2, 2):.1f}" cy="{y_base - height * 0.95 + rng.uniform(-2, 2):.1f}" r="{cap_w * 0.05:.1f}" fill="{fill}" opacity=".7"/>')
    return "".join(out)


def jellyfish(x, y, size, fill, glow):
    out = [f'<path d="M{x - size:.1f} {y:.1f} Q{x:.1f} {y - size * 1.4:.1f} {x + size:.1f} {y:.1f} Q{x:.1f} {y + size * 0.3:.1f} {x - size:.1f} {y:.1f} Z" fill="{fill}" opacity=".85"/>']
    out.append(f'<ellipse cx="{x:.1f}" cy="{y - size * 0.4:.1f}" rx="{size * 0.5:.1f}" ry="{size * 0.3:.1f}" fill="{glow}" opacity=".5"/>')
    for k in range(5):
        tx = x + (k - 2) * size * 0.4
        out.append(f'<path d="M{tx:.1f} {y:.1f} q{size * 0.2:.1f} {size * 0.8:.1f} 0 {size * 1.6:.1f} t0 {size * 1.2:.1f}" stroke="{fill}" stroke-width="{size * 0.08:.1f}" fill="none" opacity=".8"/>')
    return "".join(out)


def chain(x, y_top, length, fill, link=6):
    out = []
    for k in range(int(length / link)):
        y = y_top + k * link
        rx, ry = (link * 0.3, link * 0.55) if k % 2 == 0 else (link * 0.18, link * 0.55)
        out.append(f'<ellipse cx="{x:.1f}" cy="{y + link / 2:.1f}" rx="{rx:.1f}" ry="{ry:.1f}" fill="none" stroke="{fill}" stroke-width="{link * 0.22:.1f}"/>')
    return "".join(out)


def pointed_arch(x, y_base, span, height, fill, inner=None):
    r = span / 2
    d = f'M{x - r:.1f} {y_base:.1f} L{x - r:.1f} {y_base - height + r:.1f} Q{x - r:.1f} {y_base - height - r * 0.2:.1f} {x:.1f} {y_base - height - r * 0.6:.1f} Q{x + r:.1f} {y_base - height - r * 0.2:.1f} {x + r:.1f} {y_base - height + r:.1f} L{x + r:.1f} {y_base:.1f} Z'
    out = f'<path d="{d}" fill="{fill}"/>'
    if inner:
        r2 = r * 0.75
        d2 = f'M{x - r2:.1f} {y_base:.1f} L{x - r2:.1f} {y_base - height + r:.1f} Q{x - r2:.1f} {y_base - height - r2 * 0.2:.1f} {x:.1f} {y_base - height - r2 * 0.6:.1f} Q{x + r2:.1f} {y_base - height - r2 * 0.2:.1f} {x + r2:.1f} {y_base - height + r:.1f} L{x + r2:.1f} {y_base:.1f} Z'
        out += f'<path d="{d2}" fill="{inner}"/>'
    return out


def banner(x, y_top, width, length, fill, dark):
    return (f'<path d="M{x - width / 2:.1f} {y_top:.1f} L{x + width / 2:.1f} {y_top:.1f} L{x + width / 2:.1f} {y_top + length * 0.85:.1f} L{x:.1f} {y_top + length:.1f} L{x - width / 2:.1f} {y_top + length * 0.85:.1f} Z" fill="{fill}"/>'
            f'<path d="M{x - width / 2:.1f} {y_top + 4:.1f} L{x + width / 2:.1f} {y_top + 4:.1f}" stroke="{dark}" stroke-width="2"/>')


def skull_pile(x, y_base, width, fill, dark, seed):
    rng = random.Random(seed)
    out = []
    for k in range(int(width / 9)):
        for row in range(2):
            cx = x - width / 2 + k * 9 + row * 4.5 + 4.5
            cy = y_base - 4 - row * 7
            if row == 1 and k == int(width / 9) - 1:
                continue
            out.append(f'<circle cx="{cx:.1f}" cy="{cy:.1f}" r="4.4" fill="{fill}"/>')
            out.append(f'<circle cx="{cx - 1.5:.1f}" cy="{cy - 0.5:.1f}" r="1.1" fill="{dark}"/><circle cx="{cx + 1.5:.1f}" cy="{cy - 0.5:.1f}" r="1.1" fill="{dark}"/>')
    return "".join(out)


# =====================================================================================================================
# planos: cada tema da (lejano, medio, cercano, primer plano) como listas de SVG interno
# =====================================================================================================================

def fade_defs(seed_grain, color):
    return ('<linearGradient id="fade" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff" stop-opacity="1"/><stop offset=".7" stop-color="#fff" stop-opacity=".9"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></linearGradient>'
            '<mask id="m"><rect width="384" height="240" fill="url(#fade)"/></mask>' + noise_filter("grain", freq=0.03, octaves=3, seed=seed_grain, alpha=0.25, color=color))


def wrap_planes(t, far_items, mid_items, near_items, front_items, seed):
    """Monta los cuatro planos con niebla, grano y márgenes comunes."""
    gcol = rgb01(mix(t["far"], "#ffffff", 0.3))
    lejano = svg(W, H, f'<g mask="url(#m)" opacity=".85">{"".join(far_items)}</g><rect width="384" height="240" filter="url(#grain)" mask="url(#m)" opacity=".5"/>',
                 fade_defs(seed, gcol), texels=BG_TEXELS)
    medio = svg(W, H, "".join(mid_items) + '<rect width="384" height="240" filter="url(#grain)" opacity=".6"/>',
                noise_filter("grain", freq=0.05, octaves=3, seed=seed + 1, alpha=0.3, color=rgb01(t["fog"])), texels=BG_TEXELS)
    cercano = svg(W, H, f'<g opacity=".75">{"".join(near_items)}</g><rect width="384" height="240" filter="url(#grain)" opacity=".5"/>',
                  noise_filter("grain", freq=0.06, octaves=2, seed=seed + 2, alpha=0.3, color=rgb01(t["mid"])), texels=BG_TEXELS)
    black = mix(t["sky_top"], "#000000", 0.6)
    front = [f'<rect x="0" y="0" width="20" height="240" fill="{black}"/><rect x="364" y="0" width="20" height="240" fill="{black}"/>']
    front += front_items
    front.append(f'<rect x="0" y="232" width="384" height="8" fill="{black}"/>')
    primer = svg(W, H, "".join(front), texels=BG_TEXELS)
    return lejano, medio, cercano, primer


def planes_cueva(t):
    far, mid, near = t["far"], t["mid"], mix(t["mid"], t["ground"], 0.4)
    rng = random.Random(1)
    f = [stalactite(10 + k * 21 + rng.uniform(-6, 6), 0, rng.uniform(30, 110), rng.uniform(8, 18), far) for k in range(18)]
    f += [stalagmite(20 + k * 38 + rng.uniform(-8, 8), 240, rng.uniform(40, 120), rng.uniform(14, 30), far) for k in range(10)]
    m = [f'<path d="M{x - 14:.1f} 240 Q{x - 10:.1f} 120 {x - 6:.1f} 0 L{x + 6:.1f} 0 Q{x + 10:.1f} 120 {x + 14:.1f} 240 Z" fill="{mid}"/>' for x in (40 + k * 80 + rng.uniform(-10, 10) for k in range(5))]
    m += [stalactite(15 + k * 44 + rng.uniform(-8, 8), 0, rng.uniform(40, 90), rng.uniform(16, 28), mid) for k in range(9)]
    m += [stalagmite(30 + k * 64 + rng.uniform(-10, 10), 240, rng.uniform(30, 70), rng.uniform(18, 34), mid) for k in range(6)]
    n = [stalactite(70, 0, 140, 60, near), stalagmite(320, 240, 150, 70, near)]
    black = mix(t["sky_top"], "#000000", 0.6)
    p = [stalactite(40 + k * 50 + rng.uniform(-10, 10), -2, rng.uniform(16, 40), rng.uniform(10, 20), black) for k in range(7)]
    p += [stalagmite(rng.uniform(0, 384), 242, rng.uniform(8, 24), rng.uniform(10, 24), black) for _ in range(12)]
    return wrap_planes(t, f, m, n, p, 2)


def planes_arrecife(t):
    far, mid, near = t["far"], t["mid"], mix(t["mid"], t["ground"], 0.4)
    rng = random.Random(7)
    f = [f'<rect x="0" y="170" width="384" height="70" fill="{far}" opacity=".35"/>']
    f += [coral_branch(20 + k * 42 + rng.uniform(-6, 6), 230, rng.uniform(70, 130), far, k + 10) for k in range(9)]
    f += [kelp(45 + k * 60, 240, rng.uniform(120, 200), far, k + 20, 2.5) for k in range(6)]
    m = []
    for k in range(4):
        x = 48 + k * 96
        m.append(column(x - 40, 60, 240, 12, mid, flutes=False))
        m.append(coral_branch(x, 240, rng.uniform(90, 150), mid, k + 30, depth=4))
    m += [kelp(30 + k * 80, 240, rng.uniform(140, 220), mid, k + 40, 3.5) for k in range(5)]
    n = [coral_branch(70, 240, 200, near, 50, depth=4), coral_branch(330, 240, 180, near, 51, depth=4)]
    black = mix(t["sky_top"], "#000000", 0.6)
    p = [kelp(30 + k * 66 + rng.uniform(-10, 10), 242, rng.uniform(20, 48), black, k + 60, 2.5) for k in range(6)]
    p += [coral_branch(rng.uniform(0, 384), 242, rng.uniform(18, 36), black, k + 70, depth=2) for k in range(5)]
    return wrap_planes(t, f, m, n, p, 8)


def planes_guarida(t):
    far, mid, near = t["far"], t["mid"], mix(t["mid"], t["ground"], 0.4)
    rng = random.Random(13)
    f = [rib_arch(40 + k * 60, 240, 70, rng.uniform(120, 190), far, 5) for k in range(6)]
    f += [spine(70 + k * 90, 236, rng.uniform(60, 120), far, k + 3) for k in range(4)]
    m = [rib_arch(x, 240, 120, rng.uniform(150, 210), mid, 12) for x in (60, 190, 320)]
    m += [spine(x, 240, 180, mid, k + 9) for k, x in enumerate((120, 260))]
    m += [stalactite(20 + k * 50 + rng.uniform(-8, 8), 0, rng.uniform(20, 50), rng.uniform(6, 10), mid) for k in range(8)]
    n = [rib_arch(192, 240, 300, 230, near, 24), spine(40, 240, 220, near, 21)]
    black = mix(t["sky_top"], "#000000", 0.6)
    p = [stalactite(40 + k * 48 + rng.uniform(-8, 8), -2, rng.uniform(14, 34), rng.uniform(5, 9), black) for k in range(7)]
    p += [stalagmite(rng.uniform(0, 384), 242, rng.uniform(8, 22), rng.uniform(4, 8), black) for _ in range(14)]
    return wrap_planes(t, f, m, n, p, 14)


def planes_cristal(t):
    far, mid, near = t["far"], t["mid"], mix(t["mid"], t["ground"], 0.4)
    lf, lm = mix(far, "#ffffff", 0.35), mix(mid, "#ffffff", 0.3)
    rng = random.Random(17)
    f = [crystal_cluster(25 + k * 45 + rng.uniform(-8, 8), 240, rng.uniform(60, 130), far, lf, k + 1) for k in range(9)]
    f += [crystal_cluster(50 + k * 70, 0, rng.uniform(40, 90), far, lf, k + 20, up=False) for k in range(5)]
    m = [crystal_cluster(x, 240, rng.uniform(110, 180), mid, lm, k + 40) for k, x in enumerate((50, 150, 260, 350))]
    m += [crystal_cluster(x, 0, rng.uniform(60, 110), mid, lm, k + 50, up=False) for k, x in enumerate((100, 210, 310))]
    n = [crystal_cluster(60, 240, 230, near, mix(near, "#ffffff", 0.2), 60), crystal_cluster(330, 240, 200, near, mix(near, "#ffffff", 0.2), 61)]
    black = mix(t["sky_top"], "#000000", 0.6)
    p = [crystal_cluster(rng.uniform(20, 364), 242, rng.uniform(14, 30), black, black, k + 70) for k in range(8)]
    p += [crystal_cluster(rng.uniform(40, 344), -2, rng.uniform(12, 26), black, black, k + 80, up=False) for k in range(5)]
    return wrap_planes(t, f, m, n, p, 18)


def planes_ruinas(t):
    from castillo import arch
    far, mid, near = t["far"], t["mid"], mix(t["mid"], t["ground"], 0.4)
    rng = random.Random(23)
    f = [f'<rect x="0" y="150" width="384" height="90" fill="{far}" opacity=".35"/>']
    for k in range(7):
        x = 28 + k * 54
        if k % 3 == 1:
            f.append(column(x - 24, rng.uniform(120, 160), 190, 7, far, flutes=False))      # columna rota
        else:
            f.append(arch(x, 190, 42, 120, far, inner=t["sky_bottom"]))
            f.append(column(x - 24, 92, 190, 7, far, flutes=False))
    f += [kelp(60 + k * 70, 240, rng.uniform(60, 110), far, k + 5, 2) for k in range(5)]
    m = []
    for k in range(4):
        x = 48 + k * 96
        if k == 2:
            m.append(column(x - 40, 130, 240, 12, mid))
            m.append(f'<path d="M{x - 52} 130 L{x - 28} 118 L{x - 30} 134 Z" fill="{mid}"/>')
        else:
            m.append(arch(x, 230, 70, 170, mid, inner=mix(mid, "#000000", 0.3)))
            m.append(column(x - 40, 60, 230, 12, mid))
    m += [kelp(x, 240, rng.uniform(80, 150), mid, k + 11, 3) for k, x in enumerate((90, 200, 300))]
    n = [column(60, 20, 240, 26, near), f'<path d="M300 240 L300 120 L330 110 L332 240 Z" fill="{near}"/>', kelp(340, 240, 200, near, 31, 4)]
    black = mix(t["sky_top"], "#000000", 0.6)
    p = [kelp(30 + k * 66 + rng.uniform(-10, 10), 242, rng.uniform(20, 50), black, k + 60, 2.5) for k in range(6)]
    p += [f'<rect x="{rng.uniform(30, 330):.0f}" y="{242 - rng.uniform(8, 20):.0f}" width="{rng.uniform(14, 30):.0f}" height="30" fill="{black}" transform="rotate({rng.uniform(-15, 15):.0f} 200 240)"/>' for _ in range(4)]
    return wrap_planes(t, f, m, n, p, 24)


def planes_hongos(t):
    far, mid, near = t["far"], t["mid"], mix(t["mid"], t["ground"], 0.4)
    rng = random.Random(29)
    f = [mushroom(20 + k * 42 + rng.uniform(-6, 6), 240, rng.uniform(60, 130), far, mix(far, "#ffffff", 0.15), k + 1) for k in range(9)]
    m = [mushroom(x, 240, rng.uniform(130, 200), mid, mix(mid, t["accent"], 0.15), k + 20) for k, x in enumerate((50, 160, 270, 360))]
    m += [f'<path d="M{x} 0 Q{x + 10} 40 {x - 5} 80" stroke="{mid}" stroke-width="4" fill="none"/>' for x in (90, 220, 320)]
    n = [mushroom(70, 240, 260, near, mix(near, t["accent"], 0.1), 40), mushroom(330, 240, 220, near, mix(near, t["accent"], 0.1), 41)]
    black = mix(t["sky_top"], "#000000", 0.6)
    p = [mushroom(rng.uniform(20, 364), 242, rng.uniform(14, 32), black, black, k + 60) for k in range(7)]
    p += [f'<path d="M{x} -2 Q{x + 6} 20 {x - 4} 38" stroke="{black}" stroke-width="2.4" fill="none"/>' for x in (60, 130, 250, 320)]
    return wrap_planes(t, f, m, n, p, 30)


def planes_abismo(t):
    far, mid, near = t["far"], t["mid"], mix(t["mid"], t["ground"], 0.4)
    glow = t["accent"]
    rng = random.Random(31)
    f = [kelp(20 + k * 36, 240, rng.uniform(100, 200), far, k + 1, 2) for k in range(10)]
    f += [jellyfish(rng.uniform(30, 350), rng.uniform(30, 150), rng.uniform(8, 16), far, glow) for _ in range(5)]
    m = [kelp(x, 240, rng.uniform(150, 230), mid, k + 20, 4) for k, x in enumerate((40, 120, 210, 300, 360))]
    m += [jellyfish(rng.uniform(40, 340), rng.uniform(40, 140), rng.uniform(16, 26), mid, glow) for _ in range(3)]
    n = [jellyfish(90, 70, 40, near, glow), kelp(330, 240, 230, near, 44, 6)]
    black = mix(t["sky_top"], "#000000", 0.6)
    p = [kelp(30 + k * 60 + rng.uniform(-10, 10), 242, rng.uniform(24, 56), black, k + 60, 2.5) for k in range(7)]
    return wrap_planes(t, f, m, n, p, 32)


def planes_magma(t):
    far, mid, near = t["far"], t["mid"], mix(t["mid"], t["ground"], 0.4)
    rng = random.Random(37)
    f = [stalactite(10 + k * 24 + rng.uniform(-6, 6), 0, rng.uniform(30, 100), rng.uniform(8, 16), far) for k in range(16)]
    f += [chain(60 + k * 70, 0, rng.uniform(80, 160), far, 5) for k in range(5)]
    f.append(f'<rect x="0" y="200" width="384" height="40" fill="{t["accent"]}" opacity=".25"/>')
    m = [stalactite(15 + k * 44 + rng.uniform(-8, 8), 0, rng.uniform(50, 110), rng.uniform(16, 28), mid) for k in range(9)]
    m += [chain(x, 0, rng.uniform(120, 200), mid, 7) for x in (70, 190, 310)]
    m += [f'<path d="M{x - 14:.1f} 240 Q{x - 10:.1f} 120 {x - 6:.1f} 0 L{x + 6:.1f} 0 Q{x + 10:.1f} 120 {x + 14:.1f} 240 Z" fill="{mid}"/>' for x in (30, 350)]
    n = [stalactite(80, 0, 150, 60, near), chain(300, 0, 230, near, 10)]
    black = mix(t["sky_top"], "#000000", 0.6)
    p = [stalactite(40 + k * 50 + rng.uniform(-10, 10), -2, rng.uniform(16, 40), rng.uniform(8, 18), black) for k in range(7)]
    p += [chain(rng.uniform(40, 340), -2, rng.uniform(20, 50), black, 5) for _ in range(3)]
    p += [stalagmite(rng.uniform(0, 384), 242, rng.uniform(8, 22), rng.uniform(8, 20), black) for _ in range(10)]
    return wrap_planes(t, f, m, n, p, 38)


def planes_osario(t):
    far, mid, near = t["far"], t["mid"], mix(t["mid"], t["ground"], 0.4)
    dark_f, dark_m = mix(far, "#000000", 0.5), mix(mid, "#000000", 0.5)
    rng = random.Random(41)
    f = [rib_arch(28 + k * 54, 200, 44, rng.uniform(90, 130), far, 6) for k in range(7)]
    f += [skull_pile(50 + k * 80, 240, rng.uniform(40, 80), far, dark_f, k + 1) for k in range(5)]
    f += [column(x, 100, 200, 7, far, flutes=False) for x in (14, 370)]
    m = [rib_arch(x, 240, 110, rng.uniform(160, 200), mid, 14) for x in (60, 192, 324)]
    m += [skull_pile(x, 240, rng.uniform(70, 120), mid, dark_m, k + 20) for k, x in enumerate((110, 280))]
    m += [column(x, 50, 240, 12, mid) for x in (126, 258)]
    n = [rib_arch(192, 240, 300, 230, near, 26), skull_pile(60, 240, 110, near, mix(near, "#000000", 0.5), 40)]
    black = mix(t["sky_top"], "#000000", 0.6)
    p = [skull_pile(rng.uniform(40, 340), 242, rng.uniform(18, 40), black, black, k + 60) for k in range(6)]
    p += [stalactite(40 + k * 60, -2, rng.uniform(10, 24), rng.uniform(4, 8), black) for k in range(6)]
    return wrap_planes(t, f, m, n, p, 42)


def planes_trono(t):
    far, mid, near = t["far"], t["mid"], mix(t["mid"], t["ground"], 0.4)
    rng = random.Random(43)
    f = [pointed_arch(28 + k * 54, 190, 42, 130, far, inner=t["sky_bottom"]) for k in range(7)]
    f += [column(4 + k * 54, 70, 190, 7, far, flutes=False) for k in range(8)]
    f.append(f'<rect x="0" y="60" width="384" height="10" fill="{far}" opacity=".9"/>')
    m = [pointed_arch(x, 230, 70, 180, mid, inner=mix(mid, "#000000", 0.3)) for x in (48, 144, 240, 336)]
    m += [column(x - 40, 40, 230, 12, mid) for x in (48, 144, 240, 336)]
    m += [banner(x, 30, 26, rng.uniform(90, 140), mix(mid, t["accent"], 0.25), mid) for x in (96, 192, 288)]
    n = [column(60, 10, 240, 26, near), column(330, 10, 240, 26, near), banner(192, 0, 60, 200, near, mix(near, "#000000", 0.3))]
    black = mix(t["sky_top"], "#000000", 0.6)
    p = [banner(x, -2, 22, rng.uniform(30, 60), black, black) for x in (70, 160, 250, 330)]
    p += [f'<rect x="{rng.uniform(30, 330):.0f}" y="{242 - rng.uniform(6, 16):.0f}" width="{rng.uniform(16, 36):.0f}" height="20" fill="{black}"/>' for _ in range(5)]
    return wrap_planes(t, f, m, n, p, 44)


# =====================================================================================================================
# decoración: luz (8 × 12), colgante (6 × h), apoyada (12 × 24)
# =====================================================================================================================

def glow_defs(accent):
    return f'<radialGradient id="fl" cx="50%" cy="55%" r="55%"><stop offset="0" stop-color="#ffffff"/><stop offset=".45" stop-color="{accent}"/><stop offset="1" stop-color="{accent}" stop-opacity="0"/></radialGradient>'


def lamp_crystal(t):
    a = t["accent"]
    out = [f'<circle cx="4" cy="6" r="3.6" fill="url(#fl)" opacity=".85"/>']
    for x, h, w in ((2.6, 5.5, 1.6), (4.2, 7.5, 2.0), (5.8, 4.5, 1.4)):
        out.append(f'<path d="M{x - w / 2} 11 L{x} {11 - h} L{x + w / 2} 11 Z" fill="{a}" stroke="{INK}" stroke-width="0.35" stroke-linejoin="round" paint-order="stroke" opacity=".95"/>')
        out.append(f'<path d="M{x - w / 4} 10.5 L{x - 0.1} {11 - h + 0.8}" stroke="#ffffff" stroke-width="0.3" opacity=".8"/>')
    out.append(f'<rect x="1" y="10.6" width="6" height="1.2" fill="{t["ground"]}" stroke="{INK}" stroke-width="0.3"/>')
    return svg(8, 12, "".join(out), glow_defs(a))


def lamp_anemone(t):
    a = t["accent"]
    out = [f'<circle cx="4" cy="6" r="3.8" fill="url(#fl)" opacity=".8"/>']
    for k in range(9):
        ang = math.pi + k * math.pi / 8
        x1, y1 = 4 + 3.2 * math.cos(ang), 6.5 + 3.2 * math.sin(ang)
        out.append(f'<path d="M4 7 Q{(4 + x1) / 2 + 0.4:.2f} {(7 + y1) / 2:.2f} {x1:.2f} {y1:.2f}" stroke="{a}" stroke-width="0.7" fill="none" stroke-linecap="round"/>')
        out.append(f'<circle cx="{x1:.2f}" cy="{y1:.2f}" r="0.45" fill="#ffffff"/>')
    out.append(f'<ellipse cx="4" cy="8" rx="2.4" ry="1.4" fill="{t["far"]}" stroke="{INK}" stroke-width="0.35"/>')
    return svg(8, 12, "".join(out), glow_defs(a))


def lamp_brazier(t):
    """Brasero de bronce con llama del color del acento (guarida, magma, trono)."""
    a = t["accent"]
    out = [f'<circle cx="4" cy="5.5" r="3.6" fill="url(#fl)" opacity=".85"/>']
    out.append(f'<path d="M1.6 7.4 L6.4 7.4 L5.6 9.6 L2.4 9.6 Z" fill="#b07a2c" stroke="{INK}" stroke-width="0.4" stroke-linejoin="round"/>')
    out.append(f'<rect x="3.5" y="9.6" width="1" height="2" fill="#6e3a1c" stroke="{INK}" stroke-width="0.25"/><rect x="2.2" y="11.3" width="3.6" height="0.7" fill="#6e3a1c"/>')
    out.append(f'<path d="M4 7.2 C2.6 5.8 2.9 4.2 4 2.6 C5.1 4.2 5.4 5.8 4 7.2 Z" fill="{a}" stroke="{INK}" stroke-width="0.3"/>')
    out.append(f'<path d="M4 6.6 C3.4 5.8 3.6 5 4 4.2 C4.4 5 4.6 5.8 4 6.6 Z" fill="#ffffff" opacity=".9"/>')
    return svg(8, 12, "".join(out), glow_defs(a))


def lamp_mushroom(t):
    a = t["accent"]
    out = [f'<circle cx="4" cy="5" r="3.8" fill="url(#fl)" opacity=".8"/>']
    out.append(f'<path d="M3.2 11.5 Q2.8 8 3.4 6 L4.6 6 Q5.2 8 4.8 11.5 Z" fill="{t["rim"]}" stroke="{INK}" stroke-width="0.35"/>')
    out.append(f'<path d="M0.8 6.4 Q4 1.4 7.2 6.4 Q4 5.2 0.8 6.4 Z" fill="{a}" stroke="{INK}" stroke-width="0.4" stroke-linejoin="round"/>')
    for x in (2.4, 4.2, 5.8):
        out.append(f'<circle cx="{x}" cy="4.6" r="0.4" fill="#ffffff" opacity=".9"/>')
    return svg(8, 12, "".join(out), glow_defs(a))


def lamp_jelly(t):
    a = t["accent"]
    out = [f'<circle cx="4" cy="4.5" r="3.8" fill="url(#fl)" opacity=".8"/>']
    out.append(f'<path d="M1 5 Q4 0.6 7 5 Q4 6 1 5 Z" fill="{a}" opacity=".9" stroke="{INK}" stroke-width="0.3"/>')
    for k in range(4):
        x = 1.8 + k * 1.5
        out.append(f'<path d="M{x} 5.4 q0.6 2 0 3.6 t0 2.6" stroke="{a}" stroke-width="0.4" fill="none" opacity=".8"/>')
    return svg(8, 12, "".join(out), glow_defs(a))


def lamp_candle_skull(t):
    a = t["accent"]
    out = [f'<circle cx="4" cy="4" r="3.6" fill="url(#fl)" opacity=".8"/>']
    out.append(f'<path d="M1.4 11.6 L1.4 8 Q1.4 5.2 4 5.2 Q6.6 5.2 6.6 8 L6.6 11.6 Z" fill="{t["rim"]}" stroke="{INK}" stroke-width="0.4"/>')
    out.append(f'<ellipse cx="2.9" cy="8.6" rx="0.8" ry="1" fill="{INK}"/><ellipse cx="5.1" cy="8.6" rx="0.8" ry="1" fill="{INK}"/>')
    out.append(f'<rect x="3.5" y="2.6" width="1" height="2.8" fill="#f2e9d8" stroke="{INK}" stroke-width="0.25"/>')
    out.append(f'<path d="M4 2.8 C3.4 2.1 3.6 1.3 4 0.6 C4.4 1.3 4.6 2.1 4 2.8 Z" fill="{a}"/>')
    return svg(8, 12, "".join(out), glow_defs(a))


def hang_stalactite(t, seed, h):
    rng = random.Random(seed)
    fill, light = mix(t["ground"], "#000000", 0.4), mix(t["ground"], t["rim"], 0.2)
    out = [f'<path d="M0.4 0 Q{3 + rng.uniform(-0.6, 0.6):.1f} {h * 0.5:.1f} 3 {h - 0.3} Q{3 + rng.uniform(-0.6, 0.6):.1f} {h * 0.5:.1f} 5.6 0 Z" fill="{fill}"/>']
    out.append(f'<path d="M1.6 0.5 Q2.6 {h * 0.4:.1f} 2.9 {h - 1.5:.1f}" stroke="{light}" stroke-width="0.5" fill="none" opacity=".8"/>')
    out.append(f'<circle cx="3" cy="{h - 0.2:.1f}" r="0.45" fill="{t["accent"]}" opacity=".8"/>')
    return svg(6, h, "".join(out))


def hang_kelp(t, seed, h):
    fill = mix(t["ground"], "#000000", 0.4)
    return svg(6, h, f'<g transform="translate(0 {h}) scale(1 -1)">{kelp(3, h, h - 1, fill, seed, 1.2)}</g>')


def hang_chain(t, seed, h):
    return svg(6, h, chain(3, 0, h, mix(t["ground"], t["rim"], 0.3), 3))


def hang_root(t, seed, h):
    rng = random.Random(seed)
    fill = mix(t["ground"], "#000000", 0.4)
    pts = " ".join(f"L{3 + math.sin(k * 1.1 + seed) * 1.6:.1f} {k * h / 6:.1f}" for k in range(1, 7))
    out = [f'<path d="M3 0 {pts}" stroke="{fill}" stroke-width="1.1" fill="none" stroke-linecap="round"/>']
    out.append(f'<path d="M3 {h * 0.5:.1f} q-2 1 -2.4 3" stroke="{fill}" stroke-width="0.6" fill="none"/>')
    return svg(6, h, "".join(out))


def hang_banner(t, seed, h):
    return svg(6, h, banner(3, 0, 5, h, mix(t["mid"], t["accent"], 0.2), t["mid"]))


def hang_sinew(t, seed, h):
    fill = mix(t["ground"], t["rim"], 0.25)
    out = [f'<path d="M2 0 Q3.5 {h * 0.4:.1f} 3 {h - 2:.1f}" stroke="{fill}" stroke-width="1" fill="none"/>', f'<path d="M4 0 Q2.5 {h * 0.5:.1f} 3 {h - 2:.1f}" stroke="{fill}" stroke-width="0.7" fill="none"/>']
    out.append(f'<path d="M1.8 {h - 2:.1f} L3 {h - 0.2:.1f} L4.2 {h - 2:.1f} Z" fill="{t["rim"]}" opacity=".9"/>')
    return svg(6, h, "".join(out))


def hang_tentacle(t, seed, h):
    fill = mix(t["far"], "#000000", 0.2)
    pts = " ".join(f"L{3 + math.sin(k * 0.9 + seed) * 1.8:.1f} {k * h / 6:.1f}" for k in range(1, 7))
    out = [f'<path d="M3 0 {pts}" stroke="{fill}" stroke-width="1.3" fill="none" stroke-linecap="round"/>']
    for k in range(1, 6):
        out.append(f'<circle cx="{3 + math.sin(k * 0.9 + seed) * 1.8:.1f}" cy="{k * h / 6:.1f}" r="0.5" fill="{t["accent"]}" opacity=".7"/>')
    return svg(6, h, "".join(out))


def stand_stalagmite(t, seed):
    out = [stalagmite(6, 24, 23, 11, t["ground"]), f'<path d="M4.5 23 Q5.5 14 6 3" stroke="{mix(t["ground"], t["rim"], 0.2)}" stroke-width="0.7" fill="none" opacity=".8"/>']
    out.append(f'<path d="M6 1.5 L6.6 3.5 L5.4 3.5 Z" fill="{t["accent"]}" opacity=".9"/>')
    return svg(12, 24, "".join(out))


def stand_coral(t, seed):
    return svg(12, 24, coral_branch(6, 24, 22, t["far"], seed, depth=4) + f'<ellipse cx="6" cy="23.2" rx="4" ry="1" fill="{t["ground"]}" stroke="{INK}" stroke-width="0.3"/>')


def stand_bone_spike(t, seed):
    fill = mix(t["ground"], t["rim"], 0.3)
    out = [f'<path d="M2 24 L4.5 4 L6 0.6 L7.5 4 L10 24 Z" fill="{fill}" stroke="{INK}" stroke-width="0.4" stroke-linejoin="round"/>', f'<path d="M5 22 L5.6 5" stroke="{t["rim"]}" stroke-width="0.4" opacity=".6"/>']
    return svg(12, 24, "".join(out))


def stand_crystal(t, seed):
    return svg(12, 24, crystal_cluster(6, 24, 22, mix(t["ground"], t["rim"], 0.35), t["rim"], seed) + f'<ellipse cx="6" cy="23.4" rx="5" ry="0.8" fill="{t["ground"]}"/>')


def stand_broken_column(t, seed):
    fill, light = mix(t["ground"], t["rim"], 0.25), mix(t["ground"], t["rim"], 0.4)
    out = [f'<rect x="0.5" y="21" width="11" height="3" fill="{light}" stroke="{INK}" stroke-width="0.4"/>', f'<path d="M2.4 21 L2.4 6 L4 3 L6 5 L7.6 2 L9.6 4.5 L9.6 21 Z" fill="{fill}" stroke="{INK}" stroke-width="0.45" stroke-linejoin="round"/>']
    for x in (4.2, 6, 7.8):
        out.append(f'<path d="M{x} 6.5 L{x} 20.5" stroke="{mix(fill, "#000000", 0.3)}" stroke-width="0.5" opacity=".8"/>')
    return svg(12, 24, "".join(out))


def stand_mushroom(t, seed):
    return svg(12, 24, mushroom(6, 24, 20, mix(t["ground"], t["rim"], 0.3), mix(t["mid"], t["accent"], 0.3), seed))


def stand_tall_kelp(t, seed):
    return svg(12, 24, kelp(6, 24, 23, mix(t["far"], "#000000", 0.2), seed, 1.5))


def stand_obsidian(t, seed):
    fill = mix(t["ground"], "#000000", 0.4)
    out = [f'<path d="M1.5 24 L3.5 8 L6 0.8 L8.5 9 L10.5 24 Z" fill="{fill}" stroke="{INK}" stroke-width="0.4" stroke-linejoin="round"/>']
    out.append(f'<path d="M4 20 L5.2 6" stroke="{t["accent"]}" stroke-width="0.5" opacity=".7"/>')
    return svg(12, 24, "".join(out))


def stand_skulls(t, seed):
    return svg(12, 24, f'<g transform="scale(0.5) translate(0 0)">{skull_pile(12, 48, 24, t["rim"], INK, seed)}</g>')


def stand_pillar(t, seed):
    fill, light = mix(t["ground"], t["rim"], 0.25), mix(t["ground"], t["rim"], 0.45)
    out = [f'<rect x="1" y="21" width="10" height="3" fill="{light}" stroke="{INK}" stroke-width="0.4"/>', f'<rect x="3" y="1.5" width="6" height="19.5" fill="{fill}" stroke="{INK}" stroke-width="0.45"/>',
           f'<rect x="1.6" y="0.5" width="8.8" height="1.6" fill="{light}" stroke="{INK}" stroke-width="0.35"/>', f'<path d="M6 4 L6 19" stroke="{t["accent"]}" stroke-width="0.5" opacity=".5"/>']
    return svg(12, 24, "".join(out))


# =====================================================================================================================
# temas
# =====================================================================================================================

THEMES = {
    1: dict(tile=rock_tile, planes=planes_cueva, lamp=lamp_crystal, hang=hang_stalactite, stand=stand_stalagmite, hang_h=(16, 24, 32)),
    2: dict(tile=coral_tile, planes=planes_arrecife, lamp=lamp_anemone, hang=hang_kelp, stand=stand_coral, hang_h=(18, 26, 34)),
    3: dict(tile=bone_tile, planes=planes_guarida, lamp=lamp_brazier, hang=hang_sinew, stand=stand_bone_spike, hang_h=(14, 22, 30)),
    4: dict(tile=crystal_tile, planes=planes_cristal, lamp=lamp_crystal, hang=hang_stalactite, stand=stand_crystal, hang_h=(14, 20, 28)),
    5: dict(tile=block_tile, planes=planes_ruinas, lamp=lamp_anemone, hang=hang_kelp, stand=stand_broken_column, hang_h=(18, 26, 34)),
    6: dict(tile=wood_tile, planes=planes_hongos, lamp=lamp_mushroom, hang=hang_root, stand=stand_mushroom, hang_h=(16, 24, 32)),
    7: dict(tile=rock_tile, planes=planes_abismo, lamp=lamp_jelly, hang=hang_tentacle, stand=stand_tall_kelp, hang_h=(20, 28, 36)),
    8: dict(tile=rock_tile, planes=planes_magma, lamp=lamp_brazier, hang=hang_chain, stand=stand_obsidian, hang_h=(16, 24, 32)),
    9: dict(tile=coral_tile, planes=planes_osario, lamp=lamp_candle_skull, hang=hang_chain, stand=stand_skulls, hang_h=(14, 22, 30)),
    10: dict(tile=block_tile, planes=planes_trono, lamp=lamp_brazier, hang=hang_banner, stand=stand_pillar, hang_h=(18, 26, 34)),
}


def build():
    n = 0
    for idx, th in THEMES.items():
        t = PALETTES[idx]
        m = f"mundo{idx:02d}"
        grain = rgb01(mix(t["ground"], t["rim"], 0.5))
        base, light, dark = t["ground"], mix(t["ground"], t["rim"], 0.22), mix(t["ground"], "#000000", 0.35)
        inner, inner_l, inner_d = mix(t["ground"], "#000000", 0.4), mix(t["ground"], "#000000", 0.25), mix(t["ground"], "#000000", 0.6)
        mortar = mix(t["ground"], "#000000", 0.6)
        tile = th["tile"]
        emit(f"{m}_macizo_borde", tile(idx * 11, base, light, dark, mortar, grain), "tiles")
        emit(f"{m}_macizo_borde_v2", tile(idx * 11 + 1, base, light, dark, mortar, grain), "tiles")
        emit(f"{m}_macizo_borde_v3", tile(idx * 11 + 2, base, light, dark, mortar, grain), "tiles")
        emit(f"{m}_macizo_interior", tile(idx * 11 + 3, inner, inner_l, inner_d, mix(inner, "#000000", 0.5), grain), "tiles")
        emit(f"{m}_macizo_interior_v2", tile(idx * 11 + 4, inner, inner_l, inner_d, mix(inner, "#000000", 0.5), grain), "tiles")
        deep = mix(t["ground"], "#000000", 0.7)
        emit(f"{m}_macizo_fondo", svg(8, 8, f'<rect width="8" height="8" fill="{deep}"/><rect width="8" height="8" filter="url(#grain)" opacity=".5"/>', noise_filter("grain", freq=0.7, octaves=2, seed=idx, alpha=0.3, color=rgb01(t["mid"]))), "tiles")
        bits = f'<circle cx="1.8" cy="1.4" r="0.35" fill="{t["accent"]}" opacity=".8"/><circle cx="6.3" cy="1.1" r="0.3" fill="{t["accent"]}" opacity=".8"/>'
        emit(f"{m}_borde_suelo", rim_tile(t["rim"], bits), "tiles")
        rail = mix(t["ground"], t["rim"], 0.35)
        emit(f"{m}_escalera_izq", ladder_tile("izq", rail, t["rim"]), "tiles")
        emit(f"{m}_escalera_der", ladder_tile("der", rail, t["rim"]), "tiles")
        emit(f"{m}_agua", water_tile(t["liquid"], idx * 5), "tiles")
        emit(f"{m}_lava", lava_tile(idx * 7), "tiles")
        lejano, medio, cercano, primer = th["planes"](t)
        emit(f"{m}_fondo_lejano", lejano, "fondos")
        emit(f"{m}_fondo_medio", medio, "fondos")
        emit(f"{m}_fondo_cercano", cercano, "fondos")
        emit(f"{m}_primer_plano", primer, "fondos")
        emit(f"{m}_farola", th["lamp"](t), "decoracion")
        for k, h in enumerate(th["hang_h"], 1):
            emit(f"{m}_liana_{k:02d}", th["hang"](t, k + idx * 3, h), "decoracion")
        emit(f"{m}_estatua", th["stand"](t, idx), "decoracion")
        emit(f"{m}_columna_rota", th["stand"](t, idx + 50), "decoracion")
        n += 1
    return n


if __name__ == "__main__":
    print(build(), "mundos")
