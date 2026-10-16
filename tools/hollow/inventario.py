"""Piezas del inventario y de los amuletos de Hollow Knight para Galious: The Hollow Kingdom (InventoryMenu y
Visuals/ItemTutorial en Unity).

Sale a Assets/Art/Resources/Hollow/inventario/ (fuera de git, como el resto del arte de Team Cherry):
  - hueco.png: el hueco vacío de un amuleto (charm_backboard);
  - muesca_llena.png / muesca_vacia.png: las muescas gastadas y libres;
  - coste.png: la muesca pequeña del coste de un amuleto;
  - flecha_pestana.png: la flecha de los lados para cambiar de pestaña (LT/RT);
  - nuevo.png: el punto que marca lo nuevo;
  - luz.png: el halo de las láminas de tutorial;
  - lamina_separador.png: la línea con filigrana de las láminas (prompt_divider);
  - lamina_amuletos.png: el dibujo del Caballero en el banco con los amuletos (Charms_prompt).
"""
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/inventario"

PIECES = {
    "hueco": "hollow-knight-inventario/Charms/charm_backboard.png",
    "muesca_llena": "hollow-knight-inventario/Charms/charm_UI__0000_charm_cost_02_lit.png",
    "muesca_vacia": "hollow-knight-inventario/Charms/charm_UI__0001_charm_cost_02_unlit.png",
    "coste": "hollow-knight-inventario/Charms/charm_cost.png",
    "flecha_pestana": "hollow-knight-inventario/_compartidos/RT_LT_arrow.png",
    "nuevo": "hollow-knight-inventario/Charms/new_item_orb.png",
    "luz": "hollow-knight-inventario/_compartidos/light_effect_v02.png",
    "lamina_separador": "hollow-knight-texturas/interfaz/prompt_divider.png",
    "lamina_amuletos": "hollow-knight-texturas/interfaz/Charms_prompt.png",
}


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    for name, path in PIECES.items():
        Image.open(src / path).convert("RGBA").save(OUT / f"{name}.png")
    print(f"inventario: {len(PIECES)} piezas → {OUT}")
