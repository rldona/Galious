; jefe 7 (E700), mundo 7, vida (7F97[7]): 64
; ---- inicio (b01:7F82[6]) ----
8627  3e f0        LD A,0xf0
8629  dd a6 02     AND (IX+2) ; flags
862C  32 02 e7     LD (0xe702),A
862F  dd 36 07 68  LD (IX+7),0x68 ; x
8633  dd 36 05 40  LD (IX+5),0x40 ; y
8637  dd 36 24 30  LD (IX+0x24),0x30
863B  dd 36 0a 18  LD (IX+10),0x18 ; d1
863F  dd 34 1e     INC (IX+0x1e)
8642  dd 34 1f     INC (IX+0x1f)
8645  dd 36 30 10  LD (IX+0x30),0x10
8649  dd 36 23 b0  LD (IX+0x23),0xb0
864D  c9           RET
; ---- actualizacion (b01:7F40[6]) ----
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
7FAC  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FAF  cd 5f b1     CALL 0xb15f
7FB2  c3 15 42     JP 0x4215 ; map_a000_bank03
7FB5  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FB8  cd 37 b1     CALL 0xb137
7FBB  c3 15 42     JP 0x4215 ; map_a000_bank03
864E  dd cb 20 46  BIT 0,(IX+0x20)
8652  28 39        JR z,0x868d
8654  dd 35 21     DEC (IX+0x21)
8657  20 34        JR nz,0x868d
8659  3a 22 e7     LD A,(0xe722)
865C  b7           OR A
865D  28 1e        JR z,0x867d
865F  dd 35 22     DEC (IX+0x22)
8662  28 19        JR z,0x867d
8664  3a 22 e7     LD A,(0xe722)
8667  fe 02        CP 2
8669  dd 36 21 20  LD (IX+0x21),0x20
866D  dd 36 0a 1b  LD (IX+10),0x1b ; d1
8671  28 1a        JR z,0x868d
8673  dd 36 21 20  LD (IX+0x21),0x20
8677  dd 36 0a 1a  LD (IX+10),0x1a ; d1
867B  18 10        JR 0x868d
867D  dd 36 20 00  LD (IX+0x20),0
8681  dd 36 22 00  LD (IX+0x22),0
8685  dd 36 0a 18  LD (IX+10),0x18 ; d1
8689  dd cb 02 e6  SET 4,(IX+2) ; flags
868D  21 00 e5     LD HL,0xe500 ; player_action
8690  7e           LD A,(HL)
8691  fe 04        CP 4
8693  ca 65 87     JP z,0x8765
8696  3a 01 e7     LD A,(0xe701)
8699  cd 40 41     CALL 0x4140 ; jump_table_a
869C  a4 86        .word 0x86A4 ; caso 0
869E  c1 86        .word 0x86C1 ; caso 1
86A0  e0 86        .word 0x86E0 ; caso 2
86A2  fd 86        .word 0x86FD ; caso 3
86A4  dd cb 1e 46  BIT 0,(IX+0x1e)
86A8  28 12        JR z,0x86bc
86AA  cd e3 4d     CALL 0x4de3
86AD  e6 01        AND 1
86AF  28 0b        JR z,0x86bc
86B1  cd e3 4d     CALL 0x4de3
86B4  e6 01        AND 1
86B6  32 1c e7     LD (0xe71c),A
86B9  c3 7a 87     JP 0x877a
86BC  dd 36 01 02  LD (IX+1),2 ; estado
86C0  c9           RET
86C1  dd 35 1b     DEC (IX+0x1b)
86C4  cc c3 87     CALL z,0x87c3
86C7  dc ac 7f     CALL c,0x7fac ; invierte vx (b06:B15F)
86CA  3a 07 e7     LD A,(0xe707)
86CD  fe 68        CP 0x68
86CF  c2 20 87     JP nz,0x8720
86D2  dd 36 1f 01  LD (IX+0x1f),1
86D6  dd 36 01 02  LD (IX+1),2 ; estado
86DA  11 00 00     LD DE,0
86DD  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
86E0  dd cb 1f 46  BIT 0,(IX+0x1f)
86E4  28 12        JR z,0x86f8
86E6  cd e3 4d     CALL 0x4de3
86E9  e6 01        AND 1
86EB  28 0b        JR z,0x86f8
86ED  3a 1d e7     LD A,(0xe71d)
86F0  ee 01        XOR 1
86F2  32 1d e7     LD (0xe71d),A
86F5  c3 95 87     JP 0x8795
86F8  dd 36 01 00  LD (IX+1),0 ; estado
86FC  c9           RET
86FD  dd 35 1b     DEC (IX+0x1b)
8700  cc b0 87     CALL z,0x87b0
8703  dc d9 87     CALL c,0x87d9
8706  3a 05 e7     LD A,(0xe705)
8709  fe 70        CP 0x70
870B  28 05        JR z,0x8712
870D  fe 40        CP 0x40
870F  c2 20 87     JP nz,0x8720
8712  dd 36 1e 01  LD (IX+0x1e),1
8716  dd 36 01 00  LD (IX+1),0 ; estado
871A  11 00 00     LD DE,0
871D  c3 b5 7f     JP 0x7fb5 ; vy=DE (b06:B137)
8720  3a 22 e7     LD A,(0xe722)
8723  b7           OR A
8724  c0           RET nz
8725  dd 35 30     DEC (IX+0x30)
8728  20 28        JR nz,0x8752
872A  dd 36 30 10  LD (IX+0x30),0x10
872E  dd 36 0a 19  LD (IX+10),0x19 ; d1
8732  dd 36 20 01  LD (IX+0x20),1
8736  dd 36 21 18  LD (IX+0x21),0x18
873A  3a 07 e7     LD A,(0xe707)
873D  c6 18        ADD A,0x18
873F  5f           LD E,A
8740  3a 05 e7     LD A,(0xe705)
8743  c6 18        ADD A,0x18
8745  57           LD D,A
8746  06 00        LD B,0
8748  3e 65        LD A,0x65
874A  cd 7d 40     CALL 0x407d
874D  3e 11        LD A,0x11
874F  c3 59 40     JP 0x4059 ; sound_request
8752  3a 22 e7     LD A,(0xe722)
8755  b7           OR A
8756  c0           RET nz
8757  cd e3 4d     CALL 0x4de3
875A  e6 01        AND 1
875C  c0           RET nz
875D  dd 35 23     DEC (IX+0x23)
8760  c0           RET nz
8761  dd 36 23 b0  LD (IX+0x23),0xb0
8765  dd 36 20 01  LD (IX+0x20),1
8769  dd 36 22 03  LD (IX+0x22),3
876D  dd 36 21 20  LD (IX+0x21),0x20
8771  dd 36 0a 1a  LD (IX+10),0x1a ; d1
8775  dd cb 02 a6  RES 4,(IX+2) ; flags
8779  c9           RET
877A  dd 36 1b 08  LD (IX+0x1b),8
877E  dd 36 01 01  LD (IX+1),1 ; estado
8782  dd 36 1f 00  LD (IX+0x1f),0
8786  dd cb 1c 46  BIT 0,(IX+0x1c)
878A  11 00 01     LD DE,0x100
878D  28 03        JR z,0x8792
878F  11 00 ff     LD DE,0xff00
8792  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
8795  dd 36 1b 08  LD (IX+0x1b),8
8799  dd 36 01 03  LD (IX+1),3 ; estado
879D  dd 36 1e 00  LD (IX+0x1e),0
87A1  dd cb 1d 46  BIT 0,(IX+0x1d)
87A5  11 00 01     LD DE,0x100
87A8  28 03        JR z,0x87ad
87AA  11 00 ff     LD DE,0xff00
87AD  c3 b5 7f     JP 0x7fb5 ; vy=DE (b06:B137)
87B0  dd 34 1b     INC (IX+0x1b)
87B3  3a 05 e7     LD A,(0xe705)
87B6  dd cb 0d 7e  BIT 7,(IX+13) ; vy
87BA  28 03        JR z,0x87bf
87BC  fe 40        CP 0x40
87BE  c9           RET
87BF  fe 70        CP 0x70
87C1  3f           CCF
87C2  c9           RET
87C3  dd 34 1b     INC (IX+0x1b)
87C6  3a 07 e7     LD A,(0xe707)
87C9  dd cb 0f 7e  BIT 7,(IX+15) ; vx
87CD  28 03        JR z,0x87d2
87CF  fe 18        CP 0x18
87D1  c9           RET
87D2  dd 86 24     ADD A,(IX+0x24)
87D5  fe e0        CP 0xe0
87D7  3f           CCF
87D8  c9           RET
87D9  dd cb 0d 7e  BIT 7,(IX+13) ; vy
87DD  11 00 01     LD DE,0x100
87E0  cc 5c 41     CALL z,0x415c ; neg_de
87E3  c3 b5 7f     JP 0x7fb5 ; vy=DE (b06:B137)
