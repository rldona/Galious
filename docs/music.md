# Música y sonido (fase 3)

Leyenda como en `state_machine.md`: **[C]** código · **[O]** observado en openMSX · **[I]** interpretación · **[G]** guía de GameFAQs.

```
python3 tools/openmsx/record_music.py          # graba el PSG de 0x81..0x95 (logs/omsx/music/)
MUSIC_MAX=420 python3 tools/openmsx/record_music.py 87 88 89 93   # temas largos: hace falta ver dos vueltas
python3 tools/extract/psg_to_midi.py           # extracted/music/<id>.mid + index.json
```

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

## Canciones

| Id | Duración | Cuándo suena | Nombre probable [G] |
|---|---|---|---|
| 0x81 | 5,3 s | al invocar al jefe (estado 15, b00:49A5) [C] | |
| 0x82 | 3,2 s | b02:9E5C, junto a la pausa y el menú [C] | |
| 0x83 | 5,4 s | al aparecer el hada (b05:827E) [C][O] | |
| 0x84 | 3,9 s | jefe derrotado (b03:BD3A; estado 19) [C][O] | Take Back! [I] |
| 0x85 | 3,9 s | al recoger un objeto importante (b03:B797) [C] | |
| 0x86 | intro 0,5 s + bucle 11,2 s | demo / historia (estado 2, b00:4371) [C] | Story |
| 0x87 | intro 26,8 s + bucle 83,2 s | castillo, al empezar partida [O] | Castle |
| 0x88 | intro 25,2 s + bucle 83,2 s | castillo al volver (b00:5D1A y otros) [C]. Es la 0x87 sin sus primeros compases | Castle |
| 0x89 | intro 29,0 s + bucle 83,2 s | b05:834F (código del polvo de hada) [C]. Variante del tema del castillo [I] | |
| 0x8A | intro 0,4 s + bucle 32,0 s | dentro de los mundos (b00:5D21) [C][O] | World |
| 0x8B | intro 7,0 s + bucle 32,0 s | tras recoger objetos clave o derrotar al jefe (b03:B7AF, BD48) [C]. Variante del tema de mundo [I] | |
| 0x8C | intro 2,6 s + bucle 5,9 s | bola de cristal (estado 12, b00:4877) [C] | |
| 0x8D | intro 3,5 s + bucle 19,2 s | santuarios de los dioses (estado 16) [O] | Shrine |
| 0x8E | 3,3 s | al entrar en un mundo por la puerta (estado 8, b00:45DE) [C] | |
| 0x8F | intro 12,7 s + bucle 25,6 s | sala del jefe esperando la palabra (E615, b00:5CDC) [C] | Devil's Room |
| 0x90 | intro 3,9 s + bucle 19,2 s | combate con el jefe (E0D2, b00:5CEB) [C][O] | Great Demons |
| 0x91 | intro 1,6 s + bucle 3,2 s | cuando E0D4 ≠ 0 (b00:5CEF) y tras derrotar al jefe (b03:BD44) [C] | |
| 0x92 | 14,4 s | cinemática (estado 17, b00:4A16) [C] | |
| 0x93 | intro 9,4 s + bucle 102,4 s | cinemática (estado 17, b00:4A49) [C] | Ending [I] |
| 0x94 | 3,6 s | b03:A581, código del jugador [C] | |
| 0x95 | 8,0 s | vida perdida / fin de partida (estado 6, b00:4547) [C] | |

La guía cita también "Stone Monuments", que no está identificada.

## Conversión a MIDI (`psg_to_midi.py`)

- **Notas:** frecuencia = 1789772,5 / (16 · periodo). Empieza una nota cuando el canal tiene el tono activo y volumen, y cambia el periodo o sube el volumen. El volumen (0–15) pasa a velocity.
- **Ruido:** los canales con el ruido activo van a una pista de percusión (canal MIDI 10). El periodo de ruido decide entre caja (38) y charles (42).
- **Tempo:** fijo a 150 bpm, con 1 frame = 20 ticks, así que las duraciones son exactas aunque el compás no lo sea.
- **Bucle:** el menor periodo P tal que el final de la grabación se repite durante más de P + 10 s. Se exporta la intro más una vuelta.
- **Limitaciones:**
  - la envolvente del PSG (R11–R13) se trata como volumen fijo;
  - el vibrato y los portamentos salen como notas sueltas;
  - los efectos de sonido del juego, si suenan a la vez, se mezclan. Las grabaciones se hicieron en una sala vacía, la 1 del castillo.

## Pendiente

- Descifrar el formato de las partituras (los datos a los que apuntan los canales) para sacar duraciones y compases reales en vez de muestrear el PSG.
- Los efectos de sonido (peticiones < 0x80): tabla y catálogo.
- Identificar 0x82, 0x89, 0x91 y 0x94 en el juego.
