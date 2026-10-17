; tipo 0x1f  vida 3  patron observado: camina
; ---- inicio (b04:66B4[30]) ----
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
942F  cd f2 9a     CALL 0x9af2
9432  cd f9 9a     CALL 0x9af9
9435  11 00 01     LD DE,0x100
9438  cd 45 b1     CALL 0xb145
943B  3e a0        LD A,0xa0
943D  11 a0 03     LD DE,0x3a0
9440  dd cb 1d 46  BIT 0,(IX+0x1d)
9444  28 09        JR z,0x944f
9446  3e ac        LD A,0xac
9448  11 60 fc     LD DE,0xfc60
944B  dd 36 11 13  LD (IX+0x11),0x13 ; atributo_sprite
944F  dd 77 0a     LD (IX+10),A ; d1
9452  c3 3e b1     JP 0xb13e
9AF2  ed 5f        LD A,R
9AF4  e6 7f        AND 0x7f
9AF6  c3 cf 71     JP 0x71cf
9AF9  ed 5f        LD A,R
9AFB  e6 01        AND 1
9AFD  dd 77 1d     LD (IX+0x1d),A
9B00  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B145  dd 73 16     LD (IX+0x16),E ; ay_lo
B148  dd 72 17     LD (IX+0x17),D ; ay
B14B  c9           RET
; ---- actualizacion (b06:B472[30]) ----
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
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
7973  dd 36 11 00  LD (IX+0x11),0 ; atributo_sprite
7977  c9           RET
7BE9  dd 36 01 01  LD (IX+1),1 ; estado
7BED  dd 34 1e     INC (IX+0x1e)
7BF0  dd 7e 1e     LD A,(IX+0x1e)
7BF3  d6 03        SUB 3
7BF5  c0           RET nz
7BF6  dd 77 1e     LD (IX+0x1e),A
7BF9  c9           RET
943B  3e a0        LD A,0xa0
943D  11 a0 03     LD DE,0x3a0
9440  dd cb 1d 46  BIT 0,(IX+0x1d)
9444  28 09        JR z,0x944f
9446  3e ac        LD A,0xac
9448  11 60 fc     LD DE,0xfc60
944B  dd 36 11 13  LD (IX+0x11),0x13 ; atributo_sprite
944F  dd 77 0a     LD (IX+10),A ; d1
9452  c3 3e b1     JP 0xb13e
9455  dd 7e 01     LD A,(IX+1) ; estado
9458  3d           DEC A
9459  28 35        JR z,0x9490
945B  f2 d0 94     JP p,0x94d0
945E  cd 67 94     CALL 0x9467
9461  dd 35 08     DEC (IX+8)
9464  28 17        JR z,0x947d
9466  c9           RET
9467  cd e9 94     CALL 0x94e9
946A  cd ee 6b     CALL 0x6bee
946D  30 04        JR nc,0x9473
946F  cd 04 6c     CALL 0x6c04
9472  d0           RET nc
9473  cd f8 94     CALL 0x94f8
9476  38 26        JR c,0x949e
9478  cd 09 9b     CALL 0x9b09
947B  18 be        JR 0x943b
947D  ed 5f        LD A,R
947F  e6 03        AND 3
9481  fe 03        CP 3
9483  20 01        JR nz,0x9486
9485  af           XOR A
9486  dd 77 1e     LD (IX+0x1e),A
9489  dd 36 18 06  LD (IX+0x18),6
948D  c3 07 9a     JP 0x9a07
9490  cd 67 9a     CALL 0x9a67
9493  38 0a        JR c,0x949f
9495  cd 67 94     CALL 0x9467
9498  cd ff 94     CALL 0x94ff
949B  30 40        JR nc,0x94dd
949D  c9           RET
949E  e1           POP HL
949F  dd 7e 1e     LD A,(IX+0x1e)
94A2  cb 47        BIT 0,A
94A4  11 00 f7     LD DE,0xf700
94A7  20 03        JR nz,0x94ac
94A9  11 00 fc     LD DE,0xfc00
94AC  cd 37 b1     CALL 0xb137
94AF  87           ADD A,A
94B0  dd 86 1d     ADD A,(IX+0x1d)
94B3  21 10 95     LD HL,0x9510
94B6  cd 53 40     CALL 0x4053 ; read_word_table
94B9  cd 3e b1     CALL 0xb13e
94BC  3e a8        LD A,0xa8
94BE  dd cb 1d 46  BIT 0,(IX+0x1d)
94C2  28 02        JR z,0x94c6
94C4  3e b4        LD A,0xb4
94C6  cd 73 79     CALL 0x7973
94C9  dd 36 01 02  LD (IX+1),2 ; estado
94CD  c3 83 70     JP 0x7083
94D0  cd 1d 9b     CALL 0x9b1d
94D3  cd c9 99     CALL 0x99c9
94D6  d0           RET nc
94D7  cd 15 9b     CALL 0x9b15
94DA  cd 67 94     CALL 0x9467
94DD  cd f2 9a     CALL 0x9af2
94E0  cd d5 6b     CALL 0x6bd5
94E3  cd 29 b1     CALL 0xb129
94E6  c3 3b 94     JP 0x943b
94E9  16 04        LD D,4
94EB  dd 7e 1d     LD A,(IX+0x1d)
94EE  a7           AND A
94EF  28 02        JR z,0x94f3
94F1  16 1c        LD D,0x1c
94F3  06 13        LD B,0x13
94F5  c3 7f b1     JP 0xb17f
94F8  16 2c        LD D,0x2c
94FA  1e e2        LD E,0xe2
94FC  c3 77 9a     JP 0x9a77
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
B17F  0e 07        LD C,7
B181  18 02        JR 0xb185
B185  3a 04 e6     LD A,(0xe604)
B188  a1           AND C
B189  c0           RET nz
B18A  c3 dd b0     JP 0xb0dd
