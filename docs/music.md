# Música y sonido (fase 3)

Leyenda como en `state_machine.md`: **[C]** código · **[O]** observado en openMSX · **[I]** interpretación · **[G]** guía de GameFAQs.

```
python3 tools/openmsx/record_music.py          # graba el PSG de 0x81..0x95 (logs/omsx/music/)
MUSIC_MAX=420 python3 tools/openmsx/record_music.py 87 88 89 93   # temas largos: hace falta ver dos vueltas
python3 tools/extract/psg_to_midi.py           # extracted/music/<id>.mid + index.json (muestreo del PSG)
python3 tools/extract/score.py                 # extracted/music/score_<id>.mid + score.json (partitura decodificada)
```

Los MIDI buenos son los de `score.py`: salen de la partitura, con duraciones y bucles exactos. Los de `psg_to_midi.py` sirven como comprobación.

Los MIDI son la música original de Konami: van a `extracted/`, que no se versiona. Sirven de referencia para componer la banda sonora nueva, no para usarlos tal cual.

## Driver [C]

- **Petición:** `sound_request` (b00:4E1F) recibe en A un número.
  - menor que 0x80: efecto de sonido;
  - de 0x80 a 0xF9: música (se guarda en E1C7);
  - de 0xFA en adelante: órdenes. 0xFA corta la música actual antes de pedir otra (b00:5D0A).
- **Dónde corre:** mapea los bancos 13 y 14 en 6000/8000 mientras trabaja.
- **Tabla de canciones:** b13:743E + 6·(id & 0x7F), con tres punteros, uno por canal del PSG (A, B y C).
  - Se copian a las estructuras de canal E10C, E127 y E142, de 0x1B bytes cada una.
  - Hay 23 entradas, de 0x80 a 0x96; 0x80 y 0x96 están vacías.
- **Hardware:** el PSG es un AY-3-8910 a 1,79 MHz con 3 canales de tono más ruido y envolvente. La grabación lee sus 14 registros en cada frame (60 Hz).

## Formato de la partitura [C][O]

Lo interpreta b13:6197–6446. El driver corre una vez por frame y las duraciones se cuentan en frames.

| Byte | Significado |
|---|---|
| `00`–`BF` | **nota**: nibble alto = nota (C … B), nibble bajo + 1 = duración en unidades de tempo |
| `C0`–`CF` | **silencio** de (n + 1) unidades |
| `Dn` | **tempo**: una unidad = n frames |
| `E0`–`E5` | **octava**: periodo = tabla[nota] >> (6 − n). Tabla de 12 periodos en b13:62D0 (C = 0x1AB8 … B = 0x0E28) |
| `E6` / `EB` | bandera global E1C3 = 1 / 0 |
| `E7` | periodo + 1 (desafinado) |
| `E8 xx` | **modo guion** con el instrumento xx: cada nota ejecuta el guion de la tabla del instrumento (xx = 1–5: b13:65A7 + 0x18·(xx − 1); otro valor: b13:661F), una entrada por nota |
| `E9` | modo tono normal |
| `EA aa aa` | **salto**: es el bucle de la canción |
| `EC xx` | vibrato |
| `ED aa aa` / `EE` | llamada a subrutina / retorno |
| `Fn xx` | volumen n + 1 y parámetros de caída del volumen |
| `FE n aa aa` | repetir n veces |
| `FF` | fin del canal |

**Guiones de instrumento** (mismo intérprete que b13:64B0): una lista de pasos.
- `2x d [r12 r11]`: modo del mezclador (bit 1 tono, bit 0 ruido) y d frames por paso. Si x ≥ 8, también fija la envolvente.
- `1n` + paso: periodo de ruido 2n.
- `vp pp`: volumen v y periodo de tono de 12 bits.
- `FE n aa aa`: bucle. `FF`: fin.

Con guiones se hacen los arpegios, glissandos y percusiones, y las fanfarrias cortas (0x81–0x85) van enteras dentro de un guion.

**Validación:** `score.py` reconstruye frame a frame la nota de cada canal y la compara con la grabación del PSG.
- En las 21 canciones coincide en el 98–100 % de los frames.
- El resto son efectos que no simulo: vibrato, la caída del volumen y las variantes de tempo y transposición de E1C5/E1C9.

## Canciones

| Id | Duración (partitura) | Cuándo suena | Nombre probable [G] |
|---|---|---|---|
| 0x81 | 5,3 s | al invocar al jefe (estado 15, b00:49A5) [C] | |
| 0x82 | 3,2 s | b02:9E5C, junto a la pausa y el menú [C] | |
| 0x83 | 5,4 s | al aparecer el hada (b05:827E) [C][O] | |
| 0x84 | 3,9 s | jefe derrotado (b03:BD3A; estado 19) [C][O] | Take Back! [I] |
| 0x85 | 3,9 s | al recoger un objeto importante (b03:B797) [C] | |
| 0x86 | A y B: 11,2 s en bucle; C: 22,4 s volviendo a 11,2 s | demo / historia (estado 2, b00:4371) [C] | Story |
| 0x87 | 84,8 s, vuelve a 1,6 s (bucle de 83,2 s) | castillo, al empezar partida [O] | Castle |
| 0x88 | 83,2 s en bucle completo | castillo al volver (b00:5D1A y otros) [C]. Es la 0x87 sin sus primeros compases | Castle |
| 0x89 | 87,0 s, vuelve a 3,8 s | b05:834F (código del polvo de hada) [C]. Variante del tema del castillo [I] | |
| 0x8A | 32,0 s en bucle completo | dentro de los mundos (b00:5D21) [C][O] | World |
| 0x8B | 38,5 s, vuelve a 6,5 s | tras recoger objetos clave o derrotar al jefe (b03:B7AF, BD48) [C]. Variante del tema de mundo [I] | |
| 0x8C | 8,3 s, vuelve a 2,4 s | bola de cristal (estado 12, b00:4877) [C] | |
| 0x8D | 22,4 s, vuelve a 3,2 s | santuarios de los dioses (estado 16) [O] | Shrine |
| 0x8E | 3,3 s | al entrar en un mundo por la puerta (estado 8, b00:45DE) [C] | |
| 0x8F | 16,0 s, vuelve a 3,2 s | sala del jefe esperando la palabra (E615, b00:5CDC) [C] | Devil's Room |
| 0x90 | 22,8 s, vuelve a 3,6 s | combate con el jefe (E0D2, b00:5CEB) [C][O] | Great Demons |
| 0x91 | 3,2 s en bucle | cuando E0D4 ≠ 0 (b00:5CEF) y tras derrotar al jefe (b03:BD44) [C] | |
| 0x92 | 14,4 s | cinemática (estado 17, b00:4A16) [C] | |
| 0x93 | 60,3 s, vuelve a 9,1 s | cinemática (estado 17, b00:4A49) [C] | Ending [I] |
| 0x94 | 3,6 s | b03:A581, código del jugador [C] | |
| 0x95 | 8,0 s | vida perdida / fin de partida (estado 6, b00:4547) [C] | |

La guía cita también "Stone Monuments", que no está identificada.

## Conversión a MIDI (`psg_to_midi.py`)

- **Notas:** frecuencia = 1789772,5 / (16 · periodo). Empieza una nota cuando el canal tiene el tono activo y volumen, y cambia el periodo o sube el volumen. El volumen (0–15) pasa a velocity.
- **Ruido:** los canales con el ruido activo van a una pista de percusión (canal MIDI 10). El periodo de ruido decide entre caja (38) y charles (42).
- **Tempo:** fijo a 150 bpm, con 1 frame = 20 ticks, así que las duraciones son exactas aunque el compás no lo sea.
- **Bucle:** el menor periodo P tal que el final de la grabación se repite durante más de P + 10 s. Las duraciones de intro que salían así no son fiables: las buenas son las de la partitura.
- **Limitaciones:**
  - la envolvente del PSG (R11–R13) se trata como volumen fijo;
  - el vibrato y los portamentos salen como notas sueltas;
  - los efectos de sonido del juego, si suenan a la vez, se mezclan. Las grabaciones se hicieron en una sala vacía, la 1 del castillo.

## Pendiente

- Los efectos de sonido (peticiones < 0x80): tabla y catálogo.
- Identificar 0x82, 0x89, 0x91 y 0x94 en el juego.
