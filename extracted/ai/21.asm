; tipo 0x21  (hijo, no aparece en salas)  descriptor 81 a0 0b 02 58
; ---- inicio (b04:66B4[32]) ----
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
7E89  cd f1 81     CALL 0x81f1
7E8C  d6 10        SUB 0x10
7E8E  e6 df        AND 0xdf
7E90  c6 10        ADD A,0x10
7E92  dd 77 07     LD (IX+7),A ; x
7E95  fe 80        CP 0x80
7E97  06 00        LD B,0
7E99  38 01        JR c,0x7e9c
7E9B  04           INC B
7E9C  dd 70 14     LD (IX+0x14),B
7E9F  dd 7e 14     LD A,(IX+0x14)
7EA2  a7           AND A
7EA3  11 00 01     LD DE,0x100
7EA6  3e a0        LD A,0xa0
7EA8  28 05        JR z,0x7eaf
7EAA  11 00 ff     LD DE,0xff00
7EAD  3e a8        LD A,0xa8
7EAF  cd 3e b1     CALL 0xb13e
7EB2  c3 83 70     JP 0x7083
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[32]) ----
6CD2  dd 7e 14     LD A,(IX+0x14)
6CD5  ee 01        XOR 1
6CD7  dd 77 14     LD (IX+0x14),A
6CDA  c9           RET
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
7E9F  dd 7e 14     LD A,(IX+0x14)
7EA2  a7           AND A
7EA3  11 00 01     LD DE,0x100
7EA6  3e a0        LD A,0xa0
7EA8  28 05        JR z,0x7eaf
7EAA  11 00 ff     LD DE,0xff00
7EAD  3e a8        LD A,0xa8
7EAF  cd 3e b1     CALL 0xb13e
7EB2  c3 83 70     JP 0x7083
7EB5  dd 7e 01     LD A,(IX+1) ; estado
7EB8  3d           DEC A
7EB9  28 43        JR z,0x7efe
7EBB  cd 20 7f     CALL 0x7f20
7EBE  cd 0e 7f     CALL 0x7f0e
7EC1  30 2f        JR nc,0x7ef2
7EC3  dd cb 14 46  BIT 0,(IX+0x14)
7EC7  dd 7e 07     LD A,(IX+7) ; x
7ECA  dd 6e 05     LD L,(IX+5) ; y
7ECD  06 0e        LD B,14
7ECF  28 02        JR z,0x7ed3
7ED1  06 01        LD B,1
7ED3  80           ADD A,B
7ED4  d8           RET c
7ED5  67           LD H,A
7ED6  1e 08        LD E,8
7ED8  cd 7a 40     CALL 0x407a
7EDB  d0           RET nc
7EDC  3e 58        LD A,0x58
7EDE  dd 77 11     LD (IX+0x11),A ; atributo_sprite
7EE1  cd 5f b1     CALL 0xb15f
7EE4  cd d2 6c     CALL 0x6cd2
7EE7  cb 47        BIT 0,A
7EE9  3e a0        LD A,0xa0
7EEB  28 02        JR z,0x7eef
7EED  3e a8        LD A,0xa8
7EEF  c3 83 70     JP 0x7083
7EF2  cd 2e b1     CALL 0xb12e
7EF5  11 00 02     LD DE,0x200
7EF8  cd 37 b1     CALL 0xb137
7EFB  c3 07 9a     JP 0x9a07
7EFE  cd 0e 7f     CALL 0x7f0e
7F01  d0           RET nc
7F02  dd 35 01     DEC (IX+1) ; estado
7F05  cd 15 9b     CALL 0x9b15
7F08  cd 29 b1     CALL 0xb129
7F0B  c3 9f 7e     JP 0x7e9f
7F0E  06 10        LD B,0x10
7F10  dd 66 07     LD H,(IX+7) ; x
7F13  dd 7e 05     LD A,(IX+5) ; y
7F16  fe b0        CP 0xb0
7F18  d0           RET nc
7F19  6f           LD L,A
7F1A  11 0d 02     LD DE,0x20d
7F1D  c3 74 40     JP 0x4074
7F20  16 04        LD D,4
7F22  06 01        LD B,1
7F24  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7F28  28 01        JR z,0x7f2b
7F2A  04           INC B
7F2B  c3 7f b1     JP 0xb17f
96EA  dd 77 05     LD (IX+5),A ; y
96ED  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
9B15  dd 7e 05     LD A,(IX+5) ; y
9B18  e6 f8        AND 0xf8
9B1A  c3 ea 96     JP 0x96ea
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
B17F  0e 07        LD C,7
B181  18 02        JR 0xb185
B185  3a 04 e6     LD A,(0xe604)
B188  a1           AND C
B189  c0           RET nz
B18A  c3 dd b0     JP 0xb0dd
