; tipo 0x48  (hijo, no aparece en salas)  descriptor 81 d8 0b ff 4a
; ---- inicio (b04:66B4[71]) ----
6C42  dd cb 03 f6  SET 6,(IX+3) ; flags2
6C46  c9           RET
; ---- actualizacion (b06:B472[71]) ----
7E25  f5           PUSH AF
7E26  21 49 7e     LD HL,0x7e49
7E29  cd 53 40     CALL 0x4053 ; read_word_table
7E2C  21 00 00     LD HL,0
7E2F  c5           PUSH BC
7E30  19           ADD HL,DE
7E31  10 fd        DJNZ 0x7e30
7E33  eb           EX DE,HL
7E34  cd 3e b1     CALL 0xb13e
7E37  c1           POP BC
7E38  f1           POP AF
7E39  21 69 7e     LD HL,0x7e69
7E3C  cd 53 40     CALL 0x4053 ; read_word_table
7E3F  21 00 00     LD HL,0
7E42  19           ADD HL,DE
7E43  10 fd        DJNZ 0x7e42
7E45  eb           EX DE,HL
7E46  c3 37 b1     JP 0xb137
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
87BA  dd 7e 01     LD A,(IX+1) ; estado
87BD  3d           DEC A
87BE  28 12        JR z,0x87d2
87C0  cd f1 81     CALL 0x81f1
87C3  e6 0f        AND 15
87C5  06 01        LD B,1
87C7  cd 25 7e     CALL 0x7e25
87CA  dd 36 14 10  LD (IX+0x14),0x10
87CE  dd 34 01     INC (IX+1) ; estado
87D1  c9           RET
87D2  dd 35 14     DEC (IX+0x14)
87D5  c0           RET nz
87D6  dd 35 01     DEC (IX+1) ; estado
87D9  c9           RET
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
