#!/usr/bin/env python3
"""Mide en openMSX el daño por contacto con un enemigo, el retroceso y la invulnerabilidad.

  contact_measure.py [estado sala tipo]   (por defecto: galious_start 104 0x25, el esqueleto)
Busca el enemigo del tipo en E800 + 0x20·n, coloca al jugador encima y registra por frame
x, y, E500, E518, vitalidad (E056 Popolon / E052 Aphrodite) y la posición del enemigo. Sin 'god'.
"""
import json, re, sys
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run
from scan_rooms import goto_room, peek, ram

state, room, etype = (sys.argv[1], int(sys.argv[2]), int(sys.argv[3], 16)) if len(sys.argv) > 3 else ('galious_start', 104, 0x25)
goto_room(state, room)
send('catch {after cancel $::god_id}')
send('poke 0xE056 8'); send('poke 0xE052 8'); run(0.2)
slots = [bytes.fromhex(ram(0xE800 + 0x20 * i, 0x20)) for i in range(16)]
idx = next(i for i, s in enumerate(slots) if s[0] == etype)
base = 0xE800 + 0x20 * idx
ey, ex = peek(base + 5), peek(base + 7)
print(f'enemigo 0x{etype:02X} en el hueco {idx}: x={ex} y={ey} vida={peek(base + 0x10)}')
# El jugador encima del enemigo: pies 16 px por debajo de su y, centro 8 px a la derecha de su x.
send(f'poke 0xE507 {ex + 8}'); send(f'poke 0xE505 {ey + 16}')
f = f'[peek 0xE507] [peek 0xE505] [peek 0xE500] [peek 0xE518] [peek 0xE056] [peek {base + 7}] [peek {base + 5}]'
send('proc ct_tick {} { lappend ::ct_log "' + f + '"; incr ::ct_left -1; if {$::ct_left > 0} { after frame ct_tick } }')
send('set ::ct_log {}; set ::ct_left 150; after frame ct_tick')
run(3)
log = [list(map(int, r.split())) for r in re.findall(r'\{([^}]*)\}', send('set ::ct_log')[1])]
prev = None
for i, r in enumerate(log):
    if r[2:5] != prev:
        print(f'{i:3d} x={r[0]} y={r[1]} E500={r[2]} E518={r[3]} vit={r[4]} enemigo=({r[5]},{r[6]})')
    prev = r[2:5]
print('x,y por frame tras el golpe:', [(r[0], r[1]) for r in log[:40:2]])
json.dump(log, open('logs/omsx/contact_measure.json', 'w'))
send('god')
