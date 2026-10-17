; tipo 0x3b  (hijo, no aparece en salas)  descriptor 81 c4 09 01 61
; ---- inicio (b04:66B4[58]) ----
6C42  dd cb 03 f6  SET 6,(IX+3) ; flags2
6C46  c9           RET
91E0  cd 42 6c     CALL 0x6c42
91E3  3a 04 e6     LD A,(0xe604)
91E6  e6 1f        AND 0x1f
91E8  d6 08        SUB 8
91EA  dd 86 05     ADD A,(IX+5) ; y
91ED  5f           LD E,A
91EE  dd 56 07     LD D,(IX+7) ; x
91F1  3e 60        LD A,0x60
91F3  cd 95 b1     CALL 0xb195
91F6  cd 33 b1     CALL 0xb133
91F9  3e 12        LD A,0x12
91FB  c3 59 40     JP 0x4059 ; sound_request
B133  cd 3e b1     CALL 0xb13e
B136  eb           EX DE,HL
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B195  4f           LD C,A
B196  3a 45 e0     LD A,(0xe045)
B199  87           ADD A,A
B19A  87           ADD A,A
B19B  87           ADD A,A
B19C  81           ADD A,C
B19D  32 09 e6     LD (0xe609),A
B1A0  cd 23 b2     CALL 0xb223
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
B223  3a 05 e5     LD A,(0xe505) ; jugador_y
B226  d6 10        SUB 0x10
B228  32 00 f0     LD (0xf000),A
B22B  3a 07 e5     LD A,(0xe507) ; jugador_x
B22E  d6 08        SUB 8
B230  32 01 f0     LD (0xf001),A
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
; ---- actualizacion (b06:B472[58]) ----
91DF  c9           RET
