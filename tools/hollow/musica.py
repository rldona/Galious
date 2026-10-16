"""Música de Hollow Knight para Galious: The Hollow Kingdom (Visuals/HollowMainMenu en Unity).

Sale a Assets/Art/Resources/Hollow/musica/ (fuera de git, como el resto del arte y el sonido de Team Cherry):
  - titulo.mp3: el tema del menú principal de Hollow Knight (Title), convertido con ffmpeg.
La música de la partida sigue siendo la del original (el chip PSG, Sound.cs).
"""
import subprocess
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "unity/MazeOfKnight/Assets/Art/Resources/Hollow/musica"
TRACKS = {"titulo": "Title.m4a"}


def build(src):
    OUT.mkdir(parents=True, exist_ok=True)
    for name, track in TRACKS.items():
        subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", str(src / "hollow-knight-audio/musica" / track),
                        "-c:a", "libmp3lame", "-b:a", "192k", str(OUT / f"{name}.mp3")], check=True)
    print(f"música: {len(TRACKS)} pistas → {OUT}")
