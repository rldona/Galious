; tipo 0x25  vida 8  patron observado: camina (cae entre plataformas)
; ---- inicio (b04:66B4[36]) ----
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
7CA9  3a 07 e5     LD A,(0xe507) ; jugador_x
7CAC  dd be 07     CP (IX+7) ; x
7CAF  3f           CCF
7CB0  3e 00        LD A,0
7CB2  17           RLA
7CB3  dd 77 1d     LD (IX+0x1d),A
7CB6  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
98C0  dd cb 02 ce  SET 1,(IX+2) ; flags
98C4  cd f2 9a     CALL 0x9af2
98C7  cd a9 7c     CALL 0x7ca9
98CA  11 50 00     LD DE,0x50
98CD  cd 45 b1     CALL 0xb145
98D0  cd e7 9a     CALL 0x9ae7
98D3  dd cb 1d 46  BIT 0,(IX+0x1d)
98D7  11 20 01     LD DE,0x120
98DA  c4 68 40     CALL nz,0x4068 ; neg_de
98DD  cd 3e b1     CALL 0xb13e
98E0  dd cb 1d 46  BIT 0,(IX+0x1d)
98E4  3e ac        LD A,0xac
98E6  01 aa b4     LD BC,0xb4aa
98E9  16 4e        LD D,0x4e
98EB  28 0a        JR z,0x98f7
98ED  3e b8        LD A,0xb8
98EF  01 b2 c0     LD BC,0xc0b2
98F2  14           INC D
98F3  dd cb 02 c6  SET 0,(IX+2) ; flags
98F7  1e 0f        LD E,15
98F9  dd 70 12     LD (IX+0x12),B
98FC  dd 71 09     LD (IX+9),C ; d0
98FF  dd 72 11     LD (IX+0x11),D ; atributo_sprite
9902  dd 73 13     LD (IX+0x13),E
9905  c3 83 70     JP 0x7083
9AE7  cd f1 81     CALL 0x81f1
9AEA  e6 7f        AND 0x7f
9AEC  c6 20        ADD A,0x20
9AEE  dd 77 1a     LD (IX+0x1a),A
9AF1  c9           RET
9AF2  ed 5f        LD A,R
9AF4  e6 7f        AND 0x7f
9AF6  c3 cf 71     JP 0x71cf
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B145  dd 73 16     LD (IX+0x16),E ; ay_lo
B148  dd 72 17     LD (IX+0x17),D ; ay
B14B  c9           RET
; ---- actualizacion (b06:B472[36]) ----
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
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
736F  dd 73 0a     LD (IX+10),E ; d1
7372  dd 72 11     LD (IX+0x11),D ; atributo_sprite
7375  c9           RET
7A3D  dd 7e 09     LD A,(IX+9) ; d0
7A40  ee 03        XOR 3
7A42  dd 77 09     LD (IX+9),A ; d0
7A45  c9           RET
7BE9  dd 36 01 01  LD (IX+1),1 ; estado
7BED  dd 34 1e     INC (IX+0x1e)
7BF0  dd 7e 1e     LD A,(IX+0x1e)
7BF3  d6 03        SUB 3
7BF5  c0           RET nz
7BF6  dd 77 1e     LD (IX+0x1e),A
7BF9  c9           RET
7C5E  dd 36 14 00  LD (IX+0x14),0
7C62  01 e0 ff     LD BC,0xffe0
7C65  09           ADD HL,BC
7C66  7e           LD A,(HL)
7C67  fe d2        CP 0xd2
7C69  28 14        JR z,0x7c7f
7C6B  fe ba        CP 0xba
7C6D  3f           CCF
7C6E  d8           RET c
7C6F  dd 7e 07     LD A,(IX+7) ; x
7C72  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7C76  28 03        JR z,0x7c7b
7C78  fe 10        CP 0x10
7C7A  c9           RET
7C7B  fe d8        CP 0xd8
7C7D  3f           CCF
7C7E  c9           RET
7C7F  dd 36 14 01  LD (IX+0x14),1
7C83  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
947D  ed 5f        LD A,R
947F  e6 03        AND 3
9481  fe 03        CP 3
9483  20 01        JR nz,0x9486
9485  af           XOR A
9486  dd 77 1e     LD (IX+0x1e),A
9489  dd 36 18 06  LD (IX+0x18),6
948D  c3 07 9a     JP 0x9a07
94FF  dd 7e 18     LD A,(IX+0x18)
9502  fe ff        CP 0xff
9504  c8           RET z
9505  fe 05        CP 5
9507  3f           CCF
9508  d8           RET c
9509  1f           RRA
950A  d8           RET c
950B  cd e9 7b     CALL 0x7be9
950E  37           SCF
950F  c9           RET
96EA  dd 77 05     LD (IX+5),A ; y
96ED  c9           RET
98D3  dd cb 1d 46  BIT 0,(IX+0x1d)
98D7  11 20 01     LD DE,0x120
98DA  c4 68 40     CALL nz,0x4068 ; neg_de
98DD  cd 3e b1     CALL 0xb13e
98E0  dd cb 1d 46  BIT 0,(IX+0x1d)
98E4  3e ac        LD A,0xac
98E6  01 aa b4     LD BC,0xb4aa
98E9  16 4e        LD D,0x4e
98EB  28 0a        JR z,0x98f7
98ED  3e b8        LD A,0xb8
98EF  01 b2 c0     LD BC,0xc0b2
98F2  14           INC D
98F3  dd cb 02 c6  SET 0,(IX+2) ; flags
98F7  1e 0f        LD E,15
98F9  dd 70 12     LD (IX+0x12),B
98FC  dd 71 09     LD (IX+9),C ; d0
98FF  dd 72 11     LD (IX+0x11),D ; atributo_sprite
9902  dd 73 13     LD (IX+0x13),E
9905  c3 83 70     JP 0x7083
9908  dd 7e 01     LD A,(IX+1) ; estado
990B  fe 06        CP 6
990D  28 04        JR z,0x9913
990F  fe 02        CP 2
9911  30 06        JR nc,0x9919
9913  dd 7e 1c     LD A,(IX+0x1c)
9916  a7           AND A
9917  20 47        JR nz,0x9960
9919  dd 7e 01     LD A,(IX+1) ; estado
991C  cd 50 40     CALL 0x4050 ; jump_table_a
991F  2d 99        .word 0x992D ; caso 0
9921  7a 99        .word 0x997A ; caso 1
9923  a8 99        .word 0x99A8 ; caso 2
9925  e1 99        .word 0x99E1 ; caso 3
9927  0b 9a        .word 0x9A0B ; caso 4
9929  18 9a        .word 0x9A18 ; caso 5
992B  39 9a        .word 0x9A39 ; caso 6
992D  cd 3c 99     CALL 0x993c
9930  dd 35 08     DEC (IX+8)
9933  ca 7d 94     JP z,0x947d
9936  dd 35 1a     DEC (IX+0x1a)
9939  28 1a        JR z,0x9955
993B  c9           RET
993C  cd 46 9a     CALL 0x9a46
993F  cd ee 6b     CALL 0x6bee
9942  30 04        JR nc,0x9948
9944  cd 5e 7c     CALL 0x7c5e
9947  d0           RET nc
9948  cd 74 9a     CALL 0x9a74
994B  38 3b        JR c,0x9988
994D  cd 5f b1     CALL 0xb15f
9950  cd 01 9b     CALL 0x9b01
9953  18 8b        JR 0x98e0
9955  cd 71 6b     CALL 0x6b71
9958  cd e7 9a     CALL 0x9ae7
995B  dd 36 01 06  LD (IX+1),6 ; estado
995F  c9           RET
9960  dd 36 19 03  LD (IX+0x19),3
9964  dd 36 1b 3c  LD (IX+0x1b),0x3c
9968  dd 36 01 03  LD (IX+1),3 ; estado
996C  dd 7e 02     LD A,(IX+2) ; flags
996F  e6 af        AND 0xaf
9971  dd 77 02     LD (IX+2),A ; flags
9974  cd 3d 7a     CALL 0x7a3d
9977  c3 e7 99     JP 0x99e7
997A  cd 67 9a     CALL 0x9a67
997D  38 09        JR c,0x9988
997F  cd 3c 99     CALL 0x993c
9982  cd ff 94     CALL 0x94ff
9985  30 2f        JR nc,0x99b6
9987  c9           RET
9988  dd 36 01 02  LD (IX+1),2 ; estado
998C  dd cb 1e 46  BIT 0,(IX+0x1e)
9990  11 b0 fb     LD DE,0xfbb0
9993  20 03        JR nz,0x9998
9995  11 30 fe     LD DE,0xfe30
9998  cd 37 b1     CALL 0xb137
999B  11 00 02     LD DE,0x200
999E  dd cb 1d 46  BIT 0,(IX+0x1d)
99A2  c4 68 40     CALL nz,0x4068 ; neg_de
99A5  c3 3e b1     JP 0xb13e
99A8  cd 1d 9b     CALL 0x9b1d
99AB  c0           RET nz
99AC  cd c9 99     CALL 0x99c9
99AF  d0           RET nc
99B0  cd 15 9b     CALL 0x9b15
99B3  cd 3c 99     CALL 0x993c
99B6  cd 76 6b     CALL 0x6b76
99B9  dd cb 02 f6  SET 6,(IX+2) ; flags
99BD  cd f2 9a     CALL 0x9af2
99C0  cd d5 6b     CALL 0x6bd5
99C3  cd 29 b1     CALL 0xb129
99C6  c3 d3 98     JP 0x98d3
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
99E1  dd 7e 1c     LD A,(IX+0x1c)
99E4  a7           AND A
99E5  28 24        JR z,0x9a0b
99E7  3e 1f        LD A,0x1f
99E9  cd 59 40     CALL 0x4059 ; sound_request
99EC  dd 36 1c 00  LD (IX+0x1c),0
99F0  dd 35 19     DEC (IX+0x19)
99F3  f5           PUSH AF
99F4  dd 7e 19     LD A,(IX+0x19)
99F7  21 61 9a     LD HL,0x9a61
99FA  cd 53 40     CALL 0x4053 ; read_word_table
99FD  cd 6f 73     CALL 0x736f
9A00  f1           POP AF
9A01  dd 36 1b 3c  LD (IX+0x1b),0x3c
9A05  20 04        JR nz,0x9a0b
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
9A0B  dd 35 1b     DEC (IX+0x1b)
9A0E  c0           RET nz
9A0F  dd 36 01 05  LD (IX+1),5 ; estado
9A13  dd 36 1b 18  LD (IX+0x1b),0x18
9A17  c9           RET
9A18  dd 36 1c 00  LD (IX+0x1c),0
9A1C  dd 35 1b     DEC (IX+0x1b)
9A1F  dd 7e 1b     LD A,(IX+0x1b)
9A22  e6 07        AND 7
9A24  c0           RET nz
9A25  dd 34 19     INC (IX+0x19)
9A28  dd 7e 19     LD A,(IX+0x19)
9A2B  fe 03        CP 3
9A2D  28 03        JR z,0x9a32
9A2F  f5           PUSH AF
9A30  18 c2        JR 0x99f4
9A32  dd 36 10 08  LD (IX+0x10),8 ; vida
9A36  c3 b3 99     JP 0x99b3
9A39  dd 35 1a     DEC (IX+0x1a)
9A3C  c0           RET nz
9A3D  cd e7 9a     CALL 0x9ae7
9A40  cd 76 6b     CALL 0x6b76
9A43  c3 d5 6b     JP 0x6bd5
9A46  dd cb 0f 7e  BIT 7,(IX+15) ; vx
9A4A  3e ac        LD A,0xac
9A4C  28 02        JR z,0x9a50
9A4E  3e b8        LD A,0xb8
9A50  21 04 e6     LD HL,0xe604
9A53  cb 56        BIT 2,(HL)
9A55  ca e0 98     JP z,0x98e0
9A58  c6 04        ADD A,4
9A5A  dd cb 09 ae  RES 5,(IX+9) ; d0
9A5E  c3 83 70     JP 0x7083
9A67  21 df 9a     LD HL,0x9adf
9A6A  cd 9b 9a     CALL 0x9a9b
9A6D  d0           RET nc
9A6E  11 d0 40     LD DE,0x40d0
9A71  c3 a3 9c     JP 0x9ca3
9A74  11 e0 30     LD DE,0x30e0
9A77  dd 7e 1e     LD A,(IX+0x1e)
9A7A  a7           AND A
9A7B  c0           RET nz
9A7C  dd 6e 05     LD L,(IX+5) ; y
9A7F  dd 7e 07     LD A,(IX+7) ; x
9A82  0e 10        LD C,0x10
9A84  dd cb 0f 7e  BIT 7,(IX+15) ; vx
9A88  28 02        JR z,0x9a8c
9A8A  0e ff        LD C,0xff
9A8C  81           ADD A,C
9A8D  67           LD H,A
9A8E  cd 56 40     CALL 0x4056 ; vec_4da1
9A91  7e           LD A,(HL)
9A92  fe ba        CP 0xba
9A94  d0           RET nc
9A95  21 e6 9a     LD HL,0x9ae6
9A98  c3 b0 9c     JP 0x9cb0
9A9B  dd 7e 1e     LD A,(IX+0x1e)
9A9E  a7           AND A
9A9F  c8           RET z
9AA0  3d           DEC A
9AA1  87           ADD A,A
9AA2  dd 86 1d     ADD A,(IX+0x1d)
9AA5  cd 53 40     CALL 0x4053 ; read_word_table
9AA8  dd 7e 05     LD A,(IX+5) ; y
9AAB  82           ADD A,D
9AAC  6f           LD L,A
9AAD  d6 30        SUB 0x30
9AAF  fe 81        CP 0x81
9AB1  d0           RET nc
9AB2  dd 7e 07     LD A,(IX+7) ; x
9AB5  dd cb 1d 46  BIT 0,(IX+0x1d)
9AB9  28 04        JR z,0x9abf
9ABB  83           ADD A,E
9ABC  d0           RET nc
9ABD  18 03        JR 0x9ac2
9ABF  83           ADD A,E
9AC0  3f           CCF
9AC1  d0           RET nc
9AC2  67           LD H,A
9AC3  d6 08        SUB 8
9AC5  fe e8        CP 0xe8
9AC7  d0           RET nc
9AC8  cd 6e 40     CALL 0x406e ; vec_4dc8
9ACB  3f           CCF
9ACC  d0           RET nc
9ACD  cd d1 9a     CALL 0x9ad1
9AD0  d0           RET nc
9AD1  dd cb 1d 46  BIT 0,(IX+0x1d)
9AD5  28 03        JR z,0x9ada
9AD7  23           INC HL
9AD8  18 01        JR 0x9adb
9ADA  2b           DEC HL
9ADB  7e           LD A,(HL)
9ADC  fe ba        CP 0xba
9ADE  c9           RET
9AE7  cd f1 81     CALL 0x81f1
9AEA  e6 7f        AND 0x7f
9AEC  c6 20        ADD A,0x20
9AEE  dd 77 1a     LD (IX+0x1a),A
9AF1  c9           RET
9AF2  ed 5f        LD A,R
9AF4  e6 7f        AND 0x7f
9AF6  c3 cf 71     JP 0x71cf
9B01  dd 7e 02     LD A,(IX+2) ; flags
9B04  ee 01        XOR 1
9B06  dd 77 02     LD (IX+2),A ; flags
9B09  dd 35 18     DEC (IX+0x18)
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
B129  16 00        LD D,0
B12B  5a           LD E,D
B12C  18 09        JR 0xb137
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
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
