#!/usr/bin/env python3
"""Mide en openMSX las puertas con llave (objetos de sala de clase 1 y 2).

  keydoor_measure.py
Con 2 llaves (E04A, BCD), empuja contra la puerta desde un lado y luego desde el otro. Registra por frame
x, y, E500, llaves (E04A) y el estado de la puerta (+1 fase, +4 alto, +7 temporizador).
"""
import json, re, sys
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run
from scan_rooms import goto_room, peek

KEYS = {'left': 0x10, 'right': 0x80}


def push(slot, x, y, facing, key, frames=200):
    send(f'poke 0xE507 {x}'); send(f'poke 0xE505 {y}'); send(f'poke 0xE503 {0 if key == "right" else 1}')
    run(0.1)
    f = f'[peek 0xE507] [peek 0xE505] [peek 0xE500] [peek 0xE04A] [peek {slot + 1}] [peek {slot + 4}] [peek {slot + 7}]'
    send('proc kd_tick {} { if {[llength $::kd_log] == 2} { keymatrixdown 8 ' + str(KEYS[key]) + ' }; lappend ::kd_log "' + f + '"; incr ::kd_left -1; if {$::kd_left > 0} { after frame kd_tick } }')
    send(f'set ::kd_log {{}}; set ::kd_left {frames}; after frame kd_tick')
    run(frames / 60 + 0.4)
    send('keymatrixup 8 0xFF')
    return [list(map(int, r.split())) for r in re.findall(r'\{([^}]*)\}', send('set ::kd_log')[1])]


def summary(log):
    out = []
    for i, r in enumerate(log):
        if i == 0 or r[2:] != log[i - 1][2:]:
            out.append((i, *r))
    return out


def trial(state, room, door, slot):
    goto_room(state, room)
    send('poke 0xE04A 2')
    y = door['y'] + 0x18
    res = {}
    res['from_left'] = push(slot, door['x'] - 8, y, 0, 'right')
    res['back_from_right'] = push(slot, door['x'] + 16, y, 1, 'left')
    for k, log in res.items():
        print(f'  {k}:')
        for row in summary(log):
            print('   frame %3d x=%3d y=%3d E500=%d llaves=%02x fase=%d alto=%d timer=%d' % row)
    return res


if __name__ == '__main__':
    out = {}
    print('castillo, sala 16 (clase 1, EA00):')
    out['castle16'] = trial('galious_start', 16, {'x': 160, 'y': 96}, 0xEA00)
    print('mundo 1, sala 4 (clase 2, EA30):')
    out['world01_4'] = trial('world01_entry', 4, {'x': 224, 'y': 160}, 0xEA30)
    json.dump(out, open('logs/omsx/keydoor_measure.json', 'w'))
    send('god')
