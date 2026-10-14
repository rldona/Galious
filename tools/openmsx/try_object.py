#!/usr/bin/env python3
"""Va a una sala, coloca al jugador sobre su objeto E680 o su puerta E688 y pulsa abajo.

  try_object.py SAVESTATE SALA obj|door [segundos]

Anota la secuencia de estados (cada 0.1 s) y hace capturas en logs/omsx/try/.
El viaje y la colocacion son forzados; lo que pase al pulsar abajo lo decide el juego ([O*]).
"""
import os, sys
from omsx import RUN, run, send
from scan_rooms import goto_room, peek, ram

# (direccion de Y, desplazamiento Y, desplazamiento X) segun 9A5F / 9A56
TARGET = {'obj': (0xE682, 0x10, 8), 'door': (0xE68A, 0x18, 16)}


def main(state, room, kind, secs=8.0):
    out = os.path.join(RUN, 'try')
    os.makedirs(out, exist_ok=True)
    goto_room(state, room)
    ya, dy, dx = TARGET[kind]
    y, x = peek(ya) + dy, peek(ya + 1) + dx
    send(f'poke 0xE505 {y}')
    send(f'poke 0xE507 {x}')
    run(0.2)
    tag = f'{room:03d}_{kind}'
    send(f'screenshot -raw {out}/{tag}_0.png')
    print(f'sala {room} {kind}: objeto {ram(0xE680, 16)} jugador y={peek(0xE505)} x={peek(0xE507)}')
    run(0.2, 'down')
    seq, t, shots = [], 0.0, 0
    while t < secs:
        run(0.1)
        t = round(t + 0.1, 2)
        st = (peek(0xE000), peek(0xE001))
        if not seq or seq[-1][1] != st:
            seq.append((t, st))
            if shots < 6:
                shots += 1
                send(f'screenshot -raw {out}/{tag}_{shots}.png')
    print(' '.join(f'{s[0]}.{s[1]}@{tt:g}' for tt, s in seq), f'E041={peek(0xE041)} E042={peek(0xE042)} E690={peek(0xE690)}')
    send(f'screenshot -raw {out}/{tag}_end.png')


if __name__ == '__main__':
    main(sys.argv[1], int(sys.argv[2]), sys.argv[3], float(sys.argv[4]) if len(sys.argv) > 4 else 8.0)
