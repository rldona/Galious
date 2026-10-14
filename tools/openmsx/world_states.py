#!/usr/bin/env python3
"""Crea un savestate world<NN>_entry dentro de cada mundo entrando por su puerta del castillo.

La puerta se marca abierta (E689 = 1) y el jugador se coloca en ella; la entrada
(estado 8) la hace el juego al pulsar abajo. Mundos 1-9 (la puerta 10 es dinamica).
"""
import sys
sys.path.insert(0, __file__.rsplit('/', 2)[0] + '/extract')
from omsx import run, send
from scan_rooms import goto_room, peek
from galious_rom import Rom

rom = Rom()
for k in range(1, 10):
    room = rom.byte(0x9990 + k - 1, 0)
    goto_room('galious_start', room)
    y, x = peek(0xE68A) + 0x18, peek(0xE68B) + 16
    send('poke 0xE689 1')
    send(f'poke 0xE505 {y}')
    send(f'poke 0xE507 {x}')
    run(0.2)
    run(0.2, 'down')
    for _ in range(60):
        run(0.1)
        if peek(0xE041) == k + 1 and peek(0xE000) == 5:
            break
    ok = peek(0xE041) == k + 1 and peek(0xE000) == 5
    if ok:
        run(0.3)
        send(f'savestate world{k:02d}_entry')
    print(f'mundo {k}: puerta en sala {room} -> E041={peek(0xE041)} E042={peek(0xE042)} estado={peek(0xE000)} {"OK" if ok else "FALLO"}', flush=True)
