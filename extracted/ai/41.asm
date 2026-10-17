; tipo 0x41  (hijo, no aparece en salas)  descriptor 81 a4 0a ff 00
; ---- inicio (b04:66B4[64]) ----
6C42  dd cb 03 f6  SET 6,(IX+3) ; flags2
6C46  c9           RET
8D7F  cd 42 6c     CALL 0x6c42
8D82  11 08 08     LD DE,0x808
8D85  cd db b1     CALL 0xb1db
8D88  c6 10        ADD A,0x10
8D8A  07           RLCA
8D8B  07           RLCA
8D8C  07           RLCA
8D8D  e6 07        AND 7
8D8F  87           ADD A,A
8D90  87           ADD A,A
8D91  87           ADD A,A
8D92  4f           LD C,A
8D93  06 00        LD B,0
8D95  21 c5 8d     LD HL,0x8dc5
8D98  09           ADD HL,BC
8D99  7e           LD A,(HL)
8D9A  dd 77 0a     LD (IX+10),A ; d1
8D9D  23           INC HL
8D9E  5e           LD E,(HL)
8D9F  23           INC HL
8DA0  56           LD D,(HL)
8DA1  cd 37 b1     CALL 0xb137
8DA4  23           INC HL
8DA5  5e           LD E,(HL)
8DA6  23           INC HL
8DA7  56           LD D,(HL)
8DA8  23           INC HL
8DA9  cd 3e b1     CALL 0xb13e
8DAC  7e           LD A,(HL)
8DAD  23           INC HL
8DAE  dd 86 05     ADD A,(IX+5) ; y
8DB1  dd 77 05     LD (IX+5),A ; y
8DB4  7e           LD A,(HL)
8DB5  dd 86 07     ADD A,(IX+7) ; x
8DB8  dd 77 07     LD (IX+7),A ; x
8DBB  23           INC HL
8DBC  7e           LD A,(HL)
8DBD  dd 77 11     LD (IX+0x11),A ; atributo_sprite
8DC0  3e 19        LD A,0x19
8DC2  c3 59 40     JP 0x4059 ; sound_request
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
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
; ---- actualizacion (b06:B472[64]) ----
8D28  c9           RET
