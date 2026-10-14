#!/usr/bin/env python3
"""Catalogo de enemigos y objetos recogibles, desde la ROM (ver docs/items_enemies.md).

Enemigos: descriptor de 5 bytes en b04:67B6 + 5*tipo, copiado al crear el enemigo (b04:65DA)
en su hueco de RAM (E800 + 0x20*n):
  d0 -> IX+9, d1 -> IX+0A, d2 -> IX+0B, d3 -> IX+10 (vida; FE/FF = no se puede danar),
  d4 -> IX+11 (clase de recompensa: 647B[d4] categoria, 4 = nada; 64E5[d4] -> nivel de botin)
Objetos: icono 16x16 (b00:58E1: patrones 6E34[tipo], colores 6E80[tipo], banco 10), efecto al
recogerlo (b03:B701) y comprobaciones conocidas de su flag E06F + tipo.

Salida: extracted/entities/enemies.json, items.json
"""
import json
from collections import defaultdict
from galious_rom import Rom, ROOT

ENEMY_DESC, REWARD_CAT, REWARD_TIER = 0x67B6, 0x647B, 0x64E5
OUT = ROOT / 'extracted' / 'entities'

# Objeto -> enemigo contra el que protege (b03:B1F7..B22D: sin dano al tocarlo)
IMMUNITY = {0x24: 0x08, 0x0E: 0x2A, 0x0F: 0x28, 0x17: 0x2E, 0x22: 0x31}

ITEM_NOTES = {
    0x07: 'objeto del mundo: activa E061+mundo bit 5; se copia a su flag al entrar en el mundo (b00:463C)',
    0x08: 'objeto del mundo: bit 6', 0x09: 'objeto del mundo: bit 7', 0x0A: 'objeto del mundo: bit 4',
    0x0C: 'llave: desbloquea la puerta del mundo siguiente (E062+mundo bit 1); fanfarria (estado 20 en un mundo)',
    0x0D: 'necesario para cruzar la puerta 7 con el personaje E501=1 (b02:9A41)',
    0x23: 'necesario para cruzar la puerta 5 con el personaje E501=1 (b02:9A33)',
    0x2A: 'necesario para entrar en la sala de dios de tipo 3 (castillo, sala 87; b02:9A84)',
    0x15: 'comprobado por b02:9AC9 desde el menu de objetos (estado 11)',
    0x25: 'mejora de arma: cada golpe resta 1 de vida extra (b03:B580..B5DB)',
    0x2B: 'escudo de nivel 2 (E097 = 2)', 0x2C: 'escudo de nivel 3 (E097 = 3)',
    0x2D: 'recurso: +50 al contador E046 (primera fila del marcador)',
    0x2E: 'recurso: +10 al contador E048 (segunda fila)',
    0x2F: 'recurso: +2 al contador E04A (tercera fila)',
    0x01: 'arma secundaria: flecha (menu de objetos; E510 = arma elegida) [O*]',
    0x02: 'arma secundaria: flecha doble (ceramica) [O*]',
    0x03: 'arma secundaria: fuego rodante [O*]',
    0x04: 'arma secundaria: fuego [O*]',
    0x12: 'hace aparecer el puente de la sala 11 del mundo 2 (EAC8, b08:9F87) [O*]',
    0x10: 'mundo 5, salas 12-13: el agua (tile B6) no te hunde (b03:B037) [C]',
    0x11: 'escaleras un 25 % mas rapido (b03:A6B6) [O*]',
    0x13: 'sin el, en el mundo 8 izquierda y derecha van al reves (b03:A8A6) [O*]',
    0x14: 'en el castillo suena al entrar en ciertas salas, como pista (b00:5D24) [C]',
    0x15: 'en el menu de objetos, Return devuelve al castillo (b02:9AC9) [O*]',
    0x16: 'sin ella, la sala 6 del mundo 5 esta a oscuras (b00:55F7) [O*]',
    0x18: 'apaga generadores periodicos de enemigos en los mundos 7-10 (b04:6200) [C]',
    0x19: 'recibes la mitad de dano (b03:B3BF) [C]',
    0x1A: 'en los mundos coloca un sprite indicador desde una tabla por mundo (b01:668F) [C][I]',
    0x1B: 'duplica la experiencia ganada (b03:B62E) [C]',
    0x1F: 'Aphrodite rompe piedras como Popolon: 5 por golpe en vez de 1 (b03:B841) [O*]',
    0x20: 'CTRL congela a los enemigos ~5 s, hasta 16 veces (b02:9BB6) [O*]',
    0x21: 'un golpe de espada rompe todas las piedras de la sala (b01:6398) [O*]',
    0x26: 'premio de la secuencia de direcciones del castillo; con el, escribir UMBRELLA activa algo (b02:9BEE) [C]',
    0x27: 'en el menu de objetos, el numero de un mundo abierto te lleva a su puerta (b02:9A99) [O*]',
    0x28: 'escudo de nivel 1 (E097) [C]',
    0x29: 'bloquea proyectiles por delante a la altura del jugador (b09:A345) [C][I]',
}
for item, enemy in IMMUNITY.items():
    ITEM_NOTES[item] = f'proteccion: el enemigo {enemy:#04x} no hace dano al tocarlo'


def rle(rom, ptr, group=3):
    out = bytearray()
    while True:
        n = rom.byte(ptr, group); ptr += 1
        if n == 0:
            return bytes(out)
        if n == 0x80:
            ptr += 2
            continue
        if n & 0x80:
            out += rom.block(ptr, n & 0x7F, group); ptr += n & 0x7F
        else:
            out += bytes([rom.byte(ptr, group)]) * n; ptr += 1


def main():
    rom = Rom()
    ent = json.loads((OUT / 'entities.json').read_text())
    spawn_where, item_where = defaultdict(list), defaultdict(list)
    for world, rooms in ent.items():
        for room, e in rooms.items():
            for s in e['spawns']:
                spawn_where[s['type']].append(f'{world}:{room}')
            if e['item']:
                item_where[e['item']['type']].append(f'{world}:{room}')
    enemies = {}
    for t in sorted(spawn_where):
        d = rom.block(ENEMY_DESC + 5 * t, 5, 1)
        hp = d[3]
        enemies[f'{t:#04x}'] = {
            'descriptor': d.hex(' '),
            'hp': None if hp >= 0xFE else hp,
            'invulnerable': hp >= 0xFE,
            'reward_class': d[4],
            'reward_category': rom.byte(REWARD_CAT + d[4], 1),
            'reward_tier_value': rom.byte(REWARD_TIER + d[4], 1),
            'sprite_sheet': f'sprites_{t:02x}.png' if (OUT / f'sprites_{t:02x}.png').exists() else None,
            'immune_with_item': next((f'{i:#04x}' for i, e in IMMUNITY.items() if e == t), None),
            'spawns': len(spawn_where[t]),
            'worlds': sorted({w.split(':')[0] for w in spawn_where[t]}),
        }
    items = {}
    for t in sorted(set(item_where) | set(ITEM_NOTES) | set(range(1, 5)) | set(range(7, 0x2D))):
        items[f'{t:#04x}'] = {
            'icon_index': t - 7 if 7 <= t < 0x2D else None,
            'rooms': item_where.get(t, []),
            'effect': ITEM_NOTES.get(t),
        }
    # comportamiento observado (tools/openmsx/trace_enemies.py + analyze_enemies.py)
    beh = ROOT / 'logs' / 'omsx' / 'enemies' / 'summary.json'
    if beh.exists():
        summary = json.loads(beh.read_text())
        for k, v in enemies.items():
            v['observed'] = summary.get(k, {}).get('movement')
            v['observed_near_player'] = summary.get('near ' + k, {}).get('movement')
            v['children'] = summary.get(k, {}).get('children')
            v['trace_room'] = summary.get(k, {}).get('room')
    (OUT / 'enemies.json').write_text(json.dumps(enemies, indent=1))
    (OUT / 'items.json').write_text(json.dumps(items, indent=1))
    print(f'{len(enemies)} tipos de enemigo, {len(items)} tipos de objeto')
    for k, v in enemies.items():
        print(k, 'vida', v['hp'] if not v['invulnerable'] else 'invulnerable', 'recompensa', v['reward_class'],
              'generadores', v['spawns'], 'mundos', len(v['worlds']), 'inmune con', v['immune_with_item'])


if __name__ == '__main__':
    main()
