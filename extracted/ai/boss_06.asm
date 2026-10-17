; jefe 6 (E700), mundo 9, vida (7F97[6]): 80
; ---- inicio (b01:7F82[5]) ----
895C  dd 36 02 f0  LD (IX+2),0xf0 ; flags
8960  dd 36 05 70  LD (IX+5),0x70 ; y
8964  dd 36 07 58  LD (IX+7),0x58 ; x
8968  dd 36 0a 34  LD (IX+10),0x34 ; d1
896C  dd 36 16 10  LD (IX+0x16),0x10 ; ay_lo
8970  af           XOR A
8971  32 0c e7     LD (0xe70c),A
8974  32 0d e7     LD (0xe70d),A
8977  32 1b e7     LD (0xe71b),A
897A  32 17 e7     LD (0xe717),A
897D  dd 36 0e 80  LD (IX+14),0x80 ; vx_lo
8981  dd 77 0f     LD (IX+15),A ; vx
8984  dd 36 19 02  LD (IX+0x19),2
8988  dd 36 1a 30  LD (IX+0x1a),0x30
898C  01 40 04     LD BC,0x440
898F  c5           PUSH BC
8990  16 28        LD D,0x28
8992  59           LD E,C
8993  3e 69        LD A,0x69
8995  cd 7d 40     CALL 0x407d
8998  c1           POP BC
8999  79           LD A,C
899A  c6 20        ADD A,0x20
899C  4f           LD C,A
899D  10 f0        DJNZ 0x898f
899F  c9           RET
; ---- actualizacion (b01:7F40[5]) ----
7FA3  cd 20 42     CALL 0x4220 ; map_a000_bank06
7FA6  cd 3e b1     CALL 0xb13e
7FA9  c3 15 42     JP 0x4215 ; map_a000_bank03
86DA  11 00 00     LD DE,0
86DD  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
89A0  cd f5 89     CALL 0x89f5
89A3  cd 22 8a     CALL 0x8a22
89A6  3a 01 e7     LD A,(0xe701)
89A9  3d           DEC A
89AA  28 2b        JR z,0x89d7
89AC  3a 1c e7     LD A,(0xe71c)
89AF  a7           AND A
89B0  20 16        JR nz,0x89c8
89B2  3a 07 e7     LD A,(0xe707)
89B5  e6 07        AND 7
89B7  c0           RET nz
89B8  3a 07 e7     LD A,(0xe707)
89BB  c6 88        ADD A,0x88
89BD  fe d8        CP 0xd8
89BF  d0           RET nc
89C0  3a 0f e7     LD A,(0xe70f)
89C3  2f           CPL
89C4  32 0f e7     LD (0xe70f),A
89C7  c9           RET
89C8  af           XOR A
89C9  32 1c e7     LD (0xe71c),A
89CC  cd da 86     CALL 0x86da
89CF  dd 36 18 20  LD (IX+0x18),0x20
89D3  dd 34 01     INC (IX+1) ; estado
89D6  c9           RET
89D7  dd 35 18     DEC (IX+0x18)
89DA  28 0f        JR z,0x89eb
89DC  dd 36 0e 00  LD (IX+14),0 ; vx_lo
89E0  3a 07 e7     LD A,(0xe707)
89E3  fe 78        CP 0x78
89E5  d0           RET nc
89E6  dd 36 0e 80  LD (IX+14),0x80 ; vx_lo
89EA  c9           RET
89EB  dd 36 01 00  LD (IX+1),0 ; estado
89EF  11 80 ff     LD DE,0xff80
89F2  c3 a3 7f     JP 0x7fa3 ; vx=DE (b06:B13E)
89F5  dd 35 16     DEC (IX+0x16) ; ay_lo
89F8  c0           RET nz
89F9  cd e3 4d     CALL 0x4de3
89FC  e6 1f        AND 0x1f
89FE  c6 03        ADD A,3
8A00  32 16 e7     LD (0xe716),A
8A03  21 1e 8a     LD HL,0x8a1e
8A06  dd 35 17     DEC (IX+0x17) ; ay
8A09  28 02        JR z,0x8a0d
8A0B  23           INC HL
8A0C  23           INC HL
8A0D  ed 5f        LD A,R
8A0F  e6 01        AND 1
8A11  28 01        JR z,0x8a14
8A13  23           INC HL
8A14  7e           LD A,(HL)
8A15  32 0a e7     LD (0xe70a),A
8A18  d6 34        SUB 0x34
8A1A  32 17 e7     LD (0xe717),A
8A1D  c9           RET
8A22  dd 35 1a     DEC (IX+0x1a)
8A25  20 0c        JR nz,0x8a33
8A27  dd 36 1a 30  LD (IX+0x1a),0x30
8A2B  3a 1b e7     LD A,(0xe71b)
8A2E  ee 01        XOR 1
8A30  32 1b e7     LD (0xe71b),A
8A33  dd cb 02 e6  SET 4,(IX+2) ; flags
8A37  3a 1b e7     LD A,(0xe71b)
8A3A  a7           AND A
8A3B  c8           RET z
8A3C  dd cb 02 a6  RES 4,(IX+2) ; flags
8A40  dd 35 19     DEC (IX+0x19)
8A43  c0           RET nz
8A44  dd 36 19 10  LD (IX+0x19),0x10
8A48  dd 36 16 18  LD (IX+0x16),0x18 ; ay_lo
8A4C  3a 17 e7     LD A,(0xe717)
8A4F  fe 03        CP 3
8A51  20 02        JR nz,0x8a55
8A53  3e 01        LD A,1
8A55  21 6e 8a     LD HL,0x8a6e
8A58  cd 4c 41     CALL 0x414c ; read_word_table
8A5B  eb           EX DE,HL
8A5C  7e           LD A,(HL)
8A5D  dd 86 05     ADD A,(IX+5) ; y
8A60  57           LD D,A
8A61  23           INC HL
8A62  7e           LD A,(HL)
8A63  dd 86 07     ADD A,(IX+7) ; x
8A66  5f           LD E,A
8A67  06 02        LD B,2
8A69  3e 57        LD A,0x57
8A6B  c3 7d 40     JP 0x407d
