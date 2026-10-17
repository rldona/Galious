; tipo 0x64  vida 4  patron observado: camina
; ---- inicio (b04:66B4[99]) ----
6B71  dd cb 02 a6  RES 4,(IX+2) ; flags
6B75  c9           RET
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
A97F  cd 71 6b     CALL 0x6b71
A982  dd 36 1d 1a  LD (IX+0x1d),0x1a
A986  dd 36 1e 08  LD (IX+0x1e),8
A98A  dd 36 1f 20  LD (IX+0x1f),0x20
A98E  11 00 01     LD DE,0x100
A991  cd 3e b1     CALL 0xb13e
A994  cd 6c aa     CALL 0xaa6c
A997  dd 7e 16     LD A,(IX+0x16) ; ay_lo
A99A  c3 83 70     JP 0x7083
AA6C  dd cb 0f 7e  BIT 7,(IX+15) ; vx
AA70  20 11        JR nz,0xaa83
AA72  dd 36 16 a0  LD (IX+0x16),0xa0 ; ay_lo
AA76  dd 36 17 a4  LD (IX+0x17),0xa4 ; ay
AA7A  dd 36 18 a8  LD (IX+0x18),0xa8
AA7E  dd 36 19 ac  LD (IX+0x19),0xac
AA82  c9           RET
AA83  dd 36 16 b0  LD (IX+0x16),0xb0 ; ay_lo
AA87  dd 36 17 b4  LD (IX+0x17),0xb4 ; ay
AA8B  dd 36 18 b8  LD (IX+0x18),0xb8
AA8F  dd 36 19 bc  LD (IX+0x19),0xbc
AA93  c9           RET
AAA3  cd 71 6b     CALL 0x6b71
AAA6  dd 7e f4     LD A,(IX-12)
AAA9  dd 77 14     LD (IX+0x14),A
AAAC  dd 7e f5     LD A,(IX-11)
AAAF  dd 77 15     LD (IX+0x15),A
AAB2  c3 7f a9     JP 0xa97f
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[99]) ----
6B71  dd cb 02 a6  RES 4,(IX+2) ; flags
6B75  c9           RET
6B76  dd cb 02 e6  SET 4,(IX+2) ; flags
6B7A  c9           RET
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
A99D  dd 35 15     DEC (IX+0x15)
A9A0  c0           RET nz
A9A1  dd 34 15     INC (IX+0x15)
A9A4  21 6c b0     LD HL,0xb06c
A9A7  e5           PUSH HL
A9A8  cd 9e aa     CALL 0xaa9e
A9AB  3d           DEC A
A9AC  28 19        JR z,0xa9c7
A9AE  f2 1c aa     JP p,0xaa1c
A9B1  18 2b        JR 0xa9de
A9C7  cd 76 6b     CALL 0x6b76
A9CA  cd e1 a9     CALL 0xa9e1
A9CD  dd cb 1a 46  BIT 0,(IX+0x1a)
A9D1  20 25        JR nz,0xa9f8
A9D3  cd 1c aa     CALL 0xaa1c
A9D6  dd 7e 1b     LD A,(IX+0x1b)
A9D9  b7           OR A
A9DA  c0           RET nz
A9DB  c3 65 aa     JP 0xaa65
A9DE  cd 71 6b     CALL 0x6b71
A9E1  dd 7e 00     LD A,(IX) ; tipo
A9E4  fe 2c        CP 0x2c
A9E6  28 08        JR z,0xa9f0
A9E8  dd 7e e0     LD A,(IX-0x20)
A9EB  b7           OR A
A9EC  ca f8 a9     JP z,0xa9f8
A9EF  c9           RET
A9F0  dd 7e 20     LD A,(IX+0x20)
A9F3  b7           OR A
A9F4  ca f8 a9     JP z,0xa9f8
A9F7  c9           RET
A9F8  dd 36 1a 01  LD (IX+0x1a),1
A9FC  dd 36 1d 0d  LD (IX+0x1d),13
AA00  dd 36 1e 06  LD (IX+0x1e),6
AA04  dd 36 1f 11  LD (IX+0x1f),0x11
AA08  dd cb 0f 7e  BIT 7,(IX+15) ; vx
AA0C  11 00 02     LD DE,0x200
AA0F  c4 68 40     CALL nz,0x4068 ; neg_de
AA12  cd 3e b1     CALL 0xb13e
AA15  3e 02        LD A,2
AA17  cd 94 aa     CALL 0xaa94
AA1A  77           LD (HL),A
AA1B  c9           RET
AA1C  cd 76 6b     CALL 0x6b76
AA1F  cd ee 6b     CALL 0x6bee
AA22  30 05        JR nc,0xaa29
AA24  cd 04 6c     CALL 0x6c04
AA27  30 06        JR nc,0xaa2f
AA29  cd 5f b1     CALL 0xb15f
AA2C  cd 6c aa     CALL 0xaa6c
AA2F  dd 34 1b     INC (IX+0x1b)
AA32  dd 7e 1b     LD A,(IX+0x1b)
AA35  dd be 1d     CP (IX+0x1d)
AA38  38 0b        JR c,0xaa45
AA3A  cd 71 6b     CALL 0x6b71
AA3D  dd 7e 19     LD A,(IX+0x19)
AA40  dd 77 0a     LD (IX+10),A ; d1
AA43  18 11        JR 0xaa56
AA45  dd be 1e     CP (IX+0x1e)
AA48  38 06        JR c,0xaa50
AA4A  dd 7e 18     LD A,(IX+0x18)
AA4D  c3 83 70     JP 0x7083
AA50  dd 7e 17     LD A,(IX+0x17) ; ay
AA53  dd 77 0a     LD (IX+10),A ; d1
AA56  dd 7e 1b     LD A,(IX+0x1b)
AA59  dd 96 1f     SUB (IX+0x1f)
AA5C  c0           RET nz
AA5D  dd 77 1b     LD (IX+0x1b),A
AA60  dd 7e 16     LD A,(IX+0x16) ; ay_lo
AA63  18 e8        JR 0xaa4d
AA65  cd 9e aa     CALL 0xaa9e
AA68  ee 01        XOR 1
AA6A  18 ab        JR 0xaa17
AA6C  dd cb 0f 7e  BIT 7,(IX+15) ; vx
AA70  20 11        JR nz,0xaa83
AA72  dd 36 16 a0  LD (IX+0x16),0xa0 ; ay_lo
AA76  dd 36 17 a4  LD (IX+0x17),0xa4 ; ay
AA7A  dd 36 18 a8  LD (IX+0x18),0xa8
AA7E  dd 36 19 ac  LD (IX+0x19),0xac
AA82  c9           RET
AA83  dd 36 16 b0  LD (IX+0x16),0xb0 ; ay_lo
AA87  dd 36 17 b4  LD (IX+0x17),0xb4 ; ay
AA8B  dd 36 18 b8  LD (IX+0x18),0xb8
AA8F  dd 36 19 bc  LD (IX+0x19),0xbc
AA93  c9           RET
AA94  06 00        LD B,0
AA96  dd 4e 14     LD C,(IX+0x14)
AA99  21 38 e6     LD HL,0xe638
AA9C  09           ADD HL,BC
AA9D  c9           RET
AA9E  cd 94 aa     CALL 0xaa94
AAA1  7e           LD A,(HL)
AAA2  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
