#!/usr/bin/env python3
"""Vuelca la VRAM del final (docs/ending.md): la escena del castillo (estado 17, sub 5) y el epílogo (sub 7).

  OMSX_SOCK=... ending_vram.py [castle] [epilogue]
Desde el estado guardado end_baby: lleva al jugador hasta el bebé (←) y espera a cada fase. Guarda los registros del VDP
y la VRAM entera (16 KB) en logs/omsx/ending/vram_castle.json y vram_epilogue.json, más una captura.
"""
import json, sys, time
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, RUN

def peek(a):
    return int(send(f'peek {a}')[1])

def dump(name):
    regs = [int(send(f'vdpreg {r}')[1]) for r in range(8)]
    vram = send('binary encode hex [debug read_block VRAM 0 16384]')[1]
    json.dump({'regs': regs, 'vram': vram, 'E000': peek(0xE000), 'E001': peek(0xE001)},
              open(f'{RUN}/ending/vram_{name}.json', 'w'))
    send('set renderer SDLGL-PP')
    send(f'screenshot -raw {RUN}/ending/vram_{name}.png')
    print(name, 'regs', regs)

send('loadstate end_baby')
send('set throttle on')
send('set pause off')
want = {5: ('castle', 2.0), 7: ('epilogue', 12.0)}
if len(sys.argv) > 1: want = {k: v for k, v in want.items() if v[0] in sys.argv[1:]}
t0 = time.time()
while want and time.time() - t0 < 120:
    s0, s1 = peek(0xE000), peek(0xE001)
    if s0 == 5:
        send('keymatrixdown 8 0x10')          # el bebé está a la izquierda del jugador
    elif s0 == 17:
        send('keymatrixup 8 0xFF')
        if s1 in want:
            name, wait = want.pop(s1)
            time.sleep(wait)                   # que se dibuje la escena (o salga texto en el epílogo)
            send('set pause on')
            dump(name)
            send('set pause off')
    time.sleep(0.05)
send('keymatrixup 8 0xFF')
