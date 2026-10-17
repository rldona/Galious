; tipo 0x63  (hijo, no aparece en salas)  descriptor c4 0c 0d ff 00
; ---- inicio (b04:66B4[98]) ----
6A13  dd 36 02 80  LD (IX+2),0x80 ; flags
6A17  c9           RET
6AC4  dd 36 08 0a  LD (IX+8),10
6AC8  c3 13 6a     JP 0x6a13
; ---- actualizacion (b06:B472[98]) ----
67AF  4d           LD C,L
67B0  26 00        LD H,0
67B2  44           LD B,H
67B3  29           ADD HL,HL
67B4  29           ADD HL,HL
67B5  09           ADD HL,BC
67B6  01 b6 67     LD BC,0x67b6
67B9  09           ADD HL,BC
67BA  c9           RET
6ACB  dd 35 08     DEC (IX+8)
6ACE  c0           RET nz
6ACF  dd 36 08 0a  LD (IX+8),10
6AD3  dd 34 01     INC (IX+1) ; estado
6AD6  dd 7e 01     LD A,(IX+1) ; estado
6AD9  fe 04        CP 4
6ADB  d2 1c b4     JP nc,0xb41c
6ADE  cb 47        BIT 0,A
6AE0  3e 0c        LD A,12
6AE2  28 01        JR z,0x6ae5
6AE4  3c           INC A
6AE5  c3 83 70     JP 0x7083
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
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
