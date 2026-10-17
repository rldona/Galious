; tipo 0x2f  vida 5  patron observado: quieto
; ---- inicio (b04:66B4[46]) ----
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
B12E  16 00        LD D,0
B130  5a           LD E,D
B131  18 0b        JR 0xb13e
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[46]) ----
6BD5  dd 36 01 00  LD (IX+1),0 ; estado
6BD9  c9           RET
9219  3a 15 e5     LD A,(0xe515)
921C  3d           DEC A
921D  20 0e        JR nz,0x922d
921F  dd 7e 01     LD A,(IX+1) ; estado
9222  cd 50 40     CALL 0x4050 ; jump_table_a
9225  7a 92        .word 0x927A ; caso 0
9227  43 92        .word 0x9243 ; caso 1
9229  88 92        .word 0x9288 ; caso 2
922B  43 92        .word 0x9243 ; caso 3
922D  dd cb 0d 7e  BIT 7,(IX+13) ; vy
9231  c4 6f b1     CALL nz,0xb16f
9234  cd d5 6b     CALL 0x6bd5
9237  cd a1 92     CALL 0x92a1
923A  d0           RET nc
923B  dd 7e 1f     LD A,(IX+0x1f)
923E  dd 77 05     LD (IX+5),A ; y
9241  18 4d        JR 0x9290
9243  3a 15 e5     LD A,(0xe515)
9246  3d           DEC A
9247  20 47        JR nz,0x9290
9249  3a 0a e5     LD A,(0xe50a)
924C  a7           AND A
924D  28 41        JR z,0x9290
924F  ed 5b 12 e5  LD DE,(0xe512)
9253  cd 37 b1     CALL 0xb137
9256  dd 7e 01     LD A,(IX+1) ; estado
9259  3d           DEC A
925A  20 0e        JR nz,0x926a
925C  cd 93 92     CALL 0x9293
925F  38 4a        JR c,0x92ab
9261  dd 7e 05     LD A,(IX+5) ; y
9264  fe 08        CP 8
9266  38 4d        JR c,0x92b5
9268  18 1e        JR 0x9288
926A  cd a1 92     CALL 0x92a1
926D  d0           RET nc
926E  cd d5 6b     CALL 0x6bd5
9271  dd 7e 1f     LD A,(IX+0x1f)
9274  dd 77 05     LD (IX+5),A ; y
9277  cd 90 92     CALL 0x9290
927A  3a 13 e5     LD A,(0xe513)
927D  cb 7f        BIT 7,A
927F  c8           RET z
9280  3e 03        LD A,3
9282  cd 59 40     CALL 0x4059 ; sound_request
9285  c3 07 9a     JP 0x9a07
9288  3a 13 e5     LD A,(0xe513)
928B  a7           AND A
928C  c0           RET nz
928D  c3 07 9a     JP 0x9a07
9290  c3 29 b1     JP 0xb129
9293  06 00        LD B,0
9295  dd 66 07     LD H,(IX+7) ; x
9298  dd 6e 05     LD L,(IX+5) ; y
929B  11 0f 00     LD DE,15
929E  c3 74 40     JP 0x4074
92A1  dd 7e 05     LD A,(IX+5) ; y
92A4  47           LD B,A
92A5  04           INC B
92A6  dd 7e 1f     LD A,(IX+0x1f)
92A9  b8           CP B
92AA  c9           RET
92AB  dd 7e 05     LD A,(IX+5) ; y
92AE  e6 f8        AND 0xf8
92B0  c6 08        ADD A,8
92B2  dd 77 05     LD (IX+5),A ; y
92B5  dd 34 01     INC (IX+1) ; estado
92B8  18 d6        JR 0x9290
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B129  16 00        LD D,0
B12B  5a           LD E,D
B12C  18 09        JR 0xb137
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B16F  dd 5e 0c     LD E,(IX+12) ; vy_lo
B172  dd 56 0d     LD D,(IX+13) ; vy
B175  cd 68 40     CALL 0x4068 ; neg_de
B178  dd 73 0c     LD (IX+12),E ; vy_lo
B17B  dd 72 0d     LD (IX+13),D ; vy
B17E  c9           RET
