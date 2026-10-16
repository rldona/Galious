"""Música de Hollow Knight para Galious: The Hollow Kingdom (Visuals/HollowMainMenu y Visuals/HollowMusic en Unity).

Sale a Assets/Art/Resources/Hollow/musica/ (fuera de git, como el resto del arte y el sonido de Team Cherry), en mp3:
  - titulo: el tema del menú principal (Title);
  - castillo, mundo1 … mundo10: la de la zona de Hollow Knight de cada mapa (la misma que su escenario, escenario.py);
    las zonas que en Hollow Knight suenan por capas (bajo y melodía a la vez) se mezclan en una pista;
  - santuario: la de las salas tranquilas con un personaje (Inn);
  - jefe1 … jefe11: la de la pelea de Hollow Knight con el jefe que hace de cada uno (enemigos.py; el 5 no se usa).
Las fanfarrias del original (objetos, invocación, jefe vencido, final) siguen con el chip PSG (Sound.cs).
"""
import subprocess
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/musica"
LOUDNESS = -14            # LUFS de todas las pistas (lo pidió el usuario: la música se oía muy baja frente a los efectos)

TRACKS = {
    "titulo": ["Title"],
    "castillo": ["S19 Crossroads Bass", "S19 Crossroads Main"],
    "mundo1": ["S5 Green Path Bass", "S5 Green Path Main"],
    "mundo2": ["S31 Waterways"],
    "mundo3": ["S68-04 HIVE"],
    "mundo4": ["S26 Crystal BASS", "S26 Crystal MAIN"],
    "mundo5": ["S23-11 OUTSIDE LOOP", "S23-11 INSIDE LOOP"],
    "mundo6": ["S25 Fungal Wastes BASS Pizz", "S25 Fungal Wastes MAIN"],
    "mundo7": ["Abyss-01"],
    "mundo8": ["S32_Deepnest"],
    "mundo9": ["S39 Kingdoms Edge-23"],
    "mundo10": ["S30 White Palace"],
    "santuario": ["S56-03 Optional NPC Room"],
    "jefe1": ["Boss Battle 1"],                    # Bony Dragon → False Knight
    "jefe2": ["S47-85 Mantis Lords"],              # Elusive Dragon → Mantis Lords
    "jefe3": ["S45 HORNET-110"],                   # Arlyane → Traitor Lord
    "jefe4": ["S53-30 Dream Battle"],              # Sea Anemone → Uumuu
    "jefe6": ["S82-115 Grimm"],                    # King Dragon → Grimm
    "jefe7": ["S34-43 MAGE BOSS"],                 # Big Lip → Mawlek
    "jefe8": ["S52-99 BROKEN WANDERER"],           # Monster → Broken Vessel
    "jefe9": ["S72-140 FINAL FINAL BOSS"],         # Galious → Radiance
    "jefe10": ["S49B-23 MIMIC SPIDER"],            # Donra → Nosk
    "jefe11": ["S48-66 Dung Defender"],            # Jump Slime → Dung Defender
}


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for name, parts in TRACKS.items():
        out = OUT / f"{name}.mp3"
        inputs = []
        for p in parts:
            inputs += ["-i", str(src / "hollow-knight-audio/musica" / f"{p}.m4a")]
        # Las pistas de Hollow Knight vienen muy bajas (−25 a −29 dB de media) y quedaban por debajo de los efectos del chip:
        # se normalizan a LOUDNESS LUFS (con los picos a −1,5 dB) después de mezclar las capas.
        norm = f"loudnorm=I={LOUDNESS}:TP=-1.5:LRA=11"
        mix = ["-filter_complex", f"amix=inputs={len(parts)}:duration=longest:normalize=0,{norm}"] if len(parts) > 1 else ["-af", norm]
        subprocess.run(["ffmpeg", "-y", "-loglevel", "error", *inputs, *mix, "-ar", "44100", "-c:a", "libmp3lame", "-b:a", "160k", str(out)],
                       check=True)
    print(f"música: {len(TRACKS)} pistas → {OUT}")
