"""El ambiente de cada zona de Hollow Knight (Visuals/HollowAmbience en Unity): las texturas de sus partículas y la luz
del Caballero.

Team Cherry pone en cada escena un SceneManager (Assembly-CSharp/SceneManager.cs) con la saturación y las curvas de
color, la luz ambiente, el color de la luz del Caballero y el nivel de oscuridad (la viñeta de Knight/Vignette, con un
hueco alrededor del Caballero que la linterna agranda), y la cámara enciende las partículas de su zona
(SceneParticlesController). Aquí se copian las texturas de esas partículas (hollow-knight-texturas, muchas son tiras con
varios dibujos) a Assets/Art/Resources/Hollow/ambiente/<nombre>.png, fuera de git como el resto del arte de Team Cherry;
cuántos dibujos tiene cada tira y cómo se mueve cada zona va en HollowAmbience.
"""
import shutil
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/ambiente"

# nombre en el juego → textura de Hollow Knight (carpeta/nombre)
TEXTURES = {
    "mota": "otras/default_mote",                      # polvo de los Cruces Olvidados
    "hoja": "efectos/falling_leaf_particles",          # hojas de Sendero Verde (3)
    "gota": "efectos/particle_drip",                   # goteo de los Canales Reales
    "niebla": "efectos/Fog",                           # niebla baja de los Canales
    "hexagono": "efectos/hive_particle",               # la Colmena
    "cristal": "otras/mines_floating_crystals",        # cristales que flotan en el Pico de Cristal (3)
    "lluvia": "efectos/rain_particle",                 # la lluvia de la Ciudad de las Lágrimas
    "espora": "otras/fung_mote_02",                    # esporas de los Páramos Fúngicos
    "humo": "efectos/wispy_smoke_particle_abyss",      # humo del Abismo (5)
    "telarana": "efectos/web_particle",                # hilos de telaraña del Nido Profundo (4)
    "ceniza": "otras/white_ash",                       # ceniza de los Confines del Reino (4, fondo negro)
    "petalo": "otras/White_Flower_Petals",             # pétalos del Palacio Blanco (3)
    "luz": "efectos/light_effect_v02",                 # la luz suave alrededor del Caballero
}


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("*.png"):
        old.unlink()
    base = src / "hollow-knight-texturas"
    for name, tex in TEXTURES.items():
        shutil.copyfile(base / f"{tex}.png", OUT / f"{name}.png")
    print(f"ambiente: {len(TEXTURES)} texturas → {OUT}")
