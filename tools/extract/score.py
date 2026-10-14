#!/usr/bin/env python3
"""Decodifica las partituras de la musica (driver en b13:6197-6446) y las compara con el PSG grabado.

Formato de un canal (bytes desde el puntero de b13:743E + 6*(id & 7F) + 2*canal):
  00-BF  nota: nibble alto = nota (0 = C ... 11 = B), nibble bajo + 1 = duracion en unidades
  C0-CF  silencio de (n+1) unidades
  Dn     tempo: unidad = n frames
  E0-E5  octava: periodo = tabla[nota] >> (6 - n)        (tabla de periodos b13:62D0, 12 palabras)
  E6/EB  bandera E1C3 = 1 / 0                            E7  periodo + 1 (desafinado)
  E8 xx  modo guion: cada nota ejecuta el guion de la tabla del instrumento xx (1-5: b13:65A7 + 0x18*(xx-1);
         otro: b13:661F), una entrada por nota; el guion fija periodo, volumen y ruido por pasos
  E9     modo tono (nota normal)                         EA aa aa  salto
  EC xx  vibrato                                         ED aa aa  llamada a subrutina / EE retorno
  Fn xx  volumen n+1 y parametros de caida               FE n aa aa  repetir n veces / FF fin
Guion (mismo interprete que b13:64B0): FF fin; FE n aa aa bucle; 2x d [r12 r11]: mezclador (bit 0 tono,
bit 1 tono, bit 0 ruido) y d frames por paso; 1n xx: periodo de ruido 2n y despues un paso; vp pp: volumen v,
periodo de tono p (12 bits).

Salida: extracted/music/score_<id>.mid (duraciones exactas: 1 frame = 20 ticks a 150 bpm) y
score.json con, por cancion y canal, la lista de eventos; imprime la comparacion con el PSG.
"""
import gzip, json, math, struct
from galious_rom import Rom, ROOT
from psg_to_midi import track, TPQ, BPM, TICKS_PER_FRAME

G = 4                     # grupo de bancos 13-15: banco 13 en 6000, 14 en 8000
SONGS, PERIODS = 0x743E, 0x62D0
SCRIPT_TABLES = {1: 0x65A7, 2: 0x65BF, 3: 0x65D7, 4: 0x65EF, 5: 0x6607}
DEFAULT_SCRIPTS = 0x661F
NAMES = ['C', 'C#', 'D', 'D#', 'E', 'F', 'F#', 'G', 'G#', 'A', 'A#', 'B']
CLOCK = 3579545 / 2


def midi_of(period):
    return round(69 + 12 * math.log2(CLOCK / (16 * period) / 440)) if period else None


class Score:
    def __init__(self, rom):
        self.rom = rom
        self.table = [rom.word(PERIODS + 2 * i, G) for i in range(12)]

    def b(self, a):
        return self.rom.byte(a, G)

    def w(self, a):
        return self.rom.word(a, G)

    def script(self, ptr, max_steps=64):
        """Pasos (frames, volumen, periodo, ruido) de un guion de instrumento."""
        steps, mode, dur, loops, guard = [], 1, 1, {}, 0
        noise = None
        while guard < 400 and len(steps) < max_steps:
            guard += 1
            c = self.b(ptr); ptr += 1
            if c == 0xFF:
                break
            if c == 0xFE:
                n = loops.get(ptr, self.b(ptr))
                if n > 1:
                    loops[ptr] = n - 1; ptr = self.w(ptr + 1)
                else:
                    loops.pop(ptr, None); ptr += 3
                continue
            hi = c & 0xF0
            if hi == 0x20:
                mode = c & 3; dur = self.b(ptr); ptr += 1
                if c >= 0x28:
                    ptr += 2
                continue
            if hi == 0x10:
                noise = (c & 0x0F) * 2
                c = self.b(ptr); ptr += 1
            vol, per = c >> 4, (c & 0x0F) << 8 | self.b(ptr); ptr += 1
            steps.append((dur, vol, per if mode & 2 else None, noise if mode & 1 else None))
        return steps

    def channel(self, ptr, max_frames=60 * 600):
        """Eventos (frame, duracion, nota MIDI o None, info) de un canal, con una vuelta del bucle."""
        ev, t = [], 0
        tempo, shift, detune, script_mode, inst, vol = 1, 0, 0, False, 0, 15
        stack, loops = [], {}
        guard, seen = 0, {}
        while t < max_frames and guard < 20000:
            guard += 1
            seen.setdefault(ptr, t)            # primer instante en que se lee cada byte (para el bucle)
            c = self.b(ptr); ptr += 1
            if c < 0xC0:
                dur = tempo * ((c & 0x0F) + 1)
                note = c >> 4
                if script_mode:
                    tab = SCRIPT_TABLES.get(inst, DEFAULT_SCRIPTS)
                    steps = self.script(self.w(tab + 2 * note), max_steps=400)
                    # expande el guion en subnotas (tramos con el mismo periodo y volumen > 0), recortado a dur
                    k, cur = 0, None
                    for sd, sv, sp, sn in steps:
                        for _ in range(sd):
                            if k >= dur:
                                break
                            p = sp if sv else None
                            if cur and cur[2] == p and p is not None:
                                cur[1] += 1
                            else:
                                if cur and cur[2] is not None:
                                    ev.append((t + cur[0], cur[1], midi_of(cur[2]), {'guion': inst, 'nota': note,
                                                                                    'periodo': cur[2], 'vol': vol}))
                                elif cur and cur[3]:
                                    ev.append((t + cur[0], cur[1], 38, {'guion': inst, 'ruido': True, 'vol': vol}))
                                cur = [k, 1, p, sn is not None and bool(sv)]
                            k += 1
                    if cur and cur[2] is not None:
                        ev.append((t + cur[0], cur[1], midi_of(cur[2]), {'guion': inst, 'nota': note,
                                                                        'periodo': cur[2], 'vol': vol}))
                    elif cur and cur[3]:
                        ev.append((t + cur[0], cur[1], 38, {'guion': inst, 'ruido': True, 'vol': vol}))
                elif note < 12:
                    per = (self.table[note] >> shift) + detune
                    ev.append((t, dur, midi_of(per), {'periodo': per, 'vol': vol}))
                t += dur
            elif c < 0xD0:
                t += tempo * ((c & 0x0F) + 1)
            elif c < 0xE0:
                tempo = c & 0x0F
            elif c < 0xF0:
                n = c & 0x0F
                if n < 6:
                    shift = 6 - n
                elif n == 7:
                    detune = 1
                elif n == 8:
                    script_mode, inst = True, self.b(ptr); ptr += 1
                elif n == 9:
                    script_mode = False
                elif n == 0x0A:
                    dest = self.w(ptr)          # salto final: la cancion vuelve a dest en bucle
                    ev.append((t, 0, None, {'bucle_a': f'{dest:04X}', 'bucle_desde_frame': seen.get(dest)}))
                    break
                elif n == 0x0C:
                    ptr += 1
                elif n == 0x0D:
                    stack.append(ptr + 2); ptr = self.w(ptr)
                elif n == 0x0E:
                    ptr = stack.pop() if stack else ptr
            else:
                n = c & 0x0F
                if n == 0x0F:
                    break
                if n == 0x0E:
                    k = loops.get(ptr, self.b(ptr))
                    if k > 1:
                        loops[ptr] = k - 1; ptr = self.w(ptr + 1)
                    else:
                        loops.pop(ptr, None); ptr += 3
                else:
                    vol = n + 1; ptr += 1
        return ev


def to_midi(channels):
    tracks = []
    for ch, ev in enumerate(channels):
        out = [(0, [0xC0 | ch, 80])]
        for t, dur, m, info in ev:
            if m is None or not dur or not 0 <= m <= 127:
                continue
            k = 9 if info.get('ruido') and not info.get('periodo') else ch
            out.append((round(t * TICKS_PER_FRAME), [0x90 | k, m, 40 + 5 * min(15, info.get('vol', 12))]))
            out.append((round((t + dur) * TICKS_PER_FRAME) - 1, [0x80 | k, m, 0]))
        tracks.append(track(out))
    tempo = track([(0, [0xFF, 0x51, 0x03, *struct.pack('>I', round(60_000_000 / BPM))[1:]])])
    return b'MThd' + struct.pack('>IHHH', 6, 1, len(tracks) + 1, TPQ) + tempo + b''.join(tracks)


def compare(song, channels):
    """Fraccion de frames con nota en que la nota de la partitura coincide con la del PSG grabado."""
    f = ROOT / 'logs' / 'omsx' / 'music' / f'{song:02x}.json.gz'
    if not f.exists():
        return None
    regs = [bytes.fromhex(x) for x in json.load(gzip.open(f, 'rt'))['frames']]
    # la grabacion empieza unos frames despues de la peticion: busca el desfase que mejor encaja
    res = []
    for ch, ev in enumerate(channels):
        want = {}
        for t, dur, m, info in ev:
            if m is not None and info.get('periodo'):
                for k in range(t, t + dur):
                    want[k] = m
        if not want:
            res.append(None); continue
        best = 0
        for off in range(-6, 7):
            ok = tot = 0
            for k, m in want.items():
                i = k - off
                if 0 <= i < len(regs):
                    r = regs[i]
                    per = r[2 * ch] | (r[2 * ch + 1] & 0x0F) << 8
                    if r[8 + ch] & 0x1F:
                        tot += 1; ok += midi_of(per) == m if per else 0
            if tot and ok / tot > best:
                best = ok / tot
        res.append(round(best, 3))
    return res


def main():
    rom = Rom()
    sc = Score(rom)
    out = ROOT / 'extracted' / 'music'
    out.mkdir(parents=True, exist_ok=True)
    allsongs = {}
    for song in range(0x81, 0x96):
        a = SONGS + 6 * (song & 0x7F)
        ptrs = [rom.word(a + 2 * k, G) for k in range(3)]
        chans = [sc.channel(p) for p in ptrs]
        (out / f'score_{song:02x}.mid').write_bytes(to_midi(chans))
        allsongs[f'{song:#04x}'] = [[[t, d, m, info] for t, d, m, info in ev] for ev in chans]
        n = [sum(1 for e in ev if e[2] is not None) for ev in chans]
        loops = []
        for ev in chans:
            lp = [e for e in ev if 'bucle_a' in e[3]]
            loops.append(f"{lp[0][0] / 60:.1f}s->{lp[0][3]['bucle_desde_frame'] / 60:.1f}s" if lp and lp[0][3]['bucle_desde_frame'] is not None else 'fin')
        print(f'{song:#04x} notas {n}, canales: {loops}; misma nota que el PSG: {compare(song, chans)}')
    (out / 'score.json').write_text(json.dumps(allsongs))


if __name__ == '__main__':
    main()
