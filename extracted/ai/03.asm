; tipo 0x03  vida 1  patron observado: camina
; ---- inicio (b04:66B4[2]) ----
6B71  dd cb 02 a6  RES 4,(IX+2) ; flags
6B75  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
ABFE  cd f1 81     CALL 0x81f1
AC01  e6 01        AND 1
AC03  3d           DEC A
AC04  dd 77 01     LD (IX+1),A ; estado
AC07  28 03        JR z,0xac0c
AC09  cd 71 6b     CALL 0x6b71
AC0C  dd 36 08 18  LD (IX+8),0x18
AC10  dd 36 11 66  LD (IX+0x11),0x66 ; atributo_sprite
AC14  dd 36 14 00  LD (IX+0x14),0
AC18  cd f1 81     CALL 0x81f1
AC1B  e6 01        AND 1
AC1D  11 00 01     LD DE,0x100
AC20  28 03        JR z,0xac25
AC22  11 00 01     LD DE,0x100
AC25  c3 3e b1     JP 0xb13e
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[2]) ----
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
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
AC28  dd 7e 01     LD A,(IX+1) ; estado
AC2B  fe ff        CP 0xff
AC2D  ca c8 ac     JP z,0xacc8
AC30  dd 7e 01     LD A,(IX+1) ; estado
AC33  a7           AND A
AC34  20 65        JR nz,0xac9b
AC36  cd aa ac     CALL 0xacaa
AC39  d4 5f b1     CALL nc,0xb15f
AC3C  dd 34 14     INC (IX+0x14)
AC3F  dd 7e 14     LD A,(IX+0x14)
AC42  fe 18        CP 0x18
AC44  38 16        JR c,0xac5c
AC46  cd 71 6b     CALL 0x6b71
AC49  dd cb 0f 7e  BIT 7,(IX+15) ; vx
AC4D  3e cc        LD A,0xcc
AC4F  20 02        JR nz,0xac53
AC51  3e d0        LD A,0xd0
AC53  dd 77 0a     LD (IX+10),A ; d1
AC56  dd 36 11 77  LD (IX+0x11),0x77 ; atributo_sprite
AC5A  18 25        JR 0xac81
AC5C  fe 08        CP 8
AC5E  38 0c        JR c,0xac6c
AC60  cd 76 6b     CALL 0x6b76
AC63  dd 36 0a c8  LD (IX+10),0xc8 ; d1
AC67  dd 36 11 74  LD (IX+0x11),0x74 ; atributo_sprite
AC6B  c9           RET
AC6C  cd 71 6b     CALL 0x6b71
AC6F  dd cb 0f 7e  BIT 7,(IX+15) ; vx
AC73  3e cc        LD A,0xcc
AC75  28 02        JR z,0xac79
AC77  3e d0        LD A,0xd0
AC79  dd 77 0a     LD (IX+10),A ; d1
AC7C  dd 36 11 77  LD (IX+0x11),0x77 ; atributo_sprite
AC80  c9           RET
AC81  dd 7e 14     LD A,(IX+0x14)
AC84  fe 1c        CP 0x1c
AC86  c0           RET nz
AC87  dd 36 0a c4  LD (IX+10),0xc4 ; d1
AC8B  dd 36 11 66  LD (IX+0x11),0x66 ; atributo_sprite
AC8F  dd 36 08 18  LD (IX+8),0x18
AC93  3e 0b        LD A,11
AC95  cd 59 40     CALL 0x4059 ; sound_request
AC98  c3 07 9a     JP 0x9a07
AC9B  dd 35 08     DEC (IX+8)
AC9E  c0           RET nz
AC9F  dd 36 14 00  LD (IX+0x14),0
ACA3  cd 76 6b     CALL 0x6b76
ACA6  dd 35 01     DEC (IX+1) ; estado
ACA9  c9           RET
ACAA  dd cb 0f 7e  BIT 7,(IX+15) ; vx
ACAE  dd 7e 07     LD A,(IX+7) ; x
ACB1  20 02        JR nz,0xacb5
ACB3  c6 10        ADD A,0x10
ACB5  67           LD H,A
ACB6  dd 7e 05     LD A,(IX+5) ; y
ACB9  c6 fe        ADD A,0xfe
ACBB  6f           LD L,A
ACBC  cd 6e 40     CALL 0x406e ; vec_4dc8
ACBF  d0           RET nc
ACC0  cd ee 6b     CALL 0x6bee
ACC3  cd 04 6c     CALL 0x6c04
ACC6  3f           CCF
ACC7  c9           RET
ACC8  3a 05 e5     LD A,(0xe505) ; jugador_y
ACCB  c6 e8        ADD A,0xe8
ACCD  dd be 05     CP (IX+5) ; y
ACD0  c0           RET nz
ACD1  3a 07 e5     LD A,(0xe507) ; jugador_x
ACD4  c6 fa        ADD A,0xfa
ACD6  dd 96 07     SUB (IX+7) ; x
ACD9  30 02        JR nc,0xacdd
ACDB  ed 44        NEG
ACDD  fe 20        CP 0x20
ACDF  d0           RET nc
ACE0  18 b6        JR 0xac98
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
