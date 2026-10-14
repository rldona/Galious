#!/usr/bin/env python3
"""Recorre salas cargandolas con el propio cargador del juego.

Desde un savestate en el bucle principal, escribe el numero de sala en el vecino
derecho (E64B), pone E01A=4 y salta al estado 10: la sala la carga el propio juego. Anota objetos (E680-E68F), vecinos (E648-E64B) y una captura.

  scan_rooms.py SAVESTATE DESDE HASTA     (numeros de sala en decimal, HASTA incluido)

Salida: logs/omsx/rooms/<savestate>_<sala>.png y rooms_<savestate>.tsv
Con SCAN_DUMP=1 guarda ademas la tabla de nombres y la de sprites en vram_<savestate>.tsv.
"""
import os, sys
DUMP = os.environ.get('SCAN_DUMP') == '1'   # guarda tambien nombres y sprites de la VRAM
from omsx import RUN, run, send


def peek(a):
    return int(send(f'peek {a}')[1])


def ram(a, n):
    return send(f'binary encode hex [debug read_block memory {a} {n}]')[1]


def goto_room(state, room):
    send(f'loadstate {state}')
    send('god')
    for cmd in (f'poke 0xE64B {room}', 'poke 0xE01A 4', 'poke 0xE001 0', 'poke 0xE000 10'):
        send(cmd)
    for _ in range(30):
        run(0.1)
        if peek(0xE000) == 5:
            break
    run(0.1)   # poco: en algunas salas el jugador cae por un hueco del borde enseguida


def main(state, first, last):
    out = os.path.join(RUN, 'rooms')
    os.makedirs(out, exist_ok=True)
    tsv = open(os.path.join(RUN, f'rooms_{state}.tsv'), 'a')
    for room in range(first, last + 1):
        goto_room(state, room)
        st, world, cur = peek(0xE000), peek(0xE041), peek(0xE042)
        if DUMP:
            nt = send('binary encode hex [debug read_block VRAM 0x3800 768]')[1]
            sat = send('binary encode hex [debug read_block VRAM 0x3B00 128]')[1]
            with open(os.path.join(RUN, f'vram_{state}.tsv'), 'a') as f:
                f.write(f'{room}\t{world}\t{cur}\t{nt}\t{sat}\n')
        boss = f'E615={peek(0xE615):02X} E0D2={peek(0xE0D2):02X} E698={peek(0xE698):02X}'
        row = f'{room}\t{st}\t{world}\t{cur}\t{ram(0xE648, 4)}\t{ram(0xE680, 16)}\t{boss}'
        send(f'screenshot -raw {out}/{state}_{room:03d}.png')
        print(row, flush=True)
        tsv.write(row + '\n')
        tsv.flush()


if __name__ == '__main__':
    main(sys.argv[1], int(sys.argv[2]), int(sys.argv[3]))
