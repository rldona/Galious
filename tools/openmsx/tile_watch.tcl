# Vigila los cambios de tiles de la sala en plena partida.
# Cada frame compara el mapa de nombres de la sala en VRAM (0x3880-0x3AFF, sin el marcador) con el del
# frame anterior; solo cuenta si los dos frames estan en el bucle principal (E000 = 5) y en la misma
# sala. Cada (mundo, sala, direccion, antes, despues) se anota una vez en ::tw_hits con la posicion
# del jugador. (Vigilar los puertos del VDP da el PC, pero es decenas de veces mas lento y el PC suele
# ser la copia en bloque de b00:54BF.)
proc tw_frame {} {
    if {!$::tw_on} return
    set key "[peek 0xE000] [peek 0xE041] [peek 0xE042]"
    binary scan [debug read_block VRAM 0x3880 0x280] cu* cur
    if {$key eq $::tw_key && [lindex $key 0] == 5} {
        set i 0
        foreach a $::tw_prev b $cur {
            if {$a != $b} {
                set k "[lrange $key 1 2] [format %04X [expr {0x3880 + $i}]] [format %02X $a] [format %02X $b]"
                if {![info exists ::tw_seen($k)]} {
                    set ::tw_seen($k) 1
                    lappend ::tw_hits "- - $k [peek 0xE505] [peek 0xE507]"
                }
            }
            incr i
        }
    }
    set ::tw_prev $cur
    set ::tw_key $key
    after frame tw_frame
}
proc tw_on {} {
    set ::tw_hits {}
    array unset ::tw_seen
    set ::tw_key {}
    set ::tw_on 1
    after frame tw_frame
}
proc tw_off {} { set ::tw_on 0 }
