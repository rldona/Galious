"""Arte del castillo (mundo 00, templo de mármol): tiles, planos de fondo, piedra, puerta de mundo y marcador.

Genera los SVG en art/svg/ y los PNG en Assets/Art/Resources/Dibujado/ con los nombres que lee el juego
(docs/art/encargo/README.md). Uso: python3 tools/art/build_art.py
"""
import math
import random

from svgkit import (BRONZE, BRONZE_DARK, CASTLE, GOLD, GOLD_LIGHT, INK, TEXELS, WARM_STONE, WARM_STONE_DARK,
                    WARM_STONE_LIGHT, blur_filter, emit, noise_filter, svg)

C = CASTLE
MARBLE = "#2a3142"
MARBLE_LIGHT = "#3a4458"
MARBLE_DARK = "#1b2130"
MORTAR = "#0f131c"
INNER = "#181d29"
DEEP = "#0b0e15"


# ---------- tiles (8 × 8 px del original, 64 × 64 texels) ----------

def stone_blocks(seed, base, light, dark, mortar, cols=2, rows=2, w=8, h=8, grain=0.3):
    """Sillares de mármol que encajan al repetir el tile: la junta corre por el borde del tile y por la mitad."""
    rng = random.Random(seed)
    out = [f'<rect width="{w}" height="{h}" fill="{mortar}"/>']
    bw, bh = w / cols, h / rows
    for r in range(rows):
        for c in range(cols):
            x, y = c * bw, r * bh
            off = 0 if r % 2 == 0 else 0  # sin desplazamiento: así el tile repite sin costuras
            inset = 0.35
            out.append(f'<rect x="{x + off + inset:.2f}" y="{y + inset:.2f}" width="{bw - 2 * inset:.2f}" height="{bh - 2 * inset:.2f}" rx="0.25" fill="{base}"/>')
            # arista superior clara y sombra abajo
            out.append(f'<rect x="{x + inset:.2f}" y="{y + inset:.2f}" width="{bw - 2 * inset:.2f}" height="0.45" fill="{light}" opacity=".55"/>')
            out.append(f'<rect x="{x + inset:.2f}" y="{y + bh - inset - 0.5:.2f}" width="{bw - 2 * inset:.2f}" height="0.5" fill="{dark}" opacity=".7"/>')
            # alguna veta
            for _ in range(rng.randint(0, 2)):
                vx, vy = x + inset + rng.random() * (bw - 1), y + inset + rng.random() * (bh - 1)
                out.append(f'<path d="M{vx:.2f} {vy:.2f} l{rng.uniform(0.4, 1.2):.2f} {rng.uniform(-0.3, 0.3):.2f} l{rng.uniform(0.3, 0.9):.2f} {rng.uniform(-0.3, 0.3):.2f}" stroke="{dark}" stroke-width="0.18" fill="none" opacity=".8"/>')
    out.append(f'<rect width="{w}" height="{h}" filter="url(#grain)" opacity="{grain}"/>')
    return "".join(out)


def tile_borde():
    defs = noise_filter("grain", freq=0.9, octaves=3, seed=5, alpha=0.5, color=(0.55, 0.6, 0.72))
    return svg(8, 8, stone_blocks(11, MARBLE, MARBLE_LIGHT, MARBLE_DARK, MORTAR, grain=0.35), defs)


def tile_borde_v2():
    defs = noise_filter("grain", freq=0.9, octaves=3, seed=9, alpha=0.5, color=(0.55, 0.6, 0.72))
    body = stone_blocks(23, MARBLE, MARBLE_LIGHT, MARBLE_DARK, MORTAR, grain=0.35)
    # una grieta
    body += f'<path d="M1.2 7.6 L2.4 5.2 L2.1 3.9 L3.3 2.2" stroke="{MORTAR}" stroke-width="0.28" fill="none" stroke-linecap="round"/>'
    return svg(8, 8, body, defs)


def tile_interior():
    defs = noise_filter("grain", freq=0.9, octaves=3, seed=7, alpha=0.4, color=(0.3, 0.35, 0.45))
    return svg(8, 8, stone_blocks(31, INNER, "#232938", "#10141d", "#090c12", grain=0.3), defs)


def tile_fondo():
    defs = noise_filter("grain", freq=0.7, octaves=2, seed=8, alpha=0.3, color=(0.2, 0.22, 0.3))
    return svg(8, 8, f'<rect width="8" height="8" fill="{DEEP}"/><rect width="8" height="8" filter="url(#grain)" opacity=".5"/>', defs)


def tile_borde_suelo():
    """Borde pisable: franja clara de mármol pulido arriba, que se apaga, con briznas y polvo. Transparente abajo."""
    defs = ('<linearGradient id="rimg" x1="0" y1="0" x2="0" y2="1">'
            f'<stop offset="0" stop-color="{C["rim"]}" stop-opacity="1"/>'
            f'<stop offset="0.18" stop-color="{C["rim"]}" stop-opacity="0.85"/>'
            f'<stop offset="0.45" stop-color="{C["rim"]}" stop-opacity="0.15"/>'
            f'<stop offset="1" stop-color="{C["rim"]}" stop-opacity="0"/></linearGradient>')
    body = '<rect width="8" height="3.2" fill="url(#rimg)"/>'
    body += f'<rect width="8" height="0.55" fill="#ffffff" opacity=".85"/>'
    # adoquines sugeridos: pequeñas juntas en la franja clara
    body += f'<path d="M2.6 0.2 L2.6 1.1 M5.4 0.2 L5.4 1.1" stroke="{MARBLE}" stroke-width="0.22" opacity=".7"/>'
    # briznas y motas asomando por encima del borde (sobresalen 0, el tile acaba en y = 0)
    body += f'<path d="M1.1 1.6 q0.3 -0.9 0.2 -1.5 M6.7 1.4 q-0.2 -0.8 0.1 -1.3" stroke="{C["fog"]}" stroke-width="0.2" fill="none" opacity=".8"/>'
    return svg(8, 8, body, defs)


def tile_escalera(side):
    """Escalera de mármol en dos mitades: larguero en el lado exterior y peldaños hacia el centro."""
    rail_x = 1.2 if side == "izq" else 8 - 1.2 - 1.2
    body = f'<rect x="{rail_x}" y="0" width="1.2" height="8" fill="{MARBLE_LIGHT}" stroke="{INK}" stroke-width="0.3"/>'
    body += f'<rect x="{rail_x + 0.25}" y="0" width="0.3" height="8" fill="{C["rim"]}" opacity=".8"/>'
    x0, x1 = (rail_x + 1.2, 8) if side == "izq" else (0, rail_x)
    for y in (1.6, 5.6):
        body += f'<rect x="{x0:.2f}" y="{y}" width="{x1 - x0:.2f}" height="1.1" fill="{MARBLE_LIGHT}" stroke="{INK}" stroke-width="0.3"/>'
        body += f'<rect x="{x0:.2f}" y="{y + 0.15}" width="{x1 - x0:.2f}" height="0.3" fill="{C["rim"]}" opacity=".7"/>'
    return svg(8, 8, body)


def tile_agua():
    defs = noise_filter("grain", freq=0.5, octaves=2, seed=12, alpha=0.35, color=(0.6, 0.8, 1.0))
    body = f'<rect width="8" height="8" fill="{C["liquid"]}" opacity=".55"/><rect width="8" height="8" filter="url(#grain)" opacity=".5"/>'
    body += f'<path d="M0 1.2 q2 -0.8 4 0 t4 0" stroke="#bfe3ff" stroke-width="0.3" fill="none" opacity=".8"/>'
    body += f'<path d="M0 0.3 L8 0.3" stroke="#ffffff" stroke-width="0.35" opacity=".9"/>'
    return svg(8, 8, body, defs)


def tile_lava():
    defs = noise_filter("grain", freq=0.6, octaves=3, seed=4, alpha=0.9, color=(1.0, 0.85, 0.4))
    body = '<rect width="8" height="8" fill="#ff7320"/><rect width="8" height="8" filter="url(#grain)" opacity=".55"/>'
    body += '<path d="M0 0.4 L8 0.4" stroke="#fff0c0" stroke-width="0.5" opacity=".9"/>'
    body += '<circle cx="2.2" cy="4.5" r="0.8" fill="#ffb060" opacity=".8"/><circle cx="6" cy="6.3" r="0.5" fill="#ffd38a" opacity=".8"/>'
    return svg(8, 8, body, defs)


# ---------- planos de fondo (24 × 15 unidades = 384 × 240 px del original; a 4 texels/px → 1536 × 960) ----------

W, H = 384, 240
BG_TEXELS = 4


def column(x, y_top, y_bottom, width, fill, stroke=None, flutes=True):
    out = [f'<rect x="{x - width / 2:.1f}" y="{y_top:.1f}" width="{width:.1f}" height="{y_bottom - y_top:.1f}" fill="{fill}"'
           + (f' stroke="{stroke}" stroke-width="1"' if stroke else "") + '/>']
    cap_w = width * 1.35
    out.append(f'<rect x="{x - cap_w / 2:.1f}" y="{y_top - width * 0.35:.1f}" width="{cap_w:.1f}" height="{width * 0.4:.1f}" fill="{fill}"'
               + (f' stroke="{stroke}" stroke-width="1"' if stroke else "") + '/>')
    out.append(f'<rect x="{x - cap_w / 2:.1f}" y="{y_bottom - width * 0.3:.1f}" width="{cap_w:.1f}" height="{width * 0.35:.1f}" fill="{fill}"/>')
    if flutes:
        for k in range(1, 4):
            fx = x - width / 2 + width * k / 4
            out.append(f'<path d="M{fx:.1f} {y_top + 2:.1f} L{fx:.1f} {y_bottom - 2:.1f}" stroke="#000" stroke-opacity=".18" stroke-width="{max(0.6, width * 0.08):.2f}"/>')
    return "".join(out)


def arch(x, y_base, span, height, fill, inner=None):
    r = span / 2
    d = f'M{x - r:.1f} {y_base:.1f} L{x - r:.1f} {y_base - height + r:.1f} A{r:.1f} {r:.1f} 0 0 1 {x + r:.1f} {y_base - height + r:.1f} L{x + r:.1f} {y_base:.1f} Z'
    out = f'<path d="{d}" fill="{fill}"/>'
    if inner:
        r2 = r * 0.78
        d2 = f'M{x - r2:.1f} {y_base:.1f} L{x - r2:.1f} {y_base - height + r:.1f} A{r2:.1f} {r2:.1f} 0 0 1 {x + r2:.1f} {y_base - height + r:.1f} L{x + r2:.1f} {y_base:.1f} Z'
        out += f'<path d="{d2}" fill="{inner}"/>'
    return out


def vine(x, y_top, length, sway, seed, color, width=2.2):
    rng = random.Random(seed)
    pts = []
    y = y_top
    for k in range(6):
        y = y_top + length * (k + 1) / 6
        pts.append(f'{x + math.sin(k * 1.3 + seed) * sway:.1f} {y:.1f}')
    d = f'M{x:.1f} {y_top:.1f} C ' + ", ".join(pts[:3]) + " S " + ", ".join(pts[3:5]) + f" {pts[5]}"
    out = [f'<path d="{d}" stroke="{color}" stroke-width="{width}" fill="none" stroke-linecap="round"/>']
    for k in range(int(length / 9)):
        ly = y_top + 6 + k * 9
        lx = x + math.sin(k * 1.3 + seed) * sway * (k + 1) / 6
        side = 1 if k % 2 == 0 else -1
        out.append(f'<ellipse cx="{lx + side * 3:.1f}" cy="{ly:.1f}" rx="3.2" ry="1.8" fill="{color}" transform="rotate({side * 30} {lx + side * 3:.1f} {ly:.1f})"/>')
    return "".join(out)


def fondo_lejano():
    """Plano lejano: una arcada alta y clara, de poco contraste, fundida abajo en niebla. Transparente donde no hay nada."""
    defs = ('<linearGradient id="fade" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff" stop-opacity="1"/><stop offset=".7" stop-color="#fff" stop-opacity=".9"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></linearGradient>'
            '<mask id="m"><rect width="384" height="240" fill="url(#fade)"/></mask>'
            + noise_filter("grain", freq=0.03, octaves=3, seed=2, alpha=0.25, color=(0.7, 0.78, 0.95)))
    far, mid = C["far"], "#4c5a78"
    out = [f'<g mask="url(#m)" opacity=".85">']
    out.append(f'<rect x="0" y="150" width="384" height="90" fill="{far}" opacity=".35"/>')
    for k in range(7):
        x = 28 + k * 54
        out.append(arch(x, 190, 42, 120, far, inner=C["sky_bottom"]))
        out.append(column(x - 24, 92, 190, 7, mid, flutes=False))
    out.append(column(384 - 20, 92, 190, 7, mid, flutes=False))
    out.append(f'<rect x="0" y="86" width="384" height="8" fill="{mid}" opacity=".9"/>')
    out.append(f'<rect x="0" y="80" width="384" height="6" fill="{far}" opacity=".7"/>')
    out.append('</g>')
    out.append('<rect width="384" height="240" filter="url(#grain)" mask="url(#m)" opacity=".5"/>')
    return svg(W, H, "".join(out), defs, texels=BG_TEXELS)


def fondo_medio():
    """Plano medio: columnas y arcos más grandes y oscuros, con hornacinas y lianas que cuelgan."""
    defs = noise_filter("grain", freq=0.05, octaves=3, seed=3, alpha=0.3, color=(0.35, 0.42, 0.6))
    mid, dark = C["mid"], "#1f2740"
    out = []
    for k in range(4):
        x = 48 + k * 96
        out.append(arch(x, 230, 70, 170, mid, inner=dark))
        out.append(column(x - 40, 60, 230, 12, mid))
        # hornacina con estatua en cada otro arco
        if k % 2 == 1:
            out.append(arch(x, 200, 22, 60, dark))
            out.append(f'<path d="M{x:.0f} 150 c-6 0 -7 9 -5 16 l-3 30 h16 l-3 -30 c2 -7 1 -16 -5 -16 z" fill="{mid}" opacity=".9"/>')
            out.append(f'<circle cx="{x}" cy="147" r="5" fill="{mid}"/>')
    out.append(column(384 - 8, 60, 230, 12, mid))
    out.append(f'<rect x="0" y="52" width="384" height="10" fill="{mid}"/>')
    out.append(f'<rect x="0" y="62" width="384" height="3" fill="{dark}"/>')
    for k, x in enumerate((70, 150, 215, 300, 350)):
        out.append(vine(x, 56, 50 + (k * 23) % 60, 5, k + 1, "#121828", width=2))
    out.append('<rect width="384" height="240" filter="url(#grain)" opacity=".6"/>')
    return svg(W, H, "".join(out), defs, texels=BG_TEXELS)


def fondo_cercano():
    """Plano cercano: dos columnas grandes translúcidas y una estatua, para dar escala; muy poco detalle."""
    defs = noise_filter("grain", freq=0.06, octaves=2, seed=6, alpha=0.3, color=(0.3, 0.36, 0.5))
    near = "#25304a"
    out = [f'<g opacity=".75">']
    out.append(column(60, 20, 240, 26, near))
    out.append(column(330, 20, 240, 26, near))
    out.append(f'<path d="M196 130 c-12 0 -14 16 -10 30 l-6 60 h32 l-6 -60 c4 -14 2 -30 -10 -30 z" fill="{near}"/>')
    out.append(f'<circle cx="196" cy="124" r="10" fill="{near}"/>')
    out.append(f'<rect x="170" y="220" width="52" height="20" fill="{near}"/>')
    out.append('</g>')
    out.append('<rect width="384" height="240" filter="url(#grain)" opacity=".5"/>')
    return svg(W, H, "".join(out), defs, texels=BG_TEXELS)


def primer_plano():
    """Primer plano: siluetas negras en los márgenes (matorral abajo, columnas en los lados, lianas arriba)."""
    black = "#06080c"
    out = []
    # columnas en los bordes
    out.append(f'<rect x="0" y="0" width="22" height="240" fill="{black}"/>')
    out.append(f'<rect x="362" y="0" width="22" height="240" fill="{black}"/>')
    out.append(f'<rect x="0" y="0" width="34" height="14" fill="{black}"/><rect x="350" y="0" width="34" height="14" fill="{black}"/>')
    # lianas arriba
    for k, x in enumerate((60, 110, 250, 300, 330)):
        out.append(vine(x, -4, 34 + (k * 17) % 30, 4, k + 11, black, width=2.4))
    # matorral abajo
    rng = random.Random(5)
    for k in range(40):
        x = rng.uniform(0, 384)
        h = rng.uniform(8, 26)
        out.append(f'<path d="M{x:.1f} 240 q{rng.uniform(-6, 6):.1f} {-h / 2:.1f} {rng.uniform(-3, 3):.1f} {-h:.1f}" stroke="{black}" stroke-width="{rng.uniform(1.2, 2.4):.1f}" fill="none" stroke-linecap="round"/>')
    out.append(f'<rect x="0" y="232" width="384" height="8" fill="{black}"/>')
    return svg(W, H, "".join(out), texels=BG_TEXELS)


# ---------- objetos de sala ----------

def piedra():
    """Piedra rompible de 2 × 2 tiles: bloque de piedra cálida tallado a facetas con una grieta."""
    defs = noise_filter("grain", freq=0.5, octaves=3, seed=13, alpha=0.4, color=(0.9, 0.78, 0.62))
    d = "M2 3 L7 1.2 L14.5 2.2 L15.2 9 L13.6 14.6 L4.5 15.4 L1.2 11 Z"
    body = f'<path d="{d}" fill="{INK}" stroke="{INK}" stroke-width="1.4" stroke-linejoin="round"/>'
    body += f'<path d="{d}" fill="{WARM_STONE}"/>'
    body += f'<path d="M2 3 L7 1.2 L14.5 2.2 L12 5.5 L5 6 Z" fill="{WARM_STONE_LIGHT}"/>'
    body += f'<path d="M14.5 2.2 L15.2 9 L13.6 14.6 L12.4 9.5 L12 5.5 Z" fill="{WARM_STONE_DARK}"/>'
    body += f'<path d="M5 6 L12 5.5 L12.4 9.5 L13.6 14.6 L4.5 15.4 L3.4 9 Z" fill="{WARM_STONE}"/>'
    body += f'<path d="M8.6 2 L7.4 6.2 L9.6 8.4 L8 12.2 L9.4 15.2" stroke="{INK}" stroke-width="0.55" fill="none" stroke-linecap="round" stroke-linejoin="round"/>'
    body += f'<path d="M7.4 6.2 L9.6 8.4 L8 12.2" stroke="{WARM_STONE_LIGHT}" stroke-width="0.25" fill="none" opacity=".7" transform="translate(0.45 0.3)"/>'
    body += f'<rect width="16" height="16" filter="url(#grain)" opacity=".35" clip-path="url(#cp)"/>'
    defs += f'<clipPath id="cp"><path d="{d}"/></clipPath>'
    return svg(16, 16, body, defs)


def puerta_mundo(state):
    """Puerta de mundo de 4 × 3 tiles (32 × 24 px): pórtico griego. state 0 cerrada, 1 a medio abrir, 2 abierta."""
    defs = ('<radialGradient id="glow" cx="50%" cy="80%" r="70%"><stop offset="0" stop-color="#8fb3dd" stop-opacity=".55"/><stop offset="1" stop-color="#8fb3dd" stop-opacity="0"/></radialGradient>'
            + noise_filter("grain", freq=0.5, octaves=3, seed=21, alpha=0.4, color=(0.55, 0.6, 0.72)))
    out = []
    # columnas laterales y frontón
    for x in (0, 26):
        out.append(f'<rect x="{x}" y="6" width="6" height="18" fill="{INK}"/>')
        out.append(f'<rect x="{x + 0.5}" y="6.5" width="5" height="17.5" fill="{MARBLE_LIGHT}"/>')
        out.append(f'<rect x="{x + 1.2}" y="7" width="0.6" height="16.5" fill="{C["rim"]}" opacity=".6"/>')
        out.append(f'<rect x="{x + 3.6}" y="7" width="0.6" height="16.5" fill="{MARBLE_DARK}" opacity=".8"/>')
    out.append(f'<path d="M-0.5 7 L16 0.5 L32.5 7 Z" fill="{INK}"/><path d="M1 6.5 L16 1.6 L31 6.5 Z" fill="{MARBLE_LIGHT}"/>')
    out.append(f'<path d="M1 6.5 L16 1.6 L31 6.5" stroke="{C["rim"]}" stroke-width="0.4" fill="none" opacity=".7"/>')
    out.append(f'<rect x="0" y="6" width="32" height="1.4" fill="{MARBLE}" stroke="{INK}" stroke-width="0.4"/>')
    # hueco oscuro con brillo
    out.append(f'<rect x="6" y="7.4" width="20" height="16.6" fill="#05070b"/>')
    out.append(f'<rect x="6" y="7.4" width="20" height="16.6" fill="url(#glow)"/>')
    # losa: 0 cubre todo, 1 sube un tercio (la fila de abajo abierta), 2 subida del todo (asoma 2 px bajo el friso)
    slab_bottom = {0: 24, 1: 16, 2: 9.4}[state]
    if slab_bottom > 7.4:
        top = 7.4
        out.append(f'<rect x="6" y="{top}" width="20" height="{slab_bottom - top:.1f}" fill="{MARBLE}" stroke="{INK}" stroke-width="0.5"/>')
        # sillares
        for yy in (11.5, 15.5, 19.5):
            if yy < slab_bottom - 0.5:
                out.append(f'<path d="M6 {yy} L26 {yy}" stroke="{MORTAR}" stroke-width="0.35"/>')
        for k, xx in enumerate((11, 16, 21)):
            out.append(f'<path d="M{xx} {top} L{xx} {min(slab_bottom, 11.5):.1f} M{xx + (2.5 if k % 2 else -2.5)} 11.5 L{xx + (2.5 if k % 2 else -2.5)} {min(slab_bottom, 15.5):.1f}" stroke="{MORTAR}" stroke-width="0.35"/>')
        # bandas de bronce y el sello de la Gran Llave
        for yy in (9.0, 21.5):
            if yy < slab_bottom - 1:
                out.append(f'<rect x="6" y="{yy}" width="20" height="1.1" fill="{BRONZE}" stroke="{BRONZE_DARK}" stroke-width="0.3"/>')
        cy = min(15.5, slab_bottom - 3)
        if cy > 10:
            out.append(f'<circle cx="16" cy="{cy:.1f}" r="2.6" fill="{GOLD}" stroke="{BRONZE_DARK}" stroke-width="0.45"/>')
            out.append(f'<circle cx="16" cy="{cy:.1f}" r="1.1" fill="{GOLD_LIGHT}"/>')
            out.append(f'<path d="M16 {cy - 2.2:.1f} L16 {cy - 1.2:.1f} M16 {cy + 1.2:.1f} L16 {cy + 2.2:.1f} M13.8 {cy:.1f} L14.8 {cy:.1f} M17.2 {cy:.1f} L18.2 {cy:.1f}" stroke="{BRONZE_DARK}" stroke-width="0.4"/>')
        out.append(f'<rect x="6" y="{top}" width="20" height="{slab_bottom - top:.1f}" filter="url(#grain)" opacity=".3"/>')
    # umbral
    out.append(f'<rect x="4" y="23" width="24" height="1" fill="{C["rim"]}" opacity=".7"/>')
    return svg(32, 24, "".join(out), defs)


def tile_interior_v2():
    defs = noise_filter("grain", freq=0.9, octaves=3, seed=17, alpha=0.4, color=(0.3, 0.35, 0.45))
    body = stone_blocks(41, INNER, "#232938", "#10141d", "#090c12", grain=0.3)
    body += f'<path d="M5.4 0.6 L4.6 2.8 L5.6 4.4" stroke="{MORTAR}" stroke-width="0.25" fill="none" stroke-linecap="round"/>'
    body += f'<circle cx="2.2" cy="5.6" r="0.5" fill="{MORTAR}" opacity=".7"/>'
    return svg(8, 8, body, defs)


def tile_borde_v3():
    """Sillar con una veta de musgo y una muesca: otra variante del borde."""
    defs = noise_filter("grain", freq=0.9, octaves=3, seed=19, alpha=0.5, color=(0.55, 0.6, 0.72))
    body = stone_blocks(37, MARBLE, MARBLE_LIGHT, MARBLE_DARK, MORTAR, grain=0.35)
    body += f'<path d="M0.6 4.4 q1.5 -0.6 3.2 0.1 q1.2 0.5 2.4 -0.2" stroke="#3f5a52" stroke-width="0.5" fill="none" opacity=".8" stroke-linecap="round"/>'
    body += f'<path d="M6.2 1.0 L6.9 1.9 L6.1 2.4 Z" fill="{MORTAR}"/>'
    return svg(8, 8, body, defs)


# ---------- piezas de decoración (carpeta decoracion, pivote en el centro) ----------

def farola():
    """Aplique de pared: brazo de bronce, cuenco y llama. 8 × 12 px; la llama queda en el centro, donde está la luz."""
    defs = ('<radialGradient id="fl" cx="50%" cy="60%" r="55%"><stop offset="0" stop-color="#fff6dc"/><stop offset=".5" stop-color="#ffd59a"/><stop offset="1" stop-color="#ffd59a" stop-opacity="0"/></radialGradient>')
    out = []
    out.append(f'<path d="M1 9.5 L1 3.2 Q1 1.6 2.6 1.9 L4 2.6" stroke="{INK}" stroke-width="1.2" fill="none" stroke-linecap="round"/>')
    out.append(f'<path d="M1 9.5 L1 3.2 Q1 1.6 2.6 1.9 L4 2.6" stroke="{BRONZE}" stroke-width="0.6" fill="none" stroke-linecap="round"/>')
    out.append(f'<rect x="0.3" y="9" width="1.6" height="2.4" rx="0.3" fill="{BRONZE_DARK}" stroke="{INK}" stroke-width="0.3"/>')
    out.append(f'<path d="M2.6 7.2 L5.4 7.2 L4.9 8.6 L3.1 8.6 Z" fill="{BRONZE}" stroke="{INK}" stroke-width="0.45" stroke-linejoin="round"/>')
    out.append(f'<path d="M3.0 7.2 L5.0 7.2 L4.6 7.7 L3.4 7.7 Z" fill="{GOLD}"/>')
    out.append(f'<circle cx="4" cy="5.6" r="3.2" fill="url(#fl)" opacity=".9"/>')
    out.append(f'<path d="M4 6.9 C2.9 5.8 3.1 4.6 4 3.6 C4.9 4.6 5.1 5.8 4 6.9 Z" fill="#fff0c8" stroke="{INK}" stroke-width="0.3"/>')
    out.append(f'<path d="M4 6.4 C3.5 5.7 3.7 5.1 4 4.6 C4.3 5.1 4.5 5.7 4 6.4 Z" fill="#fff9ea"/>')
    return svg(8, 12, "".join(out), defs)


def liana(seed, h=24):
    """Liana que cuelga: tallo oscuro con hojas, 6 × h px, sin luz propia (silueta casi negra con un verde apagado)."""
    rng = random.Random(seed)
    stem, leaf = "#0e1420", "#1f3330"
    pts = [(3.0, 0.0)]
    y = 0.0
    while y < h - 2:
        y += rng.uniform(3, 5)
        pts.append((3.0 + rng.uniform(-1.2, 1.2), min(y, h - 0.5)))
    d = f"M{pts[0][0]} {pts[0][1]}" + "".join(f" L{x:.2f} {yy:.2f}" for x, yy in pts[1:])
    out = [f'<path d="{d}" stroke="{stem}" stroke-width="0.9" fill="none" stroke-linecap="round" stroke-linejoin="round"/>']
    for k, (x, yy) in enumerate(pts[1:]):
        side = 1 if k % 2 == 0 else -1
        out.append(f'<ellipse cx="{x + side * 1.3:.2f}" cy="{yy - 0.6:.2f}" rx="1.5" ry="0.8" fill="{leaf}" stroke="{stem}" stroke-width="0.35" transform="rotate({side * 35} {x + side * 1.3:.2f} {yy - 0.6:.2f})"/>')
    out.append(f'<ellipse cx="{pts[-1][0]:.2f}" cy="{pts[-1][1]:.2f}" rx="0.9" ry="1.4" fill="{leaf}" stroke="{stem}" stroke-width="0.35"/>')
    return svg(6, h, "".join(out))


def estatua():
    """Estatua en su pedestal: figura velada de mármol con una urna, 12 × 24 px; la cara la decide la paleta de la sala."""
    defs = noise_filter("grain", freq=0.5, octaves=3, seed=27, alpha=0.35, color=(0.6, 0.66, 0.78))
    out = []
    out.append(f'<rect x="1" y="19" width="10" height="5" fill="{MARBLE}" stroke="{INK}" stroke-width="0.5"/>')
    out.append(f'<rect x="0.5" y="18.4" width="11" height="1.2" fill="{MARBLE_LIGHT}" stroke="{INK}" stroke-width="0.4"/>')
    out.append(f'<path d="M6 3 C3.6 3 3 5.6 3.6 7.8 L2.4 18.4 L9.6 18.4 L8.4 7.8 C9 5.6 8.4 3 6 3 Z" fill="{MARBLE_LIGHT}" stroke="{INK}" stroke-width="0.5" stroke-linejoin="round"/>')
    out.append(f'<path d="M6 3 C3.6 3 3 5.6 3.6 7.8 L2.4 18.4 L5.2 18.4 L5.4 8 C5 6 5.2 4 6 3 Z" fill="{MARBLE}" opacity=".8"/>')
    out.append(f'<circle cx="6" cy="2.6" r="1.6" fill="{MARBLE_LIGHT}" stroke="{INK}" stroke-width="0.45"/>')
    out.append(f'<path d="M4.4 2.2 Q6 0.6 7.6 2.2" stroke="{INK}" stroke-width="0.35" fill="none"/>')
    out.append(f'<path d="M8.2 9 L10.4 9.4 L10.1 12.2 L8.6 12.2 Z" fill="{MARBLE}" stroke="{INK}" stroke-width="0.4"/>')
    out.append(f'<rect width="12" height="24" filter="url(#grain)" opacity=".4"/>')
    return svg(12, 24, "".join(out), defs)


def columna_rota():
    """Fuste de columna dórica partido por arriba, 8 × 20 px."""
    defs = noise_filter("grain", freq=0.5, octaves=3, seed=29, alpha=0.35, color=(0.6, 0.66, 0.78))
    out = []
    out.append(f'<rect x="0.5" y="17" width="7" height="3" fill="{MARBLE_LIGHT}" stroke="{INK}" stroke-width="0.45"/>')
    out.append(f'<path d="M1.4 17 L1.4 4 L2.6 2.2 L4.2 3.4 L5.4 1.4 L6.6 3 L6.6 17 Z" fill="{MARBLE}" stroke="{INK}" stroke-width="0.5" stroke-linejoin="round"/>')
    for x in (2.6, 3.8, 5.0):
        out.append(f'<path d="M{x} 4.5 L{x} 16.5" stroke="{MARBLE_DARK}" stroke-width="0.5" opacity=".8"/>')
    out.append(f'<rect width="8" height="20" filter="url(#grain)" opacity=".4"/>')
    return svg(8, 20, "".join(out), defs)


# ---------- más objetos de sala ----------

def puerta_llave():
    """Puerta con llave de 1 × 3 tiles (8 × 24 px): reja de bronce con cerradura."""
    out = [f'<rect x="0.6" y="0.4" width="6.8" height="23.2" rx="0.4" fill="#0a0d14" stroke="{INK}" stroke-width="0.5"/>']
    for x in (1.6, 4.0, 6.4):
        out.append(f'<rect x="{x - 0.45}" y="0.8" width="0.9" height="22.4" fill="{BRONZE}" stroke="{BRONZE_DARK}" stroke-width="0.2"/>')
    for y in (3, 11, 19):
        out.append(f'<rect x="0.9" y="{y}" width="6.2" height="1.1" fill="{BRONZE}" stroke="{BRONZE_DARK}" stroke-width="0.2"/>')
    out.append(f'<circle cx="4" cy="13.5" r="1.6" fill="{GOLD}" stroke="{INK}" stroke-width="0.4"/>')
    out.append(f'<path d="M4 12.8 L4 14.6 M3.4 13.2 L4.6 13.2" stroke="{BRONZE_DARK}" stroke-width="0.45"/>')
    return svg(8, 24, "".join(out))


def puerta_giratoria():
    """Puerta giratoria de 1 × 3 tiles: hoja de madera con eje de bronce."""
    out = [f'<rect x="1.2" y="0.6" width="5.6" height="22.8" rx="0.3" fill="#6b4a2a" stroke="{INK}" stroke-width="0.5"/>']
    for x in (2.6, 4.0, 5.4):
        out.append(f'<path d="M{x} 1 L{x} 23" stroke="#4a3219" stroke-width="0.35"/>')
    for y in (4, 12, 20):
        out.append(f'<rect x="1.2" y="{y}" width="5.6" height="0.9" fill="{BRONZE}" stroke="{BRONZE_DARK}" stroke-width="0.2"/>')
    out.append(f'<rect x="3.55" y="0" width="0.9" height="24" fill="{BRONZE_DARK}"/>')
    out.append(f'<circle cx="4" cy="12" r="0.9" fill="{GOLD}" stroke="{INK}" stroke-width="0.3"/>')
    return svg(8, 24, "".join(out))


def ascensor():
    """Ascensor de 32 × 16 px: losa de mármol con franja dorada y dos argollas."""
    defs = noise_filter("grain", freq=0.5, octaves=3, seed=33, alpha=0.4, color=(0.6, 0.66, 0.78))
    out = [f'<rect x="0.5" y="4" width="31" height="11.5" rx="0.6" fill="{MARBLE_LIGHT}" stroke="{INK}" stroke-width="0.6"/>']
    out.append(f'<rect x="0.5" y="4" width="31" height="1.6" fill="{C["rim"]}" opacity=".9"/>')
    out.append(f'<rect x="1" y="9" width="30" height="1.2" fill="{GOLD}" stroke="{BRONZE_DARK}" stroke-width="0.25"/>')
    for x in (6, 26):
        out.append(f'<circle cx="{x}" cy="2.6" r="1.8" fill="none" stroke="{BRONZE}" stroke-width="0.9"/>')
        out.append(f'<circle cx="{x}" cy="2.6" r="1.8" fill="none" stroke="{INK}" stroke-width="0.3"/>')
    out.append(f'<rect x="0.5" y="4" width="31" height="11.5" filter="url(#grain)" opacity=".35"/>')
    return svg(32, 16, "".join(out), defs)


# ---------- interfaz ----------

def marco():
    """Marco en 9 partes (192 × 192 texels, borde 48): filigrana blanca de doble filete con un meandro en cada esquina
    sobre negro translúcido."""
    s = 24  # dibujado a 24 × 24 "px" con 8 texels: 192
    white = "#f2efe6"
    out = [f'<rect x="1" y="1" width="{s - 2}" height="{s - 2}" rx="0.6" fill="#05070b" fill-opacity=".82"/>']
    out.append(f'<rect x="1.3" y="1.3" width="{s - 2.6}" height="{s - 2.6}" rx="0.4" fill="none" stroke="{white}" stroke-width="0.35"/>')
    out.append(f'<rect x="2.3" y="2.3" width="{s - 4.6}" height="{s - 4.6}" fill="none" stroke="{white}" stroke-width="0.18" opacity=".8"/>')
    # meandro en cada esquina, entero dentro del borde de 6 "px" (48 texels) del corte en 9 partes: lo que toca el
    # corte se estira a lo largo del lado, así que ahí solo puede haber los filetes del marco
    meander = f'<path d="M3.0 3.0 h1.8 v1.8 h-0.9 v-0.9 h-0.45 v1.35 h1.8 v-2.7" fill="none" stroke="{white}" stroke-width="0.26" stroke-linecap="square"/>'
    for tr in ("", f"rotate(90 {s / 2} {s / 2})", f"rotate(180 {s / 2} {s / 2})", f"rotate(270 {s / 2} {s / 2})"):
        out.append(f'<g transform="{tr}">{meander}</g>')
    return svg(s, s, "".join(out))


def medallon():
    """Anillo del medallón del marcador: aro dorado con filete blanco; el centro, transparente para el retrato."""
    s = 24
    out = [f'<circle cx="12" cy="12" r="10.6" fill="#05070b" fill-opacity=".75"/>']
    out.append(f'<circle cx="12" cy="12" r="10.8" fill="none" stroke="{GOLD}" stroke-width="1.1"/>')
    out.append(f'<circle cx="12" cy="12" r="11.5" fill="none" stroke="#f2efe6" stroke-width="0.3"/>')
    out.append(f'<circle cx="12" cy="12" r="10" fill="none" stroke="{BRONZE_DARK}" stroke-width="0.3" opacity=".8"/>')
    for k in range(8):
        a = k * math.pi / 4
        out.append(f'<circle cx="{12 + 10.8 * math.cos(a):.2f}" cy="{12 + 10.8 * math.sin(a):.2f}" r="0.5" fill="{GOLD_LIGHT}"/>')
    return svg(s, s, "".join(out))


def build():
    emit("mundo00_macizo_borde", tile_borde(), "tiles")
    emit("mundo00_macizo_borde_v2", tile_borde_v2(), "tiles")
    emit("mundo00_macizo_borde_v3", tile_borde_v3(), "tiles")
    emit("mundo00_macizo_interior", tile_interior(), "tiles")
    emit("mundo00_macizo_interior_v2", tile_interior_v2(), "tiles")
    emit("mundo00_farola", farola(), "decoracion")
    emit("mundo00_liana_01", liana(1, 20), "decoracion")
    emit("mundo00_liana_02", liana(2, 28), "decoracion")
    emit("mundo00_liana_03", liana(3, 36), "decoracion")
    emit("mundo00_estatua", estatua(), "decoracion")
    emit("mundo00_columna_rota", columna_rota(), "decoracion")
    emit("puerta_llave", puerta_llave(), "objetos_sala")
    emit("puerta_giratoria", puerta_giratoria(), "objetos_sala")
    emit("ascensor", ascensor(), "objetos_sala")
    emit("mundo00_macizo_fondo", tile_fondo(), "tiles")
    emit("mundo00_borde_suelo", tile_borde_suelo(), "tiles")
    emit("mundo00_escalera_izq", tile_escalera("izq"), "tiles")
    emit("mundo00_escalera_der", tile_escalera("der"), "tiles")
    emit("mundo00_agua", tile_agua(), "tiles")
    emit("mundo00_lava", tile_lava(), "tiles")
    emit("mundo00_fondo_lejano", fondo_lejano(), "fondos")
    emit("mundo00_fondo_medio", fondo_medio(), "fondos")
    emit("mundo00_fondo_cercano", fondo_cercano(), "fondos")
    emit("mundo00_primer_plano", primer_plano(), "fondos")
    emit("piedra", piedra(), "objetos_sala")
    emit("puerta_mundo_cerrada", puerta_mundo(0), "objetos_sala")
    emit("puerta_mundo_media", puerta_mundo(1), "objetos_sala")
    emit("puerta_mundo_abierta", puerta_mundo(2), "objetos_sala")
    emit("marco", marco(), "interfaz")
    emit("medallon", medallon(), "interfaz")


if __name__ == "__main__":
    build()
