; tipo 0x60  (hijo, no aparece en salas)  descriptor c7 d8 0a 04 00
; ---- inicio (b04:66B4[95]) ----
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
B133  cd 3e b1     CALL 0xb13e
B136  eb           EX DE,HL
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
B13E  dd 73 0e     LD (IX+14),E ; vx_lo
B141  dd 72 0f     LD (IX+15),D ; vx
B144  c9           RET
; ---- actualizacion (b06:B472[95]) ----
67AF  4d           LD C,L
67B0  26 00        LD H,0
67B2  44           LD B,H
67B3  29           ADD HL,HL
67B4  29           ADD HL,HL
67B5  09           ADD HL,BC
67B6  01 b6 67     LD BC,0x67b6
67B9  09           ADD HL,BC
67BA  c9           RET
8476  dd 7e 01     LD A,(IX+1) ; estado
8479  3d           DEC A
847A  28 10        JR z,0x848c
847C  3d           DEC A
847D  28 29        JR z,0x84a8
847F  dd 7e 05     LD A,(IX+5) ; y
8482  fe 90        CP 0x90
8484  c0           RET nz
8485  dd 36 09 c3  LD (IX+9),0xc3 ; d0
8489  c3 07 9a     JP 0x9a07
848C  dd 7e 05     LD A,(IX+5) ; y
848F  e6 0f        AND 15
8491  c0           RET nz
8492  dd 7e 09     LD A,(IX+9) ; d0
8495  e6 07        AND 7
8497  ee 07        XOR 7
8499  20 09        JR nz,0x84a4
849B  11 00 04     LD DE,0x400
849E  cd 37 b1     CALL 0xb137
84A1  c3 07 9a     JP 0x9a07
84A4  dd 34 09     INC (IX+9) ; d0
84A7  c9           RET
84A8  dd 7e 05     LD A,(IX+5) ; y
84AB  d6 90        SUB 0x90
84AD  20 04        JR nz,0x84b3
84AF  3c           INC A
84B0  32 28 e6     LD (0xe628),A
84B3  dd 7e 05     LD A,(IX+5) ; y
84B6  e6 0f        AND 15
84B8  c0           RET nz
84B9  dd 7e 05     LD A,(IX+5) ; y
84BC  fe a0        CP 0xa0
84BE  ca 1c b4     JP z,0xb41c
84C1  dd 35 09     DEC (IX+9) ; d0
84C4  c9           RET
9A07  dd 34 01     INC (IX+1) ; estado
9A0A  c9           RET
B137  dd 73 0c     LD (IX+12),E ; vy_lo
B13A  dd 72 0d     LD (IX+13),D ; vy
B13D  c9           RET
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
