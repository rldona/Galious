; tipo 0x02  vida 1  patron observado: vuela
; ---- inicio (b04:66B4[1]) ----
6CDB  21 be 6d     LD HL,0x6dbe
6CDE  cd 7f 6d     CALL 0x6d7f
6CE1  dd 36 1c 11  LD (IX+0x1c),0x11
6CE5  dd 36 08 02  LD (IX+8),2
6CE9  cd f1 81     CALL 0x81f1
6CEC  e6 2f        AND 0x2f
6CEE  c6 50        ADD A,0x50
6CF0  dd 77 1d     LD (IX+0x1d),A
6CF3  c9           RET
6D7F  dd 36 1b 01  LD (IX+0x1b),1
6D83  06 07        LD B,7
6D85  dd e5        PUSH IX
6D87  d1           POP DE
6D88  7b           LD A,E
6D89  c6 13        ADD A,0x13
6D8B  5f           LD E,A
6D8C  13           INC DE
6D8D  3e 08        LD A,8
6D8F  cd 62 40     CALL 0x4062 ; add_a_to_hl
6D92  e5           PUSH HL
6D93  dd 7e 01     LD A,(IX+1) ; estado
6D96  cd 83 40     CALL 0x4083
6D99  12           LD (DE),A
6D9A  e1           POP HL
6D9B  10 ef        DJNZ 0x6d8c
6D9D  cd 24 b1     CALL 0xb124
6DA0  dd 7e 01     LD A,(IX+1) ; estado
6DA3  fe 04        CP 4
6DA5  30 0b        JR nc,0x6db2
6DA7  cb 47        BIT 0,A
6DA9  11 00 01     LD DE,0x100
6DAC  c4 68 40     CALL nz,0x4068 ; neg_de
6DAF  c3 3e b1     JP 0xb13e
6DB2  cb 47        BIT 0,A
6DB4  11 00 01     LD DE,0x100
6DB7  c4 68 40     CALL nz,0x4068 ; neg_de
6DBA  c3 37 b1     JP 0xb137
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
B124  cd 2e b1     CALL 0xb12e
B127  18 0e        JR 0xb137
B12E  16 00        LD D,0
B130  5a           LD E,D
B131  18 0b        JR 0xb13e
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[1]) ----
6C15  dd 7e 07     LD A,(IX+7) ; x
6C18  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6C1C  28 03        JR z,0x6c21
6C1E  fe 08        CP 8
6C20  c9           RET
6C21  fe f0        CP 0xf0
6C23  3f           CCF
6C24  c9           RET
6CE9  cd f1 81     CALL 0x81f1
6CEC  e6 2f        AND 0x2f
6CEE  c6 50        ADD A,0x50
6CF0  dd 77 1d     LD (IX+0x1d),A
6CF3  c9           RET
6CF4  cd bd 6d     CALL 0x6dbd
6CF7  cd 5a 6d     CALL 0x6d5a
6CFA  dd cb 1b 46  BIT 0,(IX+0x1b)
6CFE  20 0a        JR nz,0x6d0a
6D00  dd 35 1d     DEC (IX+0x1d)
6D03  20 05        JR nz,0x6d0a
6D05  cd e9 6c     CALL 0x6ce9
6D08  18 6c        JR 0x6d76
6D0A  cd 3b 6d     CALL 0x6d3b
6D0D  3e 00        LD A,0
6D0F  38 18        JR c,0x6d29
6D11  dd cb 1b 46  BIT 0,(IX+0x1b)
6D15  28 0c        JR z,0x6d23
6D17  dd 35 1c     DEC (IX+0x1c)
6D1A  c0           RET nz
6D1B  dd 36 1b 00  LD (IX+0x1b),0
6D1F  dd 36 1c 11  LD (IX+0x1c),0x11
6D23  cd 43 6d     CALL 0x6d43
6D26  3e 01        LD A,1
6D28  d8           RET c
6D29  b7           OR A
6D2A  dd 7e 18     LD A,(IX+0x18)
6D2D  28 03        JR z,0x6d32
6D2F  dd 7e 19     LD A,(IX+0x19)
6D32  dd 77 01     LD (IX+1),A ; estado
6D35  21 be 6d     LD HL,0x6dbe
6D38  c3 7f 6d     JP 0x6d7f
6D3B  dd 46 14     LD B,(IX+0x14)
6D3E  dd 4e 15     LD C,(IX+0x15)
6D41  18 06        JR 0x6d49
6D43  dd 46 16     LD B,(IX+0x16) ; ay_lo
6D46  dd 4e 17     LD C,(IX+0x17) ; ay
6D49  dd 7e 07     LD A,(IX+7) ; x
6D4C  80           ADD A,B
6D4D  67           LD H,A
6D4E  dd 7e 05     LD A,(IX+5) ; y
6D51  81           ADD A,C
6D52  6f           LD L,A
6D53  cd 56 40     CALL 0x4056 ; vec_4da1
6D56  3e b9        LD A,0xb9
6D58  be           CP (HL)
6D59  c9           RET
6D5A  dd 7e 01     LD A,(IX+1) ; estado
6D5D  fe 04        CP 4
6D5F  30 06        JR nc,0x6d67
6D61  cd 15 6c     CALL 0x6c15
6D64  d0           RET nc
6D65  18 0f        JR 0x6d76
6D67  cb 47        BIT 0,A
6D69  dd 7e 05     LD A,(IX+5) ; y
6D6C  20 05        JR nz,0x6d73
6D6E  fe b0        CP 0xb0
6D70  d8           RET c
6D71  18 03        JR 0x6d76
6D73  fe 18        CP 0x18
6D75  d0           RET nc
6D76  dd 7e 1a     LD A,(IX+0x1a)
6D79  dd 77 01     LD (IX+1),A ; estado
6D7C  21 be 6d     LD HL,0x6dbe
6D7F  dd 36 1b 01  LD (IX+0x1b),1
6D83  06 07        LD B,7
6D85  dd e5        PUSH IX
6D87  d1           POP DE
6D88  7b           LD A,E
6D89  c6 13        ADD A,0x13
6D8B  5f           LD E,A
6D8C  13           INC DE
6D8D  3e 08        LD A,8
6D8F  cd 62 40     CALL 0x4062 ; add_a_to_hl
6D92  e5           PUSH HL
6D93  dd 7e 01     LD A,(IX+1) ; estado
6D96  cd 83 40     CALL 0x4083
6D99  12           LD (DE),A
6D9A  e1           POP HL
6D9B  10 ef        DJNZ 0x6d8c
6D9D  cd 24 b1     CALL 0xb124
6DA0  dd 7e 01     LD A,(IX+1) ; estado
6DA3  fe 04        CP 4
6DA5  30 0b        JR nc,0x6db2
6DA7  cb 47        BIT 0,A
6DA9  11 00 01     LD DE,0x100
6DAC  c4 68 40     CALL nz,0x4068 ; neg_de
6DAF  c3 3e b1     JP 0xb13e
6DB2  cb 47        BIT 0,A
6DB4  11 00 01     LD DE,0x100
6DB7  c4 68 40     CALL nz,0x4068 ; neg_de
6DBA  c3 37 b1     JP 0xb137
6DBD  16 04        LD D,4
6DBF  1e 02        LD E,2
6DC1  06 00        LD B,0
6DC3  c3 d6 b0     JP 0xb0d6
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
B0D6  dd 35 08     DEC (IX+8)
B0D9  c0           RET nz
B0DA  dd 73 08     LD (IX+8),E
B0DD  dd 7e 0a     LD A,(IX+10) ; d1
B0E0  aa           XOR D
B0E1  dd 77 0a     LD (IX+10),A ; d1
B0E4  dd 7e 11     LD A,(IX+0x11) ; atributo_sprite
B0E7  a8           XOR B
B0E8  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B0EB  af           XOR A
B0EC  c9           RET
B124  cd 2e b1     CALL 0xb12e
B127  18 0e        JR 0xb137
B12E  16 00        LD D,0
B130  5a           LD E,D
B131  18 0b        JR 0xb13e
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
