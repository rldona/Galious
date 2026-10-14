#!/usr/bin/env python3
"""Mide en openMSX el bucle de atraccion (estados 0-3) desde el encendido (docs/attract.md).

  OMSX_SOCK=logs/omsx/attract.sock omsx.py serve -cart <rom> &      (servidor propio)
  OMSX_SOCK=logs/omsx/attract.sock attract_measure.py [segundos] [pulsar]

Desde el encendido registra una linea por frame de video (60 Hz) en logs/omsx/attract/frames.tsv.gz:
frame, E000 E001 E004 E003, E019, E041 E042, E505 E507 E500 E501, E009, E00B E00C E022 E023 E025, E02E E033,
E604, E1C7 (musica), F01F F020 F031 F02B. Las peticiones de sonido (b00:4E1F, A) van a sounds.tsv.
Hace una captura en cada cambio de E000/E001 (+3 frames) y cada 60 frames: logs/omsx/attract/fNNNNN_sS_U.png.
Con 'pulsar' = 'space@F' o 'left@F' (frame F), pulsa la tecla 6 frames en ese frame (prueba de teclas);
con 'E023=1@F' escribe ese byte en el frame F (p. ej. E023 = 1 antes de la demo: se ve la segunda demo).
"""
import gzip, os, sys, time
sys.path.insert(0, __file__.rsplit('/', 1)[0])
from omsx import RUN, send

OUT = os.path.join(RUN, 'attract')
VARS = ['E000', 'E001', 'E004', 'E003', 'E019', 'E041', 'E042', 'E505', 'E507', 'E500', 'E501', 'E009', 'E00B',
        'E00C', 'E022', 'E023', 'E025', 'E02E', 'E033', 'E604', 'E1C7', 'F01F', 'F020', 'F031', 'F02B']
KEYS = {'space': (8, 0x01), 'left': (8, 0x10), 'right': (8, 0x80), 'L': (4, 0x02), 'F5': (7, 0x02)}

TCL = r'''
proc at_tick {} {
  incr ::at_f
  set row $::at_f
  foreach a {%VARS%} { append row " " [peek 0x$a] }
  lappend ::at_log $row
  set s "[peek 0xE000]_[peek 0xE001]"
  if {$s ne $::at_last} { set ::at_last $s; set ::at_shot [expr {$::at_f + 3}] }
  if {$::at_f == $::at_shot || $::at_f %% 60 == 0} {
    screenshot -raw [format "%s/f%%05d_s%%s.png" $::at_f $s]
  }
  if {$::at_f == $::at_press} { if {$::at_row < 0} { at_poke } else { keymatrixdown $::at_row $::at_mask } }
  if {$::at_f == $::at_press + 6 && $::at_row >= 0} { keymatrixup $::at_row $::at_mask }
  if {$::at_f < $::at_max} { after frame at_tick } else { set ::at_done 1; set pause on }
}
'''


def main(secs=100, press=''):
    os.makedirs(OUT, exist_ok=True)
    for f in os.listdir(OUT):
        if f.endswith('.png'):
            os.unlink(os.path.join(OUT, f))
    row, mask, pf = 0, 0, -1
    if press:
        k, f = press.split('@')
        pf = int(f)
        if '=' in k:
            a, v = k.split('=')
            send(f'proc at_poke {{}} {{ poke 0x{a} {int(v)} }}')
            row, mask = -1, 0
        else:
            row, mask = KEYS[k]
    send('set renderer SDLGL-PP')
    send('set throttle on')
    send(TCL.replace('%VARS%', ' '.join(VARS)).replace('%s/', OUT + '/', 1).replace('%%', '%'))
    send('set ::at_snd {}')
    send('if {[info exists ::at_bp]} { catch { debug remove_bp $::at_bp } }; '
         'set ::at_bp [debug set_bp 0x4E1F {} { lappend ::at_snd "$::at_f [reg A]" }]')
    send(f'set ::at_f 0; set ::at_log {{}}; set ::at_last ""; set ::at_shot -1; set ::at_max {int(secs * 60)}; '
         f'set ::at_done 0; set ::at_press {pf}; set ::at_row {row}; set ::at_mask {mask}')
    send('set power off')
    send('set pause off')
    send('set power on; after frame at_tick')
    while send('set ::at_done')[1] != '1':
        time.sleep(1)
    log = send('join $::at_log "\\n"')[1]
    with gzip.open(os.path.join(OUT, f'frames{"_" + press.replace("@", "_") if press else ""}.tsv.gz'), 'wt') as fh:
        fh.write('frame ' + ' '.join(VARS) + '\n' + log + '\n')
    snd = send('join $::at_snd "\\n"')[1]
    with open(os.path.join(OUT, f'sounds{"_" + press.replace("@", "_") if press else ""}.tsv'), 'w') as fh:
        fh.write('frame A\n' + snd + '\n')
    print(len(log.splitlines()), 'frames,', len(snd.splitlines()), 'sonidos ->', OUT)


if __name__ == '__main__':
    main(float(sys.argv[1]) if len(sys.argv) > 1 else 100, sys.argv[2] if len(sys.argv) > 2 else '')
