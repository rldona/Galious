; tipo 0x23  vida 64  patron observado: quieto
; ---- inicio (b04:66B4[34]) ----
96F5  dd 7e 02     LD A,(IX+2) ; flags
96F8  e6 0f        AND 15
96FA  dd 77 02     LD (IX+2),A ; flags
96FD  c9           RET
A198  dd 36 12 a4  LD (IX+0x12),0xa4
A19C  dd 36 13 0e  LD (IX+0x13),14
A1A0  dd 7e 05     LD A,(IX+5) ; y
A1A3  c6 08        ADD A,8
A1A5  dd 77 16     LD (IX+0x16),A ; ay_lo
A1A8  dd 7e 07     LD A,(IX+7) ; x
A1AB  d6 0d        SUB 13
A1AD  dd 77 14     LD (IX+0x14),A
A1B0  c6 24        ADD A,0x24
A1B2  dd 77 15     LD (IX+0x15),A
A1B5  c3 f5 96     JP 0x96f5
; ---- actualizacion (b06:B472[34]) ----
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
7DB8  dd 36 01 01  LD (IX+1),1 ; estado
7DBC  c9           RET
96EE  3e f0        LD A,0xf0
96F0  dd b6 02     OR (IX+2) ; flags
96F3  18 05        JR 0x96fa
96FA  dd 77 02     LD (IX+2),A ; flags
96FD  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
A1B8  dd 7e 01     LD A,(IX+1) ; estado
A1BB  cd 50 40     CALL 0x4050 ; jump_table_a
A1BE  c4 a1        .word 0xA1C4 ; caso 0
A1C0  fd a1        .word 0xA1FD ; caso 1
A1C2  26 a2        .word 0xA226 ; caso 2
A1C4  cd 58 a2     CALL 0xa258
A1C7  cd 4e a2     CALL 0xa24e
A1CA  d0           RET nc
A1CB  dd cb 17 c6  SET 0,(IX+0x17) ; ay
A1CF  cd 61 a2     CALL 0xa261
A1D2  cd 4c a2     CALL 0xa24c
A1D5  30 09        JR nc,0xa1e0
A1D7  dd cb 17 ce  SET 1,(IX+0x17) ; ay
A1DB  cd 7d a2     CALL 0xa27d
A1DE  28 0f        JR z,0xa1ef
A1E0  cd 6d a2     CALL 0xa26d
A1E3  cd 4c a2     CALL 0xa24c
A1E6  d0           RET nc
A1E7  dd cb 17 d6  SET 2,(IX+0x17) ; ay
A1EB  cd 7d a2     CALL 0xa27d
A1EE  c0           RET nz
A1EF  cd ee 96     CALL 0x96ee
A1F2  cd 3e b1     CALL 0xb13e
A1F5  c3 07 9a     JP 0x9a07
A1FD  dd 7e 07     LD A,(IX+7) ; x
A200  dd cb 0f 7e  BIT 7,(IX+15) ; vx
A204  20 02        JR nz,0xa208
A206  c6 0a        ADD A,10
A208  67           LD H,A
A209  dd 7e 05     LD A,(IX+5) ; y
A20C  c6 18        ADD A,0x18
A20E  6f           LD L,A
A20F  cd 6e 40     CALL 0x406e ; vec_4dc8
A212  30 04        JR nc,0xa218
A214  cd 04 6c     CALL 0x6c04
A217  d0           RET nc
A218  dd 7e 18     LD A,(IX+0x18)
A21B  ee 01        XOR 1
A21D  dd 77 18     LD (IX+0x18),A
A220  cd 24 b1     CALL 0xb124
A223  c3 07 9a     JP 0x9a07
A226  cd 58 a2     CALL 0xa258
A229  cd 4e a2     CALL 0xa24e
A22C  d0           RET nc
A22D  dd 7e 18     LD A,(IX+0x18)
A230  b7           OR A
A231  3a 07 e5     LD A,(0xe507) ; jugador_x
A234  28 09        JR z,0xa23f
A236  dd be 07     CP (IX+7) ; x
A239  11 c0 ff     LD DE,0xffc0
A23C  38 08        JR c,0xa246
A23E  c9           RET
A23F  dd be 07     CP (IX+7) ; x
A242  11 40 00     LD DE,0x40
A245  d8           RET c
A246  cd 3e b1     CALL 0xb13e
A249  c3 b8 7d     JP 0x7db8
A24C  c6 fd        ADD A,0xfd
A24E  be           CP (HL)
A24F  28 05        JR z,0xa256
A251  3c           INC A
A252  10 fa        DJNZ 0xa24e
A254  af           XOR A
A255  c9           RET
A256  37           SCF
A257  c9           RET
A258  dd 7e 16     LD A,(IX+0x16) ; ay_lo
A25B  06 11        LD B,0x11
A25D  21 05 e5     LD HL,0xe505 ; jugador_y
A260  c9           RET
A261  dd 36 18 01  LD (IX+0x18),1
A265  dd 7e 14     LD A,(IX+0x14)
A268  11 c0 ff     LD DE,0xffc0
A26B  18 0a        JR 0xa277
A26D  dd 36 18 00  LD (IX+0x18),0
A271  dd 7e 15     LD A,(IX+0x15)
A274  11 40 00     LD DE,0x40
A277  06 06        LD B,6
A279  21 07 e5     LD HL,0xe507 ; jugador_x
A27C  c9           RET
A27D  dd 7e 17     LD A,(IX+0x17) ; ay
A280  fe 07        CP 7
A282  c9           RET
B124  cd 2e b1     CALL 0xb12e
B127  18 0e        JR 0xb137
B12E  16 00        LD D,0
B130  5a           LD E,D
B131  18 0b        JR 0xb13e
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
