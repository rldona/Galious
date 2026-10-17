; tipo 0x6a  (hijo, no aparece en salas)  descriptor 81 c4 0f ff 0b
; ---- inicio (b04:66B4[105]) ----
6B71  dd cb 02 a6  RES 4,(IX+2) ; flags
6B75  c9           RET
7937  dd 36 08 0a  LD (IX+8),10
793B  cd 71 6b     CALL 0x6b71
793E  3a 24 e6     LD A,(0xe624)
7941  a7           AND A
7942  c8           RET z
7943  3d           DEC A
7944  28 05        JR z,0x794b
7946  dd 36 0a c0  LD (IX+10),0xc0 ; d1
794A  c9           RET
794B  dd 36 0b 09  LD (IX+11),9 ; d2
794F  c9           RET
; ---- actualizacion (b06:B472[105]) ----
67AF  4d           LD C,L
67B0  26 00        LD H,0
67B2  44           LD B,H
67B3  29           ADD HL,HL
67B4  29           ADD HL,HL
67B5  09           ADD HL,BC
67B6  01 b6 67     LD BC,0x67b6
67B9  09           ADD HL,BC
67BA  c9           RET
6C47  dd 35 08     DEC (IX+8)
6C4A  c2 a1 98     JP nz,0x98a1
6C4D  c3 1c b4     JP 0xb41c
98A1  dd 7e 00     LD A,(IX) ; tipo
98A4  fe 6a        CP 0x6a
98A6  c8           RET z
98A7  dd cb 0f 7e  BIT 7,(IX+15) ; vx
98AB  06 ff        LD B,0xff
98AD  20 02        JR nz,0x98b1
98AF  06 10        LD B,0x10
98B1  dd 7e 07     LD A,(IX+7) ; x
98B4  80           ADD A,B
98B5  67           LD H,A
98B6  dd 6e 05     LD L,(IX+5) ; y
98B9  cd 6e 40     CALL 0x406e ; vec_4dc8
98BC  da 1c b4     JP c,0xb41c
98BF  c9           RET
B41C  cd 3d b4     CALL 0xb43d
B41F  dd 6e 00     LD L,(IX) ; tipo
B422  cd af 67     CALL 0x67af
B425  eb           EX DE,HL
B426  af           XOR A
B427  dd 77 00     LD (IX),A ; tipo
B42A  dd 77 02     LD (IX+2),A ; flags
B42D  21 20 e6     LD HL,0xe620
B430  35           DEC (HL)
B431  2c           INC L
B432  1a           LD A,(DE)
B433  cb 7f        BIT 7,A
B435  c8           RET z
B436  e6 07        AND 7
B438  47           LD B,A
B439  7e           LD A,(HL)
B43A  90           SUB B
B43B  77           LD (HL),A
B43C  c9           RET
B43D  dd 7e 03     LD A,(IX+3) ; flags2
B440  cb 5f        BIT 3,A
B442  c8           RET z
B443  e6 07        AND 7
B445  3c           INC A
B446  47           LD B,A
B447  af           XOR A
B448  37           SCF
B449  17           RLA
B44A  10 fd        DJNZ 0xb449
B44C  2a 0c e6     LD HL,(0xe60c)
B44F  b6           OR (HL)
B450  77           LD (HL),A
B451  c9           RET
