#!/usr/bin/env python3
"""Desensambla el inicio y la actualizacion de cada jefe (como enemy_ai.py para los enemigos).

  b01:7F82  tabla de inicio por jefe (indice = E700 - 1), la llama b01:7F7F al invocarlo
  b01:7F40  tabla de actualizacion por jefe, cada frame desde b01:7F38 (salvo con el libro, E0D5)
  b01:7F97  vida por jefe (E710, tabla[E700]); con el Holy Water (E076) se divide entre 2
  b00:5E05  numero de jefe por mundo (indice E041 = mundo + 1); lo escribe la invocacion (b00:4985) en E0D3
            mundo 1->1, 2->4, 3->2, 4->11, 5->10, 6->3, 7->7, 8->8, 9->6, 10->9; el jefe 5 no se usa
El codigo de los jefes esta en b02 (8000-9FFF, grupo 0); las ayudas de movimiento del banco 6 se
llaman a traves de envoltorios (b01:7FA3-7FCD) que mapean el banco 6 en A000.

Salida: extracted/ai/boss_<n>.asm (codigo de la ROM: no se versiona) y boss_index.json
"""
import json
import enemy_ai
from galious_rom import Rom, ROOT

INIT, UPDATE, HP, COUNT, WORLD_BOSS = 0x7F82, 0x7F40, 0x7F97, 11, 0x5E05
enemy_ai.GROUP = 0                       # bancos 1-3: 8000-9FFF es el banco 2
enemy_ai.NAMES.update({0xE700: 'jefe_tipo', 0xE710: 'jefe_vida', 0xE0D2: 'jefe_fase', 0xE615: 'escuchando_palabra',
                       0x7FA3: 'vx=DE (b06:B13E)', 0x7FAC: 'invierte vx (b06:B15F)', 0x7FB5: 'vy=DE (b06:B137)',
                       0x7FBE: 'ay=DE (b06:B145)', 0x7FC7: 'vy+=ay (b06:B14C)'})


def main():
    rom = Rom()
    out = ROOT / 'extracted' / 'ai'
    out.mkdir(parents=True, exist_ok=True)
    words = lambda base: [rom.word(base + 2 * i, 0) for i in range(COUNT)]
    inits, updates = words(INIT), words(UPDATE)
    hp = [rom.byte(HP + i + 1, 0) for i in range(COUNT)]          # 4155: tabla[E700]
    world_of = {rom.byte(WORLD_BOSS + e041, None): e041 - 1 for e041 in range(2, 12)}
    index = {}
    for n in range(COUNT):
        index[n + 1] = {'world': world_of.get(n + 1), 'init': f'{inits[n]:04X}', 'update': f'{updates[n]:04X}', 'hp': hp[n]}
        text = (f'; jefe {n + 1} (E700), mundo {world_of.get(n + 1)}, vida (7F97[{n + 1}]): {hp[n]}\n'
                f'; ---- inicio (b01:7F82[{n}]) ----\n{enemy_ai.disasm_routine(rom, inits[n])}\n'
                f'; ---- actualizacion (b01:7F40[{n}]) ----\n{enemy_ai.disasm_routine(rom, updates[n])}\n')
        (out / f'boss_{n + 1:02d}.asm').write_text(text)
        print(n + 1, index[n + 1], len(text.splitlines()), 'lineas')
    (out / 'boss_index.json').write_text(json.dumps(index, indent=1))


if __name__ == '__main__':
    main()
