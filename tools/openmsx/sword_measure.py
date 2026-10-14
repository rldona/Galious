#!/usr/bin/env python3
"""Mide la espada en openMSX: duracion del golpe (E508) y alcance contra una piedra.

  sword_measure.py
1. Sala 1 del castillo: registro por frame de y, x, E500, E508 con un toque de espacio, espacio mantenido,
   espacio andando y espacio en el aire.
2. Sala 2 del castillo, piedra de abajo a la derecha: un golpe desde varias distancias, mirando cuanta vida
   le quita (byte +6 de su entrada en EC30).
"""
import re, sys, json
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run
from scan_rooms import goto_room, peek, ram

KEYS = {'space': 0x01, 'left': 0x10, 'up': 0x20, 'down': 0x40, 'right': 0x80}
LOG = 'proc sw_tick {} { sw_keys [llength $::sw_log]; lappend ::sw_log "[peek 0xE505] [peek 0xE507] [peek 0xE500] [peek 0xE508] [peek 0xE503]"; incr ::sw_left -1; if {$::sw_left > 0} { after frame sw_tick } }'


def record(presses, frames=60):
    body = ''.join(f'if {{$f == {a}}} {{keymatrixdown 8 {sum(KEYS[k] for k in keys.split(","))}}}; '
                   f'if {{$f == {b}}} {{keymatrixup 8 {sum(KEYS[k] for k in keys.split(","))}}}; ' for a, b, keys in presses)
    send('proc sw_keys {f} {' + body + '}')
    send(LOG)
    send(f'set ::sw_log {{}}; set ::sw_left {frames}; after frame sw_tick')
    run(frames / 60 + 0.4)
    send('keymatrixup 8 0xFF')
    return [list(map(int, r.split())) for r in re.findall(r'\{([^}]*)\}', send('set ::sw_log')[1])]


def timing():
    out = {}
    for name, presses in {
        'tap': [(2, 3, 'space')],
        'hold': [(2, 50, 'space')],
        'walk': [(0, 59, 'right'), (10, 11, 'space')],
        'jump': [(2, 40, 'up'), (16, 17, 'space')],
        'double': [(2, 3, 'space'), (8, 9, 'space'), (20, 21, 'space')],
    }.items():
        goto_room('galious_start', 1)
        send('poke 0xE507 96'); send('poke 0xE505 120'); run(0.2)
        log = record(presses)
        out[name] = log
        print(name, 'E508:', ''.join('%x' % min(r[3], 15) for r in log))
        print('      x:', ''.join(str(log[i][1] - log[i - 1][1]) for i in range(1, len(log))),
              'dy:', ''.join('*' if log[i][0] != log[i - 1][0] else '.' for i in range(1, len(log))))
    return out


def reach():
    res = []
    for dist in range(10, 34, 2):
        goto_room('galious_start', 2)
        rocks = bytes.fromhex(ram(0xEC30, 24))
        r = next(rocks[i:i + 8] for i in range(0, 24, 8) if rocks[i] and rocks[i + 3] > 128)
        idx = next(i for i in range(0, 24, 8) if rocks[i:i + 8] == r)
        ry, rx = r[2], r[3]
        y, x = ry + 16, rx - dist           # a la izquierda de la piedra, mirando a la derecha
        send(f'poke 0xE505 {y}'); send(f'poke 0xE507 {x}'); send('poke 0xE503 0'); run(0.1)
        hp0 = peek(0xEC30 + idx + 6)
        record([(1, 2, 'space')], 30)
        hp1 = peek(0xEC30 + idx + 6)
        res.append((dist, x, y, rx, ry, hp0, hp1))
        print(f'distancia {dist}: jugador x={x} (piedra x={rx}, y={ry}) vida {hp0}->{hp1}', flush=True)
    return res


if __name__ == '__main__':
    data = {'timing': timing(), 'reach': reach()}
    json.dump(data, open('logs/omsx/sword_measure.json', 'w'))
    send('god')
