#!/usr/bin/env python3
"""Umbral del toque de salto: ↑ mantenido N frames (4..10, con las dos fases del tick) desde el suelo.

  OMSX_SOCK=... jump_tap.py
Castillo, sala 1, desde galious_start. Para cada N y cada desfase (0 o 1 frame), registra la y mínima y E500.
"""
import sys
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from room_object_measure import setup, trace

for n in range(4, 11):
    res = []
    for phase in (0, 1):
        setup('galious_start', 1, 192, 120, 1, pokes=[('0xE518', 1)])
        log = trace([(4 + phase, 4 + phase + n, 'up')], 70)
        ys = [int(r[1]) for r in log]
        res.append(f'desfase {phase}: y mín {min(ys)} estados {"".join(sorted(set(r[2] for r in log)))}')
    print(f'{n:2d} frames  ' + '   '.join(res))
