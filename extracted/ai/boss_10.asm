; jefe 10 (E700), mundo 5, vida (7F97[10]): 72
; ---- inicio (b01:7F82[9]) ----
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
7FB5  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FB8  cd 37 b1     CALL 0xb137
7FBB  c3 15 42     JP 0x4215 ; map_a000_bank03
84F0  dd cb 09 f6  SET 6,(IX+9) ; d0
84F4  dd 36 0b 00  LD (IX+11),0 ; d2
84F8  dd 36 02 f0  LD (IX+2),0xf0 ; flags
84FC  dd 36 05 60  LD (IX+5),0x60 ; y
8500  dd 36 07 70  LD (IX+7),0x70 ; x
8504  dd 36 0a 1c  LD (IX+10),0x1c ; d1
8508  dd 36 18 23  LD (IX+0x18),0x23
850C  dd 36 19 1e  LD (IX+0x19),0x1e
8510  dd 36 1c 01  LD (IX+0x1c),1
8514  dd 36 1a 03  LD (IX+0x1a),3
8518  dd 36 1d 08  LD (IX+0x1d),8
851C  dd 36 1b 01  LD (IX+0x1b),1
8520  dd 36 1e 04  LD (IX+0x1e),4
8524  3a 05 e7     LD A,(0xe705)
8527  32 1f e7     LD (0xe71f),A
852A  11 00 04     LD DE,0x400
852D  cd b5 7f     CALL 0x7fb5 ; vy=DE (b06:B137)
8530  16 fc        LD D,0xfc
8532  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
; ---- actualizacion (b01:7F40[9]) ----
8535  cd bb 85     CALL 0x85bb
8538  3a 01 e7     LD A,(0xe701)
853B  3d           DEC A
853C  ca e1 85     JP z,0x85e1
853F  3d           DEC A
8540  ca 0c 86     JP z,0x860c
8543  dd 35 1e     DEC (IX+0x1e)
8546  ca d1 85     JP z,0x85d1
8549  cd 86 85     CALL 0x8586
854C  cd a4 85     CALL 0x85a4
854F  dd 35 1a     DEC (IX+0x1a)
8552  c0           RET nz
8553  dd 36 1a 08  LD (IX+0x1a),8
8557  3a 05 e5     LD A,(0xe505) ; jugador_y
855A  47           LD B,A
855B  3a 1c e7     LD A,(0xe71c)
855E  17           RLA
855F  17           RLA
8560  17           RLA
8561  e6 f8        AND 0xf8
8563  dd 86 05     ADD A,(IX+5) ; y
8566  0e 01        LD C,1
8568  b8           CP B
8569  38 02        JR c,0x856d
856B  0e ff        LD C,0xff
856D  3a 19 e7     LD A,(0xe719)
8570  81           ADD A,C
8571  fe 22        CP 0x22
8573  20 0a        JR nz,0x857f
8575  3d           DEC A
8576  32 19 e7     LD (0xe719),A
8579  d6 1d        SUB 0x1d
857B  32 1c e7     LD (0xe71c),A
857E  c9           RET
857F  fe 1d        CP 0x1d
8581  20 f3        JR nz,0x8576
8583  3c           INC A
8584  18 f0        JR 0x8576
8586  3a 05 e7     LD A,(0xe705)
8589  dd 86 0d     ADD A,(IX+13) ; vy
858C  fe 78        CP 0x78
858E  38 08        JR c,0x8598
8590  dd 36 0d ff  LD (IX+13),0xff ; vy
8594  dd 36 1d 01  LD (IX+0x1d),1
8598  fe 40        CP 0x40
859A  d0           RET nc
859B  dd 36 0d 02  LD (IX+13),2 ; vy
859F  dd 36 1d 01  LD (IX+0x1d),1
85A3  c9           RET
85A4  3a 07 e7     LD A,(0xe707)
85A7  dd 86 0f     ADD A,(IX+15) ; vx
85AA  fe 88        CP 0x88
85AC  38 05        JR c,0x85b3
85AE  dd 36 0f ff  LD (IX+15),0xff ; vx
85B2  c9           RET
85B3  fe 40        CP 0x40
85B5  d0           RET nc
85B6  dd 36 0f 01  LD (IX+15),1 ; vx
85BA  c9           RET
85BB  dd 35 1d     DEC (IX+0x1d)
85BE  c0           RET nz
85BF  dd 36 1d 04  LD (IX+0x1d),4
85C3  dd cb 0d 7e  BIT 7,(IX+13) ; vy
85C7  c2 cd 8a     JP nz,0x8acd
85CA  dd 36 1d 0a  LD (IX+0x1d),10
85CE  c3 cd 8a     JP 0x8acd
85D1  dd 34 01     INC (IX+1) ; estado
85D4  dd cb 02 a6  RES 4,(IX+2) ; flags
85D8  dd 36 18 22  LD (IX+0x18),0x22
85DC  dd 36 1e 10  LD (IX+0x1e),0x10
85E0  c9           RET
85E1  dd 35 1e     DEC (IX+0x1e)
85E4  c0           RET nz
85E5  dd 34 01     INC (IX+1) ; estado
85E8  dd 36 1e 20  LD (IX+0x1e),0x20
85EC  3a 1c e7     LD A,(0xe71c)
85EF  3d           DEC A
85F0  21 08 86     LD HL,0x8608
85F3  cd 55 41     CALL 0x4155 ; read_byte_table
85F6  dd 4e 05     LD C,(IX+5) ; y
85F9  81           ADD A,C
85FA  57           LD D,A
85FB  3a 07 e7     LD A,(0xe707)
85FE  d6 10        SUB 0x10
8600  5f           LD E,A
8601  06 02        LD B,2
8603  3e 57        LD A,0x57
8605  c3 7d 40     JP 0x407d
860C  dd 35 1e     DEC (IX+0x1e)
860F  c0           RET nz
8610  dd cb 02 e6  SET 4,(IX+2) ; flags
8614  cd e3 4d     CALL 0x4de3
8617  e6 1f        AND 0x1f
8619  c6 08        ADD A,8
861B  dd 77 1e     LD (IX+0x1e),A
861E  dd 36 01 00  LD (IX+1),0 ; estado
8622  dd 36 18 23  LD (IX+0x18),0x23
8626  c9           RET
8ACD  3a 0a e7     LD A,(0xe70a)
8AD0  ee 01        XOR 1
8AD2  32 0a e7     LD (0xe70a),A
8AD5  c9           RET
