"""Descompresor RLE de Galious hacia VRAM (rutina b00:5C40).

Formato del flujo:
  dir_lo dir_hi            direccion VRAM de destino
  00                       fin
  80 dir_lo dir_hi         cambia la direccion de destino
  81..FF                   copia los siguientes (n & 7F) bytes tal cual
  01..7F b                 repite b n veces
"""


def decode(rom, ptr, group, vram=None):
    """Aplica el flujo sobre una VRAM de 16 KB (bytearray) y devuelve (vram, rangos escritos, fin)."""
    vram = vram if vram is not None else bytearray(0x4000)
    rd = lambda a: rom.byte(a, group)
    spans = []
    while True:
        dst = rd(ptr) | rd(ptr + 1) << 8
        ptr += 2
        start = dst
        while True:
            n = rd(ptr); ptr += 1
            if n == 0:
                spans.append((start, dst))
                return vram, spans, ptr
            if n == 0x80:
                spans.append((start, dst))
                break
            if n & 0x80:
                for _ in range(n & 0x7F):
                    vram[dst & 0x3FFF] = rd(ptr); ptr += 1; dst += 1
            else:
                b = rd(ptr); ptr += 1
                for _ in range(n):
                    vram[dst & 0x3FFF] = b; dst += 1
