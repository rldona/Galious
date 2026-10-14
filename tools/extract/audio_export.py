#!/usr/bin/env python3
"""Sonido para el importador de Unity: registros del PSG frame a frame (docs/music.md).

  audio_export.py   -> extracted/unity/audio/music_<id>.bytes y sfx.bytes   (lo llama unity_bundle.py)

Música: las grabaciones de tools/openmsx/record_music.py (logs/omsx/music/<id>.json.gz, los 14 registros del PSG en
cada frame de 60 Hz) recortadas a una vuelta. El bucle se busca en la propia grabación: el periodo p más corto con el
que el final se repite
y el primer frame s desde el que todo se repite con él; de los posibles, el de vuelta más corta (s + p). Si la grabación acaba en
silencio, la canción no tiene bucle (fanfarrias, 0x81-0x85, 0x8E, 0x92, 0x94, 0x95) y se recortan los frames mudos
del final.
  music_<id>.bytes: 'PSG1', frames (u16), inicio del bucle (u16; 0xFFFF = sin bucle), y 14 bytes por frame.

Efectos: los guiones de b13:73DC + 2·id (sfx.py / score.py, Score.script), expandidos a un paso por frame para el
canal C, que es el que usan (docs/music.md, "Efectos de sonido").
  sfx.bytes: 'SFX1', número de efectos (u8), y por efecto: id (u8), frames (u16) y 5 bytes por frame:
  periodo de tono (u16, 0 = sin tono), volumen (0-15), periodo de ruido (0-31; 0xFF = sin ruido) y 1 byte a 0.

Es audio de Konami: queda en extracted/ y en Assets/Generated, que no se versionan.
"""
import gzip, json, struct
from galious_rom import Rom, ROOT
from score import Score

MUSIC = ROOT / 'logs' / 'omsx' / 'music'
OUT = ROOT / 'extracted' / 'unity' / 'audio'
SFX_TABLE, SFX_COUNT = 0x73DC, 0x31
G = 4


def frames_of(path):
    d = json.load(gzip.open(path))
    fr = d['frames']
    if isinstance(fr, str):
        fr = json.loads(fr.replace("'", '"'))
    return [bytes.fromhex(f) for f in fr]


def silent(f):
    mixer = f[7]
    return all((f[8 + c] & 0x1F) == 0 for c in range(3)) or (mixer & 0x3F) == 0x3F


def find_loop(fr, min_period=60):
    """(inicio, periodo) del bucle, o None si no se repite."""
    n = len(fr)
    tail = 240
    if n < 2 * min_period + tail:
        return None
    best = None
    # De todos los periodos con los que se repite el final, el que da la vuelta más corta (s + p): con el más corto a
    # secas, una frase repetida dentro del bucle (la 0x90 repite 384 frames dentro de su bucle de 1152) parece el bucle.
    for p in range(min_period, (n - tail) // 2 + 1):
        if all(fr[n - 1 - k] == fr[n - 1 - k - p] for k in range(tail)):
            s = n - p - 1
            while s > 0 and fr[s - 1] == fr[s - 1 + p]:
                s -= 1
            if best is None or s + p < sum(best):
                best = (s, p)
    if best is None or all(silent(f) for f in fr[best[0]:sum(best)]):
        return None
    return best


def music():
    out = {}
    for path in sorted(MUSIC.glob('*.json.gz')):
        sid = int(path.name[:2], 16)
        fr = frames_of(path)
        loop = find_loop(fr)
        if loop:
            s, p = loop
            fr = fr[:s + p]
            start = s
        else:
            while fr and silent(fr[-1]):
                fr.pop()
            start = 0xFFFF
        data = b'PSG1' + struct.pack('<HH', len(fr), start) + b''.join(f[:14] for f in fr)
        (OUT / f'music_{sid:02x}.bytes').write_bytes(data)
        out[sid] = (len(fr), start)
    return out


def sfx(rom):
    sc = Score(rom)
    effects = []
    for i in range(SFX_COUNT):
        ptr = rom.word(SFX_TABLE + 2 * i, G)
        steps = sc.script(ptr, max_steps=600) if 0x6000 <= ptr < 0xA000 else []
        frames = []
        for dur, vol, per, noise in steps:
            for _ in range(dur):
                frames.append(struct.pack('<HBB', per or 0, vol or 0, 0xFF if noise is None else noise))
        if frames:
            effects.append(struct.pack('<BH', i, len(frames)) + b''.join(f + b'\0' for f in frames))
    (OUT / 'sfx.bytes').write_bytes(b'SFX1' + bytes([len(effects)]) + b''.join(effects))
    return len(effects)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    m = music()
    n = sfx(Rom())
    loops = ', '.join(f'{k:02X}:{v[0]}' + (f'@{v[1]}' if v[1] != 0xFFFF else '') for k, v in m.items())
    print(f'sonido: {len(m)} canciones ({loops}), {n} efectos -> {OUT}')


if __name__ == '__main__':
    main()
