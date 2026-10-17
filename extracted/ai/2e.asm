; tipo 0x2e  vida 2  patron observado: quieto
; ---- inicio (b04:66B4[45]) ----
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
96F5  dd 7e 02     LD A,(IX+2) ; flags
96F8  e6 0f        AND 15
96FA  dd 77 02     LD (IX+2),A ; flags
96FD  c9           RET
A070  cd f1 81     CALL 0x81f1
A073  dd 77 08     LD (IX+8),A
A076  af           XOR A
A077  dd 77 01     LD (IX+1),A ; estado
A07A  3d           DEC A
A07B  dd 77 1e     LD (IX+0x1e),A
A07E  c3 f5 96     JP 0x96f5
; ---- actualizacion (b06:B472[45]) ----
733F  dd 7e 02     LD A,(IX+2) ; flags
7342  f6 e0        OR 0xe0
7344  c3 fa 96     JP 0x96fa
736F  dd 73 0a     LD (IX+10),E ; d1
7372  dd 72 11     LD (IX+0x11),D ; atributo_sprite
7375  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
96F5  dd 7e 02     LD A,(IX+2) ; flags
96F8  e6 0f        AND 15
96FA  dd 77 02     LD (IX+2),A ; flags
96FD  c9           RET
A070  cd f1 81     CALL 0x81f1
A073  dd 77 08     LD (IX+8),A
A076  af           XOR A
A077  dd 77 01     LD (IX+1),A ; estado
A07A  3d           DEC A
A07B  dd 77 1e     LD (IX+0x1e),A
A07E  c3 f5 96     JP 0x96f5
A081  dd 7e 01     LD A,(IX+1) ; estado
A084  3d           DEC A
A085  28 09        JR z,0xa090
A087  dd 35 08     DEC (IX+8)
A08A  c0           RET nz
A08B  dd 34 01     INC (IX+1) ; estado
A08E  18 04        JR 0xa094
A090  dd 35 08     DEC (IX+8)
A093  c0           RET nz
A094  cd 3f 73     CALL 0x733f
A097  dd 34 1e     INC (IX+0x1e)
A09A  dd 7e 1e     LD A,(IX+0x1e)
A09D  a7           AND A
A09E  28 02        JR z,0xa0a2
A0A0  d6 08        SUB 8
A0A2  d4 c6 a0     CALL nc,0xa0c6
A0A5  3d           DEC A
A0A6  28 c8        JR z,0xa070
A0A8  dd 7e 1e     LD A,(IX+0x1e)
A0AB  0e 10        LD C,0x10
A0AD  a7           AND A
A0AE  28 0a        JR z,0xa0ba
A0B0  fe 04        CP 4
A0B2  28 06        JR z,0xa0ba
A0B4  fe 08        CP 8
A0B6  28 02        JR z,0xa0ba
A0B8  0e 04        LD C,4
A0BA  21 d1 a0     LD HL,0xa0d1
A0BD  cd 53 40     CALL 0x4053 ; read_word_table
A0C0  dd 71 08     LD (IX+8),C
A0C3  c3 6f 73     JP 0x736f
A0C6  08           EX AF,AF'
A0C7  dd 7e 02     LD A,(IX+2) ; flags
A0CA  e6 8f        AND 0x8f
A0CC  dd 77 02     LD (IX+2),A ; flags
A0CF  08           EX AF,AF'
A0D0  c9           RET
