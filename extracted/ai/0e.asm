; tipo 0x0e  vida 1  patron observado: vuela
; ---- inicio (b04:66B4[13]) ----
7D18  dd 36 08 04  LD (IX+8),4
7D1C  dd 36 18 04  LD (IX+0x18),4
7D20  dd 36 16 0b  LD (IX+0x16),11 ; ay_lo
7D24  cd 9e 7d     CALL 0x7d9e
7D27  cd 03 a1     CALL 0xa103
7D2A  cd 17 7e     CALL 0x7e17
7D2D  d0           RET nc
7D2E  cd 06 97     CALL 0x9706
7D31  d0           RET nc
7D32  dd 36 0a a0  LD (IX+10),0xa0 ; d1
7D36  dd 34 19     INC (IX+0x19)
7D39  c9           RET
7D9E  cd f1 81     CALL 0x81f1
7DA1  e6 1f        AND 0x1f
7DA3  c6 60        ADD A,0x60
7DA5  dd 77 17     LD (IX+0x17),A ; ay
7DA8  cd f1 81     CALL 0x81f1
7DAB  e6 0f        AND 15
7DAD  c6 10        ADD A,0x10
7DAF  dd 77 15     LD (IX+0x15),A
7DB2  c9           RET
7E17  dd 66 07     LD H,(IX+7) ; x
7E1A  dd 6e 05     LD L,(IX+5) ; y
7E1D  2d           DEC L
7E1E  cd 6e 40     CALL 0x406e ; vec_4dc8
7E21  d0           RET nc
7E22  c3 71 40     JP 0x4071
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
9706  dd 66 07     LD H,(IX+7) ; x
9709  dd 6e 05     LD L,(IX+5) ; y
970C  cd 6e 40     CALL 0x406e ; vec_4dc8
970F  3f           CCF
9710  d0           RET nc
9711  23           INC HL
9712  be           CP (HL)
9713  3f           CCF
9714  d0           RET nc
9715  01 1f 00     LD BC,0x1f
9718  09           ADD HL,BC
9719  be           CP (HL)
971A  3f           CCF
971B  d0           RET nc
971C  23           INC HL
971D  be           CP (HL)
971E  3f           CCF
971F  c9           RET
A103  3e 01        LD A,1
A105  32 3e e6     LD (0xe63e),A
A108  c9           RET
; ---- actualizacion (b06:B472[13]) ----
7D3A  dd 7e 01     LD A,(IX+1) ; estado
7D3D  cd 50 40     CALL 0x4050 ; jump_table_a
7D40  4a 7d        .word 0x7D4A ; caso 0
7D42  53 7d        .word 0x7D53 ; caso 1
7D44  71 7d        .word 0x7D71 ; caso 2
7D46  8e 7d        .word 0x7D8E ; caso 3
7D48  b3 7d        .word 0x7DB3 ; caso 4
7D4A  dd 34 01     INC (IX+1) ; estado
7D4D  dd cb 19 46  BIT 0,(IX+0x19)
7D51  20 0c        JR nz,0x7d5f
7D53  cd 17 7e     CALL 0x7e17
7D56  d2 bd 7d     JP nc,0x7dbd
7D59  cd 06 97     CALL 0x9706
7D5C  d2 bd 7d     JP nc,0x7dbd
7D5F  dd 36 08 50  LD (IX+8),0x50
7D63  dd 36 0a a0  LD (IX+10),0xa0 ; d1
7D67  dd 36 11 67  LD (IX+0x11),0x67 ; atributo_sprite
7D6B  cd 24 b1     CALL 0xb124
7D6E  c3 07 9a     JP 0x9a07
7D71  dd 35 08     DEC (IX+8)
7D74  c0           RET nz
7D75  dd 36 14 0a  LD (IX+0x14),10
7D79  dd 36 08 06  LD (IX+8),6
7D7D  dd 36 0a ac  LD (IX+10),0xac ; d1
7D81  dd 36 11 69  LD (IX+0x11),0x69 ; atributo_sprite
7D85  11 00 01     LD DE,0x100
7D88  cd 37 b1     CALL 0xb137
7D8B  c3 07 9a     JP 0x9a07
7D8E  1e 06        LD E,6
7D90  cd d6 7d     CALL 0x7dd6
7D93  dd 35 14     DEC (IX+0x14)
7D96  c0           RET nz
7D97  dd 34 01     INC (IX+1) ; estado
7D9A  dd 36 08 04  LD (IX+8),4
7D9E  cd f1 81     CALL 0x81f1
7DA1  e6 1f        AND 0x1f
7DA3  c6 60        ADD A,0x60
7DA5  dd 77 17     LD (IX+0x17),A ; ay
7DA8  cd f1 81     CALL 0x81f1
7DAB  e6 0f        AND 15
7DAD  c6 10        ADD A,0x10
7DAF  dd 77 15     LD (IX+0x15),A
7DB2  c9           RET
7DB3  dd 35 17     DEC (IX+0x17) ; ay
7DB6  20 05        JR nz,0x7dbd
7DB8  dd 36 01 01  LD (IX+1),1 ; estado
7DBC  c9           RET
7DBD  cd dd 7d     CALL 0x7ddd
7DC0  1e 04        LD E,4
7DC2  cd d6 7d     CALL 0x7dd6
7DC5  dd 35 15     DEC (IX+0x15)
7DC8  c0           RET nz
7DC9  cd a8 7d     CALL 0x7da8
7DCC  cd f1 81     CALL 0x81f1
7DCF  e6 0f        AND 15
7DD1  06 02        LD B,2
7DD3  c3 25 7e     JP 0x7e25
7DD6  16 1c        LD D,0x1c
7DD8  06 66        LD B,0x66
7DDA  c3 d6 b0     JP 0xb0d6
7DDD  dd 7e 05     LD A,(IX+5) ; y
7DE0  dd cb 0d 7e  BIT 7,(IX+13) ; vy
7DE4  20 06        JR nz,0x7dec
7DE6  fe b0        CP 0xb0
7DE8  38 11        JR c,0x7dfb
7DEA  18 04        JR 0x7df0
7DEC  fe 28        CP 0x28
7DEE  30 0b        JR nc,0x7dfb
7DF0  dd 36 15 10  LD (IX+0x15),0x10
7DF4  dd 36 1d 01  LD (IX+0x1d),1
7DF8  cd 6f b1     CALL 0xb16f
7DFB  dd 7e 07     LD A,(IX+7) ; x
7DFE  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7E02  20 05        JR nz,0x7e09
7E04  fe e8        CP 0xe8
7E06  d8           RET c
7E07  18 03        JR 0x7e0c
7E09  fe 08        CP 8
7E0B  d0           RET nc
7E0C  dd 36 15 10  LD (IX+0x15),0x10
7E10  dd 36 1c 01  LD (IX+0x1c),1
7E14  c3 5f b1     JP 0xb15f
7E17  dd 66 07     LD H,(IX+7) ; x
7E1A  dd 6e 05     LD L,(IX+5) ; y
7E1D  2d           DEC L
7E1E  cd 6e 40     CALL 0x406e ; vec_4dc8
7E21  d0           RET nc
7E22  c3 71 40     JP 0x4071
7E25  f5           PUSH AF
7E26  21 49 7e     LD HL,0x7e49
7E29  cd 53 40     CALL 0x4053 ; read_word_table
7E2C  21 00 00     LD HL,0
7E2F  c5           PUSH BC
7E30  19           ADD HL,DE
7E31  10 fd        DJNZ 0x7e30
7E33  eb           EX DE,HL
7E34  cd 3e b1     CALL 0xb13e
7E37  c1           POP BC
7E38  f1           POP AF
7E39  21 69 7e     LD HL,0x7e69
7E3C  cd 53 40     CALL 0x4053 ; read_word_table
7E3F  21 00 00     LD HL,0
7E42  19           ADD HL,DE
7E43  10 fd        DJNZ 0x7e42
7E45  eb           EX DE,HL
7E46  c3 37 b1     JP 0xb137
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
9706  dd 66 07     LD H,(IX+7) ; x
9709  dd 6e 05     LD L,(IX+5) ; y
970C  cd 6e 40     CALL 0x406e ; vec_4dc8
970F  3f           CCF
9710  d0           RET nc
9711  23           INC HL
9712  be           CP (HL)
9713  3f           CCF
9714  d0           RET nc
9715  01 1f 00     LD BC,0x1f
9718  09           ADD HL,BC
9719  be           CP (HL)
971A  3f           CCF
971B  d0           RET nc
971C  23           INC HL
971D  be           CP (HL)
971E  3f           CCF
971F  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B0D6  dd 35 08     DEC (IX+8)
B0D9  c0           RET nz
B0DA  dd 73 08     LD (IX+8),E
B0DD  dd 7e 0a     LD A,(IX+10) ; d1
B0E0  aa           XOR D
B0E1  dd 77 0a     LD (IX+10),A ; d1
B0E4  dd 7e 11     LD A,(IX+0x11) ; atributo_sprite
B0E7  a8           XOR B
B0E8  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B0EB  af           XOR A
B0EC  c9           RET
B124  cd 2e b1     CALL 0xb12e
B127  18 0e        JR 0xb137
B12E  16 00        LD D,0
B130  5a           LD E,D
B131  18 0b        JR 0xb13e
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
B16F  dd 5e 0c     LD E,(IX+12) ; vy_lo
B172  dd 56 0d     LD D,(IX+13) ; vy
B175  cd 68 40     CALL 0x4068 ; neg_de
B178  dd 73 0c     LD (IX+12),E ; vy_lo
B17B  dd 72 0d     LD (IX+13),D ; vy
B17E  c9           RET
