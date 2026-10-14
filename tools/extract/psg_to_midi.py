#!/usr/bin/env python3
"""Convierte las grabaciones del PSG (tools/openmsx/record_music.py) en MIDI y en un resumen.

Para cada frame (60 Hz) se leen los 14 registros del AY-3-8910:
  R0-R5 periodo de tono de A, B, C (12 bits) · R6 periodo de ruido · R7 mezclador (bit = 0: activo)
  R8-R10 volumen de A, B, C (bit 4 = usa la envolvente) · R11-R13 envolvente
Frecuencia del tono = 3579545 / (16 * periodo) (reloj del PSG del MSX: 1,7897725 MHz).
Una nota empieza cuando el canal suena y cambia el periodo o el volumen sube; termina al callar.
El volumen del PSG se pasa a velocity. El ruido va a una pista de percusion (canal 10).

Bucle: busca el menor periodo P (>= 2 s) tal que el final de la grabacion se repite durante mas de
un periodo entero (P + 10 s), retrocede hasta donde empieza a repetirse y exporta la intro mas una
vuelta. Hace falta grabar al menos dos vueltas.

Salida: extracted/music/<id>.mid (musica de Konami: no se versiona) y extracted/music/index.json
"""
import gzip, json, math, struct
from galious_rom import ROOT

CLOCK = 3579545 / 2
TPQ, BPM = 480, 150          # 60 frames = 1 s = 2,5 negras a 150 bpm -> 1 frame = 20 ticks
TICKS_PER_FRAME = TPQ * BPM / 60 / 60


def find_loop(frames):
    n = len(frames)
    for p in range(120, n // 2):
        start = n - 2 * p - 600          # la repeticion debe cubrir al menos un periodo entero
        if start < 0:
            break
        if all(frames[i] == frames[i + p] for i in range(start, n - p)):
            # retrocede hasta donde empieza a repetirse
            s = start
            while s > 0 and frames[s - 1] == frames[s - 1 + p]:
                s -= 1
            return s, p
    return None, None


def note_of(period):
    if period <= 0:
        return None
    f = CLOCK / (16 * period)
    return round(69 + 12 * math.log2(f / 440))


def vlq(v):
    out = [v & 0x7F]
    v >>= 7
    while v:
        out.insert(0, 0x80 | (v & 0x7F))
        v >>= 7
    return bytes(out)


def track(events):
    data, last = b'', 0
    for t, ev in sorted(events, key=lambda e: (e[0], e[1][0] & 0xF0 != 0x80)):
        data += vlq(t - last) + bytes(ev)
        last = t
    data += b'\x00\xff\x2f\x00'
    return b'MTrk' + struct.pack('>I', len(data)) + data


def convert(frames):
    regs = [bytes.fromhex(f) for f in frames]
    tracks, notes_total = [], 0
    for ch in range(3):
        ev, cur, cur_vol, cnt = [], None, 0, 0
        ev.append((0, [0xC0 | ch, 80]))                     # programa: square lead
        for i, r in enumerate(regs):
            period = r[2 * ch] | (r[2 * ch + 1] & 0x0F) << 8
            tone_on = not (r[7] >> ch & 1)
            vol = r[8 + ch] & 0x0F if not r[8 + ch] & 0x10 else 12
            n = note_of(period) if tone_on and vol else None
            t = round(i * TICKS_PER_FRAME)
            if n is not None and not (0 <= n <= 127):
                n = None
            if cur is not None and (n != cur or vol > cur_vol):
                ev.append((t, [0x80 | ch, cur, 0])); cur = None
            if n is not None and cur is None:
                ev.append((t, [0x90 | ch, n, 40 + vol * 5])); cur, cnt = n, cnt + 1
            cur_vol = vol if n is not None else 0
        if cur is not None:
            ev.append((round(len(regs) * TICKS_PER_FRAME), [0x80 | ch, cur, 0]))
        tracks.append(track(ev)); notes_total += cnt
    # ruido -> percusion (canal 10, nota 38 caja / 42 charles segun el periodo)
    ev, on = [], False
    for i, r in enumerate(regs):
        noisy = any(not (r[7] >> (3 + ch) & 1) and (r[8 + ch] & 0x1F) for ch in range(3))
        t = round(i * TICKS_PER_FRAME)
        if noisy and not on:
            ev.append((t, [0x99, 38 if (r[6] & 0x1F) > 8 else 42, 90])); on = True
        elif not noisy and on:
            ev.append((t, [0x89, 38 if (r[6] & 0x1F) > 8 else 42, 0])); on = False
    tracks.append(track(ev))
    tempo = track([(0, [0xFF, 0x51, 0x03, *struct.pack('>I', round(60_000_000 / BPM))[1:]])])
    hdr = b'MThd' + struct.pack('>IHHH', 6, 1, len(tracks) + 1, TPQ)
    return hdr + tempo + b''.join(tracks), notes_total


def main():
    src = ROOT / 'logs' / 'omsx' / 'music'
    out = ROOT / 'extracted' / 'music'
    out.mkdir(parents=True, exist_ok=True)
    index = {}
    for f in sorted(src.glob('*.json.gz')):
        d = json.load(gzip.open(f, 'rt'))
        frames = d['frames']
        s, p = find_loop(frames)
        use = frames[:s + p] if p else frames
        mid, n = convert(use)
        name = f'{d["song"]:02x}'
        (out / f'{name}.mid').write_bytes(mid)
        index[f'{d["song"]:#04x}'] = {'frames_recorded': len(frames), 'intro_s': round(s / 60, 2) if p else None,
                                      'loop_s': round(p / 60, 2) if p else None, 'length_s': round(len(use) / 60, 2),
                                      'notes': n, 'midi': f'{name}.mid'}
        print(name, index[f'{d["song"]:#04x}'])
    (out / 'index.json').write_text(json.dumps(index, indent=1))


if __name__ == '__main__':
    main()
