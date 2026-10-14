#!/usr/bin/env python3
"""Comprueba en openMSX las escaleras que se retiran que el barrido de tiles no llegó a activar.

  OMSX_SOCK=... ladder_check.py [sala ...]     (por defecto 21 60 73 102 124 136; castillo, desde galious_start)
Coloca al jugador al pie de la escalera extra de la sala, la sube entera con ↑ y, en los modos 2 y 3, la vuelve a
bajar con ↓. Registra x, y, E500, EC00..EC0F y el temporizador E0AE + 2·n. Se retira si EC07 se activa.
"""
import json, sys
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from room_object_measure import setup, trace
from omsx import send

LADDERS = {21: (128, 96, 3, 3), 60: (80, 64, 7, 2), 73: (48, 64, 15, 2), 102: (224, 96, 7, 1),
           124: (160, 64, 11, 3), 136: (48, 96, 11, 2)}
out = {}
for room in [int(a) for a in sys.argv[1:]] or list(LADDERS):
    x, y, h, mode = LADDERS[room]
    down = mode != 1
    # Siempre desde el pie: sube entera y, en los modos 2 y 3, vuelve a bajarla.
    setup('galious_start', room, x + 8, y + 8 * h, 1, pokes=[('0xE518', 1)])
    t = 16 * h + 60
    presses = [(2, t, 'up')] + ([(t + 20, 2 * t + 20, 'down')] if down else [])
    log = trace(presses, 2 * t + 60 if down else t + 30, 0xEC00, 16)
    send('set renderer SDLGL-PP'); send(f'screenshot -raw logs/omsx/ladder_{room}.png')
    gone = any(int(r[4][14:16], 16) for r in log)
    n = int(log[-1][4][0:2], 16)
    timer = int(send(f'peek {0xE0AE + 2 * n}')[1])
    out[room] = {'log': log, 'retracted': gone}
    print(room, 'modo', mode, 'bajando' if down else 'subiendo', 'se retira' if gone else 'NO se retira',
          'y', log[0][1], '->', log[-1][1], 'estado', log[-1][2], 'temporizador', timer)
json.dump(out, open('logs/omsx/ladder_check.json', 'w'))
