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
- El resto son efectos que no simulo: vibrato, la caída del volumen y la variante de tempo de E1C5.
- **Variables del driver** [C]:
  - **E1C5:** escalón de tempo de la música del combate (0, 1 o 2 según quede más de 1/4, entre 1/8 y 1/4 o menos de 1/8 de la vida del jefe; b03:BC9F). Con 1 o 2, b13:61F5 toma la duración de las notas de otra tabla (62E8 o 6300), más rápida.
  - **E1C9:** **música en pausa**. La orden 0xFD (b00:4F87) la pone a 1, guarda en E1B3–E1BA el estado del driver (E1C0, E1C1, E1B1, E1C2) y los registros 0, 1, 8, 9 y 10 del PSG (tono del canal A y los tres volúmenes). La orden 0xFE (b00:5014) la pone a 0 y lo restaura. Con E1C9 = 1 no se aplican ni el tempo de E1C5 ni el silencio de E1C6, así que el jingle de pausa suena normal.
  - **E1C6:** si vale ≠ 0, b13:615E pone a 0 el volumen del canal. **Variable muerta**: solo se escribe con 0, al iniciar el sonido (b00:4DFC) y en el borrado de RAM del arranque (b00:40B9). No hay punteros, IX/IY, escrituras de 16 bits ni copias en bloque que la alcancen [C]. Un watchpoint de valores ≠ 0 durante un combate, la pausa, el menú y la partida no saltó [O]. Seguramente es un resto del driver compartido con otros juegos de Konami [I].

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

## Efectos de sonido [C]

`python3 tools/extract/sfx.py` → `extracted/sfx/sfx.json` y `sfx_<id>.wav`. Los WAV son una síntesis aproximada del PSG (onda cuadrada y ruido blanco), solo para identificarlos de oído; no se versionan.

- **Petición:** `sound_request` con A < 0x80 (b00:4F12).
- **Prioridad:** el propio número. Un efecto solo entra si su id es mayor o igual que el que está sonando (E1B0). Excepción: 0x25 (daño en el agua) no se reinicia si ya estás en el agua.
- **Formato:** cada efecto es un **guion**, con el mismo formato que los instrumentos de la música, en b13:73DC + 2·id. Suena en el canal de efectos (E15D, intérprete b13:64A5).
- **Órdenes:** 0xFA–0xFF son fundido, parada, pausa y reanudación de la música (b00:4F52–50B3).

**Comprobado** [O]: grabando el PSG durante un espadazo, el canal C reproduce frame a frame los 9 pasos del guion 0x13 (periodo, volumen y periodo de ruido). Los efectos suenan en el **canal C**, que deja de tocar la música mientras dura el efecto (en la grabación, la nota de música del canal C se corta) [I].

Hay 48 efectos (0x01–0x30). La columna "qué es" sale del código que los pide: "ficha" quiere decir que lo cita la ficha del enemigo en `enemy_ai.md`. Los que solo tienen dirección están sin identificar.

| Id | Duración | Tono (inicio→fin) | Ruido | Qué es |
|---|---|---|---|---|
| 0x01 | 0.18 s | A8→A7 | sí | b03:A2A1 |
| 0x02 | 0.10 s | A6→A8 | sí | b03:A3CC, b03:A52E |
| 0x03 | 0.15 s | D#5→F6 |  | bloque 0x2F que imita el salto [C ficha] |
| 0x04 | 0.12 s | C#7→A#7 |  | b09:A5E6 |
| 0x05 | 0.42 s | D7→D7 | sí | soldado 0x2D dispara [C ficha] |
| 0x06 | 0.43 s | E5→C5 |  | ave 0x27 cada 64 ticks [C ficha] |
| 0x07 | 0.43 s | A7→D7 | sí | b04:76C8 |
| 0x08 | 0.67 s | G2→F4 |  | 0x14 emerge del suelo [C ficha] |
| 0x09 | 0.35 s | B2→C3 |  | b04:7028, b06:ABC9 |
| 0x0a | 0.43 s | D3→D3 | sí | b02:815C, b06:A166 |
| 0x0b | 0.27 s | A8→A5 |  | gota 0x03 [C ficha] |
| 0x0c | 0.47 s | C8→C8 |  | orbes 0x36 al aterrizar [C ficha] |
| 0x0d | 0.65 s | — | sí | b04:78C8 |
| 0x0e | 0.40 s | F3→D3 | sí | b05:8FC5, b05:9F32 |
| 0x0f | 0.30 s | C#5→C7 |  | escupefuego 0x22 [C ficha] |
| 0x10 | 0.27 s | A4→C3 |  | erizo 0x11 / torreta 0x32 disparan [C ficha] |
| 0x11 | 0.43 s | B6→D#2 |  | b02:80F1, b02:874D |
| 0x12 | 0.43 s | A3→F3 | sí | b02:841F, b02:84CD, b05:91F9 |
| 0x13 | 0.15 s | A8→A7 | sí | **espadazo** (b03:A747) [C][O] |
| 0x14 | 0.22 s | D7→A7 | sí | b03:AA89 |
| 0x15 | 0.32 s | F#8→D7 | sí | (sin petición directa) |
| 0x16 | 0.08 s | G#6→D7 | sí | b03:ACD6 |
| 0x17 | 0.80 s | B3→B4 |  | b05:8468, b05:858E |
| 0x18 | 0.68 s | D6→D2 |  | b05:962B |
| 0x19 | 0.30 s | B7→D9 |  | b05:8DC0 |
| 0x1a | 0.67 s | B2→C#3 |  | b02:8909 |
| 0x1b | 0.43 s | B6→D#2 |  | b02:8C1A |
| 0x1c | 0.75 s | D3→D3 | sí | b04:775F, b06:AD77 |
| 0x1d | 0.70 s | A8→D7 | sí | **golpe sin efecto** (arma que no vale contra el jefe, b03:BCF4) [C] |
| 0x1e | 0.38 s | G#4→D#3 |  | **golpe en piedra o pared que suena** (b03:B83C, b09:A69E) [C][O] |
| 0x1f | 0.43 s | A8→A8 | sí | esqueleto 0x25 al enroscarse [C ficha] |
| 0x20 | 0.53 s | F4→B3 |  | **piedra rota** (b03:B857) [C] |
| 0x21 | 0.85 s | G2→F2 |  | **puerta con llave de los mundos** (b09:A19B) y embestida de 0x01 [C] |
| 0x22 | 0.67 s | A8→D8 | sí | **puerta con llave del castillo** (b09:A0A0) [C] |
| 0x23 | 1.00 s | D3→D#4 |  | **puerta giratoria** (b09:A278, A3B8) [C] |
| 0x24 | 0.75 s | A3→F3 | sí | **entrar en el agua** (b03:B063) [C] |
| 0x25 | 0.10 s | A2→A7 |  | **daño en el agua** (b03:B08E) [C][O] |
| 0x26 | 0.72 s | F5→A#2 |  | b03:AE63 |
| 0x27 | 0.82 s | B3→F#2 | sí | **golpe al jefe / jefe derrotado** (b03:BC94, BD50) [C] |
| 0x28 | 1.58 s | A7→A7 |  | **campana** (b00:5D63) [C] |
| 0x29 | 0.50 s | C#5→F#6 |  | b01:6153, al sumar vitalidad o experiencia [I] |
| 0x2a | 0.53 s | D7→D6 |  | **aviso de vitalidad baja** (< 6, b03:B3DC) [C] |
| 0x2b | 0.53 s | E5→C7 |  | **vida perdida** (estado 6, b00:44DD) [C] |
| 0x2c | 0.60 s | A6→F#7 |  | b01:686A, b02:934A |
| 0x2d | 0.60 s | D5→A#7 |  | **recoger un objeto / comprar** (b03:B764, b02:91CF) [C] |
| 0x2e | 1.12 s | C7→E7 |  | **algo se abre**: pared rota, evento de sala, objeto con condición, escalera retirada (b09:A6B0, b08:9D9D, b03:A051, A630) [C][O] |
| 0x2f | 0.35 s | B5→B5 |  | **no tienes monedas** en la tienda (b02:9220) [C] |
| 0x30 | 1.40 s | F#5→F#7 |  | santuario (b02:90AB; ¿resurrección?) [I] |

## Conversión a MIDI (`psg_to_midi.py`)

- **Notas:** frecuencia = 1789772,5 / (16 · periodo). Empieza una nota cuando el canal tiene el tono activo y volumen, y cambia el periodo o sube el volumen. El volumen (0–15) pasa a velocity.
- **Ruido:** los canales con el ruido activo van a una pista de percusión (canal MIDI 10). El periodo de ruido decide entre caja (38) y charles (42).
- **Tempo:** fijo a 150 bpm, con 1 frame = 20 ticks, así que las duraciones son exactas aunque el compás no lo sea.
- **Bucle:** el menor periodo P tal que el final de la grabación se repite durante más de P + 10 s. Las duraciones de intro que salían así no son fiables: las buenas son las de la partitura.
- **Limitaciones:**
  - la envolvente del PSG (R11–R13) se trata como volumen fijo;
  - el vibrato y los portamentos salen como notas sueltas;
  - los efectos de sonido del juego, si suenan a la vez, se mezclan. Las grabaciones se hicieron en una sala vacía, la 1 del castillo.

## En el remake

`tools/extract/audio_export.py` deja en `extracted/unity/audio/` los registros del PSG de cada canción, recortados a una vuelta. El bucle se busca en la propia grabación: de los periodos con los que se repite el final, el de vuelta más corta. Coinciden con la tabla de arriba. La 0x8F y la 0x93 salen con un bucle de dos vueltas, porque las vueltas no son idénticas byte a byte. También deja los efectos expandidos a un paso por frame. `Psg.cs` emula el AY-3-8910: tonos, ruido de 17 bits, envolvente y volumen logarítmico. `Sound.cs` aplica los registros a 60 Hz y pone el efecto en el canal C, con su prioridad. Es el audio de Konami: solo va en las compilaciones locales.

## Pendiente

- Identificar los efectos que solo tienen dirección (sobre todo los de b02 y los de enemigos sin ficha).
- Identificar 0x82, 0x89, 0x91 y 0x94 en el juego.
