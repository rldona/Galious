#!/usr/bin/env python3
"""Paquete de datos para el importador de Unity (fase 5).

Junta, por mapa, lo que el importador necesita de cada sala en un JSON de arrays planos (legible con
JsonUtility, sin diccionarios):
  - posicion en la rejilla del mapa (world_maps.py) y enlaces arriba/abajo/izquierda/derecha. Las salas a las
    que no se llega por enlaces (sala de dios de cada mundo, castillo 94) van en una columna aparte, placed = false;
  - cuadricula de colision de 20 x 32 (collision.py), y la de combate en la sala del jefe. La ultima sala de
    cada mundo no tiene disposicion: es una sala de dios (estado 14), shrine = true y colision vacia;
  - enemigos, objetos de sala, objeto, piedras y escaleras extra (entities.py);
  - puertas de mundo y salas de dioses del castillo;
  - generadores periodicos de enemigos (periodic.py), en rooms[].periodic.

  unity_bundle.py   -> extracted/unity/<mapa>.json y extracted/unity/enemies.json (catalogo de enemigos)

Todo sale de la ROM del usuario y se queda en local: extracted/ no se versiona.

Enlaces: -1 = sin salida (255 en la ROM); 0xF5-0xFE = salida al castillo desde la sala 1 de un mundo
(ver world_maps.md). En la rejilla, y crece hacia arriba (la ROM la guarda hacia abajo).
"""
import json
from galious_rom import Rom, ROOT
from periodic import periodic_for

DAMAGE_TABLE = 0x64E5   # b04: dano por contacto segun el tipo (b03:B39A; el Hat Helmet lo reduce a la mitad)
LOOT_RULE = 0x647B      # b04: regla de botin segun el tipo (b06:B548), ver docs/items_enemies.md "Muerte y botin"
EXP_TABLE = 0xBF27      # b03: experiencia al matarlo (b03:B624; x2 con el Vase)
LOOT_KILLS = {0: [1, 3], 1: [2, 5], 2: [4, 6]}   # regla -> muertes del tipo en la sala que dan tirada (b06:B59D)
BIG_DEATH = {0x15, 0x16, 0x40, 0x42, 0x4C}       # b03:B645: 4 explosiones (b04:69D2), cada una con su tirada

MAPS = ['castle'] + [f'world{k:02d}' for k in range(1, 11)]
DIRS = ('up', 'down', 'left', 'right')


def xy(d, **extra):
    out = {'x': d['x'], 'y': d['y']}
    out.update(extra)
    return out


def main():
    ent = json.loads((ROOT / 'extracted' / 'entities' / 'entities.json').read_text())
    out_dir = ROOT / 'extracted' / 'unity'
    out_dir.mkdir(parents=True, exist_ok=True)
    total = 0
    for name in MAPS:
        m = json.loads((ROOT / 'extracted' / 'maps' / f'{name}.json').read_text())
        col = json.loads((ROOT / 'extracted' / 'maps' / 'collision' / f'{name}.json').read_text())
        doors = m.get('doors', {})
        gods = m.get('gods', {})
        rooms = []
        placed = [m['rooms'][k]['pos'] for k in m['rooms']]
        spare_x = max(p[0] for p in placed) + 2            # columna aparte para las salas sin enlaces
        spare_y = -min(p[1] for p in placed)
        for rid in sorted(set(col) | set(m['rooms']), key=int):
            r = m['rooms'].get(rid)
            shrine = rid not in col                        # ultima sala de un mundo: sala de dios (estado 14)
            if r is None:                                  # sala sin enlaces (dioses, castillo 94)
                r = {'pos': [spare_x, -spare_y], 'links': dict.fromkeys(DIRS, 255)}
                spare_y -= 1
            e = ent[name].get(rid, {})
            room = {
                'id': int(rid),
                'placed': rid in m['rooms'],
                'gridX': r['pos'][0],
                'gridY': -r['pos'][1],
                'shrine': shrine,
                'collision': [] if shrine else col[rid]['rows'],
                'collisionBoss': [] if shrine else col[rid].get('rows_boss_fight', []),
                'collisionNoLadders': [] if shrine else col[rid].get('rows_without_ladders', []),
                'spawns': [xy(s, type=s['type']) for s in e.get('spawns', [])],
                'objects': [xy(o, type=o['type']) for o in e.get('objects', [])],
                'hasItem': bool(e.get('item')),
                'item': xy(e['item'], type=e['item']['type']) if e.get('item') else {'x': 0, 'y': 0, 'type': 0},
                'rocks': [xy(k) for k in e.get('rocks', [])],
                # escaleras extra: modo 1 se retira al subirla, 2 al bajarla, 3 en los dos casos (entities.md)
                'ladders': [xy(l, height=l['height'], mode=l['flags'][1]) for l in e.get('ladders', [])],
                'doorWorld': doors.get(rid, {}).get('world', 0),
                'door': xy(doors[rid]) if rid in doors else {'x': 0, 'y': 0},
                'godType': gods.get(rid, {}).get('type', 0),
                'godName': gods.get(rid, {}).get('who', ''),
                'god': xy(gods[rid]) if rid in gods else {'x': 0, 'y': 0},
                # generadores periodicos (b04:61A8-647B): campos documentados en periodic.py; [] si no hay
                'periodic': periodic_for(m['e041'], int(rid)),
            }
            for d in DIRS:
                link = r['links'][d]
                room[d] = -1 if link == 255 else link
            rooms.append(room)
        bundle = {
            'map': name,
            'e041': m['e041'],
            'bossRoom': m['boss_room'] or 0,
            'bossWord': m['boss_word'] or '',
            'rooms': rooms,
        }
        (out_dir / f'{name}.json').write_text(json.dumps(bundle, ensure_ascii=False))
        total += len(rooms)
        print(f'{name}: {len(rooms)} salas')
    print(f'total: {total} salas -> {out_dir}')
    rom = Rom()
    cat = json.loads((ROOT / 'extracted' / 'entities' / 'enemies.json').read_text())
    # Todos los tipos (también los hijos: proyectiles y enemigos que crea otro): vida del descriptor b04:67B6
    # (FE/FF = invulnerable), daño de b04:64E5 y objeto que protege (solo se conoce para los de las salas).
    enemies = []
    for t in range(1, 108):
        hp = rom.block(0x67B6 + 5 * t, 5, 1)[3]
        v = cat.get(f'{t:#04x}', {})
        immune = v.get('immune_with_item')
        damage = rom.byte(DAMAGE_TABLE + t, 1)
        rule = rom.byte(LOOT_RULE + t, 1) if t < 0x6B else 4     # 0x6B (6B de 647B) no muere: sin botin
        enemies.append({'type': t, 'hp': 0 if hp >= 0xFE else hp, 'invulnerable': hp >= 0xFE,
                        'damage': damage,
                        'immuneItem': int(immune, 16) if isinstance(immune, str) else (immune or 0),
                        # Botin (b04:6A3B): lootRule 0-3 hay tirada, 4 nunca. Con 0-2, en las muertes lootKills
                        # (contadas por tipo y por visita a la sala; solo se cuentan 2 tipos distintos por sala);
                        # con 3 (lootWhenLastAlive), cuando no queda otro vivo de su tipo.
                        'lootRule': rule,
                        'lootKills': LOOT_KILLS.get(rule, []),
                        'lootWhenLastAlive': rule == 3,
                        # lootLevel 0-3 (del dano por contacto: 0-2, 3-5, 6-8, >=9) -> indice de las tablas 'loot'
                        'lootLevel': 0 if damage < 3 else 1 if damage < 6 else 2 if damage < 9 else 3,
                        # explosiones al morir (cada una hace su tirada): 4 los grandes, 0 el 0x5C, 1 el resto
                        'deathExplosions': 4 if t in BIG_DEATH else 0 if t == 0x5C else 1,
                        'experience': rom.byte(EXP_TABLE + t, 0)})
    # Tirada (b04:6A77): v = (R + E604) & 15, uniforme 0-15. Con el nivel n: v < coinBelow[n] -> monedas (0x5E),
    # si no v < arrowBelow[n] -> flechas (0x5D), si no v < keyBelow[n] -> llaves (0x5F); si no, nada.
    # Cantidades BCD por nivel (6A6B); los contadores topan en 999. El objeto nace en (y & 0xF8, x & 0xF8),
    # es un bloque 16x16 de tiles quieto (no cae ni caduca).
    th = [[rom.byte(0x6A9E + 3 * n + i, 1) for i in range(3)] for n in range(4)]
    amount = lambda k: [int(f'{rom.byte(0x6A6B + 4 * k + n, 1):x}') for n in range(4)]
    loot = {'coinBelow': [x[0] for x in th], 'arrowBelow': [x[1] for x in th], 'keyBelow': [x[2] for x in th],
            'arrowAmount': amount(0), 'coinAmount': amount(1), 'keyAmount': amount(2),
            'arrowObject': 0x5D, 'coinObject': 0x5E, 'keyObject': 0x5F, 'counterCap': 999}
    (out_dir / 'enemies.json').write_text(json.dumps({'enemies': enemies, 'loot': loot}))
    print(f'catalogo: {len(enemies)} tipos de enemigo')


if __name__ == '__main__':
    main()
