#!/usr/bin/env python3
"""Graba frame a frame los huecos de enemigos (E800..E9FF, 16 x 0x20 bytes) y el jugador.

  trace_enemies.py [--near] [tipo_hex ...]   (sin tipos: todos los que tienen generador)
  --near coloca al jugador junto al primer enemigo del tipo (para emboscadas)

Para cada tipo carga la primera sala donde aparece (con invencibilidad), deja al jugador
quieto en su sitio de entrada y graba FRAMES frames dentro de openMSX (after frame).
Salida: logs/omsx/enemies/<tipo>[_near].json.gz con, por frame, [jugador_x, jugador_y, huecos].
"""
import gzip, json, os, sys
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'extract'))
from omsx import RUN, run, send
from scan_rooms import goto_room, peek
from galious_rom import ROOT

FRAMES = 360
STATE = {'castle': 'galious_start', **{f'world{k:02d}': f'world{k:02d}_entry' for k in range(1, 11)}}

REC = '''
proc rec_frame {} {
    lappend ::rec "[peek 0xE507] [peek 0xE505] [binary encode hex [debug read_block memory 0xE800 0x200]]"
    if {[incr ::rec_n] < $::rec_max} { set ::rec_id [after frame rec_frame] }
}
'''


def first_room(ent, t):
    for world, rooms in ent.items():
        for room, e in sorted(rooms.items(), key=lambda kv: int(kv[0])):
            if any(s['type'] == t for s in e['spawns']):
                return world, int(room)


def trace(t, world, room, near=False):
    goto_room(STATE[world], room)
    if near:   # coloca al jugador a 24 px a la izquierda del primer enemigo del tipo
        mem = bytes.fromhex(send('binary encode hex [debug read_block memory 0xE800 0x200]')[1])
        for i in range(0, 0x200, 0x20):
            if mem[i] == t:
                send(f'poke 0xE505 {mem[i + 5]}')
                send(f'poke 0xE507 {max(16, mem[i + 7] - 24)}')
                break
    send(REC)
    send(f'set ::rec {{}}; set ::rec_n 0; set ::rec_max {FRAMES}; set ::rec_id [after frame rec_frame]')
    run(FRAMES / 60 + 0.2)
    rows = send('join $::rec \\n')[1].split('\n')
    frames = []
    for r in rows:
        px, py, mem = r.split(' ')
        m = bytes.fromhex(mem)
        slots = [m[i:i + 0x20].hex() for i in range(0, 0x200, 0x20)]
        frames.append([int(px), int(py), slots])
    return frames


def main(types, near=False):
    ent = json.loads((ROOT / 'extracted' / 'entities' / 'entities.json').read_text())
    out = os.path.join(RUN, 'enemies')
    os.makedirs(out, exist_ok=True)
    if not types:
        types = sorted({s['type'] for w in ent.values() for e in w.values() for s in e['spawns']})
    for t in types:
        world, room = first_room(ent, t)
        frames = trace(t, world, room, near)
        with gzip.open(os.path.join(out, f'{t:02x}{"_near" if near else ""}.json.gz'), 'wt') as f:
            json.dump({'type': t, 'world': world, 'room': room, 'frames': frames, 'near': near}, f)
        alive = sum(1 for s in frames[0][2] if int(s[:2], 16) == t)
        print(f'{t:02x} {world}:{room} frames={len(frames)} huecos_del_tipo={alive}', flush=True)


if __name__ == '__main__':
    args = sys.argv[1:]
    near = '--near' in args
    main([int(a, 16) for a in args if a != '--near'], near)
