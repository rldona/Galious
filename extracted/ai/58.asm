; tipo 0x58  (hijo, no aparece en salas)  descriptor 81 a0 07 01 6f
; ---- inicio (b04:66B4[87]) ----
6C42  dd cb 03 f6  SET 6,(IX+3) ; flags2
6C46  c9           RET
ADB2  cd 42 6c     CALL 0x6c42
ADB5  3e 70        LD A,0x70
ADB7  4f           LD C,A
ADB8  3a 45 e0     LD A,(0xe045)
ADBB  87           ADD A,A
ADBC  87           ADD A,A
ADBD  87           ADD A,A
ADBE  81           ADD A,C
ADBF  32 09 e6     LD (0xe609),A
ADC2  11 08 08     LD DE,0x808
ADC5  cd db b1     CALL 0xb1db
ADC8  f5           PUSH AF
ADC9  3a 24 e6     LD A,(0xe624)
ADCC  d6 03        SUB 3
ADCE  87           ADD A,A
ADCF  87           ADD A,A
ADD0  87           ADD A,A
ADD1  47           LD B,A
ADD2  ed 5f        LD A,R
ADD4  e6 0f        AND 15
ADD6  d6 08        SUB 8
ADD8  80           ADD A,B
ADD9  47           LD B,A
ADDA  f1           POP AF
ADDB  80           ADD A,B
ADDC  cd ff b1     CALL 0xb1ff
ADDF  cd a3 b1     CALL 0xb1a3
ADE2  c3 33 b1     JP 0xb133
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
B1DB  dd 7e 05     LD A,(IX+5) ; y
B1DE  83           ADD A,E
B1DF  5f           LD E,A
B1E0  dd 7e 07     LD A,(IX+7) ; x
B1E3  82           ADD A,D
B1E4  57           LD D,A
B1E5  cd 23 b2     CALL 0xb223
B1E8  4f           LD C,A
B1E9  2a 10 f0     LD HL,(0xf010)
B1EC  7c           LD A,H
B1ED  06 00        LD B,0
B1EF  a7           AND A
B1F0  28 02        JR z,0xb1f4
B1F2  06 80        LD B,0x80
B1F4  bd           CP L
B1F5  79           LD A,C
B1F6  28 02        JR z,0xb1fa
B1F8  ed 44        NEG
B1FA  80           ADD A,B
B1FB  32 18 f0     LD (0xf018),A
B1FE  c9           RET
B1FF  4f           LD C,A
B200  87           ADD A,A
B201  17           RLA
B202  e6 01        AND 1
B204  6f           LD L,A
B205  79           LD A,C
B206  c6 40        ADD A,0x40
B208  87           ADD A,A
B209  17           RLA
B20A  e6 01        AND 1
B20C  67           LD H,A
B20D  22 10 f0     LD (0xf010),HL
B210  79           LD A,C
B211  fe 80        CP 0x80
B213  38 02        JR c,0xb217
B215  d6 80        SUB 0x80
B217  fe 40        CP 0x40
B219  38 04        JR c,0xb21f
B21B  d6 7f        SUB 0x7f
B21D  ed 44        NEG
B21F  32 17 f0     LD (0xf017),A
B222  c9           RET
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
; ---- actualizacion (b06:B472[87]) ----
ADF4  16 04        LD D,4
ADF6  06 62        LD B,0x62
ADF8  c3 7f b1     JP 0xb17f
B0DD  dd 7e 0a     LD A,(IX+10) ; d1
B0E0  aa           XOR D
B0E1  dd 77 0a     LD (IX+10),A ; d1
B0E4  dd 7e 11     LD A,(IX+0x11) ; atributo_sprite
B0E7  a8           XOR B
B0E8  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B0EB  af           XOR A
B0EC  c9           RET
B17F  0e 07        LD C,7
B181  18 02        JR 0xb185
B185  3a 04 e6     LD A,(0xe604)
B188  a1           AND C
B189  c0           RET nz
B18A  c3 dd b0     JP 0xb0dd
