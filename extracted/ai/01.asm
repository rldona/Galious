; tipo 0x01  vida 4  patron observado: camina
; ---- inicio (b04:66B4[0]) ----
6E2B  dd 70 0a     LD (IX+10),B ; d1
6E2E  dd 71 12     LD (IX+0x12),C
6E31  c9           RET
7950  ed 5f        LD A,R
7952  e6 01        AND 1
7954  11 80 01     LD DE,0x180
7957  c4 68 40     CALL nz,0x4068 ; neg_de
795A  cd 3e b1     CALL 0xb13e
795D  cd 3d 7a     CALL 0x7a3d
7960  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7964  01 b0 a0     LD BC,0xa0b0
7967  28 03        JR z,0x796c
7969  01 c4 b4     LD BC,0xb4c4
796C  cd 2b 6e     CALL 0x6e2b
796F  dd 36 13 06  LD (IX+0x13),6
7973  dd 36 11 00  LD (IX+0x11),0 ; atributo_sprite
7977  c9           RET
7A3D  dd 7e 09     LD A,(IX+9) ; d0
7A40  ee 03        XOR 3
7A42  dd 77 09     LD (IX+9),A ; d0
7A45  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[0]) ----
6B71  dd cb 02 a6  RES 4,(IX+2) ; flags
6B75  c9           RET
6B76  dd cb 02 e6  SET 4,(IX+2) ; flags
6B7A  c9           RET
6BD5  dd 36 01 00  LD (IX+1),0 ; estado
6BD9  c9           RET
6BEE  dd 7e 05     LD A,(IX+5) ; y
6BF1  c6 10        ADD A,0x10
6BF3  6f           LD L,A
6BF4  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6BF8  3e 10        LD A,0x10
6BFA  28 01        JR z,0x6bfd
6BFC  af           XOR A
6BFD  dd 86 07     ADD A,(IX+7) ; x
6C00  67           LD H,A
6C01  c3 6e 40     JP 0x406e ; vec_4dc8
6C04  01 e0 ff     LD BC,0xffe0
6C07  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6C0B  20 03        JR nz,0x6c10
6C0D  01 e0 ff     LD BC,0xffe0
6C10  09           ADD HL,BC
6C11  3e b9        LD A,0xb9
6C13  be           CP (HL)
6C14  d8           RET c
6C15  dd 7e 07     LD A,(IX+7) ; x
6C18  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6C1C  28 03        JR z,0x6c21
6C1E  fe 08        CP 8
6C20  c9           RET
6C21  fe f0        CP 0xf0
6C23  3f           CCF
6C24  c9           RET
6D49  dd 7e 07     LD A,(IX+7) ; x
6D4C  80           ADD A,B
6D4D  67           LD H,A
6D4E  dd 7e 05     LD A,(IX+5) ; y
6D51  81           ADD A,C
6D52  6f           LD L,A
6D53  cd 56 40     CALL 0x4056 ; vec_4da1
6D56  3e b9        LD A,0xb9
6D58  be           CP (HL)
6D59  c9           RET
6E2B  dd 70 0a     LD (IX+10),B ; d1
6E2E  dd 71 12     LD (IX+0x12),C
6E31  c9           RET
74A8  3a 07 e5     LD A,(0xe507) ; jugador_x
74AB  dd 96 07     SUB (IX+7) ; x
74AE  06 00        LD B,0
74B0  30 03        JR nc,0x74b5
74B2  04           INC B
74B3  ed 44        NEG
74B5  dd 70 1d     LD (IX+0x1d),B
74B8  fe 41        CP 0x41
74BA  c9           RET
7960  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7964  01 b0 a0     LD BC,0xa0b0
7967  28 03        JR z,0x796c
7969  01 c4 b4     LD BC,0xb4c4
796C  cd 2b 6e     CALL 0x6e2b
796F  dd 36 13 06  LD (IX+0x13),6
7973  dd 36 11 00  LD (IX+0x11),0 ; atributo_sprite
7977  c9           RET
7978  dd 7e 01     LD A,(IX+1) ; estado
797B  3d           DEC A
797C  28 41        JR z,0x79bf
797E  f2 e4 79     JP p,0x79e4
7981  cd 5f 7a     CALL 0x7a5f
7984  28 1e        JR z,0x79a4
7986  cd ee 6b     CALL 0x6bee
7989  d2 b5 79     JP nc,0x79b5
798C  cd 04 6c     CALL 0x6c04
798F  38 24        JR c,0x79b5
7991  dd 36 1e 00  LD (IX+0x1e),0
7995  16 04        LD D,4
7997  06 13        LD B,0x13
7999  dd cb 0f 7e  BIT 7,(IX+15) ; vx
799D  28 02        JR z,0x79a1
799F  16 0c        LD D,12
79A1  c3 7f b1     JP 0xb17f
79A4  dd 36 15 1e  LD (IX+0x15),0x1e
79A8  cd 73 79     CALL 0x7973
79AB  cd 71 6b     CALL 0x6b71
79AE  dd 36 0a a8  LD (IX+10),0xa8 ; d1
79B2  c3 07 9a     JP 0x9a07
79B5  dd 36 1e 01  LD (IX+0x1e),1
79B9  cd 5f b1     CALL 0xb15f
79BC  c3 60 79     JP 0x7960
79BF  dd 35 15     DEC (IX+0x15)
79C2  28 07        JR z,0x79cb
79C4  16 14        LD D,0x14
79C6  06 00        LD B,0
79C8  c3 7f b1     JP 0xb17f
79CB  cd 56 7a     CALL 0x7a56
79CE  dd cb 1d 46  BIT 0,(IX+0x1d)
79D2  11 80 01     LD DE,0x180
79D5  c4 68 40     CALL nz,0x4068 ; neg_de
79D8  cd 3e b1     CALL 0xb13e
79DB  cd 60 79     CALL 0x7960
79DE  cd 76 6b     CALL 0x6b76
79E1  c3 07 9a     JP 0x9a07
79E4  3d           DEC A
79E5  20 5f        JR nz,0x7a46
79E7  cd 86 79     CALL 0x7986
79EA  dd cb 1e 46  BIT 0,(IX+0x1e)
79EE  20 13        JR nz,0x7a03
79F0  dd 7e 07     LD A,(IX+7) ; x
79F3  dd cb 0f 7e  BIT 7,(IX+15) ; vx
79F7  20 06        JR nz,0x79ff
79F9  dd be 1f     CP (IX+0x1f)
79FC  d8           RET c
79FD  18 04        JR 0x7a03
79FF  dd be 1f     CP (IX+0x1f)
7A02  d0           RET nc
7A03  dd 7e 1c     LD A,(IX+0x1c)
7A06  3d           DEC A
7A07  20 44        JR nz,0x7a4d
7A09  0e f0        LD C,0xf0
7A0B  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7A0F  06 0c        LD B,12
7A11  28 02        JR z,0x7a15
7A13  06 04        LD B,4
7A15  cd 49 6d     CALL 0x6d49
7A18  30 33        JR nc,0x7a4d
7A1A  dd 36 08 0f  LD (IX+8),15
7A1E  3e ac        LD A,0xac
7A20  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7A24  28 02        JR z,0x7a28
7A26  3e c0        LD A,0xc0
7A28  dd 77 0a     LD (IX+10),A ; d1
7A2B  dd 36 11 01  LD (IX+0x11),1 ; atributo_sprite
7A2F  cd 71 6b     CALL 0x6b71
7A32  cd 76 7a     CALL 0x7a76
7A35  3e 21        LD A,0x21
7A37  cd 59 40     CALL 0x4059 ; sound_request
7A3A  dd 34 01     INC (IX+1) ; estado
7A3D  dd 7e 09     LD A,(IX+9) ; d0
7A40  ee 03        XOR 3
7A42  dd 77 09     LD (IX+9),A ; d0
7A45  c9           RET
7A46  dd 35 08     DEC (IX+8)
7A49  c0           RET nz
7A4A  cd 3d 7a     CALL 0x7a3d
7A4D  cd 76 6b     CALL 0x6b76
7A50  cd d5 6b     CALL 0x6bd5
7A53  c3 60 79     JP 0x7960
7A56  3a 07 e5     LD A,(0xe507) ; jugador_x
7A59  dd 77 1f     LD (IX+0x1f),A
7A5C  c3 a8 74     JP 0x74a8
7A5F  21 05 e5     LD HL,0xe505 ; jugador_y
7A62  06 01        LD B,1
7A64  dd 7e 05     LD A,(IX+5) ; y
7A67  d6 10        SUB 0x10
7A69  be           CP (HL)
7A6A  28 05        JR z,0x7a71
7A6C  c6 20        ADD A,0x20
7A6E  be           CP (HL)
7A6F  c0           RET nz
7A70  04           INC B
7A71  dd 70 1c     LD (IX+0x1c),B
7A74  af           XOR A
7A75  c9           RET
7A76  3a 05 e5     LD A,(0xe505) ; jugador_y
7A79  47           LD B,A
7A7A  dd 7e 05     LD A,(IX+5) ; y
7A7D  90           SUB B
7A7E  fe 10        CP 0x10
7A80  c0           RET nz
7A81  3a 07 e5     LD A,(0xe507) ; jugador_x
7A84  dd 96 07     SUB (IX+7) ; x
7A87  fe 10        CP 0x10
7A89  d0           RET nc
7A8A  3a 03 e5     LD A,(0xe503)
7A8D  32 14 e5     LD (0xe514),A
7A90  c3 92 40     JP 0x4092
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B0DD  dd 7e 0a     LD A,(IX+10) ; d1
B0E0  aa           XOR D
B0E1  dd 77 0a     LD (IX+10),A ; d1
B0E4  dd 7e 11     LD A,(IX+0x11) ; atributo_sprite
B0E7  a8           XOR B
B0E8  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B0EB  af           XOR A
B0EC  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
B17F  0e 07        LD C,7
B181  18 02        JR 0xb185
B185  3a 04 e6     LD A,(0xe604)
B188  a1           AND C
B189  c0           RET nz
B18A  c3 dd b0     JP 0xb0dd
