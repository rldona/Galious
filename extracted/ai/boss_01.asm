; jefe 1 (E700), mundo 1, vida (7F97[1]): 24
; ---- inicio (b01:7F82[0]) ----
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
7FBE  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FC1  cd 45 b1     CALL 0xb145
7FC4  c3 15 42     JP 0x4215 ; map_a000_bank03
8013  3e 68        LD A,0x68
8015  dd 77 05     LD (IX+5),A ; y
8018  dd 77 1e     LD (IX+0x1e),A
801B  dd 36 07 98  LD (IX+7),0x98 ; x
801F  cd 07 81     CALL 0x8107
8022  cd f6 80     CALL 0x80f6
8025  11 80 00     LD DE,0x80
8028  cd be 7f     CALL 0x7fbe ; ay=DE (b06:B145)
802B  c3 c9 80     JP 0x80c9
80C9  3a 07 e5     LD A,(0xe507) ; jugador_x
80CC  dd be 07     CP (IX+7) ; x
80CF  11 80 00     LD DE,0x80
80D2  dc 5c 41     CALL c,0x415c ; neg_de
80D5  dd 36 0a 0c  LD (IX+10),12 ; d1
80D9  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
80F6  ed 5f        LD A,R
80F8  e6 1f        AND 0x1f
80FA  c6 10        ADD A,0x10
80FC  32 1f e7     LD (0xe71f),A
80FF  c9           RET
8100  ed 5f        LD A,R
8102  e6 7f        AND 0x7f
8104  c6 50        ADD A,0x50
8106  c9           RET
8107  cd 00 81     CALL 0x8100
810A  32 1d e7     LD (0xe71d),A
810D  c9           RET
; ---- actualizacion (b01:7F40[0]) ----
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
7FAC  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FAF  cd 5f b1     CALL 0xb15f
7FB2  c3 15 42     JP 0x4215 ; map_a000_bank03
7FB5  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FB8  cd 37 b1     CALL 0xb137
7FBB  c3 15 42     JP 0x4215 ; map_a000_bank03
7FC7  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FCA  cd 4c b1     CALL 0xb14c
7FCD  c3 15 42     JP 0x4215 ; map_a000_bank03
802E  3a 01 e7     LD A,(0xe701)
8031  3d           DEC A
8032  28 38        JR z,0x806c
8034  f2 af 80     JP p,0x80af
8037  dd 35 1d     DEC (IX+0x1d)
803A  28 10        JR z,0x804c
803C  cd 0e 81     CALL 0x810e
803F  dd 35 1f     DEC (IX+0x1f)
8042  cc dc 80     CALL z,0x80dc
8045  cd 36 81     CALL 0x8136
8048  dc ac 7f     CALL c,0x7fac ; invierte vx (b06:B15F)
804B  c9           RET
804C  dd 34 01     INC (IX+1) ; estado
804F  dd 36 0a 0d  LD (IX+10),13 ; d1
8053  cd da 86     CALL 0x86da
8056  ed 5f        LD A,R
8058  e6 03        AND 3
805A  3c           INC A
805B  3c           INC A
805C  3c           INC A
805D  87           ADD A,A
805E  87           ADD A,A
805F  87           ADD A,A
8060  87           ADD A,A
8061  c6 05        ADD A,5
8063  dd 77 18     LD (IX+0x18),A
8066  11 00 f9     LD DE,0xf900
8069  c3 b5 7f     JP 0x7fb5 ; vy=DE (b06:B137)
806C  cd 46 81     CALL 0x8146
806F  cd 56 81     CALL 0x8156
8072  cd c7 7f     CALL 0x7fc7 ; vy+=ay (b06:B14C)
8075  3a 05 e7     LD A,(0xe705)
8078  dd cb 0d 7e  BIT 7,(IX+13) ; vy
807C  20 06        JR nz,0x8084
807E  dd be 1e     CP (IX+0x1e)
8081  30 12        JR nc,0x8095
8083  c9           RET
8084  dd be 18     CP (IX+0x18)
8087  d0           RET nc
8088  ed 5b 0c e7  LD DE,(0xe70c)
808C  cd 5c 41     CALL 0x415c ; neg_de
808F  ed 53 0c e7  LD (0xe70c),DE
8093  18 47        JR 0x80dc
8095  cd 07 81     CALL 0x8107
8098  dd 36 0a 0c  LD (IX+10),12 ; d1
809C  dd 36 08 3c  LD (IX+8),0x3c
80A0  dd 36 01 02  LD (IX+1),2 ; estado
80A4  cd 75 87     CALL 0x8775
80A7  cd 1a 87     CALL 0x871a
80AA  3e 21        LD A,0x21
80AC  c3 1f 4e     JP 0x4e1f ; sound_request
80AF  dd 35 08     DEC (IX+8)
80B2  c0           RET nz
80B3  cd f8 86     CALL 0x86f8
80B6  dd cb 02 e6  SET 4,(IX+2) ; flags
80BA  dd 34 19     INC (IX+0x19)
80BD  3a 19 e7     LD A,(0xe719)
80C0  e6 03        AND 3
80C2  20 05        JR nz,0x80c9
80C4  ed 5f        LD A,R
80C6  1f           RRA
80C7  18 06        JR 0x80cf
80C9  3a 07 e5     LD A,(0xe507) ; jugador_x
80CC  dd be 07     CP (IX+7) ; x
80CF  11 80 00     LD DE,0x80
80D2  dc 5c 41     CALL c,0x415c ; neg_de
80D5  dd 36 0a 0c  LD (IX+10),12 ; d1
80D9  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
80DC  cd f6 80     CALL 0x80f6
80DF  06 03        LD B,3
80E1  c5           PUSH BC
80E2  dd 5e 07     LD E,(IX+7) ; x
80E5  dd 56 05     LD D,(IX+5) ; y
80E8  05           DEC B
80E9  3e 54        LD A,0x54
80EB  cd 7d 40     CALL 0x407d
80EE  c1           POP BC
80EF  10 f0        DJNZ 0x80e1
80F1  3e 11        LD A,0x11
80F3  c3 59 40     JP 0x4059 ; sound_request
80F6  ed 5f        LD A,R
80F8  e6 1f        AND 0x1f
80FA  c6 10        ADD A,0x10
80FC  32 1f e7     LD (0xe71f),A
80FF  c9           RET
8100  ed 5f        LD A,R
8102  e6 7f        AND 0x7f
8104  c6 50        ADD A,0x50
8106  c9           RET
8107  cd 00 81     CALL 0x8100
810A  32 1d e7     LD (0xe71d),A
810D  c9           RET
810E  dd 36 05 68  LD (IX+5),0x68 ; y
8112  dd 36 0a 0c  LD (IX+10),12 ; d1
8116  dd cb 0f 7e  BIT 7,(IX+15) ; vx
811A  20 0f        JR nz,0x812b
811C  3a 04 e6     LD A,(0xe604)
811F  cb 67        BIT 4,A
8121  c0           RET nz
8122  dd 36 0a 0e  LD (IX+10),14 ; d1
8126  dd 36 05 60  LD (IX+5),0x60 ; y
812A  c9           RET
812B  3a 04 e6     LD A,(0xe604)
812E  cb 67        BIT 4,A
8130  c0           RET nz
8131  dd 36 0a 0d  LD (IX+10),13 ; d1
8135  c9           RET
8136  dd cb 0f 7e  BIT 7,(IX+15) ; vx
813A  3a 07 e7     LD A,(0xe707)
813D  28 03        JR z,0x8142
813F  fe 92        CP 0x92
8141  c9           RET
8142  fe a0        CP 0xa0
8144  3f           CCF
8145  c9           RET
8146  3a 04 e6     LD A,(0xe604)
8149  e6 03        AND 3
814B  c0           RET nz
814C  3a 0a e7     LD A,(0xe70a)
814F  ee 02        XOR 2
8151  18 19        JR 0x816c
8156  3a 04 e6     LD A,(0xe604)
8159  e6 0f        AND 15
815B  c0           RET nz
815C  3e 0a        LD A,10
815E  c3 1f 4e     JP 0x4e1f ; sound_request
816C  32 0a e7     LD (0xe70a),A
816F  c9           RET
86DA  11 00 00     LD DE,0
86DD  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
86F8  dd 36 01 00  LD (IX+1),0 ; estado
86FC  c9           RET
871A  11 00 00     LD DE,0
871D  c3 b5 7f     JP 0x7fb5 ; vy=DE (b06:B137)
8775  dd cb 02 a6  RES 4,(IX+2) ; flags
8779  c9           RET
