"""Construye desde la ROM los juegos de tiles (SCREEN 2) que usan las salas.

Lo que hace el juego al cargar una sala (b00:57A4, b00:59F0), con todas las listas en el
banco 10 (grupo 3):
  - lista base de piezas en 6000 (formato de 6 bytes, ver apply_pieces)
  - lista de espejos en 6037 (formato de 4 bytes, ver apply_mirrors)
  - mundos (E041 > 1): lista de piezas 6044
  - castillo: la zona de la sala (medio byte en 60D7) elige combinaciones de listas
    (jump table de b00:5A1B)
Cada pieza descomprime un flujo RLE de patrones y otro de colores (vram_rle.py) en uno o
varios de los tres tercios de la pantalla. Aqui solo se guarda el tercio 0: las salas
cargan lo mismo en los tres.
"""
from galious_rom import Rom

G = 3                        # bancos 10, 11, 12
ZONE_TABLE = 0x60D7
# Listas por zona del castillo (b00:5A2C..5A80): ('p', lista de piezas) / ('m', lista de espejos)
CASTLE_ZONES = None          # se lee del codigo en zones()


def _rle(rom, ptr, dst, out):
    """Decodifica un flujo RLE que empieza sin direccion (entrada b00:5C46)."""
    while True:
        n = rom.byte(ptr, G); ptr += 1
        if n == 0:
            return
        if n == 0x80:
            dst = rom.word(ptr, G); ptr += 2
            continue
        if n & 0x80:
            for _ in range(n & 0x7F):
                out[dst & 0x3FFF] = rom.byte(ptr, G); ptr += 1; dst += 1
        else:
            b = rom.byte(ptr, G); ptr += 1
            for _ in range(n):
                out[dst & 0x3FFF] = b; dst += 1


def apply_pieces(rom, vram, ix):
    """Entradas de 6 bytes: mascara de tercios, puntero patrones, tile destino, puntero colores."""
    while (mask := rom.byte(ix, G)):
        pat, tile, col = rom.word(ix + 1, G), rom.byte(ix + 3, G), rom.word(ix + 4, G)
        for bit, off in ((1, 0x1000), (2, 0x800), (4, 0)):
            if mask & bit:
                _rle(rom, pat, 0x2000 + off + tile * 8, vram)
                _rle(rom, col, off + tile * 8, vram)
        ix += 6


def apply_mirrors(rom, vram, ix):
    """Entradas de 4 bytes: tile origen, tile destino, flags, numero de tiles.
    Copia n tiles del tercio 0 invirtiendo los bits de cada byte de patron (espejo horizontal)."""
    while (src := rom.byte(ix, G)):
        dst, _flags, n = rom.byte(ix + 1, G), rom.byte(ix + 2, G), rom.byte(ix + 3, G)
        for i in range(n * 8):
            b = vram[0x2000 + src * 8 + i]
            vram[0x2000 + dst * 8 + i] = int(f'{b:08b}'[::-1], 2)
            vram[dst * 8 + i] = vram[src * 8 + i]
        ix += 4


def zones(rom):
    """Lee de b00:5A2C.. las listas que carga cada zona del castillo."""
    table = [rom.word(0x5A1E + 2 * i) for i in range(7)]
    out = []
    for a in table:
        steps = []
        while True:
            assert rom.block(a, 2) == b'\xdd\x21', hex(a)
            ix = rom.word(a + 2)
            op, target = rom.byte(a + 4), rom.word(a + 5)
            steps.append(('p' if target == 0x5AB3 else 'm', ix))
            a += 7
            if op == 0xC3:
                break
        out.append(steps)
    return out


def castle_zone(rom, room):
    b = rom.byte(ZONE_TABLE + room // 2, G)
    return b & 0x0F if room & 1 else b >> 4


def tileset(rom, e041, room=None):
    """Devuelve (patrones, colores): 2 KB cada uno, 256 tiles del tercio 0."""
    vram = bytearray(0x4000)
    apply_pieces(rom, vram, 0x6000)
    apply_mirrors(rom, vram, 0x6037)
    if e041 > 1:
        apply_pieces(rom, vram, 0x6044)
    else:
        for kind, ix in zones(rom)[castle_zone(rom, room)]:
            (apply_pieces if kind == 'p' else apply_mirrors)(rom, vram, ix)
    return bytes(vram[0x2000:0x2800]), bytes(vram[0:0x800])


if __name__ == '__main__':
    r = Rom()
    for i, z in enumerate(zones(r)):
        print('zona', i, [(k, hex(ix)) for k, ix in z])
