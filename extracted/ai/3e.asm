; tipo 0x3e  (hijo, no aparece en salas)  descriptor 81 c4 0a 02 0f
; ---- inicio (b04:66B4[61]) ----
A283  3a 69 e6     LD A,(0xe669)
A286  fe 05        CP 5
A288  dd 36 07 b0  LD (IX+7),0xb0 ; x
A28C  30 04        JR nc,0xa292
A28E  dd 36 07 40  LD (IX+7),0x40 ; x
A292  dd 36 05 28  LD (IX+5),0x28 ; y
A296  dd 36 1b 0c  LD (IX+0x1b),12
A29A  dd 36 08 06  LD (IX+8),6
A29E  11 00 02     LD DE,0x200
A2A1  c3 37 b1     JP 0xb137
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
; ---- actualizacion (b06:B472[61]) ----
6BD5  dd 36 01 00  LD (IX+1),0 ; estado
6BD9  c9           RET
6CD2  dd 7e 14     LD A,(IX+0x14)
6CD5  ee 01        XOR 1
6CD7  dd 77 14     LD (IX+0x14),A
6CDA  c9           RET
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
7EDD  58           LD E,B
7EDE  dd 77 11     LD (IX+0x11),A ; atributo_sprite
7EE1  cd 5f b1     CALL 0xb15f
7EE4  cd d2 6c     CALL 0x6cd2
7EE7  cb 47        BIT 0,A
7EE9  3e a0        LD A,0xa0
7EEB  28 02        JR z,0x7eef
7EED  3e a8        LD A,0xa8
7EEF  c3 83 70     JP 0x7083
96EA  dd 77 05     LD (IX+5),A ; y
96ED  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
A2A4  cd b0 a3     CALL 0xa3b0
A2A7  dd 7e 01     LD A,(IX+1) ; estado
A2AA  cd 50 40     CALL 0x4050 ; jump_table_a
A2AD  b3 a2        .word 0xA2B3 ; caso 0
A2AF  e7 a2        .word 0xA2E7 ; caso 1
A2B1  05 a3        .word 0xA305 ; caso 2
A2B3  dd 7e        .word 0x7EDD ; caso 3
A2E7  dd 7e 16     LD A,(IX+0x16) ; ay_lo
A2EA  dd be 05     CP (IX+5) ; y
A2ED  c0           RET nz
A2EE  cd 29 b1     CALL 0xb129
A2F1  dd cb 14 46  BIT 0,(IX+0x14)
A2F5  11 00 01     LD DE,0x100
A2F8  c4 68 40     CALL nz,0x4068 ; neg_de
A2FB  cd 3e b1     CALL 0xb13e
A2FE  dd 36 19 00  LD (IX+0x19),0
A302  c3 07 9a     JP 0x9a07
A305  dd 7e 19     LD A,(IX+0x19)
A308  21 ba a3     LD HL,0xa3ba
A30B  cd 83 40     CALL 0x4083
A30E  dd cb 15 46  BIT 0,(IX+0x15)
A312  dd 77 1a     LD (IX+0x1a),A
A315  c4 a1 a3     CALL nz,0xa3a1
A318  dd 34 19     INC (IX+0x19)
A31B  dd cb 17 46  BIT 0,(IX+0x17) ; ay
A31F  28 1e        JR z,0xa33f
A321  dd 7e 19     LD A,(IX+0x19)
A324  fe 0e        CP 14
A326  20 17        JR nz,0xa33f
A328  dd 35 17     DEC (IX+0x17) ; ay
A32B  cd a7 a3     CALL 0xa3a7
A32E  3e 02        LD A,2
A330  dd 77 19     LD (IX+0x19),A
A333  21 ba a3     LD HL,0xa3ba
A336  cd 83 40     CALL 0x4083
A339  dd 77 1a     LD (IX+0x1a),A
A33C  cd a1 a3     CALL 0xa3a1
A33F  dd 7e 19     LD A,(IX+0x19)
A342  fe 0f        CP 15
A344  30 0e        JR nc,0xa354
A346  fe 09        CP 9
A348  d4 9e a3     CALL nc,0xa39e
A34B  dd 7e 05     LD A,(IX+5) ; y
A34E  dd 86 1a     ADD A,(IX+0x1a)
A351  c3 ea 96     JP 0x96ea
A354  cd a7 a3     CALL 0xa3a7
A357  cd d5 6b     CALL 0x6bd5
A35A  cd 2e b1     CALL 0xb12e
A35D  dd cb 15 46  BIT 0,(IX+0x15)
A361  11 00 02     LD DE,0x200
A364  c4 68 40     CALL nz,0x4068 ; neg_de
A367  c3 37 b1     JP 0xb137
A39E  dd 7e 1a     LD A,(IX+0x1a)
A3A1  ed 44        NEG
A3A3  dd 77 1a     LD (IX+0x1a),A
A3A6  c9           RET
A3A7  dd 7e 15     LD A,(IX+0x15)
A3AA  ee 01        XOR 1
A3AC  dd 77 15     LD (IX+0x15),A
A3AF  c9           RET
A3B0  dd 56 1b     LD D,(IX+0x1b)
A3B3  1e 06        LD E,6
A3B5  06 01        LD B,1
A3B7  c3 d6 b0     JP 0xb0d6
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
B129  16 00        LD D,0
B12B  5a           LD E,D
B12C  18 09        JR 0xb137
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
