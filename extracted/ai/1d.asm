; tipo 0x1d  (hijo, no aparece en salas)  descriptor 81 a0 0b ff 05
; ---- inicio (b04:66B4[28]) ----
6C42  dd cb 03 f6  SET 6,(IX+3) ; flags2
6C46  c9           RET
; ---- actualizacion (b06:B472[28]) ----
67AF  4d           LD C,L
67B0  26 00        LD H,0
67B2  44           LD B,H
67B3  29           ADD HL,HL
67B4  29           ADD HL,HL
67B5  09           ADD HL,BC
67B6  01 b6 67     LD BC,0x67b6
67B9  09           ADD HL,BC
67BA  c9           RET
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
