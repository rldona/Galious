; tipo 0x04  vida 1  patron observado: quieto
; ---- inicio (b04:66B4[3]) ----
6B71  dd cb 02 a6  RES 4,(IX+2) ; flags
6B75  c9           RET
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
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
; ---- actualizacion (b06:B472[3]) ----
6F7A  dd 77 0c     LD (IX+12),A ; vy_lo
6F7D  c3 cf 71     JP 0x71cf
6F80  dd 7e 01     LD A,(IX+1) ; estado
6F83  3d           DEC A
6F84  28 39        JR z,0x6fbf
6F86  f2 e3 6f     JP p,0x6fe3
6F89  dd 35 08     DEC (IX+8)
6F8C  c0           RET nz
6F8D  dd 7e 0c     LD A,(IX+12) ; vy_lo
6F90  dd 77 08     LD (IX+8),A
6F93  dd 7e 05     LD A,(IX+5) ; y
6F96  fe b0        CP 0xb0
6F98  30 1c        JR nc,0x6fb6
6F9A  c6 08        ADD A,8
6F9C  6f           LD L,A
6F9D  dd 66 07     LD H,(IX+7) ; x
6FA0  cd 6e 40     CALL 0x406e ; vec_4dc8
6FA3  38 11        JR c,0x6fb6
6FA5  cd 71 40     CALL 0x4071
6FA8  38 0c        JR c,0x6fb6
6FAA  dd 7e 05     LD A,(IX+5) ; y
6FAD  c6 08        ADD A,8
6FAF  dd 77 05     LD (IX+5),A ; y
6FB2  dd 34 14     INC (IX+0x14)
6FB5  c9           RET
6FB6  dd 7e 01     LD A,(IX+1) ; estado
6FB9  fe 03        CP 3
6FBB  c8           RET z
6FBC  c3 07 9a     JP 0x9a07
6FBF  dd 35 08     DEC (IX+8)
6FC2  c0           RET nz
6FC3  dd 7e 0c     LD A,(IX+12) ; vy_lo
6FC6  dd 77 08     LD (IX+8),A
6FC9  dd 7e 14     LD A,(IX+0x14)
6FCC  b7           OR A
6FCD  c8           RET z
6FCE  dd 7e 05     LD A,(IX+5) ; y
6FD1  d6 08        SUB 8
6FD3  dd 77 05     LD (IX+5),A ; y
6FD6  dd 35 14     DEC (IX+0x14)
6FD9  c0           RET nz
6FDA  dd 7e 01     LD A,(IX+1) ; estado
6FDD  fe 03        CP 3
6FDF  c8           RET z
6FE0  c3 a6 ac     JP 0xaca6
6FE3  3d           DEC A
6FE4  20 1b        JR nz,0x7001
6FE6  cd f6 6f     CALL 0x6ff6
6FE9  d0           RET nc
6FEA  dd 34 01     INC (IX+1) ; estado
6FED  dd 36 0a 12  LD (IX+10),0x12 ; d1
6FF1  3e 0f        LD A,15
6FF3  c3 7a 6f     JP 0x6f7a
6FF6  3a 05 e5     LD A,(0xe505) ; jugador_y
6FF9  dd 96 05     SUB (IX+5) ; y
6FFC  3f           CCF
6FFD  d0           RET nc
6FFE  fe 19        CP 0x19
7000  c9           RET
7001  3a 05 e5     LD A,(0xe505) ; jugador_y
7004  dd be 05     CP (IX+5) ; y
7007  da bf 6f     JP c,0x6fbf
700A  c3 89 6f     JP 0x6f89
71CF  dd 77 08     LD (IX+8),A
71D2  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
ACA6  dd 35 01     DEC (IX+1) ; estado
ACA9  c9           RET
