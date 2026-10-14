#!/usr/bin/env python3
"""Comprueba en openMSX los muros que se cierran (clase 5) que el barrido de tiles no llegó a activar.

  OMSX_SOCK=... wall5_check.py [sala ...]      (por defecto 15 17 29 41 76; castillo, desde galious_start)
Coloca al jugador 24 px a la izquierda de la columna del muro, a la altura de su base, y anda a la derecha 200 frames
(y luego a la izquierda desde el otro lado). Registra la columna del muro en el mapa de nombres (ED00).
Salida: logs/omsx/wall5_check.json y un resumen por sala.
"""
import json, sys
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from room_object_measure import setup, trace, changes

ROOMS = {15: (88, 128), 17: (160, 64), 29: (64, 64), 41: (88, 64), 76: (152, 128)}
# Inicio por la izquierda si la x - 24 cae en un hueco o contra un pilar (sala 17: pozo en x 112..143).
START_LEFT = {17: 150}

out = {}
for room in [int(a) for a in sys.argv[1:]] or list(ROOMS):
    x, y = ROOMS[room]
    col, row = x // 8, (y - 32) // 8
    cells = [(r, col) for r in range(row, row + 3)]
    for side, start, keys, facing in (('derecha', START_LEFT.get(room, x - 24), 'right', 0), ('izquierda', x + 32, 'left', 1)):
        setup('galious_start', room, start, y + 24, 1, facing, pokes=[('0xE518', 1)])
        log = trace([(2, 60, keys)], 200, 0xEAA0, 8, cells)
        from omsx import send
        send('set renderer SDLGL-PP'); send(f'screenshot -raw logs/omsx/wall5_{room}_{keys}.png')
        ch = changes(log)
        tiles = [r[-3:] for r in log]
        closed = tiles[-1] != tiles[0]
        out[f'{room}_{side}'] = {'log': log, 'closed': closed}
        phase = [int(r[4][2:4], 16) for r in log]
        print(room, side, 'fase final', phase[-1], 'cuenta', int(log[-1][4][8:10], 16), 'x', log[0][0], '->', log[-1][0], 'y', log[0][1], '->', log[-1][1],
              'tiles', ' '.join(tiles[0]), '->', ' '.join(tiles[-1]))
json.dump(out, open('logs/omsx/wall5_check.json', 'w'))
