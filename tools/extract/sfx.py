#!/usr/bin/env python3
"""Catalogo de efectos de sonido (peticiones < 0x80 a sound_request, b00:4F12).

Cada efecto es un guion (mismo formato que los instrumentos de la musica, ver score.py) cuyo puntero
esta en b13:73DC + 2*id; suena en el canal de efectos (E15D, interprete b13:64A5). Prioridad: el
numero del efecto; uno nuevo solo entra si su id >= el que suena (E1B0).

Para cada id: duracion, tono inicial y final, si usa ruido, y donde se pide en el codigo
(LD A,id seguido de una llamada o salto a sound_request: 4E1F, 4E34 o el vector 4059).

Salida: extracted/sfx/sfx.json (y sfx_<id>.wav, sintesis aproximada del PSG para escucharlos)
"""
import json, math, re, struct, wave
from galious_rom import Rom, ROOT
from score import Score, midi_of, CLOCK, NAMES

SFX_TABLE, COUNT = 0x73DC, 0x31
CALLS = tuple(bytes([op]) + t for op in (0xCD, 0xC3, 0xDC, 0xD4, 0xCC, 0xC4, 0xCA, 0xC2, 0xDA, 0xD2)
              for t in (b'\x1f\x4e', b'\x59\x40', b'\x34\x4e'))   # CALL/JP, tambien condicionales


def addr(o):
    b = o // 0x2000
    return (0x4000 if b == 0 else 0x6000 + 0x2000 * ((b - 1) % 3)) + o % 0x2000


def callers(data):
    out = {}
    for m in re.finditer(rb'\x3e([\x00-\x30])', data):
        o = m.start()
        if any(data[o + 2:o + 2 + k].endswith(c) for c in CALLS for k in (3, 4, 5)):
            out.setdefault(data[o + 1], []).append(f'b{o // 0x2000:02d}:{addr(o):04X}')
    return out


def note_name(period):
    m = midi_of(period)
    return f'{NAMES[m % 12]}{m // 12 - 1}' if m is not None else None


def synth(steps, path, rate=22050):
    """Sintesis simple: onda cuadrada del tono y ruido blanco, volumen PSG logaritmico."""
    import random
    rnd = random.Random(1)
    frames = []
    phase = 0.0
    for dur, vol, per, noise in steps:
        amp = 0 if not vol else 10 ** ((vol - 15) * 1.5 / 20)
        for _ in range(dur):
            for _ in range(rate // 60):
                s = 0.0
                if per:
                    phase += CLOCK / (16 * per) / rate
                    s += 0.5 if (phase % 1) < 0.5 else -0.5
                if noise is not None:
                    s += rnd.uniform(-0.5, 0.5)
                frames.append(int(max(-1, min(1, s * amp)) * 12000))
    with wave.open(str(path), 'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(rate)
        w.writeframes(struct.pack(f'<{len(frames)}h', *frames))


def main():
    rom = Rom()
    sc = Score(rom)
    who = callers(rom.data)
    out = ROOT / 'extracted' / 'sfx'
    out.mkdir(parents=True, exist_ok=True)
    cat = {}
    for i in range(COUNT):
        ptr = rom.word(SFX_TABLE + 2 * i, 4)
        steps = sc.script(ptr, max_steps=600) if 0x6000 <= ptr < 0xA000 else []
        frames = sum(d for d, *_ in steps)
        pers = [p for d, v, p, n in steps if p and v]
        cat[f'{i:#04x}'] = {
            'ptr': f'{ptr:04X}', 'frames': frames, 'seconds': round(frames / 60, 2),
            'noise': any(n is not None and v for d, v, p, n in steps),
            'tone_start': note_name(pers[0]) if pers else None, 'tone_end': note_name(pers[-1]) if pers else None,
            'tone_min': note_name(max(pers)) if pers else None, 'tone_max': note_name(min(pers)) if pers else None,
            'callers': who.get(i, []),
        }
        if steps and frames:
            synth(steps, out / f'sfx_{i:02x}.wav')
        c = cat[f'{i:#04x}']
        print(f"{i:#04x} {c['seconds']:5.2f}s ruido={'si' if c['noise'] else 'no'} tono {c['tone_start']}->{c['tone_end']} "
              f"({c['tone_min']}..{c['tone_max']})  pedido en {' '.join(c['callers'][:5])}{' ...' if len(c['callers']) > 5 else ''}")
    (out / 'sfx.json').write_text(json.dumps(cat, indent=1))


if __name__ == '__main__':
    main()
