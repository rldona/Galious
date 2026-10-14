# Traza de la maquina de estados de Knightmare II (Galious) en openMSX.
#
# Pone un watchpoint de escritura en game_state (E000) y game_substate (E001) y
# registra cada CAMBIO de valor en un TSV, con quien escribe y una foto de los
# flags que despachan el estado 5. Ver docs/state_machine.md.
#
# Uso: al cargarlo empieza a registrar en logs/state_trace_<fecha>.tsv
#   openmsx -cart <rom> -script tools/openmsx/galious_state_trace.tcl
#   o desde la consola de openMSX (F10): source tools/openmsx/galious_state_trace.tcl
# Despues, en la consola:
#   galious_trace stop
#   galious_trace start [fichero.tsv]

namespace eval galious_trace {

variable script_dir [file dirname [file normalize [info script]]]
variable root [file dirname [file dirname $script_dir]]
variable wp ""
variable fh ""
variable path ""
variable last
variable syms {}

# Nombres de docs/state_machine.md (0-5 observados en openMSX; el resto provisionales).
variable state_names {
	intro title demo game_start enter_room main_loop
	life_lost game_over st08_e01f1 st09_9e07 st10_e01a st11_9e21 st12_e01b
	st13_e01f2 st14_e690 st15_e020 st16_e01c st17_e024 st18_e019 st19_e02c
	st20_e031
}

# Flags que lee game_state_05 para elegir estado, mas los de vidas/personaje.
variable flags {
	E002 E004 E019 E01A E01B E01C E01F E020 E024 E026 E02C E031 E058 E059 E501 E690
}

# Simbolos del banco 0 y la BIOS desde los seeds de REA (opcional).
proc load_syms {} {
	variable root
	variable syms
	set syms {}
	set f [file join $root extracted rea seeds.tsv]
	if {![file exists $f]} return
	set ch [open $f r]
	foreach line [split [read $ch] \n] {
		lassign [split $line \t] a kind name
		if {$kind eq "function" && $name ne ""} { lappend syms [list [expr {$a}] $name] }
	}
	close $ch
	set syms [lsort -integer -index 0 $syms]
}

# Direccion -> "nombre+off"; en 6000-BFFF antepone el banco mapeado (sombras F0F1-F0F3).
proc sym {addr} {
	variable syms
	if {$addr >= 0x6000 && $addr < 0xC000} {
		set bank [peek [expr {0xF0F1 + ($addr - 0x6000) / 0x2000}]]
		if {$bank > 15} { return [format "b??:%04X" $addr] }   ;# sombras aun sin inicializar
		return [format "b%02d:%04X" $bank $addr]
	}
	if {$addr >= 0x6000} { return [format "%04X" $addr] }
	set best ""
	foreach s $syms {
		if {[lindex $s 0] > $addr} break
		set best $s
	}
	if {$best eq ""} { return [format "%04X" $addr] }
	lassign $best base name
	if {$base == $addr} { return $name }
	return [format "%s+%X" $name [expr {$addr - $base}]]
}

proc state_name {n} {
	variable state_names
	if {$n < [llength $state_names]} { return [lindex $state_names $n] }
	return "?"
}

proc on_write {} {
	variable last
	variable fh
	variable flags
	set addr $::wp_last_address
	set new $::wp_last_value
	set old $last($addr)
	if {$new == $old} return
	set last($addr) $new

	# El test de RAM de la BIOS escribe patrones en E000 al arrancar: no es el juego.
	set pc [reg PC]
	if {$pc < 0x4000} return
	# peek16 falla con SP=FFFF (lee 0x10000) mientras arranca la BIOS: envolver a 16 bits.
	set sp [reg SP]
	set ret [expr {[peek $sp] | ([peek [expr {($sp + 1) & 0xFFFF}]] << 8)}]
	set t [format "%.4f" [machine_info time]]
	set frame [peek 0xE003]
	if {$addr == 0xE000} {
		set var state
		set desc "[state_name $old] -> [state_name $new]"
		set snap {}
		foreach f $flags { lappend snap "$f=[format %02X [peek 0x$f]]" }
		message [format "Galious estado %d -> %d (%s)" $old $new [state_name $new]]
	} else {
		set var substate
		set desc "estado [peek 0xE000] ([state_name [peek 0xE000]])"
		set snap {}
	}
	puts $fh [join [list $t $frame $var $old $new [sym $pc] [sym $ret] $desc [join $snap " "]] \t]
	flush $fh
}

proc start {{file ""}} {
	variable wp
	variable fh
	variable path
	variable last
	variable root
	if {$wp ne ""} { return "galious_trace ya esta activo: $path" }
	if {$file eq ""} {
		file mkdir [file join $root logs]
		# El Tcl de openMSX no trae zona horaria: fecha en UTC o, si falla, segundos Unix.
		set now [clock seconds]
		if {[catch {clock format $now -gmt 1 -format "%Y%m%d_%H%M%SZ"} stamp]} { set stamp $now }
		set file [file join $root logs "state_trace_$stamp.tsv"]
	}
	load_syms
	set path [file normalize $file]
	set fh [open $path a]
	puts $fh [join {time_s frame var old new writer_pc caller desc flags} \t]
	flush $fh
	arm
	return "galious_trace activo -> $path"
}

# loadstate crea una maquina nueva y se lleva el watchpoint: rearmar en cada cambio.
proc arm {} {
	variable wp
	variable last
	variable after_id
	disarm
	array set last [list [expr {0xE000}] [peek 0xE000] [expr {0xE001}] [peek 0xE001]]
	set wp [debug set_watchpoint write_mem {0xE000 0xE001} {} galious_trace::on_write]
	set after_id [after machine_switch [namespace code on_machine_switch]]
}

# Quita nuestros watchpoints (aunque vengan de un source anterior) y el manejador pendiente.
proc disarm {} {
	variable after_id
	foreach line [split [debug list_watchpoints] \n] {
		if {[lindex $line end] eq "galious_trace::on_write"} { debug remove_watchpoint [lindex $line 0] }
	}
	if {[info exists after_id]} { catch {after cancel $after_id} }
	foreach line [split [after info] \n] {
		if {[string match "*galious_trace on_machine_switch*" $line]} {
			after cancel [string trimright [lindex $line 0] :]
		}
	}
}

proc on_machine_switch {} {
	variable wp
	variable fh
	variable switch_time
	if {$wp eq ""} return
	# openMSX lo dispara dos veces por loadstate: una sola linea por instante.
	set t [machine_info time]
	if {[info exists switch_time] && $switch_time == $t} { arm; return }
	set switch_time $t
	puts $fh [join [list [format "%.4f" [machine_info time]] [peek 0xE003] machine_switch {} {} {} {} \
		"estado [peek 0xE000] ([state_name [peek 0xE000]])" {}] \t]
	flush $fh
	arm
}

proc stop {} {
	variable wp
	variable fh
	variable path
	if {$wp eq ""} { return "galious_trace no esta activo" }
	disarm
	close $fh
	set wp ""
	set fh ""
	return "galious_trace parado; log en $path"
}

namespace export start stop
namespace ensemble create -command ::galious_trace
}

# Al cargar el script (source o -script) la traza arranca sola.
puts [galious_trace start]
