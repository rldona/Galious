#!/usr/bin/env python3
"""Genera vistas Z80 de 64 KB de la ROM (mapper Konami) para analizarlas con REA/Ghidra.

REA solo acepta ELF/PE/Mach-O, asi que cada vista es un ELF32 (e_machine=3) cuyos
PT_LOAD colocan la imagen Z80 en vaddr 0. Con REA_GHIDRA_LANGUAGE_ID=z80:LE:16:default
el cargador ELF de Ghidra mapea esos segmentos en el espacio por defecto, asi que
direccion Ghidra == direccion Z80 (la imagen empieza en el offset de fichero IMG_OFF).
Ojo: con una cabecera ELF sin program headers Ghidra 12.1 NO carga en crudo, sino que
mete el fichero en un overlay 'unallocated_0' y el espacio Z80 queda vacio.

Mapa de cada vista:
  0000-3FFF  RET (stubs de la BIOS), r-x
  4000-5FFF  banco 0 (fijo)
  6000-BFFF  grupo k: bancos 3k+1, 3k+2, 3k+3 (asi los conmuta el codigo de 0x416C-0x4244)
  4000-BFFF  ROM, r-x
  C000-FFFF  RAM sin inicializar, rw-
"""
import pathlib, struct, sys

BIOS = {
    0x000C: 'RDSLT', 0x0010: 'CHRGTR', 0x0014: 'WRSLT',
    0x0018: 'OUTDO', 0x001C: 'CALSLT', 0x0020: 'DCOMPR', 0x0024: 'ENASLT', 0x0028: 'GETYPR',
    0x0030: 'CALLF', 0x0038: 'KEYINT', 0x003B: 'INITIO', 0x003E: 'INIFNK', 0x0041: 'DISSCR',
    0x0044: 'ENASCR', 0x0047: 'WRTVDP', 0x004A: 'RDVRM', 0x004D: 'WRTVRM', 0x0050: 'SETRD',
    0x0053: 'SETWRT', 0x0056: 'FILVRM', 0x0059: 'LDIRMV', 0x005C: 'LDIRVM', 0x005F: 'CHGMOD',
    0x0062: 'CHGCLR', 0x0066: 'NMI', 0x0069: 'CLRSPR', 0x006C: 'INITXT', 0x006F: 'INIT32',
    0x0072: 'INIGRP', 0x0075: 'INIMLT', 0x0078: 'SETTXT', 0x007B: 'SETT32', 0x007E: 'SETGRP',
    0x0081: 'SETMLT', 0x0084: 'CALPAT', 0x0087: 'CALATR', 0x008A: 'GSPSIZ', 0x008D: 'GRPPRT',
    0x0090: 'GICINI', 0x0093: 'WRTPSG', 0x0096: 'RDPSG', 0x0099: 'STRTMS', 0x009C: 'CHSNS',
    0x009F: 'CHGET', 0x00A2: 'CHPUT', 0x00A5: 'LPTOUT', 0x00A8: 'LPTSTT', 0x00AB: 'CNVCHR',
    0x00AE: 'PINLIN', 0x00B1: 'INLIN', 0x00B4: 'QINLIN', 0x00B7: 'BREAKX', 0x00BA: 'ISCNTC',
    0x00BD: 'CKCNTC', 0x00C0: 'BEEP', 0x00C3: 'CLS', 0x00C6: 'POSIT', 0x00C9: 'FNKSB',
    0x00CC: 'ERAFNK', 0x00CF: 'DSPFNK', 0x00D2: 'TOTEXT', 0x00D5: 'GTSTCK', 0x00D8: 'GTTRIG',
    0x00DB: 'GTPAD', 0x00DE: 'GTPDL', 0x00E1: 'TAPION', 0x00E4: 'TAPIN', 0x00E7: 'TAPIOF',
    0x00EA: 'TAPOON', 0x00ED: 'TAPOUT', 0x00F0: 'TAPOOF', 0x00F3: 'STMOTR', 0x0132: 'CHGCAP',
    0x0135: 'CHGSND', 0x0138: 'RSLREG', 0x013B: 'WSLREG', 0x013E: 'RDVDP', 0x0141: 'SNSMAT',
    0x0156: 'KILBUF',
}
RAM = {
    0xF3DF: 'RG0SAV', 0xF3E0: 'RG1SAV', 0xF3E6: 'RG7SAV', 0xF3E7: 'STATFL', 0xFCC1: 'EXPTBL',
    0xFCC5: 'SLTTBL', 0xFD9A: 'H_KEYI', 0xFD9F: 'H_TIMI', 0xFFE7: 'RG8SAV',
    0x6000: 'MAPPER_6000', 0x8000: 'MAPPER_8000', 0xA000: 'MAPPER_A000',
    0xE000: 'game_state', 0xE001: 'game_substate', 0xE002: 'game_flags', 0xE003: 'frame_counter',
    0xE004: 'state_timer', 0xE019: 'player_alive', 0xE01A: 'room_exit_dir',
    0xE008: 'input_held', 0xE009: 'input_new', 0xE041: 'world', 0xE042: 'room',
    0xE052: 'vit_aphrodite', 0xE056: 'vit_popolon', 0xE648: 'room_link_up', 0xE649: 'room_link_down',
    0xE64A: 'room_link_left', 0xE64B: 'room_link_right', 0xE680: 'room_god_obj', 0xE688: 'room_door',
    0xE689: 'room_door_state', 0xE690: 'god_room_type',
    0xE020: 'boss_summoned', 0xE02C: 'boss_defeated', 0xE031: 'key_fanfare', 0xE0D2: 'boss_phase',
    0xE615: 'boss_listen', 0xE6B0: 'typed_buffer', 0xE700: 'boss_obj', 0xE710: 'boss_hp', 0xE740: 'item_obj',
    0x5DFB: 'boss_room_table',
    0xE046: 'counter0', 0xE048: 'counter1', 0xE04A: 'counter2', 0xE06F: 'item_flags', 0xE800: 'enemy_slots',
    0xE500: 'player_action', 0xE505: 'player_y', 0xE507: 'player_x',
    0xE005: 'frame_busy', 0xE006: 'isr_busy',
    0xF0F1: 'bank_shadow_6000', 0xF0F2: 'bank_shadow_8000', 0xF0F3: 'bank_shadow_a000',
    0xF0F4: 'snd_saved_6000', 0xF0F5: 'snd_saved_8000',
}
# Solo banco 0 (4000-5FFF): es igual en todas las vistas.
GAME = {
    0x4095: 'rom_init', 0x40EF: 'isr_keyi', 0x4277: 'game_frame',
    0x4136: 'add_a_to_hl', 0x413B: 'add_a_to_de', 0x4140: 'jump_table_a',
    0x414C: 'read_word_table', 0x4155: 'read_byte_table', 0x415C: 'neg_de', 0x4164: 'neg_hl',
    0x416C: 'map_banks_01_02_03', 0x4185: 'map_banks_04_05_06', 0x419E: 'map_banks_07_08_09',
    0x41B7: 'map_banks_10_11_12', 0x41D0: 'map_banks_13_14_15',
    0x41E9: 'map_6000_bank01', 0x41F4: 'map_6000_bank04',
    0x41FF: 'map_8000_bank02', 0x420A: 'map_8000_bank14',
    0x4215: 'map_a000_bank03', 0x4220: 'map_a000_bank06', 0x422B: 'map_a000_bank09',
    0x4236: 'map_a000_bank12', 0x4241: 'map_a000_bank15',
    0x424C: 'farcall_b04_6551', 0x4257: 'farcall_b06_b41c', 0x4260: 'farcall_b08_9d5f',
    0x4269: 'farcall_b03_b389_ret_g456',
    0x435B: 'next_substate_timer', 0x435E: 'next_substate', 0x43A9: 'set_game_state',
    0x4439: 'next_game_state', 0x443B: 'next_game_state_timer', 0x4442: 'reset_substate', 0x4443: 'set_substate', 0x4BFA: 'attract_check_start',
    0x5DE4: 'in_boss_room', 0x5218: 'load_room_links', 0x5561: 'load_room_graphics',
    0x55F7: 'build_room_tilemap', 0x59F0: 'load_room_tileset', 0x5A8D: 'mirror_tiles',
    0x5AB3: 'load_tile_pieces', 0x5B08: 'bit_reverse_buffer', 0x5BEB: 'fill_vram_3thirds',
    0x5BFC: 'rle_to_vram_3thirds', 0x5960: 'load_room_sprite_types', 0x59A6: 'load_sprite_gfx',
    0x5DC9: 'find_room_item', 0x55CE: 'draw_boss_emblem', 0x58E1: 'load_item_icon',
    0x4D5A: 'add_counter0_bcd', 0x4D7B: 'add_counter1_bcd', 0x4D82: 'add_counter2_bcd', 0x5C40: 'rle_to_vram', 0x5C46: 'rle_to_vram_at',
    0x4E1F: 'sound_request', 0x4E34: 'sound_request_nocheck',
}
# Tablas inline tras 'CALL jump_table_a' en el banco 0: prefijo -> direccion de la tabla.
# La longitud se deduce: la tabla acaba donde empieza el destino mas bajo.
JUMP_TABLES = {
    'game_state': 0x428E,          # indice = game_state (E000)
    'game_state_02_sub': 0x4340,   # indice = game_substate (B)
    'game_state_16_sub': 0x48C9,
    'game_state_17_sub': 0x49BE,
}


def jump_table(rom, addr):
    w = lambda a: rom[a - 0x4000] | rom[a - 0x3FFF] << 8
    assert w(addr - 2) == 0x4140 and rom[addr - 0x4003] == 0xCD, f'{addr:04X} no sigue a CALL 4140'
    ents, end = [], 0x6000
    while addr + 2 * len(ents) < end:
        e = w(addr + 2 * len(ents))
        assert 0x4000 <= e < 0x6000, f'entrada {e:04X} fuera del banco 0'
        ents.append(e); end = min(end, e)
    return ents


IMG_OFF = 0x100


def elf_header():
    # (p_offset, p_vaddr, p_filesz, p_memsz, p_flags)  PF_X=1 PF_W=2 PF_R=4
    segs = [(IMG_OFF, 0x0000, 0x4000, 0x4000, 5),
            (IMG_OFF + 0x4000, 0x4000, 0x8000, 0x8000, 5),
            (IMG_OFF + 0xC000, 0xC000, 0, 0x4000, 6)]
    hdr = b'\x7fELF' + bytes([1, 1, 1]) + bytes(9)
    hdr += struct.pack('<HHIIIIIHHHHHH', 2, 3, 1, 0x4095, 0x34, 0, 0, 0x34, 0x20, len(segs), 0, 0, 0)
    for off, va, fsz, msz, fl in segs:
        hdr += struct.pack('<IIIIIIII', 1, off, va, va, fsz, msz, fl, 0x100)
    assert len(hdr) <= IMG_OFF
    return hdr.ljust(IMG_OFF, b'\0')


def build(rom, k):
    img = bytearray(b'\xC9' * 0x4000) + bytearray(0x8000)
    img[0x4000:0x6000] = rom[0:0x2000]
    for i in range(3):
        b = 3 * k + 1 + i
        img[0x6000 + i * 0x2000:0x8000 + i * 0x2000] = rom[b * 0x2000:(b + 1) * 0x2000]
    return elf_header() + bytes(img)


def main(rom_path, out_dir):
    rom = pathlib.Path(rom_path).read_bytes()
    assert len(rom) == 0x20000 and rom[:2] == b'AB', 'se espera ROM MSX de 128 KB'
    out = pathlib.Path(out_dir); out.mkdir(parents=True, exist_ok=True)
    for k in range(5):
        p = out / f'galious_g{k}_b{3*k+1:02d}-{3*k+3:02d}.elf'
        p.write_bytes(build(rom, k))
        print(p)
    lines = ['# seeds REA/Ghidra: address<TAB>kind[<TAB>name]']
    for a, n in sorted(BIOS.items()):
        lines += [f'0x{a:04x}\tfunction\tBIOS_{n}']
    game = dict(GAME)
    for prefix, t in JUMP_TABLES.items():
        for i, e in enumerate(jump_table(rom, t)):
            game.setdefault(e, f'{prefix}_{i:02d}')
    for a, n in sorted(game.items()):
        if n is not None:
            lines += [f'0x{a:04x}\tfunction\t{n}']
    for prefix, t in JUMP_TABLES.items():
        lines += [f'0x{t:04x}\tlabel\t{prefix}_table']
    for a, n in sorted(RAM.items()):
        lines += [f'0x{a:04x}\tlabel\t{n}']
    (out / 'seeds.tsv').write_text('\n'.join(lines) + '\n')
    print(out / 'seeds.tsv')


if __name__ == '__main__':
    main(*sys.argv[1:3])
