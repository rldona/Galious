; tipo 0x2a  vida 3  patron observado: quieto
; ---- inicio (b04:66B4[41]) ----
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
969A  dd 36 08 10  LD (IX+8),0x10
969E  dd cb 02 7e  BIT 7,(IX+2) ; flags
96A2  c4 f5 96     CALL nz,0x96f5
96A5  cd ab 96     CALL 0x96ab
96A8  d0           RET nc
96A9  18 43        JR 0x96ee
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
; ---- actualizacion (b06:B472[41]) ----
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
9696  dd 35 08     DEC (IX+8)
9699  c0           RET nz
969A  dd 36 08 10  LD (IX+8),0x10
969E  dd cb 02 7e  BIT 7,(IX+2) ; flags
96A2  c4 f5 96     CALL nz,0x96f5
96A5  cd ab 96     CALL 0x96ab
96A8  d0           RET nc
96A9  18 43        JR 0x96ee
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
