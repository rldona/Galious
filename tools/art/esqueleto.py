"""Esqueleto 0x25 dibujado: guerrero descarnado que se arrastra con su escudo por delante (solo se le hiere por la espalda).

Caja al andar: 4E, 25 × 16 px (docs/art/encargo/cajas_colision.md); el lienzo tiene esa proporción y el juego lo escala
a la caja. Pies en el borde de abajo, mirando a la derecha. Clips: enemigo25_mover (4) y enemigo25_quieto (2).
"""
import math

from svgkit import BONE, BONE_SHADE, BRONZE, BRONZE_DARK, HOLE, INK, SHELL, emit, svg

W, H = 25, 16
SW = 0.5


def P(d, fill, sw=SW):
    return f'<path d="{d}" fill="{fill}" stroke="{INK}" stroke-width="{sw}" stroke-linejoin="round" stroke-linecap="round" paint-order="stroke"/>'


def L(x0, y0, x1, y1, color, w):
    return f'<path d="M{x0:.2f} {y0:.2f} L{x1:.2f} {y1:.2f}" stroke="{color}" stroke-width="{w}" stroke-linecap="round" fill="none"/>'


def bone(x0, y0, x1, y1, w=1.4):
    return L(x0, y0, x1, y1, INK, w + SW * 2) + L(x0, y0, x1, y1, BONE, w)


def frame(ph, idle=False):
    """ph: fase 0..1 del arrastre. El cuerpo va bajo, a cuatro patas; la cabeza mira adelante; el escudo cubre el frente."""
    crawl = 0.0 if idle else math.sin(ph * 2 * math.pi)
    lift = 0.0 if idle else max(0.0, math.cos(ph * 2 * math.pi))
    out = []
    # columna y costillas (cuerpo tendido, de la cadera a los hombros)
    hip = (6.0, 10.5 - 0.3 * lift)
    sh = (15.0, 7.5 - 0.5 * lift)
    out.append(bone(*hip, *sh, 1.6))
    for k in range(4):
        t = (k + 1) / 5
        x = hip[0] + (sh[0] - hip[0]) * t
        y = hip[1] + (sh[1] - hip[1]) * t
        out.append(bone(x, y - 1.8, x + 0.4, y + 2.4, 0.8))
    # pierna trasera y pelvis
    out.append(P(f"M{hip[0] - 1.6:.2f} {hip[1] - 1.6:.2f} L{hip[0] + 1.4:.2f} {hip[1] - 1.8:.2f} L{hip[0] + 1.8:.2f} {hip[1] + 1.4:.2f} L{hip[0] - 1.8:.2f} {hip[1] + 1.6:.2f} Z", BONE_SHADE))
    kx, ky = hip[0] - 2.0 - crawl * 1.2, 13.0
    out.append(bone(hip[0], hip[1], kx, ky, 1.2))
    out.append(bone(kx, ky, kx + 2.5 + crawl * 1.5, 15.6, 1.2))
    # pierna delantera (más adelante, empujando)
    kx2, ky2 = hip[0] + 3.5 + crawl * 1.2, 13.4
    out.append(bone(hip[0] + 1.0, hip[1], kx2, ky2, 1.2))
    out.append(bone(kx2, ky2, kx2 - 1.5 - crawl * 1.4, 15.6, 1.2))
    # brazo trasero (apoyado en el suelo) y espada rota en la mano
    ex, ey = sh[0] - 1.5 + crawl * 1.0, 12.0
    out.append(bone(sh[0], sh[1], ex, ey, 1.1))
    out.append(bone(ex, ey, ex + 1.5, 15.6, 1.1))
    out.append(L(ex + 1.5, 15.2, ex + 6.0, 14.0, INK, 1.2 + SW * 2))
    out.append(L(ex + 1.5, 15.2, ex + 6.0, 14.0, "#9aa1ad", 1.2))
    # cráneo: cuenca grande y hueca, mandíbula abierta
    cx, cy = sh[0] + 3.2, sh[1] - 2.4 + 0.3 * crawl
    out.append(P(f"M{cx - 2.8:.2f} {cy + 2.4:.2f} L{cx - 3.0:.2f} {cy - 0.4:.2f} C{cx - 3.0:.2f} {cy - 3.6:.2f} {cx + 3.2:.2f} {cy - 3.6:.2f} {cx + 3.2:.2f} {cy - 0.4:.2f} "
                 f"L{cx + 3.0:.2f} {cy + 1.6:.2f} L{cx + 1.6:.2f} {cy + 2.6:.2f} L{cx - 0.6:.2f} {cy + 2.0:.2f} Z", BONE))
    out.append(f'<ellipse cx="{cx + 1.1:.2f}" cy="{cy - 0.6:.2f}" rx="1.25" ry="1.45" fill="{HOLE}"/>')
    out.append(f'<ellipse cx="{cx - 1.5:.2f}" cy="{cy - 0.5:.2f}" rx="0.8" ry="1.1" fill="{HOLE}"/>')
    out.append(f'<path d="M{cx + 1.5:.2f} {cy + 1.0:.2f} L{cx + 2.2:.2f} {cy + 1.0:.2f}" stroke="{HOLE}" stroke-width="0.6"/>')
    out.append(P(f"M{cx - 1.4:.2f} {cy + 2.6:.2f} L{cx + 2.4:.2f} {cy + 3.0:.2f} L{cx + 2.0:.2f} {cy + 4.2:.2f} L{cx - 0.8:.2f} {cy + 3.8:.2f} Z", BONE_SHADE, 0.4))
    out.append(f'<circle cx="{cx + 1.5:.2f}" cy="{cy - 0.8:.2f}" r="0.3" fill="#8fb3dd"/>')
    # brazo delantero con el escudo redondo de bronce, por delante de todo
    ax, ay = sh[0] + 1.0, sh[1] + 1.0
    scx, scy = 20.2, 10.4 - 0.4 * lift
    out.append(bone(ax, ay, scx - 1.5, scy + 1.0, 1.1))
    r = 4.4
    out.append(f'<circle cx="{scx:.2f}" cy="{scy:.2f}" r="{r}" fill="{BRONZE}" stroke="{INK}" stroke-width="{SW + 0.2}" paint-order="stroke"/>')
    out.append(f'<circle cx="{scx:.2f}" cy="{scy:.2f}" r="{r - 1.1:.2f}" fill="none" stroke="{BRONZE_DARK}" stroke-width="0.45"/>')
    out.append(f'<circle cx="{scx:.2f}" cy="{scy:.2f}" r="1.1" fill="{BRONZE_DARK}"/>')
    out.append(f'<path d="M{scx - 3.0:.2f} {scy - 1.4:.2f} L{scx + 2.2:.2f} {scy - 2.6:.2f} L{scx + 0.4:.2f} {scy + 2.8:.2f}" stroke="{INK}" stroke-width="0.35" fill="none" opacity=".7"/>')
    out.append(f'<path d="M{scx - 2.6:.2f} {scy - 2.6:.2f} A{r:.2f} {r:.2f} 0 0 1 {scx + 1.0:.2f} {scy - 4.0:.2f}" stroke="#ffffff" stroke-width="0.45" fill="none" opacity=".5"/>')
    # jirones de tela oscura colgando de la cadera
    out.append(P(f"M{hip[0] - 1.0:.2f} {hip[1] + 1.0:.2f} L{hip[0] - 3.5 - crawl:.2f} {hip[1] + 4.5:.2f} L{hip[0] - 1.5:.2f} {hip[1] + 3.6:.2f} L{hip[0] + 0.5:.2f} {hip[1] + 4.8:.2f} L{hip[0] + 1.5:.2f} {hip[1] + 1.2:.2f} Z", SHELL, 0.4))
    return svg(W, H, "".join(out))


def build():
    n = 0
    for i in range(4):
        emit(f"enemigo25_mover_{i:02d}", frame(i / 4), "enemigos")
        n += 1
    for i in range(2):
        emit(f"enemigo25_quieto_{i:02d}", frame(0.25 * i, idle=True), "enemigos")
        n += 1
    return n


if __name__ == "__main__":
    print(build(), "fotogramas")
