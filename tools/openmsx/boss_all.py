"""Invoca a cada jefe en su sala (palabra de b02:9B55) y mide como se dibuja durante el combate.

  boss_all.py DIR_SALIDA [mundos...]  -> DIR_SALIDA/boss_all.json, capturas logs/omsx/bossNN_{room,fight}.png
Por frame: E705 E706 E707 E708, celdas cambiadas en la tabla de nombres (VRAM 3800) y bytes cambiados en los
patrones (0000-17FF). Tambien cuenta los tiles cuyo color pasa a 00 al empezar el combate (decoracion apagada).
Necesita los savestates worldNN_entry (world_states.py).
"""
import sys, re, json
sys.path.insert(0, 'tools/openmsx'); sys.path.insert(0, 'tools/extract')
from omsx import run, send
from scan_rooms import goto_room, peek
from galious_rom import Rom
OUT = sys.argv[1]
rom = Rom()
send('source tools/openmsx/boss_frames.tcl')
res = {}
for k in [int(a) for a in sys.argv[2:]] or range(1, 11):
    e041 = k + 1
    p = rom.word(0x9B55 + 3 * (k - 1), 0); n = rom.byte(0x9B55 + 3 * (k - 1) + 2, 0)
    word = bytes(rom.byte(p + i, 0) for i in range(n)).decode()
    room = rom.byte(0x5DFB + e041)
    goto_room(f'world{k:02d}_entry', room)
    run(0.5)
    if (peek(0xE041), peek(0xE042)) != (e041, room):
        print(k, 'no llegué', peek(0xE041), peek(0xE042)); continue
    col_before = send('binary encode hex [debug read_block VRAM 0x2000 0x1800]')[1]
    send(f'screenshot logs/omsx/boss{k:02d}_room.png')
    send(f'type {word.lower()}')
    for _ in range(80):
        run(0.25)
        if peek(0xE0D2) == 4: break
    if peek(0xE0D2) != 4:
        print(k, word, 'no empezó el combate: E0D2 =', peek(0xE0D2), 'E000 =', peek(0xE000)); continue
    run(1.0)
    send(f'savestate w{k}_boss_fight_auto')
    col_after = send('binary encode hex [debug read_block VRAM 0x2000 0x1800]')[1]
    send(f'screenshot logs/omsx/boss{k:02d}_fight.png')
    send('bf_start 240'); run(4.5)
    log = [list(map(int, x.split())) for x in re.findall(r'\{([^}]*)\}', send('set ::bf_log')[1])]
    cb, ca = bytes.fromhex(col_before), bytes.fromhex(col_after)
    blacked = sorted({(i % 0x800) // 8 for i in range(len(cb)) if ca[i] == 0 and cb[i] != 0})
    res[k] = {'word': word, 'boss_id': rom.byte(0x5E05 + e041), 'log': log, 'blacked': blacked}
    nd = [r[4] for r in log[1:]]; pd = [r[5] for r in log[1:]]
    ys = sorted({r[0] for r in log}); xs = sorted({r[2] for r in log})
    print(f'mundo {k} {word} jefe {res[k]["boss_id"]}: frames con nombres cambiados {sum(1 for v in nd if v)}/{len(nd)} '
          f'(max {max(nd)}), con patrones cambiados {sum(1 for v in pd if v)} (max {max(pd)} bytes); '
          f'E705 {ys[:6]}..{len(ys)} valores, E707 {xs[:6]}..{len(xs)} valores; tiles apagados {len(blacked)}', flush=True)
json.dump(res, open(f'{OUT}/boss_all.json', 'w'))
send('god')
