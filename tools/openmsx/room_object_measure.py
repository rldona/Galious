#!/usr/bin/env python3
"""Mide en openMSX los objetos de sala (puertas giratorias, muros que se cierran, paredes que se rompen).

  room_object_measure.py
Registra por frame x, y, E500, E503, unos bytes del estado del objeto y los tiles de una ventana del mapa de
nombres en RAM (ED00: fila r de la sala en ED00 + (r + 4) * 32 + columna).
"""
import json, re, sys
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run
from scan_rooms import goto_room, peek

KEYS = {'space': 0x01, 'left': 0x10, 'up': 0x20, 'down': 0x40, 'right': 0x80}


def trace(presses, frames, obj=None, nobj=8, cells=()):
    body = ''.join(f'if {{$f == {a}}} {{keymatrixdown 8 {sum(KEYS[k] for k in keys.split(","))}}}; '
                   f'if {{$f == {b}}} {{keymatrixup 8 {sum(KEYS[k] for k in keys.split(","))}}}; ' for a, b, keys in presses)
    send('proc ro_keys {f} {' + body + '}')
    parts = ['[peek 0xE507]', '[peek 0xE505]', '[peek 0xE500]', '[peek 0xE503]']
    if obj:
        parts.append(f'[binary encode hex [debug read_block memory {obj} {nobj}]]')
    if cells:
        parts += [f'[format %02x [peek {0xED00 + (r + 4) * 32 + c}]]' for r, c in cells]
    send('proc ro_tick {} { ro_keys [llength $::ro_log]; lappend ::ro_log "' + ' '.join(parts) + '"; incr ::ro_left -1; if {$::ro_left > 0} { after frame ro_tick } }')
    send(f'set ::ro_log {{}}; set ::ro_left {frames}; after frame ro_tick')
    run(frames / 60 + 0.4)
    send('keymatrixup 8 0xFF')
    return [r.split() for r in re.findall(r'\{([^}]*)\}', send('set ::ro_log')[1])]


def changes(log):
    out = []
    for i, r in enumerate(log):
        if i == 0 or r[2:] != log[i - 1][2:]:
            out.append((i, ' '.join(r)))
    return out


def setup(state, room, x, y, char=1, facing=None, pokes=()):
    goto_room(state, room)
    send(f'poke 0xE501 {char}')
    for a, v in pokes:
        send(f'poke {a} {v}')
    send(f'poke 0xE507 {x}'); send(f'poke 0xE505 {y}')
    if facing is not None:
        send(f'poke 0xE503 {facing}')
    run(0.2)


def report(name, log, limit=60):
    print(name)
    for i, r in changes(log)[:limit]:
        print(f'  {i:4d} {r}')
    return log


if __name__ == '__main__':
    out = {}
    which = sys.argv[1:] or ['door3', 'door4', 'wall5', 'wall7']
    if 'door3' in which:
        # Mundo 2, sala 6: puerta de un solo sentido 0x03 en (120, 64); jugador a y = 64 + 0x18.
        cells = [(r, 15) for r in range(4, 7)]
        setup('world02_entry', 6, 136, 88, 1, 1)
        out['door3_left'] = report('clase 3, empujando a la izquierda', trace([(2, 150, 'left')], 160, 0xEA50, 8, cells))
        setup('world02_entry', 6, 112, 88, 1, 0)
        out['door3_right'] = report('clase 3, empujando a la derecha', trace([(2, 100, 'right')], 110, 0xEA50, 8, cells))
    if 'door4' in which:
        # Mundo 1, sala 7: puertas giratorias (clase 4) en (104, 64) y (128, 128).
        cells = [(r, 13) for r in range(4, 7)]
        for char, name in ((1, 'popolon'), (0, 'aphrodite')):
            setup('world01_entry', 7, 96, 88, char, 0)
            out[f'door4_{name}_right'] = report(f'clase 4, {name}, empujando a la derecha', trace([(2, 150, 'right')], 160, 0xEA60, 16, cells))
            setup('world01_entry', 7, 120, 88, char, 1)
            out[f'door4_{name}_left'] = report(f'clase 4, {name}, empujando a la izquierda', trace([(2, 150, 'left')], 160, 0xEA60, 16, cells))
    if 'wall5' in which:
        # Castillo, sala 48: muro que se cierra (clase 5) en (160, 64).
        cells = [(r, 20) for r in range(3, 9)]
        setup('galious_start', 48, 136, 88, 1, 0)
        out['wall5_right'] = report('clase 5, andando a la derecha', trace([(2, 22, 'right')], 200, 0xEAA0, 8, cells))
    if 'wall7' in which:
        # Castillo, sala 97: pared que se rompe (0x37, 3 golpes) en (128, 96).
        cells = [(r, c) for r in range(8, 11) for c in range(16, 20)]
        setup('galious_start', 97, 118, 112, 1, 0)
        presses = [(2 + 12 * k, 6 + 12 * k, 'space') for k in range(6)]
        out['wall7_popolon'] = report('clase 7, Popolon, 6 espadazos', trace(presses, 90, 0xEA20, 8, cells))
        setup('galious_start', 97, 118, 112, 0, 0)
        out['wall7_aphrodite'] = report('clase 7, Aphrodite sin Pendant, 6 espadazos', trace(presses, 90, 0xEA20, 8, cells))
    json.dump(out, open('logs/omsx/room_object_measure.json', 'w'))
    send('god')
