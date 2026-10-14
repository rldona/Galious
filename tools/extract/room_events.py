#!/usr/bin/env python3
"""Eventos programados por sala: muros que se abren, escaleras que aparecen y el puente.

Las rutinas son cadenas de comparaciones (mundo, sala) escritas en codigo, no tablas, asi que
se ejecutan con un trazador Z80 minimo (solo las instrucciones que usan) para cada sala y se
anotan las escrituras en RAM. Ver docs/entities.md, "Eventos de sala".

  b04:60AE  salas con evento por mundo (E610 = 1 al cargar: los enemigos de la lista b04:6584
            que nacen en ella suman 1 a E622; b03:B6AE resta 1 al morir y, a 0, llama al evento)
  b08:9D5F  evento al limpiar la sala (mundos; en el castillo solo pone E61C = 1)
  b08:9F1A  al cargar la sala: bloque de muro 4x3 (EAC0/EAC2, EAC9) o puente (EAC8)
  b08:9E61  al cargar la sala: escalera extra (EC08..EC0B) si el bit 3 del byte de la sala esta puesto

Salida: extracted/entities/room_events.json
"""
import json
from galious_rom import Rom, ROOT

ON_CLEAR, ON_LOAD_WALL, ON_LOAD_LADDER, EVENT_ROOMS = 0x9D5F, 0x9F1A, 0x9E61, 0x60AE
NAME_BASE = 0xED00          # copia en RAM del mapa de nombres: fila = (dir - ED00) // 32
WORLDS = ['castle'] + [f'world{k:02d}' for k in range(1, 11)]


class Stop(Exception):
    pass


class Tracer:
    """Ejecuta un subconjunto de Z80 desde el banco 0 + grupo 2 (bancos 7-9) con RAM ficticia."""

    def __init__(self, rom, ram):
        self.rom, self.ram = rom, dict(ram)
        self.writes, self.calls, self.reads = [], [], set()
        self.a = self.b = self.c = self.d = self.e = self.h = self.l = 0
        self.z = self.cy = self.s = False

    def mem(self, a):
        if 0xE0A0 <= a <= 0xE0AC:
            self.reads.add(a)
        if a >= 0xC000:
            return self.ram.get(a, 0)
        if a < 0x4000:
            raise Stop(f'lectura fuera de ROM/RAM {a:04x}')
        return self.rom.byte(a, None if a < 0x6000 else 2)

    def word(self, a):
        return self.mem(a) | self.mem(a + 1) << 8

    def put(self, a, v):
        self.ram[a] = v & 0xFF
        self.writes.append((a, v & 0xFF))

    hl = property(lambda s: s.h << 8 | s.l)
    de = property(lambda s: s.d << 8 | s.e)

    def set_hl(self, v):
        self.h, self.l = v >> 8 & 0xFF, v & 0xFF

    def flags(self, r):
        self.z, self.s = (r & 0xFF) == 0, bool(r & 0x80)

    def run(self, pc, stack=None, steps=400):
        stack = stack if stack is not None else []
        for _ in range(steps):
            op = self.mem(pc)
            n, nn = self.mem(pc + 1), self.word(pc + 1)
            rel = pc + 2 + (n - 256 if n > 127 else n)
            if op == 0x7A: self.a = self.d; pc += 1
            elif op == 0x7B: self.a = self.e; pc += 1
            elif op == 0x7C: self.a = self.h; pc += 1
            elif op == 0x7D: self.a = self.l; pc += 1
            elif op == 0x79: self.a = self.c; pc += 1
            elif op == 0x68: self.l = self.b; pc += 1
            elif op == 0x2D: self.l = self.l - 1 & 0xFF; pc += 1
            elif op == 0x2C: self.l = self.l + 1 & 0xFF; pc += 1
            elif op == 0x23: self.set_hl(self.hl + 1 & 0xFFFF); pc += 1
            elif op == 0x3D: self.a = self.a - 1 & 0xFF; self.flags(self.a); pc += 1
            elif op == 0xFE: r = self.a - n; self.cy = r < 0; self.flags(r); pc += 2
            elif op == 0xD6: r = self.a - n; self.cy = r < 0; self.a = r & 0xFF; self.flags(r); pc += 2
            elif op == 0xB7: self.cy = False; self.flags(self.a); pc += 1
            elif op == 0xAF: self.a = 0; self.flags(0); self.cy = False; pc += 1
            elif op == 0x3E: self.a = n; pc += 2
            elif op == 0x0E: self.c = n; pc += 2
            elif op == 0x26: self.h = n; pc += 2
            elif op == 0x01: self.c, self.b = nn & 0xFF, nn >> 8; pc += 3
            elif op == 0x11: self.e, self.d = nn & 0xFF, nn >> 8; pc += 3
            elif op == 0x21: self.set_hl(nn); pc += 3
            elif op == 0x2A: self.set_hl(self.word(nn)); pc += 3
            elif op == 0x3A: self.a = self.mem(nn); pc += 3
            elif op == 0x32: self.put(nn, self.a); pc += 3
            elif op == 0x36: self.put(self.hl, n); pc += 2
            elif op == 0x77: self.put(self.hl, self.a); pc += 1
            elif op == 0x71: self.put(self.hl, self.c); pc += 1
            elif op == 0x72: self.put(self.hl, self.d); pc += 1
            elif op == 0x73: self.put(self.hl, self.e); pc += 1
            elif op == 0x34: self.put(self.hl, self.mem(self.hl) + 1); self.flags(self.mem(self.hl)); pc += 1
            elif op == 0x7E: self.a = self.mem(self.hl); pc += 1
            elif op == 0xEB: self.d, self.e, self.h, self.l = self.h, self.l, self.d, self.e; pc += 1
            elif op == 0x19: self.set_hl(self.hl + self.de & 0xFFFF); pc += 1
            elif op == 0xED and n == 0x5B: v = self.word(self.word(pc + 2)); self.e, self.d = v & 0xFF, v >> 8; pc += 4
            elif op == 0xED and n == 0x52:
                r = self.hl - self.de - self.cy; self.cy = r < 0; self.set_hl(r & 0xFFFF); self.z = (r & 0xFFFF) == 0; pc += 2
            elif op == 0xCB:   # BIT/SET b,(HL)
                bit = (n >> 3) & 7
                if 0x40 <= n < 0x80: self.z = not self.mem(self.hl) >> bit & 1
                elif n >= 0xC0: self.put(self.hl, self.mem(self.hl) | 1 << bit)
                else: raise Stop(f'CB {n:02x} en {pc:04x}')
                pc += 2
            elif op in (0x18, 0x28, 0x20, 0x38, 0x30):
                take = {0x18: True, 0x28: self.z, 0x20: not self.z, 0x38: self.cy, 0x30: not self.cy}[op]
                pc = rel if take else pc + 2
            elif op in (0xC3, 0xCA, 0xC2):
                take = {0xC3: True, 0xCA: self.z, 0xC2: not self.z}[op]
                pc = nn if take else pc + 3
            elif op in (0xC9, 0xC0, 0xC8, 0xF8, 0xD0, 0xD8):
                take = {0xC9: True, 0xC0: not self.z, 0xC8: self.z, 0xF8: self.s, 0xD0: not self.cy, 0xD8: self.cy}[op]
                if not take:
                    pc += 1
                elif not stack:
                    return
                else:
                    pc = stack.pop()
            elif op == 0xCD:
                if nn == 0x4050:                       # jump_table_a: tabla de saltos tras la llamada
                    pc = self.word(pc + 3 + 2 * self.a)
                elif nn == 0x4053:                     # DE = palabra [HL + 2A]
                    v = self.word(self.hl + 2 * self.a); self.e, self.d = v & 0xFF, v >> 8; pc += 3
                elif nn == 0x4059:                     # sound_request
                    self.calls.append(('sonido', self.a)); pc += 3
                else:
                    stack.append(pc + 3); pc = nn
            else:
                raise Stop(f'opcode {op:02x} en {pc:04x}')
        raise Stop('demasiados pasos')


def event_rooms(rom):
    out = {}
    for w in range(11):
        p, rooms = rom.word(EVENT_ROOMS + 2 * w, 1), []
        while (b := rom.byte(p, 1)):
            rooms.append(b); p += 1
        out[w + 1] = rooms
    return out


def cell(addr):
    off = addr - NAME_BASE
    return {'row': off // 32 - 4, 'col': off % 32}       # fila relativa a la sala (sin marcador)


def trace(rom, pc, e041, room, ram=()):
    t = Tracer(rom, {0xE041: e041, 0xE042: room, **dict(ram)})
    try:
        t.run(pc)
    except Stop as s:
        t.calls.append(('parado', str(s)))
    return t


def main():
    rom = Rom()
    rooms_by_world = event_rooms(rom)
    room_bits = 0xE300                                    # (E60E) -> byte de la sala (bits persistentes)
    out = {}
    for e041, rooms in rooms_by_world.items():
        for room in rooms:
            ev = {'world': WORLDS[e041 - 1], 'room': room, 'trigger': 'matar a los enemigos marcados de la sala'}
            if e041 == 1:
                ev['effect'] = 'E61C = 1 (castillo: hace aparecer un objeto en las salas 48, 75 y 24; b02:9F82)'
            else:
                t = trace(rom, ON_CLEAR, e041, room, {0xE60E: room_bits & 0xFF, 0xE60F: room_bits >> 8})
                ev['writes'] = {f'{a:04X}': v for a, v in t.writes}
                w = dict(t.writes)
                if any(0xE0A2 <= a <= 0xE0AC for a in w):
                    ev['effect'] = 'abre un bloque de muro'
                elif w.get(room_bits, 0) & 8:
                    ev['effect'] = 'escalera en esta sala'
                elif w:                                   # 9E4E: bit 3 del byte de otra sala del mundo
                    a = next(iter(w))
                    base = rom.word(0xAF09 + 2 * e041, 2) + 0x150
                    ev['effect'] = f'escalera en la sala {a - base + 1}'
                ev['sound'] = [v for k, v in t.calls if k == 'sonido']
            out.setdefault(WORLDS[e041 - 1], []).append(ev)
    # al cargar: muros (con su flag a 0), puente (con el objeto 0x12) y escaleras (con el bit 3)
    loads = []
    for e041 in range(1, 12):
        for room in range(1, 0x30):
            t = trace(rom, ON_LOAD_WALL, e041, room, {0xE081: 1, 0xE60E: room_bits & 0xFF, 0xE60F: room_bits >> 8})
            w = dict(t.writes)
            if 0xEAC0 in w:
                flag = max(t.reads)
                opener = [f"{ev['world']}:{ev['room']}" for evs in out.values() for ev in evs if f'{flag:04X}' in ev.get('writes', {})]
                loads.append({'world': WORLDS[e041 - 1], 'room': room, 'kind': 'muro 4x3',
                              'pos': cell(w[0xEAC2] | w[0xEAC3] << 8), 'flag': f'{flag:04X}',
                              'opened_by': f'matar a los enemigos marcados de {opener[0]}' if opener else '?'})
            if 0xEAC9 in w:
                loads.append({'world': WORLDS[e041 - 1], 'room': room, 'kind': 'muro 4x3',
                              'pos': {'row': 0, 'col': 28}, 'flag': 'E0A1',   # posicion fija en b01:735F
                              'opened_by': 'subir 5 veces seguidas por el pozo de la sala (b00:479C)'})
            if 0xEAC8 in w:
                loads.append({'world': WORLDS[e041 - 1], 'room': room, 'kind': 'puente (con el objeto 0x12)',
                              'pos': {'row': 7, 'col': 8}})
            t = trace(rom, ON_LOAD_LADDER, e041, room, {0xE60E: room_bits & 0xFF, 0xE60F: room_bits >> 8, room_bits: 8})
            w = dict(t.writes)
            if 0xEC08 in w:
                wn = WORLDS[e041 - 1]
                opener = [f"{wn}:{ev['room']}" for ev in out.get(wn, [])
                          if ev.get('effect') in ('escalera en esta sala' if ev['room'] == room else '', f'escalera en la sala {room}')]
                loads.append({'world': wn, 'room': room, 'kind': 'escalera',
                              'pos': cell(w[0xEC0A] | w[0xEC0B] << 8), 'height': w[0xEC09],
                              'opened_by': f'matar a los enemigos marcados de {opener[0]}' if opener else '?'})
    (ROOT / 'extracted' / 'entities' / 'room_events.json').write_text(
        json.dumps({'on_clear': out, 'on_load': loads}, indent=1, ensure_ascii=False))
    for wname, evs in out.items():
        for ev in evs:
            print(f"{wname:8s} sala {ev['room']:3d}: {ev.get('effect', '?'):26s} {ev.get('writes', '')}")
    for l in loads:
        print(f"al cargar {l['world']:8s} sala {l['room']:3d}: {l['kind']:28s} {l['pos']} {l.get('height', '')} {l.get('opened_by', '')}")


if __name__ == '__main__':
    main()
