# Vigila los cambios de tiles de la sala en plena partida (E000 = 5).
# El juego escribe la VRAM por los puertos del VDP: se sigue el puntero de direccion (dos OUT a 0x99)
# y cada OUT a 0x98 dentro del mapa de nombres de la sala (0x3880-0x3AFF, sin el marcador) que cambia
# el tile se anota una vez por (PC, retorno, mundo, sala, direccion) en ::tw_hits.
set ::tw_latch 0
set ::tw_addr 0
proc tw_ctrl {} {
    if {$::tw_latch == 0} {
        set ::tw_lo $::wp_last_value
        set ::tw_latch 1
    } else {
        set ::tw_latch 0
        if {!($::wp_last_value & 0x80)} { set ::tw_addr [expr {(($::wp_last_value & 0x3F) << 8) | $::tw_lo}] }
    }
}
proc tw_data {} {
    set a $::tw_addr
    set ::tw_addr [expr {($a + 1) & 0x3FFF}]
    set ::tw_latch 0
    if {$a < 0x3880 || $a >= 0x3B00 || [peek 0xE000] != 5} return
    set old [vpeek $a]
    if {$old == $::wp_last_value} return
    set ret [format %04X [peek16 [reg SP]]]
    set key "[format %04X [reg PC]] $ret [peek 0xE041] [peek 0xE042] [format %04X $a]"
    if {![info exists ::tw_seen($key)]} {
        set ::tw_seen($key) 1
        lappend ::tw_hits "$key [format %02X $old] [format %02X $::wp_last_value] [peek 0xE505] [peek 0xE507] [peek 0xF0F1] [peek 0xF0F2] [peek 0xF0F3]"
    }
}
proc tw_on {} {
    set ::tw_hits {}
    array unset ::tw_seen
    set ::tw_ids [list [debug set_watchpoint write_io 0x99 {} tw_ctrl] [debug set_watchpoint write_io 0x98 {} tw_data]]
}
proc tw_off {} { foreach id $::tw_ids { catch {debug remove_watchpoint $id} } }
