; tipo 0x07  vida 2  patron observado: camina (cae entre plataformas)
; ---- inicio (b04:66B4[6]) ----
A830  11 80 00     LD DE,0x80
A833  cd 3e b1     CALL 0xb13e
A836  ed 5f        LD A,R
A838  e6 01        AND 1
A83A  cc 5f b1     CALL z,0xb15f
A83D  dd 7e 05     LD A,(IX+5) ; y
A840  dd 77 16     LD (IX+0x16),A ; ay_lo
A843  dd 36 08 0a  LD (IX+8),10
A847  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
; ---- actualizacion (b06:B472[6]) ----
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
96FE  dd 66 1e     LD H,(IX+0x1e)
9701  dd 6e 1f     LD L,(IX+0x1f)
9704  18 06        JR 0x970c
970C  cd 6e 40     CALL 0x406e ; vec_4dc8
970F  3f           CCF
9710  d0           RET nc
9711  23           INC HL
9712  be           CP (HL)
9713  3f           CCF
9714  d0           RET nc
9715  01 1f 00     LD BC,0x1f
9718  09           ADD HL,BC
9719  be           CP (HL)
971A  3f           CCF
971B  d0           RET nc
971C  23           INC HL
971D  be           CP (HL)
971E  3f           CCF
971F  c9           RET
A848  dd 7e 01     LD A,(IX+1) ; estado
A84B  b7           OR A
A84C  20 60        JR nz,0xa8ae
A84E  cd d6 a8     CALL 0xa8d6
A851  cd ee 6b     CALL 0x6bee
A854  30 07        JR nc,0xa85d
A856  cd 04 6c     CALL 0x6c04
A859  dc 5f b1     CALL c,0xb15f
A85C  c9           RET
A85D  dd cb 0f 7e  BIT 7,(IX+15) ; vx
A861  28 0c        JR z,0xa86f
A863  dd 7e 07     LD A,(IX+7) ; x
A866  fe 38        CP 0x38
A868  da 5f b1     JP c,0xb15f
A86B  c6 d3        ADD A,0xd3
A86D  18 0a        JR 0xa879
A86F  dd 7e 07     LD A,(IX+7) ; x
A872  fe b8        CP 0xb8
A874  d2 5f b1     JP nc,0xb15f
A877  c6 30        ADD A,0x30
A879  67           LD H,A
A87A  dd 77 1e     LD (IX+0x1e),A
A87D  dd 7e 05     LD A,(IX+5) ; y
A880  dd 77 1f     LD (IX+0x1f),A
A883  c6 10        ADD A,0x10
A885  6f           LD L,A
A886  cd 6e 40     CALL 0x406e ; vec_4dc8
A889  d2 5f b1     JP nc,0xb15f
A88C  cd 71 40     CALL 0x4071
A88F  d2 5f b1     JP nc,0xb15f
A892  cd fe 96     CALL 0x96fe
A895  d2 5f b1     JP nc,0xb15f
A898  dd 34 01     INC (IX+1) ; estado
A89B  11 e0 fd     LD DE,0xfde0
A89E  cd 37 b1     CALL 0xb137
A8A1  11 60 01     LD DE,0x160
A8A4  dd cb 0f 7e  BIT 7,(IX+15) ; vx
A8A8  c4 68 40     CALL nz,0x4068 ; neg_de
A8AB  c3 3e b1     JP 0xb13e
A8AE  dd 7e 16     LD A,(IX+0x16) ; ay_lo
A8B1  dd be 05     CP (IX+5) ; y
A8B4  38 12        JR c,0xa8c8
A8B6  dd 36 0a a4  LD (IX+10),0xa4 ; d1
A8BA  11 22 00     LD DE,0x22
A8BD  dd 66 0d     LD H,(IX+13) ; vy
A8C0  dd 6e 0c     LD L,(IX+12) ; vy_lo
A8C3  19           ADD HL,DE
A8C4  eb           EX DE,HL
A8C5  c3 37 b1     JP 0xb137
A8C8  dd 35 01     DEC (IX+1) ; estado
A8CB  dd 77 05     LD (IX+5),A ; y
A8CE  cd 29 b1     CALL 0xb129
A8D1  11 80 00     LD DE,0x80
A8D4  18 ce        JR 0xa8a4
A8D6  16 04        LD D,4
A8D8  1e 0a        LD E,10
A8DA  06 01        LD B,1
A8DC  c3 d6 b0     JP 0xb0d6
B0D6  dd 35 08     DEC (IX+8)
B0D9  c0           RET nz
B0DA  dd 73 08     LD (IX+8),E
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
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
