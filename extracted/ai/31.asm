; tipo 0x31  vida None  patron observado: quieto
; ---- inicio (b04:66B4[48]) ----
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
8B32  cd f5 96     CALL 0x96f5
8B35  dd 36 18 03  LD (IX+0x18),3
8B39  cd f1 81     CALL 0x81f1
8B3C  c3 cf 71     JP 0x71cf
96F5  dd 7e 02     LD A,(IX+2) ; flags
96F8  e6 0f        AND 15
96FA  dd 77 02     LD (IX+2),A ; flags
96FD  c9           RET
; ---- actualizacion (b06:B472[48]) ----
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
7DB8  dd 36 01 01  LD (IX+1),1 ; estado
7DBC  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
8B39  cd f1 81     CALL 0x81f1
8B3C  c3 cf 71     JP 0x71cf
8B3F  dd 7e 01     LD A,(IX+1) ; estado
8B42  cd 50 40     CALL 0x4050 ; jump_table_a
8B45  4b 8b        .word 0x8B4B ; caso 0
8B47  68 8b        .word 0x8B68 ; caso 1
8B49  8d 8b        .word 0x8B8D ; caso 2
8B4B  dd 35 08     DEC (IX+8)
8B4E  c0           RET nz
8B4F  dd 7e 05     LD A,(IX+5) ; y
8B52  c6 10        ADD A,0x10
8B54  6f           LD L,A
8B55  dd 66 07     LD H,(IX+7) ; x
8B58  cd 6e 40     CALL 0x406e ; vec_4dc8
8B5B  30 03        JR nc,0x8b60
8B5D  dd 34 17     INC (IX+0x17) ; ay
8B60  dd 34 01     INC (IX+1) ; estado
8B63  cd da 8b     CALL 0x8bda
8B66  18 4f        JR 0x8bb7
8B68  dd 35 08     DEC (IX+8)
8B6B  20 2b        JR nz,0x8b98
8B6D  dd 36 08 20  LD (IX+8),0x20
8B71  dd 34 16     INC (IX+0x16) ; ay_lo
8B74  cd da 8b     CALL 0x8bda
8B77  dd 7e 16     LD A,(IX+0x16) ; ay_lo
8B7A  fe 08        CP 8
8B7C  d8           RET c
8B7D  dd 36 16 00  LD (IX+0x16),0 ; ay_lo
8B81  cd 39 8b     CALL 0x8b39
8B84  cd f5 96     CALL 0x96f5
8B87  cd da 8b     CALL 0x8bda
8B8A  c3 07 9a     JP 0x9a07
8B8D  dd 35 08     DEC (IX+8)
8B90  c0           RET nz
8B91  dd 36 08 20  LD (IX+8),0x20
8B95  c3 b8 7d     JP 0x7db8
8B98  dd 35 18     DEC (IX+0x18)
8B9B  c0           RET nz
8B9C  dd 36 18 03  LD (IX+0x18),3
8BA0  dd 7e 0a     LD A,(IX+10) ; d1
8BA3  dd be 14     CP (IX+0x14)
8BA6  dd 7e 15     LD A,(IX+0x15)
8BA9  28 03        JR z,0x8bae
8BAB  dd 7e 14     LD A,(IX+0x14)
8BAE  dd 77 0a     LD (IX+10),A ; d1
8BB1  21 17 8c     LD HL,0x8c17
8BB4  cd 88 b0     CALL 0xb088
8BB7  dd 7e 0a     LD A,(IX+10) ; d1
8BBA  fe c4        CP 0xc4
8BBC  28 0f        JR z,0x8bcd
8BBE  fe d4        CP 0xd4
8BC0  28 0b        JR z,0x8bcd
8BC2  b7           OR A
8BC3  ca f5 96     JP z,0x96f5
8BC6  3e c0        LD A,0xc0
8BC8  dd b6 02     OR (IX+2) ; flags
8BCB  18 0a        JR 0x8bd7
8BCD  3e 0f        LD A,15
8BCF  dd a6 02     AND (IX+2) ; flags
8BD2  3e 80        LD A,0x80
8BD4  dd b6 02     OR (IX+2) ; flags
8BD7  c3 fa 96     JP 0x96fa
8BDA  dd cb 17 46  BIT 0,(IX+0x17) ; ay
8BDE  21 0e 8c     LD HL,0x8c0e
8BE1  28 03        JR z,0x8be6
8BE3  21 05 8c     LD HL,0x8c05
8BE6  dd 7e 16     LD A,(IX+0x16) ; ay_lo
8BE9  cd 83 40     CALL 0x4083
8BEC  dd 77 14     LD (IX+0x14),A
8BEF  dd 77 0a     LD (IX+10),A ; d1
8BF2  23           INC HL
8BF3  7e           LD A,(HL)
8BF4  dd 77 15     LD (IX+0x15),A
8BF7  dd 36 08 20  LD (IX+8),0x20
8BFB  dd 36 18 03  LD (IX+0x18),3
8BFF  21 17 8c     LD HL,0x8c17
8C02  c3 88 b0     JP 0xb088
96F5  dd 7e 02     LD A,(IX+2) ; flags
96F8  e6 0f        AND 15
96FA  dd 77 02     LD (IX+2),A ; flags
96FD  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B088  dd 7e 0a     LD A,(IX+10) ; d1
B08B  d6 c4        SUB 0xc4
B08D  18 05        JR 0xb094
B094  1f           RRA
B095  1f           RRA
B096  e6 3f        AND 0x3f
B098  cd 83 40     CALL 0x4083
B09B  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B09E  c9           RET
