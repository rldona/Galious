; tipo 0x0b  vida 1  patron observado: camina
; ---- inicio (b04:66B4[10]) ----
7A93  dd 36 08 08  LD (IX+8),8
7A97  ed 5f        LD A,R
7A99  e6 01        AND 1
7A9B  3e a0        LD A,0xa0
7A9D  06 3f        LD B,0x3f
7A9F  11 00 04     LD DE,0x400
7AA2  28 07        JR z,0x7aab
7AA4  3e a8        LD A,0xa8
7AA6  06 05        LD B,5
7AA8  11 00 fc     LD DE,0xfc00
7AAB  dd 77 0a     LD (IX+10),A ; d1
7AAE  dd 70 11     LD (IX+0x11),B ; atributo_sprite
7AB1  cd 3e b1     CALL 0xb13e
7AB4  ed 5f        LD A,R
7AB6  e6 2f        AND 0x2f
7AB8  dd 77 15     LD (IX+0x15),A
7ABB  c6 50        ADD A,0x50
7ABD  dd 77 14     LD (IX+0x14),A
7AC0  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[10]) ----
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
7AB4  ed 5f        LD A,R
7AB6  e6 2f        AND 0x2f
7AB8  dd 77 15     LD (IX+0x15),A
7ABB  c6 50        ADD A,0x50
7ABD  dd 77 14     LD (IX+0x14),A
7AC0  c9           RET
7AC1  dd 7e 01     LD A,(IX+1) ; estado
7AC4  b7           OR A
7AC5  20 23        JR nz,0x7aea
7AC7  cd ee 6b     CALL 0x6bee
7ACA  cd 04 6c     CALL 0x6c04
7ACD  30 06        JR nc,0x7ad5
7ACF  cd f7 7a     CALL 0x7af7
7AD2  cd 5f b1     CALL 0xb15f
7AD5  16 04        LD D,4
7AD7  06 3a        LD B,0x3a
7AD9  1e 08        LD E,8
7ADB  cd d6 b0     CALL 0xb0d6
7ADE  dd 35 14     DEC (IX+0x14)
7AE1  28 01        JR z,0x7ae4
7AE3  c9           RET
7AE4  cd 71 6b     CALL 0x6b71
7AE7  c3 07 9a     JP 0x9a07
7AEA  dd 35 15     DEC (IX+0x15)
7AED  c0           RET nz
7AEE  cd 76 6b     CALL 0x6b76
7AF1  cd b4 7a     CALL 0x7ab4
7AF4  c3 a6 ac     JP 0xaca6
7AF7  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7AFB  dd 36 0a a0  LD (IX+10),0xa0 ; d1
7AFF  dd 36 11 3e  LD (IX+0x11),0x3e ; atributo_sprite
7B03  c0           RET nz
7B04  dd 36 0a a8  LD (IX+10),0xa8 ; d1
7B08  dd 36 11 05  LD (IX+0x11),5 ; atributo_sprite
7B0C  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
ACA6  dd 35 01     DEC (IX+1) ; estado
ACA9  c9           RET
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
