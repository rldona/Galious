; jefe 3 (E700), mundo 6, vida (7F97[3]): 56
; ---- inicio (b01:7F82[2]) ----
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
8100  ed 5f        LD A,R
8102  e6 7f        AND 0x7f
8104  c6 50        ADD A,0x50
8106  c9           RET
8384  cd 2b 84     CALL 0x842b
8387  dd 36 05 90  LD (IX+5),0x90 ; y
838B  dd 36 07 60  LD (IX+7),0x60 ; x
838F  dd 36 0a 3a  LD (IX+10),0x3a ; d1
8393  ed 5f        LD A,R
8395  e6 1f        AND 0x1f
8397  32 08 e7     LD (0xe708),A
839A  ed 5f        LD A,R
839C  e6 01        AND 1
839E  11 00 ff     LD DE,0xff00
83A1  cc 5c 41     CALL z,0x415c ; neg_de
83A4  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
842B  cd 00 81     CALL 0x8100
842E  e6 7f        AND 0x7f
8430  c6 1f        ADD A,0x1f
8432  32 1f e7     LD (0xe71f),A
8435  c9           RET
; ---- actualizacion (b01:7F40[2]) ----
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
8100  ed 5f        LD A,R
8102  e6 7f        AND 0x7f
8104  c6 50        ADD A,0x50
8106  c9           RET
816C  32 0a e7     LD (0xe70a),A
816F  c9           RET
8393  ed 5f        LD A,R
8395  e6 1f        AND 0x1f
8397  32 08 e7     LD (0xe708),A
839A  ed 5f        LD A,R
839C  e6 01        AND 1
839E  11 00 ff     LD DE,0xff00
83A1  cc 5c 41     CALL z,0x415c ; neg_de
83A4  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
83A7  3a 01 e7     LD A,(0xe701)
83AA  3d           DEC A
83AB  28 4f        JR z,0x83fc
83AD  cd 36 84     CALL 0x8436
83B0  dd 35 1f     DEC (IX+0x1f)
83B3  28 2d        JR z,0x83e2
83B5  cd 55 84     CALL 0x8455
83B8  3a 05 e5     LD A,(0xe505) ; jugador_y
83BB  dd be 05     CP (IX+5) ; y
83BE  30 06        JR nc,0x83c6
83C0  dd 35 08     DEC (IX+8)
83C3  c0           RET nz
83C4  18 cd        JR 0x8393
83C6  3a 04 e6     LD A,(0xe604)
83C9  e6 1f        AND 0x1f
83CB  c0           RET nz
83CC  3a 07 e7     LD A,(0xe707)
83CF  e6 f8        AND 0xf8
83D1  c6 28        ADD A,0x28
83D3  47           LD B,A
83D4  3a 07 e5     LD A,(0xe507) ; jugador_x
83D7  d6 08        SUB 8
83D9  b8           CP B
83DA  3e 00        LD A,0
83DC  30 01        JR nc,0x83df
83DE  3c           INC A
83DF  c3 9c 83     JP 0x839c
83E2  ed 5f        LD A,R
83E4  e6 03        AND 3
83E6  3c           INC A
83E7  87           ADD A,A
83E8  87           ADD A,A
83E9  87           ADD A,A
83EA  87           ADD A,A
83EB  e6 30        AND 0x30
83ED  c6 0f        ADD A,15
83EF  32 1f e7     LD (0xe71f),A
83F2  cd 75 87     CALL 0x8775
83F5  cd da 84     CALL 0x84da
83F8  3e 3c        LD A,0x3c
83FA  18 6f        JR 0x846b
83FC  dd 35 1f     DEC (IX+0x1f)
83FF  28 23        JR z,0x8424
8401  3a 1f e7     LD A,(0xe71f)
8404  e6 0f        AND 15
8406  c0           RET nz
8407  06 06        LD B,6
8409  c5           PUSH BC
840A  3a 07 e7     LD A,(0xe707)
840D  c6 20        ADD A,0x20
840F  5f           LD E,A
8410  3a 05 e7     LD A,(0xe705)
8413  c6 10        ADD A,0x10
8415  57           LD D,A
8416  05           DEC B
8417  3e 58        LD A,0x58
8419  cd 7d 40     CALL 0x407d
841C  c1           POP BC
841D  10 ea        DJNZ 0x8409
841F  3e 12        LD A,0x12
8421  c3 1f 4e     JP 0x4e1f ; sound_request
8424  dd 35 01     DEC (IX+1) ; estado
8427  dd cb 02 e6  SET 4,(IX+2) ; flags
842B  cd 00 81     CALL 0x8100
842E  e6 7f        AND 0x7f
8430  c6 1f        ADD A,0x1f
8432  32 1f e7     LD (0xe71f),A
8435  c9           RET
8436  dd cb 0f 7e  BIT 7,(IX+15) ; vx
843A  3a 07 e7     LD A,(0xe707)
843D  28 09        JR z,0x8448
843F  06 29        LD B,0x29
8441  b8           CP B
8442  d0           RET nc
8443  11 00 01     LD DE,0x100
8446  18 07        JR 0x844f
8448  06 8c        LD B,0x8c
844A  b8           CP B
844B  d8           RET c
844C  11 00 ff     LD DE,0xff00
844F  dd 70 07     LD (IX+7),B ; x
8452  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
8455  3a 04 e6     LD A,(0xe604)
8458  e6 07        AND 7
845A  c0           RET nz
845B  dd 34 19     INC (IX+0x19)
845E  3a 19 e7     LD A,(0xe719)
8461  e6 03        AND 3
8463  fe 03        CP 3
8465  20 02        JR nz,0x8469
8467  3e 01        LD A,1
8469  c6 3a        ADD A,0x3a
846B  c3 6c 81     JP 0x816c
84DA  dd 34 01     INC (IX+1) ; estado
84DD  c9           RET
8775  dd cb 02 a6  RES 4,(IX+2) ; flags
8779  c9           RET
