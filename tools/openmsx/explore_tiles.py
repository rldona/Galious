#!/usr/bin/env python3
"""Barrido de salas buscando tiles que cambian en plena partida (escaleras que se van, muros que salen...).

  explore_tiles.py [mapa ...]        (castle, world01..world10; sin argumentos, todos)

En cada sala, con el jugador invencible, para cada altura de suelo (y = 56, 88, 120, 152, 184)
lo coloca en un borde y lo hace andar hacia el otro durante WALK segundos, primero de izquierda a
derecha y luego al reves. tile_watch.tcl anota cada escritura en el mapa de nombres de la sala que
cambia un tile (E000 = 5). Salida: logs/omsx/tile_changes.tsv
  mapa  sala_cargada  pasada  mundo  sala  pc  retorno  dir_vram  antes  despues  jugador_y  jugador_x
"""
import json, os, sys
from omsx import RUN, run, send
from scan_rooms import goto_room, peek
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'extract'))
from galious_rom import ROOT

WALK, FLOORS = 2.5, (56, 88, 120, 152, 184)
STATE = {'castle': 'galious_start', **{f'world{k:02d}': f'world{k:02d}_entry' for k in range(1, 11)}}


def main(maps):
    idx = json.loads((ROOT / 'extracted' / 'maps' / 'index.json').read_text())
    send('source ' + os.path.abspath(os.path.join(os.path.dirname(__file__), 'tile_watch.tcl')))
    send('set throttle off')
    out = open(os.path.join(RUN, 'tile_changes.tsv'), 'a')
    for name in maps:
        info = idx[name]
        n = info['table_rooms'] - (0 if info['e041'] in (1, 11) else 1)
        for room in range(1, n + 1):
            changes = 0
            for y in FLOORS:
                for x, key in ((16, 'right'), (232, 'left')):
                    goto_room(STATE[name], room)
                    if peek(0xE042) != room:
                        continue
                    send(f'poke 0xE505 {y}'); send(f'poke 0xE507 {x}')
                    send('tw_on')
                    run(WALK, key)
                    hits = send('join $::tw_hits \\n')[1]
                    send('tw_off')
                    for h in filter(None, hits.split('\n')):
                        out.write(f'{name}\t{room}\t{y}{key[0]}\t' + '\t'.join(h.split()[:9]) + '\n')
                        changes += 1
            out.flush()
            print(f'{name} sala {room}: {changes} cambios', flush=True)
    send('set throttle on')


if __name__ == '__main__':
    main(sys.argv[1:] or list(STATE))
