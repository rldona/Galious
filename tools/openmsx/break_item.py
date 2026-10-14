#!/usr/bin/env python3
"""Rompe la piedra que tapa el objeto de una sala, lo recoge y muestra que cambia en la RAM.

  break_item.py SAVESTATE SALA

Coloca (forzado) al jugador a la izquierda de la piedra y golpea con la espada hasta romperla;
despues lo pone encima del objeto. Captura antes/despues en logs/omsx/items/ y compara E000-E0FF.
"""
import os, sys
from omsx import RUN, run, send
from scan_rooms import goto_room, peek, ram

PIN = 'proc pin {} { if {$::pin_on} { poke 0xE505 $::pin_y; poke 0xE507 $::pin_x; after frame pin } }'


def hold(y, x, secs, keys=''):
    send(f'set ::pin_y {y}; set ::pin_x {x}; set ::pin_on 1; after frame pin')
    run(secs, keys)
    send('set ::pin_on 0')


def main(state, room):
    out = os.path.join(RUN, 'items')
    os.makedirs(out, exist_ok=True)
    send(PIN)
    goto_room(state, room)
    item = bytes.fromhex(ram(0xE740, 8))
    t, iy, ix = item[0], item[2], item[3]
    print(f'sala {room}: objeto tipo {t:#04x} en x={ix} y={iy}, piedras {ram(0xEC30, 24)}')
    send(f'screenshot -raw {out}/{room:03d}_0.png')
    before = bytes.fromhex(ram(0xE000, 0x100))
    rocks = bytes.fromhex(ram(0xEC30, 24))
    ry = next((rocks[i + 2] for i in range(0, 24, 8) if rocks[i] and rocks[i + 3] == ix), iy - 8)
    px, face = (ix - 18, 'right') if ix >= 40 else (ix + 18, 'left')
    hold(ry + 16, px, 0.05, face)
    for _ in range(6):
        hold(ry + 16, px, 0.1, 'space')
        hold(ry + 16, px, 0.1)
    send(f'screenshot -raw {out}/{room:03d}_1.png')
    print('piedras tras golpear', ram(0xEC30, 24), 'E740', ram(0xE740, 8))
    hold(ry + 16, ix, 0.6)
    run(0.5)
    after = bytes.fromhex(ram(0xE000, 0x100))
    send(f'screenshot -raw {out}/{room:03d}_2.png')
    print('E740 tras pisar', ram(0xE740, 8))
    print('cambios E000-E0FF:', ' '.join(f'{0xE000 + i:04X}:{a:02x}->{b:02x}' for i, (a, b) in enumerate(zip(before, after))
                                       if a != b and i not in (3, 4, 8, 9)))


if __name__ == '__main__':
    main(sys.argv[1], int(sys.argv[2]))
