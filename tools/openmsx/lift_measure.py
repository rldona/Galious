#!/usr/bin/env python3
"""Mide los ascensores (docs/entities.md, "Ascensores") registrando la RAM frame a frame.

  OMSX_SOCK=logs/omsx/lift.sock omsx.py serve -cart <rom> &
  OMSX_SOCK=logs/omsx/lift.sock lift_measure.py demo0            (demo 0 desde el encendido)
  OMSX_SOCK=logs/omsx/lift.sock lift_measure.py room N SEG [teclas@frame,...]   (castillo, sala N, desde galious_start)

Una linea por frame en logs/omsx/lift/<nombre>.tsv.gz: frame, E000 E001 E003 E042 E500 E505 E507 E519 E52B E52C
E52D, EC10..EC2F y E4A0..E4FF. Capturas cada 30 frames en logs/omsx/lift/<nombre>_fNNNNN.png.
"""
import gzip, os, sys, time
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import RUN, send

OUT = os.path.join(RUN, 'lift')
VARS = ['E000', 'E001', 'E003', 'E042', 'E500', 'E505', 'E507', 'E519', 'E52B', 'E52C', 'E52D'] + \
       [f'{a:04X}' for a in range(0xEC10, 0xEC30)] + [f'{a:04X}' for a in range(0xE4A0, 0xE500)]
KEYS = {'space': (8, 0x01), 'left': (8, 0x10), 'right': (8, 0x80), 'up': (8, 0x20), 'down': (8, 0x40)}

TCL = r'''
proc lf_tick {} {
  incr ::lf_f
  if {$::lf_f >= $::lf_from} {
    set row $::lf_f
    foreach a {%VARS%} { append row " " [peek 0x$a] }
    lappend ::lf_log $row
    if {$::lf_f %% 30 == 0} { screenshot -raw [format "%OUT%_f%%05d.png" $::lf_f] }
  }
  foreach k $::lf_keys { lassign $k f r m d
    if {$::lf_f == $f} { keymatrixdown $r $m }
    if {$::lf_f == $f + $d} { keymatrixup $r $m } }
  if {$::lf_f < $::lf_max} { after frame lf_tick } else { set ::lf_done 1; set pause on }
}
'''


def run(name, start, frames, keys=()):
    os.makedirs(OUT, exist_ok=True)
    send('set renderer SDLGL-PP')
    send('set throttle on')
    send(TCL.replace('%VARS%', ' '.join(VARS)).replace('%OUT%', os.path.join(OUT, name)).replace('%%', '%'))
    ks = ' '.join(f'{{{f} {KEYS[k][0]} {KEYS[k][1]} {d}}}' for k, f, d in keys)
    send(f'set ::lf_keys [list {ks}]')
    from_f = start()
    send(f'set ::lf_f 0; set ::lf_log {{}}; set ::lf_from {from_f}; set ::lf_max {frames}; set ::lf_done 0')
    send('set pause off; after frame lf_tick')
    while send('set ::lf_done')[1] != '1':
        time.sleep(1)
    log = send('join $::lf_log "\\n"')[1]
    with gzip.open(os.path.join(OUT, name + '.tsv.gz'), 'wt') as fh:
        fh.write('frame ' + ' '.join(VARS) + '\n' + log + '\n')
    print(len(log.splitlines()), 'frames ->', OUT)


def main(a):
    if a[0] == 'demo0':
        def start():
            send('set power off'); send('set power on')
            return 880
        run('demo0', start, 920 + 2500)
    else:
        room, secs = int(a[1]), float(a[2])
        keys = []
        for k in (a[3].split(',') if len(a) > 3 else []):
            name, rest = k.split('@')
            f, _, d = rest.partition('+')
            keys.append((name, int(f), int(d or 6)))

        def start():
            send('loadstate galious_start')
            # viaje: sala en E64B y modo de entrada por la izquierda (scan_rooms.goto_room)
            send(f'poke 0xE64B {room}; poke 0xE01A 4; poke 0xE001 0; poke 0xE000 10')
            return 0
        run(f'room{room}', start, int(secs * 60), keys)


if __name__ == '__main__':
    main(sys.argv[1:])
