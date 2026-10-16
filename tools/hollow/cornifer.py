"""Cornifer, el cartógrafo de Hollow Knight, para Galious: The Hollow Kingdom (Cornifers y Visuals/CorniferView en Unity).

Sale a Assets/Art/Resources/Hollow/cornifer/ (fuera de git, como el resto del arte, el sonido y los textos de Team Cherry):
  - cornifer_escribir_NN.png: sentado dibujando su mapa (fotogramas 0–9 del atlas «Cornifer Cln»);
  - cornifer_hablar_NN.png: mirando al Caballero mientras habla (23–28).
    Todos en el mismo lienzo, alineados por abajo en el centro de su celda del atlas, a la escala del Caballero
    (×1,375) y con el pivote en los pies (la carpeta está en las de pies del ArtImporter);
  - tarareo.wav: su tarareo, que se oye al acercarse (Cornifer_Hum, convertido con afconvert);
  - textos.json: sus frases en castellano ({"lines": [{key, text}]}; las páginas separadas por «<page>»): la de
    conocerle, el saludo y lo que dice tras vender el mapa en la zona de Hollow Knight de cada mundo, y las de
    preguntar, no tener geo, no querer y volver.
"""
import json
import re
import subprocess
from pathlib import Path

from PIL import Image

import atlas_piezas

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/cornifer"
SCALE = 1.375
CLIPS = {"escribir": range(0, 10), "hablar": range(23, 29)}
LINES = ["CORNIFER_MEET", "CORNIFER_PROMPT", "CORNIFER_NOT_ENOUGH", "CORNIFER_REFUSE", "CORNIFER_AGAIN",
         "CROSSROADS_GREET", "CROSSROADS_BOUGHT", "GREENPATH_GREET", "GREENPATH_BOUGHT", "WATERWAYS_GREET", "WATERWAYS_BOUGHT", "FOG_CANYON_GREET",
         "FOG_CANYON_BOUGHT", "MINES_GREET", "MINES_BOUGHT", "CITY_GREET", "CITY_BOUGHT", "FUNGAL_WASTES_GREET",
         "FUNGAL_WASTES_BOUGHT", "ABYSS_GREET", "ABYSS_BOUGHT", "DEEPNEST_GREET", "DEEPNEST_BOUGHT", "OUTSKIRTS_GREET",
         "OUTSKIRTS_BOUGHT"]


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    atlas = Image.open(src / "hollow-knight-texturas/atlas_tk2d/Cornifer Cln_atlas0.png").convert("RGBA")
    boxes = atlas_piezas.boxes(atlas)
    w = max(b[2] - b[0] for b in boxes)
    h = max(b[3] - b[1] for b in boxes)
    for clip, frames in CLIPS.items():
        for n, i in enumerate(frames):
            x0, y0, x1, y1 = boxes[i]
            canvas = Image.new("RGBA", (w, h))
            canvas.paste(atlas.crop(boxes[i]), ((w - (x1 - x0)) // 2, h - (y1 - y0)))
            canvas = canvas.resize((round(w * SCALE), round(h * SCALE)), Image.LANCZOS)
            canvas.save(OUT / f"cornifer_{clip}_{n:02d}.png")
    subprocess.run(["afconvert", "-f", "WAVE", "-d", "LEI16@44100", "-c", "1",
                    str(src / "hollow-knight-audio/efectos/Cornifer_Hum.m4a"), str(OUT / "tarareo.wav")], check=True)
    xml = (src / "hollow-knight-textos/es/Cornifer.xml").read_text(encoding="utf-8")
    entries = dict(re.findall(r'<entry name="([^"]+)">(.*?)</entry>', xml, re.S))
    unesc = lambda t: t.replace("&lt;", "<").replace("&gt;", ">").replace("&amp;", "&").strip()
    lines = [dict(key=k, text=unesc(entries[k])) for k in LINES if k in entries]
    (OUT / "textos.json").write_text(json.dumps({"lines": lines}, ensure_ascii=False, indent=1), encoding="utf-8")
    print(f"cornifer: {sum(len(f) for f in CLIPS.values())} fotogramas, tarareo y {len(lines)} frases → {OUT}")
