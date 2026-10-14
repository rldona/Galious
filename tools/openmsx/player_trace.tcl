# Registro por frame de la posicion del jugador: "y x E500 E50E" (ver tools/openmsx/jump_measure.py).
proc pt_tick {} {
    lappend ::pt_log "[peek 0xE505] [peek 0xE507] [peek 0xE500] [peek 0xE50E]"
    incr ::pt_left -1
    if {$::pt_left > 0} { after frame pt_tick }
}
proc pt_start {n} { set ::pt_log {}; set ::pt_left $n; after frame pt_tick }
