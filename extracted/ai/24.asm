; tipo 0x24  vida 3  patron observado: camina
; ---- inicio (b04:66B4[35]) ----
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
9AF9  ed 5f        LD A,R
9AFB  e6 01        AND 1
9AFD  dd 77 1d     LD (IX+0x1d),A
9B00  c9           RET
9B30  cd ab 9b     CALL 0x9bab
9B33  dd 7e 05     LD A,(IX+5) ; y
9B36  e6 e0        AND 0xe0
9B38  1f           RRA
9B39  1f           RRA
9B3A  1f           RRA
9B3B  1f           RRA
9B3C  1f           RRA
9B3D  3d           DEC A
9B3E  dd 77 19     LD (IX+0x19),A
9B41  cd f9 9a     CALL 0x9af9
9B44  cd 37 9d     CALL 0x9d37
9B47  dd cb 1d 46  BIT 0,(IX+0x1d)
9B4B  3e a0        LD A,0xa0
9B4D  28 02        JR z,0x9b51
9B4F  3e ac        LD A,0xac
9B51  c3 83 70     JP 0x7083
9BAB  cd f1 81     CALL 0x81f1
9BAE  c3 cf 71     JP 0x71cf
9D37  dd cb 1d 46  BIT 0,(IX+0x1d)
9D3B  11 00 02     LD DE,0x200
9D3E  c4 68 40     CALL nz,0x4068 ; neg_de
9D41  cd 3e b1     CALL 0xb13e
9D44  c3 47 9b     JP 0x9b47
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[35]) ----
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
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
96EA  dd 77 05     LD (IX+5),A ; y
96ED  c9           RET
99C9  dd 7e 05     LD A,(IX+5) ; y
99CC  c6 10        ADD A,0x10
99CE  6f           LD L,A
99CF  dd cb 1d 46  BIT 0,(IX+0x1d)
99D3  1e 04        LD E,4
99D5  28 02        JR z,0x99d9
99D7  1e 0c        LD E,12
99D9  dd 7e 07     LD A,(IX+7) ; x
99DC  83           ADD A,E
99DD  67           LD H,A
99DE  c3 6e 40     JP 0x406e ; vec_4dc8
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
9B0C  dd 7e 1d     LD A,(IX+0x1d)
9B0F  ee 01        XOR 1
9B11  dd 77 1d     LD (IX+0x1d),A
9B14  c9           RET
9B15  dd 7e 05     LD A,(IX+5) ; y
9B18  e6 f8        AND 0xf8
9B1A  c3 ea 96     JP 0x96ea
9B1D  cd 4c b1     CALL 0xb14c
9B20  dd 7e 0d     LD A,(IX+13) ; vy
9B23  cb 7f        BIT 7,A
9B25  c0           RET nz
9B26  fe 06        CP 6
9B28  38 04        JR c,0x9b2e
9B2A  dd 36 0d 06  LD (IX+13),6 ; vy
9B2E  af           XOR A
9B2F  c9           RET
9B47  dd cb 1d 46  BIT 0,(IX+0x1d)
9B4B  3e a0        LD A,0xa0
9B4D  28 02        JR z,0x9b51
9B4F  3e ac        LD A,0xac
9B51  c3 83 70     JP 0x7083
9B54  dd 7e 01     LD A,(IX+1) ; estado
9B57  3d           DEC A
9B58  28 35        JR z,0x9b8f
9B5A  f2 3e 9c     JP p,0x9c3e
9B5D  dd 35 08     DEC (IX+8)
9B60  28 27        JR z,0x9b89
9B62  cd 47 9d     CALL 0x9d47
9B65  cd ee 6b     CALL 0x6bee
9B68  d2 80 9b     JP nc,0x9b80
9B6B  cd 04 6c     CALL 0x6c04
9B6E  da 80 9b     JP c,0x9b80
9B71  dd cb 1d 46  BIT 0,(IX+0x1d)
9B75  16 04        LD D,4
9B77  28 02        JR z,0x9b7b
9B79  16 1c        LD D,0x1c
9B7B  06 00        LD B,0
9B7D  c3 7f b1     JP 0xb17f
9B80  cd 0c 9b     CALL 0x9b0c
9B83  cd 5f b1     CALL 0xb15f
9B86  c3 47 9b     JP 0x9b47
9B89  cd ab 9b     CALL 0x9bab
9B8C  c3 07 9a     JP 0x9a07
9B8F  dd 35 08     DEC (IX+8)
9B92  28 14        JR z,0x9ba8
9B94  dd 7e 08     LD A,(IX+8)
9B97  e6 20        AND 0x20
9B99  20 ca        JR nz,0x9b65
9B9B  cd 6c 9c     CALL 0x9c6c
9B9E  cd 9b 9c     CALL 0x9c9b
9BA1  38 0e        JR c,0x9bb1
9BA3  cd 37 9d     CALL 0x9d37
9BA6  18 bd        JR 0x9b65
9BA8  dd 35 01     DEC (IX+1) ; estado
9BAB  cd f1 81     CALL 0x81f1
9BAE  c3 cf 71     JP 0x71cf
9BB1  dd 7e 1e     LD A,(IX+0x1e)
9BB4  3d           DEC A
9BB5  21 1c 9c     LD HL,0x9c1c
9BB8  e5           PUSH HL
9BB9  cd 50 40     CALL 0x4050 ; jump_table_a
9BBC  c4 9b        .word 0x9BC4 ; caso 0
9BBE  cc 9b        .word 0x9BCC ; caso 1
9BC0  e6 9b        .word 0x9BE6 ; caso 2
9BC2  fd 9b        .word 0x9BFD ; caso 3
9BC4  dd 35 19     DEC (IX+0x19)
9BC7  11 00 fa     LD DE,0xfa00
9BCA  18 0e        JR 0x9bda
9BCC  dd 34 19     INC (IX+0x19)
9BCF  dd 7e 05     LD A,(IX+5) ; y
9BD2  c6 02        ADD A,2
9BD4  dd 77 05     LD (IX+5),A ; y
9BD7  11 00 fd     LD DE,0xfd00
9BDA  cd 37 b1     CALL 0xb137
9BDD  11 80 00     LD DE,0x80
9BE0  cd 45 b1     CALL 0xb145
9BE3  c3 37 9d     JP 0x9d37
9BE6  dd 35 19     DEC (IX+0x19)
9BE9  dd 35 19     DEC (IX+0x19)
9BEC  11 b0 f9     LD DE,0xf9b0
9BEF  cd 37 b1     CALL 0xb137
9BF2  11 50 00     LD DE,0x50
9BF5  cd 45 b1     CALL 0xb145
9BF8  11 80 01     LD DE,0x180
9BFB  18 15        JR 0x9c12
9BFD  dd 34 19     INC (IX+0x19)
9C00  dd 34 19     INC (IX+0x19)
9C03  11 40 fd     LD DE,0xfd40
9C06  cd 37 b1     CALL 0xb137
9C09  11 40 00     LD DE,0x40
9C0C  cd 45 b1     CALL 0xb145
9C0F  11 00 01     LD DE,0x100
9C12  dd cb 1d 46  BIT 0,(IX+0x1d)
9C16  c4 68 40     CALL nz,0x4068 ; neg_de
9C19  c3 3e b1     JP 0xb13e
9C3E  cd 1d 9b     CALL 0x9b1d
9C41  c0           RET nz
9C42  dd 7e 05     LD A,(IX+5) ; y
9C45  dd be 1a     CP (IX+0x1a)
9C48  d8           RET c
9C49  cd c9 99     CALL 0x99c9
9C4C  d0           RET nc
9C4D  cd 15 9b     CALL 0x9b15
9C50  cd 29 b1     CALL 0xb129
9C53  af           XOR A
9C54  dd 77 01     LD (IX+1),A ; estado
9C57  dd 77 11     LD (IX+0x11),A ; atributo_sprite
9C5A  cd ab 9b     CALL 0x9bab
9C5D  cd 37 9d     CALL 0x9d37
9C60  c3 47 9b     JP 0x9b47
9C6C  dd 7e 08     LD A,(IX+8)
9C6F  e6 07        AND 7
9C71  c0           RET nz
9C72  3a 07 e5     LD A,(0xe507) ; jugador_x
9C75  dd be 07     CP (IX+7) ; x
9C78  3e 00        LD A,0
9C7A  17           RLA
9C7B  dd 77 1d     LD (IX+0x1d),A
9C7E  dd 7e 05     LD A,(IX+5) ; y
9C81  c6 10        ADD A,0x10
9C83  21 05 e5     LD HL,0xe505 ; jugador_y
9C86  96           SUB (HL)
9C87  28 0e        JR z,0x9c97
9C89  06 03        LD B,3
9C8B  30 03        JR nc,0x9c90
9C8D  ed 44        NEG
9C8F  04           INC B
9C90  fe 21        CP 0x21
9C92  30 02        JR nc,0x9c96
9C94  05           DEC B
9C95  05           DEC B
9C96  78           LD A,B
9C97  dd 77 1e     LD (IX+0x1e),A
9C9A  c9           RET
9C9B  3a 00 e5     LD A,(0xe500) ; player_action
9C9E  a7           AND A
9C9F  c0           RET nz
9CA0  11 d8 38     LD DE,0x38d8
9CA3  dd 7e 1e     LD A,(IX+0x1e)
9CA6  a7           AND A
9CA7  c8           RET z
9CA8  4f           LD C,A
9CA9  0d           DEC C
9CAA  06 00        LD B,0
9CAC  21 68 9c     LD HL,0x9c68
9CAF  09           ADD HL,BC
9CB0  dd 7e 05     LD A,(IX+5) ; y
9CB3  cb 7e        BIT 7,(HL)
9CB5  20 05        JR nz,0x9cbc
9CB7  86           ADD A,(HL)
9CB8  3f           CCF
9CB9  d0           RET nc
9CBA  18 02        JR 0x9cbe
9CBC  86           ADD A,(HL)
9CBD  d0           RET nc
9CBE  6f           LD L,A
9CBF  d6 28        SUB 0x28
9CC1  fe 91        CP 0x91
9CC3  d0           RET nc
9CC4  dd 7e 07     LD A,(IX+7) ; x
9CC7  dd cb 1d 46  BIT 0,(IX+0x1d)
9CCB  28 04        JR z,0x9cd1
9CCD  83           ADD A,E
9CCE  d0           RET nc
9CCF  18 03        JR 0x9cd4
9CD1  82           ADD A,D
9CD2  3f           CCF
9CD3  d0           RET nc
9CD4  67           LD H,A
9CD5  d6 10        SUB 0x10
9CD7  fe e0        CP 0xe0
9CD9  d0           RET nc
9CDA  cd 6e 40     CALL 0x406e ; vec_4dc8
9CDD  d0           RET nc
9CDE  cd 2d 9d     CALL 0x9d2d
9CE1  d0           RET nc
9CE2  cd 1c 9d     CALL 0x9d1c
9CE5  d0           RET nc
9CE6  cd 1c 9d     CALL 0x9d1c
9CE9  d0           RET nc
9CEA  dd 7e 00     LD A,(IX) ; tipo
9CED  fe 24        CP 0x24
9CEF  28 44        JR z,0x9d35
9CF1  dd 7e 1e     LD A,(IX+0x1e)
9CF4  3d           DEC A
9CF5  28 3e        JR z,0x9d35
9CF7  dd 7e 05     LD A,(IX+5) ; y
9CFA  c6 10        ADD A,0x10
9CFC  6f           LD L,A
9CFD  dd 7e 07     LD A,(IX+7) ; x
9D00  0e 10        LD C,0x10
9D02  dd cb 1d 46  BIT 0,(IX+0x1d)
9D06  28 02        JR z,0x9d0a
9D08  0e f0        LD C,0xf0
9D0A  81           ADD A,C
9D0B  67           LD H,A
9D0C  cd 56 40     CALL 0x4056 ; vec_4da1
9D0F  e5           PUSH HL
9D10  cd 16 9d     CALL 0x9d16
9D13  e1           POP HL
9D14  d0           RET nc
9D15  23           INC HL
9D16  7e           LD A,(HL)
9D17  fe ba        CP 0xba
9D19  d0           RET nc
9D1A  18 11        JR 0x9d2d
9D1C  11 21 00     LD DE,0x21
9D1F  dd cb 1d 46  BIT 0,(IX+0x1d)
9D23  20 03        JR nz,0x9d28
9D25  11 1f 00     LD DE,0x1f
9D28  19           ADD HL,DE
9D29  3e b9        LD A,0xb9
9D2B  be           CP (HL)
9D2C  d0           RET nc
9D2D  11 e0 ff     LD DE,0xffe0
9D30  19           ADD HL,DE
9D31  7e           LD A,(HL)
9D32  fe ba        CP 0xba
9D34  c9           RET
9D35  37           SCF
9D36  c9           RET
9D37  dd cb 1d 46  BIT 0,(IX+0x1d)
9D3B  11 00 02     LD DE,0x200
9D3E  c4 68 40     CALL nz,0x4068 ; neg_de
9D41  cd 3e b1     CALL 0xb13e
9D44  c3 47 9b     JP 0x9b47
9D47  dd 7e 08     LD A,(IX+8)
9D4A  e6 1f        AND 0x1f
9D4C  c0           RET nz
9D4D  cd f1 81     CALL 0x81f1
9D50  e6 07        AND 7
9D52  fe 05        CP 5
9D54  38 02        JR c,0x9d58
9D56  d6 05        SUB 5
9D58  4f           LD C,A
9D59  a7           AND A
9D5A  28 2e        JR z,0x9d8a
9D5C  dd 7e 19     LD A,(IX+0x19)
9D5F  cd 50 40     CALL 0x4050 ; jump_table_a
9D62  85 9d        .word 0x9D85 ; caso 0
9D64  7c 9d        .word 0x9D7C ; caso 1
9D66  8a 9d        .word 0x9D8A ; caso 2
9D68  73 9d        .word 0x9D73 ; caso 3
9D6A  6c 9d        .word 0x9D6C ; caso 4
9D6C  cb 41        BIT 0,C
9D6E  20 1a        JR nz,0x9d8a
9D70  0d           DEC C
9D71  18 17        JR 0x9d8a
9D73  79           LD A,C
9D74  fe 04        CP 4
9D76  20 12        JR nz,0x9d8a
9D78  0e 00        LD C,0
9D7A  18 0e        JR 0x9d8a
9D7C  79           LD A,C
9D7D  fe 03        CP 3
9D7F  20 09        JR nz,0x9d8a
9D81  0e 00        LD C,0
9D83  18 05        JR 0x9d8a
9D85  cb 41        BIT 0,C
9D87  28 01        JR z,0x9d8a
9D89  0c           INC C
9D8A  dd 71 1e     LD (IX+0x1e),C
9D8D  cd 9b 9c     CALL 0x9c9b
9D90  d2 37 9d     JP nc,0x9d37
9D93  e1           POP HL
9D94  c3 b1 9b     JP 0x9bb1
B0DD  dd 7e 0a     LD A,(IX+10) ; d1
B0E0  aa           XOR D
B0E1  dd 77 0a     LD (IX+10),A ; d1
B0E4  dd 7e 11     LD A,(IX+0x11) ; atributo_sprite
B0E7  a8           XOR B
B0E8  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B0EB  af           XOR A
B0EC  c9           RET
B129  16 00        LD D,0
B12B  5a           LD E,D
B12C  18 09        JR 0xb137
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B145  dd 73 16     LD (IX+0x16),E ; ay_lo
B148  dd 72 17     LD (IX+0x17),D ; ay
B14B  c9           RET
B14C  dd 7e 0c     LD A,(IX+12) ; vy_lo
B14F  dd 86 16     ADD A,(IX+0x16) ; ay_lo
B152  dd 77 0c     LD (IX+12),A ; vy_lo
B155  dd 7e 0d     LD A,(IX+13) ; vy
B158  dd 8e 17     ADC A,(IX+0x17) ; ay
B15B  dd 77 0d     LD (IX+13),A ; vy
B15E  c9           RET
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
