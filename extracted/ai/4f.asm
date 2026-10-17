; tipo 0x4f  (hijo, no aparece en salas)  descriptor 82 a4 0f 00 00
; ---- inicio (b04:66B4[78]) ----
82F9  dd 36 09 81  LD (IX+9),0x81 ; d0
82FD  dd cb 02 ae  RES 5,(IX+2) ; flags
8301  dd cb 02 b6  RES 6,(IX+2) ; flags
8305  dd 36 19 50  LD (IX+0x19),0x50
8309  dd 36 1a 02  LD (IX+0x1a),2
830D  dd 36 18 00  LD (IX+0x18),0
8311  c3 24 b1     JP 0xb124
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
; ---- actualizacion (b06:B472[78]) ----
7DB8  dd 36 01 01  LD (IX+1),1 ; estado
7DBC  c9           RET
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
8145  dd 7e 01     LD A,(IX+1) ; estado
8148  3d           DEC A
8149  28 1f        JR z,0x816a
814B  3d           DEC A
814C  28 27        JR z,0x8175
814E  3d           DEC A
814F  28 46        JR z,0x8197
8151  06 01        LD B,1
8153  cd f1 81     CALL 0x81f1
8156  e6 0f        AND 15
8158  cd 25 7e     CALL 0x7e25
815B  cd f1 81     CALL 0x81f1
815E  e6 1f        AND 0x1f
8160  fe 03        CP 3
8162  38 f7        JR c,0x815b
8164  dd 77 17     LD (IX+0x17),A ; ay
8167  c3 07 9a     JP 0x9a07
816A  cd b2 81     CALL 0x81b2
816D  dd 35 17     DEC (IX+0x17) ; ay
8170  c0           RET nz
8171  dd 35 01     DEC (IX+1) ; estado
8174  c9           RET
8175  dd 34 01     INC (IX+1) ; estado
8178  dd 36 17 18  LD (IX+0x17),0x18 ; ay
817C  dd 7e 16     LD A,(IX+0x16) ; ay_lo
817F  21 9e 81     LD HL,0x819e
8182  cd 53 40     CALL 0x4053 ; read_word_table
8185  cd f1 81     CALL 0x81f1
8188  e6 03        AND 3
818A  fe 03        CP 3
818C  28 f7        JR z,0x8185
818E  cd 65 40     CALL 0x4065 ; add_a_to_de
8191  1a           LD A,(DE)
8192  06 01        LD B,1
8194  c3 25 7e     JP 0x7e25
8197  dd 35 17     DEC (IX+0x17) ; ay
819A  c0           RET nz
819B  c3 b8 7d     JP 0x7db8
81B2  01 38 28     LD BC,0x2838
81B5  11 71 50     LD DE,0x5071
81B8  dd cb 09 56  BIT 2,(IX+9) ; d0
81BC  28 06        JR z,0x81c4
81BE  01 48 38     LD BC,0x3848
81C1  11 81 60     LD DE,0x6081
81C4  dd 7e 07     LD A,(IX+7) ; x
81C7  80           ADD A,B
81C8  b9           CP C
81C9  38 06        JR c,0x81d1
81CB  dd 7e 05     LD A,(IX+5) ; y
81CE  82           ADD A,D
81CF  bb           CP E
81D0  d0           RET nc
81D1  dd 36 16 00  LD (IX+0x16),0 ; ay_lo
81D5  dd 7e 05     LD A,(IX+5) ; y
81D8  fe 70        CP 0x70
81DA  38 04        JR c,0x81e0
81DC  dd 36 16 02  LD (IX+0x16),2 ; ay_lo
81E0  dd 7e 07     LD A,(IX+7) ; x
81E3  fe 80        CP 0x80
81E5  38 03        JR c,0x81ea
81E7  dd 34 16     INC (IX+0x16) ; ay_lo
81EA  dd 36 17 02  LD (IX+0x17),2 ; ay
81EE  c3 07 9a     JP 0x9a07
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
8314  dd 7e 18     LD A,(IX+0x18)
8317  3d           DEC A
8318  28 3a        JR z,0x8354
831A  dd 35 19     DEC (IX+0x19)
831D  28 11        JR z,0x8330
831F  dd 35 1a     DEC (IX+0x1a)
8322  c0           RET nz
8323  dd 36 1a 02  LD (IX+0x1a),2
8327  dd 7e 0a     LD A,(IX+10) ; d1
832A  ee 0c        XOR 12
832C  dd 77 0a     LD (IX+10),A ; d1
832F  c9           RET
8330  dd 36 11 13  LD (IX+0x11),0x13 ; atributo_sprite
8334  dd cb 02 f6  SET 6,(IX+2) ; flags
8338  dd 34 18     INC (IX+0x18)
833B  dd 36 09 c2  LD (IX+9),0xc2 ; d0
833F  dd 36 0a 00  LD (IX+10),0 ; d1
8343  dd 36 0b 08  LD (IX+11),8 ; d2
8347  dd 36 1a 04  LD (IX+0x1a),4
834B  dd 36 1b 80  LD (IX+0x1b),0x80
834F  3e 89        LD A,0x89
8351  c3 80 40     JP 0x4080
8354  dd 7e 1b     LD A,(IX+0x1b)
8357  dd ae 0f     XOR (IX+15) ; vx
835A  e6 80        AND 0x80
835C  28 1a        JR z,0x8378
835E  dd 36 1a 01  LD (IX+0x1a),1
8362  01 08 00     LD BC,8
8365  dd 7e 0f     LD A,(IX+15) ; vx
8368  dd 77 1b     LD (IX+0x1b),A
836B  e6 80        AND 0x80
836D  28 03        JR z,0x8372
836F  01 09 02     LD BC,0x209
8372  dd 70 0a     LD (IX+10),B ; d1
8375  dd 71 0b     LD (IX+11),C ; d2
8378  dd 35 1a     DEC (IX+0x1a)
837B  c2 45 81     JP nz,0x8145
837E  dd 36 1a 02  LD (IX+0x1a),2
8382  dd 7e 0a     LD A,(IX+10) ; d1
8385  ee 01        XOR 1
8387  dd 77 0a     LD (IX+10),A ; d1
838A  c3 45 81     JP 0x8145
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
