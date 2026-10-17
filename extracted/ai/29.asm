; tipo 0x29  vida None  patron observado: vuela
; ---- inicio (b04:66B4[40]) ----
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
92BA  3e 01        LD A,1
92BC  32 27 e6     LD (0xe627),A
92BF  cd f1 81     CALL 0x81f1
92C2  e6 7f        AND 0x7f
92C4  c6 40        ADD A,0x40
92C6  dd 77 07     LD (IX+7),A ; x
92C9  cd f1 81     CALL 0x81f1
92CC  e6 3f        AND 0x3f
92CE  c6 40        ADD A,0x40
92D0  dd 77 05     LD (IX+5),A ; y
92D3  cd f1 81     CALL 0x81f1
92D6  e6 0f        AND 15
92D8  dd 77 1f     LD (IX+0x1f),A
92DB  3e 01        LD A,1
92DD  dd 77 1e     LD (IX+0x1e),A
92E0  c3 cf 71     JP 0x71cf
; ---- actualizacion (b06:B472[40]) ----
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
92E3  cd 38 93     CALL 0x9338
92E6  dd 7e 1d     LD A,(IX+0x1d)
92E9  a7           AND A
92EA  28 19        JR z,0x9305
92EC  dd 7e 1e     LD A,(IX+0x1e)
92EF  a7           AND A
92F0  28 1c        JR z,0x930e
92F2  dd 7e 1f     LD A,(IX+0x1f)
92F5  06 01        LD B,1
92F7  cd 25 7e     CALL 0x7e25
92FA  af           XOR A
92FB  dd 77 1e     LD (IX+0x1e),A
92FE  dd 77 1d     LD (IX+0x1d),A
9301  dd 36 08 10  LD (IX+8),0x10
9305  dd 35 08     DEC (IX+8)
9308  20 04        JR nz,0x930e
930A  dd 36 1d 01  LD (IX+0x1d),1
930E  dd 7e 05     LD A,(IX+5) ; y
9311  dd cb 0d 7e  BIT 7,(IX+13) ; vy
9315  28 07        JR z,0x931e
9317  fe 20        CP 0x20
9319  dc 6f b1     CALL c,0xb16f
931C  18 05        JR 0x9323
931E  fe b0        CP 0xb0
9320  d4 6f b1     CALL nc,0xb16f
9323  dd 7e 07     LD A,(IX+7) ; x
9326  dd cb 0f 7e  BIT 7,(IX+15) ; vx
932A  28 06        JR z,0x9332
932C  fe 08        CP 8
932E  dc 5f b1     CALL c,0xb15f
9331  c9           RET
9332  fe e0        CP 0xe0
9334  d4 5f b1     CALL nc,0xb15f
9337  c9           RET
9338  3a 04 e6     LD A,(0xe604)
933B  e6 07        AND 7
933D  c0           RET nz
933E  16 0c        LD D,12
9340  06 66        LD B,0x66
9342  c3 dd b0     JP 0xb0dd
B0DD  dd 7e 0a     LD A,(IX+10) ; d1
B0E0  aa           XOR D
B0E1  dd 77 0a     LD (IX+10),A ; d1
B0E4  dd 7e 11     LD A,(IX+0x11) ; atributo_sprite
B0E7  a8           XOR B
B0E8  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B0EB  af           XOR A
B0EC  c9           RET
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
