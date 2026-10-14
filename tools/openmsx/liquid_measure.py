#!/usr/bin/env python3
"""Mide en openMSX el agua y la lava frame a frame: hundimiento, movimiento, dano y muerte.

  liquid_measure.py
Agua: castillo, sala 137 (agua en las filas 12-18, suelo en la 19). Lava: mundo 8, sala 16 (filas 12-19).
Registra y, x, E500, E511, E524, vitalidad de Popolon (E056) y de Aphrodite (E052) y E000. Sin 'god'.
"""
import json, re, sys
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run
from scan_rooms import goto_room, peek

KEYS = {'space': 0x01, 'left': 0x10, 'up': 0x20, 'down': 0x40, 'right': 0x80}
FIELDS = '[peek 0xE505] [peek 0xE507] [peek 0xE500] [peek 0xE511] [peek 0xE524] [peek 0xE056] [peek 0xE052] [peek 0xE000]'


def trial(state, room, char, x, y, presses, frames):
    goto_room(state, room)
    send('catch {after cancel $::god_id}')
    send(f'poke 0xE501 {char}'); send('poke 0xE056 32'); send('poke 0xE052 32')
    send(f'poke 0xE507 {x}'); send(f'poke 0xE505 {y}')
    body = ''.join(f'if {{$f == {a}}} {{keymatrixdown 8 {sum(KEYS[k] for k in keys.split(","))}}}; '
                   f'if {{$f == {b}}} {{keymatrixup 8 {sum(KEYS[k] for k in keys.split(","))}}}; ' for a, b, keys in presses)
    send('proc lq_keys {f} {' + body + '}')
    send('proc lq_tick {} { lq_keys [llength $::lq_log]; lappend ::lq_log "' + FIELDS + '"; incr ::lq_left -1; if {$::lq_left > 0} { after frame lq_tick } }')
    send(f'set ::lq_log {{}}; set ::lq_left {frames}; after frame lq_tick')
    run(frames / 60 + 0.5)
    send('keymatrixup 8 0xFF')
    return [list(map(int, r.split())) for r in re.findall(r'\{([^}]*)\}', send('set ::lq_log')[1])]


TESTS = {
    'water_popolon_drop': ('galious_start', 137, 1, 80, 104, [], 240),
    'water_aphrodite_drop': ('galious_start', 137, 0, 80, 104, [], 720),
    'water_aphrodite_walk': ('galious_start', 137, 0, 80, 104, [(90, 200, 'right')], 240),
    'water_aphrodite_jump': ('galious_start', 137, 0, 80, 104, [(150, 200, 'up')], 260),
    'water_popolon_jump': ('galious_start', 137, 1, 80, 104, [(20, 100, 'up')], 160),
    'lava_popolon_drop': ('world08_entry', 16, 1, 64, 96, [], 180),
    'lava_aphrodite_drop': ('world08_entry', 16, 0, 64, 96, [], 180),
}

if __name__ == '__main__':
    out = {}
    for name, t in TESTS.items():
        log = trial(*t)
        out[name] = log
        first = next((i for i, r in enumerate(log) if r[3]), None)
        dmg = [i for i in range(1, len(log)) if log[i][5] < log[i - 1][5] or log[i][6] < log[i - 1][6]]
        print(f'{name}: entra en el liquido en el frame {first}, y final {log[-1][0]}, '
              f'bajadas de vitalidad en frames {dmg[:12]}{"..." if len(dmg) > 12 else ""} '
              f'vit final P{log[-1][5]} A{log[-1][6]}, E000 final {log[-1][7]}', flush=True)
    json.dump(out, open('logs/omsx/liquid_measure.json', 'w'))
    send('god')
