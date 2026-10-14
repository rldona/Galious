#!/usr/bin/env python3
"""Mide las armas secundarias (tecla M) en openMSX. Ver docs/player.md, "Armas secundarias".

  weapon_measure.py
Necesita el renderer activo (set renderer SDLGL-PP) para que corran los 'after frame'. Da las armas forzando
E06F + tipo, elige con E510 y pone 99 flechas (E046). M es la fila 4, bit 2 de la matriz.
1. Castillo, sala 1 (suelo y = 120): un disparo de cada arma, limites con pulsaciones repetidas
   (Popolon / Aphrodite, Earrings E08C, Bracelet E08D), sin flechas, en el aire y en la escalera.
2. Sala 3 (plataforma suelta, filas 11, x 96-159): fuego rodante y fuego a cada lado.
3. Mina: armado, explosion y dano al jugador.
4. Sala 104, esqueleto (0x25, vida 8) congelado con E0D5: dano de cada arma, Saber (E094), ceramica y mina.
"""
import os, sys, re, json

sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run
from scan_rooms import goto_room, peek, ram

ROWS = {'space': (8, 0x01), 'left': (8, 0x10), 'up': (8, 0x20), 'down': (8, 0x40), 'right': (8, 0x80), 'm': (4, 0x04)}
send('proc wm_tick {} { wm_keys [llength $::wm_log]; lappend ::wm_log "[binary encode hex [debug read_block memory 0xE500 0x100]][binary encode hex [debug read_block memory 0xE040 0x20]]"; incr ::wm_left -1; if {$::wm_left > 0} { after frame wm_tick } }')

def record(presses, frames=120):
    body = ''
    for a, b, keys in presses:
        for k in keys.split(','):
            r, m = ROWS[k]
            body += f'if {{$f == {a}}} {{keymatrixdown {r} {m}}}; if {{$f == {b}}} {{keymatrixup {r} {m}}}; '
    send('proc wm_keys {f} {' + body + '}')
    send(f'set ::wm_log {{}}; set ::wm_left {frames}; after frame wm_tick')
    run(frames / 60 + 0.4)
    send('keymatrixup 8 0xFF'); send('keymatrixup 4 0xFF')
    out = []
    for h in re.findall(r'[0-9a-f]{576}', send('set ::wm_log')[1]):
        b = bytes.fromhex(h)
        out.append(b)
    return out

def setup(room=1, weapon=1, y=120, x=96, face=0, popolon=None, extra=()):
    goto_room('galious_start', room)
    for t in range(1, 7): send(f'poke {0xE06F + t} 1')
    send(f'poke 0xE510 {weapon}'); send('poke 0xE046 0x99'); send('poke 0xE047 0x00')
    if popolon is not None: send(f'poke 0xE501 {popolon}')
    send(f'poke 0xE505 {y}'); send(f'poke 0xE507 {x}'); send(f'poke 0xE503 {face}')
    for a, v in extra: send(f'poke {a} {v}')
    run(0.1)

def show(log, slots=range(6), every=1):
    prev = None
    for i, b in enumerate(log):
        pl = b[0x05], b[0x07], b[0x00], b[0x03]
        ammo = b[0x106] | b[0x107] << 8
        s = []
        for k in slots:
            o = 0x40 + 0x20 * k
            if b[o]:
                s.append(f'[{k}:t{b[o]} s{b[o+1]} f{b[o+2]:02x} y{b[o+5]} x{b[o+7]} vy{b[o+0xd]:02x} vx{b[o+0xf]:02x} c{b[o+8]:02x} 1a{b[o+0x1a]}]')
        line = f'{i:3d} v{b[0x116]}/{b[0x112]} inv{b[0x18]} p y{pl[0]} x{pl[1]} a{pl[2]} f{pl[3]} ammo{ammo:04x} n{b[0x1c]}/{b[0x1d]} ' + ' '.join(s)
        if line[4:] != prev:
            print(line)
        prev = line[4:]
def esetup(weapon, dist=60, extra=()):
    goto_room('galious_start', 104)
    send('proc frz {} { poke 0xE0D5 255; set ::frz_id [after frame frz] }'); send('frz'); run(0.2)
    slots = [bytes.fromhex(ram(0xE800 + 0x20 * i, 0x20)) for i in range(16)]
    idx = next(i for i, s in enumerate(slots) if s[0] == 0x25)
    base = 0xE800 + 0x20 * idx
    ex, ey = peek(base + 7), peek(base + 5)
    for t in range(1, 7): send(f'poke {0xE06F + t} 1')
    send(f'poke 0xE510 {weapon}'); send('poke 0xE046 0x99'); send('poke 0xE047 0')
    send(f'poke 0xE507 {ex - dist}'); send(f'poke 0xE505 {ey + 16}'); send('poke 0xE503 0'); send('poke 0xE500 0')
    for a, v in extra: send(f'poke {a} {v}')
    run(0.1)
    return base, ex, ey
def hpwatch(base, frames):
    send(f'set ::hp {{}}; proc hpw {{n}} {{ lappend ::hp "[peek {base+0x10}] [peek {base}] [peek 0xE604]"; if {{$n > 0}} {{ after frame "hpw [expr $n-1]" }} }}; hpw {frames}')

def hits(base, presses, frames):
    hpwatch(base, frames)
    log = record(presses, frames)
    seq = re.findall(r'\{(\d+) (\d+) (\d+)\}', send('set ::hp')[1])
    return [(i, int(h)) for i, (h, t, c) in enumerate(seq) if i == 0 or seq[i - 1][0] != h], log


if __name__ == '__main__':
    send('set renderer SDLGL-PP'); send('set throttle off')
    taps = [(2 + 6 * i, 5 + 6 * i, 'm') for i in range(6)]
    for w in range(1, 6):
        print('=== sala 1, arma', w); setup(weapon=w, x=60); show(record([(2, 5, 'm')], 24))
    for who in (1, 0):
        for w in (1, 3, 5):
            setup(weapon=w, popolon=who, x=40)
            print('=== 6 pulsaciones, arma', w, 'Popolon' if who else 'Aphrodite'); show([record(taps, 40)[39]])
    for w, item in ((1, 0xE08C), (5, 0xE08D)):
        setup(weapon=w, x=40, popolon=1, extra=[(item, 1)])
        print('=== Popolon con', hex(item), 'arma', w); show([record(taps, 40)[39]])
    print('=== sin flechas'); setup(weapon=1, x=40, extra=[(0xE046, 0)]); show(record([(2, 6, 'm')], 12))
    for w in (1, 3, 4, 5):
        print('=== saltando, arma', w); setup(weapon=w, x=40); show([record([(2, 30, 'up'), (14, 17, 'm')], 22)[21]])
        print('=== escalera, arma', w); setup(weapon=w, x=120); show([record([(2, 20, 'up'), (24, 27, 'm')], 32)[31]])
    for w in (3, 4):
        for face in (0, 1):
            print('=== sala 3, plataforma, arma', w, 'mirando', face)
            setup(room=3, weapon=w, y=88, x=128, face=face); show(record([(2, 5, 'm')], 130), slots=[0])
    print('=== mina: poner, alejarse y volver'); setup(weapon=5, x=60, popolon=1)
    show(record([(2, 5, 'm'), (10, 40, 'right'), (60, 100, 'left')], 220), slots=[3])
    for w, extra, name in ((1, (), 'flecha'), (1, ((0xE094, 1),), 'flecha+Saber'), (2, (), 'ceramica'), (3, (), 'fuego rodante')):
        base, ex, ey = esetup(w, extra=extra)
        print(name, 'vida (frame, vida):', hits(base, [(2, 5, 'm')], 150)[0]); send('after cancel $::frz_id')
    base, ex, ey = esetup(4, dist=40)
    print('fuego', 'vida (frame, vida):', hits(base, [(2, 5, 'm')], 150)[0]); send('after cancel $::frz_id')
    for extra, name in (((), 'mina'), (((0xE094, 1),), 'mina+Saber')):
        base, ex, ey = esetup(5, dist=40, extra=extra)
        record([(2, 5, 'm')], 10)
        send('poke 0xE507 200'); run(0.1)                       # el jugador se aleja: la mina se arma
        send(f'poke 0xE5A7 {ex}'); send(f'poke 0xE5A5 {ey + 12}')   # y se lleva sobre el esqueleto
        print(name, 'vida (frame, vida):', hits(base, [], 200)[0]); send('after cancel $::frz_id')
