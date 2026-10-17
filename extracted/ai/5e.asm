; tipo 0x5e  (hijo, no aparece en salas)  descriptor 40 64 07 00 00
; ---- inicio (b04:66B4[93]) ----
6AAA  dd 36 02 c0  LD (IX+2),0xc0 ; flags
6AAE  dd 6e 05     LD L,(IX+5) ; y
6AB1  dd 66 07     LD H,(IX+7) ; x
6AB4  cd 56 40     CALL 0x4056 ; vec_4da1
6AB7  dd 75 12     LD (IX+0x12),L
6ABA  dd 74 13     LD (IX+0x13),H
6ABD  3a 24 e6     LD A,(0xe624)
6AC0  dd 77 18     LD (IX+0x18),A
6AC3  c9           RET
; ---- actualizacion (b06:B472[93]) ----
6AC3  c9           RET
