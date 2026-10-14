#!/usr/bin/env python3
"""Escudo del esqueleto 0x25 (IX+2 bit 1, lado en el bit 0; b03:B6C1): ¿la espada le quita vida por delante y por detrás?

  OMSX_SOCK=... shield_check.py
Castillo, sala 104 (estado room104). Congela a los enemigos con el libro (E0D5) y el jugador es invulnerable (E518).
Para cada lado, pone al jugador a 12 px del esqueleto, mirándolo, da 3 espadazos y lee su vida (+0x10) y su IX+2.
"""
import sys, time
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run

def peek(a): return int(send(f'peek {a}')[1])

def skeleton():
    for n in range(16):
        b = 0xE800 + 0x20 * n
        if peek(b) == 0x25:
            return b
    return None

for side in ('izquierda', 'derecha'):
    send('loadstate room104')
    send('proc freeze {} { poke 0xE0D5 200; poke 0xE518 1; after frame freeze }; freeze')
    run(0.3)
    b = skeleton()
    if b is None:
        print('sin esqueleto'); break
    x, y, f = peek(b + 7), peek(b + 5), peek(b + 2)
    hp0 = peek(b + 0x10)
    px = x - 4 if side == 'izquierda' else x + 20     # centro del jugador a un lado de la caja 16 px
    send(f'poke 0xE507 {px}; poke 0xE505 {y + 16}; poke 0xE503 {0 if side == "izquierda" else 1}')
    for k in range(3):
        send('keymatrixdown 8 0x01'); run(0.1); send('keymatrixup 8 0x01'); run(0.25)
    hp1 = peek(b + 0x10)
    print(f'jugador a la {side} (x {px}), esqueleto x {x} y {y}, IX+2 {f:08b} (bit0 = {f & 1}): vida {hp0} -> {hp1}')
    send('after cancel [lindex [after info] 0]')
