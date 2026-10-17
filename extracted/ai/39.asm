; tipo 0x39  (hijo, no aparece en salas)  descriptor 81 c4 0f 28 4c
; ---- inicio (b04:66B4[56]) ----
6B71  dd cb 02 a6  RES 4,(IX+2) ; flags
6B75  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
A8DF  cd 71 6b     CALL 0x6b71
A8E2  dd 7e 05     LD A,(IX+5) ; y
A8E5  dd 77 15     LD (IX+0x15),A
A8E8  af           XOR A
A8E9  dd 77 16     LD (IX+0x16),A ; ay_lo
A8EC  dd 77 17     LD (IX+0x17),A ; ay
A8EF  dd 36 18 08  LD (IX+0x18),8
A8F3  dd 36 1b 80  LD (IX+0x1b),0x80
A8F7  cd f1 81     CALL 0x81f1
A8FA  e6 01        AND 1
A8FC  dd 77 1e     LD (IX+0x1e),A
A8FF  dd 46 07     LD B,(IX+7) ; x
A902  3e 0a        LD A,10
A904  28 02        JR z,0xa908
A906  ed 44        NEG
A908  80           ADD A,B
A909  dd 77 14     LD (IX+0x14),A
A90C  c9           RET
; ---- actualizacion (b06:B472[56]) ----
A90D  dd 6e 16     LD L,(IX+0x16) ; ay_lo
A910  dd 66 17     LD H,(IX+0x17) ; ay
A913  06 00        LD B,0
A915  dd 4e 1b     LD C,(IX+0x1b)
A918  09           ADD HL,BC
A919  dd 75 16     LD (IX+0x16),L ; ay_lo
A91C  dd 74 17     LD (IX+0x17),H ; ay
A91F  cd 2f a9     CALL 0xa92f
A922  dd 7e 1c     LD A,(IX+0x1c)
A925  dd 77 05     LD (IX+5),A ; y
A928  dd 7e 1d     LD A,(IX+0x1d)
A92B  dd 77 07     LD (IX+7),A ; x
A92E  c9           RET
A92F  dd 7e 18     LD A,(IX+0x18)
A932  dd 86 19     ADD A,(IX+0x19)
A935  dd 77 19     LD (IX+0x19),A
A938  cd 58 a9     CALL 0xa958
A93B  dd 86 15     ADD A,(IX+0x15)
A93E  dd 77 1c     LD (IX+0x1c),A
A941  dd cb 1e 46  BIT 0,(IX+0x1e)
A945  3e 40        LD A,0x40
A947  28 02        JR z,0xa94b
A949  3e c0        LD A,0xc0
A94B  dd 86 19     ADD A,(IX+0x19)
A94E  cd 58 a9     CALL 0xa958
A951  dd 86 14     ADD A,(IX+0x14)
A954  dd 77 1d     LD (IX+0x1d),A
A957  c9           RET
A958  4f           LD C,A
A959  cb 77        BIT 6,A
A95B  28 01        JR z,0xa95e
A95D  2f           CPL
A95E  e6 3f        AND 0x3f
A960  21 74 b3     LD HL,0xb374
A963  cd 62 40     CALL 0x4062 ; add_a_to_hl
A966  5e           LD E,(HL)
A967  dd 66 17     LD H,(IX+0x17) ; ay
A96A  cd 6b 40     CALL 0x406b ; vec_4dcf
A96D  7c           LD A,H
A96E  cb 79        BIT 7,C
A970  c8           RET z
A971  ed 44        NEG
A973  c9           RET
