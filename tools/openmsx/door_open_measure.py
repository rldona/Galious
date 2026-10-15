#!/usr/bin/env python3
"""Mide en openMSX como se abre una puerta de mundo desbloqueada (castillo, sala 104, puerta del mundo 1).

  OMSX_SOCK=logs/omsx/door.sock door_open_measure.py
Pone E689 = 2 (desbloqueada, sin abrir) y registra por frame x, y, E500, E689, E68C-E68F y los nombres de la
VRAM de la zona de la puerta. Prueba 1: el jugador quieto delante de la puerta. Prueba 2: llega andando desde la
derecha. Prueba 3: llega andando desde la izquierda. Capturas en logs/omsx/door_anim_*.png.
"""
import json, re, sys
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run, RUN
from scan_rooms import goto_room, peek

ROOM, DX, DY = 104, 64, 64
COL, ROW = DX // 8, DY // 8
KEYS = {'left': 0x10, 'right': 0x80, '': 0}


def names():
    rows = []
    for r in range(ROW - 1, ROW + 4):
        rows.append(' '.join('%02X' % int(send(f'vpeek {0x3800 + 32 * r + c}')[1]) for c in range(COL - 1, COL + 5)))
    return ' / '.join(rows)


def trial(x, key, frames, shots=()):
    goto_room('galious_start', ROOM)
    send('poke 0xE689 2')
    send(f'poke 0xE507 {x}'); send(f'poke 0xE505 {DY + 0x18}')
    run(0.05)
    f = '[peek 0xE507] [peek 0xE505] [peek 0xE500] [peek 0xE689] [peek 0xE68C] [peek 0xE68D] [peek 0xE68E] [peek 0xE68F]'
    tiles = ' '.join(f'[vpeek {0x3800 + 32 * r + c}]' for r in range(ROW, ROW + 3) for c in range(COL, COL + 4))
    send('proc dm_tick {} { lappend ::dm_log "' + f + ' ' + tiles + '"; incr ::dm_left -1; if {$::dm_left > 0} { after time 0.016688 dm_tick } }')
    if key:
        send(f'keymatrixdown 8 {KEYS[key]}')
    send(f'set ::dm_log {{}}; set ::dm_left {frames}; after time 0.016688 dm_tick')
    run(frames / 60 + 0.3)
    send('keymatrixup 8 0xFF')
    return [list(map(int, r.split())) for r in re.findall(r'\{([^}]*)\}', send('set ::dm_log')[1])]


def show(log):
    for i, r in enumerate(log):
        if i == 0 or r[2:] != log[i - 1][2:]:
            print('  f%3d x=%3d y=%3d E500=%d E689=%d E68C=%d E68D=%d E68E=%d E68F=%d  ' % (i, *r[:8]) +
                  ' '.join('%02X' % t for t in r[8:]))


if __name__ == '__main__':
    out = {}
    for name, x, key in (('quieto', DX + 16, ''), ('desde_derecha', DX + 60, 'left'), ('desde_izquierda', DX - 30, 'right')):
        print(name)
        out[name] = trial(x, key, 150)
        show(out[name])
    # Capturas de la animacion (quieto delante).
    goto_room('galious_start', ROOM)
    send(f'screenshot -raw {RUN}/door_anim_closed.png')
    send('poke 0xE689 2'); send(f'poke 0xE507 {DX + 16}'); send(f'poke 0xE505 {DY + 0x18}')
    for k in range(8):
        run(0.1)
        send(f'screenshot -raw {RUN}/door_anim_{k}.png')
    json.dump(out, open(f'{RUN}/door_open_measure.json', 'w'))
    send('god')
