; tipo 0x05  vida 3  patron observado: camina
; ---- inicio (b04:66B4[4]) ----
6AE8  dd 36 15 78  LD (IX+0x15),0x78
6AEC  ed 5f        LD A,R
6AEE  e6 01        AND 1
6AF0  11 00 01     LD DE,0x100
6AF3  c4 68 40     CALL nz,0x4068 ; neg_de
6AF6  cd 3e b1     CALL 0xb13e
6AF9  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6AFD  3e a0        LD A,0xa0
6AFF  28 02        JR z,0x6b03
6B01  3e b0        LD A,0xb0
6B03  c3 83 70     JP 0x7083
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[4]) ----
6551  dd e5        PUSH IX
6553  22 14 f0     LD (0xf014),HL
6556  21 00 00     LD HL,0
6559  22 12 f0     LD (0xf012),HL
655C  cd e3 65     CALL 0x65e3
655F  dd e1        POP IX
6561  c9           RET
6562  af           XOR A
6563  32 11 e6     LD (0xe611),A
6566  3a 10 e6     LD A,(0xe610)
6569  b7           OR A
656A  c8           RET z
656B  3a 19 f0     LD A,(0xf019)
656E  06 17        LD B,0x17
6570  21 84 65     LD HL,0x6584
6573  be           CP (HL)
6574  28 04        JR z,0x657a
6576  23           INC HL
6577  10 fa        DJNZ 0x6573
6579  c9           RET
657A  21 22 e6     LD HL,0xe622
657D  34           INC (HL)
657E  3e 01        LD A,1
6580  32 11 e6     LD (0xe611),A
6583  c9           RET
659B  af           XOR A
659C  32 25 e6     LD (0xe625),A
659F  3a 97 e0     LD A,(0xe097)
65A2  3d           DEC A
65A3  f8           RET m
65A4  21 bc 65     LD HL,0x65bc
65A7  cd 53 40     CALL 0x4053 ; read_word_table
65AA  eb           EX DE,HL
65AB  3a 19 f0     LD A,(0xf019)
65AE  47           LD B,A
65AF  7e           LD A,(HL)
65B0  b7           OR A
65B1  c8           RET z
65B2  23           INC HL
65B3  b8           CP B
65B4  20 f9        JR nz,0x65af
65B6  3e 80        LD A,0x80
65B8  32 25 e6     LD (0xe625),A
65BB  c9           RET
65E3  32 19 f0     LD (0xf019),A
65E6  ed 53 1a f0  LD (0xf01a),DE
65EA  78           LD A,B
65EB  32 24 e6     LD (0xe624),A
65EE  3a 35 e6     LD A,(0xe635)
65F1  47           LD B,A
65F2  3a 20 e6     LD A,(0xe620)
65F5  b8           CP B
65F6  d0           RET nc
65F7  3a 19 f0     LD A,(0xf019)
65FA  6f           LD L,A
65FB  cd af 67     CALL 0x67af
65FE  22 1c f0     LD (0xf01c),HL
6601  7e           LD A,(HL)
6602  cb 7f        BIT 7,A
6604  28 1c        JR z,0x6622
6606  e6 07        AND 7
6608  4f           LD C,A
6609  3a 36 e6     LD A,(0xe636)
660C  3c           INC A
660D  47           LD B,A
660E  3a 21 e6     LD A,(0xe621)
6611  81           ADD A,C
6612  b8           CP B
6613  d0           RET nc
6614  cd 8d 67     CALL 0x678d
6617  c0           RET nz
6618  3a 21 e6     LD A,(0xe621)
661B  81           ADD A,C
661C  32 21 e6     LD (0xe621),A
661F  c3 26 66     JP 0x6626
6622  cd 8d 67     CALL 0x678d
6625  c0           RET nz
6626  e5           PUSH HL
6627  dd e1        POP IX
6629  22 20 f0     LD (0xf020),HL
662C  21 20 e6     LD HL,0xe620
662F  34           INC (HL)
6630  cd 62 65     CALL 0x6562
6633  cd 9b 65     CALL 0x659b
6636  2a 20 f0     LD HL,(0xf020)
6639  3a 19 f0     LD A,(0xf019)
663C  77           LD (HL),A
663D  2c           INC L
663E  36 00        LD (HL),0
6640  2c           INC L
6641  3a 11 e6     LD A,(0xe611)
6644  b7           OR A
6645  06 f0        LD B,0xf0
6647  28 02        JR z,0x664b
6649  06 f8        LD B,0xf8
664B  70           LD (HL),B
664C  2c           INC L
664D  3a 12 e6     LD A,(0xe612)
6650  b7           OR A
6651  28 06        JR z,0x6659
6653  3a 13 e6     LD A,(0xe613)
6656  3d           DEC A
6657  f6 08        OR 8
6659  47           LD B,A
665A  3a 25 e6     LD A,(0xe625)
665D  b0           OR B
665E  77           LD (HL),A
665F  2c           INC L
6660  36 00        LD (HL),0
6662  ed 5b 1a f0  LD DE,(0xf01a)
6666  2c           INC L
6667  72           LD (HL),D
6668  2c           INC L
6669  36 00        LD (HL),0
666B  2c           INC L
666C  73           LD (HL),E
666D  2c           INC L
666E  36 00        LD (HL),0
6670  2c           INC L
6671  eb           EX DE,HL
6672  2a 1c f0     LD HL,(0xf01c)
6675  ed a0        LDI
6677  ed a0        LDI
6679  ed a0        LDI
667B  eb           EX DE,HL
667C  ed 4b 12 f0  LD BC,(0xf012)
6680  71           LD (HL),C
6681  2c           INC L
6682  70           LD (HL),B
6683  2c           INC L
6684  ed 4b 14 f0  LD BC,(0xf014)
6688  71           LD (HL),C
6689  2c           INC L
668A  70           LD (HL),B
668B  2c           INC L
668C  eb           EX DE,HL
668D  ed a0        LDI
668F  ed a0        LDI
6691  af           XOR A
6692  06 0e        LD B,14
6694  12           LD (DE),A
6695  1c           INC E
6696  10 fc        DJNZ 0x6694
6698  dd 7e 10     LD A,(IX+0x10) ; vida
669B  3c           INC A
669C  20 04        JR nz,0x66a2
669E  dd cb 02 ae  RES 5,(IX+2) ; flags
66A2  3c           INC A
66A3  20 04        JR nz,0x66a9
66A5  dd cb 02 d6  SET 2,(IX+2) ; flags
66A9  21 8b 67     LD HL,0x678b
66AC  e5           PUSH HL
66AD  3a 19 f0     LD A,(0xf019)
66B0  3d           DEC A
66B1  cd 50 40     CALL 0x4050 ; jump_table_a
66B4  50 79        .word 0x7950 ; caso 0
66B6  db 6c        .word 0x6CDB ; caso 1
66B8  fe ab        .word 0xABFE ; caso 2
66BA  5d 6f        .word 0x6F5D ; caso 3
66BC  e8 6a        .word 0x6AE8 ; caso 4
66BE  79 76        .word 0x7679 ; caso 5
66C0  30 a8        .word 0xA830 ; caso 6
66C2  fe 6d        .word 0x6DFE ; caso 7
66C4  0d 70        .word 0x700D ; caso 8
66C6  0d 7b        .word 0x7B0D ; caso 9
66C8  93 7a        .word 0x7A93 ; caso 10
66CA  97 9d        .word 0x9D97 ; caso 11
66CC  4b 78        .word 0x784B ; caso 12
66CE  18 7d        .word 0x7D18 ; caso 13
66D0  47 73        .word 0x7347 ; caso 14
66D2  c9 71        .word 0x71C9 ; caso 15
66D4  ba 73        .word 0x73BA ; caso 16
66D6  97 70        .word 0x7097 ; caso 17
66D8  e2 a3        .word 0xA3E2 ; caso 18
66DA  49 72        .word 0x7249 ; caso 19
66DC  05 8e        .word 0x8E05 ; caso 20
66DE  1f 8c        .word 0x8C1F ; caso 21
66E0  1c 95        .word 0x951C ; caso 22
66E2  50 6c        .word 0x6C50 ; caso 23
66E4  3d 86        .word 0x863D ; caso 24
66E6  32 6c        .word 0x6C32 ; caso 25
66E8  c2 6e        .word 0x6EC2 ; caso 26
66EA  55 6f        .word 0x6F55 ; caso 27
66EC  42 6c        .word 0x6C42 ; caso 28
66EE  ec 8c        .word 0x8CEC ; caso 29
66F0  2f 94        .word 0x942F ; caso 30
66F2  d4 93        .word 0x93D4 ; caso 31
66F4  89 7e        .word 0x7E89 ; caso 32
66F6  c0 80        .word 0x80C0 ; caso 33
66F8  98 a1        .word 0xA198 ; caso 34
66FA  30 9b        .word 0x9B30 ; caso 35
66FC  c0 98        .word 0x98C0 ; caso 36
66FE  2a a4        .word 0xA42A ; caso 37
6700  35 89        .word 0x8935 ; caso 38
6702  57 93        .word 0x9357 ; caso 39
6704  ba 92        .word 0x92BA ; caso 40
6706  9a 96        .word 0x969A ; caso 41
6708  da 87        .word 0x87DA ; caso 42
670A  74 a9        .word 0xA974 ; caso 43
670C  25 97        .word 0x9725 ; caso 44
670E  70 a0        .word 0xA070 ; caso 45
6710  fe 91        .word 0x91FE ; caso 46
6712  2e 7f        .word 0x7F2E ; caso 47
6714  32 8b        .word 0x8B32 ; caso 48
6716  78 90        .word 0x9078 ; caso 49
6718  43 91        .word 0x9143 ; caso 50
671A  f4 aa        .word 0xAAF4 ; caso 51
671C  88 ab        .word 0xAB88 ; caso 52
671E  39 8a        .word 0x8A39 ; caso 53
6720  81 86        .word 0x8681 ; caso 54
6722  a8 a7        .word 0xA7A8 ; caso 55
6724  df a8        .word 0xA8DF ; caso 56
6726  59 91        .word 0x9159 ; caso 57
6728  e0 91        .word 0x91E0 ; caso 58
672A  4c ae        .word 0xAE4C ; caso 59
672C  c4 ae        .word 0xAEC4 ; caso 60
672E  83 a2        .word 0xA283 ; caso 61
6730  90 98        .word 0x9890 ; caso 62
6732  04 8d        .word 0x8D04 ; caso 63
6734  7f 8d        .word 0x8D7F ; caso 64
6736  8d 83        .word 0x838D ; caso 65
6738  d5 84        .word 0x84D5 ; caso 66
673A  1e 9f        .word 0x9F1E ; caso 67
673C  ba 8f        .word 0x8FBA ; caso 68
673E  e6 8c        .word 0x8CE6 ; caso 69
6740  ac 95        .word 0x95AC ; caso 70
6742  42 6c        .word 0x6C42 ; caso 71
6744  e3 a0        .word 0xA0E3 ; caso 72
6746  fb 80        .word 0x80FB ; caso 73
6748  48 a5        .word 0xA548 ; caso 74
674A  ca 8f        .word 0x8FCA ; caso 75
674C  f7 8f        .word 0x8FF7 ; caso 76
674E  2a 82        .word 0x822A ; caso 77
6750  f9 82        .word 0x82F9 ; caso 78
6752  59 91        .word 0x9159 ; caso 79
6754  78 90        .word 0x9078 ; caso 80
6756  f1 a0        .word 0xA0F1 ; caso 81
6758  f1 a0        .word 0xA0F1 ; caso 82
675A  ed 6e        .word 0x6EED ; caso 83
675C  05 ad        .word 0xAD05 ; caso 84
675E  1e 9f        .word 0x9F1E ; caso 85
6760  51 ad        .word 0xAD51 ; caso 86
6762  b2 ad        .word 0xADB2 ; caso 87
6764  03 ae        .word 0xAE03 ; caso 88
6766  ba 74        .word 0x74BA ; caso 89
6768  05 6a        .word 0x6A05 ; caso 90
676A  be aa        .word 0xAABE ; caso 91
676C  aa 6a        .word 0x6AAA ; caso 92
676E  aa 6a        .word 0x6AAA ; caso 93
6770  aa 6a        .word 0x6AAA ; caso 94
6772  58 84        .word 0x8458 ; caso 95
6774  c5 84        .word 0x84C5 ; caso 96
6776  44 85        .word 0x8544 ; caso 97
6778  c4 6a        .word 0x6AC4 ; caso 98
677A  a3 aa        .word 0xAAA3 ; caso 99
677C  60 a5        .word 0xA560 ; caso 100
677E  8a 67        .word 0x678A ; caso 101
6780  e0 a5        .word 0xA5E0 ; caso 102
6782  3f 9f        .word 0x9F3F ; caso 103
6784  bb 74        .word 0x74BB ; caso 104
6786  37 79        .word 0x7937 ; caso 105
6788  b5 aa        .word 0xAAB5 ; caso 106
678A  c9           RET
678D  3a 19 f0     LD A,(0xf019)
6790  fe 43        CP 0x43
6792  28 11        JR z,0x67a5
6794  21 00 e8     LD HL,0xe800 ; enemy_slots
6797  3a 35 e6     LD A,(0xe635)
679A  47           LD B,A
679B  11 20 00     LD DE,0x20
679E  af           XOR A
679F  be           CP (HL)
67A0  c8           RET z
67A1  19           ADD HL,DE
67A2  10 fb        DJNZ 0x679f
67A4  c9           RET
67A5  21 00 e9     LD HL,0xe900
67A8  3a 35 e6     LD A,(0xe635)
67AB  d6 08        SUB 8
67AD  18 eb        JR 0x679a
67AF  4d           LD C,L
67B0  26 00        LD H,0
67B2  44           LD B,H
67B3  29           ADD HL,HL
67B4  29           ADD HL,HL
67B5  09           ADD HL,BC
67B6  01 b6 67     LD BC,0x67b6
67B9  09           ADD HL,BC
67BA  c9           RET
6A05  dd 36 08 03  LD (IX+8),3
6A09  dd 36 01 08  LD (IX+1),8 ; estado
6A0D  3a c7 e6     LD A,(0xe6c7)
6A10  dd 77 11     LD (IX+0x11),A ; atributo_sprite
6A13  dd 36 02 80  LD (IX+2),0x80 ; flags
6A17  c9           RET
6AAA  dd 36 02 c0  LD (IX+2),0xc0 ; flags
6AAE  dd 6e 05     LD L,(IX+5) ; y
6AB1  dd 66 07     LD H,(IX+7) ; x
6AB4  cd 56 40     CALL 0x4056 ; vec_4da1
6AB7  dd 75 12     LD (IX+0x12),L
6ABA  dd 74 13     LD (IX+0x13),H
6ABD  3a 24 e6     LD A,(0xe624)
6AC0  dd 77 18     LD (IX+0x18),A
6AC3  c9           RET
6AC4  dd 36 08 0a  LD (IX+8),10
6AC8  c3 13 6a     JP 0x6a13
6AE8  dd 36 15 78  LD (IX+0x15),0x78
6AEC  ed 5f        LD A,R
6AEE  e6 01        AND 1
6AF0  11 00 01     LD DE,0x100
6AF3  c4 68 40     CALL nz,0x4068 ; neg_de
6AF6  cd 3e b1     CALL 0xb13e
6AF9  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6AFD  3e a0        LD A,0xa0
6AFF  28 02        JR z,0x6b03
6B01  3e b0        LD A,0xb0
6B03  c3 83 70     JP 0x7083
6B06  21 80 b0     LD HL,0xb080
6B09  e5           PUSH HL
6B0A  dd 7e 01     LD A,(IX+1) ; estado
6B0D  3d           DEC A
6B0E  28 6b        JR z,0x6b7b
6B10  3d           DEC A
6B11  ca a9 6b     JP z,0x6ba9
6B14  f2 da 6b     JP p,0x6bda
6B17  cd 25 6c     CALL 0x6c25
6B1A  38 4d        JR c,0x6b69
6B1C  cd ee 6b     CALL 0x6bee
6B1F  d2 63 6b     JP nc,0x6b63
6B22  cd 04 6c     CALL 0x6c04
6B25  da 63 6b     JP c,0x6b63
6B28  dd 35 15     DEC (IX+0x15)
6B2B  28 1f        JR z,0x6b4c
6B2D  dd 34 1f     INC (IX+0x1f)
6B30  dd 7e 1f     LD A,(IX+0x1f)
6B33  47           LD B,A
6B34  e6 07        AND 7
6B36  c0           RET nz
6B37  11 a4 a0     LD DE,0xa0a4
6B3A  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6B3E  28 03        JR z,0x6b43
6B40  11 b4 b0     LD DE,0xb0b4
6B43  cb 58        BIT 3,B
6B45  28 01        JR z,0x6b48
6B47  53           LD D,E
6B48  dd 72 0a     LD (IX+10),D ; d1
6B4B  c9           RET
6B4C  dd 36 15 0a  LD (IX+0x15),10
6B50  dd 34 01     INC (IX+1) ; estado
6B53  cd 71 6b     CALL 0x6b71
6B56  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6B5A  3e a8        LD A,0xa8
6B5C  28 02        JR z,0x6b60
6B5E  3e b8        LD A,0xb8
6B60  c3 83 70     JP 0x7083
6B63  cd 5f b1     CALL 0xb15f
6B66  c3 f9 6a     JP 0x6af9
6B69  dd 36 01 02  LD (IX+1),2 ; estado
6B6D  dd 36 08 78  LD (IX+8),0x78
6B71  dd cb 02 a6  RES 4,(IX+2) ; flags
6B75  c9           RET
6B76  dd cb 02 e6  SET 4,(IX+2) ; flags
6B7A  c9           RET
6B7B  dd 35 15     DEC (IX+0x15)
6B7E  c0           RET nz
6B7F  dd 36 15 78  LD (IX+0x15),0x78
6B83  dd 35 01     DEC (IX+1) ; estado
6B86  cd 76 6b     CALL 0x6b76
6B89  cd f9 6a     CALL 0x6af9
6B8C  06 00        LD B,0
6B8E  21 00 04     LD HL,0x400
6B91  dd cb 0f 7e  BIT 7,(IX+15) ; vx
6B95  28 04        JR z,0x6b9b
6B97  04           INC B
6B98  21 00 fc     LD HL,0xfc00
6B9B  dd 5e 07     LD E,(IX+7) ; x
6B9E  dd 7e 05     LD A,(IX+5) ; y
6BA1  c6 04        ADD A,4
6BA3  57           LD D,A
6BA4  3e 1a        LD A,0x1a
6BA6  c3 51 65     JP 0x6551
6BA9  dd 35 08     DEC (IX+8)
6BAC  28 19        JR z,0x6bc7
6BAE  dd 7e 08     LD A,(IX+8)
6BB1  e6 1f        AND 0x1f
6BB3  28 d7        JR z,0x6b8c
6BB5  3a 07 e5     LD A,(0xe507) ; jugador_x
6BB8  dd be 07     CP (IX+7) ; x
6BBB  11 00 01     LD DE,0x100
6BBE  dc 68 40     CALL c,0x4068 ; neg_de
6BC1  cd 3e b1     CALL 0xb13e
6BC4  c3 56 6b     JP 0x6b56
6BC7  dd 36 01 03  LD (IX+1),3 ; estado
6BCB  dd 36 08 3c  LD (IX+8),0x3c
6BCF  cd 76 6b     CALL 0x6b76
6BD2  c3 f9 6a     JP 0x6af9
6BD5  dd 36 01 00  LD (IX+1),0 ; estado
6BD9  c9           RET
6BDA  dd 35 08     DEC (IX+8)
6BDD  28 f6        JR z,0x6bd5
6BDF  cd ee 6b     CALL 0x6bee
6BE2  d2 63 6b     JP nc,0x6b63
6BE5  cd 04 6c     CALL 0x6c04
6BE8  da 63 6b     JP c,0x6b63
6BEB  c3 2d 6b     JP 0x6b2d
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
6C25  3a 05 e5     LD A,(0xe505) ; jugador_y
6C28  d6 10        SUB 0x10
6C2A  dd 96 05     SUB (IX+5) ; y
6C2D  c6 04        ADD A,4
6C2F  fe 09        CP 9
6C31  c9           RET
6C32  cd c5 8f     CALL 0x8fc5
6C35  dd 36 08 20  LD (IX+8),0x20
6C39  3a 24 e6     LD A,(0xe624)
6C3C  b7           OR A
6C3D  c8           RET z
6C3E  dd 36 0a bc  LD (IX+10),0xbc ; d1
6C42  dd cb 03 f6  SET 6,(IX+3) ; flags2
6C46  c9           RET
6C50  ed 5f        LD A,R
6C52  e6 01        AND 1
6C54  dd 77 14     LD (IX+0x14),A
6C57  11 80 00     LD DE,0x80
6C5A  c3 8c 96     JP 0x968c
6CDB  21 be 6d     LD HL,0x6dbe
6CDE  cd 7f 6d     CALL 0x6d7f
6CE1  dd 36 1c 11  LD (IX+0x1c),0x11
6CE5  dd 36 08 02  LD (IX+8),2
6CE9  cd f1 81     CALL 0x81f1
6CEC  e6 2f        AND 0x2f
6CEE  c6 50        ADD A,0x50
6CF0  dd 77 1d     LD (IX+0x1d),A
6CF3  c9           RET
6D7F  dd 36 1b 01  LD (IX+0x1b),1
6D83  06 07        LD B,7
6D85  dd e5        PUSH IX
6D87  d1           POP DE
6D88  7b           LD A,E
6D89  c6 13        ADD A,0x13
6D8B  5f           LD E,A
6D8C  13           INC DE
6D8D  3e 08        LD A,8
6D8F  cd 62 40     CALL 0x4062 ; add_a_to_hl
6D92  e5           PUSH HL
6D93  dd 7e 01     LD A,(IX+1) ; estado
6D96  cd 83 40     CALL 0x4083
6D99  12           LD (DE),A
6D9A  e1           POP HL
6D9B  10 ef        DJNZ 0x6d8c
6D9D  cd 24 b1     CALL 0xb124
6DA0  dd 7e 01     LD A,(IX+1) ; estado
6DA3  fe 04        CP 4
6DA5  30 0b        JR nc,0x6db2
6DA7  cb 47        BIT 0,A
6DA9  11 00 01     LD DE,0x100
6DAC  c4 68 40     CALL nz,0x4068 ; neg_de
6DAF  c3 3e b1     JP 0xb13e
6DB2  cb 47        BIT 0,A
6DB4  11 00 01     LD DE,0x100
6DB7  c4 68 40     CALL nz,0x4068 ; neg_de
6DBA  c3 37 b1     JP 0xb137
6DFE  3a 04 e6     LD A,(0xe604)
6E01  e6 7f        AND 0x7f
6E03  c6 40        ADD A,0x40
6E05  dd 77 07     LD (IX+7),A ; x
6E08  ed 5f        LD A,R
6E0A  e6 3f        AND 0x3f
6E0C  c6 30        ADD A,0x30
6E0E  dd 77 08     LD (IX+8),A
6E11  11 00 fc     LD DE,0xfc00
6E14  cd 37 b1     CALL 0xb137
6E17  cd 90 6e     CALL 0x6e90
6E1A  dd 36 13 08  LD (IX+0x13),8
6E1E  3a 04 e6     LD A,(0xe604)
6E21  01 d4 cc     LD BC,0xccd4
6E24  cb 5f        BIT 3,A
6E26  28 03        JR z,0x6e2b
6E28  01 d8 d0     LD BC,0xd0d8
6E2B  dd 70 0a     LD (IX+10),B ; d1
6E2E  dd 71 12     LD (IX+0x12),C
6E31  c9           RET
6E90  dd 56 05     LD D,(IX+5) ; y
6E93  06 01        LD B,1
6E95  c3 b7 78     JP 0x78b7
6EC2  3a 41 e0     LD A,(0xe041) ; world
6EC5  21 e5 6e     LD HL,0x6ee5
6EC8  fe 0a        CP 10
6ECA  30 03        JR nc,0x6ecf
6ECC  21 dd 6e     LD HL,0x6edd
6ECF  3a 24 e6     LD A,(0xe624)
6ED2  cd 83 40     CALL 0x4083
6ED5  dd 77 08     LD (IX+8),A
6ED8  01 13 6f     LD BC,0x6f13
6EDB  18 13        JR 0x6ef0
6EED  01 43 6f     LD BC,0x6f43
6EF0  3a 24 e6     LD A,(0xe624)
6EF3  6f           LD L,A
6EF4  26 00        LD H,0
6EF6  29           ADD HL,HL
6EF7  54           LD D,H
6EF8  5d           LD E,L
6EF9  29           ADD HL,HL
6EFA  19           ADD HL,DE
6EFB  09           ADD HL,BC
6EFC  5e           LD E,(HL)
6EFD  23           INC HL
6EFE  56           LD D,(HL)
6EFF  cd 3e b1     CALL 0xb13e
6F02  23           INC HL
6F03  5e           LD E,(HL)
6F04  23           INC HL
6F05  56           LD D,(HL)
6F06  cd 37 b1     CALL 0xb137
6F09  23           INC HL
6F0A  5e           LD E,(HL)
6F0B  23           INC HL
6F0C  56           LD D,(HL)
6F0D  cd 45 b1     CALL 0xb145
6F10  c3 42 6c     JP 0x6c42
6F55  dd 36 01 02  LD (IX+1),2 ; estado
6F59  3e 13        LD A,0x13
6F5B  18 02        JR 0x6f5f
6F5D  3e 12        LD A,0x12
6F5F  dd 77 0a     LD (IX+10),A ; d1
6F62  dd 36 14 00  LD (IX+0x14),0
6F66  cd 71 6b     CALL 0x6b71
6F69  dd 6e 05     LD L,(IX+5) ; y
6F6C  dd 66 07     LD H,(IX+7) ; x
6F6F  cd 56 40     CALL 0x4056 ; vec_4da1
6F72  dd 75 12     LD (IX+0x12),L
6F75  dd 74 13     LD (IX+0x13),H
6F78  3e 0f        LD A,15
6F7A  dd 77 0c     LD (IX+12),A ; vy_lo
6F7D  c3 cf 71     JP 0x71cf
700D  11 80 00     LD DE,0x80
7010  cd 45 b1     CALL 0xb145
7013  ed 5f        LD A,R
7015  cb 47        BIT 0,A
7017  11 00 01     LD DE,0x100
701A  c4 68 40     CALL nz,0x4068 ; neg_de
701D  c3 3e b1     JP 0xb13e
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
7097  ed 5f        LD A,R
7099  e6 3f        AND 0x3f
709B  c6 60        ADD A,0x60
709D  dd 77 08     LD (IX+8),A
70A0  cd 71 6b     CALL 0x6b71
70A3  dd cb 02 d6  SET 2,(IX+2) ; flags
70A7  c9           RET
71C9  cd 71 6b     CALL 0x6b71
71CC  dd 7e 07     LD A,(IX+7) ; x
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
7249  cd f5 96     CALL 0x96f5
724C  ed 5f        LD A,R
724E  e6 3f        AND 0x3f
7250  c6 80        ADD A,0x80
7252  c3 cf 71     JP 0x71cf
7347  ed 5f        LD A,R
7349  e6 01        AND 1
734B  11 00 01     LD DE,0x100
734E  06 33        LD B,0x33
7350  28 04        JR z,0x7356
7352  11 00 ff     LD DE,0xff00
7355  05           DEC B
7356  cd 3e b1     CALL 0xb13e
7359  21 aa 73     LD HL,0x73aa
735C  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7360  28 03        JR z,0x7365
7362  21 b2 73     LD HL,0x73b2
7365  dd 7e 1e     LD A,(IX+0x1e)
7368  cd 53 40     CALL 0x4053 ; read_word_table
736B  dd 36 08 04  LD (IX+8),4
736F  dd 73 0a     LD (IX+10),E ; d1
7372  dd 72 11     LD (IX+0x11),D ; atributo_sprite
7375  c9           RET
73BA  dd 36 15 78  LD (IX+0x15),0x78
73BE  ed 5f        LD A,R
73C0  e6 01        AND 1
73C2  11 00 02     LD DE,0x200
73C5  c4 68 40     CALL nz,0x4068 ; neg_de
73C8  cd 3e b1     CALL 0xb13e
73CB  dd 36 11 58  LD (IX+0x11),0x58 ; atributo_sprite
73CF  dd cb 0f 7e  BIT 7,(IX+15) ; vx
73D3  3e a4        LD A,0xa4
73D5  28 02        JR z,0x73d9
73D7  3e b4        LD A,0xb4
73D9  c3 83 70     JP 0x7083
74BA  c9           RET
74BB  ed 4b 41 e0  LD BC,(0xe041) ; world
74BF  79           LD A,C
74C0  fe 0a        CP 10
74C2  20 0a        JR nz,0x74ce
74C4  78           LD A,B
74C5  fe 19        CP 0x19
74C7  20 05        JR nz,0x74ce
74C9  3e 01        LD A,1
74CB  dd 77 1d     LD (IX+0x1d),A
74CE  dd cb 02 d6  SET 2,(IX+2) ; flags
74D2  3a 2a e6     LD A,(0xe62a)
74D5  dd 77 15     LD (IX+0x15),A
74D8  3c           INC A
74D9  32 2a e6     LD (0xe62a),A
74DC  3d           DEC A
74DD  20 0d        JR nz,0x74ec
74DF  32 2b e6     LD (0xe62b),A
74E2  dd 77 18     LD (IX+0x18),A
74E5  dd 36 16 28  LD (IX+0x16),0x28 ; ay_lo
74E9  cd 46 76     CALL 0x7646
74EC  dd 7e 05     LD A,(IX+5) ; y
74EF  dd 77 1b     LD (IX+0x1b),A
74F2  dd 36 08 08  LD (IX+8),8
74F6  11 80 00     LD DE,0x80
74F9  cd f1 81     CALL 0x81f1
74FC  e6 01        AND 1
74FE  c4 68 40     CALL nz,0x4068 ; neg_de
7501  cd 37 b1     CALL 0xb137
7504  11 20 00     LD DE,0x20
7507  ed 5f        LD A,R
7509  e6 01        AND 1
750B  c4 68 40     CALL nz,0x4068 ; neg_de
750E  c3 3e b1     JP 0xb13e
7646  dd 7e 18     LD A,(IX+0x18)
7649  a7           AND A
764A  06 03        LD B,3
764C  20 02        JR nz,0x7650
764E  06 07        LD B,7
7650  cd f1 81     CALL 0x81f1
7653  a0           AND B
7654  20 01        JR nz,0x7657
7656  78           LD A,B
7657  dd 77 19     LD (IX+0x19),A
765A  c9           RET
7679  dd cb 02 d6  SET 2,(IX+2) ; flags
767D  dd 36 14 00  LD (IX+0x14),0
7681  cd 24 b1     CALL 0xb124
7684  3a 24 e6     LD A,(0xe624)
7687  47           LD B,A
7688  dd 7e 07     LD A,(IX+7) ; x
768B  fe 27        CP 0x27
768D  38 0d        JR c,0x769c
768F  fe d5        CP 0xd5
7691  38 12        JR c,0x76a5
7693  78           LD A,B
7694  fe 05        CP 5
7696  38 0e        JR c,0x76a6
7698  d6 05        SUB 5
769A  18 0a        JR 0x76a6
769C  78           LD A,B
769D  fe 05        CP 5
769F  30 05        JR nc,0x76a6
76A1  c6 05        ADD A,5
76A3  18 01        JR 0x76a6
76A5  78           LD A,B
76A6  87           ADD A,A
76A7  87           ADD A,A
76A8  87           ADD A,A
76A9  21 fb 77     LD HL,0x77fb
76AC  cd 83 40     CALL 0x4083
76AF  dd 74 1e     LD (IX+0x1e),H
76B2  dd 75 1f     LD (IX+0x1f),L
76B5  dd 77 0a     LD (IX+10),A ; d1
76B8  dd 36 16 01  LD (IX+0x16),1 ; ay_lo
76BC  dd 7e 05     LD A,(IX+5) ; y
76BF  dd 77 12     LD (IX+0x12),A
76C2  dd 7e 07     LD A,(IX+7) ; x
76C5  dd 77 13     LD (IX+0x13),A
76C8  3e 07        LD A,7
76CA  c3 59 40     JP 0x4059 ; sound_request
784B  ed 5f        LD A,R
784D  e6 01        AND 1
784F  dd 77 14     LD (IX+0x14),A
7852  ed 5f        LD A,R
7854  cb 57        BIT 2,A
7856  20 27        JR nz,0x787f
7858  dd 7e 14     LD A,(IX+0x14)
785B  06 f0        LD B,0xf0
785D  a7           AND A
785E  28 02        JR z,0x7862
7860  06 10        LD B,0x10
7862  3a 07 e5     LD A,(0xe507) ; jugador_x
7865  80           ADD A,B
7866  cd f2 78     CALL 0x78f2
7869  3a 05 e5     LD A,(0xe505) ; jugador_y
786C  06 00        LD B,0
786E  fe 3d        CP 0x3d
7870  38 17        JR c,0x7889
7872  04           INC B
7873  fe 5d        CP 0x5d
7875  38 12        JR c,0x7889
7877  04           INC B
7878  fe 7d        CP 0x7d
787A  38 0d        JR c,0x7889
787C  04           INC B
787D  18 0a        JR 0x7889
787F  ed 5f        LD A,R
7881  cd f2 78     CALL 0x78f2
7884  ed 5f        LD A,R
7886  e6 03        AND 3
7888  47           LD B,A
7889  dd 70 1c     LD (IX+0x1c),B
788C  78           LD A,B
788D  21 27 79     LD HL,0x7927
7890  cd 53 40     CALL 0x4053 ; read_word_table
7893  cd 8c 96     CALL 0x968c
7896  78           LD A,B
7897  21 2f 79     LD HL,0x792f
789A  cd 53 40     CALL 0x4053 ; read_word_table
789D  cd 37 b1     CALL 0xb137
78A0  11 80 00     LD DE,0x80
78A3  cd 45 b1     CALL 0xb145
78A6  dd cb 14 46  BIT 0,(IX+0x14)
78AA  3e c8        LD A,0xc8
78AC  28 02        JR z,0x78b0
78AE  3e d0        LD A,0xd0
78B0  dd 77 0a     LD (IX+10),A ; d1
78B3  16 90        LD D,0x90
78B5  06 00        LD B,0
78B7  dd 5e 07     LD E,(IX+7) ; x
78BA  3e 6a        LD A,0x6a
78BC  cd 51 65     CALL 0x6551
78BF  dd 7e 00     LD A,(IX) ; tipo
78C2  fe 30        CP 0x30
78C4  3e 24        LD A,0x24
78C6  28 02        JR z,0x78ca
78C8  3e 0d        LD A,13
78CA  c3 59 40     JP 0x4059 ; sound_request
78F2  f5           PUSH AF
78F3  3a 24 e6     LD A,(0xe624)
78F6  3d           DEC A
78F7  21 1f 79     LD HL,0x791f
78FA  cd 53 40     CALL 0x4053 ; read_word_table
78FD  7a           LD A,D
78FE  c6 30        ADD A,0x30
7900  47           LD B,A
7901  7b           LD A,E
7902  d6 18        SUB 0x18
7904  4f           LD C,A
7905  f1           POP AF
7906  ba           CP D
7907  38 06        JR c,0x790f
7909  bb           CP E
790A  38 04        JR c,0x7910
790C  7b           LD A,E
790D  18 01        JR 0x7910
790F  7a           LD A,D
7910  dd 77 07     LD (IX+7),A ; x
7913  b8           CP B
7914  1e 00        LD E,0
7916  38 03        JR c,0x791b
7918  b9           CP C
7919  d8           RET c
791A  1c           INC E
791B  dd 73 14     LD (IX+0x14),E
791E  c9           RET
7937  dd 36 08 0a  LD (IX+8),10
793B  cd 71 6b     CALL 0x6b71
793E  3a 24 e6     LD A,(0xe624)
7941  a7           AND A
7942  c8           RET z
7943  3d           DEC A
7944  28 05        JR z,0x794b
7946  dd 36 0a c0  LD (IX+10),0xc0 ; d1
794A  c9           RET
794B  dd 36 0b 09  LD (IX+11),9 ; d2
794F  c9           RET
7950  ed 5f        LD A,R
7952  e6 01        AND 1
7954  11 80 01     LD DE,0x180
7957  c4 68 40     CALL nz,0x4068 ; neg_de
795A  cd 3e b1     CALL 0xb13e
795D  cd 3d 7a     CALL 0x7a3d
7960  dd cb 0f 7e  BIT 7,(IX+15) ; vx
7964  01 b0 a0     LD BC,0xa0b0
7967  28 03        JR z,0x796c
7969  01 c4 b4     LD BC,0xb4c4
796C  cd 2b 6e     CALL 0x6e2b
796F  dd 36 13 06  LD (IX+0x13),6
7973  dd 36 11 00  LD (IX+0x11),0 ; atributo_sprite
7977  c9           RET
7A3D  dd 7e 09     LD A,(IX+9) ; d0
7A40  ee 03        XOR 3
7A42  dd 77 09     LD (IX+9),A ; d0
7A45  c9           RET
7A93  dd 36 08 08  LD (IX+8),8
7A97  ed 5f        LD A,R
7A99  e6 01        AND 1
7A9B  3e a0        LD A,0xa0
7A9D  06 3f        LD B,0x3f
7A9F  11 00 04     LD DE,0x400
7AA2  28 07        JR z,0x7aab
7AA4  3e a8        LD A,0xa8
7AA6  06 05        LD B,5
7AA8  11 00 fc     LD DE,0xfc00
7AAB  dd 77 0a     LD (IX+10),A ; d1
7AAE  dd 70 11     LD (IX+0x11),B ; atributo_sprite
7AB1  cd 3e b1     CALL 0xb13e
7AB4  ed 5f        LD A,R
7AB6  e6 2f        AND 0x2f
7AB8  dd 77 15     LD (IX+0x15),A
7ABB  c6 50        ADD A,0x50
7ABD  dd 77 14     LD (IX+0x14),A
7AC0  c9           RET
7B0D  3e 01        LD A,1
7B0F  32 2e e6     LD (0xe62e),A
7B12  dd cb 02 ce  SET 1,(IX+2) ; flags
7B16  cd f2 9a     CALL 0x9af2
7B19  cd a9 7c     CALL 0x7ca9
7B1C  11 50 00     LD DE,0x50
7B1F  cd 45 b1     CALL 0xb145
7B22  dd 36 18 05  LD (IX+0x18),5
7B26  dd cb 1d 46  BIT 0,(IX+0x1d)
7B2A  11 70 00     LD DE,0x70
7B2D  c4 68 40     CALL nz,0x4068 ; neg_de
7B30  cd 3e b1     CALL 0xb13e
7B33  dd cb 1d 46  BIT 0,(IX+0x1d)
7B37  3e b0        LD A,0xb0
7B39  01 aa bc     LD BC,0xbcaa
7B3C  16 4e        LD D,0x4e
7B3E  28 0a        JR z,0x7b4a
7B40  3e c0        LD A,0xc0
7B42  01 b2 cc     LD BC,0xccb2
7B45  14           INC D
7B46  dd cb 02 c6  SET 0,(IX+2) ; flags
7B4A  1e 07        LD E,7
7B4C  c3 f9 98     JP 0x98f9
7CA9  3a 07 e5     LD A,(0xe507) ; jugador_x
7CAC  dd be 07     CP (IX+7) ; x
7CAF  3f           CCF
7CB0  3e 00        LD A,0
7CB2  17           RLA
7CB3  dd 77 1d     LD (IX+0x1d),A
7CB6  c9           RET
7D18  dd 36 08 04  LD (IX+8),4
7D1C  dd 36 18 04  LD (IX+0x18),4
7D20  dd 36 16 0b  LD (IX+0x16),11 ; ay_lo
7D24  cd 9e 7d     CALL 0x7d9e
7D27  cd 03 a1     CALL 0xa103
7D2A  cd 17 7e     CALL 0x7e17
7D2D  d0           RET nc
7D2E  cd 06 97     CALL 0x9706
7D31  d0           RET nc
7D32  dd 36 0a a0  LD (IX+10),0xa0 ; d1
7D36  dd 34 19     INC (IX+0x19)
7D39  c9           RET
7D9E  cd f1 81     CALL 0x81f1
7DA1  e6 1f        AND 0x1f
7DA3  c6 60        ADD A,0x60
7DA5  dd 77 17     LD (IX+0x17),A ; ay
7DA8  cd f1 81     CALL 0x81f1
7DAB  e6 0f        AND 15
7DAD  c6 10        ADD A,0x10
7DAF  dd 77 15     LD (IX+0x15),A
7DB2  c9           RET
7E17  dd 66 07     LD H,(IX+7) ; x
7E1A  dd 6e 05     LD L,(IX+5) ; y
7E1D  2d           DEC L
7E1E  cd 6e 40     CALL 0x406e ; vec_4dc8
7E21  d0           RET nc
7E22  c3 71 40     JP 0x4071
7E25  f5           PUSH AF
7E26  21 49 7e     LD HL,0x7e49
7E29  cd 53 40     CALL 0x4053 ; read_word_table
7E2C  21 00 00     LD HL,0
7E2F  c5           PUSH BC
7E30  19           ADD HL,DE
7E31  10 fd        DJNZ 0x7e30
7E33  eb           EX DE,HL
7E34  cd 3e b1     CALL 0xb13e
7E37  c1           POP BC
7E38  f1           POP AF
7E39  21 69 7e     LD HL,0x7e69
7E3C  cd 53 40     CALL 0x4053 ; read_word_table
7E3F  21 00 00     LD HL,0
7E42  19           ADD HL,DE
7E43  10 fd        DJNZ 0x7e42
7E45  eb           EX DE,HL
7E46  c3 37 b1     JP 0xb137
7E89  cd f1 81     CALL 0x81f1
7E8C  d6 10        SUB 0x10
7E8E  e6 df        AND 0xdf
7E90  c6 10        ADD A,0x10
7E92  dd 77 07     LD (IX+7),A ; x
7E95  fe 80        CP 0x80
7E97  06 00        LD B,0
7E99  38 01        JR c,0x7e9c
7E9B  04           INC B
7E9C  dd 70 14     LD (IX+0x14),B
7E9F  dd 7e 14     LD A,(IX+0x14)
7EA2  a7           AND A
7EA3  11 00 01     LD DE,0x100
7EA6  3e a0        LD A,0xa0
7EA8  28 05        JR z,0x7eaf
7EAA  11 00 ff     LD DE,0xff00
7EAD  3e a8        LD A,0xa8
7EAF  cd 3e b1     CALL 0xb13e
7EB2  c3 83 70     JP 0x7083
7F2E  ed 5f        LD A,R
7F30  e6 03        AND 3
7F32  dd 77 1c     LD (IX+0x1c),A
7F35  cd f1 81     CALL 0x81f1
7F38  e6 bf        AND 0xbf
7F3A  c6 20        ADD A,0x20
7F3C  dd 77 07     LD (IX+7),A ; x
7F3F  fe 80        CP 0x80
7F41  06 00        LD B,0
7F43  38 01        JR c,0x7f46
7F45  04           INC B
7F46  dd 70 14     LD (IX+0x14),B
7F49  11 00 fc     LD DE,0xfc00
7F4C  cd 37 b1     CALL 0xb137
7F4F  11 80 00     LD DE,0x80
7F52  cd 45 b1     CALL 0xb145
7F55  cd 1c 80     CALL 0x801c
7F58  dd 36 18 02  LD (IX+0x18),2
7F5C  cd b7 80     CALL 0x80b7
7F5F  dd cb 14 46  BIT 0,(IX+0x14)
7F63  01 c8 c4     LD BC,0xc4c8
7F66  28 03        JR z,0x7f6b
7F68  01 d4 d8     LD BC,0xd8d4
7F6B  dd 36 13 09  LD (IX+0x13),9
7F6F  c3 2b 6e     JP 0x6e2b
801C  dd 7e 07     LD A,(IX+7) ; x
801F  c6 04        ADD A,4
8021  5f           LD E,A
8022  16 90        LD D,0x90
8024  06 02        LD B,2
8026  c3 ba 78     JP 0x78ba
804C  dd 7e 05     LD A,(IX+5) ; y
804F  c6 10        ADD A,0x10
8051  6f           LD L,A
8052  dd 7e 00     LD A,(IX) ; tipo
8055  fe 30        CP 0x30
8057  28 06        JR z,0x805f
8059  dd cb 0f 7e  BIT 7,(IX+15) ; vx
805D  18 04        JR 0x8063
805F  dd cb 14 46  BIT 0,(IX+0x14)
8063  3e 20        LD A,0x20
8065  28 01        JR z,0x8068
8067  af           XOR A
8068  dd 86 07     ADD A,(IX+7) ; x
806B  67           LD H,A
806C  c3 6e 40     JP 0x406e ; vec_4dc8
806F  01 df ff     LD BC,0xffdf
8072  09           ADD HL,BC
8073  3e b9        LD A,0xb9
8075  be           CP (HL)
8076  d8           RET c
8077  dd 7e 07     LD A,(IX+7) ; x
807A  dd cb 0f 7e  BIT 7,(IX+15) ; vx
807E  28 03        JR z,0x8083
8080  fe 10        CP 0x10
8082  c9           RET
8083  fe e0        CP 0xe0
8085  3f           CCF
8086  c9           RET
80B7  dd 36 1e 00  LD (IX+0x1e),0
80BB  c9           RET
80C0  dd 36 08 40  LD (IX+8),0x40
80C4  dd 36 15 04  LD (IX+0x15),4
80C8  c9           RET
80FB  cd 42 6c     CALL 0x6c42
80FE  11 00 fc     LD DE,0xfc00
8101  cd 37 b1     CALL 0xb137
8104  cd f1 81     CALL 0x81f1
8107  e6 03        AND 3
8109  21 41 81     LD HL,0x8141
810C  cd 62 40     CALL 0x4062 ; add_a_to_hl
810F  56           LD D,(HL)
8110  1e 00        LD E,0
8112  cd f1 81     CALL 0x81f1
8115  e6 01        AND 1
8117  28 04        JR z,0x811d
8119  7a           LD A,D
811A  ed 44        NEG
811C  57           LD D,A
811D  cd 3e b1     CALL 0xb13e
8120  cd f1 81     CALL 0x81f1
8123  e6 03        AND 3
8125  1f           RRA
8126  1f           RRA
8127  1f           RRA
8128  1f           RRA
8129  e6 0f        AND 15
812B  c6 40        ADD A,0x40
812D  dd 77 14     LD (IX+0x14),A
8130  c9           RET
81F1  ed 5f        LD A,R
81F3  0f           RRCA
81F4  0f           RRCA
81F5  0f           RRCA
81F6  21 03 e0     LD HL,0xe003 ; frame_counter
81F9  ae           XOR (HL)
81FA  c9           RET
822A  dd 36 1a 02  LD (IX+0x1a),2
822E  dd 7e 02     LD A,(IX+2) ; flags
8231  e6 9f        AND 0x9f
8233  dd 77 02     LD (IX+2),A ; flags
8236  dd 36 0a a0  LD (IX+10),0xa0 ; d1
823A  dd 36 18 70  LD (IX+0x18),0x70
823E  cd f1 81     CALL 0x81f1
8241  e6 0f        AND 15
8243  dd 77 19     LD (IX+0x19),A
8246  c9           RET
82F9  dd 36 09 81  LD (IX+9),0x81 ; d0
82FD  dd cb 02 ae  RES 5,(IX+2) ; flags
8301  dd cb 02 b6  RES 6,(IX+2) ; flags
8305  dd 36 19 50  LD (IX+0x19),0x50
8309  dd 36 1a 02  LD (IX+0x1a),2
830D  dd 36 18 00  LD (IX+0x18),0
8311  c3 24 b1     JP 0xb124
838D  3e 01        LD A,1
838F  32 03 e6     LD (0xe603),A
8392  cd 24 b1     CALL 0xb124
8395  dd 36 16 a0  LD (IX+0x16),0xa0 ; ay_lo
8399  af           XOR A
839A  32 28 e6     LD (0xe628),A
839D  cd f1 81     CALL 0x81f1
83A0  e6 03        AND 3
83A2  21 be 83     LD HL,0x83be
83A5  cd 83 40     CALL 0x4083
83A8  dd 77 07     LD (IX+7),A ; x
83AB  11 80 ff     LD DE,0xff80
83AE  cd 37 b1     CALL 0xb137
83B1  dd 7e 02     LD A,(IX+2) ; flags
83B4  e6 0f        AND 15
83B6  dd 77 02     LD (IX+2),A ; flags
83B9  dd 36 14 20  LD (IX+0x14),0x20
83BD  c9           RET
8458  dd 7e 09     LD A,(IX+9) ; d0
845B  e6 fa        AND 0xfa
845D  dd 77 09     LD (IX+9),A ; d0
8460  dd 36 16 a0  LD (IX+0x16),0xa0 ; ay_lo
8464  dd 36 15 00  LD (IX+0x15),0
8468  3e 17        LD A,0x17
846A  cd 59 40     CALL 0x4059 ; sound_request
846D  21 00 fc     LD HL,0xfc00
8470  11 00 00     LD DE,0
8473  c3 33 b1     JP 0xb133
84C5  cd 42 6c     CALL 0x6c42
84C8  3e 40        LD A,0x40
84CA  cd 8f b1     CALL 0xb18f
84CD  cd 33 b1     CALL 0xb133
84D0  3e 0f        LD A,15
84D2  c3 59 40     JP 0x4059 ; sound_request
84D5  3e 01        LD A,1
84D7  32 03 e6     LD (0xe603),A
84DA  dd 77 14     LD (IX+0x14),A
84DD  dd 36 12 d8  LD (IX+0x12),0xd8
84E1  dd 36 13 03  LD (IX+0x13),3
84E5  c3 23 86     JP 0x8623
8544  dd 36 09 81  LD (IX+9),0x81 ; d0
8548  dd cb 02 a6  RES 4,(IX+2) ; flags
854C  01 c4 fc     LD BC,0xfcc4
854F  21 00 fc     LD HL,0xfc00
8552  16 00        LD D,0
8554  dd 7e 05     LD A,(IX+5) ; y
8557  fe 30        CP 0x30
8559  38 0c        JR c,0x8567
855B  fe a0        CP 0xa0
855D  30 0f        JR nc,0x856e
855F  3a 05 e5     LD A,(0xe505) ; jugador_y
8562  dd be 05     CP (IX+5) ; y
8565  38 07        JR c,0x856e
8567  01 cc 0c     LD BC,0xccc
856A  14           INC D
856B  cd 5c 40     CALL 0x405c ; neg_hl
856E  dd 7e 05     LD A,(IX+5) ; y
8571  80           ADD A,B
8572  dd 77 05     LD (IX+5),A ; y
8575  dd 71 0a     LD (IX+10),C ; d1
8578  dd 72 15     LD (IX+0x15),D
857B  11 00 00     LD DE,0
857E  c3 33 b1     JP 0xb133
8623  11 00 02     LD DE,0x200
8626  cd f1 81     CALL 0x81f1
8629  e6 01        AND 1
862B  16 fe        LD D,0xfe
862D  cd 3e b1     CALL 0xb13e
8630  cd 4c 80     CALL 0x804c
8633  d2 5f b1     JP nc,0xb15f
8636  cd 6f 80     CALL 0x806f
8639  da 5f b1     JP c,0xb15f
863C  c9           RET
863D  dd 36 19 80  LD (IX+0x19),0x80
8641  dd 36 08 04  LD (IX+8),4
8645  dd 36 1c 50  LD (IX+0x1c),0x50
8649  3a 2f e6     LD A,(0xe62f)
864C  dd 77 1b     LD (IX+0x1b),A
864F  b7           OR A
8650  20 0e        JR nz,0x8660
8652  3a 2c e6     LD A,(0xe62c)
8655  3c           INC A
8656  32 2c e6     LD (0xe62c),A
8659  3d           DEC A
865A  cb 3f        SRL A
865C  dd 77 18     LD (IX+0x18),A
865F  c9           RET
8660  11 48 c8     LD DE,0xc848
8663  0e 02        LD C,2
8665  3d           DEC A
8666  28 05        JR z,0x866d
8668  11 46 c0     LD DE,0xc046
866B  cb 21        SLA C
866D  dd 72 0a     LD (IX+10),D ; d1
8670  dd 73 11     LD (IX+0x11),E ; atributo_sprite
8673  dd 71 10     LD (IX+0x10),C ; vida
8676  dd 36 01 06  LD (IX+1),6 ; estado
867A  3a 24 e6     LD A,(0xe624)
867D  dd 77 18     LD (IX+0x18),A
8680  c9           RET
8681  dd 36 1b 03  LD (IX+0x1b),3
8685  dd 36 08 04  LD (IX+8),4
8689  dd 36 1c 30  LD (IX+0x1c),0x30
868D  18 59        JR 0x86e8
86E8  dd 36 01 00  LD (IX+1),0 ; estado
86EC  dd cb 18 fe  SET 7,(IX+0x18)
86F0  c9           RET
87DA  cd d5 6b     CALL 0x6bd5
87DD  dd 36 1c 11  LD (IX+0x1c),0x11
87E1  dd 36 1d 00  LD (IX+0x1d),0
87E5  dd 36 1f 01  LD (IX+0x1f),1
87E9  dd 36 0a c4  LD (IX+10),0xc4 ; d1
87ED  dd 36 08 c4  LD (IX+8),0xc4
87F1  dd 36 1e c8  LD (IX+0x1e),0xc8
87F5  21 f5 88     LD HL,0x88f5
87F8  c3 7f 6d     JP 0x6d7f
8935  dd 36 08 04  LD (IX+8),4
8939  cd f1 81     CALL 0x81f1
893C  e6 0f        AND 15
893E  dd 77 1f     LD (IX+0x1f),A
8941  c9           RET
8A39  dd cb 02 d6  SET 2,(IX+2) ; flags
8A3D  cd f1 81     CALL 0x81f1
8A40  e6 03        AND 3
8A42  3c           INC A
8A43  dd 77 08     LD (IX+8),A
8A46  dd 7e 05     LD A,(IX+5) ; y
8A49  dd 77 18     LD (IX+0x18),A
8A4C  dd 7e 07     LD A,(IX+7) ; x
8A4F  dd 77 19     LD (IX+0x19),A
8A52  11 00 00     LD DE,0
8A55  21 00 02     LD HL,0x200
8A58  cd 33 b1     CALL 0xb133
8A5B  3a 33 e6     LD A,(0xe633)
8A5E  dd 77 14     LD (IX+0x14),A
8A61  3c           INC A
8A62  32 33 e6     LD (0xe633),A
8A65  c6 02        ADD A,2
8A67  dd 77 0b     LD (IX+11),A ; d2
8A6A  fe 0a        CP 10
8A6C  20 06        JR nz,0x8a74
8A6E  cd 24 b1     CALL 0xb124
8A71  dd 34 01     INC (IX+1) ; estado
8A74  3e 08        LD A,8
8A76  32 34 e6     LD (0xe634),A
8A79  c9           RET
8B32  cd f5 96     CALL 0x96f5
8B35  dd 36 18 03  LD (IX+0x18),3
8B39  cd f1 81     CALL 0x81f1
8B3C  c3 cf 71     JP 0x71cf
8C1F  cd d2 90     CALL 0x90d2
8C22  dd cb 02 a6  RES 4,(IX+2) ; flags
8C26  3a 07 e5     LD A,(0xe507) ; jugador_x
8C29  dd be 07     CP (IX+7) ; x
8C2C  3e 00        LD A,0
8C2E  30 02        JR nc,0x8c32
8C30  3e 02        LD A,2
8C32  dd 77 0a     LD (IX+10),A ; d1
8C35  c9           RET
8CE6  cd 8d b1     CALL 0xb18d
8CE9  cd 33 b1     CALL 0xb133
8CEC  cd 42 6c     CALL 0x6c42
8CEF  3e 0f        LD A,15
8CF1  c3 59 40     JP 0x4059 ; sound_request
8D04  dd 36 15 78  LD (IX+0x15),0x78
8D08  cd 61 8d     CALL 0x8d61
8D0B  18 42        JR 0x8d4f
8D4F  dd 36 08 3c  LD (IX+8),0x3c
8D53  3e 10        LD A,0x10
8D55  cd 8f b1     CALL 0xb18f
8D58  c3 33 b1     JP 0xb133
8D61  3a 04 e6     LD A,(0xe604)
8D64  cb 5f        BIT 3,A
8D66  3e 04        LD A,4
8D68  28 01        JR z,0x8d6b
8D6A  3c           INC A
8D6B  dd 77 0a     LD (IX+10),A ; d1
8D6E  dd 7e 05     LD A,(IX+5) ; y
8D71  c6 0a        ADD A,10
8D73  dd 77 12     LD (IX+0x12),A
8D76  dd 7e 07     LD A,(IX+7) ; x
8D79  c6 0d        ADD A,13
8D7B  dd 77 13     LD (IX+0x13),A
8D7E  c9           RET
8D7F  cd 42 6c     CALL 0x6c42
8D82  11 08 08     LD DE,0x808
8D85  cd db b1     CALL 0xb1db
8D88  c6 10        ADD A,0x10
8D8A  07           RLCA
8D8B  07           RLCA
8D8C  07           RLCA
8D8D  e6 07        AND 7
8D8F  87           ADD A,A
8D90  87           ADD A,A
8D91  87           ADD A,A
8D92  4f           LD C,A
8D93  06 00        LD B,0
8D95  21 c5 8d     LD HL,0x8dc5
8D98  09           ADD HL,BC
8D99  7e           LD A,(HL)
8D9A  dd 77 0a     LD (IX+10),A ; d1
8D9D  23           INC HL
8D9E  5e           LD E,(HL)
8D9F  23           INC HL
8DA0  56           LD D,(HL)
8DA1  cd 37 b1     CALL 0xb137
8DA4  23           INC HL
8DA5  5e           LD E,(HL)
8DA6  23           INC HL
8DA7  56           LD D,(HL)
8DA8  23           INC HL
8DA9  cd 3e b1     CALL 0xb13e
8DAC  7e           LD A,(HL)
8DAD  23           INC HL
8DAE  dd 86 05     ADD A,(IX+5) ; y
8DB1  dd 77 05     LD (IX+5),A ; y
8DB4  7e           LD A,(HL)
8DB5  dd 86 07     ADD A,(IX+7) ; x
8DB8  dd 77 07     LD (IX+7),A ; x
8DBB  23           INC HL
8DBC  7e           LD A,(HL)
8DBD  dd 77 11     LD (IX+0x11),A ; atributo_sprite
8DC0  3e 19        LD A,0x19
8DC2  c3 59 40     JP 0x4059 ; sound_request
8E05  dd cb 02 a6  RES 4,(IX+2) ; flags
8E09  dd 36 17 60  LD (IX+0x17),0x60 ; ay
8E0D  dd 36 19 80  LD (IX+0x19),0x80
8E11  af           XOR A
8E12  dd 77 1a     LD (IX+0x1a),A
8E15  dd 77 1d     LD (IX+0x1d),A
8E18  dd 36 1b 01  LD (IX+0x1b),1
8E1C  dd 36 1c 3c  LD (IX+0x1c),0x3c
8E20  ed 5f        LD A,R
8E22  e6 7f        AND 0x7f
8E24  c6 80        ADD A,0x80
8E26  dd 77 08     LD (IX+8),A
8E29  dd 36 1e 3c  LD (IX+0x1e),0x3c
8E2D  cd b2 8e     CALL 0x8eb2
8E30  cd bb 8e     CALL 0x8ebb
8E33  ed 5f        LD A,R
8E35  e6 3f        AND 0x3f
8E37  c6 20        ADD A,0x20
8E39  dd 77 15     LD (IX+0x15),A
8E3C  c9           RET
8EB2  cd 3a 8f     CALL 0x8f3a
8EB5  cd 75 8f     CALL 0x8f75
8EB8  cd d2 8e     CALL 0x8ed2
8EBB  3a 04 e6     LD A,(0xe604)
8EBE  47           LD B,A
8EBF  e6 07        AND 7
8EC1  c0           RET nz
8EC2  11 42 08     LD DE,0x842
8EC5  cb 58        BIT 3,B
8EC7  28 02        JR z,0x8ecb
8EC9  14           INC D
8ECA  1c           INC E
8ECB  dd 72 0a     LD (IX+10),D ; d1
8ECE  dd 73 11     LD (IX+0x11),E ; atributo_sprite
8ED1  c9           RET
8ED2  dd 35 15     DEC (IX+0x15)
8ED5  c0           RET nz
8ED6  cd 33 8e     CALL 0x8e33
8ED9  dd 7e 05     LD A,(IX+5) ; y
8EDC  c6 08        ADD A,8
8EDE  57           LD D,A
8EDF  dd 7e 07     LD A,(IX+7) ; x
8EE2  c6 08        ADD A,8
8EE4  5f           LD E,A
8EE5  06 00        LD B,0
8EE7  48           LD C,B
8EE8  3e 45        LD A,0x45
8EEA  c3 51 65     JP 0x6551
8F3A  dd 7e 1a     LD A,(IX+0x1a)
8F3D  dd 86 1b     ADD A,(IX+0x1b)
8F40  dd 77 1a     LD (IX+0x1a),A
8F43  dd 35 1c     DEC (IX+0x1c)
8F46  c0           RET nz
8F47  dd 36 1c 3c  LD (IX+0x1c),0x3c
8F4B  dd cb 1d 46  BIT 0,(IX+0x1d)
8F4F  28 13        JR z,0x8f64
8F51  dd 7e 1b     LD A,(IX+0x1b)
8F54  d6 08        SUB 8
8F56  ed 44        NEG
8F58  fe 10        CP 0x10
8F5A  38 15        JR c,0x8f71
8F5C  dd 36 1d 00  LD (IX+0x1d),0
8F60  dd 34 1b     INC (IX+0x1b)
8F63  c9           RET
8F64  dd 7e 1b     LD A,(IX+0x1b)
8F67  c6 08        ADD A,8
8F69  fe 10        CP 0x10
8F6B  38 f3        JR c,0x8f60
8F6D  dd 36 1d 01  LD (IX+0x1d),1
8F71  dd 35 1b     DEC (IX+0x1b)
8F74  c9           RET
8F75  dd 7e 1a     LD A,(IX+0x1a)
8F78  cd ff b1     CALL 0xb1ff
8F7B  5f           LD E,A
8F7C  16 00        LD D,0
8F7E  d6 3f        SUB 0x3f
8F80  ed 44        NEG
8F82  21 74 b3     LD HL,0xb374
8F85  e5           PUSH HL
8F86  19           ADD HL,DE
8F87  4e           LD C,(HL)
8F88  e1           POP HL
8F89  5f           LD E,A
8F8A  19           ADD HL,DE
8F8B  7e           LD A,(HL)
8F8C  32 16 f0     LD (0xf016),A
8F8F  79           LD A,C
8F90  1f           RRA
8F91  1f           RRA
8F92  1f           RRA
8F93  e6 1f        AND 0x1f
8F95  21 10 f0     LD HL,0xf010
8F98  cb 46        BIT 0,(HL)
8F9A  28 02        JR z,0x8f9e
8F9C  ed 44        NEG
8F9E  dd 86 17     ADD A,(IX+0x17) ; ay
8FA1  dd 77 05     LD (IX+5),A ; y
8FA4  3a 16 f0     LD A,(0xf016)
8FA7  1f           RRA
8FA8  1f           RRA
8FA9  1f           RRA
8FAA  e6 1f        AND 0x1f
8FAC  2c           INC L
8FAD  cb 46        BIT 0,(HL)
8FAF  28 02        JR z,0x8fb3
8FB1  ed 44        NEG
8FB3  dd 86 19     ADD A,(IX+0x19)
8FB6  dd 77 07     LD (IX+7),A ; x
8FB9  c9           RET
8FBA  cd 42 6c     CALL 0x6c42
8FBD  3e 40        LD A,0x40
8FBF  cd 8f b1     CALL 0xb18f
8FC2  cd 33 b1     CALL 0xb133
8FC5  3e 0e        LD A,14
8FC7  c3 59 40     JP 0x4059 ; sound_request
8FCA  dd 36 15 10  LD (IX+0x15),0x10
8FCE  dd 36 08 08  LD (IX+8),8
8FD2  c9           RET
8FF7  cd 42 6c     CALL 0x6c42
8FFA  dd 36 0b 01  LD (IX+11),1 ; d2
8FFE  cd f1 81     CALL 0x81f1
9001  e6 0f        AND 15
9003  dd 77 15     LD (IX+0x15),A
9006  dd 36 18 04  LD (IX+0x18),4
900A  c9           RET
9078  cd d2 90     CALL 0x90d2
907B  dd 36 15 1e  LD (IX+0x15),0x1e
907F  dd cb 02 b6  RES 6,(IX+2) ; flags
9083  c9           RET
90D2  dd 36 08 1e  LD (IX+8),0x1e
90D6  c9           RET
9143  cd 42 6c     CALL 0x6c42
9146  dd cb 0f 7e  BIT 7,(IX+15) ; vx
914A  28 08        JR z,0x9154
914C  dd 36 0a c8  LD (IX+10),0xc8 ; d1
9150  dd 36 11 60  LD (IX+0x11),0x60 ; atributo_sprite
9154  3e 10        LD A,0x10
9156  c3 59 40     JP 0x4059 ; sound_request
9159  dd 36 08 3c  LD (IX+8),0x3c
915D  c3 7f 90     JP 0x907f
91E0  cd 42 6c     CALL 0x6c42
91E3  3a 04 e6     LD A,(0xe604)
91E6  e6 1f        AND 0x1f
91E8  d6 08        SUB 8
91EA  dd 86 05     ADD A,(IX+5) ; y
91ED  5f           LD E,A
91EE  dd 56 07     LD D,(IX+7) ; x
91F1  3e 60        LD A,0x60
91F3  cd 95 b1     CALL 0xb195
91F6  cd 33 b1     CALL 0xb133
91F9  3e 12        LD A,0x12
91FB  c3 59 40     JP 0x4059 ; sound_request
91FE  3a 15 e5     LD A,(0xe515)
9201  a7           AND A
9202  28 08        JR z,0x920c
9204  3c           INC A
9205  21 13 e5     LD HL,0xe513
9208  cb 7e        BIT 7,(HL)
920A  28 01        JR z,0x920d
920C  af           XOR A
920D  dd 77 01     LD (IX+1),A ; estado
9210  dd 7e 05     LD A,(IX+5) ; y
9213  dd 77 1f     LD (IX+0x1f),A
9216  c3 2e b1     JP 0xb12e
92BA  3e 01        LD A,1
92BC  32 27 e6     LD (0xe627),A
92BF  cd f1 81     CALL 0x81f1
92C2  e6 7f        AND 0x7f
92C4  c6 40        ADD A,0x40
92C6  dd 77 07     LD (IX+7),A ; x
92C9  cd f1 81     CALL 0x81f1
92CC  e6 3f        AND 0x3f
92CE  c6 40        ADD A,0x40
92D0  dd 77 05     LD (IX+5),A ; y
92D3  cd f1 81     CALL 0x81f1
92D6  e6 0f        AND 15
92D8  dd 77 1f     LD (IX+0x1f),A
92DB  3e 01        LD A,1
92DD  dd 77 1e     LD (IX+0x1e),A
92E0  c3 cf 71     JP 0x71cf
9357  cd f1 81     CALL 0x81f1
935A  e6 3f        AND 0x3f
935C  c6 07        ADD A,7
935E  dd 77 15     LD (IX+0x15),A
9361  cd f1 81     CALL 0x81f1
9364  e6 0f        AND 15
9366  dd 77 18     LD (IX+0x18),A
9369  06 03        LD B,3
936B  cd 25 7e     CALL 0x7e25
936E  dd 7e 18     LD A,(IX+0x18)
9371  21 9c 93     LD HL,0x939c
9374  cd 83 40     CALL 0x4083
9377  dd 77 0a     LD (IX+10),A ; d1
937A  21 cc 93     LD HL,0x93cc
937D  c3 88 b0     JP 0xb088
93D4  11 40 00     LD DE,0x40
93D7  cd 3e b1     CALL 0xb13e
93DA  ed 5f        LD A,R
93DC  e6 01        AND 1
93DE  c4 5f b1     CALL nz,0xb15f
93E1  c9           RET
942F  cd f2 9a     CALL 0x9af2
9432  cd f9 9a     CALL 0x9af9
9435  11 00 01     LD DE,0x100
9438  cd 45 b1     CALL 0xb145
943B  3e a0        LD A,0xa0
943D  11 a0 03     LD DE,0x3a0
9440  dd cb 1d 46  BIT 0,(IX+0x1d)
9444  28 09        JR z,0x944f
9446  3e ac        LD A,0xac
9448  11 60 fc     LD DE,0xfc60
944B  dd 36 11 13  LD (IX+0x11),0x13 ; atributo_sprite
944F  dd 77 0a     LD (IX+10),A ; d1
9452  c3 3e b1     JP 0xb13e
951C  ed 5f        LD A,R
951E  e6 01        AND 1
9520  11 00 02     LD DE,0x200
9523  c4 68 40     CALL nz,0x4068 ; neg_de
9526  cd 3e b1     CALL 0xb13e
9529  dd cb 0f 7e  BIT 7,(IX+15) ; vx
952D  3e c8        LD A,0xc8
952F  06 00        LD B,0
9531  28 03        JR z,0x9536
9533  3e d0        LD A,0xd0
9535  04           INC B
9536  dd 70 14     LD (IX+0x14),B
9539  dd 36 11 13  LD (IX+0x11),0x13 ; atributo_sprite
953D  c3 83 70     JP 0x7083
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
969A  dd 36 08 10  LD (IX+8),0x10
969E  dd cb 02 7e  BIT 7,(IX+2) ; flags
96A2  c4 f5 96     CALL nz,0x96f5
96A5  cd ab 96     CALL 0x96ab
96A8  d0           RET nc
96A9  18 43        JR 0x96ee
96AB  cd f1 81     CALL 0x81f1
96AE  e6 03        AND 3
96B0  47           LD B,A
96B1  cd f1 81     CALL 0x81f1
96B4  e6 01        AND 1
96B6  80           ADD A,B
96B7  21 20 97     LD HL,0x9720
96BA  cd 83 40     CALL 0x4083
96BD  f5           PUSH AF
96BE  cd f1 81     CALL 0x81f1
96C1  f6 10        OR 0x10
96C3  e6 f0        AND 0xf0
96C5  67           LD H,A
96C6  d6 20        SUB 0x20
96C8  fe b0        CP 0xb0
96CA  30 f2        JR nc,0x96be
96CC  dd 74 1e     LD (IX+0x1e),H
96CF  f1           POP AF
96D0  dd 77 1f     LD (IX+0x1f),A
96D3  c6 10        ADD A,0x10
96D5  6f           LD L,A
96D6  cd 6e 40     CALL 0x406e ; vec_4dc8
96D9  d0           RET nc
96DA  23           INC HL
96DB  be           CP (HL)
96DC  d0           RET nc
96DD  cd fe 96     CALL 0x96fe
96E0  d0           RET nc
96E1  dd 7e 1e     LD A,(IX+0x1e)
96E4  dd 77 07     LD (IX+7),A ; x
96E7  dd 7e 1f     LD A,(IX+0x1f)
96EA  dd 77 05     LD (IX+5),A ; y
96ED  c9           RET
96EE  3e f0        LD A,0xf0
96F0  dd b6 02     OR (IX+2) ; flags
96F3  18 05        JR 0x96fa
96F5  dd 7e 02     LD A,(IX+2) ; flags
96F8  e6 0f        AND 15
96FA  dd 77 02     LD (IX+2),A ; flags
96FD  c9           RET
96FE  dd 66 1e     LD H,(IX+0x1e)
9701  dd 6e 1f     LD L,(IX+0x1f)
9704  18 06        JR 0x970c
9706  dd 66 07     LD H,(IX+7) ; x
9709  dd 6e 05     LD L,(IX+5) ; y
970C  cd 6e 40     CALL 0x406e ; vec_4dc8
970F  3f           CCF
9710  d0           RET nc
9711  23           INC HL
9712  be           CP (HL)
9713  3f           CCF
9714  d0           RET nc
9715  01 1f 00     LD BC,0x1f
9718  09           ADD HL,BC
9719  be           CP (HL)
971A  3f           CCF
971B  d0           RET nc
971C  23           INC HL
971D  be           CP (HL)
971E  3f           CCF
971F  c9           RET
9725  cd f2 9a     CALL 0x9af2
9728  ed 5f        LD A,R
972A  e6 01        AND 1
972C  dd 77 1d     LD (IX+0x1d),A
972F  11 80 01     LD DE,0x180
9732  c4 68 40     CALL nz,0x4068 ; neg_de
9735  cd 3e b1     CALL 0xb13e
9738  11 40 00     LD DE,0x40
973B  cd 45 b1     CALL 0xb145
973E  dd cb 0f 7e  BIT 7,(IX+15) ; vx
9742  3e a0        LD A,0xa0
9744  06 0c        LD B,12
9746  28 04        JR z,0x974c
9748  3e ac        LD A,0xac
974A  06 0d        LD B,13
974C  dd 77 0a     LD (IX+10),A ; d1
974F  dd 70 11     LD (IX+0x11),B ; atributo_sprite
9752  a7           AND A
9753  c9           RET
9890  cd 42 6c     CALL 0x6c42
9893  3a 24 e6     LD A,(0xe624)
9896  b7           OR A
9897  c8           RET z
9898  dd 36 0a b4  LD (IX+10),0xb4 ; d1
989C  dd 36 11 21  LD (IX+0x11),0x21 ; atributo_sprite
98A0  c9           RET
98C0  dd cb 02 ce  SET 1,(IX+2) ; flags
98C4  cd f2 9a     CALL 0x9af2
98C7  cd a9 7c     CALL 0x7ca9
98CA  11 50 00     LD DE,0x50
98CD  cd 45 b1     CALL 0xb145
98D0  cd e7 9a     CALL 0x9ae7
98D3  dd cb 1d 46  BIT 0,(IX+0x1d)
98D7  11 20 01     LD DE,0x120
98DA  c4 68 40     CALL nz,0x4068 ; neg_de
98DD  cd 3e b1     CALL 0xb13e
98E0  dd cb 1d 46  BIT 0,(IX+0x1d)
98E4  3e ac        LD A,0xac
98E6  01 aa b4     LD BC,0xb4aa
98E9  16 4e        LD D,0x4e
98EB  28 0a        JR z,0x98f7
98ED  3e b8        LD A,0xb8
98EF  01 b2 c0     LD BC,0xc0b2
98F2  14           INC D
98F3  dd cb 02 c6  SET 0,(IX+2) ; flags
98F7  1e 0f        LD E,15
98F9  dd 70 12     LD (IX+0x12),B
98FC  dd 71 09     LD (IX+9),C ; d0
98FF  dd 72 11     LD (IX+0x11),D ; atributo_sprite
9902  dd 73 13     LD (IX+0x13),E
9905  c3 83 70     JP 0x7083
9AE7  cd f1 81     CALL 0x81f1
9AEA  e6 7f        AND 0x7f
9AEC  c6 20        ADD A,0x20
9AEE  dd 77 1a     LD (IX+0x1a),A
9AF1  c9           RET
9AF2  ed 5f        LD A,R
9AF4  e6 7f        AND 0x7f
9AF6  c3 cf 71     JP 0x71cf
9AF9  ed 5f        LD A,R
9AFB  e6 01        AND 1
9AFD  dd 77 1d     LD (IX+0x1d),A
9B00  c9           RET
9B30  cd ab 9b     CALL 0x9bab
9B33  dd 7e 05     LD A,(IX+5) ; y
9B36  e6 e0        AND 0xe0
9B38  1f           RRA
9B39  1f           RRA
9B3A  1f           RRA
9B3B  1f           RRA
9B3C  1f           RRA
9B3D  3d           DEC A
9B3E  dd 77 19     LD (IX+0x19),A
9B41  cd f9 9a     CALL 0x9af9
9B44  cd 37 9d     CALL 0x9d37
9B47  dd cb 1d 46  BIT 0,(IX+0x1d)
9B4B  3e a0        LD A,0xa0
9B4D  28 02        JR z,0x9b51
9B4F  3e ac        LD A,0xac
9B51  c3 83 70     JP 0x7083
9BAB  cd f1 81     CALL 0x81f1
9BAE  c3 cf 71     JP 0x71cf
9D37  dd cb 1d 46  BIT 0,(IX+0x1d)
9D3B  11 00 02     LD DE,0x200
9D3E  c4 68 40     CALL nz,0x4068 ; neg_de
9D41  cd 3e b1     CALL 0xb13e
9D44  c3 47 9b     JP 0x9b47
9D97  dd 36 17 0a  LD (IX+0x17),10 ; ay
9D9B  dd 36 08 08  LD (IX+8),8
9D9F  dd 36 18 08  LD (IX+0x18),8
9DA3  dd 36 15 01  LD (IX+0x15),1
9DA7  dd 36 16 0a  LD (IX+0x16),10 ; ay_lo
9DAB  dd 36 13 04  LD (IX+0x13),4
9DAF  dd 36 12 00  LD (IX+0x12),0
9DB3  cd f1 81     CALL 0x81f1
9DB6  e6 4f        AND 0x4f
9DB8  c6 30        ADD A,0x30
9DBA  dd 77 1b     LD (IX+0x1b),A
9DBD  c9           RET
9F1E  cd 42 6c     CALL 0x6c42
9F21  3e 40        LD A,0x40
9F23  cd 8f b1     CALL 0xb18f
9F26  cd 33 b1     CALL 0xb133
9F29  dd 7e 00     LD A,(IX) ; tipo
9F2C  fe 44        CP 0x44
9F2E  3e 0f        LD A,15
9F30  28 02        JR z,0x9f34
9F32  3e 0e        LD A,14
9F34  c3 59 40     JP 0x4059 ; sound_request
9F3F  3a 2f e6     LD A,(0xe62f)
9F42  3c           INC A
9F43  32 2f e6     LD (0xe62f),A
9F46  e6 03        AND 3
9F48  21 66 9f     LD HL,0x9f66
9F4B  cd 83 40     CALL 0x4083
9F4E  dd 77 0d     LD (IX+13),A ; vy
9F51  23           INC HL
9F52  7e           LD A,(HL)
9F53  dd 77 0f     LD (IX+15),A ; vx
9F56  af           XOR A
9F57  dd 77 0e     LD (IX+14),A ; vx_lo
9F5A  dd 77 0c     LD (IX+12),A ; vy_lo
9F5D  dd 36 08 08  LD (IX+8),8
9F61  dd cb 02 a6  RES 4,(IX+2) ; flags
9F65  c9           RET
A070  cd f1 81     CALL 0x81f1
A073  dd 77 08     LD (IX+8),A
A076  af           XOR A
A077  dd 77 01     LD (IX+1),A ; estado
A07A  3d           DEC A
A07B  dd 77 1e     LD (IX+0x1e),A
A07E  c3 f5 96     JP 0x96f5
A0E3  dd 36 12 bc  LD (IX+0x12),0xbc
A0E7  dd 36 13 01  LD (IX+0x13),1
A0EB  dd 36 14 01  LD (IX+0x14),1
A0EF  18 04        JR 0xa0f5
A0F1  dd 36 14 08  LD (IX+0x14),8
A0F5  dd cb 02 ae  RES 5,(IX+2) ; flags
A0F9  dd 36 15 01  LD (IX+0x15),1
A0FD  11 00 00     LD DE,0
A100  cd 33 b1     CALL 0xb133
A103  3e 01        LD A,1
A105  32 3e e6     LD (0xe63e),A
A108  c9           RET
A198  dd 36 12 a4  LD (IX+0x12),0xa4
A19C  dd 36 13 0e  LD (IX+0x13),14
A1A0  dd 7e 05     LD A,(IX+5) ; y
A1A3  c6 08        ADD A,8
A1A5  dd 77 16     LD (IX+0x16),A ; ay_lo
A1A8  dd 7e 07     LD A,(IX+7) ; x
A1AB  d6 0d        SUB 13
A1AD  dd 77 14     LD (IX+0x14),A
A1B0  c6 24        ADD A,0x24
A1B2  dd 77 15     LD (IX+0x15),A
A1B5  c3 f5 96     JP 0x96f5
A283  3a 69 e6     LD A,(0xe669)
A286  fe 05        CP 5
A288  dd 36 07 b0  LD (IX+7),0xb0 ; x
A28C  30 04        JR nc,0xa292
A28E  dd 36 07 40  LD (IX+7),0x40 ; x
A292  dd 36 05 28  LD (IX+5),0x28 ; y
A296  dd 36 1b 0c  LD (IX+0x1b),12
A29A  dd 36 08 06  LD (IX+8),6
A29E  11 00 02     LD DE,0x200
A2A1  c3 37 b1     JP 0xb137
A3E2  dd 36 17 0a  LD (IX+0x17),10 ; ay
A3E6  dd 36 08 08  LD (IX+8),8
A3EA  dd 36 18 04  LD (IX+0x18),4
A3EE  dd 36 15 01  LD (IX+0x15),1
A3F2  dd 36 13 03  LD (IX+0x13),3
A3F6  dd 36 16 0a  LD (IX+0x16),10 ; ay_lo
A3FA  dd 36 12 05  LD (IX+0x12),5
A3FE  c3 b3 9d     JP 0x9db3
A42A  cd 71 6b     CALL 0x6b71
A42D  dd 36 0a cc  LD (IX+10),0xcc ; d1
A431  dd 7e 07     LD A,(IX+7) ; x
A434  dd 77 14     LD (IX+0x14),A
A437  dd 7e 05     LD A,(IX+5) ; y
A43A  dd 77 15     LD (IX+0x15),A
A43D  dd 36 1b 10  LD (IX+0x1b),0x10
A441  dd 36 18 06  LD (IX+0x18),6
A445  dd 36 1a 30  LD (IX+0x1a),0x30
A449  dd 36 1f 08  LD (IX+0x1f),8
A44D  cd f1 81     CALL 0x81f1
A450  e6 2f        AND 0x2f
A452  c6 10        ADD A,0x10
A454  dd 77 12     LD (IX+0x12),A
A457  c9           RET
A548  cd 42 6c     CALL 0x6c42
A54B  3e 60        LD A,0x60
A54D  cd 8f b1     CALL 0xb18f
A550  cd 33 b1     CALL 0xb133
A553  3e 0f        LD A,15
A555  c3 59 40     JP 0x4059 ; sound_request
A560  21 07 e5     LD HL,0xe507 ; jugador_x
A563  3a 07 e7     LD A,(0xe707)
A566  be           CP (HL)
A567  38 03        JR c,0xa56c
A569  af           XOR A
A56A  18 0c        JR 0xa578
A56C  3a 07 e7     LD A,(0xe707)
A56F  c6 40        ADD A,0x40
A571  be           CP (HL)
A572  3e 03        LD A,3
A574  30 02        JR nc,0xa578
A576  3e 06        LD A,6
A578  dd 77 14     LD (IX+0x14),A
A57B  21 05 e5     LD HL,0xe505 ; jugador_y
A57E  3a 05 e7     LD A,(0xe705)
A581  be           CP (HL)
A582  30 0b        JR nc,0xa58f
A584  c6 40        ADD A,0x40
A586  be           CP (HL)
A587  30 03        JR nc,0xa58c
A589  dd 34 14     INC (IX+0x14)
A58C  dd 34 14     INC (IX+0x14)
A58F  dd 7e 14     LD A,(IX+0x14)
A592  21 d7 a5     LD HL,0xa5d7
A595  cd 83 40     CALL 0x4083
A598  dd 77 0a     LD (IX+10),A ; d1
A59B  dd 7e 14     LD A,(IX+0x14)
A59E  21 b3 a5     LD HL,0xa5b3
A5A1  cd 53 40     CALL 0x4053 ; read_word_table
A5A4  cd 3e b1     CALL 0xb13e
A5A7  dd 7e 14     LD A,(IX+0x14)
A5AA  21 c5 a5     LD HL,0xa5c5
A5AD  cd 53 40     CALL 0x4053 ; read_word_table
A5B0  c3 37 b1     JP 0xb137
A5E0  cd 71 6b     CALL 0x6b71
A5E3  dd 36 18 08  LD (IX+0x18),8
A5E7  dd 36 17 32  LD (IX+0x17),0x32 ; ay
A5EB  3a 1a e7     LD A,(0xe71a)
A5EE  dd 77 1a     LD (IX+0x1a),A
A5F1  18 54        JR 0xa647
A647  dd 7e 1a     LD A,(IX+0x1a)
A64A  dd 77 19     LD (IX+0x19),A
A64D  3a 1d e7     LD A,(0xe71d)
A650  dd 77 14     LD (IX+0x14),A
A653  3a 1e e7     LD A,(0xe71e)
A656  dd 77 15     LD (IX+0x15),A
A659  c9           RET
A7A8  dd 36 1e a1  LD (IX+0x1e),0xa1
A7AC  dd 36 01 01  LD (IX+1),1 ; estado
A7B0  11 80 ff     LD DE,0xff80
A7B3  cd 45 b1     CALL 0xb145
A7B6  ed 5f        LD A,R
A7B8  cb 47        BIT 0,A
A7BA  11 00 01     LD DE,0x100
A7BD  c4 68 40     CALL nz,0x4068 ; neg_de
A7C0  c3 3e b1     JP 0xb13e
A830  11 80 00     LD DE,0x80
A833  cd 3e b1     CALL 0xb13e
A836  ed 5f        LD A,R
A838  e6 01        AND 1
A83A  cc 5f b1     CALL z,0xb15f
A83D  dd 7e 05     LD A,(IX+5) ; y
A840  dd 77 16     LD (IX+0x16),A ; ay_lo
A843  dd 36 08 0a  LD (IX+8),10
A847  c9           RET
A8DF  cd 71 6b     CALL 0x6b71
A8E2  dd 7e 05     LD A,(IX+5) ; y
A8E5  dd 77 15     LD (IX+0x15),A
A8E8  af           XOR A
A8E9  dd 77 16     LD (IX+0x16),A ; ay_lo
A8EC  dd 77 17     LD (IX+0x17),A ; ay
A8EF  dd 36 18 08  LD (IX+0x18),8
A8F3  dd 36 1b 80  LD (IX+0x1b),0x80
A8F7  cd f1 81     CALL 0x81f1
A8FA  e6 01        AND 1
A8FC  dd 77 1e     LD (IX+0x1e),A
A8FF  dd 46 07     LD B,(IX+7) ; x
A902  3e 0a        LD A,10
A904  28 02        JR z,0xa908
A906  ed 44        NEG
A908  80           ADD A,B
A909  dd 77 14     LD (IX+0x14),A
A90C  c9           RET
A974  21 37 e6     LD HL,0xe637
A977  7e           LD A,(HL)
A978  dd 77 14     LD (IX+0x14),A
A97B  34           INC (HL)
A97C  cd a8 7d     CALL 0x7da8
A97F  cd 71 6b     CALL 0x6b71
A982  dd 36 1d 1a  LD (IX+0x1d),0x1a
A986  dd 36 1e 08  LD (IX+0x1e),8
A98A  dd 36 1f 20  LD (IX+0x1f),0x20
A98E  11 00 01     LD DE,0x100
A991  cd 3e b1     CALL 0xb13e
A994  cd 6c aa     CALL 0xaa6c
A997  dd 7e 16     LD A,(IX+0x16) ; ay_lo
A99A  c3 83 70     JP 0x7083
AA6C  dd cb 0f 7e  BIT 7,(IX+15) ; vx
AA70  20 11        JR nz,0xaa83
AA72  dd 36 16 a0  LD (IX+0x16),0xa0 ; ay_lo
AA76  dd 36 17 a4  LD (IX+0x17),0xa4 ; ay
AA7A  dd 36 18 a8  LD (IX+0x18),0xa8
AA7E  dd 36 19 ac  LD (IX+0x19),0xac
AA82  c9           RET
AA83  dd 36 16 b0  LD (IX+0x16),0xb0 ; ay_lo
AA87  dd 36 17 b4  LD (IX+0x17),0xb4 ; ay
AA8B  dd 36 18 b8  LD (IX+0x18),0xb8
AA8F  dd 36 19 bc  LD (IX+0x19),0xbc
AA93  c9           RET
AAA3  cd 71 6b     CALL 0x6b71
AAA6  dd 7e f4     LD A,(IX-12)
AAA9  dd 77 14     LD (IX+0x14),A
AAAC  dd 7e f5     LD A,(IX-11)
AAAF  dd 77 15     LD (IX+0x15),A
AAB2  c3 7f a9     JP 0xa97f
AAB5  dd 7e 02     LD A,(IX+2) ; flags
AAB8  e6 cf        AND 0xcf
AABA  dd 77 02     LD (IX+2),A ; flags
AABD  c9           RET
AABE  cd 7f 90     CALL 0x907f
AAC1  3a 31 f0     LD A,(0xf031)
AAC4  b7           OR A
AAC5  c0           RET nz
AAC6  3a 30 f0     LD A,(0xf030)
AAC9  b7           OR A
AACA  20 14        JR nz,0xaae0
AACC  cd f1 81     CALL 0x81f1
AACF  e6 03        AND 3
AAD1  47           LD B,A
AAD2  cd f1 81     CALL 0x81f1
AAD5  e6 07        AND 7
AAD7  80           ADD A,B
AAD8  32 33 f0     LD (0xf033),A
AADB  3e 01        LD A,1
AADD  32 30 f0     LD (0xf030),A
AAE0  21 32 f0     LD HL,0xf032
AAE3  3a 33 f0     LD A,(0xf033)
AAE6  be           CP (HL)
AAE7  28 02        JR z,0xaaeb
AAE9  34           INC (HL)
AAEA  c9           RET
AAEB  dd 34 1f     INC (IX+0x1f)
AAEE  3e 01        LD A,1
AAF0  32 31 f0     LD (0xf031),A
AAF3  c9           RET
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
AB88  11 40 00     LD DE,0x40
AB8B  cd 45 b1     CALL 0xb145
AB8E  cd f1 81     CALL 0x81f1
AB91  d6 10        SUB 0x10
AB93  e6 df        AND 0xdf
AB95  c6 10        ADD A,0x10
AB97  dd 77 07     LD (IX+7),A ; x
AB9A  17           RLA
AB9B  11 80 01     LD DE,0x180
AB9E  dc 68 40     CALL c,0x4068 ; neg_de
ABA1  cd 3e b1     CALL 0xb13e
ABA4  c3 29 b1     JP 0xb129
ABFE  cd f1 81     CALL 0x81f1
AC01  e6 01        AND 1
AC03  3d           DEC A
AC04  dd 77 01     LD (IX+1),A ; estado
AC07  28 03        JR z,0xac0c
AC09  cd 71 6b     CALL 0x6b71
AC0C  dd 36 08 18  LD (IX+8),0x18
AC10  dd 36 11 66  LD (IX+0x11),0x66 ; atributo_sprite
AC14  dd 36 14 00  LD (IX+0x14),0
AC18  cd f1 81     CALL 0x81f1
AC1B  e6 01        AND 1
AC1D  11 00 01     LD DE,0x100
AC20  28 03        JR z,0xac25
AC22  11 00 01     LD DE,0x100
AC25  c3 3e b1     JP 0xb13e
AD05  dd 36 14 08  LD (IX+0x14),8
AD09  dd cb 02 ae  RES 5,(IX+2) ; flags
AD0D  01 49 6f     LD BC,0x6f49
AD10  c3 f0 6e     JP 0x6ef0
AD51  dd cb 02 a6  RES 4,(IX+2) ; flags
AD55  3a 24 e6     LD A,(0xe624)
AD58  dd 77 14     LD (IX+0x14),A
AD5B  a7           AND A
AD5C  06 0c        LD B,12
AD5E  28 02        JR z,0xad62
AD60  06 00        LD B,0
AD62  dd 70 0a     LD (IX+10),B ; d1
AD65  dd 36 1f 01  LD (IX+0x1f),1
AD69  dd cb 14 4e  BIT 1,(IX+0x14)
AD6D  dd 36 1c 0d  LD (IX+0x1c),13
AD71  28 04        JR z,0xad77
AD73  dd 36 1c 09  LD (IX+0x1c),9
AD77  3e 1c        LD A,0x1c
AD79  cd 59 40     CALL 0x4059 ; sound_request
AD7C  dd 6e 05     LD L,(IX+5) ; y
AD7F  dd 66 07     LD H,(IX+7) ; x
AD82  cd 56 40     CALL 0x4056 ; vec_4da1
AD85  dd 75 12     LD (IX+0x12),L
AD88  dd 74 13     LD (IX+0x13),H
AD8B  c9           RET
ADB2  cd 42 6c     CALL 0x6c42
ADB5  3e 70        LD A,0x70
ADB7  4f           LD C,A
ADB8  3a 45 e0     LD A,(0xe045)
ADBB  87           ADD A,A
ADBC  87           ADD A,A
ADBD  87           ADD A,A
ADBE  81           ADD A,C
ADBF  32 09 e6     LD (0xe609),A
ADC2  11 08 08     LD DE,0x808
ADC5  cd db b1     CALL 0xb1db
ADC8  f5           PUSH AF
ADC9  3a 24 e6     LD A,(0xe624)
ADCC  d6 03        SUB 3
ADCE  87           ADD A,A
ADCF  87           ADD A,A
ADD0  87           ADD A,A
ADD1  47           LD B,A
ADD2  ed 5f        LD A,R
ADD4  e6 0f        AND 15
ADD6  d6 08        SUB 8
ADD8  80           ADD A,B
ADD9  47           LD B,A
ADDA  f1           POP AF
ADDB  80           ADD A,B
ADDC  cd ff b1     CALL 0xb1ff
ADDF  cd a3 b1     CALL 0xb1a3
ADE2  c3 33 b1     JP 0xb133
AE03  cd 42 6c     CALL 0x6c42
AE06  3a 24 e6     LD A,(0xe624)
AE09  1f           RRA
AE0A  f5           PUSH AF
AE0B  47           LD B,A
AE0C  21 2c ae     LD HL,0xae2c
AE0F  cd 53 40     CALL 0x4053 ; read_word_table
AE12  cd 37 b1     CALL 0xb137
AE15  78           LD A,B
AE16  21 3c ae     LD HL,0xae3c
AE19  cd 53 40     CALL 0x4053 ; read_word_table
AE1C  f1           POP AF
AE1D  dc 68 40     CALL c,0x4068 ; neg_de
AE20  cd 3e b1     CALL 0xb13e
AE23  11 80 00     LD DE,0x80
AE26  c3 45 b1     JP 0xb145
AE4C  dd 36 16 20  LD (IX+0x16),0x20 ; ay_lo
AE50  dd 36 17 06  LD (IX+0x17),6 ; ay
AE54  dd 36 08 40  LD (IX+8),0x40
AE58  c9           RET
AEC4  cd 42 6c     CALL 0x6c42
AEC7  3e 40        LD A,0x40
AEC9  c3 4d a5     JP 0xa54d
B088  dd 7e 0a     LD A,(IX+10) ; d1
B08B  d6 c4        SUB 0xc4
B08D  18 05        JR 0xb094
B094  1f           RRA
B095  1f           RRA
B096  e6 3f        AND 0x3f
B098  cd 83 40     CALL 0x4083
B09B  dd 77 11     LD (IX+0x11),A ; atributo_sprite
B09E  c9           RET
B124  cd 2e b1     CALL 0xb12e
B127  18 0e        JR 0xb137
B129  16 00        LD D,0
B12B  5a           LD E,D
B12C  18 09        JR 0xb137
B12E  16 00        LD D,0
B130  5a           LD E,D
B131  18 0b        JR 0xb13e
B133  cd 3e b1     CALL 0xb13e
B136  eb           EX DE,HL
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
B145  dd 73 16     LD (IX+0x16),E ; ay_lo
B148  dd 72 17     LD (IX+0x17),D ; ay
B14B  c9           RET
B15F  dd 5e 0e     LD E,(IX+14) ; vx_lo
B162  dd 56 0f     LD D,(IX+15) ; vx
B165  cd 68 40     CALL 0x4068 ; neg_de
B168  dd 73 0e     LD (IX+14),E ; vx_lo
B16B  dd 72 0f     LD (IX+15),D ; vx
B16E  c9           RET
B16F  dd 5e 0c     LD E,(IX+12) ; vy_lo
B172  dd 56 0d     LD D,(IX+13) ; vy
B175  cd 68 40     CALL 0x4068 ; neg_de
B178  dd 73 0c     LD (IX+12),E ; vy_lo
B17B  dd 72 0d     LD (IX+13),D ; vy
B17E  c9           RET
B18D  3e 80        LD A,0x80
B18F  dd 5e 05     LD E,(IX+5) ; y
B192  dd 56 07     LD D,(IX+7) ; x
B195  4f           LD C,A
B196  3a 45 e0     LD A,(0xe045)
B199  87           ADD A,A
B19A  87           ADD A,A
B19B  87           ADD A,A
B19C  81           ADD A,C
B19D  32 09 e6     LD (0xe609),A
B1A0  cd 23 b2     CALL 0xb223
B1A3  3a 17 f0     LD A,(0xf017)
B1A6  5f           LD E,A
B1A7  16 00        LD D,0
B1A9  d6 3f        SUB 0x3f
B1AB  ed 44        NEG
B1AD  21 74 b3     LD HL,0xb374
B1B0  e5           PUSH HL
B1B1  19           ADD HL,DE
B1B2  4e           LD C,(HL)
B1B3  e1           POP HL
B1B4  5f           LD E,A
B1B5  19           ADD HL,DE
B1B6  7e           LD A,(HL)
B1B7  32 16 f0     LD (0xf016),A
B1BA  59           LD E,C
B1BB  cd 62 b2     CALL 0xb262
B1BE  3a 10 f0     LD A,(0xf010)
B1C1  a7           AND A
B1C2  c4 68 40     CALL nz,0x4068 ; neg_de
B1C5  ed 53 12 f0  LD (0xf012),DE
B1C9  3a 16 f0     LD A,(0xf016)
B1CC  5f           LD E,A
B1CD  cd 62 b2     CALL 0xb262
B1D0  3a 11 f0     LD A,(0xf011)
B1D3  a7           AND A
B1D4  c4 68 40     CALL nz,0x4068 ; neg_de
B1D7  2a 12 f0     LD HL,(0xf012)
B1DA  c9           RET
B1DB  dd 7e 05     LD A,(IX+5) ; y
B1DE  83           ADD A,E
B1DF  5f           LD E,A
B1E0  dd 7e 07     LD A,(IX+7) ; x
B1E3  82           ADD A,D
B1E4  57           LD D,A
B1E5  cd 23 b2     CALL 0xb223
B1E8  4f           LD C,A
B1E9  2a 10 f0     LD HL,(0xf010)
B1EC  7c           LD A,H
B1ED  06 00        LD B,0
B1EF  a7           AND A
B1F0  28 02        JR z,0xb1f4
B1F2  06 80        LD B,0x80
B1F4  bd           CP L
B1F5  79           LD A,C
B1F6  28 02        JR z,0xb1fa
B1F8  ed 44        NEG
B1FA  80           ADD A,B
B1FB  32 18 f0     LD (0xf018),A
B1FE  c9           RET
B1FF  4f           LD C,A
B200  87           ADD A,A
B201  17           RLA
B202  e6 01        AND 1
B204  6f           LD L,A
B205  79           LD A,C
B206  c6 40        ADD A,0x40
B208  87           ADD A,A
B209  17           RLA
B20A  e6 01        AND 1
B20C  67           LD H,A
B20D  22 10 f0     LD (0xf010),HL
B210  79           LD A,C
B211  fe 80        CP 0x80
B213  38 02        JR c,0xb217
B215  d6 80        SUB 0x80
B217  fe 40        CP 0x40
B219  38 04        JR c,0xb21f
B21B  d6 7f        SUB 0x7f
B21D  ed 44        NEG
B21F  32 17 f0     LD (0xf017),A
B222  c9           RET
B223  3a 05 e5     LD A,(0xe505) ; jugador_y
B226  d6 10        SUB 0x10
B228  32 00 f0     LD (0xf000),A
B22B  3a 07 e5     LD A,(0xe507) ; jugador_x
B22E  d6 08        SUB 8
B230  32 01 f0     LD (0xf001),A
B233  21 10 f0     LD HL,0xf010
B236  36 00        LD (HL),0
B238  3a 00 f0     LD A,(0xf000)
B23B  93           SUB E
B23C  30 03        JR nc,0xb241
B23E  ed 44        NEG
B240  34           INC (HL)
B241  23           INC HL
B242  36 00        LD (HL),0
B244  e6 f0        AND 0xf0
B246  5f           LD E,A
B247  3a 01 f0     LD A,(0xf001)
B24A  92           SUB D
B24B  30 03        JR nc,0xb250
B24D  ed 44        NEG
B24F  34           INC (HL)
B250  1f           RRA
B251  1f           RRA
B252  1f           RRA
B253  1f           RRA
B254  e6 0f        AND 15
B256  83           ADD A,E
B257  21 74 b2     LD HL,0xb274
B25A  cd 62 40     CALL 0x4062 ; add_a_to_hl
B25D  7e           LD A,(HL)
B25E  32 17 f0     LD (0xf017),A
B261  c9           RET
B262  3a 09 e6     LD A,(0xe609)
B265  67           LD H,A
B266  cd 6b 40     CALL 0x406b ; vec_4dcf
B269  af           XOR A
B26A  29           ADD HL,HL
B26B  8f           ADC A,A
B26C  29           ADD HL,HL
B26D  8f           ADC A,A
B26E  29           ADD HL,HL
B26F  8f           ADC A,A
B270  6c           LD L,H
B271  67           LD H,A
B272  eb           EX DE,HL
B273  c9           RET
