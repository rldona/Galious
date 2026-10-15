#!/usr/bin/env python3
"""Construye el arte dibujado de Maze of Knight: SVG en art/svg/ y PNG en Assets/Art/Resources/Dibujado/.

Uso: python3 tools/art/build_art.py [castillo|popolon|esqueleto ...]   (sin argumentos: todo)
Necesita rsvg-convert (brew install librsvg).
"""
import sys
import time
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import castillo  # noqa: E402
import enemigos  # noqa: E402
import esqueleto  # noqa: E402
import mundos  # noqa: E402
import popolon  # noqa: E402
from svgkit import OUT_DIR  # noqa: E402

# Los personajes dibujados (popolon.py) se descartaron el 15 de octubre de 2026: el usuario prefiere el Popolon generado por
# código (CharacterArt.cs), su estocada y su animación. El generador queda por si se retoma, pero no se construye por defecto.
BUILDERS = {"castillo": castillo.build, "esqueleto": esqueleto.build, "enemigos": enemigos.build, "mundos": mundos.build}
OPTIONAL = {"popolon": popolon.build}


def main(args):
    names = args or list(BUILDERS)
    t0 = time.time()
    for n in names:
        r = {**BUILDERS, **OPTIONAL}[n]()
        print(f"{n}: {r if r is not None else 'ok'}")
    pngs = list(OUT_DIR.rglob("*.png"))
    print(f"{len(pngs)} PNG en {OUT_DIR} ({sum(p.stat().st_size for p in pngs) // 1024} KB) en {time.time() - t0:.1f} s")


if __name__ == "__main__":
    main(sys.argv[1:])
