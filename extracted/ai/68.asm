; tipo 0x68  (hijo, no aparece en salas)  descriptor 81 a0 02 01 00
; ---- inicio (b04:66B4[103]) ----
9F3F  3a 2f e6     LD A,(0xe62f)
9F42  3c           INC A
9F43  32 2f e6     LD (0xe62f),A
9F46  e6 03        AND 3
9F48  21 66 9f     LD HL,0x9f66
9F4B  cd 83 40     CALL 0x4083
9F4E  dd 77 0d     LD (IX+13),A ; vy
9F51  23           INC HL
9F52  7e           LD A,(HL)
9F53  dd 77 0f     LD (IX+15),A ; vx
9F56  af           XOR A
9F57  dd 77 0e     LD (IX+14),A ; vx_lo
9F5A  dd 77 0c     LD (IX+12),A ; vy_lo
9F5D  dd 36 08 08  LD (IX+8),8
9F61  dd cb 02 a6  RES 4,(IX+2) ; flags
9F65  c9           RET
; ---- actualizacion (b06:B472[103]) ----
67AF  4d           LD C,L
67B0  26 00        LD H,0
67B2  44           LD B,H
67B3  29           ADD HL,HL
67B4  29           ADD HL,HL
67B5  09           ADD HL,BC
67B6  01 b6 67     LD BC,0x67b6
67B9  09           ADD HL,BC
67BA  c9           RET
9F6B  dd 7e 01     LD A,(IX+1) ; estado
9F6E  3d           DEC A
9F6F  28 13        JR z,0x9f84
9F71  dd 35 08     DEC (IX+8)
9F74  c0           RET nz
9F75  cd 49 a0     CALL 0xa049
9F78  dd cb 02 e6  SET 4,(IX+2) ; flags
9F7C  dd 34 01     INC (IX+1) ; estado
9F7F  dd 36 08 c0  LD (IX+8),0xc0
9F83  c9           RET
9F84  dd 35 08     DEC (IX+8)
9F87  ca 1c b4     JP z,0xb41c
9F8A  cd d8 9f     CALL 0x9fd8
9F8D  cd 11 a0     CALL 0xa011
9F90  3e ff        LD A,0xff
9F92  06 00        LD B,0
9F94  dd cb 0d 7e  BIT 7,(IX+13) ; vy
9F98  20 03        JR nz,0x9f9d
9F9A  04           INC B
9F9B  3e 11        LD A,0x11
9F9D  dd 86 05     ADD A,(IX+5) ; y
9FA0  6f           LD L,A
9FA1  dd 70 16     LD (IX+0x16),B ; ay_lo
9FA4  3e ff        LD A,0xff
9FA6  06 00        LD B,0
9FA8  dd cb 0f 7e  BIT 7,(IX+15) ; vx
9FAC  20 03        JR nz,0x9fb1
9FAE  04           INC B
9FAF  3e 11        LD A,0x11
9FB1  dd 86 07     ADD A,(IX+7) ; x
9FB4  67           LD H,A
9FB5  dd 70 17     LD (IX+0x17),B ; ay
9FB8  cd 6e 40     CALL 0x406e ; vec_4dc8
9FBB  30 0e        JR nc,0x9fcb
9FBD  dd 7e 15     LD A,(IX+0x15)
9FC0  dd 86 14     ADD A,(IX+0x14)
9FC3  20 06        JR nz,0x9fcb
9FC5  dd 34 15     INC (IX+0x15)
9FC8  dd 34 14     INC (IX+0x14)
9FCB  dd 35 14     DEC (IX+0x14)
9FCE  cc 6f b1     CALL z,0xb16f
9FD1  dd 35 15     DEC (IX+0x15)
9FD4  ca 5f b1     JP z,0xb15f
9FD7  c9           RET
9FD8  dd 36 14 00  LD (IX+0x14),0
9FDC  dd cb 0d 7e  BIT 7,(IX+13) ; vy
9FE0  28 19        JR z,0x9ffb
9FE2  dd 7e 05     LD A,(IX+5) ; y
9FE5  fe 20        CP 0x20
9FE7  38 24        JR c,0xa00d
9FE9  dd 6e 05     LD L,(IX+5) ; y
9FEC  2d           DEC L
9FED  dd 66 07     LD H,(IX+7) ; x
9FF0  cd 6e 40     CALL 0x406e ; vec_4dc8
9FF3  38 18        JR c,0xa00d
9FF5  cd 71 40     CALL 0x4071
9FF8  38 13        JR c,0xa00d
9FFA  c9           RET
9FFB  dd 7e 05     LD A,(IX+5) ; y
9FFE  c6 11        ADD A,0x11
A000  6f           LD L,A
A001  dd 66 07     LD H,(IX+7) ; x
A004  cd 6e 40     CALL 0x406e ; vec_4dc8
A007  38 04        JR c,0xa00d
A009  cd 71 40     CALL 0x4071
A00C  d0           RET nc
A00D  dd 34 14     INC (IX+0x14)
A010  c9           RET
A011  dd 36 15 00  LD (IX+0x15),0
A015  dd cb 0f 7e  BIT 7,(IX+15) ; vx
A019  28 12        JR z,0xa02d
A01B  dd 66 07     LD H,(IX+7) ; x
A01E  25           DEC H
A01F  dd 6e 05     LD L,(IX+5) ; y
A022  cd 6e 40     CALL 0x406e ; vec_4dc8
A025  38 18        JR c,0xa03f
A027  cd 43 a0     CALL 0xa043
A02A  38 13        JR c,0xa03f
A02C  c9           RET
A02D  dd 7e 07     LD A,(IX+7) ; x
A030  c6 11        ADD A,0x11
A032  67           LD H,A
A033  dd 6e 05     LD L,(IX+5) ; y
A036  cd 6e 40     CALL 0x406e ; vec_4dc8
A039  38 04        JR c,0xa03f
A03B  cd 43 a0     CALL 0xa043
A03E  d0           RET nc
A03F  dd 34 15     INC (IX+0x15)
A042  c9           RET
A043  11 20 00     LD DE,0x20
A046  19           ADD HL,DE
A047  be           CP (HL)
A048  c9           RET
A049  dd 7e 05     LD A,(IX+5) ; y
A04C  e6 03        AND 3
A04E  fe 02        CP 2
A050  dd 7e 05     LD A,(IX+5) ; y
A053  38 02        JR c,0xa057
A055  c6 04        ADD A,4
A057  e6 fc        AND 0xfc
A059  dd 77 05     LD (IX+5),A ; y
A05C  dd 7e 07     LD A,(IX+7) ; x
A05F  e6 03        AND 3
A061  fe 02        CP 2
A063  dd 7e 07     LD A,(IX+7) ; x
A066  38 02        JR c,0xa06a
A068  c6 04        ADD A,4
A06A  e6 fc        AND 0xfc
A06C  dd 77 07     LD (IX+7),A ; x
A06F  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
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
