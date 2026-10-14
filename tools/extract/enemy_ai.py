#!/usr/bin/env python3
"""Desensambla la rutina de inicio y la de actualizacion de cada tipo de enemigo.

  b04:66B4  tabla de inicio por tipo (indice = tipo - 1), la llama la creacion b04:65DA
  b06:B472  tabla de actualizacion por tipo, la llama cada frame b06:B45E (salvo con el libro, E0D5)

Sigue el flujo (JR/JP/CALL y tablas en linea tras CALL 4050) dentro del grupo de bancos 4-6
(6000-BFFF); las llamadas al banco 0 se anotan con su nombre, sin entrar. Necesita z80dis
(pip install z80dis; no es dependencia del proyecto).

Salida: extracted/ai/<tipo>.asm (codigo de la ROM: no se versiona) e index.json
"""
import json, re, sys
from galious_rom import Rom, ROOT
sys.path.insert(0, str(ROOT / 'tools'))
from rom_views import GAME, RAM
from z80dis import z80

INIT, UPDATE, NTYPES, GROUP = 0x66B4, 0xB472, 107, 1
VECTORS = {0x4050: 'jump_table_a', 0x4053: 'read_word_table', 0x4056: 'vec_4da1', 0x4059: 'sound_request',
           0x405C: 'neg_hl', 0x405F: 'vec_4db5', 0x4062: 'add_a_to_hl', 0x4065: 'add_a_to_de',
           0x4068: 'neg_de', 0x406B: 'vec_4dcf', 0x406E: 'vec_4dc8'}
# campos del hueco de enemigo (E800 + 0x20 n), lo que se sabe hasta ahora
SLOT = {0: 'tipo', 1: 'estado', 2: 'flags', 3: 'flags2', 5: 'y', 7: 'x', 9: 'd0', 0x0A: 'd1', 0x0B: 'd2',
        0x0C: 'vy_lo', 0x0D: 'vy', 0x0E: 'vx_lo', 0x0F: 'vx', 0x10: 'vida', 0x11: 'atributo_sprite',
        0x16: 'ay_lo', 0x17: 'ay'}   # b06:B0ED suma vy/vx a y/x; B14C suma ay a vy
NAMES = {**GAME, **VECTORS, **RAM, 0xE505: 'jugador_y', 0xE507: 'jugador_x', 0xE0D5: 'libro_congela'}


def read(rom, a):
    return rom.byte(a, None if a < 0x6000 else GROUP)


def disasm_routine(rom, entry):
    todo, seen, lines = [entry], set(), {}
    while todo:
        pc = todo.pop()
        while 0x4000 <= pc < 0xC000 and pc not in seen and not (pc < 0x6000 and pc != entry):
            seen.add(pc)
            raw = bytes(read(rom, pc + i) for i in range(4))
            text = z80.disasm(raw, pc)
            size = z80.decode(raw, pc).len or 1
            note = ''
            for m in re.finditer(r'0x([0-9a-fA-F]{4})', text):
                v = int(m.group(1), 16)
                if v in NAMES:
                    note += f' ; {NAMES[v]}'
            m = re.search(r'\(I[XY]([+-]0x[0-9a-fA-F]+|[+-]\d+)?\)', text)
            if m:
                off = int(m.group(1).replace('+', ''), 0) if m.group(1) else 0
                if off in SLOT:
                    note += f' ; {SLOT[off]}'
            lines[pc] = f'{pc:04X}  {raw[:size].hex(" "):12s} {text}{note}'
            op = text.split()[0]
            tgt = re.search(r'0x([0-9a-fA-F]{4})$', text)
            if op in ('JR', 'JP', 'CALL', 'DJNZ') and tgt and '(' not in text:
                t = int(tgt.group(1), 16)
                if op == 'CALL' and t == 0x4050:                  # tabla de saltos en linea
                    a = pc + 3
                    while 0x6000 <= (w := read(rom, a) | read(rom, a + 1) << 8) < 0xC000 and a not in seen:
                        lines[a] = f'{a:04X}  {w & 0xFF:02x} {w >> 8:02x}        .word 0x{w:04X} ; caso {(a - pc - 3) // 2}'
                        seen.update((a, a + 1)); todo.append(w); a += 2
                    break
                if 0x6000 <= t < 0xC000:
                    todo.append(t)
                if op in ('JP', 'JR') and ',' not in text:      # salto incondicional
                    break
            if op in ('RET', 'RETI') and len(text.split()) == 1 or op == 'JP' and '(' in text:
                break                                     # 'RET nz' (condicional) sigue
            pc += size
    return '\n'.join(lines[a] for a in sorted(lines))


def main():
    rom = Rom()
    ent = json.loads((ROOT / 'extracted' / 'entities' / 'enemies.json').read_text())
    out = ROOT / 'extracted' / 'ai'
    out.mkdir(parents=True, exist_ok=True)
    index = {}
    for t in range(1, NTYPES + 1):
        ini = read(rom, INIT + 2 * (t - 1)) | read(rom, INIT + 2 * t - 1) << 8
        upd = read(rom, UPDATE + 2 * (t - 1)) | read(rom, UPDATE + 2 * t - 1) << 8
        key = f'{t:#04x}'
        index[key] = {'init': f'{ini:04X}', 'update': f'{upd:04X}', 'in_rooms': key in ent}
        if key not in ent:
            continue
        text = (f'; tipo {key}  vida {ent[key]["hp"]}  patron observado: '
                f'{(ent[key].get("observed") or {}).get("pattern")}\n'
                f'; ---- inicio (b04:66B4[{t - 1}]) ----\n{disasm_routine(rom, ini)}\n'
                f'; ---- actualizacion (b06:B472[{t - 1}]) ----\n{disasm_routine(rom, upd)}\n')
        (out / f'{t:02x}.asm').write_text(text)
    (out / 'index.json').write_text(json.dumps(index, indent=1))
    shared = {}
    for k, v in index.items():
        shared.setdefault(v['update'], []).append(k)
    print(f'{sum(v["in_rooms"] for v in index.values())} tipos en salas; rutinas de actualizacion compartidas:',
          {a: ks for a, ks in shared.items() if len(ks) > 1})


if __name__ == '__main__':
    main()
