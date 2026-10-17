; tipo 0x55  (hijo, no aparece en salas)  descriptor 81 a8 0f 01 00
; ---- inicio (b04:66B4[84]) ----
6C42  dd cb 03 f6  SET 6,(IX+3) ; flags2
6C46  c9           RET
6EF0  3a 24 e6     LD A,(0xe624)
6EF3  6f           LD L,A
6EF4  26 00        LD H,0
6EF6  29           ADD HL,HL
6EF7  54           LD D,H
6EF8  5d           LD E,L
6EF9  29           ADD HL,HL
6EFA  19           ADD HL,DE
6EFB  09           ADD HL,BC
6EFC  5e           LD E,(HL)
6EFD  23           INC HL
6EFE  56           LD D,(HL)
6EFF  cd 3e b1     CALL 0xb13e
6F02  23           INC HL
6F03  5e           LD E,(HL)
6F04  23           INC HL
6F05  56           LD D,(HL)
6F06  cd 37 b1     CALL 0xb137
6F09  23           INC HL
6F0A  5e           LD E,(HL)
6F0B  23           INC HL
6F0C  56           LD D,(HL)
6F0D  cd 45 b1     CALL 0xb145
6F10  c3 42 6c     JP 0x6c42
AD05  dd 36 14 08  LD (IX+0x14),8
AD09  dd cb 02 ae  RES 5,(IX+2) ; flags
AD0D  01 49 6f     LD BC,0x6f49
AD10  c3 f0 6e     JP 0x6ef0
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B145  dd 73 16     LD (IX+0x16),E ; ay_lo
B148  dd 72 17     LD (IX+0x17),D ; ay
B14B  c9           RET
; ---- actualizacion (b06:B472[84]) ----
6CBE  cd 53 40     CALL 0x4053 ; read_word_table
6CC1  c3 6f 73     JP 0x736f
736F  dd 73 0a     LD (IX+10),E ; d1
7372  dd 72 11     LD (IX+0x11),D ; atributo_sprite
7375  c9           RET
AD13  dd 35 14     DEC (IX+0x14)
AD16  20 04        JR nz,0xad1c
AD18  dd cb 02 ee  SET 5,(IX+2) ; flags
AD1C  cd 22 ad     CALL 0xad22
AD1F  c3 4c b1     JP 0xb14c
AD22  3a 04 e6     LD A,(0xe604)
AD25  e6 01        AND 1
AD27  c0           RET nz
AD28  dd 34 1e     INC (IX+0x1e)
AD2B  dd 7e 1e     LD A,(IX+0x1e)
AD2E  e6 03        AND 3
AD30  4f           LD C,A
AD31  dd 7e 00     LD A,(IX) ; tipo
AD34  fe 55        CP 0x55
AD36  79           LD A,C
AD37  20 02        JR nz,0xad3b
AD39  c6 04        ADD A,4
AD3B  21 41 ad     LD HL,0xad41
AD3E  c3 be 6c     JP 0x6cbe
B14C  dd 7e 0c     LD A,(IX+12) ; vy_lo
B14F  dd 86 16     ADD A,(IX+0x16) ; ay_lo
B152  dd 77 0c     LD (IX+12),A ; vy_lo
B155  dd 7e 0d     LD A,(IX+13) ; vy
B158  dd 8e 17     ADC A,(IX+0x17) ; ay
B15B  dd 77 0d     LD (IX+13),A ; vy
B15E  c9           RET
