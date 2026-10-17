; tipo 0x36  vida None  patron observado: vuela
; ---- inicio (b04:66B4[53]) ----
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
8A39  dd cb 02 d6  SET 2,(IX+2) ; flags
8A3D  cd f1 81     CALL 0x81f1
8A40  e6 03        AND 3
8A42  3c           INC A
8A43  dd 77 08     LD (IX+8),A
8A46  dd 7e 05     LD A,(IX+5) ; y
8A49  dd 77 18     LD (IX+0x18),A
8A4C  dd 7e 07     LD A,(IX+7) ; x
8A4F  dd 77 19     LD (IX+0x19),A
8A52  11 00 00     LD DE,0
8A55  21 00 02     LD HL,0x200
8A58  cd 33 b1     CALL 0xb133
8A5B  3a 33 e6     LD A,(0xe633)
8A5E  dd 77 14     LD (IX+0x14),A
8A61  3c           INC A
8A62  32 33 e6     LD (0xe633),A
8A65  c6 02        ADD A,2
8A67  dd 77 0b     LD (IX+11),A ; d2
8A6A  fe 0a        CP 10
8A6C  20 06        JR nz,0x8a74
8A6E  cd 24 b1     CALL 0xb124
8A71  dd 34 01     INC (IX+1) ; estado
8A74  3e 08        LD A,8
8A76  32 34 e6     LD (0xe634),A
8A79  c9           RET
B124  cd 2e b1     CALL 0xb12e
B127  18 0e        JR 0xb137
B12E  16 00        LD D,0
B130  5a           LD E,D
B131  18 0b        JR 0xb13e
B133  cd 3e b1     CALL 0xb13e
B136  eb           EX DE,HL
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[53]) ----
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
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
8A7A  11 04 0c     LD DE,0xc04
8A7D  06 03        LD B,3
8A7F  cd d6 b0     CALL 0xb0d6
8A82  dd 7e 01     LD A,(IX+1) ; estado
8A85  3d           DEC A
8A86  28 12        JR z,0x8a9a
8A88  3d           DEC A
8A89  28 3c        JR z,0x8ac7
8A8B  11 80 00     LD DE,0x80
8A8E  dd 7e 18     LD A,(IX+0x18)
8A91  dd be 05     CP (IX+5) ; y
8A94  dc 68 40     CALL c,0x4068 ; neg_de
8A97  c3 1c 82     JP 0x821c
8A9A  3a 34 e6     LD A,(0xe634)
8A9D  47           LD B,A
8A9E  cd f1 81     CALL 0x81f1
8AA1  e6 07        AND 7
8AA3  b8           CP B
8AA4  28 f4        JR z,0x8a9a
8AA6  32 34 e6     LD (0xe634),A
8AA9  dd 34 01     INC (IX+1) ; estado
8AAC  dd e5        PUSH IX
8AAE  cd 14 8b     CALL 0x8b14
8AB1  dd 66 18     LD H,(IX+0x18)
8AB4  dd 6e 19     LD L,(IX+0x19)
8AB7  dd e1        POP IX
8AB9  dd 74 15     LD (IX+0x15),H
8ABC  dd 75 16     LD (IX+0x16),L ; ay_lo
8ABF  3e c0        LD A,0xc0
8AC1  cd fd 81     CALL 0x81fd
8AC4  c3 33 b1     JP 0xb133
8AC7  dd 7e 15     LD A,(IX+0x15)
8ACA  d6 18        SUB 0x18
8ACC  dd be 05     CP (IX+5) ; y
8ACF  d0           RET nc
8AD0  c6 30        ADD A,0x30
8AD2  dd be 05     CP (IX+5) ; y
8AD5  d8           RET c
8AD6  dd 7e 16     LD A,(IX+0x16) ; ay_lo
8AD9  d6 12        SUB 0x12
8ADB  dd be 07     CP (IX+7) ; x
8ADE  d0           RET nc
8ADF  c6 24        ADD A,0x24
8AE1  dd be 07     CP (IX+7) ; x
8AE4  d8           RET c
8AE5  3e 0c        LD A,12
8AE7  cd 59 40     CALL 0x4059 ; sound_request
8AEA  af           XOR A
8AEB  dd 77 01     LD (IX+1),A ; estado
8AEE  57           LD D,A
8AEF  5f           LD E,A
8AF0  6f           LD L,A
8AF1  26 02        LD H,2
8AF3  cd 33 b1     CALL 0xb133
8AF6  dd 66 15     LD H,(IX+0x15)
8AF9  dd 6e 16     LD L,(IX+0x16) ; ay_lo
8AFC  dd 74 05     LD (IX+5),H ; y
8AFF  dd 75 07     LD (IX+7),L ; x
8B02  dd 74 18     LD (IX+0x18),H
8B05  dd 75 19     LD (IX+0x19),L
8B08  dd e5        PUSH IX
8B0A  cd 14 8b     CALL 0x8b14
8B0D  dd 36 01 01  LD (IX+1),1 ; estado
8B11  dd e1        POP IX
8B13  c9           RET
8B14  dd 21 00 e8  LD IX,0xe800 ; enemy_slots
8B18  06 08        LD B,8
8B1A  11 20 00     LD DE,0x20
8B1D  dd 7e 00     LD A,(IX) ; tipo
8B20  fe 36        CP 0x36
8B22  28 05        JR z,0x8b29
8B24  dd 19        ADD IX,DE
8B26  10 f5        DJNZ 0x8b1d
8B28  c9           RET
8B29  3a 34 e6     LD A,(0xe634)
8B2C  dd be 14     CP (IX+0x14)
8B2F  20 f3        JR nz,0x8b24
8B31  c9           RET
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
