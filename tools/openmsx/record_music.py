#!/usr/bin/env python3
"""Graba frame a frame los registros del PSG mientras suena cada musica del juego.

  record_music.py [id_hex ...]      (sin argumentos: 0x81..0x95)

Las musicas son las peticiones de sonido >= 0x80 (b00:4E1F; tabla de 3 punteros de canal por
cancion en b13:743E). Para pedir una se inyecta la llamada: un breakpoint al principio del frame
(b00:4277) mete la direccion de vuelta en la pila y salta a sound_request con A = id. Se graba
en una sala vacia del castillo hasta 2 s de silencio o MAX segundos.

Salida: logs/omsx/music/<id>.json.gz con los 14 registros del PSG (hex) por frame (60 Hz).
"""
import gzip, json, os, sys
from omsx import RUN, run, send
from scan_rooms import goto_room

MAX, ROOM = int(os.environ.get('MUSIC_MAX', 150)), 1   # MUSIC_MAX=420 para los temas largos
TCL = '''
proc play_song {id} {
  set ::song_id $id
  set ::song_bp [debug set_bp 0x4277 {} {
    debug remove_bp $::song_bp
    set sp [expr {[reg SP] - 2}]
    reg SP $sp
    poke16 $sp [reg PC]
    reg A $::song_id
    reg PC 0x4E1F
  }]
}
proc psg_rec {n} {
  set r [debug read_block "PSG regs" 0 14]
  lappend ::psg [binary encode hex $r]
  binary scan $r @8cu3 v
  if {([lindex $v 0] & 0x1F) || ([lindex $v 1] & 0x1F) || ([lindex $v 2] & 0x1F)} { set ::quiet 0 } else { incr ::quiet }
  if {$n > 1 && !($::quiet > 120 && [llength $::psg] > 180)} { after frame [list psg_rec [expr {$n-1}]] } else { set ::rec_done 1 }
}
'''


def record(song):
    goto_room('galious_start', ROOM)
    send(f'play_song {song}')
    run(0.05)
    send('set ::psg {}; set ::quiet 0; set ::rec_done 0')
    send(f'psg_rec {MAX * 60}')
    t = 0
    while t < MAX + 1:
        run(5)
        t += 5
        if send('set ::rec_done')[1] == '1':
            break
    return send('join $::psg " "')[1].split()


def main(songs):
    out = os.path.join(RUN, 'music')
    os.makedirs(out, exist_ok=True)
    send(TCL)
    send('set throttle off')
    for s in songs:
        frames = record(s)
        with gzip.open(os.path.join(out, f'{s:02x}.json.gz'), 'wt') as f:
            json.dump({'song': s, 'frames': frames}, f)
        print(f'{s:#04x}: {len(frames)} frames ({len(frames) / 60:.1f} s)', flush=True)
    send('set throttle on')


if __name__ == '__main__':
    main([int(a, 16) for a in sys.argv[1:]] or list(range(0x81, 0x96)))
