#!/usr/bin/env python3
"""Lista y extrae los ficheros de una imagen de disco MSX-DOS (FAT12)."""
import struct, sys, pathlib

def main(img_path, out_dir=None):
    d = pathlib.Path(img_path).read_bytes()
    bps, spc, rsv, nfat, nroot, tot, media, spf = struct.unpack_from('<HBHBHHBH', d, 11)
    print(f'OEM={d[3:11]!r} bps={bps} spc={spc} reserved={rsv} fats={nfat} root={nroot} '
          f'sectors={tot} media={media:#x} spf={spf}')
    fat_off = rsv * bps
    root_off = fat_off + nfat * spf * bps
    data_off = root_off + nroot * 32
    csize = spc * bps
    fat = d[fat_off:fat_off + spf * bps]

    def next_cluster(c):
        v = struct.unpack_from('<H', fat, c * 3 // 2)[0]
        return (v >> 4) if c & 1 else (v & 0xFFF)

    for i in range(nroot):
        e = d[root_off + i * 32: root_off + i * 32 + 32]
        if e[0] == 0:
            break
        if e[0] == 0xE5 or e[11] & 0x08:
            continue
        name = e[:8].decode('latin1').rstrip() + '.' + e[8:11].decode('latin1').rstrip()
        size = struct.unpack_from('<I', e, 28)[0]
        c = struct.unpack_from('<H', e, 26)[0]
        data = bytearray()
        while 2 <= c < 0xFF8 and len(data) < size:
            o = data_off + (c - 2) * csize
            data += d[o:o + csize]
            c = next_cluster(c)
        data = bytes(data[:size])
        print(f'{name:14} {size:7d} bytes  attr={e[11]:#04x}  head={data[:8].hex(" ")}')
        if out_dir:
            p = pathlib.Path(out_dir); p.mkdir(parents=True, exist_ok=True)
            (p / name.rstrip('.')).write_bytes(data)

if __name__ == '__main__':
    main(*sys.argv[1:])
