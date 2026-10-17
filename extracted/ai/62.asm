; tipo 0x62  (hijo, no aparece en salas)  descriptor 86 c4 08 04 00
; ---- inicio (b04:66B4[97]) ----
8544  dd 36 09 81  LD (IX+9),0x81 ; d0
8548  dd cb 02 a6  RES 4,(IX+2) ; flags
854C  01 c4 fc     LD BC,0xfcc4
854F  21 00 fc     LD HL,0xfc00
8552  16 00        LD D,0
8554  dd 7e 05     LD A,(IX+5) ; y
8557  fe 30        CP 0x30
8559  38 0c        JR c,0x8567
855B  fe a0        CP 0xa0
855D  30 0f        JR nc,0x856e
855F  3a 05 e5     LD A,(0xe505) ; jugador_y
8562  dd be 05     CP (IX+5) ; y
8565  38 07        JR c,0x856e
8567  01 cc 0c     LD BC,0xccc
856A  14           INC D
856B  cd 5c 40     CALL 0x405c ; neg_hl
856E  dd 7e 05     LD A,(IX+5) ; y
8571  80           ADD A,B
8572  dd 77 05     LD (IX+5),A ; y
8575  dd 71 0a     LD (IX+10),C ; d1
8578  dd 72 15     LD (IX+0x15),D
857B  11 00 00     LD DE,0
857E  c3 33 b1     JP 0xb133
B133  cd 3e b1     CALL 0xb13e
B136  eb           EX DE,HL
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[97]) ----
67AF  4d           LD C,L
67B0  26 00        LD H,0
67B2  44           LD B,H
67B3  29           ADD HL,HL
67B4  29           ADD HL,HL
67B5  09           ADD HL,BC
67B6  01 b6 67     LD BC,0x67b6
67B9  09           ADD HL,BC
67BA  c9           RET
8581  dd 7e 01     LD A,(IX+1) ; estado
8584  3d           DEC A
8585  28 2a        JR z,0x85b1
8587  3d           DEC A
8588  28 73        JR z,0x85fd
858A  3d           DEC A
858B  ca 0b 86     JP z,0x860b
858E  3e 17        LD A,0x17
8590  cd 59 40     CALL 0x4059 ; sound_request
8593  dd 34 01     INC (IX+1) ; estado
8596  dd 36 18 00  LD (IX+0x18),0
859A  dd 36 19 00  LD (IX+0x19),0
859E  dd 36 09 c2  LD (IX+9),0xc2 ; d0
85A2  dd 36 0b 0c  LD (IX+11),12 ; d2
85A6  dd 7e 05     LD A,(IX+5) ; y
85A9  dd 77 16     LD (IX+0x16),A ; ay_lo
85AC  dd cb 02 e6  SET 4,(IX+2) ; flags
85B0  c9           RET
85B1  dd 7e 15     LD A,(IX+0x15)
85B4  3d           DEC A
85B5  dd 7e 05     LD A,(IX+5) ; y
85B8  28 06        JR z,0x85c0
85BA  fe 20        CP 0x20
85BC  38 20        JR c,0x85de
85BE  18 04        JR 0x85c4
85C0  fe b0        CP 0xb0
85C2  30 1a        JR nc,0x85de
85C4  dd 34 18     INC (IX+0x18)
85C7  dd 7e 18     LD A,(IX+0x18)
85CA  d6 04        SUB 4
85CC  c0           RET nz
85CD  dd 77 18     LD (IX+0x18),A
85D0  dd 34 19     INC (IX+0x19)
85D3  dd 7e 19     LD A,(IX+0x19)
85D6  fe 05        CP 5
85D8  28 18        JR z,0x85f2
85DA  dd 34 09     INC (IX+9) ; d0
85DD  c9           RET
85DE  dd 34 19     INC (IX+0x19)
85E1  3e 04        LD A,4
85E3  dd 34 18     INC (IX+0x18)
85E6  dd 46 18     LD B,(IX+0x18)
85E9  90           SUB B
85EA  dd 77 18     LD (IX+0x18),A
85ED  20 03        JR nz,0x85f2
85EF  dd 34 19     INC (IX+0x19)
85F2  dd 36 08 10  LD (IX+8),0x10
85F6  dd cb 02 a6  RES 4,(IX+2) ; flags
85FA  c3 07 9a     JP 0x9a07
85FD  dd 35 08     DEC (IX+8)
8600  c0           RET nz
8601  dd 34 01     INC (IX+1) ; estado
8604  dd cb 02 e6  SET 4,(IX+2) ; flags
8608  c3 6f b1     JP 0xb16f
860B  dd 34 18     INC (IX+0x18)
860E  dd 7e 18     LD A,(IX+0x18)
8611  d6 04        SUB 4
8613  c0           RET nz
8614  dd 77 18     LD (IX+0x18),A
8617  dd 35 19     DEC (IX+0x19)
861A  28 04        JR z,0x8620
861C  dd 35 09     DEC (IX+9) ; d0
861F  c9           RET
8620  c3 1c b4     JP 0xb41c
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B16F  dd 5e 0c     LD E,(IX+12) ; vy_lo
B172  dd 56 0d     LD D,(IX+13) ; vy
B175  cd 68 40     CALL 0x4068 ; neg_de
B178  dd 73 0c     LD (IX+12),E ; vy_lo
B17B  dd 72 0d     LD (IX+13),D ; vy
B17E  c9           RET
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
