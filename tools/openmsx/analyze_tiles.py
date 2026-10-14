#!/usr/bin/env python3
"""Resume logs/omsx/tile_changes.tsv (explore_tiles.py) en cambios de tiles con sentido.

Descarta el ruido conocido:
  - el redibujado al cambiar de sala (la sala real no es la cargada, o el tile anterior es 00),
  - los objetos recogidos al pisarlos (cofre 76-79 o tiles propios 60-75 que pasan a fondo).
Agrupa el resto por (mapa, sala, pasada) en rectangulos de filas/columnas de la sala (sin marcador).
"""
import collections, os, sys
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'extract'))
from galious_rom import ROOT

ITEM_TILES = set(range(0x60, 0x7A))


def main():
    rows = [l.rstrip('\n').split('\t') for l in open(ROOT / 'logs' / 'omsx' / 'tile_changes.tsv')]
    groups = collections.defaultdict(list)
    for name, loaded, pas, pc, ret, e041, room, addr, old, new, py, px in rows:
        old_t, new_t = int(old, 16), int(new, 16)
        if room != loaded or old_t == 0 or old_t in ITEM_TILES:
            continue
        a = int(addr, 16) - 0x3880
        groups[(name, int(loaded), pas)].append((a // 32, a % 32, old_t, new_t, pc, ret, int(py), int(px)))
    for (name, room, pas), ch in sorted(groups.items()):
        rs, cs = [c[0] for c in ch], [c[1] for c in ch]
        olds = ' '.join(sorted({f'{c[2]:02X}' for c in ch}))
        news = ' '.join(sorted({f'{c[3]:02X}' for c in ch}))
        pcs = ' '.join(sorted({f'{c[4]}<{c[5]}' for c in ch}))
        print(f'{name:8s} sala {room:3d} pasada {pas:5s} {len(ch):3d} tiles  filas {min(rs)}-{max(rs)} cols {min(cs)}-{max(cs)}'
              f'  {olds} -> {news}  jugador y={ch[0][6]} x={ch[0][7]}  {pcs}')


if __name__ == '__main__':
    main()
