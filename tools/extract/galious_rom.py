"""Acceso a la ROM de Galious (mapper Konami, 16 bancos de 8 KB) con direcciones Z80.

El banco 0 esta fijo en 4000-5FFF. El resto del codigo conmuta los bancos en grupos de
tres (3k+1, 3k+2, 3k+3 en 6000, 8000, A000), asi que una direccion de 6000-BFFF solo
tiene sentido junto al grupo mapeado.
"""
import pathlib

ROOT = pathlib.Path(__file__).resolve().parents[2]
ROM_PATH = next((ROOT / 'rom').glob('*.rom'))


class Rom:
    def __init__(self, path=ROM_PATH):
        self.data = pathlib.Path(path).read_bytes()
        assert len(self.data) == 0x20000

    def offset(self, addr, group=None):
        """Offset en el fichero de una direccion Z80, con el grupo de bancos k mapeado."""
        if 0x4000 <= addr < 0x6000:
            return addr - 0x4000
        assert group is not None and 0x6000 <= addr < 0xC000, hex(addr)
        slot = (addr - 0x6000) // 0x2000
        return (3 * group + 1 + slot) * 0x2000 + (addr & 0x1FFF)

    def byte(self, addr, group=None):
        return self.data[self.offset(addr, group)]

    def word(self, addr, group=None):
        return self.byte(addr, group) | self.byte(addr + 1, group) << 8

    def block(self, addr, n, group=None):
        o = self.offset(addr, group)
        return self.data[o:o + n]
