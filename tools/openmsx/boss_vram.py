#!/usr/bin/env python3
"""Vuelca la VRAM de los 10 combates contra los jefes (docs/boss_ai.md, "Cómo se dibuja el jefe").

  OMSX_SOCK=... boss_vram.py [mundo ...]
Desde los estados w{k}_boss_fight_auto (tools/openmsx/boss_all.py): medio segundo de juego y se guardan los registros
del VDP, la VRAM entera y la posición del jefe (E705 y, E707 x; E700 número de jefe) en
logs/omsx/bosses/vram_w{k}.json, más una captura. Lo usa tools/extract/boss_gfx.py.
"""
import json, sys, time
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import send, run, RUN

WAIT = {}                                        # segundos de juego antes del volcado, por mundo
worlds = [int(a) for a in sys.argv[1:]] or range(1, 11)
for k in worlds:
    send(f'loadstate w{k}_boss_fight_auto')
    send('god')
    run(WAIT.get(k, 0.5))
    peek = lambda a: int(send(f'peek {a}')[1])
    d = {'regs': [int(send(f'vdpreg {r}')[1]) for r in range(8)],
         'vram': send('binary encode hex [debug read_block VRAM 0 16384]')[1],
         'E700': peek(0xE700), 'E705': peek(0xE705), 'E707': peek(0xE707), 'E0D2': peek(0xE0D2), 'E042': peek(0xE042)}
    json.dump(d, open(f'{RUN}/bosses/vram_w{k}.json', 'w'))
    send('set renderer SDLGL-PP')
    send(f'screenshot -raw {RUN}/bosses/vram_w{k}.png')
    print(k, 'jefe', d['E700'], 'x', d['E707'], 'y', d['E705'], 'E0D2', d['E0D2'], 'regs', d['regs'][:5])
