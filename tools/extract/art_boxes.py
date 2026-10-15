#!/usr/bin/env python3
"""Lista de cajas de colisión para el encargo de arte → docs/art/encargo/cajas_colision.md.

Junta, para cada enemigo y jefe, lo que el dibujante necesita para ajustar su dibujo a lo que golpea:
  - extracted/unity/enemies.json (tools/extract/unity_bundle.py): vida, daño, cajas de la tabla b14:9006 por tipo;
  - extracted/unity/bosses.json (tools/extract/bosses.py): tamaño en tiles y cajas de cada pose de los jefes;
  - docs/enemy_ai.md: el nombre descriptivo de cada tipo (cabeceras "#### 0xNN — …");
  - unity/.../Enemies/EnemySystem.cs y Visuals/EnemyArt.cs: clase y familia visual (arquetipo) de cada tipo.

Las cajas del original tienen los bordes incluidos: una caja (dy, dx, alto, ancho) cubre (alto + 1) × (ancho + 1) px.
Aquí se dan ya en píxeles cubiertos. El dibujo se entrega a 8 texels por píxel del original.

Uso: python3 tools/extract/art_boxes.py
"""
import json
import re
from collections import OrderedDict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ENEMIES = ROOT / "extracted/unity/enemies.json"
BOSSES = ROOT / "extracted/unity/bosses.json"
AI_DOC = ROOT / "docs/enemy_ai.md"
SYSTEM_CS = ROOT / "unity/MazeOfKnight/Assets/Scripts/Runtime/Game/Enemies/EnemySystem.cs"
ART_CS = ROOT / "unity/MazeOfKnight/Assets/Scripts/Runtime/Game/Visuals/EnemyArt.cs"
OUT = ROOT / "docs/art/encargo/cajas_colision.md"
TEXELS = 8

FAMILY = {
    "Beetle": "escarabajo andante", "Worm": "gusano o babosa", "Winged": "alado", "Wisp": "espíritu", "Ball": "bola",
    "Fish": "pez", "Flame": "llama o chorro", "Stone": "trampa de piedra", "Robe": "hechicero", "Spikes": "pinchos",
    "Orb": "proyectil (orbe)", "Bone": "hueso", "Dart": "dardo", "Plant": "brote", "Loot": "botín", "Burst": "explosión",
    "Baby": "bebé", "Fairy": "hada",
}
MOD = {
    "Horns": "cuernos", "Shield": "escudo", "Spear": "lanza", "Skeleton": "esqueleto", "Wide": "ancho", "Spiky": "pinchos",
    "Shell": "caparazón", "Rock": "piedra", "Bomb": "mecha", "Water": "agua", "Mouth": "boca", "Piston": "pistón",
    "Arrow": "flechas", "Coins": "monedas", "Keys": "llaves", "Head": "cabeza",
}
# Grupos de la ficha de IA (docs/enemy_ai.md): tipos de las salas, hijos y generados periódicamente.
PERIODIC = {0x09, 0x38, 0x08, 0x1B, 0x6A, 0x0D, 0x21, 0x35, 0x30, 0x3E}
LOOT = {0x5D: "flechas", 0x5E: "monedas", 0x5F: "llaves"}
SPECIAL = {0x5B: "explosión de la muerte (efecto)", 0x4F: "hada", 0x6B: "bebé (final)"}
# Hijos documentados en enemy_ai.md con "/" en la cabecera.
WORLD_NAMES = {0: "castillo"}


def names_from_doc():
    """tipo → (nombre, grupo) según la sección de docs/enemy_ai.md: sala, hijos o periodicos."""
    names = {}
    group = "sala"
    for line in AI_DOC.read_text(encoding="utf-8").splitlines():
        if line.startswith("## "):
            low = line.lower()
            group = "hijos" if "hijos" in low else "periodicos" if "periódicamente" in low or "periodicamente" in low else "sala"
        m = re.match(r"#### (0x[0-9A-Fa-f]{2}(?: / 0x[0-9A-Fa-f]{2})*) — (.+)", line)
        if not m:
            continue
        desc = m.group(2).strip()
        desc = re.sub(r"\s*\(.*?\)\s*$", "", desc).strip().strip('"“”')
        for t in re.findall(r"0x([0-9A-Fa-f]{2})", m.group(1)):
            names[int(t, 16)] = (desc, group)
    return names


def classes_from_system():
    classes = {}
    for m in re.finditer(r"\{\s*0x([0-9A-Fa-f]{2}),\s*\(\)\s*=>\s*new\s+(\w+)\(", SYSTEM_CS.read_text(encoding="utf-8")):
        classes[int(m.group(1), 16)] = m.group(2)
    return classes


def families_from_art():
    fam = {}
    pending = []
    src = ART_CS.read_text(encoding="utf-8")
    body = src[src.index("Classify(Enemy e)"):]
    body = body[: body.index("// Jefes sin su imagen")]
    for line in body.splitlines():
        pending += re.findall(r'case "(\w+)":', line)
        r = re.search(r"return \(Kind\.(\w+), ((?:Mod\.\w+(?: \| )?)+)", line)
        if r:
            mods = [MOD[m] for m in re.findall(r"Mod\.(\w+)", r.group(2)) if m != "None" and m in MOD]
            label = FAMILY.get(r.group(1), r.group(1)) + (f" ({', '.join(mods)})" if mods else "")
            for c in pending:
                fam[c] = label
            pending = []
    return fam


def px(rect):
    return rect["w"] + 1, rect["h"] + 1


def main():
    data = json.loads(ENEMIES.read_text(encoding="utf-8"))
    bosses = json.loads(BOSSES.read_text(encoding="utf-8"))
    names = names_from_doc()
    classes = classes_from_system()
    families = families_from_art()
    rects = data["boxRects"]
    enemies = sorted(data["enemies"], key=lambda e: e["type"])

    out = []
    w = out.append
    w("# Cajas de colisión para el encargo de arte")
    w("")
    w("Generado por `tools/extract/art_boxes.py` a partir de los datos extraídos del original (docs/items_enemies.md, "
      "«Cajas de colisión»; docs/boss_ai.md, «Cajas de los jefes»). **Lo que se ve es lo que golpea:** cada fotograma se "
      "dibuja en un lienzo con la proporción de su caja y el juego lo escala exactamente a ella. Un dibujo que sobresalga "
      "de la caja engaña al jugador; uno más pequeño deja aire que hace daño.")
    w("")
    w(f"Medidas en píxeles del original. El arte se entrega a **{TEXELS} texels por píxel**: un enemigo de 16 × 16 px se "
      f"dibuja en {16 * TEXELS} × {16 * TEXELS} texels. Las cajas del original incluyen sus bordes; aquí van ya como "
      "píxeles cubiertos (ancho × alto).")
    w("")
    w("## Jugador")
    w("")
    pc = data["playerContactBox"]
    sw = data["swordBoxRight"]
    w("| Caja | Desde los pies (x centro, y pies) | Píxeles | Texels |")
    w("|---|---|---|---|")
    w(f"| Contacto con enemigos y objetos | x {pc['dx']:+d} … x {pc['dx'] + pc['w']:+d}, y {pc['dy']:+d} … y {pc['dy'] + pc['h']:+d} "
      f"| {pc['w'] + 1} × {pc['h'] + 1} | {(pc['w'] + 1) * TEXELS} × {(pc['h'] + 1) * TEXELS} |")
    w(f"| Espada (a la derecha; a la izquierda, simétrica) | x {sw['dx']:+d} … x {sw['dx'] + sw['w']:+d}, y {sw['dy']:+d} … y {sw['dy'] + sw['h']:+d} "
      f"| {sw['w'] + 1} × {sw['h'] + 1} | {(sw['w'] + 1) * TEXELS} × {(sw['h'] + 1) * TEXELS} |")
    w("| Sprite del original | 16 × 16 con los pies en el borde de abajo | 16 × 16 | 128 × 128 |")
    w("")
    w("- La caja del jugador no cambia con la postura (escalera, salto, espadazo). El dibujo puede ser más alto que la caja "
      "(yelmo, cresta) siempre que el cuerpo que «se toca» quede dentro de los 13 × 13 px.")
    w("- La hoja de la espada llega a x + 18 en el tick del golpe. La estocada golpea en el **primer** tick del espadazo: "
      "el fotograma «golpe» es el primero que se ve; la anticipación no cabe antes.")
    w("- Armas secundarias (cajas del original): flecha y flecha de cerámica 13 × 4 (en la rejilla de 8 px), fuego rodante 9 × 9, "
      "fuego 9 × 5, mina 13 × 5 y su explosión 33 × 33.")
    w("")

    def row(e):
        t = e["type"]
        name = SPECIAL.get(t) or (f"botín: {LOOT[t]}" if t in LOOT else names.get(t, ("—", ""))[0])
        cls = classes.get(t, "")
        fam = families.get(cls, "")
        boxes = e.get("boxes") or ([e["box"]] if e.get("box") is not None else [])
        sizes = []
        for b in boxes:
            if 0 <= b < len(rects):
                bw, bh = px(rects[b])
                sizes.append(f"{bw}×{bh}")
        sizes = OrderedDict.fromkeys(sizes)
        init = ""
        if e.get("box") is not None and 0 <= e["box"] < len(rects):
            bw, bh = px(rects[e["box"]])
            init = f"{bw} × {bh}"
        hp = "indestructible" if e.get("invulnerable") else str(e.get("hp", ""))
        special = e.get("boxSpecial") or ""
        note = (e.get("boxNote") or "").replace("|", "/")
        if special:
            note = f"caja calculada ({special}). " + note
        canvas = ""
        if sizes:
            mw = max(int(s.split("×")[0]) for s in sizes)
            mh = max(int(s.split("×")[1]) for s in sizes)
            canvas = f"{mw * TEXELS} × {mh * TEXELS}"
        return (f"| 0x{t:02X} | {name} | {fam} | {hp} | {e.get('damage', '')} | {init} | {' '.join(sizes)} | "
                f"{'sí' if e.get('boxSizeChanges') else 'no'} | {canvas} | {note} |")

    header = ("| Tipo | Qué es | Familia visual | Vida | Daño | Caja inicial | Cajas (ancho×alto px) | Cambia | Lienzo mayor (texels) | Notas |\n"
              "|---|---|---|---|---|---|---|---|---|---|")

    def group(e):
        t = e["type"]
        if t in names:
            return names[t][1]
        return "periodicos" if t in PERIODIC else "hijos"
    room = [e for e in enemies if group(e) == "sala"]
    w("## Enemigos de las salas")
    w("")
    w(f"{len(room)} tipos colocados en las salas (docs/enemy_ai.md, grupos G1–G4). Daño en unidades de vitalidad (la barra llena son 8).")
    w("")
    w(header)
    for e in room:
        w(row(e))
    w("")
    periodic = [e for e in enemies if group(e) == "periodicos"]
    w("## Generados periódicamente")
    w("")
    w(f"{len(periodic)} tipos que salen de los generadores de las salas (bolas de fuego de la lava, peces, bolas rodantes…).")
    w("")
    w(header)
    for e in periodic:
        w(row(e))
    w("")
    children = [e for e in enemies if group(e) == "hijos"]
    w("## Hijos: proyectiles, botín y otros")
    w("")
    w(f"{len(children)} tipos que crean otros enemigos (proyectiles, llamaradas, esporas…), el botín, el hada, la explosión y el bebé.")
    w("")
    w(header)
    for e in children:
        w(row(e))
    w("")

    w("## Jefes")
    w("")
    w("Tamaño del bloque de tiles de cada pose (lo que se dibuja) y sus dos cajas: la que **recibe las armas** y la que "
      "**hace daño al tocarla**, ambas desde la esquina superior izquierda del bloque. Los jefes se dibujan a tamaño del "
      "bloque: el lienzo es tiles × 8 px × 8 texels. El original cambia de pose por fotograma; aquí cada pose es una fila.")
    w("")
    for b in sorted(bosses["bosses"], key=lambda b: b["world"]):
        w(f"### Mundo {b['world']}: {b['name']} (palabra {b['word']}, vida {b['hp']}, daño al tocarlo {b['contactDamage']})")
        w("")
        kind = b.get("hitTest", {}).get("kind", "")
        notes = {
            "minSprite": "solo recibe golpes en las poses abiertas (a partir del sprite mínimo)",
            "cross": "solo la Cross le hace daño, en el núcleo",
            "threeBoxes": "tres cajas de armas por pose (vale cualquiera)",
            "head": "las armas solo cuentan en la cabeza",
            "elusive": "cajas calculadas: el cuello crece por tiles; las armas solo cuentan en los 64 px del lado de la cabeza",
            "none": "sin prueba de armas",
        }
        if kind in notes:
            w(f"- {notes[kind]}.")
        w("")
        w("| Pose (sprite) | Parte | Bloque (tiles) | Dibujo (px) | Lienzo (texels) | Caja de armas (x, y, ancho×alto) | Caja de contacto (x, y, ancho×alto) |")
        w("|---|---|---|---|---|---|---|")
        for s in b["sprites"]:
            def box(k):
                r = s.get(k) or {}
                if not r.get("valid"):
                    return ""
                return f"({r['dx']}, {r['dy']}) {r['w'] + 1}×{r['h'] + 1}"
            weapon = " · ".join(x for x in (box("weaponBox"), box("weaponBox2"), box("weaponBox3")) if x) or "—"
            contact = " · ".join(x for x in (box("contactBox"), box("contactBox2")) if x) or "—"
            w(f"| 0x{s['id']:02X} | {s.get('part', '')} | {s['wTiles']} × {s['hTiles']} | {s['w']} × {s['h']} | "
              f"{s['w'] * TEXELS} × {s['h'] * TEXELS} | {weapon} | {contact} |")
        w("")

    w("## Tabla de cajas del original (b14:9006)")
    w("")
    w("Índice → (dx, dy) desde la posición del enemigo y ancho × alto cubiertos. Es la que citan las notas de arriba.")
    w("")
    w("| i | dx, dy | px | i | dx, dy | px | i | dx, dy | px | i | dx, dy | px |")
    w("|---|---|---|---|---|---|---|---|---|---|---|---|")
    cells = []
    for i, r in enumerate(rects):
        bw, bh = px(r)
        cells.append(f"{i:02X} | {r['dx']}, {r['dy']} | {bw}×{bh}")
    for k in range(0, len(cells), 4):
        chunk = cells[k:k + 4] + [" |  | "] * (4 - len(cells[k:k + 4]))
        w("| " + " | ".join(chunk) + " |")
    w("")
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text("\n".join(out) + "\n", encoding="utf-8")
    print(f"{OUT}: {len(room)} tipos de sala, {len(periodic)} periódicos, {len(children)} hijos, {len(bosses['bosses'])} jefes")


if __name__ == "__main__":
    main()
