proc bf_tick {} {
    set nt [debug read_block VRAM 0x3800 768]
    set pat [debug read_block VRAM 0x0000 0x1800]
    set nd 0; set pd 0
    if {[info exists ::bf_nt]} {
        binary scan $nt cu* a; binary scan $::bf_nt cu* b
        foreach x $a y $b { if {$x != $y} {incr nd} }
        if {$pat ne $::bf_pat} {
            binary scan $pat cu* a; binary scan $::bf_pat cu* b
            foreach x $a y $b { if {$x != $y} {incr pd} }
        }
    }
    set ::bf_nt $nt; set ::bf_pat $pat
    lappend ::bf_log [format "%d %d %d %d %d %d" [peek 0xE705] [peek 0xE706] [peek 0xE707] [peek 0xE708] $nd $pd]
    incr ::bf_left -1
    if {$::bf_left > 0} { after frame bf_tick }
}
proc bf_start {n} { set ::bf_log {}; set ::bf_left $n; catch {unset ::bf_nt}; after frame bf_tick }
