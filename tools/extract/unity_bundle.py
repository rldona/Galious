#!/usr/bin/env python3
"""Paquete de datos para el importador de Unity (fase 5).

Junta, por mapa, lo que el importador necesita de cada sala en un JSON de arrays planos (legible con
JsonUtility, sin diccionarios):
  - posicion en la rejilla del mapa (world_maps.py) y enlaces arriba/abajo/izquierda/derecha. Las salas a las
    que no se llega por enlaces (sala de dios de cada mundo, castillo 94) van en una columna aparte, placed = false;
  - cuadricula de colision de 20 x 32 (collision.py), y la de combate en la sala del jefe. La ultima sala de
    cada mundo no tiene disposicion: es una sala de dios (estado 14), shrine = true y colision vacia;
  - enemigos, objetos de sala, objeto, piedras y escaleras extra (entities.py);
  - puertas de mundo y salas de dioses del castillo.

  unity_bundle.py   -> extracted/unity/<mapa>.json y extracted/unity/enemies.json (catalogo de enemigos)

Todo sale de la ROM del usuario y se queda en local: extracted/ no se versiona.

Enlaces: -1 = sin salida (255 en la ROM); 0xF5-0xFE = salida al castillo desde la sala 1 de un mundo
(ver world_maps.md). En la rejilla, y crece hacia arriba (la ROM la guarda hacia abajo).
"""
import json
from galious_rom import Rom, ROOT

DAMAGE_TABLE = 0x64E5   # b04: dano por contacto segun el tipo (b03:B39A; el Hat Helmet lo reduce a la mitad)

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
    enemies = [{'type': int(k, 16), 'hp': v['hp'], 'invulnerable': v['invulnerable'],
                'damage': rom.byte(DAMAGE_TABLE + int(k, 16), 1),
                'immuneItem': int(v['immune_with_item'], 16) if isinstance(v.get('immune_with_item'), str)
                else (v.get('immune_with_item') or 0)}
               for k, v in sorted(cat.items())]
    (out_dir / 'enemies.json').write_text(json.dumps({'enemies': enemies}))
    print(f'catalogo: {len(enemies)} tipos de enemigo')


if __name__ == '__main__':
    main()
