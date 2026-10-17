; tipo 0x10  vida 1  patron observado: quieto
; ---- inicio (b04:66B4[15]) ----
6B71  dd cb 02 a6  RES 4,(IX+2) ; flags
6B75  c9           RET
71C9  cd 71 6b     CALL 0x6b71
71CC  dd 7e 07     LD A,(IX+7) ; x
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
; ---- actualizacion (b06:B472[15]) ----
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
6C25  3a 05 e5     LD A,(0xe505) ; jugador_y
6C28  d6 10        SUB 0x10
6C2A  dd 96 05     SUB (IX+5) ; y
6C2D  c6 04        ADD A,4
6C2F  fe 09        CP 9
6C31  c9           RET
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
71D3  dd 7e 01     LD A,(IX+1) ; estado
71D6  3d           DEC A
71D7  28 20        JR z,0x71f9
71D9  f2 18 72     JP p,0x7218
71DC  cd 25 6c     CALL 0x6c25
71DF  d0           RET nc
71E0  dd 34 01     INC (IX+1) ; estado
71E3  cd 76 6b     CALL 0x6b76
71E6  dd 36 0a a0  LD (IX+10),0xa0 ; d1
71EA  3a 07 e5     LD A,(0xe507) ; jugador_x
71ED  dd be 07     CP (IX+7) ; x
71F0  11 00 02     LD DE,0x200
71F3  dc 68 40     CALL c,0x4068 ; neg_de
71F6  c3 3e b1     JP 0xb13e
71F9  06 00        LD B,0
71FB  cd 2d 72     CALL 0x722d
71FE  cd ee 6b     CALL 0x6bee
7201  d2 08 72     JP nc,0x7208
7204  cd 04 6c     CALL 0x6c04
7207  d0           RET nc
7208  dd cb 0f 7e  BIT 7,(IX+15) ; vx
720C  11 00 01     LD DE,0x100
720F  cc 68 40     CALL z,0x4068 ; neg_de
7212  cd 3e b1     CALL 0xb13e
7215  c3 07 9a     JP 0x9a07
7218  dd 7e 07     LD A,(IX+7) ; x
721B  dd be 08     CP (IX+8)
721E  06 01        LD B,1
7220  20 0b        JR nz,0x722d
7222  cd d5 6b     CALL 0x6bd5
7225  cd 71 6b     CALL 0x6b71
7228  dd 36 0a a0  LD (IX+10),0xa0 ; d1
722C  c9           RET
722D  04           INC B
722E  04           INC B
722F  dd 34 1f     INC (IX+0x1f)
7232  dd 7e 1f     LD A,(IX+0x1f)
7235  1f           RRA
7236  10 fd        DJNZ 0x7235
7238  e6 03        AND 3
723A  4f           LD C,A
723B  06 00        LD B,0
723D  21 45 72     LD HL,0x7245
7240  09           ADD HL,BC
7241  7e           LD A,(HL)
7242  c3 83 70     JP 0x7083
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
