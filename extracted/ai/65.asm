; tipo 0x65  (hijo, no aparece en salas)  descriptor 81 a0 0f 01 71
; ---- inicio (b04:66B4[100]) ----
A560  21 07 e5     LD HL,0xe507 ; jugador_x
A563  3a 07 e7     LD A,(0xe707)
A566  be           CP (HL)
A567  38 03        JR c,0xa56c
A569  af           XOR A
A56A  18 0c        JR 0xa578
A56C  3a 07 e7     LD A,(0xe707)
A56F  c6 40        ADD A,0x40
A571  be           CP (HL)
A572  3e 03        LD A,3
A574  30 02        JR nc,0xa578
A576  3e 06        LD A,6
A578  dd 77 14     LD (IX+0x14),A
A57B  21 05 e5     LD HL,0xe505 ; jugador_y
A57E  3a 05 e7     LD A,(0xe705)
A581  be           CP (HL)
A582  30 0b        JR nc,0xa58f
A584  c6 40        ADD A,0x40
A586  be           CP (HL)
A587  30 03        JR nc,0xa58c
A589  dd 34 14     INC (IX+0x14)
A58C  dd 34 14     INC (IX+0x14)
A58F  dd 7e 14     LD A,(IX+0x14)
A592  21 d7 a5     LD HL,0xa5d7
A595  cd 83 40     CALL 0x4083
A598  dd 77 0a     LD (IX+10),A ; d1
A59B  dd 7e 14     LD A,(IX+0x14)
A59E  21 b3 a5     LD HL,0xa5b3
A5A1  cd 53 40     CALL 0x4053 ; read_word_table
A5A4  cd 3e b1     CALL 0xb13e
A5A7  dd 7e 14     LD A,(IX+0x14)
A5AA  21 c5 a5     LD HL,0xa5c5
A5AD  cd 53 40     CALL 0x4053 ; read_word_table
A5B0  c3 37 b1     JP 0xb137
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[100]) ----
74BA  c9           RET
