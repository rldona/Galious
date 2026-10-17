; tipo 0x18  vida 1  patron observado: camina
; ---- inicio (b04:66B4[23]) ----
6C50  ed 5f        LD A,R
6C52  e6 01        AND 1
6C54  dd 77 14     LD (IX+0x14),A
6C57  11 80 00     LD DE,0x80
6C5A  c3 8c 96     JP 0x968c
968C  dd cb 14 46  BIT 0,(IX+0x14)
9690  c4 68 40     CALL nz,0x4068 ; neg_de
9693  c3 3e b1     JP 0xb13e
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[23]) ----
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
6C5D  cd 90 6c     CALL 0x6c90
6C60  dd 7e 01     LD A,(IX+1) ; estado
6C63  a7           AND A
6C64  20 17        JR nz,0x6c7d
6C66  cd ee 6b     CALL 0x6bee
6C69  30 04        JR nc,0x6c6f
6C6B  cd 04 6c     CALL 0x6c04
6C6E  d0           RET nc
6C6F  dd 7e 1f     LD A,(IX+0x1f)
6C72  3d           DEC A
6C73  fe 05        CP 5
6C75  30 13        JR nc,0x6c8a
6C77  cd 71 6b     CALL 0x6b71
6C7A  c3 07 9a     JP 0x9a07
6C7D  dd 7e 1f     LD A,(IX+0x1f)
6C80  3d           DEC A
6C81  fe 05        CP 5
6C83  d8           RET c
6C84  cd 76 6b     CALL 0x6b76
6C87  dd 35 01     DEC (IX+1) ; estado
6C8A  cd d2 6c     CALL 0x6cd2
6C8D  c3 5f b1     JP 0xb15f
6C90  dd 34 08     INC (IX+8)
6C93  dd 7e 08     LD A,(IX+8)
6C96  e6 03        AND 3
6C98  c0           RET nz
6C99  dd 7e 14     LD A,(IX+0x14)
6C9C  a7           AND A
6C9D  28 0e        JR z,0x6cad
6C9F  dd 35 1f     DEC (IX+0x1f)
6CA2  dd 7e 1f     LD A,(IX+0x1f)
6CA5  fe ff        CP 0xff
6CA7  20 0f        JR nz,0x6cb8
6CA9  3e 06        LD A,6
6CAB  18 0b        JR 0x6cb8
6CAD  dd 34 1f     INC (IX+0x1f)
6CB0  dd 7e 1f     LD A,(IX+0x1f)
6CB3  fe 07        CP 7
6CB5  20 01        JR nz,0x6cb8
6CB7  af           XOR A
6CB8  dd 77 1f     LD (IX+0x1f),A
6CBB  21 c4 6c     LD HL,0x6cc4
6CBE  cd 53 40     CALL 0x4053 ; read_word_table
6CC1  c3 6f 73     JP 0x736f
6CD2  dd 7e 14     LD A,(IX+0x14)
6CD5  ee 01        XOR 1
6CD7  dd 77 14     LD (IX+0x14),A
6CDA  c9           RET
736F  dd 73 0a     LD (IX+10),E ; d1
7372  dd 72 11     LD (IX+0x11),D ; atributo_sprite
7375  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
