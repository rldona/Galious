#!/usr/bin/env python3
"""Fuerza los flags que despacha game_state_05 y anota a que estado salta el juego.

Necesita el servidor de omsx.py en marcha y un savestate en el bucle principal (estado 5).
Resultados en logs/omsx/force/: <caso>_<t>.png y un resumen en force.tsv.
Lo que sale de aqui es [F] (forzado): puede no corresponder a una situacion real del juego.
"""
import os, sys
from omsx import RUN, run, send

CASES = [  # (nombre, direccion, valor) en el orden de prioridad de game_state_05
    ('E020', 0xE020, 1), ('E01F_1', 0xE01F, 1), ('E01F_2', 0xE01F, 2), ('E01C', 0xE01C, 1),
    ('E031', 0xE031, 1), ('E01B', 0xE01B, 1), ('E024', 0xE024, 1), ('E02C', 0xE02C, 1),
]
SHOTS = (0.5, 2.0, 6.0)
STEP = 0.1
TOTAL = 8.0


def peek(a):
    return int(send(f'peek {a}')[1])


def main(state='galious_start', only=None):
    out = os.path.join(RUN, 'force')
    os.makedirs(out, exist_ok=True)
    rows = []
    for name, addr, val in CASES:
        if only and name not in only:
            continue
        send(f'loadstate {state}')
        send(f'poke {addr} {val}')
        seq, t = [], 0.0
        while t < TOTAL - 1e-6:
            run(STEP)
            t = round(t + STEP, 3)
            s = (peek(0xE000), peek(0xE001))
            if not seq or seq[-1][1] != s[0]:
                seq.append((t, s[0]))
            if any(abs(t - x) < 1e-6 for x in SHOTS):
                send(f'screenshot -raw {out}/{name}_{t:g}.png')
        trail = ' '.join(f'{st}@{tt:g}' for tt, st in seq)
        rows.append(f'{name}\t{val}\t{trail}\tflag_final={peek(addr)}')
        print(rows[-1], flush=True)
    with open(os.path.join(out, 'force.tsv'), 'a') as f:
        f.write('\n'.join(rows) + '\n')


if __name__ == '__main__':
    main(only=sys.argv[1:] or None)
