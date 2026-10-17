; tipo 0x59  (hijo, no aparece en salas)  descriptor 81 a0 08 01 70
; ---- inicio (b04:66B4[88]) ----
6C42  dd cb 03 f6  SET 6,(IX+3) ; flags2
6C46  c9           RET
AE03  cd 42 6c     CALL 0x6c42
AE06  3a 24 e6     LD A,(0xe624)
AE09  1f           RRA
AE0A  f5           PUSH AF
AE0B  47           LD B,A
AE0C  21 2c ae     LD HL,0xae2c
AE0F  cd 53 40     CALL 0x4053 ; read_word_table
AE12  cd 37 b1     CALL 0xb137
AE15  78           LD A,B
AE16  21 3c ae     LD HL,0xae3c
AE19  cd 53 40     CALL 0x4053 ; read_word_table
AE1C  f1           POP AF
AE1D  dc 68 40     CALL c,0x4068 ; neg_de
AE20  cd 3e b1     CALL 0xb13e
AE23  11 80 00     LD DE,0x80
AE26  c3 45 b1     JP 0xb145
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B145  dd 73 16     LD (IX+0x16),E ; ay_lo
B148  dd 72 17     LD (IX+0x17),D ; ay
B14B  c9           RET
; ---- actualizacion (b06:B472[88]) ----
AE29  c3 4c b1     JP 0xb14c
B14C  dd 7e 0c     LD A,(IX+12) ; vy_lo
B14F  dd 86 16     ADD A,(IX+0x16) ; ay_lo
B152  dd 77 0c     LD (IX+12),A ; vy_lo
B155  dd 7e 0d     LD A,(IX+13) ; vy
B158  dd 8e 17     ADC A,(IX+0x17) ; ay
B15B  dd 77 0d     LD (IX+13),A ; vy
B15E  c9           RET
