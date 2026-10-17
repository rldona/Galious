; tipo 0x14  vida 4  patron observado: quieto
; ---- inicio (b04:66B4[19]) ----
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
7249  cd f5 96     CALL 0x96f5
724C  ed 5f        LD A,R
724E  e6 3f        AND 0x3f
7250  c6 80        ADD A,0x80
7252  c3 cf 71     JP 0x71cf
96F5  dd 7e 02     LD A,(IX+2) ; flags
96F8  e6 0f        AND 15
96FA  dd 77 02     LD (IX+2),A ; flags
96FD  c9           RET
; ---- actualizacion (b06:B472[19]) ----
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
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
71B8  dd 7e 0a     LD A,(IX+10) ; d1
71BB  b8           CP B
71BC  20 01        JR nz,0x71bf
71BE  41           LD B,C
71BF  dd 70 0a     LD (IX+10),B ; d1
71C2  c9           RET
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
7249  cd f5 96     CALL 0x96f5
724C  ed 5f        LD A,R
724E  e6 3f        AND 0x3f
7250  c6 80        ADD A,0x80
7252  c3 cf 71     JP 0x71cf
7255  21 76 b0     LD HL,0xb076
7258  e5           PUSH HL
7259  dd 7e 01     LD A,(IX+1) ; estado
725C  cd 50 40     CALL 0x4050 ; jump_table_a
725F  6b 72        .word 0x726B ; caso 0
7261  86 72        .word 0x7286 ; caso 1
7263  c7 72        .word 0x72C7 ; caso 2
7265  d4 72        .word 0x72D4 ; caso 3
7267  0d 73        .word 0x730D ; caso 4
7269  28 73        .word 0x7328 ; caso 5
726B  dd 35 08     DEC (IX+8)
726E  c0           RET nz
726F  dd cb 02 fe  SET 7,(IX+2) ; flags
7273  dd 36 0a a0  LD (IX+10),0xa0 ; d1
7277  dd 36 1f 00  LD (IX+0x1f),0
727B  dd 36 08 06  LD (IX+8),6
727F  3e 08        LD A,8
7281  cd 59 40     CALL 0x4059 ; sound_request
7284  18 4b        JR 0x72d1
7286  dd 35 08     DEC (IX+8)
7289  c0           RET nz
728A  dd 36 08 06  LD (IX+8),6
728E  dd 34 1f     INC (IX+0x1f)
7291  dd 7e 1f     LD A,(IX+0x1f)
7294  fe 03        CP 3
7296  30 0c        JR nc,0x72a4
7298  21 a1 72     LD HL,0x72a1
729B  cd 83 40     CALL 0x4083
729E  c3 83 70     JP 0x7083
72A4  dd 36 08 0a  LD (IX+8),10
72A8  cd 3f 73     CALL 0x733f
72AB  ed 5f        LD A,R
72AD  cb 47        BIT 0,A
72AF  3e ac        LD A,0xac
72B1  11 00 01     LD DE,0x100
72B4  28 05        JR z,0x72bb
72B6  3e b4        LD A,0xb4
72B8  11 00 ff     LD DE,0xff00
72BB  dd 77 0a     LD (IX+10),A ; d1
72BE  dd 36 0b 09  LD (IX+11),9 ; d2
72C2  cd 3e b1     CALL 0xb13e
72C5  18 0a        JR 0x72d1
72C7  dd 35 08     DEC (IX+8)
72CA  c0           RET nz
72CB  cd 4c 72     CALL 0x724c
72CE  cd 76 6b     CALL 0x6b76
72D1  c3 07 9a     JP 0x9a07
72D4  dd 35 08     DEC (IX+8)
72D7  28 2b        JR z,0x7304
72D9  cd ee 6b     CALL 0x6bee
72DC  30 05        JR nc,0x72e3
72DE  cd 04 6c     CALL 0x6c04
72E1  30 05        JR nc,0x72e8
72E3  cd 5f b1     CALL 0xb15f
72E6  18 09        JR 0x72f1
72E8  dd 34 1f     INC (IX+0x1f)
72EB  dd 7e 1f     LD A,(IX+0x1f)
72EE  fe 0a        CP 10
72F0  d8           RET c
72F1  dd 36 1f 00  LD (IX+0x1f),0
72F5  dd cb 0f 7e  BIT 7,(IX+15) ; vx
72F9  01 b0 ac     LD BC,0xacb0
72FC  28 03        JR z,0x7301
72FE  01 b8 b4     LD BC,0xb4b8
7301  c3 b8 71     JP 0x71b8
7304  cd 71 6b     CALL 0x6b71
7307  dd 36 08 0a  LD (IX+8),10
730B  18 c4        JR 0x72d1
730D  dd 35 08     DEC (IX+8)
7310  c0           RET nz
7311  dd 36 0a a8  LD (IX+10),0xa8 ; d1
7315  dd 36 08 06  LD (IX+8),6
7319  dd 36 0b 0e  LD (IX+11),14 ; d2
731D  dd 36 1f 02  LD (IX+0x1f),2
7321  3e 08        LD A,8
7323  cd 59 40     CALL 0x4059 ; sound_request
7326  18 a9        JR 0x72d1
7328  dd 35 08     DEC (IX+8)
732B  c0           RET nz
732C  dd 36 08 06  LD (IX+8),6
7330  dd 35 1f     DEC (IX+0x1f)
7333  dd 7e 1f     LD A,(IX+0x1f)
7336  f2 98 72     JP p,0x7298
7339  cd d5 6b     CALL 0x6bd5
733C  c3 49 72     JP 0x7249
733F  dd 7e 02     LD A,(IX+2) ; flags
7342  f6 e0        OR 0xe0
7344  c3 fa 96     JP 0x96fa
96F5  dd 7e 02     LD A,(IX+2) ; flags
96F8  e6 0f        AND 15
96FA  dd 77 02     LD (IX+2),A ; flags
96FD  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
