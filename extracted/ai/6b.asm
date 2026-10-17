; tipo 0x6b  (hijo, no aparece en salas)  descriptor 00 17 00 00 00
; ---- inicio (b04:66B4[106]) ----
AAB5  dd 7e 02     LD A,(IX+2) ; flags
AAB8  e6 cf        AND 0xcf
AABA  dd 77 02     LD (IX+2),A ; flags
AABD  c9           RET
; ---- actualizacion (b06:B472[106]) ----
AABD  c9           RET
