; jefe 2 (E700), mundo 3, vida (7F97[2]): 64
; ---- inicio (b01:7F82[1]) ----
64CD  3a 10 ec     LD A,(0xec10)
64D0  dd be 00     CP (IX) ; tipo
64D3  c0           RET nz
64D4  3a 2d e5     LD A,(0xe52d)
64D7  dd be 00     CP (IX) ; tipo
64DA  20 07        JR nz,0x64e3
64DC  af           XOR A
64DD  32 2b e5     LD (0xe52b),A
64E0  32 2d e5     LD (0xe52d),A
64E3  3a 12 ec     LD A,(0xec12)
64E6  cb 47        BIT 0,A
64E8  c8           RET z
64E9  dd cb 02 d6  SET 2,(IX+2) ; flags
64ED  c9           RET
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
8279  dd 36 01 05  LD (IX+1),5 ; estado
827D  3a 19 e7     LD A,(0xe719)
8280  1f           RRA
8281  1f           RRA
8282  e6 03        AND 3
8284  cd 40 41     CALL 0x4140 ; jump_table_a
8287  8f 82        .word 0x828F ; caso 0
8289  9b 82        .word 0x829B ; caso 1
828B  c6 82        .word 0x82C6 ; caso 2
828D  db 82        .word 0x82DB ; caso 3
828F  cd 64        .word 0x64CD ; caso 4
829B  cd 64 83     CALL 0x8364
829E  0d           DEC C
829F  ed 5f        LD A,R
82A1  e6 07        AND 7
82A3  21 49 83     LD HL,0x8349
82A6  5f           LD E,A
82A7  16 00        LD D,0
82A9  19           ADD HL,DE
82AA  7e           LD A,(HL)
82AB  81           ADD A,C
82AC  28 0b        JR z,0x82b9
82AE  fa b9 82     JP m,0x82b9
82B1  fe 05        CP 5
82B3  38 05        JR c,0x82ba
82B5  3e 04        LD A,4
82B7  18 01        JR 0x82ba
82B9  3c           INC A
82BA  47           LD B,A
82BB  3a 07 e5     LD A,(0xe507) ; jugador_x
82BE  fe 80        CP 0x80
82C0  3f           CCF
82C1  cb 10        RL B
82C3  78           LD A,B
82C4  18 19        JR 0x82df
82C6  3a 05 e5     LD A,(0xe505) ; jugador_y
82C9  0e 00        LD C,0
82CB  fe 70        CP 0x70
82CD  38 02        JR c,0x82d1
82CF  0e 04        LD C,4
82D1  3a 07 e5     LD A,(0xe507) ; jugador_x
82D4  fe 80        CP 0x80
82D6  cb 11        RL C
82D8  79           LD A,C
82D9  18 04        JR 0x82df
82DB  ed 5f        LD A,R
82DD  e6 0f        AND 15
82DF  32 1d e7     LD (0xe71d),A
82E2  dd cb 09 ae  RES 5,(IX+9) ; d0
82E6  dd cb 1d 46  BIT 0,(IX+0x1d)
82EA  01 06 f8     LD BC,0xf806
82ED  11 00 fe     LD DE,0xfe00
82F0  20 13        JR nz,0x8305
82F2  01 08 00     LD BC,8
82F5  11 00 02     LD DE,0x200
82F8  dd cb 09 ee  SET 5,(IX+9) ; d0
82FC  dd 36 0b 01  LD (IX+11),1 ; d2
8300  cd 75 87     CALL 0x8775
8303  18 04        JR 0x8309
8305  dd 36 15 01  LD (IX+0x15),1
8309  dd 71 0a     LD (IX+10),C ; d1
830C  dd 70 07     LD (IX+7),B ; x
830F  cd a3 7f     CALL 0x7fa3 ; vx=DE (b06:B13E)
8312  3a 1d e7     LD A,(0xe71d)
8315  1f           RRA
8316  21 41 83     LD HL,0x8341
8319  cd 55 41     CALL 0x4155 ; read_byte_table
831C  32 05 e7     LD (0xe705),A
831F  3e 01        LD A,1
8321  32 1b e6     LD (0xe61b),A
8324  c9           RET
8364  0e 00        LD C,0
8366  3a 05 e5     LD A,(0xe505) ; jugador_y
8369  fe 40        CP 0x40
836B  d8           RET c
836C  0c           INC C
836D  fe 60        CP 0x60
836F  d8           RET c
8370  0c           INC C
8371  fe 80        CP 0x80
8373  d8           RET c
8374  0c           INC C
8375  fe a0        CP 0xa0
8377  d8           RET c
8378  0c           INC C
8379  c9           RET
8775  dd cb 02 a6  RES 4,(IX+2) ; flags
8779  c9           RET
; ---- actualizacion (b01:7F40[1]) ----
61CD  c0           RET nz
61CE  c5           PUSH BC
61CF  3a 01 e5     LD A,(0xe501)
61D2  b7           OR A
61D3  c4 37 b3     CALL nz,0xb337
61D6  c1           POP BC
61D7  c9           RET
64CD  3a 10 ec     LD A,(0xec10)
64D0  dd be 00     CP (IX) ; tipo
64D3  c0           RET nz
64D4  3a 2d e5     LD A,(0xe52d)
64D7  dd be 00     CP (IX) ; tipo
64DA  20 07        JR nz,0x64e3
64DC  af           XOR A
64DD  32 2b e5     LD (0xe52b),A
64E0  32 2d e5     LD (0xe52d),A
64E3  3a 12 ec     LD A,(0xec12)
64E6  cb 47        BIT 0,A
64E8  c8           RET z
64E9  dd cb 02 d6  SET 2,(IX+2) ; flags
64ED  c9           RET
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
7FAC  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FAF  cd 5f b1     CALL 0xb15f
7FB2  c3 15 42     JP 0x4215 ; map_a000_bank03
8161  3a 04 e6     LD A,(0xe604)
8164  e6 07        AND 7
8166  c0           RET nz
8167  3a 0a e7     LD A,(0xe70a)
816A  ee 01        XOR 1
816C  32 0a e7     LD (0xe70a),A
816F  c9           RET
8170  3a 01 e7     LD A,(0xe701)
8173  cd 40 41     CALL 0x4140 ; jump_table_a
8176  82 81        .word 0x8182 ; caso 0
8178  01 82        .word 0x8201 ; caso 1
817A  2f 82        .word 0x822F ; caso 2
817C  4b 82        .word 0x824B ; caso 3
817E  76 82        .word 0x8276 ; caso 4
8180  25 83        .word 0x8325 ; caso 5
8182  3a 05 e5     LD A,(0xe505) ; jugador_y
8185  dd 96 05     SUB (IX+5) ; y
8188  fe 19        CP 0x19
818A  30 40        JR nc,0x81cc
818C  dd cb 09 6e  BIT 5,(IX+9) ; d0
8190  28 0b        JR z,0x819d
8192  cd 7a 83     CALL 0x837a
8195  47           LD B,A
8196  3a 07 e5     LD A,(0xe507) ; jugador_x
8199  d6 08        SUB 8
819B  18 0b        JR 0x81a8
819D  3a 07 e5     LD A,(0xe507) ; jugador_x
81A0  d6 08        SUB 8
81A2  47           LD B,A
81A3  3a 07 e7     LD A,(0xe707)
81A6  e6 f8        AND 0xf8
81A8  90           SUB B
81A9  fe 68        CP 0x68
81AB  30 1f        JR nc,0x81cc
81AD  3a 0b e7     LD A,(0xe70b)
81B0  dd cb 09 6e  BIT 5,(IX+9) ; d0
81B4  20 03        JR nz,0x81b9
81B6  3a 15 e7     LD A,(0xe715)
81B9  fe 0c        CP 12
81BB  38 0f        JR c,0x81cc
81BD  ed 5f        LD A,R
81BF  e6 03        AND 3
81C1  cd 40 41     CALL 0x4140 ; jump_table_a
81C4  cc 81        .word 0x81CC ; caso 0
81C6  e9 81        .word 0x81E9 ; caso 1
81C8  44 82        .word 0x8244 ; caso 2
81CA  e9 81        .word 0x81E9 ; caso 3
81CC  cd 61        .word 0x61CD ; caso 4
81E9  dd 34 01     INC (IX+1) ; estado
81EC  dd 36 08 1e  LD (IX+8),0x1e
81F0  cd 75 87     CALL 0x8775
81F3  06 0a        LD B,10
81F5  dd cb 09 6e  BIT 5,(IX+9) ; d0
81F9  28 02        JR z,0x81fd
81FB  06 0b        LD B,11
81FD  dd 70 0a     LD (IX+10),B ; d1
8200  c9           RET
8201  dd 35 08     DEC (IX+8)
8204  c0           RET nz
8205  dd 34 01     INC (IX+1) ; estado
8208  dd 36 08 60  LD (IX+8),0x60
820C  06 00        LD B,0
820E  dd cb 09 6e  BIT 5,(IX+9) ; d0
8212  28 07        JR z,0x821b
8214  cd 7a 83     CALL 0x837a
8217  c6 08        ADD A,8
8219  18 08        JR 0x8223
821B  04           INC B
821C  3a 07 e7     LD A,(0xe707)
821F  e6 f8        AND 0xf8
8221  d6 10        SUB 0x10
8223  5f           LD E,A
8224  3a 05 e7     LD A,(0xe705)
8227  c6 08        ADD A,8
8229  57           LD D,A
822A  3e 57        LD A,0x57
822C  c3 7d 40     JP 0x407d
822F  dd 35 08     DEC (IX+8)
8232  c0           RET nz
8233  06 08        LD B,8
8235  dd cb 09 6e  BIT 5,(IX+9) ; d0
8239  20 06        JR nz,0x8241
823B  06 06        LD B,6
823D  dd cb 02 e6  SET 4,(IX+2) ; flags
8241  dd 70 0a     LD (IX+10),B ; d1
8244  dd 36 01 03  LD (IX+1),3 ; estado
8248  c3 ac 7f     JP 0x7fac ; invierte vx (b06:B15F)
824B  cd 61 81     CALL 0x8161
824E  dd cb 09 6e  BIT 5,(IX+9) ; d0
8252  28 09        JR z,0x825d
8254  3a 04 e6     LD A,(0xe604)
8257  e6 03        AND 3
8259  c0           RET nz
825A  dd 35 0b     DEC (IX+11) ; d2
825D  cd 51 83     CALL 0x8351
8260  d0           RET nc
8261  dd cb 09 6e  BIT 5,(IX+9) ; d0
8265  28 04        JR z,0x826b
8267  dd 36 0b 01  LD (IX+11),1 ; d2
826B  dd 36 08 1e  LD (IX+8),0x1e
826F  dd 36 02 00  LD (IX+2),0 ; flags
8273  c3 da 84     JP 0x84da
8276  dd 34 19     INC (IX+0x19)
8279  dd 36 01 05  LD (IX+1),5 ; estado
827D  3a 19 e7     LD A,(0xe719)
8280  1f           RRA
8281  1f           RRA
8282  e6 03        AND 3
8284  cd 40 41     CALL 0x4140 ; jump_table_a
8287  8f 82        .word 0x828F ; caso 0
8289  9b 82        .word 0x829B ; caso 1
828B  c6 82        .word 0x82C6 ; caso 2
828D  db 82        .word 0x82DB ; caso 3
828F  cd 64        .word 0x64CD ; caso 4
829B  cd 64 83     CALL 0x8364
829E  0d           DEC C
829F  ed 5f        LD A,R
82A1  e6 07        AND 7
82A3  21 49 83     LD HL,0x8349
82A6  5f           LD E,A
82A7  16 00        LD D,0
82A9  19           ADD HL,DE
82AA  7e           LD A,(HL)
82AB  81           ADD A,C
82AC  28 0b        JR z,0x82b9
82AE  fa b9 82     JP m,0x82b9
82B1  fe 05        CP 5
82B3  38 05        JR c,0x82ba
82B5  3e 04        LD A,4
82B7  18 01        JR 0x82ba
82B9  3c           INC A
82BA  47           LD B,A
82BB  3a 07 e5     LD A,(0xe507) ; jugador_x
82BE  fe 80        CP 0x80
82C0  3f           CCF
82C1  cb 10        RL B
82C3  78           LD A,B
82C4  18 19        JR 0x82df
82C6  3a 05 e5     LD A,(0xe505) ; jugador_y
82C9  0e 00        LD C,0
82CB  fe 70        CP 0x70
82CD  38 02        JR c,0x82d1
82CF  0e 04        LD C,4
82D1  3a 07 e5     LD A,(0xe507) ; jugador_x
82D4  fe 80        CP 0x80
82D6  cb 11        RL C
82D8  79           LD A,C
82D9  18 04        JR 0x82df
82DB  ed 5f        LD A,R
82DD  e6 0f        AND 15
82DF  32 1d e7     LD (0xe71d),A
82E2  dd cb 09 ae  RES 5,(IX+9) ; d0
82E6  dd cb 1d 46  BIT 0,(IX+0x1d)
82EA  01 06 f8     LD BC,0xf806
82ED  11 00 fe     LD DE,0xfe00
82F0  20 13        JR nz,0x8305
82F2  01 08 00     LD BC,8
82F5  11 00 02     LD DE,0x200
82F8  dd cb 09 ee  SET 5,(IX+9) ; d0
82FC  dd 36 0b 01  LD (IX+11),1 ; d2
8300  cd 75 87     CALL 0x8775
8303  18 04        JR 0x8309
8305  dd 36 15 01  LD (IX+0x15),1
8309  dd 71 0a     LD (IX+10),C ; d1
830C  dd 70 07     LD (IX+7),B ; x
830F  cd a3 7f     CALL 0x7fa3 ; vx=DE (b06:B13E)
8312  3a 1d e7     LD A,(0xe71d)
8315  1f           RRA
8316  21 41 83     LD HL,0x8341
8319  cd 55 41     CALL 0x4155 ; read_byte_table
831C  32 05 e7     LD (0xe705),A
831F  3e 01        LD A,1
8321  32 1b e6     LD (0xe61b),A
8324  c9           RET
8325  dd e5        PUSH IX
8327  cd 7b 58     CALL 0x587b
832A  dd e1        POP IX
832C  3a 1b e6     LD A,(0xe61b)
832F  a7           AND A
8330  c0           RET nz
8331  dd cb 1d 46  BIT 0,(IX+0x1d)
8335  3e f0        LD A,0xf0
8337  20 02        JR nz,0x833b
8339  3e e0        LD A,0xe0
833B  32 02 e7     LD (0xe702),A
833E  c3 f8 86     JP 0x86f8
8351  dd cb 09 6e  BIT 5,(IX+9) ; d0
8355  20 07        JR nz,0x835e
8357  3a 07 e7     LD A,(0xe707)
835A  fe f8        CP 0xf8
835C  3f           CCF
835D  c9           RET
835E  3a 0b e7     LD A,(0xe70b)
8361  fe 02        CP 2
8363  c9           RET
8364  0e 00        LD C,0
8366  3a 05 e5     LD A,(0xe505) ; jugador_y
8369  fe 40        CP 0x40
836B  d8           RET c
836C  0c           INC C
836D  fe 60        CP 0x60
836F  d8           RET c
8370  0c           INC C
8371  fe 80        CP 0x80
8373  d8           RET c
8374  0c           INC C
8375  fe a0        CP 0xa0
8377  d8           RET c
8378  0c           INC C
8379  c9           RET
837A  3a 0b e7     LD A,(0xe70b)
837D  87           ADD A,A
837E  87           ADD A,A
837F  87           ADD A,A
8380  dd 86 07     ADD A,(IX+7) ; x
8383  c9           RET
84DA  dd 34 01     INC (IX+1) ; estado
84DD  c9           RET
86F8  dd 36 01 00  LD (IX+1),0 ; estado
86FC  c9           RET
8775  dd cb 02 a6  RES 4,(IX+2) ; flags
8779  c9           RET
A3E3  06 00        LD B,0
A3E5  3a 03 e5     LD A,(0xe503)
A3E8  a7           AND A
A3E9  28 02        JR z,0xa3ed
A3EB  04           INC B
A3EC  04           INC B
A3ED  c3 11 a9     JP 0xa911
A911  78           LD A,B
A912  32 02 e5     LD (0xe502),A
A915  c9           RET
B337  3a 01 e5     LD A,(0xe501)
B33A  b7           OR A
B33B  3e 00        LD A,0
B33D  6f           LD L,A
B33E  67           LD H,A
B33F  32 c5 e1     LD (0xe1c5),A
B342  20 0e        JR nz,0xb352
B344  32 50 e0     LD (0xe050),A
B347  32 52 e0     LD (0xe052),A ; vit_aphrodite
B34A  22 5c e0     LD (0xe05c),HL
B34D  32 5e e0     LD (0xe05e),A
B350  18 0c        JR 0xb35e
B352  32 54 e0     LD (0xe054),A
B355  32 56 e0     LD (0xe056),A ; vit_popolon
B358  22 5f e0     LD (0xe05f),HL
B35B  32 61 e0     LD (0xe061),A
B35E  3e 05        LD A,5
B360  cd 66 b3     CALL 0xb366
B363  c3 32 58     JP 0x5832
B366  32 00 e5     LD (0xe500),A ; player_action
B369  af           XOR A
B36A  32 0e e5     LD (0xe50e),A
B36D  32 33 e5     LD (0xe533),A
B370  32 08 e5     LD (0xe508),A
B373  32 30 e5     LD (0xe530),A
B376  32 1b e5     LD (0xe51b),A
B379  32 26 e5     LD (0xe526),A
B37C  32 38 e5     LD (0xe538),A
B37F  3c           INC A
B380  32 18 e5     LD (0xe518),A
B383  c3 e3 a3     JP 0xa3e3
