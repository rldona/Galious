; tipo 0x20  vida None  patron observado: camina
; ---- inicio (b04:66B4[31]) ----
93D4  11 40 00     LD DE,0x40
93D7  cd 3e b1     CALL 0xb13e
93DA  ed 5f        LD A,R
93DC  e6 01        AND 1
93DE  c4 5f b1     CALL nz,0xb15f
93E1  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
; ---- actualizacion (b06:B472[31]) ----
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
93E2  cd ee 6b     CALL 0x6bee
93E5  30 05        JR nc,0x93ec
93E7  cd 04 6c     CALL 0x6c04
93EA  30 0e        JR nc,0x93fa
93EC  dd cb 0f 7e  BIT 7,(IX+15) ; vx
93F0  11 40 00     LD DE,0x40
93F3  cd 3e b1     CALL 0xb13e
93F6  c0           RET nz
93F7  c3 5f b1     JP 0xb15f
93FA  3a 07 e5     LD A,(0xe507) ; jugador_x
93FD  dd 96 07     SUB (IX+7) ; x
9400  30 02        JR nc,0x9404
9402  ed 44        NEG
9404  fe 50        CP 0x50
9406  d0           RET nc
9407  dd cb 0f 7e  BIT 7,(IX+15) ; vx
940B  28 11        JR z,0x941e
940D  cd 25 6c     CALL 0x6c25
9410  d0           RET nc
9411  3a 07 e5     LD A,(0xe507) ; jugador_x
9414  dd be 07     CP (IX+7) ; x
9417  dc 29 94     CALL c,0x9429
941A  dc 5f b1     CALL c,0xb15f
941D  c9           RET
941E  3a 07 e5     LD A,(0xe507) ; jugador_x
9421  dd be 07     CP (IX+7) ; x
9424  d8           RET c
9425  cd 25 6c     CALL 0x6c25
9428  d0           RET nc
9429  11 00 01     LD DE,0x100
942C  c3 3e b1     JP 0xb13e
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
