; tipo 0x28  vida 1  patron observado: vuela
; ---- inicio (b04:66B4[39]) ----
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
9357  cd f1 81     CALL 0x81f1
935A  e6 3f        AND 0x3f
935C  c6 07        ADD A,7
935E  dd 77 15     LD (IX+0x15),A
9361  cd f1 81     CALL 0x81f1
9364  e6 0f        AND 15
9366  dd 77 18     LD (IX+0x18),A
9369  06 03        LD B,3
936B  cd 25 7e     CALL 0x7e25
936E  dd 7e 18     LD A,(IX+0x18)
9371  21 9c 93     LD HL,0x939c
9374  cd 83 40     CALL 0x4083
9377  dd 77 0a     LD (IX+10),A ; d1
937A  21 cc 93     LD HL,0x93cc
937D  c3 88 b0     JP 0xb088
B088  dd 7e 0a     LD A,(IX+10) ; d1
B08B  d6 c4        SUB 0xc4
B08D  18 05        JR 0xb094
B094  1f           RRA
B095  1f           RRA
B096  e6 3f        AND 0x3f
B098  cd 83 40     CALL 0x4083
B09B  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B09E  c9           RET
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[39]) ----
7DDD  dd 7e 05     LD A,(IX+5) ; y
7DE0  dd cb 0d 7e  BIT 7,(IX+13) ; vy
7DE4  20 06        JR nz,0x7dec
7DE6  fe b0        CP 0xb0
7DE8  38 11        JR c,0x7dfb
7DEA  18 04        JR 0x7df0
7DEC  fe 28        CP 0x28
7DEE  30 0b        JR nc,0x7dfb
7DF0  dd 36 15 10  LD (IX+0x15),0x10
7DF4  dd 36 1d 01  LD (IX+0x1d),1
7DF8  cd 6f b1     CALL 0xb16f
7DFB  dd 7e 07     LD A,(IX+7) ; x
7DFE  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7E02  20 05        JR nz,0x7e09
7E04  fe e8        CP 0xe8
7E06  d8           RET c
7E07  18 03        JR 0x7e0c
7E09  fe 08        CP 8
7E0B  d0           RET nc
7E0C  dd 36 15 10  LD (IX+0x15),0x10
7E10  dd 36 1c 01  LD (IX+0x1c),1
7E14  c3 5f b1     JP 0xb15f
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
9345  af           XOR A
9346  dd 77 1c     LD (IX+0x1c),A
9349  dd 77 1d     LD (IX+0x1d),A
934C  cd dd 7d     CALL 0x7ddd
934F  cd 80 93     CALL 0x9380
9352  dd 35 15     DEC (IX+0x15)
9355  20 17        JR nz,0x936e
9357  cd f1 81     CALL 0x81f1
935A  e6 3f        AND 0x3f
935C  c6 07        ADD A,7
935E  dd 77 15     LD (IX+0x15),A
9361  cd f1 81     CALL 0x81f1
9364  e6 0f        AND 15
9366  dd 77 18     LD (IX+0x18),A
9369  06 03        LD B,3
936B  cd 25 7e     CALL 0x7e25
936E  dd 7e 18     LD A,(IX+0x18)
9371  21 9c 93     LD HL,0x939c
9374  cd 83 40     CALL 0x4083
9377  dd 77 0a     LD (IX+10),A ; d1
937A  21 cc 93     LD HL,0x93cc
937D  c3 88 b0     JP 0xb088
9380  dd cb 1c 46  BIT 0,(IX+0x1c)
9384  21 ac 93     LD HL,0x93ac
9387  c4 92 93     CALL nz,0x9392
938A  dd cb 1d 46  BIT 0,(IX+0x1d)
938E  c8           RET z
938F  21 bc 93     LD HL,0x93bc
9392  dd 7e 18     LD A,(IX+0x18)
9395  cd 83 40     CALL 0x4083
9398  dd 77 18     LD (IX+0x18),A
939B  c9           RET
B088  dd 7e 0a     LD A,(IX+10) ; d1
B08B  d6 c4        SUB 0xc4
B08D  18 05        JR 0xb094
B094  1f           RRA
B095  1f           RRA
B096  e6 3f        AND 0x3f
B098  cd 83 40     CALL 0x4083
B09B  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B09E  c9           RET
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
B16F  dd 5e 0c     LD E,(IX+12) ; vy_lo
B172  dd 56 0d     LD D,(IX+13) ; vy
B175  cd 68 40     CALL 0x4068 ; neg_de
B178  dd 73 0c     LD (IX+12),E ; vy_lo
B17B  dd 72 0d     LD (IX+13),D ; vy
B17E  c9           RET
