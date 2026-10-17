; tipo 0x5c  (hijo, no aparece en salas)  descriptor 00 39 00 05 00
; ---- inicio (b04:66B4[91]) ----
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
907F  dd cb 02 b6  RES 6,(IX+2) ; flags
9083  c9           RET
AABE  cd 7f 90     CALL 0x907f
AAC1  3a 31 f0     LD A,(0xf031)
AAC4  b7           OR A
AAC5  c0           RET nz
AAC6  3a 30 f0     LD A,(0xf030)
AAC9  b7           OR A
AACA  20 14        JR nz,0xaae0
AACC  cd f1 81     CALL 0x81f1
AACF  e6 03        AND 3
AAD1  47           LD B,A
AAD2  cd f1 81     CALL 0x81f1
AAD5  e6 07        AND 7
AAD7  80           ADD A,B
AAD8  32 33 f0     LD (0xf033),A
AADB  3e 01        LD A,1
AADD  32 30 f0     LD (0xf030),A
AAE0  21 32 f0     LD HL,0xf032
AAE3  3a 33 f0     LD A,(0xf033)
AAE6  be           CP (HL)
AAE7  28 02        JR z,0xaaeb
AAE9  34           INC (HL)
AAEA  c9           RET
AAEB  dd 34 1f     INC (IX+0x1f)
AAEE  3e 01        LD A,1
AAF0  32 31 f0     LD (0xf031),A
AAF3  c9           RET
; ---- actualizacion (b06:B472[91]) ----
AAF3  c9           RET
