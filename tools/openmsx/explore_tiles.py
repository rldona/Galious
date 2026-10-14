#!/usr/bin/env python3
"""Barrido de salas buscando tiles que cambian en plena partida (escaleras que se van, muros que salen...).

  explore_tiles.py [mapa ...]        (castle, world01..world10; sin argumentos, todos)

En cada sala, con el jugador invencible, para cada altura de suelo (y = 56, 88, 120, 152, 184)
lo coloca en un borde y lo hace andar hacia el otro durante WALK segundos (proc Tcl drive, frame a
frame): de izquierda a derecha y de derecha a izquierda pulsando arriba a ratos (salta, o sube si
hay escalera) y una tercera pasada pulsando abajo a ratos (baja escaleras). Teclas: arriba = saltar
y subir, abajo = bajar, espacio = espada, M = arma secundaria (b00:4C75, b03:A71E). tile_watch.tcl anota cada escritura en el mapa de nombres de la sala que
cambia un tile (E000 = 5). Salida: logs/omsx/tile_changes.tsv
  mapa  sala_cargada  pasada  mundo  sala  pc  retorno  dir_vram  antes  despues  jugador_y  jugador_x
"""
import json, os, sys
from omsx import RUN, run, send
from scan_rooms import goto_room, peek
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'extract'))
from galious_rom import ROOT, Rom
from render_rooms import tilemap
import overlays

WALK, FLOORS = 3.0, (56, 88, 120, 152, 184)
# (x inicial, direccion, tecla extra) por pasada
PASSES = ((16, 0x80, 0x20, 'rj'), (232, 0x10, 0x20, 'lj'), (16, 0x80, 0x40, 'rd'))
DRIVE = '''
proc drive {dir extra n} {
    if {$n <= 0} { keymatrixup 8 [expr {$dir | $extra}]; return }
    keymatrixdown 8 $dir
    if {($n % 40) < 18} { keymatrixdown 8 $extra } else { keymatrixup 8 $extra }
    after frame [list drive $dir $extra [expr {$n - 1}]]
}
'''


def ladder_spans(rom, e041, room, ent, castle):
    """Escaleras de la sala (columnas de 9A 9B en el mapa previsto): [(x_jugador, y_arriba, y_abajo)]."""
    _, tm = tilemap(rom, e041, room)
    names = bytearray(128) + bytes(sum(tm, []))
    overlays.apply(names, overlays.stamps(rom, e041, room, ent, castle))
    out = []
    for c in range(31):
        r = 4
        while r < 24:
            if names[32 * r + c] == 0x9A and names[32 * r + c + 1] == 0x9B:
                top = r
                while r < 24 and names[32 * r + c] == 0x9A:
                    r += 1
                out.append((c * 8 + 8, top * 8 - 8, r * 8 - 8))
            r += 1
    return out


STATE = {'castle': 'galious_start', **{f'world{k:02d}': f'world{k:02d}_entry' for k in range(1, 11)}}


def main(maps):
    idx = json.loads((ROOT / 'extracted' / 'maps' / 'index.json').read_text())
    send('source ' + os.path.abspath(os.path.join(os.path.dirname(__file__), 'tile_watch.tcl')))
    send(DRIVE)
    send('set throttle off')
    out = open(os.path.join(RUN, 'tile_changes.tsv'), 'a')
    rom = Rom()
    ent = json.loads((ROOT / 'extracted' / 'entities' / 'entities.json').read_text())
    castle = json.loads((ROOT / 'extracted' / 'maps' / 'castle.json').read_text())
    for name in maps:
        info = idx[name]
        n = info['table_rooms'] - (0 if info['e041'] in (1, 11) else 1)
        for room in range(1, n + 1):
            changes = 0
            tries = [(y, x, dkey, extra, f'{y}{tag}') for y in FLOORS for x, dkey, extra, tag in PASSES]
            for lx, top, bottom in ladder_spans(rom, info['e041'], room, ent[name][str(room)], castle):
                tries += [(top, lx, 0, 0x40, f'L{lx}d'), (bottom, lx, 0, 0x20, f'L{lx}u')]
            for y, x, dkey, extra, tag in tries:
                goto_room(STATE[name], room)
                if peek(0xE042) != room:
                    continue
                send(f'poke 0xE505 {y}'); send(f'poke 0xE507 {x}')
                send('tw_on')
                send(f'drive {dkey} {extra} {int(WALK * 60)}' if dkey else f'keymatrixdown 8 {extra}; after time {WALK} {{keymatrixup 8 {extra}}}')
                run(WALK + 0.05)
                hits = send('join $::tw_hits \\n')[1]
                send('tw_off')
                for h in filter(None, hits.split('\n')):
                    out.write(f'{name}\t{room}\t{tag}\t' + '\t'.join(h.split()[:9]) + '\n')
                    changes += 1
            out.flush()
            print(f'{name} sala {room}: {changes} cambios', flush=True)
    send('set throttle on')


if __name__ == '__main__':
    main(sys.argv[1:] or list(STATE))
