"""Enemigos del castillo dibujados, uno por tipo y por estado con caja distinta (docs/art/encargo/cajas_colision.md).

Cada fotograma se dibuja en un lienzo con la proporción de su caja; el juego lo escala a la caja y elige, entre los
clips del tipo (enemigoXX_<estado>), el de proporción más parecida a la caja del momento (EnemyArt.DrawnFor). Lenguaje
común: caparazón oscuro, máscara o hueso claro, ojos huecos, contorno de tinta y un acento por familia. Mirando a la
derecha; el juego voltea.
"""
import math

from svgkit import (BONE, BONE_SHADE, BRONZE, BRONZE_DARK, GOLD, HOLE, INK, SHELL, SHELL_LIGHT, emit, noise_filter, svg)

SW = 0.5
STONE, STONE_LIGHT, STONE_DARK = "#5b5d68", "#7c7f8c", "#3a3c46"
WISP, WISP_DEEP = "#9fd6ff", "#3b6fa8"
FLAME, FLAME_DEEP = "#ffb060", "#ff6a2a"
MOSS = "#3f5a52"
BAT_WING = "#1d2030"


def P(d, fill, sw=SW, extra=""):
    return f'<path d="{d}" fill="{fill}" stroke="{INK}" stroke-width="{sw}" stroke-linejoin="round" stroke-linecap="round" paint-order="stroke" {extra}/>'


def E(cx, cy, rx, ry, fill, sw=SW, extra=""):
    return f'<ellipse cx="{cx:.2f}" cy="{cy:.2f}" rx="{rx:.2f}" ry="{ry:.2f}" fill="{fill}" stroke="{INK}" stroke-width="{sw}" paint-order="stroke" {extra}/>'


def Cc(cx, cy, r, fill, sw=SW, extra=""):
    return f'<circle cx="{cx:.2f}" cy="{cy:.2f}" r="{r:.2f}" fill="{fill}" stroke="{INK}" stroke-width="{sw}" paint-order="stroke" {extra}/>'


def L(x0, y0, x1, y1, color, w):
    return f'<path d="M{x0:.2f} {y0:.2f} L{x1:.2f} {y1:.2f}" stroke="{color}" stroke-width="{w}" stroke-linecap="round" fill="none"/>'


def eyes(cx, cy, dx=1.4, r=0.55, glow=None):
    out = f'<ellipse cx="{cx - dx:.2f}" cy="{cy:.2f}" rx="{r * 0.8:.2f}" ry="{r:.2f}" fill="{HOLE}"/><ellipse cx="{cx + dx:.2f}" cy="{cy:.2f}" rx="{r * 0.8:.2f}" ry="{r:.2f}" fill="{HOLE}"/>'
    if glow:
        out += f'<circle cx="{cx - dx + 0.15:.2f}" cy="{cy - 0.1:.2f}" r="{r * 0.3:.2f}" fill="{glow}"/><circle cx="{cx + dx + 0.15:.2f}" cy="{cy - 0.1:.2f}" r="{r * 0.3:.2f}" fill="{glow}"/>'
    return out


def inked_leg(x0, y0, x1, y1, w=1.0, color=SHELL):
    return L(x0, y0, x1, y1, INK, w + SW * 2) + L(x0, y0, x1, y1, color, w)


# ---------- 0x0E murciélago: colgado (17 × 10) y volando (11 × 12) ----------

def bat_hanging():
    w, h = 17, 10
    out = [P(f"M2 1 Q5 4 8.5 9.2 Q12 4 15 1 Q12.5 3 8.5 2.4 Q4.5 3 2 1 Z", BAT_WING)]        # alas plegadas hacia abajo
    out.append(E(8.5, 3.6, 3.2, 2.6, SHELL))                                                  # cuerpo
    out.append(P("M6 1.2 Q8.5 -0.2 11 1.2 L8.5 0.4 Z", SHELL))                                 # patas agarradas al techo
    out.append(E(8.5, 6.6, 2.4, 2.0, BONE))                                                   # máscara boca abajo
    out.append(eyes(8.5, 6.4, 1.0, 0.5, "#ffd59a"))
    out.append(P("M7.2 8.4 L8.5 9.4 L9.8 8.4", HOLE, 0.3))
    return svg(w, h, "".join(out))


def bat_flying(ph):
    w, h = 11, 12
    up = math.sin(ph * math.pi * 2)
    wy = 2.5 - 2.2 * up
    out = [P(f"M5.5 6 Q2 {wy:.1f} 0.4 {wy + 1.5:.1f} Q2.5 {wy + 4:.1f} 5.5 8 Q8.5 {wy + 4:.1f} 10.6 {wy + 1.5:.1f} Q9 {wy:.1f} 5.5 6 Z", BAT_WING)]
    out.append(E(5.5, 7.0, 2.4, 2.8, SHELL))
    out.append(E(5.5, 5.2, 2.6, 2.2, BONE))
    out.append(P("M3.6 3.6 L4.4 5.4 L3.0 5.2 Z M7.4 3.6 L6.6 5.4 L8.0 5.2 Z", BONE, 0.35))   # orejas
    out.append(eyes(5.5, 5.4, 1.0, 0.55, "#ffd59a"))
    out.append(P("M4.6 9.4 L5.5 11.2 L6.4 9.4", SHELL, 0.4))                                   # patas
    return svg(w, h, "".join(out))


# ---------- 0x25 ya está en esqueleto.py ----------

# ---------- 0x0A caballero (25 × 16 andando; 15 × 17 en la escalera) ----------

def knight(ph):
    w, h = 25, 16
    crawl = math.sin(ph * 2 * math.pi)
    out = []
    # patas (tres pares, escarabajo acorazado agachado)
    for k, x in enumerate((7, 11, 15)):
        s = crawl if k % 2 == 0 else -crawl
        out.append(inked_leg(x, 11, x - 1.5 + s * 1.2, 15.6, 1.1))
    out.append(P("M4 12 Q4 4 12 3.2 L18 3.6 Q21 5 20.5 12 Z", SHELL))                           # caparazón
    for x in (8, 11.5, 15):
        out.append(f'<path d="M{x} 3.6 Q{x + 1} 8 {x} 12" stroke="{SHELL_LIGHT}" stroke-width="0.5" fill="none"/>')
    out.append(P("M10 3.2 Q12.5 -0.5 15.5 3.4 Z", "#c8342a"))                                     # cresta roja
    out.append(E(18.5, 7.4, 3.0, 3.4, BONE))                                                      # yelmo-máscara
    out.append(P("M18.2 5.4 L19.6 5.4 L19.6 9.2 L18.9 9.2 L18.9 6.4 L18.2 6.4 Z", HOLE, 0.2))       # rendija en T
    out.append(inked_leg(16.5, 9.5, 21.5, 11.5, 1.2))                                             # brazo de la lanza
    out.append(L(19, 12.6, 24.6, 9.6, INK, 1.4 + SW * 2))
    out.append(L(19, 12.6, 24.6, 9.6, "#9aa1ad", 1.4))                                            # lanza corta
    out.append(Cc(21.2, 9.8, 4.0, BRONZE, SW + 0.2))                                               # escudo redondo delante
    out.append(f'<circle cx="21.2" cy="9.8" r="2.8" fill="none" stroke="{BRONZE_DARK}" stroke-width="0.45"/>')
    out.append(Cc(21.2, 9.8, 1.0, GOLD, 0.25))
    return svg(w, h, "".join(out))


def knight_ladder():
    w, h = 15, 17
    out = [P("M3 16 Q2 6 7.5 2.5 Q13 6 12 16 Z", SHELL)]
    out.append(E(7.5, 6.5, 3.0, 3.2, BONE))
    out.append(P("M7.2 4.6 L8.4 4.6 L8.4 8 L7.8 8 L7.8 5.6 L7.2 5.6 Z", HOLE, 0.2))
    for k, (x, y) in enumerate(((3.2, 4), (11.8, 7), (3.2, 10), (11.8, 13))):
        out.append(inked_leg(7.5 if x < 7 else 7.5, y + 2, x, y, 1.1))
    out.append(Cc(7.5, 12, 3.4, BRONZE, SW + 0.2))
    out.append(Cc(7.5, 12, 0.9, GOLD, 0.25))
    return svg(w, h, "".join(out))


# ---------- 0x2E brote de llama: cuatro alturas (12 × 7, 9, 13, 16) ----------

def sprout(stage):
    hs = {1: 7, 2: 9, 3: 13, 4: 16}
    w, h = 12, hs[stage]
    out = [P(f"M1.5 {h} Q1 {h - 3} 3.5 {h - 3.5} L8.5 {h - 3.5} Q11 {h - 3} 10.5 {h} Z", "#2a2f3c")]   # bulbo oscuro
    out.append(f'<path d="M3 {h - 1.2} Q6 {h - 2.4} 9 {h - 1.2}" stroke="{SHELL_LIGHT}" stroke-width="0.4" fill="none"/>')
    top = h - 3.5
    flame_h = top - 0.6
    if flame_h > 1.5:
        out.append(P(f"M6 {top + 0.5} C2.8 {top - flame_h * 0.4} 4.2 {top - flame_h * 0.8} 6 {0.8} C7.8 {top - flame_h * 0.8} 9.2 {top - flame_h * 0.4} 6 {top + 0.5} Z", FLAME_DEEP, 0.45))
        out.append(f'<path d="M6 {top} C4.6 {top - flame_h * 0.4} 5.2 {top - flame_h * 0.6} 6 {top - flame_h * 0.75} C6.8 {top - flame_h * 0.6} 7.4 {top - flame_h * 0.4} 6 {top} Z" fill="{FLAME}"/>')
        out.append(f'<path d="M6 {top - 0.5} C5.4 {top - flame_h * 0.3} 5.6 {top - flame_h * 0.4} 6 {top - flame_h * 0.5} C6.4 {top - flame_h * 0.4} 6.6 {top - flame_h * 0.3} 6 {top - 0.5} Z" fill="#fff0c8"/>')
    else:
        out.append(E(6, top + 0.2, 1.6, 1.0, FLAME, 0.4))
    out.append(eyes(6, h - 1.8, 1.6, 0.5, "#ffd59a"))
    return svg(w, h, "".join(out))


# ---------- 0x31 pinchos (9 × 8 pequeño; 13 × 13 grande) ----------

def spikes(w, h, n, seed):
    out = []
    for k in range(n):
        x = (k + 0.5) * w / n
        hh = h * (0.55 + 0.45 * (0.5 + 0.5 * math.sin(k * 2.1 + seed)))
        out.append(P(f"M{x - w / n * 0.5:.2f} {h} L{x:.2f} {h - hh:.2f} L{x + w / n * 0.5:.2f} {h} Z", BONE, 0.45))
        out.append(f'<path d="M{x - w / n * 0.2:.2f} {h} L{x - 0.2:.2f} {h - hh + 1:.2f}" stroke="{BONE_SHADE}" stroke-width="0.4" fill="none"/>')
    out.append(f'<rect x="0" y="{h - 1.2}" width="{w}" height="1.2" fill="{SHELL}" stroke="{INK}" stroke-width="0.35"/>')
    return svg(w, h, "".join(out))


# ---------- 0x18 rodador (17 × 12) ----------

def roller(ph):
    w, h = 17, 12
    out = []
    for k, x in enumerate((4, 8, 12)):
        s = math.sin(ph * 2 * math.pi + k)
        out.append(inked_leg(x + 1, 9, x + s, 11.6, 1.0))
    out.append(P("M1.5 9 Q1.5 2 8.5 1.6 Q15.5 2 15.5 9 Z", STONE))
    for k in range(4):
        x = 3 + k * 3.2
        out.append(f'<path d="M{x} 2.4 Q{x + 0.8} 5.5 {x} 9" stroke="{STONE_DARK}" stroke-width="0.6" fill="none"/>')
        out.append(P(f"M{x + 1.2} 1.8 L{x + 1.9} 0.3 L{x + 2.6} 1.9 Z", BONE, 0.35))
    out.append(E(15.2, 7.2, 1.8, 2.0, BONE))
    out.append(eyes(15.4, 7.0, 0.7, 0.4))
    return svg(w, h, "".join(out))


# ---------- 0x19 murciélagos que se fusionan: 6 × 6, 11 × 14, 17 × 13 ----------

def bat_small(ph):
    w, h = 6, 6
    up = math.sin(ph * 2 * math.pi)
    out = [P(f"M3 3.2 Q1 {1.6 - up:.1f} 0.3 {2.6 - up:.1f} Q1.5 4 3 4.4 Q4.5 4 5.7 {2.6 - up:.1f} Q5 {1.6 - up:.1f} 3 3.2 Z", BAT_WING, 0.35)]
    out.append(Cc(3, 3.4, 1.5, SHELL, 0.35))
    out.append(Cc(3, 3.0, 1.1, BONE, 0.3))
    out.append(eyes(3, 3.0, 0.5, 0.3))
    return svg(w, h, "".join(out))


def bat_medium(ph):
    w, h = 11, 14
    up = math.sin(ph * 2 * math.pi)
    out = [P(f"M5.5 7 Q2 {4 - 2.5 * up:.1f} 0.4 {5.5 - 2.5 * up:.1f} Q2.5 9 5.5 10 Q8.5 9 10.6 {5.5 - 2.5 * up:.1f} Q9 {4 - 2.5 * up:.1f} 5.5 7 Z", BAT_WING)]
    out.append(E(5.5, 8.5, 2.6, 3.2, SHELL))
    out.append(E(5.5, 5.8, 2.8, 2.5, BONE))
    out.append(P("M3.4 3.8 L4.4 6 L2.8 5.8 Z M7.6 3.8 L6.6 6 L8.2 5.8 Z", BONE, 0.35))
    out.append(eyes(5.5, 6.0, 1.1, 0.6, "#ffd59a"))
    out.append(P("M4.4 11.4 L5.5 13.4 L6.6 11.4", SHELL, 0.4))
    return svg(w, h, "".join(out))


def bat_big(ph):
    w, h = 17, 13
    up = math.sin(ph * 2 * math.pi)
    out = [P(f"M8.5 6 Q4 {2.5 - 2 * up:.1f} 0.4 {4.5 - 2 * up:.1f} Q3.5 9 8.5 10 Q13.5 9 16.6 {4.5 - 2 * up:.1f} Q13 {2.5 - 2 * up:.1f} 8.5 6 Z", BAT_WING)]
    out.append(E(8.5, 8.0, 3.4, 3.6, SHELL))
    out.append(E(8.5, 5.2, 3.4, 3.0, BONE))
    out.append(P("M5.6 2.6 L6.8 5.2 L4.8 5 Z M11.4 2.6 L10.2 5.2 L12.2 5 Z", BONE, 0.4))
    out.append(eyes(8.5, 5.4, 1.4, 0.7, "#ff8a60"))
    out.append(P("M7 5.8 L8.5 8.2 L10 5.8", HOLE, 0.3))
    return svg(w, h, "".join(out))


# ---------- 0x34 cabeza de piedra (9 × 8); 0x04 pistón (9 × 8) ----------

def stone_head(angry=False):
    w, h = 9, 8
    out = [P("M0.8 7.6 L0.8 2.4 Q1 0.6 3 0.6 L6 0.6 Q8.2 0.6 8.2 2.4 L8.2 7.6 Z", STONE)]
    out.append(f'<path d="M1.6 1.6 L7.4 1.6" stroke="{STONE_LIGHT}" stroke-width="0.5"/>')
    out.append(E(4.5, 4.0, 2.6, 2.2, BONE_SHADE, 0.4))
    out.append(eyes(4.5, 3.8, 1.1, 0.55, "#ff8a60" if angry else None))
    out.append(P("M3.2 5.6 L5.8 5.6 L5.4 6.6 L3.6 6.6 Z", HOLE, 0.25))
    return svg(w, h, "".join(out))


# ---------- 0x4E almas (17 × 17) ----------

def soul(ph):
    w, h = 17, 17
    sway = math.sin(ph * 2 * math.pi)
    defs = '<radialGradient id="g" cx="50%" cy="45%" r="55%"><stop offset="0" stop-color="#cfefff" stop-opacity=".95"/><stop offset=".55" stop-color="#9fd6ff" stop-opacity=".55"/><stop offset="1" stop-color="#3b6fa8" stop-opacity="0"/></radialGradient>'
    out = [f'<circle cx="8.5" cy="8" r="8" fill="url(#g)"/>']
    out.append(P(f"M8.5 2.5 C12 2.5 13.5 6 12.5 9 C12 11 {10 + sway:.1f} 13 {9 + sway * 1.5:.1f} 16 C8 13.5 {7 - sway:.1f} 12 5 10 C3.5 8 4 2.5 8.5 2.5 Z", "#dff3ff", 0.4, 'opacity=".9"'))
    out.append(E(8.5, 6.4, 2.6, 2.4, BONE, 0.35))
    out.append(eyes(8.5, 6.4, 1.1, 0.6))
    out.append(P("M7.6 8.2 L8.5 9.2 L9.4 8.2", HOLE, 0.25))
    return svg(w, h, "".join(out), defs)


# ---------- 0x11 erizo (17 × 17 andando; 17 × 11 descansando) ----------

def hedgehog(ph, resting=False):
    w, h = (17, 11) if resting else (17, 17)
    out = []
    body_y = h - 5
    if not resting:
        for k, x in enumerate((5, 9, 13)):
            s = math.sin(ph * 2 * math.pi + k * 2)
            out.append(inked_leg(x, body_y + 2, x - 1 + s * 1.3, h - 0.4, 1.0))
    out.append(P(f"M1.5 {body_y + 3} Q1 {body_y - 5} 8.5 {body_y - 6} Q16 {body_y - 5} 15.5 {body_y + 3} Z", SHELL))
    for k in range(7):
        x = 2.5 + k * 2
        top = body_y - 6 + abs(k - 3) * 0.9
        out.append(P(f"M{x} {body_y - 1} L{x + 0.6} {top - 3.2} L{x + 1.6} {body_y - 1} Z", BONE, 0.35))
    out.append(E(14.5, body_y + 1.4, 2.2, 1.8, BONE))
    out.append(eyes(14.8, body_y + 1.2, 0.8, 0.45))
    out.append(Cc(16.4, body_y + 1.6, 0.5, HOLE, 0.2))
    return svg(w, h, "".join(out))


# ---------- 0x32 / 0x51 torreta de flechas (17 × 25); 0x23 estatua dormida (15 × 25) ----------

def arrow_turret():
    w, h = 17, 25
    defs = noise_filter("grain", freq=0.5, octaves=3, seed=41, alpha=0.35, color=(0.6, 0.62, 0.72))
    out = [P("M2 24.5 L2 10 Q2 6.5 5 6 L12 6 Q15 6.5 15 10 L15 24.5 Z", STONE)]
    out.append(P("M4 6 Q4 1 8.5 0.8 Q13 1 13 6 Z", STONE_LIGHT))                                  # cabeza
    out.append(E(8.5, 4.0, 2.6, 2.2, BONE_SHADE, 0.4))
    out.append(eyes(8.5, 3.8, 1.1, 0.5))
    out.append(P("M5.5 13 L12 13 L12 15.4 L5.5 15.4 Z", HOLE, 0.3))                                 # ranura de flechas
    out.append(f'<path d="M6 14.2 L11.4 14.2" stroke="#9aa1ad" stroke-width="0.6"/>')
    out.append(f'<path d="M11.4 14.2 L10.4 13.4 M11.4 14.2 L10.4 15" stroke="#9aa1ad" stroke-width="0.4"/>')
    out.append(f'<rect x="2" y="6" width="13" height="18.5" filter="url(#grain)" opacity=".35"/>')
    out.append(f'<path d="M3 19 L14 19 M3 22 L14 22" stroke="{STONE_DARK}" stroke-width="0.4"/>')
    return svg(w, h, "".join(out), defs)


def sleeping_statue(awake=False):
    w, h = 15, 25
    defs = noise_filter("grain", freq=0.5, octaves=3, seed=43, alpha=0.35, color=(0.6, 0.62, 0.72))
    out = [P("M2 24.5 L2 20 L13 20 L13 24.5 Z", STONE_DARK)]
    out.append(P("M3 20 Q2.5 8 7.5 6.5 Q12.5 8 12 20 Z", STONE))
    out.append(E(7.5, 5.2, 3.2, 3.6, BONE_SHADE))
    out.append(P("M5.2 2.2 Q7.5 0.4 9.8 2.2 L9.4 3.4 Q7.5 2.4 5.6 3.4 Z", STONE_LIGHT, 0.4))        # yelmo
    if awake:
        out.append(eyes(7.5, 5.4, 1.2, 0.6, "#ff8a60"))
    else:
        out.append(f'<path d="M5.8 5.6 L7 5.6 M8 5.6 L9.2 5.6" stroke="{HOLE}" stroke-width="0.5"/>')
    out.append(L(4.2, 11, 4.2, 19, INK, 1.4 + SW * 2) + L(4.2, 11, 4.2, 19, STONE_LIGHT, 1.4))        # brazos cruzados
    out.append(L(10.8, 11, 10.8, 19, INK, 1.4 + SW * 2) + L(10.8, 11, 10.8, 19, STONE_LIGHT, 1.4))
    out.append(f'<rect x="2" y="6" width="11" height="18.5" filter="url(#grain)" opacity=".35"/>')
    return svg(w, h, "".join(out), defs)


# ---------- 0x02 chispa (13 × 13); 0x50 / 0x3A boca de pared (17 × 17) ----------

def spark(ph):
    w, h = 13, 13
    defs = '<radialGradient id="g" cx="50%" cy="50%" r="50%"><stop offset="0" stop-color="#fff3d6" stop-opacity=".9"/><stop offset="1" stop-color="#ffd59a" stop-opacity="0"/></radialGradient>'
    out = [f'<circle cx="6.5" cy="6.5" r="6.4" fill="url(#g)"/>']
    for k in range(6):
        a = ph * 2 * math.pi + k * math.pi / 3
        out.append(L(6.5, 6.5, 6.5 + 5.2 * math.cos(a), 6.5 + 5.2 * math.sin(a), "#ffe9b8", 0.6))
    out.append(Cc(6.5, 6.5, 2.4, "#fff3d6", 0.45))
    out.append(eyes(6.5, 6.4, 0.9, 0.45))
    return svg(w, h, "".join(out), defs)


def wall_mouth():
    w, h = 17, 17
    defs = noise_filter("grain", freq=0.5, octaves=3, seed=47, alpha=0.35, color=(0.6, 0.62, 0.72))
    out = [P("M0.6 16.4 L0.6 0.6 L16.4 0.6 L16.4 16.4 Z", STONE)]
    out.append(E(8.5, 8.5, 6.2, 6.0, STONE_LIGHT, 0.45))
    out.append(P("M3.6 9.6 Q8.5 14.6 13.4 9.6 Q8.5 11.6 3.6 9.6 Z", HOLE, 0.35))                   # boca
    for x in (5.4, 7.4, 9.6, 11.6):
        out.append(P(f"M{x} 9.9 L{x + 0.6} 11.6 L{x + 1.2} 10.2 Z", BONE, 0.25))
    out.append(eyes(8.5, 6.0, 2.2, 0.7, "#ff8a60"))
    out.append(f'<rect x="0.6" y="0.6" width="15.8" height="15.8" filter="url(#grain)" opacity=".35"/>')
    return svg(w, h, "".join(out), defs)


# ---------- 0x26 fantasma (14 × 17); 0x3C hechicero (17 × 16); 0x42 monstruo del agua (23 × 16) ----------

def ghost(ph):
    w, h = 14, 17
    sway = math.sin(ph * 2 * math.pi)
    out = [P(f"M7 1.5 C11 1.5 12.5 5 12 9 L{12 + sway:.1f} 16 L{9.5 + sway * .5:.1f} 13.5 L7 16.2 L{4.5 - sway * .5:.1f} 13.5 L{2 - sway:.1f} 16 C1.5 5 3 1.5 7 1.5 Z", "#2b3550", 0.5, 'opacity=".92"')]
    out.append(E(7, 5.8, 3.4, 3.0, BONE))
    out.append(eyes(7, 5.8, 1.4, 0.7, "#9fd6ff"))
    out.append(P("M5.6 8.2 Q7 9.6 8.4 8.2", HOLE, 0.3))
    return svg(w, h, "".join(out))


def sorcerer(ph):
    w, h = 17, 16
    lift = math.sin(ph * 2 * math.pi)
    out = [P(f"M8.5 2 C13 2 13.5 8 12.5 15.5 L4.5 15.5 C3.5 8 4 2 8.5 2 Z", "#3a2a6e")]
    out.append(P("M8.5 0.4 L12.2 6 L4.8 6 Z", "#2a1c52"))                                          # capucha en pico
    out.append(E(8.5, 6.2, 2.4, 2.2, BONE))
    out.append(eyes(8.5, 6.2, 1.0, 0.55, "#c9a6ff"))
    out.append(inked_leg(11.5, 9, 15.5, 8 - lift, 1.1, "#3a2a6e"))
    out.append(Cc(16.0, 7.6 - lift, 1.6, "#c9a6ff", 0.35))                                          # orbe en la mano
    out.append(f'<path d="M6 9 L6 14 M8.5 9 L8.5 14.5 M11 9 L11 14" stroke="{INK}" stroke-width="0.3" opacity=".6"/>')
    return svg(w, h, "".join(out))


def water_monster(ph):
    w, h = 23, 16
    out = [P("M1 15.5 Q1 6 8 4.5 L16 4.5 Q22 6 22 15.5 Z", "#1f4a63")]
    for k in range(3):
        x = 5 + k * 5
        out.append(P(f"M{x} 4.6 L{x + 1.6} 1.2 L{x + 3.2} 4.6 Z", "#2f7a9a", 0.4))                     # aletas dorsales
    out.append(E(17.5, 9.5, 3.4, 3.0, BONE))
    out.append(eyes(18, 9.2, 1.2, 0.65, "#9fd6ff"))
    out.append(P("M15 11.6 Q18.5 14 21.5 11.6", HOLE, 0.35))
    for x in (16.2, 18.2, 20.2):
        out.append(P(f"M{x} 11.8 L{x + 0.5} 13.2 L{x + 1} 11.9 Z", BONE, 0.2))
    out.append(f'<path d="M3 13 Q6 11 9 13 T15 13" stroke="#6fc3e8" stroke-width="0.6" fill="none" opacity=".7"/>')
    return svg(w, h, "".join(out))


# ---------- generados periódicamente ----------

def falling_walker(ph):
    w, h = 17, 11
    out = []
    for k, x in enumerate((5, 9, 13)):
        s = math.sin(ph * 2 * math.pi + k)
        out.append(inked_leg(x, 7, x - 1 + s * 1.2, 10.6, 1.0))
    out.append(P("M1.5 8 Q2 1.5 8.5 1.2 Q15 1.5 15.5 8 Z", SHELL))
    out.append(f'<path d="M5 2.2 Q6 5 5 8 M9 1.6 Q10 5 9 8 M12.5 2.4 Q13.2 5 12.5 8" stroke="{SHELL_LIGHT}" stroke-width="0.45" fill="none"/>')
    out.append(E(14, 5.6, 2.2, 1.9, BONE))
    out.append(eyes(14.2, 5.4, 0.8, 0.45))
    return svg(w, h, "".join(out))


def bouncing_ball(ph):
    w, h = 17, 16
    out = [Cc(8.5, 8.5, 7.3, STONE)]
    out.append(f'<circle cx="8.5" cy="8.5" r="5.2" fill="none" stroke="{STONE_DARK}" stroke-width="0.6"/>')
    for k in range(8):
        a = k * math.pi / 4 + ph
        out.append(P(f"M{8.5 + 6.6 * math.cos(a - 0.18):.2f} {8.5 + 6.6 * math.sin(a - 0.18):.2f} L{8.5 + 8.4 * math.cos(a):.2f} {8.5 + 8.4 * math.sin(a):.2f} L{8.5 + 6.6 * math.cos(a + 0.18):.2f} {8.5 + 6.6 * math.sin(a + 0.18):.2f} Z", BONE, 0.3))
    out.append(E(8.5, 8.2, 2.6, 2.2, BONE_SHADE, 0.4))
    out.append(eyes(8.5, 8.0, 1.0, 0.5))
    return svg(w, h, "".join(out))


def leap_fish(ph):
    w, h = 17, 17
    out = [P("M2 9 Q5 3 11 4 Q15.5 5 16 9 Q15.5 13 11 14 Q5 15 2 9 Z", "#1f4a63")]
    out.append(P("M2.4 9 L0.4 4.5 L4.5 7.2 Z M2.4 9 L0.4 13.5 L4.5 10.8 Z", "#2f7a9a", 0.4))         # cola
    out.append(P("M8 4.4 L10 0.8 L12 4.6 Z", "#2f7a9a", 0.4))                                       # aleta
    out.append(E(13, 8.4, 2.4, 2.2, BONE))
    out.append(eyes(13.4, 8.2, 0.9, 0.5, "#9fd6ff"))
    out.append(P("M11.6 10.4 Q13.4 12 15.2 10.4", HOLE, 0.3))
    return svg(w, h, "".join(out))


def fireball(ph):
    w, h = 15, 15
    defs = '<radialGradient id="g" cx="50%" cy="55%" r="50%"><stop offset="0" stop-color="#fff0c8"/><stop offset=".45" stop-color="#ffb060"/><stop offset="1" stop-color="#ff6a2a" stop-opacity="0"/></radialGradient>'
    out = [f'<circle cx="7.5" cy="7.5" r="7.4" fill="url(#g)"/>']
    out.append(P(f"M7.5 13.5 C3.5 13.5 2.5 9 4.5 6 C5.5 7.5 6 7 6 5 C6.5 7 7.5 6 7.5 1.5 C8.5 5 9.5 6.5 9 7.5 C10 6.5 10.5 7 11 6 C12.5 9 11.5 13.5 7.5 13.5 Z", "#ff8a3a", 0.4))
    out.append(P(f"M7.5 12.2 C5.6 12.2 5.2 9.6 6.4 7.6 C7 9 7.5 8 7.6 6 C8.4 8 9.8 9.6 9.4 10.8 C9 11.8 8.4 12.2 7.5 12.2 Z", "#fff0c8", 0.2))
    out.append(eyes(7.5, 10, 1.0, 0.5))
    return svg(w, h, "".join(out), defs)


def ceiling_drop(falling=False):
    w, h = (11, 12) if falling else (15, 7)
    if falling:
        out = [P("M5.5 0.6 Q9.5 4 9.2 8 Q9 11.4 5.5 11.4 Q2 11.4 1.8 8 Q1.5 4 5.5 0.6 Z", "#2b3550")]
        out.append(E(5.5, 7.8, 2.2, 2.0, BONE))
        out.append(eyes(5.5, 7.8, 0.9, 0.5))
    else:
        out = [P("M0.6 0.6 L14.4 0.6 Q14 5.2 7.5 6.4 Q1 5.2 0.6 0.6 Z", "#2b3550")]
        out.append(E(7.5, 3.6, 2.4, 1.8, BONE))
        out.append(eyes(7.5, 3.6, 1.0, 0.5))
    return svg(w, h, "".join(out))


def rolling_stone(ph):
    w, h = 17, 16
    defs = noise_filter("grain", freq=0.6, octaves=3, seed=51, alpha=0.4, color=(0.9, 0.78, 0.62))
    out = [P("M3 2.6 L10 1 L15.5 4 L16 10 L13 15 L5 15.2 L1.2 10 Z", "#9a7a62")]
    out.append(P("M3 2.6 L10 1 L15.5 4 L12 6 L6 6.4 Z", "#c9a98c", 0.3))
    out.append(P("M15.5 4 L16 10 L13 15 L12.4 9 L12 6 Z", "#5e4536", 0.3))
    out.append(f'<path d="M8 1.6 L7.2 6 L9.4 9 L8 13" stroke="{INK}" stroke-width="0.5" fill="none" stroke-linecap="round"/>')
    out.append(f'<rect width="17" height="16" filter="url(#grain)" opacity=".3"/>')
    return svg(w, h, "".join(out), defs)


# ---------- hijos: flecha (15 × 4), bola del hechicero (9 × 9), bola del fantasma (5 × 5), murcielaguito (6 × 6), púa (15 × 7) ----------

def arrow():
    w, h = 15, 4
    out = [L(1, 2, 13, 2, INK, 1.0 + SW * 2), L(1, 2, 13, 2, "#9aa1ad", 1.0)]
    out.append(P("M12 0.4 L14.8 2 L12 3.6 Z", "#d6dbe6", 0.35))
    out.append(P("M0.4 0.6 L3 2 L0.4 3.4 L1.6 2 Z", "#c8342a", 0.3))
    return svg(w, h, "".join(out))


def magic_ball(ph):
    w, h = 9, 9
    defs = '<radialGradient id="g" cx="50%" cy="50%" r="50%"><stop offset="0" stop-color="#f2dcff"/><stop offset=".6" stop-color="#c9a6ff" stop-opacity=".8"/><stop offset="1" stop-color="#6a3a94" stop-opacity="0"/></radialGradient>'
    out = [f'<circle cx="4.5" cy="4.5" r="4.4" fill="url(#g)"/>', Cc(4.5, 4.5, 2.0, "#f2dcff", 0.4)]
    return svg(w, h, "".join(out), defs)


def small_orb(color):
    w, h = 5, 5
    defs = f'<radialGradient id="g" cx="50%" cy="50%" r="50%"><stop offset="0" stop-color="#ffffff"/><stop offset=".6" stop-color="{color}" stop-opacity=".9"/><stop offset="1" stop-color="{color}" stop-opacity="0"/></radialGradient>'
    return svg(w, h, f'<circle cx="2.5" cy="2.5" r="2.4" fill="url(#g)"/>' + Cc(2.5, 2.5, 1.0, "#ffffff", 0.3), defs)


def spine():
    w, h = 15, 7
    out = [P("M0.6 3.5 L13 1.2 L14.4 3.5 L13 5.8 Z", BONE, 0.45)]
    out.append(f'<path d="M2 3.5 L12.6 2.2" stroke="{BONE_SHADE}" stroke-width="0.45"/>')
    return svg(w, h, "".join(out))


def build():
    n = 0
    def e(name, s):
        nonlocal n
        emit(name, s, "enemigos")
        n += 1
    e("enemigo0e_colgado_00", bat_hanging())
    for i in range(4): e(f"enemigo0e_volar_{i:02d}", bat_flying(i / 4))
    # 0x0A caballero: el usuario prefiere el arquetipo generado (15 de octubre de 2026); no se construye.
    for s in range(1, 5): e(f"enemigo2e_fase{s}_00", sprout(s))
    e("enemigo31_pequeno_00", spikes(9, 8, 3, 1))
    e("enemigo31_grande_00", spikes(13, 13, 4, 2))
    e("enemigo31_ancho_00", spikes(13, 11, 4, 3))
    for i in range(4): e(f"enemigo18_rodar_{i:02d}", roller(i / 4))
    for i in range(2): e(f"enemigo19_pequeno_{i:02d}", bat_small(i / 2))
    for i in range(2): e(f"enemigo19_mediano_{i:02d}", bat_medium(i / 2))
    for i in range(2): e(f"enemigo19_grande_{i:02d}", bat_big(i / 2))
    e("enemigo34_quieto_00", stone_head())
    e("enemigo34_mover_00", stone_head(True))
    e("enemigo04_quieto_00", stone_head())
    for i in range(4): e(f"enemigo4e_volar_{i:02d}", soul(i / 4))
    for i in range(4): e(f"enemigo11_andar_{i:02d}", hedgehog(i / 4))
    e("enemigo11_descansa_00", hedgehog(0, resting=True))
    e("enemigo32_quieto_00", arrow_turret())
    e("enemigo51_quieto_00", arrow_turret())
    e("enemigo23_quieto_00", sleeping_statue())
    e("enemigo23_mover_00", sleeping_statue(True))
    for i in range(3): e(f"enemigo02_mover_{i:02d}", spark(i / 3))
    e("enemigo50_quieto_00", wall_mouth())
    e("enemigo3a_quieto_00", wall_mouth())
    for i in range(4): e(f"enemigo26_volar_{i:02d}", ghost(i / 4))
    for i in range(2): e(f"enemigo3c_quieto_{i:02d}", sorcerer(i / 2))
    for i in range(2): e(f"enemigo42_mover_{i:02d}", water_monster(i / 2))
    for i in range(4): e(f"enemigo21_andar_{i:02d}", falling_walker(i / 4))
    for i in range(2): e(f"enemigo09_rodar_{i:02d}", bouncing_ball(i * 0.4))
    for i in range(2): e(f"enemigo0d_nadar_{i:02d}", leap_fish(i / 2))
    for i in range(3): e(f"enemigo08_volar_{i:02d}", fireball(i / 3))
    e("enemigo38_quieto_00", ceiling_drop())
    e("enemigo38_mover_00", ceiling_drop(True))
    for i in range(2): e(f"enemigo35_rodar_{i:02d}", rolling_stone(i / 2))
    e("enemigo33_mover_00", arrow())
    e("enemigo3d_mover_00", magic_ball(0))
    e("enemigo4b_mover_00", small_orb("#9fd6ff"))
    e("enemigo48_volar_00", bat_small(0))
    e("enemigo48_volar_01", bat_small(0.5))
    e("enemigo1d_mover_00", spine())
    return n


if __name__ == "__main__":
    print(build(), "fotogramas")
