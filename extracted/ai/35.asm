; tipo 0x35  (hijo, no aparece en salas)  descriptor 81 c4 0e fe 13
; ---- inicio (b04:66B4[52]) ----
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
AB88  11 40 00     LD DE,0x40
AB8B  cd 45 b1     CALL 0xb145
AB8E  cd f1 81     CALL 0x81f1
AB91  d6 10        SUB 0x10
AB93  e6 df        AND 0xdf
AB95  c6 10        ADD A,0x10
AB97  dd 77 07     LD (IX+7),A ; x
AB9A  17           RLA
AB9B  11 80 01     LD DE,0x180
AB9E  dc 68 40     CALL c,0x4068 ; neg_de
ABA1  cd 3e b1     CALL 0xb13e
ABA4  c3 29 b1     JP 0xb129
B129  16 00        LD D,0
B12B  5a           LD E,D
B12C  18 09        JR 0xb137
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B145  dd 73 16     LD (IX+0x16),E ; ay_lo
B148  dd 72 17     LD (IX+0x17),D ; ay
B14B  c9           RET
; ---- actualizacion (b06:B472[52]) ----
6BEE  dd 7e 05     LD A,(IX+5) ; y
6BF1  c6 10        ADD A,0x10
6BF3  6f           LD L,A
6BF4  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6BF8  3e 10        LD A,0x10
6BFA  28 01        JR z,0x6bfd
6BFC  af           XOR A
6BFD  dd 86 07     ADD A,(IX+7) ; x
6C00  67           LD H,A
6C01  c3 6e 40     JP 0x406e ; vec_4dc8
6C04  01 e0 ff     LD BC,0xffe0
6C07  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6C0B  20 03        JR nz,0x6c10
6C0D  01 e0 ff     LD BC,0xffe0
6C10  09           ADD HL,BC
6C11  3e b9        LD A,0xb9
6C13  be           CP (HL)
6C14  d8           RET c
6C15  dd 7e 07     LD A,(IX+7) ; x
6C18  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6C1C  28 03        JR z,0x6c21
6C1E  fe 08        CP 8
6C20  c9           RET
6C21  fe f0        CP 0xf0
6C23  3f           CCF
6C24  c9           RET
7062  3a 04 e6     LD A,(0xe604)
7065  e6 03        AND 3
7067  c0           RET nz
7068  dd 7e 0a     LD A,(IX+10) ; d1
706B  dd cb 0f 7e  BIT 7,(IX+15) ; vx
706F  28 0a        JR z,0x707b
7071  c6 04        ADD A,4
7073  fe d4        CP 0xd4
7075  20 0c        JR nz,0x7083
7077  3e c4        LD A,0xc4
7079  18 08        JR 0x7083
707B  d6 04        SUB 4
707D  fe c0        CP 0xc0
707F  20 02        JR nz,0x7083
7081  3e d0        LD A,0xd0
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
96EA  dd 77 05     LD (IX+5),A ; y
96ED  c9           RET
9B15  dd 7e 05     LD A,(IX+5) ; y
9B18  e6 f8        AND 0xf8
9B1A  c3 ea 96     JP 0x96ea
ABA7  cd 62 70     CALL 0x7062
ABAA  dd cb 01 46  BIT 0,(IX+1) ; estado
ABAE  20 1e        JR nz,0xabce
ABB0  cd f1 ab     CALL 0xabf1
ABB3  dc 5f b1     CALL c,0xb15f
ABB6  cd e1 ab     CALL 0xabe1
ABB9  38 03        JR c,0xabbe
ABBB  c3 4c b1     JP 0xb14c
ABBE  dd 34 01     INC (IX+1) ; estado
ABC1  cd 15 9b     CALL 0x9b15
ABC4  fe b0        CP 0xb0
ABC6  dc 29 b1     CALL c,0xb129
ABC9  3e 09        LD A,9
ABCB  c3 59 40     JP 0x4059 ; sound_request
ABCE  cd f1 ab     CALL 0xabf1
ABD1  dc 5f b1     CALL c,0xb15f
ABD4  cd e1 ab     CALL 0xabe1
ABD7  d8           RET c
ABD8  dd 35 01     DEC (IX+1) ; estado
ABDB  11 40 00     LD DE,0x40
ABDE  c3 45 b1     JP 0xb145
ABE1  dd 7e 05     LD A,(IX+5) ; y
ABE4  c6 10        ADD A,0x10
ABE6  6f           LD L,A
ABE7  dd 7e 07     LD A,(IX+7) ; x
ABEA  c6 08        ADD A,8
ABEC  67           LD H,A
ABED  cd 6e 40     CALL 0x406e ; vec_4dc8
ABF0  c9           RET
ABF1  cd 15 6c     CALL 0x6c15
ABF4  30 02        JR nc,0xabf8
ABF6  3f           CCF
ABF7  c9           RET
ABF8  cd ee 6b     CALL 0x6bee
ABFB  c3 04 6c     JP 0x6c04
B129  16 00        LD D,0
B12B  5a           LD E,D
B12C  18 09        JR 0xb137
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B145  dd 73 16     LD (IX+0x16),E ; ay_lo
B148  dd 72 17     LD (IX+0x17),D ; ay
B14B  c9           RET
B14C  dd 7e 0c     LD A,(IX+12) ; vy_lo
B14F  dd 86 16     ADD A,(IX+0x16) ; ay_lo
B152  dd 77 0c     LD (IX+12),A ; vy_lo
B155  dd 7e 0d     LD A,(IX+13) ; vy
B158  dd 8e 17     ADC A,(IX+0x17) ; ay
B15B  dd 77 0d     LD (IX+13),A ; vy
B15E  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
