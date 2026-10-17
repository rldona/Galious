; tipo 0x0a  vida 6  patron observado: camina (cae entre plataformas)
; ---- inicio (b04:66B4[9]) ----
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
7B0D  3e 01        LD A,1
7B0F  32 2e e6     LD (0xe62e),A
7B12  dd cb 02 ce  SET 1,(IX+2) ; flags
7B16  cd f2 9a     CALL 0x9af2
7B19  cd a9 7c     CALL 0x7ca9
7B1C  11 50 00     LD DE,0x50
7B1F  cd 45 b1     CALL 0xb145
7B22  dd 36 18 05  LD (IX+0x18),5
7B26  dd cb 1d 46  BIT 0,(IX+0x1d)
7B2A  11 70 00     LD DE,0x70
7B2D  c4 68 40     CALL nz,0x4068 ; neg_de
7B30  cd 3e b1     CALL 0xb13e
7B33  dd cb 1d 46  BIT 0,(IX+0x1d)
7B37  3e b0        LD A,0xb0
7B39  01 aa bc     LD BC,0xbcaa
7B3C  16 4e        LD D,0x4e
7B3E  28 0a        JR z,0x7b4a
7B40  3e c0        LD A,0xc0
7B42  01 b2 cc     LD BC,0xccb2
7B45  14           INC D
7B46  dd cb 02 c6  SET 0,(IX+2) ; flags
7B4A  1e 07        LD E,7
7B4C  c3 f9 98     JP 0x98f9
7CA9  3a 07 e5     LD A,(0xe507) ; jugador_x
7CAC  dd be 07     CP (IX+7) ; x
7CAF  3f           CCF
7CB0  3e 00        LD A,0
7CB2  17           RLA
7CB3  dd 77 1d     LD (IX+0x1d),A
7CB6  c9           RET
98F9  dd 70 12     LD (IX+0x12),B
98FC  dd 71 09     LD (IX+9),C ; d0
98FF  dd 72 11     LD (IX+0x11),D ; atributo_sprite
9902  dd 73 13     LD (IX+0x13),E
9905  c3 83 70     JP 0x7083
9AF2  ed 5f        LD A,R
9AF4  e6 7f        AND 0x7f
9AF6  c3 cf 71     JP 0x71cf
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B145  dd 73 16     LD (IX+0x16),E ; ay_lo
B148  dd 72 17     LD (IX+0x17),D ; ay
B14B  c9           RET
; ---- actualizacion (b06:B472[9]) ----
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
7A3D  dd 7e 09     LD A,(IX+9) ; d0
7A40  ee 03        XOR 3
7A42  dd 77 09     LD (IX+9),A ; d0
7A45  c9           RET
7B22  dd 36 18 05  LD (IX+0x18),5
7B26  dd cb 1d 46  BIT 0,(IX+0x1d)
7B2A  11 70 00     LD DE,0x70
7B2D  c4 68 40     CALL nz,0x4068 ; neg_de
7B30  cd 3e b1     CALL 0xb13e
7B33  dd cb 1d 46  BIT 0,(IX+0x1d)
7B37  3e b0        LD A,0xb0
7B39  01 aa bc     LD BC,0xbcaa
7B3C  16 4e        LD D,0x4e
7B3E  28 0a        JR z,0x7b4a
7B40  3e c0        LD A,0xc0
7B42  01 b2 cc     LD BC,0xccb2
7B45  14           INC D
7B46  dd cb 02 c6  SET 0,(IX+2) ; flags
7B4A  1e 07        LD E,7
7B4C  c3 f9 98     JP 0x98f9
7B4F  dd 7e 01     LD A,(IX+1) ; estado
7B52  3d           DEC A
7B53  28 4c        JR z,0x7ba1
7B55  f2 0a 7c     JP p,0x7c0a
7B58  cd 68 7b     CALL 0x7b68
7B5B  dd 35 08     DEC (IX+8)
7B5E  28 3b        JR z,0x7b9b
7B60  dd 7e 18     LD A,(IX+0x18)
7B63  a7           AND A
7B64  ca 7d 94     JP z,0x947d
7B67  c9           RET
7B68  cd fd 7c     CALL 0x7cfd
7B6B  cd ee 6b     CALL 0x6bee
7B6E  30 04        JR nc,0x7b74
7B70  cd 5e 7c     CALL 0x7c5e
7B73  d0           RET nc
7B74  dd cb 14 46  BIT 0,(IX+0x14)
7B78  20 0e        JR nz,0x7b88
7B7A  cd 74 9a     CALL 0x9a74
7B7D  da fa 7b     JP c,0x7bfa
7B80  cd 5f b1     CALL 0xb15f
7B83  cd 01 9b     CALL 0x9b01
7B86  18 ab        JR 0x7b33
7B88  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7B8C  3a 63 ea     LD A,(0xea63)
7B8F  06 f0        LD B,0xf0
7B91  28 02        JR z,0x7b95
7B93  06 08        LD B,8
7B95  80           ADD A,B
7B96  dd 77 07     LD (IX+7),A ; x
7B99  e1           POP HL
7B9A  c9           RET
7B9B  cd 84 7c     CALL 0x7c84
7B9E  c3 89 94     JP 0x9489
7BA1  ed 5f        LD A,R
7BA3  e6 01        AND 1
7BA5  28 07        JR z,0x7bae
7BA7  cd 67 9a     CALL 0x9a67
7BAA  38 4e        JR c,0x7bfa
7BAC  18 05        JR 0x7bb3
7BAE  cd b7 7c     CALL 0x7cb7
7BB1  da bd 7b     JP c,0x7bbd
7BB3  7b           LD A,E
7BB4  cd 68 7b     CALL 0x7b68
7BB7  cd ff 94     CALL 0x94ff
7BBA  30 5f        JR nc,0x7c1b
7BBC  c9           RET
7BBD  dd 7e 1e     LD A,(IX+0x1e)
7BC0  3d           DEC A
7BC1  11 00 ff     LD DE,0xff00
7BC4  3e a0        LD A,0xa0
7BC6  28 0d        JR z,0x7bd5
7BC8  11 00 01     LD DE,0x100
7BCB  dd 7e 05     LD A,(IX+5) ; y
7BCE  c6 09        ADD A,9
7BD0  dd 77 05     LD (IX+5),A ; y
7BD3  3e a8        LD A,0xa8
7BD5  dd 77 0a     LD (IX+10),A ; d1
7BD8  dd 36 11 55  LD (IX+0x11),0x55 ; atributo_sprite
7BDC  cd 3d 7a     CALL 0x7a3d
7BDF  cd 37 b1     CALL 0xb137
7BE2  dd 36 01 03  LD (IX+1),3 ; estado
7BE6  c3 2e b1     JP 0xb12e
7BE9  dd 36 01 01  LD (IX+1),1 ; estado
7BED  dd 34 1e     INC (IX+0x1e)
7BF0  dd 7e 1e     LD A,(IX+0x1e)
7BF3  d6 03        SUB 3
7BF5  c0           RET nz
7BF6  dd 77 1e     LD (IX+0x1e),A
7BF9  c9           RET
7BFA  cd 88 99     CALL 0x9988
7BFD  dd cb 1d 46  BIT 0,(IX+0x1d)
7C01  3e b8        LD A,0xb8
7C03  28 02        JR z,0x7c07
7C05  3e c8        LD A,0xc8
7C07  c3 83 70     JP 0x7083
7C0A  3d           DEC A
7C0B  20 1a        JR nz,0x7c27
7C0D  cd 1d 9b     CALL 0x9b1d
7C10  c0           RET nz
7C11  cd c9 99     CALL 0x99c9
7C14  d0           RET nc
7C15  cd 15 9b     CALL 0x9b15
7C18  cd 68 7b     CALL 0x7b68
7C1B  cd f2 9a     CALL 0x9af2
7C1E  cd 29 b1     CALL 0xb129
7C21  cd d5 6b     CALL 0x6bd5
7C24  c3 22 7b     JP 0x7b22
7C27  16 04        LD D,4
7C29  06 00        LD B,0
7C2B  cd 7f b1     CALL 0xb17f
7C2E  dd 7e 05     LD A,(IX+5) ; y
7C31  d6 20        SUB 0x20
7C33  fe 90        CP 0x90
7C35  d4 53 7c     CALL nc,0x7c53
7C38  cd c9 99     CALL 0x99c9
7C3B  d0           RET nc
7C3C  dd 6e 05     LD L,(IX+5) ; y
7C3F  dd 66 07     LD H,(IX+7) ; x
7C42  cd 56 40     CALL 0x4056 ; vec_4da1
7C45  7e           LD A,(HL)
7C46  fe ba        CP 0xba
7C48  d2 53 7c     JP nc,0x7c53
7C4B  cd 15 9b     CALL 0x9b15
7C4E  cd 99 7c     CALL 0x7c99
7C51  18 c5        JR 0x7c18
7C53  dd 7e 0a     LD A,(IX+10) ; d1
7C56  ee 08        XOR 8
7C58  dd 77 0a     LD (IX+10),A ; d1
7C5B  c3 6f b1     JP 0xb16f
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
7C84  3a 05 e5     LD A,(0xe505) ; jugador_y
7C87  d6 10        SUB 0x10
7C89  dd be 05     CP (IX+5) ; y
7C8C  06 00        LD B,0
7C8E  28 05        JR z,0x7c95
7C90  06 01        LD B,1
7C92  38 01        JR c,0x7c95
7C94  04           INC B
7C95  dd 70 1e     LD (IX+0x1e),B
7C98  c9           RET
7C99  dd cb 1d 46  BIT 0,(IX+0x1d)
7C9D  11 70 00     LD DE,0x70
7CA0  c4 68 40     CALL nz,0x4068 ; neg_de
7CA3  cd 3e b1     CALL 0xb13e
7CA6  c3 33 7b     JP 0x7b33
7CB7  dd 7e 1e     LD A,(IX+0x1e)
7CBA  3d           DEC A
7CBB  20 09        JR nz,0x7cc6
7CBD  dd 7e 05     LD A,(IX+5) ; y
7CC0  c6 0f        ADD A,15
7CC2  6f           LD L,A
7CC3  c3 ce 7c     JP 0x7cce
7CC6  3d           DEC A
7CC7  c0           RET nz
7CC8  dd 7e 05     LD A,(IX+5) ; y
7CCB  c6 18        ADD A,0x18
7CCD  6f           LD L,A
7CCE  dd 66 07     LD H,(IX+7) ; x
7CD1  0e 04        LD C,4
7CD3  e5           PUSH HL
7CD4  7c           LD A,H
7CD5  81           ADD A,C
7CD6  67           LD H,A
7CD7  cd 56 40     CALL 0x4056 ; vec_4da1
7CDA  7e           LD A,(HL)
7CDB  e1           POP HL
7CDC  d6 9a        SUB 0x9a
7CDE  fe 02        CP 2
7CE0  d0           RET nc
7CE1  3e 10        LD A,0x10
7CE3  91           SUB C
7CE4  4f           LD C,A
7CE5  7c           LD A,H
7CE6  81           ADD A,C
7CE7  67           LD H,A
7CE8  e5           PUSH HL
7CE9  cd 56 40     CALL 0x4056 ; vec_4da1
7CEC  7e           LD A,(HL)
7CED  e1           POP HL
7CEE  d6 9a        SUB 0x9a
7CF0  fe 02        CP 2
7CF2  d0           RET nc
7CF3  7c           LD A,H
7CF4  d6 08        SUB 8
7CF6  e6 f8        AND 0xf8
7CF8  dd 77 07     LD (IX+7),A ; x
7CFB  37           SCF
7CFC  c9           RET
7CFD  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7D01  3e b0        LD A,0xb0
7D03  28 02        JR z,0x7d07
7D05  3e c0        LD A,0xc0
7D07  21 04 e6     LD HL,0xe604
7D0A  cb 56        BIT 2,(HL)
7D0C  ca 33 7b     JP z,0x7b33
7D0F  c6 04        ADD A,4
7D11  dd cb 09 ae  RES 5,(IX+9) ; d0
7D15  c3 83 70     JP 0x7083
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
98F9  dd 70 12     LD (IX+0x12),B
98FC  dd 71 09     LD (IX+9),C ; d0
98FF  dd 72 11     LD (IX+0x11),D ; atributo_sprite
9902  dd 73 13     LD (IX+0x13),E
9905  c3 83 70     JP 0x7083
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
B12E  16 00        LD D,0
B130  5a           LD E,D
B131  18 0b        JR 0xb13e
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
B16F  dd 5e 0c     LD E,(IX+12) ; vy_lo
B172  dd 56 0d     LD D,(IX+13) ; vy
B175  cd 68 40     CALL 0x4068 ; neg_de
B178  dd 73 0c     LD (IX+12),E ; vy_lo
B17B  dd 72 0d     LD (IX+13),D ; vy
B17E  c9           RET
B17F  0e 07        LD C,7
B181  18 02        JR 0xb185
B185  3a 04 e6     LD A,(0xe604)
B188  a1           AND C
B189  c0           RET nz
B18A  c3 dd b0     JP 0xb0dd
