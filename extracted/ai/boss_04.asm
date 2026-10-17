; jefe 4 (E700), mundo 2, vida (7F97[4]): 40
; ---- inicio (b01:7F82[3]) ----
8100  ed 5f        LD A,R
8102  e6 7f        AND 0x7f
8104  c6 50        ADD A,0x50
8106  c9           RET
846E  cd 75 87     CALL 0x8775
8471  dd 36 05 48  LD (IX+5),0x48 ; y
8475  dd 36 07 68  LD (IX+7),0x68 ; x
8479  18 6a        JR 0x84e5
84E5  cd 00 81     CALL 0x8100
84E8  32 08 e7     LD (0xe708),A
84EB  dd 36 0a 10  LD (IX+10),0x10 ; d1
84EF  c9           RET
8775  dd cb 02 a6  RES 4,(IX+2) ; flags
8779  c9           RET
; ---- actualizacion (b01:7F40[3]) ----
8100  ed 5f        LD A,R
8102  e6 7f        AND 0x7f
8104  c6 50        ADD A,0x50
8106  c9           RET
847B  3a 01 e7     LD A,(0xe701)
847E  3d           DEC A
847F  28 1f        JR z,0x84a0
8481  f2 de 84     JP p,0x84de
8484  dd 35 08     DEC (IX+8)
8487  28 0b        JR z,0x8494
8489  3a 08 e7     LD A,(0xe708)
848C  fe 14        CP 0x14
848E  d0           RET nc
848F  dd 36 0a 11  LD (IX+10),0x11 ; d1
8493  c9           RET
8494  cd 00 81     CALL 0x8100
8497  32 08 e7     LD (0xe708),A
849A  dd 36 0a 12  LD (IX+10),0x12 ; d1
849E  18 3a        JR 0x84da
84A0  dd 35 08     DEC (IX+8)
84A3  28 2d        JR z,0x84d2
84A5  3a 08 e7     LD A,(0xe708)
84A8  e6 03        AND 3
84AA  c0           RET nz
84AB  3a 0a e7     LD A,(0xe70a)
84AE  ee 01        XOR 1
84B0  32 0a e7     LD (0xe70a),A
84B3  fe 13        CP 0x13
84B5  c0           RET nz
84B6  cd 00 81     CALL 0x8100
84B9  e6 0f        AND 15
84BB  47           LD B,A
84BC  3e 10        LD A,0x10
84BE  dd 86 07     ADD A,(IX+7) ; x
84C1  5f           LD E,A
84C2  dd 7e 05     LD A,(IX+5) ; y
84C5  d6 08        SUB 8
84C7  57           LD D,A
84C8  3e 59        LD A,0x59
84CA  cd 7d 40     CALL 0x407d
84CD  3e 12        LD A,0x12
84CF  c3 59 40     JP 0x4059 ; sound_request
84D2  dd 36 0a 11  LD (IX+10),0x11 ; d1
84D6  dd 36 08 14  LD (IX+8),0x14
84DA  dd 34 01     INC (IX+1) ; estado
84DD  c9           RET
84DE  dd 35 08     DEC (IX+8)
84E1  c0           RET nz
84E2  cd f8 86     CALL 0x86f8
84E5  cd 00 81     CALL 0x8100
84E8  32 08 e7     LD (0xe708),A
84EB  dd 36 0a 10  LD (IX+10),0x10 ; d1
84EF  c9           RET
86F8  dd 36 01 00  LD (IX+1),0 ; estado
86FC  c9           RET
