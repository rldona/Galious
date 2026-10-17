; tipo 0x4a  (hijo, no aparece en salas)  descriptor 81 cc 0b ff 0a
; ---- inicio (b04:66B4[73]) ----
6C42  dd cb 03 f6  SET 6,(IX+3) ; flags2
6C46  c9           RET
80FB  cd 42 6c     CALL 0x6c42
80FE  11 00 fc     LD DE,0xfc00
8101  cd 37 b1     CALL 0xb137
8104  cd f1 81     CALL 0x81f1
8107  e6 03        AND 3
8109  21 41 81     LD HL,0x8141
810C  cd 62 40     CALL 0x4062 ; add_a_to_hl
810F  56           LD D,(HL)
8110  1e 00        LD E,0
8112  cd f1 81     CALL 0x81f1
8115  e6 01        AND 1
8117  28 04        JR z,0x811d
8119  7a           LD A,D
811A  ed 44        NEG
811C  57           LD D,A
811D  cd 3e b1     CALL 0xb13e
8120  cd f1 81     CALL 0x81f1
8123  e6 03        AND 3
8125  1f           RRA
8126  1f           RRA
8127  1f           RRA
8128  1f           RRA
8129  e6 0f        AND 15
812B  c6 40        ADD A,0x40
812D  dd 77 14     LD (IX+0x14),A
8130  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[73]) ----
8131  dd 7e 0c     LD A,(IX+12) ; vy_lo
8134  dd 56 0d     LD D,(IX+13) ; vy
8137  dd 86 14     ADD A,(IX+0x14)
813A  5f           LD E,A
813B  30 01        JR nc,0x813e
813D  14           INC D
813E  c3 37 b1     JP 0xb137
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
