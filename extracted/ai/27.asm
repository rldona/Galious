; tipo 0x27  vida 12  patron observado: vuela
; ---- inicio (b04:66B4[38]) ----
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
8935  dd 36 08 04  LD (IX+8),4
8939  cd f1 81     CALL 0x81f1
893C  e6 0f        AND 15
893E  dd 77 1f     LD (IX+0x1f),A
8941  c9           RET
; ---- actualizacion (b06:B472[38]) ----
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
81FB  3e 80        LD A,0x80
81FD  dd 5e 05     LD E,(IX+5) ; y
8200  dd 56 07     LD D,(IX+7) ; x
8203  4f           LD C,A
8204  3a 45 e0     LD A,(0xe045)
8207  87           ADD A,A
8208  87           ADD A,A
8209  87           ADD A,A
820A  81           ADD A,C
820B  32 09 e6     LD (0xe609),A
820E  7c           LD A,H
820F  32 00 f0     LD (0xf000),A
8212  7d           LD A,L
8213  32 01 f0     LD (0xf001),A
8216  cd 33 b2     CALL 0xb233
8219  c3 a3 b1     JP 0xb1a3
821C  dd 66 0d     LD H,(IX+13) ; vy
821F  dd 6e 0c     LD L,(IX+12) ; vy_lo
8222  19           ADD HL,DE
8223  dd 74 0d     LD (IX+13),H ; vy
8226  dd 75 0c     LD (IX+12),L ; vy_lo
8229  c9           RET
8939  cd f1 81     CALL 0x81f1
893C  e6 0f        AND 15
893E  dd 77 1f     LD (IX+0x1f),A
8941  c9           RET
8942  3a 03 e0     LD A,(0xe003) ; frame_counter
8945  e6 3f        AND 0x3f
8947  20 05        JR nz,0x894e
8949  3e 06        LD A,6
894B  cd 59 40     CALL 0x4059 ; sound_request
894E  3e c4        LD A,0xc4
8950  dd be 0a     CP (IX+10) ; d1
8953  20 0e        JR nz,0x8963
8955  dd 7e 1c     LD A,(IX+0x1c)
8958  1f           RRA
8959  30 10        JR nc,0x896b
895B  dd 36 11 00  LD (IX+0x11),0 ; atributo_sprite
895F  dd 36 0a c8  LD (IX+10),0xc8 ; d1
8963  11 04 04     LD DE,0x404
8966  06 00        LD B,0
8968  cd d6 b0     CALL 0xb0d6
896B  dd 7e 01     LD A,(IX+1) ; estado
896E  fe 04        CP 4
8970  28 63        JR z,0x89d5
8972  fe 05        CP 5
8974  ca 00 8a     JP z,0x8a00
8977  dd 35 1f     DEC (IX+0x1f)
897A  20 0b        JR nz,0x8987
897C  cd 39 89     CALL 0x8939
897F  dd 7e 1a     LD A,(IX+0x1a)
8982  ee 01        XOR 1
8984  dd 77 1a     LD (IX+0x1a),A
8987  dd 7e 1a     LD A,(IX+0x1a)
898A  a7           AND A
898B  c2 45 81     JP nz,0x8145
898E  3a 05 e5     LD A,(0xe505) ; jugador_y
8991  47           LD B,A
8992  dd 7e 05     LD A,(IX+5) ; y
8995  d6 30        SUB 0x30
8997  b8           CP B
8998  d2 45 81     JP nc,0x8145
899B  c6 70        ADD A,0x70
899D  b8           CP B
899E  da 45 81     JP c,0x8145
89A1  3a 07 e5     LD A,(0xe507) ; jugador_x
89A4  47           LD B,A
89A5  dd 7e 07     LD A,(IX+7) ; x
89A8  d6 38        SUB 0x38
89AA  b8           CP B
89AB  d2 45 81     JP nc,0x8145
89AE  c6 70        ADD A,0x70
89B0  b8           CP B
89B1  da 45 81     JP c,0x8145
89B4  dd 7e 05     LD A,(IX+5) ; y
89B7  c6 58        ADD A,0x58
89B9  fe 88        CP 0x88
89BB  da 45 81     JP c,0x8145
89BE  dd 7e 05     LD A,(IX+5) ; y
89C1  dd 77 19     LD (IX+0x19),A
89C4  dd 36 01 04  LD (IX+1),4 ; estado
89C8  dd 36 1b 20  LD (IX+0x1b),0x20
89CC  11 00 00     LD DE,0
89CF  21 00 03     LD HL,0x300
89D2  c3 33 b1     JP 0xb133
89D5  dd 35 1b     DEC (IX+0x1b)
89D8  28 0f        JR z,0x89e9
89DA  11 80 00     LD DE,0x80
89DD  dd 7e 19     LD A,(IX+0x19)
89E0  dd be 05     CP (IX+5) ; y
89E3  dc 68 40     CALL c,0x4068 ; neg_de
89E6  c3 1c 82     JP 0x821c
89E9  dd 34 01     INC (IX+1) ; estado
89EC  3a 05 e5     LD A,(0xe505) ; jugador_y
89EF  dd 77 1d     LD (IX+0x1d),A
89F2  67           LD H,A
89F3  3a 07 e5     LD A,(0xe507) ; jugador_x
89F6  dd 77 1e     LD (IX+0x1e),A
89F9  6f           LD L,A
89FA  cd fb 81     CALL 0x81fb
89FD  c3 33 b1     JP 0xb133
8A00  dd 7e 05     LD A,(IX+5) ; y
8A03  c6 58        ADD A,0x58
8A05  fe 78        CP 0x78
8A07  38 27        JR c,0x8a30
8A09  dd 7e 07     LD A,(IX+7) ; x
8A0C  c6 10        ADD A,0x10
8A0E  fe 18        CP 0x18
8A10  38 1e        JR c,0x8a30
8A12  dd 7e 1d     LD A,(IX+0x1d)
8A15  d6 14        SUB 0x14
8A17  dd be 05     CP (IX+5) ; y
8A1A  d0           RET nc
8A1B  c6 28        ADD A,0x28
8A1D  dd be 05     CP (IX+5) ; y
8A20  d8           RET c
8A21  dd 7e 1e     LD A,(IX+0x1e)
8A24  d6 14        SUB 0x14
8A26  dd be 07     CP (IX+7) ; x
8A29  d0           RET nc
8A2A  c6 28        ADD A,0x28
8A2C  dd be 07     CP (IX+7) ; x
8A2F  d8           RET c
8A30  dd 36 01 00  LD (IX+1),0 ; estado
8A34  dd 36 1a 01  LD (IX+0x1a),1
8A38  c9           RET
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
B133  cd 3e b1     CALL 0xb13e
B136  eb           EX DE,HL
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B1A3  3a 17 f0     LD A,(0xf017)
B1A6  5f           LD E,A
B1A7  16 00        LD D,0
B1A9  d6 3f        SUB 0x3f
B1AB  ed 44        NEG
B1AD  21 74 b3     LD HL,0xb374
B1B0  e5           PUSH HL
B1B1  19           ADD HL,DE
B1B2  4e           LD C,(HL)
B1B3  e1           POP HL
B1B4  5f           LD E,A
B1B5  19           ADD HL,DE
B1B6  7e           LD A,(HL)
B1B7  32 16 f0     LD (0xf016),A
B1BA  59           LD E,C
B1BB  cd 62 b2     CALL 0xb262
B1BE  3a 10 f0     LD A,(0xf010)
B1C1  a7           AND A
B1C2  c4 68 40     CALL nz,0x4068 ; neg_de
B1C5  ed 53 12 f0  LD (0xf012),DE
B1C9  3a 16 f0     LD A,(0xf016)
B1CC  5f           LD E,A
B1CD  cd 62 b2     CALL 0xb262
B1D0  3a 11 f0     LD A,(0xf011)
B1D3  a7           AND A
B1D4  c4 68 40     CALL nz,0x4068 ; neg_de
B1D7  2a 12 f0     LD HL,(0xf012)
B1DA  c9           RET
B233  21 10 f0     LD HL,0xf010
B236  36 00        LD (HL),0
B238  3a 00 f0     LD A,(0xf000)
B23B  93           SUB E
B23C  30 03        JR nc,0xb241
B23E  ed 44        NEG
B240  34           INC (HL)
B241  23           INC HL
B242  36 00        LD (HL),0
B244  e6 f0        AND 0xf0
B246  5f           LD E,A
B247  3a 01 f0     LD A,(0xf001)
B24A  92           SUB D
B24B  30 03        JR nc,0xb250
B24D  ed 44        NEG
B24F  34           INC (HL)
B250  1f           RRA
B251  1f           RRA
B252  1f           RRA
B253  1f           RRA
B254  e6 0f        AND 15
B256  83           ADD A,E
B257  21 74 b2     LD HL,0xb274
B25A  cd 62 40     CALL 0x4062 ; add_a_to_hl
B25D  7e           LD A,(HL)
B25E  32 17 f0     LD (0xf017),A
B261  c9           RET
B262  3a 09 e6     LD A,(0xe609)
B265  67           LD H,A
B266  cd 6b 40     CALL 0x406b ; vec_4dcf
B269  af           XOR A
B26A  29           ADD HL,HL
B26B  8f           ADC A,A
B26C  29           ADD HL,HL
B26D  8f           ADC A,A
B26E  29           ADD HL,HL
B26F  8f           ADC A,A
B270  6c           LD L,H
B271  67           LD H,A
B272  eb           EX DE,HL
B273  c9           RET
