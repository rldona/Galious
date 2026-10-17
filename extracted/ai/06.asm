; tipo 0x06  (hijo, no aparece en salas)  descriptor 81 a0 08 ff 10
; ---- inicio (b04:66B4[5]) ----
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
B124  cd 2e b1     CALL 0xb12e
B127  18 0e        JR 0xb137
B12E  16 00        LD D,0
B130  5a           LD E,D
B131  18 0b        JR 0xb13e
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[5]) ----
67AF  4d           LD C,L
67B0  26 00        LD H,0
67B2  44           LD B,H
67B3  29           ADD HL,HL
67B4  29           ADD HL,HL
67B5  09           ADD HL,BC
67B6  01 b6 67     LD BC,0x67b6
67B9  09           ADD HL,BC
67BA  c9           RET
7083  dd 77 0a     LD (IX+10),A ; d1
7086  c9           RET
76CD  cd cf 77     CALL 0x77cf
76D0  21 e2 76     LD HL,0x76e2
76D3  e5           PUSH HL
76D4  dd 7e 01     LD A,(IX+1) ; estado
76D7  cd 50 40     CALL 0x4050 ; jump_table_a
76DA  ff 76        .word 0x76FF ; caso 0
76DC  1e 77        .word 0x771E ; caso 1
76DE  30 77        .word 0x7730 ; caso 2
76E0  6e 77        .word 0x776E ; caso 3
76FF  dd 7e 09     LD A,(IX+9) ; d0
7702  f6 c0        OR 0xc0
7704  3c           INC A
7705  dd 77 09     LD (IX+9),A ; d0
7708  dd 36 0b 10  LD (IX+11),0x10 ; d2
770C  dd 34 16     INC (IX+0x16) ; ay_lo
770F  cd e5 77     CALL 0x77e5
7712  dd 7e 16     LD A,(IX+0x16) ; ay_lo
7715  d6 06        SUB 6
7717  c0           RET nz
7718  dd 77 17     LD (IX+0x17),A ; ay
771B  c3 07 9a     JP 0x9a07
771E  cd 82 77     CALL 0x7782
7721  dd 34 17     INC (IX+0x17) ; ay
7724  cd e5 77     CALL 0x77e5
7727  dd 7e 12     LD A,(IX+0x12)
772A  fe 90        CP 0x90
772C  d8           RET c
772D  c3 07 9a     JP 0x9a07
7730  cd 82 77     CALL 0x7782
7733  dd 35 16     DEC (IX+0x16) ; ay_lo
7736  dd 7e 16     LD A,(IX+0x16) ; ay_lo
7739  3d           DEC A
773A  20 2b        JR nz,0x7767
773C  dd 36 09 81  LD (IX+9),0x81 ; d0
7740  dd 36 0b 02  LD (IX+11),2 ; d2
7744  2e b8        LD L,0xb8
7746  dd 66 13     LD H,(IX+0x13)
7749  cd 6e 40     CALL 0x406e ; vec_4dc8
774C  d2 1c b4     JP nc,0xb41c
774F  dd 36 08 30  LD (IX+8),0x30
7753  dd 36 0a b4  LD (IX+10),0xb4 ; d1
7757  dd 36 0b 08  LD (IX+11),8 ; d2
775B  dd 36 05 a8  LD (IX+5),0xa8 ; y
775F  3e 1c        LD A,0x1c
7761  cd 59 40     CALL 0x4059 ; sound_request
7764  c3 07 9a     JP 0x9a07
7767  cd e5 77     CALL 0x77e5
776A  dd 35 09     DEC (IX+9) ; d0
776D  c9           RET
776E  dd 35 08     DEC (IX+8)
7771  ca 1c b4     JP z,0xb41c
7774  dd 7e 08     LD A,(IX+8)
7777  e6 03        AND 3
7779  c0           RET nz
777A  dd 7e 0a     LD A,(IX+10) ; d1
777D  ee 0c        XOR 12
777F  c3 83 70     JP 0x7083
7782  dd 7e 05     LD A,(IX+5) ; y
7785  c6 10        ADD A,0x10
7787  dd 77 05     LD (IX+5),A ; y
778A  21 c5 77     LD HL,0x77c5
778D  dd 56 1e     LD D,(IX+0x1e)
7790  dd 5e 1f     LD E,(IX+0x1f)
7793  1a           LD A,(DE)
7794  d6 a0        SUB 0xa0
7796  1f           RRA
7797  1f           RRA
7798  e6 07        AND 7
779A  cd 83 40     CALL 0x4083
779D  08           EX AF,AF'
779E  13           INC DE
779F  21 ca 77     LD HL,0x77ca
77A2  1a           LD A,(DE)
77A3  d6 a0        SUB 0xa0
77A5  1f           RRA
77A6  1f           RRA
77A7  e6 07        AND 7
77A9  cd 83 40     CALL 0x4083
77AC  67           LD H,A
77AD  08           EX AF,AF'
77AE  6f           LD L,A
77AF  dd 7e 07     LD A,(IX+7) ; x
77B2  85           ADD A,L
77B3  94           SUB H
77B4  dd 77 07     LD (IX+7),A ; x
77B7  dd 66 1e     LD H,(IX+0x1e)
77BA  dd 6e 1f     LD L,(IX+0x1f)
77BD  23           INC HL
77BE  dd 74 1e     LD (IX+0x1e),H
77C1  dd 75 1f     LD (IX+0x1f),L
77C4  c9           RET
77CF  dd 7e 14     LD A,(IX+0x14)
77D2  3c           INC A
77D3  fe 15        CP 0x15
77D5  20 01        JR nz,0x77d8
77D7  af           XOR A
77D8  dd 77 14     LD (IX+0x14),A
77DB  21 e4 82     LD HL,0x82e4
77DE  cd 83 40     CALL 0x4083
77E1  dd 77 15     LD (IX+0x15),A
77E4  c9           RET
77E5  dd e5        PUSH IX
77E7  e1           POP HL
77E8  11 18 00     LD DE,0x18
77EB  19           ADD HL,DE
77EC  eb           EX DE,HL
77ED  dd 66 1e     LD H,(IX+0x1e)
77F0  dd 6e 1f     LD L,(IX+0x1f)
77F3  06 00        LD B,0
77F5  dd 4e 16     LD C,(IX+0x16) ; ay_lo
77F8  ed b0        LDIR
77FA  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
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
