; tipo 0x34  vida 4  patron observado: vertical
; ---- inicio (b04:66B4[51]) ----
AAF4  11 00 01     LD DE,0x100
AAF7  cd 37 b1     CALL 0xb137
AAFA  dd 7e 05     LD A,(IX+5) ; y
AAFD  fe 20        CP 0x20
AAFF  30 0a        JR nc,0xab0b
AB01  dd 36 14 00  LD (IX+0x14),0
AB05  cd 33 ab     CALL 0xab33
AB08  c3 37 b1     JP 0xb137
AB0B  cd 20 ab     CALL 0xab20
AB0E  30 0b        JR nc,0xab1b
AB10  cd 6f b1     CALL 0xb16f
AB13  dd 7e 14     LD A,(IX+0x14)
AB16  ee 01        XOR 1
AB18  dd 77 14     LD (IX+0x14),A
AB1B  cd 33 ab     CALL 0xab33
AB1E  18 5b        JR 0xab7b
AB20  dd 66 07     LD H,(IX+7) ; x
AB23  dd 7e 05     LD A,(IX+5) ; y
AB26  dd cb 0d 7e  BIT 7,(IX+13) ; vy
AB2A  20 02        JR nz,0xab2e
AB2C  c6 08        ADD A,8
AB2E  6f           LD L,A
AB2F  cd 6e 40     CALL 0x406e ; vec_4dc8
AB32  c9           RET
AB33  3a 07 e5     LD A,(0xe507) ; jugador_x
AB36  dd 96 07     SUB (IX+7) ; x
AB39  30 02        JR nc,0xab3d
AB3B  ed 44        NEG
AB3D  fe 40        CP 0x40
AB3F  11 00 01     LD DE,0x100
AB42  30 11        JR nc,0xab55
AB44  fe 28        CP 0x28
AB46  11 80 01     LD DE,0x180
AB49  30 0a        JR nc,0xab55
AB4B  fe 18        CP 0x18
AB4D  11 00 02     LD DE,0x200
AB50  30 03        JR nc,0xab55
AB52  11 80 02     LD DE,0x280
AB55  3a 05 e5     LD A,(0xe505) ; jugador_y
AB58  dd 96 05     SUB (IX+5) ; y
AB5B  30 02        JR nc,0xab5f
AB5D  ed 44        NEG
AB5F  fe 80        CP 0x80
AB61  21 00 01     LD HL,0x100
AB64  d0           RET nc
AB65  fe 60        CP 0x60
AB67  21 80 01     LD HL,0x180
AB6A  d0           RET nc
AB6B  fe 40        CP 0x40
AB6D  21 00 02     LD HL,0x200
AB70  d0           RET nc
AB71  fe 20        CP 0x20
AB73  21 80 02     LD HL,0x280
AB76  d0           RET nc
AB77  21 00 03     LD HL,0x300
AB7A  c9           RET
AB7B  19           ADD HL,DE
AB7C  eb           EX DE,HL
AB7D  cd 37 b1     CALL 0xb137
AB80  dd cb 14 46  BIT 0,(IX+0x14)
AB84  c8           RET z
AB85  c3 6f b1     JP 0xb16f
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B16F  dd 5e 0c     LD E,(IX+12) ; vy_lo
B172  dd 56 0d     LD D,(IX+13) ; vy
B175  cd 68 40     CALL 0x4068 ; neg_de
B178  dd 73 0c     LD (IX+12),E ; vy_lo
B17B  dd 72 0d     LD (IX+13),D ; vy
B17E  c9           RET
; ---- actualizacion (b06:B472[51]) ----
AAFA  dd 7e 05     LD A,(IX+5) ; y
AAFD  fe 20        CP 0x20
AAFF  30 0a        JR nc,0xab0b
AB01  dd 36 14 00  LD (IX+0x14),0
AB05  cd 33 ab     CALL 0xab33
AB08  c3 37 b1     JP 0xb137
AB0B  cd 20 ab     CALL 0xab20
AB0E  30 0b        JR nc,0xab1b
AB10  cd 6f b1     CALL 0xb16f
AB13  dd 7e 14     LD A,(IX+0x14)
AB16  ee 01        XOR 1
AB18  dd 77 14     LD (IX+0x14),A
AB1B  cd 33 ab     CALL 0xab33
AB1E  18 5b        JR 0xab7b
AB20  dd 66 07     LD H,(IX+7) ; x
AB23  dd 7e 05     LD A,(IX+5) ; y
AB26  dd cb 0d 7e  BIT 7,(IX+13) ; vy
AB2A  20 02        JR nz,0xab2e
AB2C  c6 08        ADD A,8
AB2E  6f           LD L,A
AB2F  cd 6e 40     CALL 0x406e ; vec_4dc8
AB32  c9           RET
AB33  3a 07 e5     LD A,(0xe507) ; jugador_x
AB36  dd 96 07     SUB (IX+7) ; x
AB39  30 02        JR nc,0xab3d
AB3B  ed 44        NEG
AB3D  fe 40        CP 0x40
AB3F  11 00 01     LD DE,0x100
AB42  30 11        JR nc,0xab55
AB44  fe 28        CP 0x28
AB46  11 80 01     LD DE,0x180
AB49  30 0a        JR nc,0xab55
AB4B  fe 18        CP 0x18
AB4D  11 00 02     LD DE,0x200
AB50  30 03        JR nc,0xab55
AB52  11 80 02     LD DE,0x280
AB55  3a 05 e5     LD A,(0xe505) ; jugador_y
AB58  dd 96 05     SUB (IX+5) ; y
AB5B  30 02        JR nc,0xab5f
AB5D  ed 44        NEG
AB5F  fe 80        CP 0x80
AB61  21 00 01     LD HL,0x100
AB64  d0           RET nc
AB65  fe 60        CP 0x60
AB67  21 80 01     LD HL,0x180
AB6A  d0           RET nc
AB6B  fe 40        CP 0x40
AB6D  21 00 02     LD HL,0x200
AB70  d0           RET nc
AB71  fe 20        CP 0x20
AB73  21 80 02     LD HL,0x280
AB76  d0           RET nc
AB77  21 00 03     LD HL,0x300
AB7A  c9           RET
AB7B  19           ADD HL,DE
AB7C  eb           EX DE,HL
AB7D  cd 37 b1     CALL 0xb137
AB80  dd cb 14 46  BIT 0,(IX+0x14)
AB84  c8           RET z
AB85  c3 6f b1     JP 0xb16f
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B16F  dd 5e 0c     LD E,(IX+12) ; vy_lo
B172  dd 56 0d     LD D,(IX+13) ; vy
B175  cd 68 40     CALL 0x4068 ; neg_de
B178  dd 73 0c     LD (IX+12),E ; vy_lo
B17B  dd 72 0d     LD (IX+13),D ; vy
B17E  c9           RET
