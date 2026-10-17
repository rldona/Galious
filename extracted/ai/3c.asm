; tipo 0x3c  vida 7  patron observado: quieto
; ---- inicio (b04:66B4[59]) ----
AE4C  dd 36 16 20  LD (IX+0x16),0x20 ; ay_lo
AE50  dd 36 17 06  LD (IX+0x17),6 ; ay
AE54  dd 36 08 40  LD (IX+8),0x40
AE58  c9           RET
; ---- actualizacion (b06:B472[59]) ----
6BD5  dd 36 01 00  LD (IX+1),0 ; estado
6BD9  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
96AB  cd f1 81     CALL 0x81f1
96AE  e6 03        AND 3
96B0  47           LD B,A
96B1  cd f1 81     CALL 0x81f1
96B4  e6 01        AND 1
96B6  80           ADD A,B
96B7  21 20 97     LD HL,0x9720
96BA  cd 83 40     CALL 0x4083
96BD  f5           PUSH AF
96BE  cd f1 81     CALL 0x81f1
96C1  f6 10        OR 0x10
96C3  e6 f0        AND 0xf0
96C5  67           LD H,A
96C6  d6 20        SUB 0x20
96C8  fe b0        CP 0xb0
96CA  30 f2        JR nc,0x96be
96CC  dd 74 1e     LD (IX+0x1e),H
96CF  f1           POP AF
96D0  dd 77 1f     LD (IX+0x1f),A
96D3  c6 10        ADD A,0x10
96D5  6f           LD L,A
96D6  cd 6e 40     CALL 0x406e ; vec_4dc8
96D9  d0           RET nc
96DA  23           INC HL
96DB  be           CP (HL)
96DC  d0           RET nc
96DD  cd fe 96     CALL 0x96fe
96E0  d0           RET nc
96E1  dd 7e 1e     LD A,(IX+0x1e)
96E4  dd 77 07     LD (IX+7),A ; x
96E7  dd 7e 1f     LD A,(IX+0x1f)
96EA  dd 77 05     LD (IX+5),A ; y
96ED  c9           RET
96EE  3e f0        LD A,0xf0
96F0  dd b6 02     OR (IX+2) ; flags
96F3  18 05        JR 0x96fa
96F5  dd 7e 02     LD A,(IX+2) ; flags
96F8  e6 0f        AND 15
96FA  dd 77 02     LD (IX+2),A ; flags
96FD  c9           RET
96FE  dd 66 1e     LD H,(IX+0x1e)
9701  dd 6e 1f     LD L,(IX+0x1f)
9704  18 06        JR 0x970c
970C  cd 6e 40     CALL 0x406e ; vec_4dc8
970F  3f           CCF
9710  d0           RET nc
9711  23           INC HL
9712  be           CP (HL)
9713  3f           CCF
9714  d0           RET nc
9715  01 1f 00     LD BC,0x1f
9718  09           ADD HL,BC
9719  be           CP (HL)
971A  3f           CCF
971B  d0           RET nc
971C  23           INC HL
971D  be           CP (HL)
971E  3f           CCF
971F  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
9ACD  cd d1 9a     CALL 0x9ad1
9AD0  d0           RET nc
9AD1  dd cb 1d 46  BIT 0,(IX+0x1d)
9AD5  28 03        JR z,0x9ada
9AD7  23           INC HL
9AD8  18 01        JR 0x9adb
9ADA  2b           DEC HL
9ADB  7e           LD A,(HL)
9ADC  fe ba        CP 0xba
9ADE  c9           RET
AE59  dd 7e 01     LD A,(IX+1) ; estado
AE5C  cd 50 40     CALL 0x4050 ; jump_table_a
AE5F  65 ae        .word 0xAE65 ; caso 0
AE61  74 ae        .word 0xAE74 ; caso 1
AE63  8c ae        .word 0xAE8C ; caso 2
AE65  cd 9a        .word 0x9ACD ; caso 3
AE71  c3 07 9a     JP 0x9a07
AE74  dd cb 02 7e  BIT 7,(IX+2) ; flags
AE78  28 05        JR z,0xae7f
AE7A  cd f5 96     CALL 0x96f5
AE7D  18 03        JR 0xae82
AE7F  cd ee 96     CALL 0x96ee
AE82  dd 35 08     DEC (IX+8)
AE85  20 13        JR nz,0xae9a
AE87  cd f5 96     CALL 0x96f5
AE8A  18 e5        JR 0xae71
AE8C  cd ab 96     CALL 0x96ab
AE8F  d0           RET nc
AE90  cd ee 96     CALL 0x96ee
AE93  dd 36 08 40  LD (IX+8),0x40
AE97  c3 d5 6b     JP 0x6bd5
AE9A  dd 35 17     DEC (IX+0x17) ; ay
AE9D  c0           RET nz
AE9E  dd 36 17 06  LD (IX+0x17),6 ; ay
AEA2  16 04        LD D,4
AEA4  06 00        LD B,0
AEA6  c3 dd b0     JP 0xb0dd
B0DD  dd 7e 0a     LD A,(IX+10) ; d1
B0E0  aa           XOR D
B0E1  dd 77 0a     LD (IX+10),A ; d1
B0E4  dd 7e 11     LD A,(IX+0x11) ; atributo_sprite
B0E7  a8           XOR B
B0E8  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B0EB  af           XOR A
B0EC  c9           RET
