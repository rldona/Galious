; tipo 0x47  (hijo, no aparece en salas)  descriptor c4 c0 0f ff 5c
; ---- inicio (b04:66B4[70]) ----
95AC  3a 24 e6     LD A,(0xe624)
95AF  dd 77 14     LD (IX+0x14),A
95B2  3a 07 e5     LD A,(0xe507) ; jugador_x
95B5  dd 77 1a     LD (IX+0x1a),A
95B8  dd 7e 09     LD A,(IX+9) ; d0
95BB  ee 45        XOR 0x45
95BD  dd 77 09     LD (IX+9),A ; d0
95C0  cd 50 96     CALL 0x9650
95C3  dd 71 08     LD (IX+8),C
95C6  c9           RET
9650  dd 7e 07     LD A,(IX+7) ; x
9653  dd 96 1a     SUB (IX+0x1a)
9656  30 02        JR nc,0x965a
9658  ed 44        NEG
965A  0e 12        LD C,0x12
965C  11 00 01     LD DE,0x100
965F  fe 28        CP 0x28
9661  38 29        JR c,0x968c
9663  0e 0f        LD C,15
9665  11 80 01     LD DE,0x180
9668  fe 48        CP 0x48
966A  38 20        JR c,0x968c
966C  0e 0c        LD C,12
966E  11 00 02     LD DE,0x200
9671  fe 60        CP 0x60
9673  38 17        JR c,0x968c
9675  0e 09        LD C,9
9677  11 80 02     LD DE,0x280
967A  fe 74        CP 0x74
967C  38 0e        JR c,0x968c
967E  0e 06        LD C,6
9680  11 00 03     LD DE,0x300
9683  fe 84        CP 0x84
9685  38 05        JR c,0x968c
9687  0e 03        LD C,3
9689  11 80 03     LD DE,0x380
968C  dd cb 14 46  BIT 0,(IX+0x14)
9690  c4 68 40     CALL nz,0x4068 ; neg_de
9693  c3 3e b1     JP 0xb13e
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[70]) ----
67AF  4d           LD C,L
67B0  26 00        LD H,0
67B2  44           LD B,H
67B3  29           ADD HL,HL
67B4  29           ADD HL,HL
67B5  09           ADD HL,BC
67B6  01 b6 67     LD BC,0x67b6
67B9  09           ADD HL,BC
67BA  c9           RET
6B71  dd cb 02 a6  RES 4,(IX+2) ; flags
6B75  c9           RET
6BEE  dd 7e 05     LD A,(IX+5) ; y
6BF1  c6 10        ADD A,0x10
6BF3  6f           LD L,A
6BF4  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6BF8  3e 10        LD A,0x10
6BFA  28 01        JR z,0x6bfd
6BFC  af           XOR A
6BFD  dd 86 07     ADD A,(IX+7) ; x
6C00  67           LD H,A
6C01  c3 6e 40     JP 0x406e ; vec_4dc8
6C04  01 e0 ff     LD BC,0xffe0
6C07  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6C0B  20 03        JR nz,0x6c10
6C0D  01 e0 ff     LD BC,0xffe0
6C10  09           ADD HL,BC
6C11  3e b9        LD A,0xb9
6C13  be           CP (HL)
6C14  d8           RET c
6C15  dd 7e 07     LD A,(IX+7) ; x
6C18  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6C1C  28 03        JR z,0x6c21
6C1E  fe 08        CP 8
6C20  c9           RET
6C21  fe f0        CP 0xf0
6C23  3f           CCF
6C24  c9           RET
95C7  dd 7e 01     LD A,(IX+1) ; estado
95CA  3d           DEC A
95CB  28 36        JR z,0x9603
95CD  f2 33 96     JP p,0x9633
95D0  dd 7e 07     LD A,(IX+7) ; x
95D3  dd cb 0f 7e  BIT 7,(IX+15) ; vx
95D7  20 07        JR nz,0x95e0
95D9  dd be 1a     CP (IX+0x1a)
95DC  30 16        JR nc,0x95f4
95DE  18 05        JR 0x95e5
95E0  dd be 1a     CP (IX+0x1a)
95E3  38 0f        JR c,0x95f4
95E5  cd 50 96     CALL 0x9650
95E8  cd 48 96     CALL 0x9648
95EB  cd ee 6b     CALL 0x6bee
95EE  30 04        JR nc,0x95f4
95F0  cd 04 6c     CALL 0x6c04
95F3  d0           RET nc
95F4  cd 71 6b     CALL 0x6b71
95F7  ed 5f        LD A,R
95F9  e6 1f        AND 0x1f
95FB  c6 20        ADD A,0x20
95FD  dd 77 15     LD (IX+0x15),A
9600  c3 07 9a     JP 0x9a07
9603  dd 35 15     DEC (IX+0x15)
9606  c0           RET nz
9607  dd 36 09 c4  LD (IX+9),0xc4 ; d0
960B  dd 36 0b 06  LD (IX+11),6 ; d2
960F  dd 36 0a 0a  LD (IX+10),10 ; d1
9613  dd 36 11 3d  LD (IX+0x11),0x3d ; atributo_sprite
9617  dd 36 15 10  LD (IX+0x15),0x10
961B  dd 7e 07     LD A,(IX+7) ; x
961E  d6 08        SUB 8
9620  dd 77 07     LD (IX+7),A ; x
9623  dd 7e 05     LD A,(IX+5) ; y
9626  d6 08        SUB 8
9628  dd 77 05     LD (IX+5),A ; y
962B  3e 18        LD A,0x18
962D  cd 59 40     CALL 0x4059 ; sound_request
9630  c3 07 9a     JP 0x9a07
9633  dd 35 15     DEC (IX+0x15)
9636  ca 1c b4     JP z,0xb41c
9639  dd 7e 15     LD A,(IX+0x15)
963C  fe 08        CP 8
963E  c0           RET nz
963F  dd 36 0a 0b  LD (IX+10),11 ; d1
9643  dd 36 11 5b  LD (IX+0x11),0x5b ; atributo_sprite
9647  c9           RET
9648  16 04        LD D,4
964A  59           LD E,C
964B  06 01        LD B,1
964D  c3 d6 b0     JP 0xb0d6
9650  dd 7e 07     LD A,(IX+7) ; x
9653  dd 96 1a     SUB (IX+0x1a)
9656  30 02        JR nc,0x965a
9658  ed 44        NEG
965A  0e 12        LD C,0x12
965C  11 00 01     LD DE,0x100
965F  fe 28        CP 0x28
9661  38 29        JR c,0x968c
9663  0e 0f        LD C,15
9665  11 80 01     LD DE,0x180
9668  fe 48        CP 0x48
966A  38 20        JR c,0x968c
966C  0e 0c        LD C,12
966E  11 00 02     LD DE,0x200
9671  fe 60        CP 0x60
9673  38 17        JR c,0x968c
9675  0e 09        LD C,9
9677  11 80 02     LD DE,0x280
967A  fe 74        CP 0x74
967C  38 0e        JR c,0x968c
967E  0e 06        LD C,6
9680  11 00 03     LD DE,0x300
9683  fe 84        CP 0x84
9685  38 05        JR c,0x968c
9687  0e 03        LD C,3
9689  11 80 03     LD DE,0x380
968C  dd cb 14 46  BIT 0,(IX+0x14)
9690  c4 68 40     CALL nz,0x4068 ; neg_de
9693  c3 3e b1     JP 0xb13e
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B0D6  dd 35 08     DEC (IX+8)
B0D9  c0           RET nz
B0DA  dd 73 08     LD (IX+8),E
B0DD  dd 7e 0a     LD A,(IX+10) ; d1
B0E0  aa           XOR D
B0E1  dd 77 0a     LD (IX+10),A ; d1
B0E4  dd 7e 11     LD A,(IX+0x11) ; atributo_sprite
B0E7  a8           XOR B
B0E8  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B0EB  af           XOR A
B0EC  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B41C  cd 3d b4     CALL 0xb43d
B41F  dd 6e 00     LD L,(IX) ; tipo
B422  cd af 67     CALL 0x67af
B425  eb           EX DE,HL
B426  af           XOR A
B427  dd 77 00     LD (IX),A ; tipo
B42A  dd 77 02     LD (IX+2),A ; flags
B42D  21 20 e6     LD HL,0xe620
B430  35           DEC (HL)
B431  2c           INC L
B432  1a           LD A,(DE)
B433  cb 7f        BIT 7,A
B435  c8           RET z
B436  e6 07        AND 7
B438  47           LD B,A
B439  7e           LD A,(HL)
B43A  90           SUB B
B43B  77           LD (HL),A
B43C  c9           RET
B43D  dd 7e 03     LD A,(IX+3) ; flags2
B440  cb 5f        BIT 3,A
B442  c8           RET z
B443  e6 07        AND 7
B445  3c           INC A
B446  47           LD B,A
B447  af           XOR A
B448  37           SCF
B449  17           RLA
B44A  10 fd        DJNZ 0xb449
B44C  2a 0c e6     LD HL,(0xe60c)
B44F  b6           OR (HL)
B450  77           LD (HL),A
B451  c9           RET
