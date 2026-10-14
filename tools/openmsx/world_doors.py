#!/usr/bin/env python3
"""Mide en openMSX donde aparece el jugador al entrar en cada mundo por su puerta y al volver al castillo.

  world_doors.py
Entrada: puerta abierta a mano (E689 = 1), jugador en y = Y_puerta + 0x18, x = X_puerta + 16, abajo.
Salida: desde la sala 1 del mundo se cruza el borde que lleva al castillo (enlace 0xF5-0xFE), colocando al
jugador junto a ese borde con la colision de extracted/unity.
"""
import json, sys
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run
from scan_rooms import goto_room, peek

ROOT = __file__.rsplit('/', 3)[0]
castle = json.load(open(f'{ROOT}/extracted/maps/castle.json'))


def wait_state5(e041, secs=8):
    for _ in range(int(secs / 0.2)):
        run(0.2)
        if peek(0xE000) == 5 and peek(0xE041) == e041:
            return True
    return False


def where():
    return {'e041': peek(0xE041), 'room': peek(0xE042), 'x': peek(0xE507), 'y': peek(0xE505), 'E500': peek(0xE500)}


def exit_spot(world):
    b = json.load(open(f'{ROOT}/extracted/unity/world{world:02d}.json'))
    r1 = next(r for r in b['rooms'] if r['id'] == 1)
    g = r1['collision']
    d = next(d for d in ('up', 'down', 'left', 'right') if r1[d] >= 0xF5)
    if d in ('left', 'right'):
        col = 0 if d == 'left' else 31
        for row in range(19, 0, -1):
            if g[row - 1][col] == '.' and g[row][col] == '#':
                return d, (12 if d == 'left' else 244), 8 * row + 32
    if d == 'up':
        for c in range(31):
            if g[0][c] == 'H' and g[0][c + 1] == 'H':
                return d, 8 * c + 8, 40
    if d == 'down':
        for c in range(1, 31):
            if g[19][c - 1] == '.' and g[19][c] == '.':
                return d, 8 * c, 180
    return d, None, None


def main():
    res = {}
    for k in range(1, 10):
        room, door = next((int(r), v) for r, v in castle['doors'].items() if v['world'] == k)
        goto_room('galious_start', room)
        send('poke 0xE689 1')
        send(f'poke 0xE505 {door["y"] + 0x18}'); send(f'poke 0xE507 {door["x"] + 16}')
        run(0.2); run(0.3, 'down')
        ok = wait_state5(k + 1)
        entry = where()
        d, x, y = exit_spot(k)
        out = None
        if ok and x is not None:
            send(f'poke 0xE507 {x}'); send(f'poke 0xE505 {y}')
            if d == 'up':
                run(0.2, 'up')           # engancharse a la escalera
            run(0.1)
            run(3.0, d if d != 'down' else '')
            wait_state5(1, 6)
            out = where()
        res[k] = {'door_room': room, 'door': door, 'entry': entry, 'exit_dir': d, 'exit_from': [x, y], 'back': out}
        print(f'mundo {k}: puerta sala {room} ({door["x"]},{door["y"]}) -> entra en {entry}; '
              f'sale por {d} desde ({x},{y}) -> {out}', flush=True)
    json.dump(res, open(f'{ROOT}/logs/omsx/world_doors.json', 'w'))
    send('god')


if __name__ == '__main__':
    main()
