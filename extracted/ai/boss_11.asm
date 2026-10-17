; jefe 11 (E700), mundo 4, vida (7F97[11]): 112
; ---- inicio (b01:7F82[10]) ----
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
87E6  3e f0        LD A,0xf0
87E8  dd b6 02     OR (IX+2) ; flags
87EB  32 02 e7     LD (0xe702),A
87EE  dd 36 07 80  LD (IX+7),0x80 ; x
87F2  3e 70        LD A,0x70
87F4  32 05 e7     LD (0xe705),A
87F7  32 1b e7     LD (0xe71b),A
87FA  dd 36 0a 2b  LD (IX+10),0x2b ; d1
87FE  dd 36 18 08  LD (IX+0x18),8
8802  dd 36 24 40  LD (IX+0x24),0x40
8806  ed 5f        LD A,R
8808  e6 01        AND 1
880A  11 80 00     LD DE,0x80
880D  28 03        JR z,0x8812
880F  11 80 ff     LD DE,0xff80
8812  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
; ---- actualizacion (b01:7F40[10]) ----
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
7FAC  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FAF  cd 5f b1     CALL 0xb15f
7FB2  c3 15 42     JP 0x4215 ; map_a000_bank03
7FB5  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FB8  cd 37 b1     CALL 0xb137
7FBB  c3 15 42     JP 0x4215 ; map_a000_bank03
7FBE  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FC1  cd 45 b1     CALL 0xb145
7FC4  c3 15 42     JP 0x4215 ; map_a000_bank03
7FC7  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FCA  cd 4c b1     CALL 0xb14c
7FCD  c3 15 42     JP 0x4215 ; map_a000_bank03
87C6  3a 07 e7     LD A,(0xe707)
87C9  dd cb 0f 7e  BIT 7,(IX+15) ; vx
87CD  28 03        JR z,0x87d2
87CF  fe 18        CP 0x18
87D1  c9           RET
87D2  dd 86 24     ADD A,(IX+0x24)
87D5  fe e0        CP 0xe0
87D7  3f           CCF
87D8  c9           RET
8815  3a 01 e7     LD A,(0xe701)
8818  cd 40 41     CALL 0x4140 ; jump_table_a
881B  25 88        .word 0x8825 ; caso 0
881D  5f 88        .word 0x885F ; caso 1
881F  83 88        .word 0x8883 ; caso 2
8821  ba 88        .word 0x88BA ; caso 3
8823  0e 89        .word 0x890E ; caso 4
8825  cd 45 89     CALL 0x8945
8828  cd c6 87     CALL 0x87c6
882B  30 03        JR nc,0x8830
882D  cd ac 7f     CALL 0x7fac ; invierte vx (b06:B15F)
8830  21 07 e5     LD HL,0xe507 ; jugador_x
8833  dd cb 0f 7e  BIT 7,(IX+15) ; vx
8837  3a 07 e7     LD A,(0xe707)
883A  20 08        JR nz,0x8844
883C  c6 70        ADD A,0x70
883E  be           CP (HL)
883F  06 2d        LD B,0x2d
8841  d8           RET c
8842  18 06        JR 0x884a
8844  c6 e0        ADD A,0xe0
8846  be           CP (HL)
8847  06 2e        LD B,0x2e
8849  d0           RET nc
884A  cd e3 4d     CALL 0x4de3
884D  e6 03        AND 3
884F  c8           RET z
8850  78           LD A,B
8851  32 0a e7     LD (0xe70a),A
8854  32 1d e7     LD (0xe71d),A
8857  dd 36 18 08  LD (IX+0x18),8
885B  dd 34 01     INC (IX+1) ; estado
885E  c9           RET
885F  dd cb 02 a6  RES 4,(IX+2) ; flags
8863  dd 35 18     DEC (IX+0x18)
8866  c0           RET nz
8867  dd cb 0f 7e  BIT 7,(IX+15) ; vx
886B  3e 2f        LD A,0x2f
886D  28 02        JR z,0x8871
886F  3e 30        LD A,0x30
8871  32 0a e7     LD (0xe70a),A
8874  32 1c e7     LD (0xe71c),A
8877  dd 36 18 08  LD (IX+0x18),8
887B  dd cb 02 e6  SET 4,(IX+2) ; flags
887F  dd 34 01     INC (IX+1) ; estado
8882  c9           RET
8883  dd 35 18     DEC (IX+0x18)
8886  c0           RET nz
8887  dd 34 01     INC (IX+1) ; estado
888A  dd 36 08 08  LD (IX+8),8
888E  dd 36 19 00  LD (IX+0x19),0
8892  dd cb 0f 7e  BIT 7,(IX+15) ; vx
8896  11 80 01     LD DE,0x180
8899  28 03        JR z,0x889e
889B  11 80 fe     LD DE,0xfe80
889E  cd a3 7f     CALL 0x7fa3 ; vx=DE (b06:B13E)
88A1  11 40 00     LD DE,0x40
88A4  cd be 7f     CALL 0x7fbe ; ay=DE (b06:B145)
88A7  cd e3 4d     CALL 0x4de3
88AA  e6 03        AND 3
88AC  fe 01        CP 1
88AE  11 80 fb     LD DE,0xfb80
88B1  c2 b5 7f     JP nz,0x7fb5 ; vy=DE (b06:B137)
88B4  11 00 fa     LD DE,0xfa00
88B7  c3 b5 7f     JP 0x7fb5 ; vy=DE (b06:B137)
88BA  cd c6 87     CALL 0x87c6
88BD  30 06        JR nc,0x88c5
88BF  11 00 00     LD DE,0
88C2  cd a3 7f     CALL 0x7fa3 ; vx=DE (b06:B13E)
88C5  cd c7 7f     CALL 0x7fc7 ; vy+=ay (b06:B14C)
88C8  dd 34 19     INC (IX+0x19)
88CB  3a 19 e7     LD A,(0xe719)
88CE  fe 30        CP 0x30
88D0  30 09        JR nc,0x88db
88D2  fe 20        CP 0x20
88D4  30 05        JR nc,0x88db
88D6  dd 36 0a 2b  LD (IX+10),0x2b ; d1
88DA  c9           RET
88DB  3a 1c e7     LD A,(0xe71c)
88DE  ee 1f        XOR 0x1f
88E0  32 0a e7     LD (0xe70a),A
88E3  3a 1b e7     LD A,(0xe71b)
88E6  dd be 05     CP (IX+5) ; y
88E9  d0           RET nc
88EA  dd 36 05 70  LD (IX+5),0x70 ; y
88EE  dd 36 08 20  LD (IX+8),0x20
88F2  3a 1d e7     LD A,(0xe71d)
88F5  ee 03        XOR 3
88F7  32 0a e7     LD (0xe70a),A
88FA  dd 34 01     INC (IX+1) ; estado
88FD  11 00 00     LD DE,0
8900  cd a3 7f     CALL 0x7fa3 ; vx=DE (b06:B13E)
8903  11 00 00     LD DE,0
8906  cd b5 7f     CALL 0x7fb5 ; vy=DE (b06:B137)
8909  3e 1a        LD A,0x1a
890B  c3 59 40     JP 0x4059 ; sound_request
890E  dd 35 08     DEC (IX+8)
8911  c0           RET nz
8912  dd 34 1e     INC (IX+0x1e)
8915  3a 1e e7     LD A,(0xe71e)
8918  fe 04        CP 4
891A  20 0c        JR nz,0x8928
891C  dd 36 1e 00  LD (IX+0x1e),0
8920  ed 5f        LD A,R
8922  e6 01        AND 1
8924  d6 01        SUB 1
8926  18 06        JR 0x892e
8928  3a 07 e5     LD A,(0xe507) ; jugador_x
892B  dd be 07     CP (IX+7) ; x
892E  11 80 ff     LD DE,0xff80
8931  38 03        JR c,0x8936
8933  11 80 00     LD DE,0x80
8936  dd 36 01 00  LD (IX+1),0 ; estado
893A  dd 36 08 01  LD (IX+8),1
893E  dd 36 18 08  LD (IX+0x18),8
8942  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
8945  dd 35 18     DEC (IX+0x18)
8948  c0           RET nz
8949  dd 36 18 08  LD (IX+0x18),8
894D  3a 0a e7     LD A,(0xe70a)
8950  fe 2b        CP 0x2b
8952  3e 2c        LD A,0x2c
8954  28 02        JR z,0x8958
8956  3e 2b        LD A,0x2b
8958  32 0a e7     LD (0xe70a),A
895B  c9           RET
