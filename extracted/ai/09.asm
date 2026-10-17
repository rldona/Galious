; tipo 0x09  (hijo, no aparece en salas)  descriptor 81 c4 0e fe 13
; ---- inicio (b04:66B4[8]) ----
700D  11 80 00     LD DE,0x80
7010  cd 45 b1     CALL 0xb145
7013  ed 5f        LD A,R
7015  cb 47        BIT 0,A
7017  11 00 01     LD DE,0x100
701A  c4 68 40     CALL nz,0x4068 ; neg_de
701D  c3 3e b1     JP 0xb13e
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B145  dd 73 16     LD (IX+0x16),E ; ay_lo
B148  dd 72 17     LD (IX+0x17),D ; ay
B14B  c9           RET
; ---- actualizacion (b06:B472[8]) ----
7013  ed 5f        LD A,R
7015  cb 47        BIT 0,A
7017  11 00 01     LD DE,0x100
701A  c4 68 40     CALL nz,0x4068 ; neg_de
701D  c3 3e b1     JP 0xb13e
7020  cd 62 70     CALL 0x7062
7023  cd 3e 70     CALL 0x703e
7026  30 05        JR nc,0x702d
7028  3e 09        LD A,9
702A  c3 59 40     JP 0x4059 ; sound_request
702D  dd 7e 0d     LD A,(IX+13) ; vy
7030  b7           OR A
7031  fa 4c b1     JP m,0xb14c
7034  fe 06        CP 6
7036  da 4c b1     JP c,0xb14c
7039  dd 36 0d 06  LD (IX+13),6 ; vy
703D  c9           RET
703E  dd 7e 05     LD A,(IX+5) ; y
7041  dd be 08     CP (IX+8)
7044  3f           CCF
7045  d0           RET nc
7046  c6 10        ADD A,0x10
7048  01 e0 ff     LD BC,0xffe0
704B  cd 87 70     CALL 0x7087
704E  d0           RET nc
704F  dd 7e 05     LD A,(IX+5) ; y
7052  c6 10        ADD A,0x10
7054  dd 77 08     LD (IX+8),A
7057  11 00 fc     LD DE,0xfc00
705A  cd 37 b1     CALL 0xb137
705D  cd 13 70     CALL 0x7013
7060  37           SCF
7061  c9           RET
7062  3a 04 e6     LD A,(0xe604)
7065  e6 03        AND 3
7067  c0           RET nz
7068  dd 7e 0a     LD A,(IX+10) ; d1
706B  dd cb 0f 7e  BIT 7,(IX+15) ; vx
706F  28 0a        JR z,0x707b
7071  c6 04        ADD A,4
7073  fe d4        CP 0xd4
7075  20 0c        JR nz,0x7083
7077  3e c4        LD A,0xc4
7079  18 08        JR 0x7083
707B  d6 04        SUB 4
707D  fe c0        CP 0xc0
707F  20 02        JR nz,0x7083
7081  3e d0        LD A,0xd0
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
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
