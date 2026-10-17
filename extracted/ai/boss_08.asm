; jefe 8 (E700), mundo 8, vida (7F97[8]): 80
; ---- inicio (b01:7F82[7]) ----
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
7FB5  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FB8  cd 37 b1     CALL 0xb137
7FBB  c3 15 42     JP 0x4215 ; map_a000_bank03
86DA  11 00 00     LD DE,0
86DD  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
871A  11 00 00     LD DE,0
871D  c3 b5 7f     JP 0x7fb5 ; vy=DE (b06:B137)
8A7A  dd 36 1a 0f  LD (IX+0x1a),15
8A7E  dd 36 19 04  LD (IX+0x19),4
8A82  dd 36 02 f0  LD (IX+2),0xf0 ; flags
8A86  dd 36 05 20  LD (IX+5),0x20 ; y
8A8A  dd 36 07 68  LD (IX+7),0x68 ; x
8A8E  dd 36 0a 28  LD (IX+10),0x28 ; d1
8A92  dd 36 18 30  LD (IX+0x18),0x30
8A96  dd 36 1c 80  LD (IX+0x1c),0x80
8A9A  dd 36 1e 0a  LD (IX+0x1e),10
8A9E  cd da 86     CALL 0x86da
8AA1  c3 1a 87     JP 0x871a
; ---- actualizacion (b01:7F40[7]) ----
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
7FB5  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FB8  cd 37 b1     CALL 0xb137
7FBB  c3 15 42     JP 0x4215 ; map_a000_bank03
86DA  11 00 00     LD DE,0
86DD  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
871A  11 00 00     LD DE,0
871D  c3 b5 7f     JP 0x7fb5 ; vy=DE (b06:B137)
8AA4  cd 8c 8c     CALL 0x8c8c
8AA7  3a 01 e7     LD A,(0xe701)
8AAA  21 b0 8a     LD HL,0x8ab0
8AAD  cd 40 41     CALL 0x4140 ; jump_table_a
8AB0  c0 8a        .word 0x8AC0 ; caso 0
8AB2  14 8b        .word 0x8B14 ; caso 1
8AB4  3a 8b        .word 0x8B3A ; caso 2
8AB6  96 8b        .word 0x8B96 ; caso 3
8AB8  ac 8b        .word 0x8BAC ; caso 4
8ABA  be 8b        .word 0x8BBE ; caso 5
8ABC  e6 8b        .word 0x8BE6 ; caso 6
8ABE  00 8c        .word 0x8C00 ; caso 7
8AC0  dd 35 18     DEC (IX+0x18)
8AC3  28 11        JR z,0x8ad6
8AC5  dd 35 1a     DEC (IX+0x1a)
8AC8  c0           RET nz
8AC9  dd 36 1a 0f  LD (IX+0x1a),15
8ACD  3a 0a e7     LD A,(0xe70a)
8AD0  ee 01        XOR 1
8AD2  32 0a e7     LD (0xe70a),A
8AD5  c9           RET
8AD6  dd 36 0a 25  LD (IX+10),0x25 ; d1
8ADA  dd 34 01     INC (IX+1) ; estado
8ADD  3a 07 e5     LD A,(0xe507) ; jugador_x
8AE0  32 17 e7     LD (0xe717),A
8AE3  3a 05 e5     LD A,(0xe505) ; jugador_y
8AE6  e6 e0        AND 0xe0
8AE8  d6 08        SUB 8
8AEA  32 16 e7     LD (0xe716),A
8AED  3a 07 e7     LD A,(0xe707)
8AF0  fe 80        CP 0x80
8AF2  38 10        JR c,0x8b04
8AF4  fe 98        CP 0x98
8AF6  d2 60 8c     JP nc,0x8c60
8AF9  3a 17 e7     LD A,(0xe717)
8AFC  fe 80        CP 0x80
8AFE  da 60 8c     JP c,0x8c60
8B01  c3 57 8c     JP 0x8c57
8B04  fe 38        CP 0x38
8B06  da 57 8c     JP c,0x8c57
8B09  3a 17 e7     LD A,(0xe717)
8B0C  fe 80        CP 0x80
8B0E  da 60 8c     JP c,0x8c60
8B11  c3 57 8c     JP 0x8c57
8B14  cd 7b 8c     CALL 0x8c7b
8B17  3a 07 e7     LD A,(0xe707)
8B1A  fe 38        CP 0x38
8B1C  28 03        JR z,0x8b21
8B1E  fe 98        CP 0x98
8B20  c0           RET nz
8B21  dd 34 01     INC (IX+1) ; estado
8B24  3a 05 e7     LD A,(0xe705)
8B27  fe a0        CP 0xa0
8B29  28 0d        JR z,0x8b38
8B2B  cd cc 8c     CALL 0x8ccc
8B2E  cd da 86     CALL 0x86da
8B31  dd 36 0a 2a  LD (IX+10),0x2a ; d1
8B35  c3 69 8c     JP 0x8c69
8B38  18 32        JR 0x8b6c
8B3A  3a 05 e7     LD A,(0xe705)
8B3D  fe 98        CP 0x98
8B3F  c0           RET nz
8B40  cd bb 8c     CALL 0x8cbb
8B43  cd 1a 87     CALL 0x871a
8B46  3a 07 e7     LD A,(0xe707)
8B49  fe 38        CP 0x38
8B4B  3a 17 e7     LD A,(0xe717)
8B4E  20 0e        JR nz,0x8b5e
8B50  fe 80        CP 0x80
8B52  38 18        JR c,0x8b6c
8B54  dd 36 0a 25  LD (IX+10),0x25 ; d1
8B58  dd 34 01     INC (IX+1) ; estado
8B5B  c3 57 8c     JP 0x8c57
8B5E  fe 80        CP 0x80
8B60  30 0a        JR nc,0x8b6c
8B62  dd 36 0a 25  LD (IX+10),0x25 ; d1
8B66  dd 34 01     INC (IX+1) ; estado
8B69  c3 60 8c     JP 0x8c60
8B6C  dd cb 02 a6  RES 4,(IX+2) ; flags
8B70  dd 36 20 08  LD (IX+0x20),8
8B74  dd 36 01 04  LD (IX+1),4 ; estado
8B78  af           XOR A
8B79  32 04 e7     LD (0xe704),A
8B7C  32 06 e7     LD (0xe706),A
8B7F  57           LD D,A
8B80  5f           LD E,A
8B81  cd da 86     CALL 0x86da
8B84  dd 36 0a 24  LD (IX+10),0x24 ; d1
8B88  cd 72 8c     CALL 0x8c72
8B8B  3a 05 e7     LD A,(0xe705)
8B8E  d6 08        SUB 8
8B90  dd be 16     CP (IX+0x16) ; ay_lo
8B93  28 33        JR z,0x8bc8
8B95  c9           RET
8B96  cd 7b 8c     CALL 0x8c7b
8B99  3a 0f e7     LD A,(0xe70f)
8B9C  e6 80        AND 0x80
8B9E  3a 07 e7     LD A,(0xe707)
8BA1  28 04        JR z,0x8ba7
8BA3  fe 38        CP 0x38
8BA5  28 c5        JR z,0x8b6c
8BA7  fe 98        CP 0x98
8BA9  c0           RET nz
8BAA  18 c0        JR 0x8b6c
8BAC  dd 35 20     DEC (IX+0x20)
8BAF  c0           RET nz
8BB0  dd 34 01     INC (IX+1) ; estado
8BB3  dd 36 0a 2a  LD (IX+10),0x2a ; d1
8BB7  dd cb 02 e6  SET 4,(IX+2) ; flags
8BBB  c3 cc 8c     JP 0x8ccc
8BBE  3a 05 e7     LD A,(0xe705)
8BC1  dd be 16     CP (IX+0x16) ; ay_lo
8BC4  c0           RET nz
8BC5  cd bb 8c     CALL 0x8cbb
8BC8  dd cb 02 e6  SET 4,(IX+2) ; flags
8BCC  dd 36 0a 25  LD (IX+10),0x25 ; d1
8BD0  cd 1a 87     CALL 0x871a
8BD3  dd 36 18 50  LD (IX+0x18),0x50
8BD7  dd 36 01 06  LD (IX+1),6 ; estado
8BDB  3a 07 e7     LD A,(0xe707)
8BDE  dd be 17     CP (IX+0x17) ; ay
8BE1  d2 60 8c     JP nc,0x8c60
8BE4  18 71        JR 0x8c57
8BE6  cd 7b 8c     CALL 0x8c7b
8BE9  dd 35 18     DEC (IX+0x18)
8BEC  c0           RET nz
8BED  dd 36 0a 28  LD (IX+10),0x28 ; d1
8BF1  11 00 00     LD DE,0
8BF4  cd a3 7f     CALL 0x7fa3 ; vx=DE (b06:B13E)
8BF7  dd 36 18 30  LD (IX+0x18),0x30
8BFB  dd 36 01 00  LD (IX+1),0 ; estado
8BFF  c9           RET
8C00  dd 35 1d     DEC (IX+0x1d)
8C03  28 3d        JR z,0x8c42
8C05  3a 1d e7     LD A,(0xe71d)
8C08  fe 20        CP 0x20
8C0A  20 04        JR nz,0x8c10
8C0C  dd 36 0a 27  LD (IX+10),0x27 ; d1
8C10  fe 10        CP 0x10
8C12  30 0c        JR nc,0x8c20
8C14  dd 36 0a 24  LD (IX+10),0x24 ; d1
8C18  fe 09        CP 9
8C1A  3e 1b        LD A,0x1b
8C1C  ca 1f 4e     JP z,0x4e1f ; sound_request
8C1F  c9           RET
8C20  fe 16        CP 0x16
8C22  d0           RET nc
8C23  fe 11        CP 0x11
8C25  d8           RET c
8C26  3a 05 e7     LD A,(0xe705)
8C29  c6 04        ADD A,4
8C2B  57           LD D,A
8C2C  3a 07 e7     LD A,(0xe707)
8C2F  e6 f8        AND 0xf8
8C31  dd 86 1e     ADD A,(IX+0x1e)
8C34  5f           LD E,A
8C35  3a 1e e7     LD A,(0xe71e)
8C38  ee 1c        XOR 0x1c
8C3A  32 1e e7     LD (0xe71e),A
8C3D  3e 68        LD A,0x68
8C3F  c3 7d 40     JP 0x407d
8C42  dd 36 1c 80  LD (IX+0x1c),0x80
8C46  dd cb 02 e6  SET 4,(IX+2) ; flags
8C4A  3a 1b e7     LD A,(0xe71b)
8C4D  32 01 e7     LD (0xe701),A
8C50  3a 1f e7     LD A,(0xe71f)
8C53  32 0a e7     LD (0xe70a),A
8C56  c9           RET
8C57  dd 36 0f 00  LD (IX+15),0 ; vx
8C5B  dd 36 0e 80  LD (IX+14),0x80 ; vx_lo
8C5F  c9           RET
8C60  dd 36 0f ff  LD (IX+15),0xff ; vx
8C64  dd 36 0e 80  LD (IX+14),0x80 ; vx_lo
8C68  c9           RET
8C69  dd 36 0d 04  LD (IX+13),4 ; vy
8C6D  dd 36 0c 00  LD (IX+12),0 ; vy_lo
8C71  c9           RET
8C72  dd 36 0d fe  LD (IX+13),0xfe ; vy
8C76  dd 36 0c 00  LD (IX+12),0 ; vy_lo
8C7A  c9           RET
8C7B  dd 35 19     DEC (IX+0x19)
8C7E  c0           RET nz
8C7F  dd 36 19 04  LD (IX+0x19),4
8C83  3a 0a e7     LD A,(0xe70a)
8C86  ee 03        XOR 3
8C88  32 0a e7     LD (0xe70a),A
8C8B  c9           RET
8C8C  dd 35 1c     DEC (IX+0x1c)
8C8F  c0           RET nz
8C90  dd 36 1c 01  LD (IX+0x1c),1
8C94  3a 0d e7     LD A,(0xe70d)
8C97  dd 86 0c     ADD A,(IX+12) ; vy_lo
8C9A  c0           RET nz
8C9B  32 1c e7     LD (0xe71c),A
8C9E  dd 36 1d 30  LD (IX+0x1d),0x30
8CA2  dd cb 02 a6  RES 4,(IX+2) ; flags
8CA6  3a 01 e7     LD A,(0xe701)
8CA9  32 1b e7     LD (0xe71b),A
8CAC  dd 36 01 07  LD (IX+1),7 ; estado
8CB0  3a 0a e7     LD A,(0xe70a)
8CB3  32 1f e7     LD (0xe71f),A
8CB6  dd 36 0a 24  LD (IX+10),0x24 ; d1
8CBA  c9           RET
8CBB  3a 05 e7     LD A,(0xe705)
8CBE  c6 08        ADD A,8
8CC0  32 05 e7     LD (0xe705),A
8CC3  3a 07 e7     LD A,(0xe707)
8CC6  d6 08        SUB 8
8CC8  32 07 e7     LD (0xe707),A
8CCB  c9           RET
8CCC  3a 05 e7     LD A,(0xe705)
8CCF  d6 08        SUB 8
8CD1  32 05 e7     LD (0xe705),A
8CD4  3a 07 e7     LD A,(0xe707)
8CD7  c6 08        ADD A,8
8CD9  32 07 e7     LD (0xe707),A
8CDC  c9           RET
