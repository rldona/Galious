"""Utilidades para dibujar el arte de Maze of Knight como SVG y rasterizarlo con rsvg-convert.

Las medidas de los dibujos van en píxeles del original (un tile = 8, un personaje cabe en 48 × 32) y se rasterizan a
TEXELS texels por píxel (docs/art/encargo/README.md: 8). Todo es arte propio: nada sale de la ROM ni de otro juego.
"""
import subprocess
from pathlib import Path

TEXELS = 8
ROOT = Path(__file__).resolve().parents[2]
SVG_DIR = ROOT / "art" / "svg"
OUT_DIR = ROOT / "unity/MazeOfKnight/Assets/Art/Resources/Dibujado"

# Paleta del castillo (WorldPalette.cs) y la familia común (hoja de papeles de color).
CASTLE = dict(sky_top="#141a29", sky_bottom="#3b4862", far="#5a6884", mid="#2c3650", ground="#2a3142", rim="#d6e0f2",
              fog="#8b9cba", accent="#ffd59a", motes="#e2ebff", liquid="#3d6fa8")
INK = "#07070b"
BONE = "#ece6da"
BONE_SHADE = "#bfb7a6"
HOLE = "#08080d"
SHELL = "#272a3a"
SHELL_LIGHT = "#3b4058"
CREST = "#c8342a"
CREST_LIGHT = "#e8564a"
CAPE = "#5a1d2a"
CAPE_LIGHT = "#7a2a3a"
BRONZE = "#b07a2c"
BRONZE_DARK = "#6e3a1c"
GOLD = "#e5b65a"
GOLD_LIGHT = "#fff3d6"
STEEL = "#d6dbe6"
WARM_STONE = "#9a7a62"
WARM_STONE_DARK = "#5e4536"
WARM_STONE_LIGHT = "#c9a98c"


def svg(w, h, body, defs="", texels=TEXELS):
    """Documento SVG de w × h píxeles del original, con el viewBox en píxeles y el tamaño final en texels."""
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="{w * texels}" height="{h * texels}" viewBox="0 0 {w} {h}">'
            f'<defs>{defs}</defs>{body}</svg>')


def noise_filter(fid, freq=0.6, octaves=3, seed=1, alpha=0.35, color=(0.5, 0.5, 0.55)):
    """Filtro de grano: ruido fractal teñido, para dar textura de piedra o niebla. alpha = fuerza."""
    r, g, b = color
    return (f'<filter id="{fid}" x="0" y="0" width="100%" height="100%">'
            f'<feTurbulence type="fractalNoise" baseFrequency="{freq}" numOctaves="{octaves}" seed="{seed}" result="n"/>'
            f'<feColorMatrix in="n" type="matrix" values="0 0 0 0 {r}  0 0 0 0 {g}  0 0 0 0 {b}  0 0 0 {alpha} 0"/>'
            f'</filter>')


def blur_filter(fid, radius):
    return f'<filter id="{fid}" x="-20%" y="-20%" width="140%" height="140%"><feGaussianBlur stdDeviation="{radius}"/></filter>'


def rasterize(svg_text, out_png, w_texels=None, h_texels=None):
    out_png = Path(out_png)
    out_png.parent.mkdir(parents=True, exist_ok=True)
    args = ["rsvg-convert"]
    if w_texels:
        args += ["-w", str(w_texels)]
    if h_texels:
        args += ["-h", str(h_texels)]
    args += ["-o", str(out_png)]
    subprocess.run(args, input=svg_text.encode("utf-8"), check=True)


def save_svg(name, svg_text, folder):
    p = SVG_DIR / folder / f"{name}.svg"
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(svg_text, encoding="utf-8")
    return p


def emit(name, svg_text, folder):
    """Guarda el SVG fuente en art/svg/<folder>/ y el PNG en Dibujado/<folder>/."""
    save_svg(name, svg_text, folder)
    rasterize(svg_text, OUT_DIR / folder / f"{name}.png")


def path(d, fill="none", stroke=None, sw=0.6, extra=""):
    s = f'<path d="{d}" fill="{fill}"'
    if stroke:
        s += f' stroke="{stroke}" stroke-width="{sw}" stroke-linejoin="round" stroke-linecap="round"'
    return s + f' {extra}/>'


def inked(d, fill, sw=0.7, extra=""):
    """Forma rellena con contorno de tinta (el trazo va por fuera: se dibuja primero el contorno más grueso)."""
    return (f'<path d="{d}" fill="{INK}" stroke="{INK}" stroke-width="{sw * 2}" stroke-linejoin="round" stroke-linecap="round" {extra}/>'
            f'<path d="{d}" fill="{fill}" {extra}/>')
