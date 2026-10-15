"""Popolon dibujado: fotogramas de todas sus acciones a partir de una pose paramétrica.

Lienzo de 48 × 32 px del original (384 × 256 texels), pies en el borde de abajo y en el centro (x = 24), mirando a la
derecha; el juego lo voltea. La caja de contacto es 13 × 13 px (x ± 6, y − 14 … y − 2) y la hoja de la espada llega a
x + 18 en el fotograma «golpe» (docs/art/encargo/cajas_colision.md). El escudo va en capa aparte, por metal.
"""
import math

from svgkit import (BONE, BONE_SHADE, BRONZE, BRONZE_DARK, CAPE, CAPE_LIGHT, CREST, CREST_LIGHT, GOLD, GOLD_LIGHT, HOLE,
                    INK, SHELL, SHELL_LIGHT, STEEL, emit, svg)

CX, FLOOR = 24.0, 32.0
SW = 0.55  # grosor del trazo de tinta (px del original)

METALS = {"cobre": (BRONZE, BRONZE_DARK, "#e0a860"), "plata": ("#c9ced8", "#6f7684", "#ffffff"), "oro": (GOLD, BRONZE_DARK, GOLD_LIGHT)}

# Popolon: yelmo corintio con cresta roja transversal, capa granate, un sol en el escudo.
# Aphrodite: yelmo con alas, melena dorada que asoma por detrás, capa violeta, una rosa en el escudo.
STYLES = {
    "popolon": dict(cape=CAPE, cape_light=CAPE_LIGHT, crest=True, wings=False, hair=False, emblem="sol"),
    "aphrodite": dict(cape="#4a2a6e", cape_light="#6c3f96", crest=False, wings=True, hair=True, emblem="rosa"),
}
HAIR, HAIR_LIGHT = "#d9a441", "#f2cf7a"
WING, WING_SHADE = "#f4f1ea", "#cfc8bb"


def P(d, fill, sw=SW, extra=""):
    return f'<path d="{d}" fill="{fill}" stroke="{INK}" stroke-width="{sw}" stroke-linejoin="round" stroke-linecap="round" paint-order="stroke" {extra}/>'


def L(x0, y0, x1, y1, color, w, cap="round"):
    return f'<path d="M{x0:.2f} {y0:.2f} L{x1:.2f} {y1:.2f}" stroke="{color}" stroke-width="{w}" stroke-linecap="{cap}" fill="none"/>'


class Pose:
    def __init__(self, **k):
        self.lean = k.get("lean", 0.0)          # inclinación del tronco hacia delante (px arriba)
        self.crouch = k.get("crouch", 0.0)      # cuánto baja el cuerpo
        self.bob = k.get("bob", 0.0)            # rebote de la cabeza (px, positivo = arriba)
        self.front = k.get("front", (1.0, 0.0))  # pie delantero: (dx, alto)
        self.back = k.get("back", (-1.0, 0.0))   # pie trasero
        self.cape = k.get("cape", 0.4)          # cuánto vuela la capa hacia atrás
        self.cape_lift = k.get("cape_lift", 0.0)  # cuánto sube la punta de la capa
        self.sword = k.get("sword", "guard")    # guard | thrust | half | up | none | ladder
        self.shield = k.get("shield", True)
        self.ladder = k.get("ladder", 0)        # 0 no; 1 o 2: fase de los brazos de espaldas
        self.hurt = k.get("hurt", False)
        self.dead = k.get("dead", 0.0)          # 0..1 caído
        self.cheer = k.get("cheer", False)
        self.crest = k.get("crest", 0.0)        # vaivén de la cresta
        self.turn = k.get("turn", 0.0)          # 0..1 de frente al girarse


def body_parts(p, st=None):
    """Devuelve (capa, cuerpo sin escudo, escudos por metal) como SVG. Todo en px del original."""
    st = st or STYLES["popolon"]
    yfoot = FLOOR
    hip_y = yfoot - 8 + p.crouch
    torso_top = hip_y - 7
    cx = CX + p.lean * 0.35
    cape_svg, body, shields = [], [], {m: [] for m in METALS}

    # --- capa (detrás) ---
    sx, sy = cx - 1.5 - p.lean * 0.2, torso_top + 1.5
    tail_x = sx - 5 - p.cape * 4
    tail_y = yfoot - 3 - p.cape_lift * 4 - p.cape * 1.5
    mid_x = sx - 3 - p.cape * 2
    d = (f"M{sx:.2f} {sy:.2f} C{sx - 2:.2f} {sy + 4:.2f} {mid_x:.2f} {sy + 8:.2f} {tail_x:.2f} {tail_y:.2f} "
         f"L{tail_x + 3.5 + p.cape:.2f} {tail_y + 1.2 - p.cape_lift:.2f} C{cx - 2:.2f} {sy + 9:.2f} {cx:.2f} {sy + 5:.2f} {cx + 1.5:.2f} {sy:.2f} Z")
    cape_svg.append(P(d, st["cape"]))
    cape_svg.append(f'<path d="M{sx - 0.5:.2f} {sy + 1:.2f} C{sx - 2:.2f} {sy + 5:.2f} {mid_x + 0.5:.2f} {sy + 8:.2f} {tail_x + 1.5:.2f} {tail_y - 0.5:.2f}" stroke="{st["cape_light"]}" stroke-width="0.7" fill="none" opacity=".8"/>')

    # --- pierna trasera ---
    def leg(foot, hipx):
        fx, fh = foot
        kx = (hipx + cx + fx) / 2 + (0.6 if fh > 0.3 else 0.2)
        ky = (hip_y + yfoot - fh) / 2 - fh * 0.3
        d = f"M{hipx:.2f} {hip_y:.2f} Q{kx:.2f} {ky:.2f} {cx + fx:.2f} {yfoot - fh:.2f}"
        out = f'<path d="{d}" stroke="{INK}" stroke-width="{2.6 + SW * 2:.2f}" fill="none" stroke-linecap="round"/>'
        out += f'<path d="{d}" stroke="{SHELL}" stroke-width="2.6" fill="none" stroke-linecap="round"/>'
        # pie: bota corta
        bx, by = cx + fx, yfoot - fh
        out += P(f"M{bx - 1.6:.2f} {by - 1.4:.2f} L{bx + 2.2:.2f} {by - 1.4:.2f} L{bx + 2.4:.2f} {by:.2f} L{bx - 1.8:.2f} {by:.2f} Z", SHELL_LIGHT)
        return out

    body.append(leg(p.back, cx - 1.2))

    # --- tronco: coraza ---
    tl = p.lean * 0.5
    d = (f"M{cx - 3.6 + tl:.2f} {torso_top:.2f} L{cx + 3.4 + tl:.2f} {torso_top:.2f} "
         f"Q{cx + 4.4 + tl * 0.5:.2f} {torso_top + 3.5:.2f} {cx + 3.2:.2f} {hip_y + 0.6:.2f} "
         f"L{cx - 3.0:.2f} {hip_y + 0.6:.2f} Q{cx - 4.6 + tl * 0.5:.2f} {torso_top + 3.5:.2f} {cx - 3.6 + tl:.2f} {torso_top:.2f} Z")
    body.append(P(d, SHELL))
    body.append(f'<path d="M{cx - 2.2 + tl:.2f} {torso_top + 1.2:.2f} L{cx + 2.4 + tl:.2f} {torso_top + 1.2:.2f} L{cx + 2.0:.2f} {hip_y - 0.6:.2f} L{cx - 1.8:.2f} {hip_y - 0.6:.2f} Z" fill="{SHELL_LIGHT}"/>')
    # falda de tiras (pteruges)
    for k in range(-2, 3):
        x = cx + k * 1.3
        body.append(f'<path d="M{x:.2f} {hip_y + 0.4:.2f} L{x:.2f} {hip_y + 2.4:.2f}" stroke="{INK}" stroke-width="1.1" stroke-linecap="round"/>')
        body.append(f'<path d="M{x:.2f} {hip_y + 0.4:.2f} L{x:.2f} {hip_y + 2.2:.2f}" stroke="{BRONZE}" stroke-width="0.6" stroke-linecap="round"/>')
    # cinturón
    body.append(f'<rect x="{cx - 3.2:.2f}" y="{hip_y - 1.2:.2f}" width="6.4" height="1.1" fill="{BRONZE}" stroke="{INK}" stroke-width="0.3"/>')

    # --- pierna delantera ---
    body.append(leg(p.front, cx + 1.2))

    # --- brazo de la espada (lado lejano, tras el escudo) y espada ---
    sh_x, sh_y = cx + 2.2 + tl, torso_top + 2.0
    sword = []
    if p.sword == "thrust":
        hx, hy = cx + 7.5, sh_y + 2.2
        tip = CX + 18.0
        sword.append(L(sh_x, sh_y, hx, hy, INK, 2.2 + SW * 2))
        sword.append(L(sh_x, sh_y, hx, hy, SHELL, 2.2))
        sword.append(L(hx, hy + 0.1, tip, hy + 0.1, INK, 1.4 + SW * 2, cap="butt"))
        sword.append(L(hx, hy + 0.1, tip, hy + 0.1, STEEL, 1.4, cap="butt"))
        sword.append(L(hx + 0.5, hy - 0.3, tip - 0.6, hy - 0.3, "#ffffff", 0.35))
        sword.append(L(hx + 0.6, hy - 1.6, hx + 0.6, hy + 1.8, BRONZE, 1.0))
    elif p.sword == "half":
        hx, hy = cx + 5.4, sh_y + 2.6
        tip_x, tip_y = cx + 13.0, sh_y + 1.0
        sword.append(L(sh_x, sh_y, hx, hy, INK, 2.2 + SW * 2))
        sword.append(L(sh_x, sh_y, hx, hy, SHELL, 2.2))
        sword.append(L(hx, hy, tip_x, tip_y, INK, 1.4 + SW * 2, cap="butt"))
        sword.append(L(hx, hy, tip_x, tip_y, STEEL, 1.4, cap="butt"))
        sword.append(L(hx + 0.3, hy - 1.4, hx - 0.3, hy + 1.6, BRONZE, 1.0))
    elif p.sword == "guard":
        hx, hy = cx + 3.6, sh_y + 4.2
        sword.append(L(sh_x, sh_y, hx, hy, INK, 2.2 + SW * 2))
        sword.append(L(sh_x, sh_y, hx, hy, SHELL, 2.2))
        sword.append(L(hx, hy, hx + 7.0, hy - 6.5, INK, 1.4 + SW * 2, cap="butt"))
        sword.append(L(hx, hy, hx + 7.0, hy - 6.5, STEEL, 1.4, cap="butt"))
        sword.append(L(hx - 1.0, hy - 0.4, hx + 1.2, hy + 1.0, BRONZE, 1.0))
    elif p.sword == "up":
        hx, hy = cx + 3.0, sh_y - 2.0
        sword.append(L(sh_x, sh_y, hx, hy, INK, 2.2 + SW * 2))
        sword.append(L(sh_x, sh_y, hx, hy, SHELL, 2.2))
        sword.append(L(hx, hy, hx + 2.5, hy - 9.5, INK, 1.4 + SW * 2, cap="butt"))
        sword.append(L(hx, hy, hx + 2.5, hy - 9.5, STEEL, 1.4, cap="butt"))
    body.extend(sword)

    # --- brazo cercano: sostiene el escudo (o va desnudo) ---
    ax, ay = cx - 2.4 + tl, torso_top + 2.0
    if p.ladder:
        for k, dx in enumerate((-3.2, 3.2)):
            up = (p.ladder == 1) == (k == 0)
            hy = torso_top - (5.5 if up else 1.5)
            body.append(L(cx + dx * 0.6, torso_top + 1.5, cx + dx, hy, INK, 2.2 + SW * 2))
            body.append(L(cx + dx * 0.6, torso_top + 1.5, cx + dx, hy, SHELL, 2.2))
            body.append(f'<circle cx="{cx + dx:.2f}" cy="{hy:.2f}" r="1.3" fill="{BONE_SHADE}" stroke="{INK}" stroke-width="{SW}"/>')
    elif p.cheer:
        body.append(L(ax, ay, cx - 5.5, torso_top - 6, INK, 2.2 + SW * 2))
        body.append(L(ax, ay, cx - 5.5, torso_top - 6, SHELL, 2.2))
    else:
        hx, hy = cx - 2.0, torso_top + 6.0
        body.append(L(ax, ay, hx, hy, INK, 2.2 + SW * 2))
        body.append(L(ax, ay, hx, hy, SHELL, 2.2))
        body.append(f'<circle cx="{hx:.2f}" cy="{hy:.2f}" r="1.2" fill="{BONE_SHADE}" stroke="{INK}" stroke-width="{SW}"/>')
        # escudo redondo en capa aparte, centrado en el antebrazo
        scx, scy, r = hx - 0.6, hy - 1.0, 4.3
        for metal, (fill, dark, light) in METALS.items():
            s = []
            s.append(f'<circle cx="{scx:.2f}" cy="{scy:.2f}" r="{r}" fill="{fill}" stroke="{INK}" stroke-width="{SW + 0.2}" paint-order="stroke"/>')
            s.append(f'<circle cx="{scx:.2f}" cy="{scy:.2f}" r="{r - 1.0:.2f}" fill="none" stroke="{dark}" stroke-width="0.45"/>')
            if st["emblem"] == "sol":
                s.append(f'<circle cx="{scx:.2f}" cy="{scy:.2f}" r="1.2" fill="{light}" stroke="{dark}" stroke-width="0.3"/>')
                for k in range(8):
                    a = k * math.pi / 4
                    x0, y0 = scx + 1.8 * math.cos(a), scy + 1.8 * math.sin(a)
                    x1, y1 = scx + 3.0 * math.cos(a), scy + 3.0 * math.sin(a)
                    s.append(L(x0, y0, x1, y1, light, 0.4))
            else:
                # rosa: cinco pétalos y el botón
                for k in range(5):
                    a = k * 2 * math.pi / 5 - math.pi / 2
                    s.append(f'<ellipse cx="{scx + 1.6 * math.cos(a):.2f}" cy="{scy + 1.6 * math.sin(a):.2f}" rx="1.1" ry="0.75" fill="{light}" stroke="{dark}" stroke-width="0.25" transform="rotate({math.degrees(a):.1f} {scx + 1.6 * math.cos(a):.2f} {scy + 1.6 * math.sin(a):.2f})"/>')
                s.append(f'<circle cx="{scx:.2f}" cy="{scy:.2f}" r="0.9" fill="{CREST}" stroke="{dark}" stroke-width="0.25"/>')
            s.append(f'<path d="M{scx - r * 0.7:.2f} {scy - r * 0.55:.2f} A{r:.2f} {r:.2f} 0 0 1 {scx + r * 0.2:.2f} {scy - r * 0.95:.2f}" stroke="#ffffff" stroke-width="0.5" fill="none" opacity=".55"/>')
            shields[metal] = s

    # --- cabeza: yelmo corintio con cresta ---
    hy0 = torso_top - 9.5 - p.bob + (p.lean * 0.15)
    hx0 = cx + p.lean * 0.9
    if p.turn > 0:
        hx0 -= p.turn * 1.5
    d = (f"M{hx0 - 4.6:.2f} {hy0 + 10.2:.2f} L{hx0 - 4.8:.2f} {hy0 + 4.5:.2f} "
         f"C{hx0 - 4.8:.2f} {hy0 - 0.2:.2f} {hx0 + 4.8:.2f} {hy0 - 0.2:.2f} {hx0 + 4.8:.2f} {hy0 + 4.5:.2f} "
         f"L{hx0 + 4.6:.2f} {hy0 + 8.0:.2f} L{hx0 + 3.0:.2f} {hy0 + 10.6:.2f} L{hx0 + 1.0:.2f} {hy0 + 9.0:.2f} "
         f"L{hx0 - 1.0:.2f} {hy0 + 9.0:.2f} L{hx0 - 3.0:.2f} {hy0 + 10.6:.2f} Z")
    # cresta (detrás del yelmo)
    cr = p.crest
    crest = (f"M{hx0 - 5.2:.2f} {hy0 + 3.2:.2f} C{hx0 - 4.0 - cr:.2f} {hy0 - 4.5:.2f} {hx0 + 3.5 - cr:.2f} {hy0 - 4.8:.2f} {hx0 + 5.4:.2f} {hy0 + 2.4:.2f} "
             f"L{hx0 + 4.0:.2f} {hy0 + 2.0:.2f} C{hx0 + 2.6 - cr:.2f} {hy0 - 2.6:.2f} {hx0 - 2.8 - cr:.2f} {hy0 - 2.4:.2f} {hx0 - 4.0:.2f} {hy0 + 2.8:.2f} Z")
    head = []
    if st["hair"]:
        # melena dorada que cae por detrás del yelmo hasta los hombros, con el vaivén de la cresta
        hair = (f"M{hx0 - 3.8:.2f} {hy0 + 4.0:.2f} C{hx0 - 6.5 - cr:.2f} {hy0 + 7.0:.2f} {hx0 - 6.0 - cr * 1.5:.2f} {hy0 + 12.0:.2f} {hx0 - 4.5 - cr:.2f} {hy0 + 14.5:.2f} "
                f"L{hx0 - 2.0:.2f} {hy0 + 13.0:.2f} C{hx0 - 3.0:.2f} {hy0 + 10.0:.2f} {hx0 - 2.5:.2f} {hy0 + 7.0:.2f} {hx0 - 1.5:.2f} {hy0 + 5.0:.2f} Z")
        head.append(P(hair, HAIR))
        head.append(f'<path d="M{hx0 - 4.4:.2f} {hy0 + 6.0:.2f} C{hx0 - 5.4 - cr:.2f} {hy0 + 9.0:.2f} {hx0 - 5.0 - cr:.2f} {hy0 + 11.5:.2f} {hx0 - 4.2 - cr:.2f} {hy0 + 13.2:.2f}" stroke="{HAIR_LIGHT}" stroke-width="0.6" fill="none" opacity=".9"/>')
    if st["crest"]:
        head.append(P(crest, CREST))
        head.append(f'<path d="M{hx0 - 3.6:.2f} {hy0 + 1.8:.2f} C{hx0 - 2.2 - cr:.2f} {hy0 - 2.2:.2f} {hx0 + 2.2 - cr:.2f} {hy0 - 2.4:.2f} {hx0 + 3.8:.2f} {hy0 + 1.4:.2f}" stroke="{CREST_LIGHT}" stroke-width="0.6" fill="none" opacity=".9"/>')
    if st["wings"]:
        # alas a los lados del yelmo (la de atrás, un poco más alta); se abren con el vaivén
        for side, lift in ((-1, 0.8), (1, 0.0)):
            bx, by = hx0 + side * 3.6, hy0 + 3.6 - lift
            wing = (f"M{bx:.2f} {by:.2f} C{bx + side * 2.0:.2f} {by - 2.5 - cr:.2f} {bx + side * 4.5:.2f} {by - 5.0 - cr * 2:.2f} {bx + side * 6.5:.2f} {by - 4.8 - cr * 2:.2f} "
                    f"C{bx + side * 5.5:.2f} {by - 2.6:.2f} {bx + side * 4.5:.2f} {by - 0.6:.2f} {bx + side * 2.8:.2f} {by + 1.2:.2f} Z")
            head.append(P(wing, WING if side > 0 else WING_SHADE, 0.5))
            head.append(f'<path d="M{bx + side * 1.5:.2f} {by - 0.6:.2f} L{bx + side * 4.8:.2f} {by - 3.6 - cr:.2f} M{bx + side * 1.8:.2f} {by + 0.4:.2f} L{bx + side * 4.6:.2f} {by - 1.8:.2f}" stroke="{INK}" stroke-width="0.25" fill="none" opacity=".6"/>')
    head.append(P(d, BONE))
    # sombra de la mejilla y la abertura en T
    head.append(f'<path d="M{hx0 - 4.4:.2f} {hy0 + 6.0:.2f} L{hx0 - 4.6:.2f} {hy0 + 10.0:.2f} L{hx0 - 3.0:.2f} {hy0 + 10.4:.2f} L{hx0 - 1.2:.2f} {hy0 + 8.8:.2f} L{hx0 - 1.8:.2f} {hy0 + 6.2:.2f} Z" fill="{BONE_SHADE}"/>')
    slit_x = hx0 + 1.6 if p.turn == 0 else hx0 + 0.2
    if p.hurt:
        head.append(f'<path d="M{slit_x - 1.6:.2f} {hy0 + 5.4:.2f} L{slit_x + 1.6:.2f} {hy0 + 5.4:.2f}" stroke="{HOLE}" stroke-width="1.2" stroke-linecap="round"/>')
        head.append(f'<path d="M{slit_x - 0.4:.2f} {hy0 + 4.9:.2f} L{slit_x + 0.8:.2f} {hy0 + 6.1:.2f} M{slit_x + 0.8:.2f} {hy0 + 4.9:.2f} L{slit_x - 0.4:.2f} {hy0 + 6.1:.2f}" stroke="{CREST}" stroke-width="0.4"/>')
    else:
        head.append(f'<path d="M{slit_x - 1.7:.2f} {hy0 + 5.4:.2f} L{slit_x + 1.9:.2f} {hy0 + 5.4:.2f}" stroke="{HOLE}" stroke-width="1.3" stroke-linecap="round"/>')
        head.append(f'<path d="M{slit_x + 0.5:.2f} {hy0 + 5.4:.2f} L{slit_x + 0.5:.2f} {hy0 + 8.6:.2f}" stroke="{HOLE}" stroke-width="1.0" stroke-linecap="round"/>')
        head.append(f'<circle cx="{slit_x + 0.9:.2f}" cy="{hy0 + 5.3:.2f}" r="0.35" fill="#ffffff" opacity=".9"/>')
    # brillo del yelmo
    head.append(f'<path d="M{hx0 - 2.6:.2f} {hy0 + 1.8:.2f} C{hx0 - 1.0:.2f} {hy0 + 0.6:.2f} {hx0 + 1.4:.2f} {hy0 + 0.6:.2f} {hx0 + 2.6:.2f} {hy0 + 1.6:.2f}" stroke="#ffffff" stroke-width="0.5" fill="none" opacity=".6"/>')
    body.extend(head)
    return "".join(cape_svg), "".join(body), {m: "".join(s) for m, s in shields.items()}


def frame_svg(p, with_shield_metal=None, shield_only=False, st=None):
    cape, body, shields = body_parts(p, st)
    inner = ""
    if shield_only:
        inner = shields.get(with_shield_metal, "")
    else:
        inner = cape + body
    if p.dead > 0:
        # caído: gira todo alrededor de los pies
        # cae hacia delante (sentido horario alrededor de los pies) y queda tendido sobre el suelo
        inner = f'<g transform="translate({-8 * p.dead:.1f} 0) rotate({80 * p.dead:.1f} {CX} {FLOOR - 0.5})">{inner}</g>'
    return svg(48, 32, inner)


def run_cycle(i, n=8):
    ph = 2 * math.pi * i / n
    stride = 3.2
    fdx, bdx = stride * math.sin(ph), -stride * math.sin(ph)
    flift, blift = max(0.0, math.cos(ph)) * 2.2, max(0.0, -math.cos(ph)) * 2.2
    return Pose(lean=2.2, front=(fdx + 0.6, flift), back=(bdx - 0.6, blift), bob=-0.5 * abs(math.cos(ph)) + 0.3,
                cape=1.0 + 0.35 * math.sin(ph * 2), cape_lift=0.4 + 0.2 * math.sin(ph * 2 + 1), crest=0.5 * math.sin(ph * 2), sword="guard")


CLIPS = {
    "quieto": [Pose(bob=0.35 * math.sin(2 * math.pi * i / 6), cape=0.35 + 0.08 * math.sin(2 * math.pi * i / 6 + 1), crest=0.2 * math.sin(2 * math.pi * i / 6)) for i in range(6)],
    "correr": [run_cycle(i) for i in range(8)],
    "girar": [Pose(turn=0.5, lean=-0.5, front=(1.2, 0), back=(-1.2, 0), cape=1.4, cape_lift=0.3), Pose(turn=1.0, lean=-1.0, front=(0.8, 0), back=(-0.8, 0), cape=0.6, cape_lift=0.8)],
    "salto_preparar": [Pose(crouch=2.6, lean=1.0, front=(2.2, 0), back=(-2.4, 0), cape=0.2, cape_lift=-0.3)],
    "salto_subir": [Pose(crouch=-0.5, lean=1.2, front=(1.6, 3.0), back=(-1.8, 1.6), cape=-0.4, cape_lift=-1.2, bob=0.6)],
    "salto_caer": [Pose(lean=0.4, front=(2.4, 1.0), back=(-2.6, 0.4), cape=1.6, cape_lift=1.6, crest=0.6)],
    "escalera": [Pose(ladder=1, sword="none", cape=0.1, front=(1.2, 1.0), back=(-1.2, 0)), Pose(ladder=2, sword="none", cape=0.1, front=(1.2, 0), back=(-1.2, 1.0))],
    "estocada": [Pose(lean=4.0, crouch=1.4, front=(5.0, 0), back=(-4.5, 0), cape=2.0, cape_lift=0.6, sword="thrust", crest=0.8),
                 Pose(lean=2.4, crouch=0.8, front=(3.5, 0), back=(-3.2, 0), cape=1.4, cape_lift=0.3, sword="half", crest=0.4),
                 Pose(lean=1.0, front=(2.0, 0), back=(-1.8, 0), cape=0.8, sword="guard")],
    "golpe": [Pose(lean=-3.0, crouch=1.2, front=(1.0, 0), back=(-2.6, 0), cape=1.8, cape_lift=1.6, hurt=True, sword="half", crest=-0.8)],
    "muerte": [Pose(lean=-2.0, crouch=1.5, hurt=True, sword="none", cape=1.2, dead=0.0), Pose(lean=-2.0, crouch=2.5, hurt=True, sword="none", cape=1.0, dead=0.5),
               Pose(lean=-2.0, crouch=3.0, hurt=True, sword="none", cape=0.8, dead=1.0)],
    "celebrar": [Pose(sword="up", cheer=True, bob=0.4, cape=0.6, cape_lift=0.4), Pose(sword="up", cheer=True, bob=1.2, cape=0.9, cape_lift=1.0, front=(1.0, 1.0), back=(-1.0, 1.0))],
}
# La estocada marca el fotograma del golpe (el primero): el juego lo alinea con el tick del golpe.
TAGS = {("estocada", 0): "golpe"}


def build():
    n = 0
    for character, st in STYLES.items():
        for action, poses in CLIPS.items():
            for i, p in enumerate(poses):
                tag = TAGS.get((action, i))
                name = f"{character}_{action}_{i:02d}" + (f"_{tag}" if tag else "")
                emit(name, frame_svg(p, st=st), "personajes")
                n += 1
                if p.shield and not p.ladder and not p.cheer:
                    for metal in METALS:
                        emit(f"{character}_escudo_{metal}_{action}_{i:02d}" + (f"_{tag}" if tag else ""), frame_svg(p, metal, shield_only=True, st=st), "personajes")
    return n


if __name__ == "__main__":
    print(build(), "fotogramas")
