; tipo 0x38  (hijo, no aparece en salas)  descriptor 81 c4 08 01 66
; ---- inicio (b04:66B4[55]) ----
A7A8  dd 36 1e a1  LD (IX+0x1e),0xa1
A7AC  dd 36 01 01  LD (IX+1),1 ; estado
A7B0  11 80 ff     LD DE,0xff80
A7B3  cd 45 b1     CALL 0xb145
A7B6  ed 5f        LD A,R
A7B8  cb 47        BIT 0,A
A7BA  11 00 01     LD DE,0x100
A7BD  c4 68 40     CALL nz,0x4068 ; neg_de
A7C0  c3 3e b1     JP 0xb13e
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B145  dd 73 16     LD (IX+0x16),E ; ay_lo
B148  dd 72 17     LD (IX+0x17),D ; ay
B14B  c9           RET
; ---- actualizacion (b06:B472[55]) ----
6B71  dd cb 02 a6  RES 4,(IX+2) ; flags
6B75  c9           RET
7087  6f           LD L,A
7088  dd 66 07     LD H,(IX+7) ; x
708B  cd 6e 40     CALL 0x406e ; vec_4dc8
708E  38 03        JR c,0x7093
7090  23           INC HL
7091  be           CP (HL)
7092  d0           RET nc
7093  09           ADD HL,BC
7094  be           CP (HL)
7095  3f           CCF
7096  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
A7B6  ed 5f        LD A,R
A7B8  cb 47        BIT 0,A
A7BA  11 00 01     LD DE,0x100
A7BD  c4 68 40     CALL nz,0x4068 ; neg_de
A7C0  c3 3e b1     JP 0xb13e
A7C3  dd 7e 01     LD A,(IX+1) ; estado
A7C6  3d           DEC A
A7C7  28 19        JR z,0xa7e2
A7C9  dd 35 08     DEC (IX+8)
A7CC  c0           RET nz
A7CD  dd cb 02 e6  SET 4,(IX+2) ; flags
A7D1  dd 36 0a c8  LD (IX+10),0xc8 ; d1
A7D5  dd 36 11 67  LD (IX+0x11),0x67 ; atributo_sprite
A7D9  11 00 04     LD DE,0x400
A7DC  cd 37 b1     CALL 0xb137
A7DF  c3 07 9a     JP 0x9a07
A7E2  cd f7 a7     CALL 0xa7f7
A7E5  d8           RET c
A7E6  dd 7e 0d     LD A,(IX+13) ; vy
A7E9  b7           OR A
A7EA  f2 4c b1     JP p,0xb14c
A7ED  fe fa        CP 0xfa
A7EF  da 4c b1     JP c,0xb14c
A7F2  dd 36 0d fa  LD (IX+13),0xfa ; vy
A7F6  c9           RET
A7F7  dd 7e 05     LD A,(IX+5) ; y
A7FA  dd be 1e     CP (IX+0x1e)
A7FD  d0           RET nc
A7FE  01 20 00     LD BC,0x20
A801  cd 87 70     CALL 0x7087
A804  d0           RET nc
A805  dd 7e 05     LD A,(IX+5) ; y
A808  e6 f8        AND 0xf8
A80A  c6 08        ADD A,8
A80C  dd 77 05     LD (IX+5),A ; y
A80F  d6 20        SUB 0x20
A811  dd 77 1e     LD (IX+0x1e),A
A814  cd b6 a7     CALL 0xa7b6
A817  cd 71 6b     CALL 0x6b71
A81A  dd 36 0a c4  LD (IX+10),0xc4 ; d1
A81E  dd 36 11 66  LD (IX+0x11),0x66 ; atributo_sprite
A822  dd 35 01     DEC (IX+1) ; estado
A825  dd 36 08 3c  LD (IX+8),0x3c
A829  3e 0b        LD A,11
A82B  cd 59 40     CALL 0x4059 ; sound_request
A82E  37           SCF
A82F  c9           RET
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B14C  dd 7e 0c     LD A,(IX+12) ; vy_lo
B14F  dd 86 16     ADD A,(IX+0x16) ; ay_lo
B152  dd 77 0c     LD (IX+12),A ; vy_lo
B155  dd 7e 0d     LD A,(IX+13) ; vy
B158  dd 8e 17     ADC A,(IX+0x17) ; ay
B15B  dd 77 0d     LD (IX+13),A ; vy
B15E  c9           RET
