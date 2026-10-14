#!/usr/bin/env python3
"""Mide en openMSX las cajas de colision jugador-enemigo y espada-enemigo, con el enemigo congelado (E0D5).

  hitbox_measure.py
Esqueleto (0x25) del castillo, sala 104. Para cada desplazamiento (dx, dy) del jugador respecto al enemigo,
mira si el contacto le hace dano (E500 = 4) y, mirando hacia el, si un espadazo le quita vida.
"""
import json, sys
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run
from scan_rooms import goto_room, peek, ram


def setup():
    goto_room('galious_start', 104)
    send('proc frz {} { poke 0xE0D5 255; poke 0xE518 0; set ::frz_id [after frame frz] }')
    send('frz'); run(0.2)
    slots = [bytes.fromhex(ram(0xE800 + 0x20 * i, 0x20)) for i in range(16)]
    idx = next(i for i, s in enumerate(slots) if s[0] == 0x25)
    base = 0xE800 + 0x20 * idx
    return base, peek(base + 7), peek(base + 5)


def contact(dx, dy):
    base, ex, ey = setup()
    send(f'poke 0xE507 {ex + dx}'); send(f'poke 0xE505 {ey + dy}'); send('poke 0xE500 0')
    hit = False
    for _ in range(4):
        run(0.05)
        hit |= peek(0xE500) == 4
    send('after cancel $::frz_id')
    return hit


def sword(dx, dy, facing):
    base, ex, ey = setup()
    send('proc frz {} { poke 0xE0D5 255; poke 0xE518 1; set ::frz_id [after frame frz] }')
    hp0 = peek(base + 0x10)
    send(f'poke 0xE507 {ex + dx}'); send(f'poke 0xE505 {ey + dy}'); send(f'poke 0xE503 {facing}'); send('poke 0xE500 0')
    run(0.1); run(0.1, 'space'); run(0.3)
    send('after cancel $::frz_id')
    return hp0 - peek(base + 0x10)


if __name__ == '__main__':
    res = {'contact': {}, 'sword': {}}
    base, ex, ey = setup(); send('after cancel $::frz_id')
    print('esqueleto en', ex, ey)
    for dx in range(-12, 30, 2):
        res['contact'][dx] = contact(dx, 16)
    print('contacto (dy = 16), dx -> daño:', ''.join('X' if res['contact'][dx] else '.' for dx in range(-12, 30, 2)), '(dx de -12 a 28)')
    for dy in range(0, 34, 2):
        res['contact'][f'dy{dy}'] = contact(8, dy)
    print('contacto (dx = 8), dy -> daño:', ''.join('X' if res['contact'][f'dy{dy}'] else '.' for dy in range(0, 34, 2)), '(dy de 0 a 32)')
    for dx in range(-40, -8, 2):
        res['sword'][dx] = sword(dx, 16, 0)
    print('espada mirando a la derecha, dx -> vida quitada:', [(dx, res['sword'][dx]) for dx in range(-40, -8, 2)])
    json.dump(res, open('logs/omsx/hitbox_measure.json', 'w'))
    send('god')
