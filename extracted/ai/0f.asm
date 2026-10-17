; tipo 0x0f  vida 6  patron observado: camina
; ---- inicio (b04:66B4[14]) ----
7347  ed 5f        LD A,R
7349  e6 01        AND 1
734B  11 00 01     LD DE,0x100
734E  06 33        LD B,0x33
7350  28 04        JR z,0x7356
7352  11 00 ff     LD DE,0xff00
7355  05           DEC B
7356  cd 3e b1     CALL 0xb13e
7359  21 aa 73     LD HL,0x73aa
735C  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7360  28 03        JR z,0x7365
7362  21 b2 73     LD HL,0x73b2
7365  dd 7e 1e     LD A,(IX+0x1e)
7368  cd 53 40     CALL 0x4053 ; read_word_table
736B  dd 36 08 04  LD (IX+8),4
736F  dd 73 0a     LD (IX+10),E ; d1
7372  dd 72 11     LD (IX+0x11),D ; atributo_sprite
7375  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[14]) ----
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
7359  21 aa 73     LD HL,0x73aa
735C  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7360  28 03        JR z,0x7365
7362  21 b2 73     LD HL,0x73b2
7365  dd 7e 1e     LD A,(IX+0x1e)
7368  cd 53 40     CALL 0x4053 ; read_word_table
736B  dd 36 08 04  LD (IX+8),4
736F  dd 73 0a     LD (IX+10),E ; d1
7372  dd 72 11     LD (IX+0x11),D ; atributo_sprite
7375  c9           RET
7376  dd 7e 1c     LD A,(IX+0x1c)
7379  a7           AND A
737A  c4 96 73     CALL nz,0x7396
737D  cd 8e 73     CALL 0x738e
7380  cd ee 6b     CALL 0x6bee
7383  30 04        JR nc,0x7389
7385  cd 04 6c     CALL 0x6c04
7388  d0           RET nc
7389  cd 5f b1     CALL 0xb15f
738C  18 cb        JR 0x7359
738E  11 04 04     LD DE,0x404
7391  06 00        LD B,0
7393  c3 d6 b0     JP 0xb0d6
7396  dd 36 1c 00  LD (IX+0x1c),0
739A  dd 34 1e     INC (IX+0x1e)
739D  dd 7e 1e     LD A,(IX+0x1e)
73A0  fe 04        CP 4
73A2  38 b5        JR c,0x7359
73A4  dd 36 1e 03  LD (IX+0x1e),3
73A8  18 af        JR 0x7359
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
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
