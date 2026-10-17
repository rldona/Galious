; tipo 0x4d  (hijo, no aparece en salas)  descriptor 81 c0 01 ff 54
; ---- inicio (b04:66B4[76]) ----
6C42  dd cb 03 f6  SET 6,(IX+3) ; flags2
6C46  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
8FF7  cd 42 6c     CALL 0x6c42
8FFA  dd 36 0b 01  LD (IX+11),1 ; d2
8FFE  cd f1 81     CALL 0x81f1
9001  e6 0f        AND 15
9003  dd 77 15     LD (IX+0x15),A
9006  dd 36 18 04  LD (IX+0x18),4
900A  c9           RET
; ---- actualizacion (b06:B472[76]) ----
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
900B  cd 4f 90     CALL 0x904f
900E  dd 7e 01     LD A,(IX+1) ; estado
9011  3d           DEC A
9012  28 33        JR z,0x9047
9014  dd 7e 14     LD A,(IX+0x14)
9017  fe 04        CP 4
9019  20 04        JR nz,0x901f
901B  dd 36 16 ff  LD (IX+0x16),0xff ; ay_lo
901F  a7           AND A
9020  20 04        JR nz,0x9026
9022  dd 36 16 01  LD (IX+0x16),1 ; ay_lo
9026  dd 7e 15     LD A,(IX+0x15)
9029  21 64 90     LD HL,0x9064
902C  cd 62 40     CALL 0x4062 ; add_a_to_hl
902F  dd 7e 14     LD A,(IX+0x14)
9032  dd 86 16     ADD A,(IX+0x16) ; ay_lo
9035  dd 77 14     LD (IX+0x14),A
9038  cd 83 40     CALL 0x4083
903B  06 01        LD B,1
903D  cd 25 7e     CALL 0x7e25
9040  dd 36 17 08  LD (IX+0x17),8 ; ay
9044  c3 07 9a     JP 0x9a07
9047  dd 35 17     DEC (IX+0x17) ; ay
904A  c0           RET nz
904B  dd 35 01     DEC (IX+1) ; estado
904E  c9           RET
904F  dd 35 18     DEC (IX+0x18)
9052  c0           RET nz
9053  dd 36 18 04  LD (IX+0x18),4
9057  dd 7e 0a     LD A,(IX+10) ; d1
905A  c6 04        ADD A,4
905C  e6 0f        AND 15
905E  f6 c0        OR 0xc0
9060  dd 77 0a     LD (IX+10),A ; d1
9063  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
