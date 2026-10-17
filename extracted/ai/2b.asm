; tipo 0x2b  vida 3  patron observado: vuela
; ---- inicio (b04:66B4[42]) ----
6BD5  dd 36 01 00  LD (IX+1),0 ; estado
6BD9  c9           RET
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
87DA  cd d5 6b     CALL 0x6bd5
87DD  dd 36 1c 11  LD (IX+0x1c),0x11
87E1  dd 36 1d 00  LD (IX+0x1d),0
87E5  dd 36 1f 01  LD (IX+0x1f),1
87E9  dd 36 0a c4  LD (IX+10),0xc4 ; d1
87ED  dd 36 08 c4  LD (IX+8),0xc4
87F1  dd 36 1e c8  LD (IX+0x1e),0xc8
87F5  21 f5 88     LD HL,0x88f5
87F8  c3 7f 6d     JP 0x6d7f
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
; ---- actualizacion (b06:B472[42]) ----
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
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
87FB  cd e4 88     CALL 0x88e4
87FE  cd 94 88     CALL 0x8894
8801  cd 3e 88     CALL 0x883e
8804  3e 00        LD A,0
8806  38 1d        JR c,0x8825
8808  dd cb 1b 46  BIT 0,(IX+0x1b)
880C  28 0c        JR z,0x881a
880E  dd 35 1c     DEC (IX+0x1c)
8811  c0           RET nz
8812  dd 36 1b 00  LD (IX+0x1b),0
8816  dd 36 1c 10  LD (IX+0x1c),0x10
881A  cd 65 88     CALL 0x8865
881D  3e 01        LD A,1
881F  d8           RET c
8820  dd cb 1d 46  BIT 0,(IX+0x1d)
8824  c0           RET nz
8825  dd 36 1d 00  LD (IX+0x1d),0
8829  b7           OR A
882A  dd 7e 18     LD A,(IX+0x18)
882D  28 03        JR z,0x8832
882F  dd 7e 19     LD A,(IX+0x19)
8832  dd 77 01     LD (IX+1),A ; estado
8835  21 f5 88     LD HL,0x88f5
8838  cd 7f 6d     CALL 0x6d7f
883B  c3 cb 88     JP 0x88cb
883E  cd 3b 6d     CALL 0x6d3b
8841  d8           RET c
8842  dd 7e 01     LD A,(IX+1) ; estado
8845  fe 04        CP 4
8847  3e 08        LD A,8
8849  30 0d        JR nc,0x8858
884B  cd 8c 88     CALL 0x888c
884E  dd 86 15     ADD A,(IX+0x15)
8851  4f           LD C,A
8852  dd 46 14     LD B,(IX+0x14)
8855  c3 49 6d     JP 0x6d49
8858  cd 8c 88     CALL 0x888c
885B  dd 86 14     ADD A,(IX+0x14)
885E  47           LD B,A
885F  dd 4e 15     LD C,(IX+0x15)
8862  c3 49 6d     JP 0x6d49
8865  cd 43 6d     CALL 0x6d43
8868  d8           RET c
8869  dd 7e 01     LD A,(IX+1) ; estado
886C  fe 04        CP 4
886E  3e 08        LD A,8
8870  30 0d        JR nc,0x887f
8872  cd 8c 88     CALL 0x888c
8875  dd 86 16     ADD A,(IX+0x16) ; ay_lo
8878  47           LD B,A
8879  dd 4e 17     LD C,(IX+0x17) ; ay
887C  c3 49 6d     JP 0x6d49
887F  cd 8c 88     CALL 0x888c
8882  dd 86 17     ADD A,(IX+0x17) ; ay
8885  4f           LD C,A
8886  dd 46 16     LD B,(IX+0x16) ; ay_lo
8889  c3 49 6d     JP 0x6d49
888C  dd cb 01 46  BIT 0,(IX+1) ; estado
8890  c8           RET z
8891  c6 07        ADD A,7
8893  c9           RET
8894  dd 7e 01     LD A,(IX+1) ; estado
8897  fe 04        CP 4
8899  30 11        JR nc,0x88ac
889B  cb 47        BIT 0,A
889D  dd 7e 07     LD A,(IX+7) ; x
88A0  20 05        JR nz,0x88a7
88A2  fe f0        CP 0xf0
88A4  d8           RET c
88A5  18 14        JR 0x88bb
88A7  fe 08        CP 8
88A9  d0           RET nc
88AA  18 0f        JR 0x88bb
88AC  cb 47        BIT 0,A
88AE  dd 7e 05     LD A,(IX+5) ; y
88B1  20 05        JR nz,0x88b8
88B3  fe a8        CP 0xa8
88B5  d8           RET c
88B6  18 03        JR 0x88bb
88B8  fe 20        CP 0x20
88BA  d0           RET nc
88BB  dd 36 1d 01  LD (IX+0x1d),1
88BF  dd 7e 18     LD A,(IX+0x18)
88C2  dd 77 01     LD (IX+1),A ; estado
88C5  21 f5 88     LD HL,0x88f5
88C8  cd 7f 6d     CALL 0x6d7f
88CB  dd 7e 0f     LD A,(IX+15) ; vx
88CE  b7           OR A
88CF  c8           RET z
88D0  cb 7f        BIT 7,A
88D2  dd 36 08 c4  LD (IX+8),0xc4
88D6  dd 36 1e c8  LD (IX+0x1e),0xc8
88DA  28 10        JR z,0x88ec
88DC  dd 36 08 cc  LD (IX+8),0xcc
88E0  dd 36 1e d0  LD (IX+0x1e),0xd0
88E4  dd 35 1f     DEC (IX+0x1f)
88E7  c0           RET nz
88E8  dd 36 1f 06  LD (IX+0x1f),6
88EC  dd 7e 0a     LD A,(IX+10) ; d1
88EF  dd be 08     CP (IX+8)
88F2  dd 7e 1e     LD A,(IX+0x1e)
88F5  28 03        JR z,0x88fa
88F7  dd 7e 08     LD A,(IX+8)
88FA  c3 83 70     JP 0x7083
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
