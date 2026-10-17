; tipo 0x57  (hijo, no aparece en salas)  descriptor 40 00 01 ff 00
; ---- inicio (b04:66B4[86]) ----
AD51  dd cb 02 a6  RES 4,(IX+2) ; flags
AD55  3a 24 e6     LD A,(0xe624)
AD58  dd 77 14     LD (IX+0x14),A
AD5B  a7           AND A
AD5C  06 0c        LD B,12
AD5E  28 02        JR z,0xad62
AD60  06 00        LD B,0
AD62  dd 70 0a     LD (IX+10),B ; d1
AD65  dd 36 1f 01  LD (IX+0x1f),1
AD69  dd cb 14 4e  BIT 1,(IX+0x14)
AD6D  dd 36 1c 0d  LD (IX+0x1c),13
AD71  28 04        JR z,0xad77
AD73  dd 36 1c 09  LD (IX+0x1c),9
AD77  3e 1c        LD A,0x1c
AD79  cd 59 40     CALL 0x4059 ; sound_request
AD7C  dd 6e 05     LD L,(IX+5) ; y
AD7F  dd 66 07     LD H,(IX+7) ; x
AD82  cd 56 40     CALL 0x4056 ; vec_4da1
AD85  dd 75 12     LD (IX+0x12),L
AD88  dd 74 13     LD (IX+0x13),H
AD8B  c9           RET
; ---- actualizacion (b06:B472[86]) ----
67AF  4d           LD C,L
67B0  26 00        LD H,0
67B2  44           LD B,H
67B3  29           ADD HL,HL
67B4  29           ADD HL,HL
67B5  09           ADD HL,BC
67B6  01 b6 67     LD BC,0x67b6
67B9  09           ADD HL,BC
67BA  c9           RET
AD85  dd 75 12     LD (IX+0x12),L
AD88  dd 74 13     LD (IX+0x13),H
AD8B  c9           RET
AD8C  dd 34 0a     INC (IX+10) ; d1
AD8F  dd 34 1f     INC (IX+0x1f)
AD92  dd 7e 1f     LD A,(IX+0x1f)
AD95  dd be 1c     CP (IX+0x1c)
AD98  ca 1c b4     JP z,0xb41c
AD9B  dd 6e 12     LD L,(IX+0x12)
AD9E  dd 66 13     LD H,(IX+0x13)
ADA1  dd 7e 14     LD A,(IX+0x14)
ADA4  a7           AND A
ADA5  c8           RET z
ADA6  dd 7e 07     LD A,(IX+7) ; x
ADA9  d6 08        SUB 8
ADAB  dd 77 07     LD (IX+7),A ; x
ADAE  2b           DEC HL
ADAF  c3 85 ad     JP 0xad85
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
