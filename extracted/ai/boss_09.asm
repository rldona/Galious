; jefe 9 (E700), mundo 10, vida (7F97[9]): 248
; ---- inicio (b01:7F82[8]) ----
8CDD  dd 36 17 30  LD (IX+0x17),0x30 ; ay
8CE1  dd 36 34 01  LD (IX+0x34),1
8CE5  dd 36 08 fa  LD (IX+8),0xfa
8CE9  dd 36 0a 37  LD (IX+10),0x37 ; d1
8CED  dd 36 33 01  LD (IX+0x33),1
8CF1  dd 36 07 60  LD (IX+7),0x60 ; x
8CF5  dd 36 05 40  LD (IX+5),0x40 ; y
8CF9  dd 36 1d 74  LD (IX+0x1d),0x74
8CFD  dd 36 1e 58  LD (IX+0x1e),0x58
8D01  dd 36 19 08  LD (IX+0x19),8
8D05  dd 7e 18     LD A,(IX+0x18)
8D08  21 d7 8e     LD HL,0x8ed7
8D0B  cd 83 40     CALL 0x4083
8D0E  dd 77 1a     LD (IX+0x1a),A
8D11  1e 60        LD E,0x60
8D13  16 40        LD D,0x40
8D15  06 00        LD B,0
8D17  3e 67        LD A,0x67
8D19  cd 7d 40     CALL 0x407d
8D1C  dd 34 18     INC (IX+0x18)
8D1F  dd 35 19     DEC (IX+0x19)
8D22  20 e1        JR nz,0x8d05
8D24  3e e0        LD A,0xe0
8D26  dd b6 02     OR (IX+2) ; flags
8D29  dd 77 02     LD (IX+2),A ; flags
8D2C  c9           RET
; ---- actualizacion (b01:7F40[8]) ----
8D24  3e e0        LD A,0xe0
8D26  dd b6 02     OR (IX+2) ; flags
8D29  dd 77 02     LD (IX+2),A ; flags
8D2C  c9           RET
8D2D  dd 7e 01     LD A,(IX+1) ; estado
8D30  cd 40 41     CALL 0x4140 ; jump_table_a
8D33  3d 8d        .word 0x8D3D ; caso 0
8D35  65 8d        .word 0x8D65 ; caso 1
8D37  8b 8d        .word 0x8D8B ; caso 2
8D39  c1 8d        .word 0x8DC1 ; caso 3
8D3B  f7 8d        .word 0x8DF7 ; caso 4
8D3D  dd 36 1b 00  LD (IX+0x1b),0
8D41  cd 6d 8e     CALL 0x8e6d
8D44  28 16        JR z,0x8d5c
8D46  cd 97 8e     CALL 0x8e97
8D49  cd 82 8e     CALL 0x8e82
8D4C  20 0a        JR nz,0x8d58
8D4E  dd 35 17     DEC (IX+0x17) ; ay
8D51  20 09        JR nz,0x8d5c
8D53  dd 36 01 04  LD (IX+1),4 ; estado
8D57  c9           RET
8D58  cd 3e 8e     CALL 0x8e3e
8D5B  c0           RET nz
8D5C  dd 35 08     DEC (IX+8)
8D5F  c0           RET nz
8D60  dd 36 01 01  LD (IX+1),1 ; estado
8D64  c9           RET
8D65  cd 6d 8e     CALL 0x8e6d
8D68  28 0c        JR z,0x8d76
8D6A  cd 97 8e     CALL 0x8e97
8D6D  cd 82 8e     CALL 0x8e82
8D70  28 04        JR z,0x8d76
8D72  cd 3e 8e     CALL 0x8e3e
8D75  c0           RET nz
8D76  dd 36 08 3f  LD (IX+8),0x3f
8D7A  dd 36 01 02  LD (IX+1),2 ; estado
8D7E  dd 36 1b 01  LD (IX+0x1b),1
8D82  3e 0f        LD A,15
8D84  dd a6 02     AND (IX+2) ; flags
8D87  dd 77 02     LD (IX+2),A ; flags
8D8A  c9           RET
8D8B  dd 35 08     DEC (IX+8)
8D8E  c0           RET nz
8D8F  cd e3 4d     CALL 0x4de3
8D92  e6 07        AND 7
8D94  21 df 8e     LD HL,0x8edf
8D97  cd 4c 41     CALL 0x414c ; read_word_table
8D9A  dd 73 07     LD (IX+7),E ; x
8D9D  dd 72 05     LD (IX+5),D ; y
8DA0  7b           LD A,E
8DA1  c6 14        ADD A,0x14
8DA3  dd 77 1d     LD (IX+0x1d),A
8DA6  7a           LD A,D
8DA7  c6 18        ADD A,0x18
8DA9  dd 77 1e     LD (IX+0x1e),A
8DAC  dd 36 1b 02  LD (IX+0x1b),2
8DB0  dd 36 08 18  LD (IX+8),0x18
8DB4  dd 36 01 03  LD (IX+1),3 ; estado
8DB8  dd 36 18 03  LD (IX+0x18),3
8DBC  dd 36 19 00  LD (IX+0x19),0
8DC0  c9           RET
8DC1  dd 35 18     DEC (IX+0x18)
8DC4  c0           RET nz
8DC5  dd 36 18 03  LD (IX+0x18),3
8DC9  dd cb 19 46  BIT 0,(IX+0x19)
8DCD  28 0a        JR z,0x8dd9
8DCF  3e a0        LD A,0xa0
8DD1  dd b6 02     OR (IX+2) ; flags
8DD4  dd 77 02     LD (IX+2),A ; flags
8DD7  18 03        JR 0x8ddc
8DD9  cd 82 8d     CALL 0x8d82
8DDC  dd 7e 19     LD A,(IX+0x19)
8DDF  ee 01        XOR 1
8DE1  dd 77 19     LD (IX+0x19),A
8DE4  dd 35 08     DEC (IX+8)
8DE7  c0           RET nz
8DE8  dd 36 17 30  LD (IX+0x17),0x30 ; ay
8DEC  dd 36 08 fa  LD (IX+8),0xfa
8DF0  dd 36 01 00  LD (IX+1),0 ; estado
8DF4  c3 24 8d     JP 0x8d24
8DF7  dd 36 17 30  LD (IX+0x17),0x30 ; ay
8DFB  cd e3 4d     CALL 0x4de3
8DFE  e6 01        AND 1
8E00  3e 80        LD A,0x80
8E02  20 01        JR nz,0x8e05
8E04  af           XOR A
8E05  dd 77 24     LD (IX+0x24),A
8E08  dd 77 25     LD (IX+0x25),A
8E0B  3e 04        LD A,4
8E0D  dd 77 22     LD (IX+0x22),A
8E10  dd 77 1b     LD (IX+0x1b),A
8E13  cd e3 4d     CALL 0x4de3
8E16  e6 0f        AND 15
8E18  dd 77 20     LD (IX+0x20),A
8E1B  af           XOR A
8E1C  dd 77 01     LD (IX+1),A ; estado
8E1F  dd 77 32     LD (IX+0x32),A
8E22  dd 36 0a 38  LD (IX+10),0x38 ; d1
8E26  dd 36 34 20  LD (IX+0x34),0x20
8E2A  dd 36 23 01  LD (IX+0x23),1
8E2E  dd e5        PUSH IX
8E30  e1           POP HL
8E31  3e 26        LD A,0x26
8E33  cd 36 41     CALL 0x4136 ; add_a_to_hl
8E36  06 0c        LD B,12
8E38  af           XOR A
8E39  77           LD (HL),A
8E3A  2c           INC L
8E3B  10 fc        DJNZ 0x8e39
8E3D  c9           RET
8E3E  dd 7e 25     LD A,(IX+0x25)
8E41  21 00 e8     LD HL,0xe800 ; enemy_slots
8E44  cd 36 41     CALL 0x4136 ; add_a_to_hl
8E47  06 04        LD B,4
8E49  7e           LD A,(HL)
8E4A  b7           OR A
8E4B  cc 63 8e     CALL z,0x8e63
8E4E  e5           PUSH HL
8E4F  3e 1f        LD A,0x1f
8E51  cd 36 41     CALL 0x4136 ; add_a_to_hl
8E54  cb 46        BIT 0,(HL)
8E56  e1           POP HL
8E57  c0           RET nz
8E58  3e 20        LD A,0x20
8E5A  cd 36 41     CALL 0x4136 ; add_a_to_hl
8E5D  10 ea        DJNZ 0x8e49
8E5F  dd 70 23     LD (IX+0x23),B
8E62  c9           RET
8E63  e5           PUSH HL
8E64  3e 1f        LD A,0x1f
8E66  cd 36 41     CALL 0x4136 ; add_a_to_hl
8E69  36 00        LD (HL),0
8E6B  e1           POP HL
8E6C  c9           RET
8E6D  af           XOR A
8E6E  06 08        LD B,8
8E70  21 00 e8     LD HL,0xe800 ; enemy_slots
8E73  cd 36 41     CALL 0x4136 ; add_a_to_hl
8E76  7e           LD A,(HL)
8E77  b7           OR A
8E78  c0           RET nz
8E79  3e 20        LD A,0x20
8E7B  10 f6        DJNZ 0x8e73
8E7D  dd 36 0a 37  LD (IX+10),0x37 ; d1
8E81  c9           RET
8E82  dd 35 34     DEC (IX+0x34)
8E85  c0           RET nz
8E86  dd 34 34     INC (IX+0x34)
8E89  dd 36 0a 37  LD (IX+10),0x37 ; d1
8E8D  dd cb 23 46  BIT 0,(IX+0x23)
8E91  c0           RET nz
8E92  dd cb 1f 46  BIT 0,(IX+0x1f)
8E96  c9           RET
8E97  dd 34 33     INC (IX+0x33)
8E9A  dd 7e 33     LD A,(IX+0x33)
8E9D  e6 0f        AND 15
8E9F  fe 02        CP 2
8EA1  30 02        JR nc,0x8ea5
8EA3  3e 02        LD A,2
8EA5  dd 77 33     LD (IX+0x33),A
8EA8  dd 7e 23     LD A,(IX+0x23)
8EAB  b7           OR A
8EAC  c0           RET nz
8EAD  3a 07 e5     LD A,(0xe507) ; jugador_x
8EB0  dd 96 1d     SUB (IX+0x1d)
8EB3  30 01        JR nc,0x8eb6
8EB5  2f           CPL
8EB6  fe 1e        CP 0x1e
8EB8  30 18        JR nc,0x8ed2
8EBA  3a 05 e5     LD A,(0xe505) ; jugador_y
8EBD  d6 10        SUB 0x10
8EBF  dd 96 1e     SUB (IX+0x1e)
8EC2  30 01        JR nc,0x8ec5
8EC4  2f           CPL
8EC5  fe 1e        CP 0x1e
8EC7  30 09        JR nc,0x8ed2
8EC9  dd 36 1c 90  LD (IX+0x1c),0x90
8ECD  dd 36 1f 01  LD (IX+0x1f),1
8ED1  c9           RET
8ED2  dd 36 1c 70  LD (IX+0x1c),0x70
8ED6  c9           RET
