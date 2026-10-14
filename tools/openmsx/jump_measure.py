#!/usr/bin/env python3
"""Mide en openMSX la fisica del jugador (salto, caida, aire) frame a frame, en la sala 1 del castillo.

  jump_measure.py [salida.json]
Cada prueba coloca al jugador, pulsa unas teclas un tiempo y registra y, x, E500 y E50E en cada frame
(player_trace.tcl). La sala 1 tiene el suelo en la fila 11 (y = 120) y aire libre encima.
"""
import json, re, sys
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run
from scan_rooms import goto_room, peek

KEYS = {'space': 0x01, 'left': 0x10, 'up': 0x20, 'down': 0x40, 'right': 0x80}


def trial(char, x, y, presses, frames=120):
    """presses: lista de (frame_inicio, frame_fin, teclas)."""
    goto_room('galious_start', 1)
    send(f'poke 0xE501 {char}')
    run(0.3)
    send(f'poke 0xE507 {x}'); send(f'poke 0xE505 {y}')
    run(0.2)
    send('source tools/openmsx/player_trace.tcl')
    # programa pulsaciones por frame dentro del propio registro
    script = 'proc pt_keys {f} {' + ''.join(
        f'if {{$f == {a}}} {{keymatrixdown 8 {sum(KEYS[k] for k in keys.split(","))}}}; '
        f'if {{$f == {b}}} {{keymatrixup 8 {sum(KEYS[k] for k in keys.split(","))}}}; ' for a, b, keys in presses) + '}'
    send(script)
    send('proc pt_tick {} { pt_keys [llength $::pt_log]; lappend ::pt_log "[peek 0xE505] [peek 0xE507] [peek 0xE500] [peek 0xE50E]"; '
         'incr ::pt_left -1; if {$::pt_left > 0} { after frame pt_tick } }')
    send(f'pt_start {frames}')
    run(frames / 60 + 0.5)
    send('keymatrixup 8 0xFF')
    rows = re.findall(r'\{([^}]*)\}', send('set ::pt_log')[1])
    return [list(map(int, r.split())) for r in rows]


TESTS = {
    'popolon_up_hold': (1, 64, 120, [(2, 60, 'up')]),
    'popolon_up_tap4': (1, 64, 120, [(2, 6, 'up')]),
    'popolon_up_tap10': (1, 64, 120, [(2, 12, 'up')]),
    'popolon_up_tap20': (1, 64, 120, [(2, 22, 'up')]),
    'aphrodite_up_hold': (0, 64, 120, [(2, 60, 'up')]),
    'aphrodite_up_tap4': (0, 64, 120, [(2, 6, 'up')]),
    'popolon_up_right': (1, 64, 120, [(2, 60, 'up'), (2, 100, 'right')]),
    'popolon_fall': (1, 64, 40, []),
    'popolon_walk': (1, 64, 120, [(2, 100, 'right')]),
}

if __name__ == '__main__':
    out = {}
    for name, (char, x, y, presses) in TESTS.items():
        log = trial(char, x, y, presses)
        out[name] = log
        ys = [r[0] for r in log]
        print(f'{name}: y min {min(ys)} (sube {log[0][0] - min(ys)} px), frames {len(log)}, '
              f'x {log[0][1]}->{log[-1][1]}, E500 {sorted(set(r[2] for r in log))}', flush=True)
    json.dump(out, open(sys.argv[1] if len(sys.argv) > 1 else 'logs/omsx/jump_measure.json', 'w'))
    send('god')
