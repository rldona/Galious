# Máquina de estados global (fase 2)

Fuente: análisis estático de la vista `galious_g0_b01-03.elf` (REA/Ghidra) y bytes de la ROM.
Leyenda: **[C]** confirmado en el código · **[O]** observado en openMSX con `tools/openmsx/galious_state_trace.tcl` · **[O*]** observado jugando, pero con alguna ayuda forzada (invencibilidad o posición escrita en RAM); la transición la decide el código real · **[F]** el flag que dispara el estado se escribió a mano y no se ha visto en juego normal · **[I]** interpretación pendiente de verificar.

## Bucle por frame [C]

`isr_keyi` (0x40EF, en H.KEYI) → sonido (bancos 13/14, `CALL 6004`) → `game_frame` (0x4277) con `EI`, protegido por `frame_busy` (E005).

`game_frame`:
1. `frame_counter` (E003) += 1.
2. Si `game_state` < 3, apila `attract_check_start` (0x4BFA) como dirección de retorno.
3. `jump_table_a(game_state)` → tabla en 0x428E (21 entradas).

Cada estado recibe `B = game_substate` (E001) y suele despachar con `DJNZ` por subestado.

## Variables y helpers [C]

| Dirección | Nombre | Uso |
|---|---|---|
| E000 | `game_state` | índice de la tabla 0x428E |
| E001 | `game_substate` | paso dentro del estado |
| E002 | `game_flags` | bit 6 = partida en curso: vale 0x40 desde el estado 3 y vuelve a 0 al salir del game over [O]; con él, `sound_request` suena aunque el sonido de demo esté apagado [C] |
| E019 | `player_alive` [O] | 1 mientras se juega; pasa a 0 al morir y el estado 5 salta al 6 [O] |
| E01A | `room_exit_dir` [O] | lado por el que se sale de la pantalla: 1 = arriba, 2 = abajo, 3 = izquierda, 4 = derecha; dispara el estado 10 [O] |
| E008 / E009 | `input_held` / `input_new` [O] | teclas mantenidas / recién pulsadas: bit 0 arriba, 1 abajo, 2 izquierda, 3 derecha, 4 espacio, 5 M |
| E041 | `world` [O] | 1 = castillo, 2 = mundo 1, …; al entrar por una puerta, E041 = E688 + 1 [C][O*] |
| E042 | `room` [O] | número de sala; la sala inicial del castillo es la 1 |
| E052 / E056 | vitalidad de Aphrodite / Popolon [O] | escribir E056 llena la barra VIT de Popolon |
| E062 + n | estado de la puerta n [C] | bit 1 = desbloqueada, bit 0 = abierta (persiste) |
| E648–E64B | vecinos de la sala [O] | arriba, abajo, izquierda, derecha; FF = no hay salida; ≥ F6 = salida del mundo al castillo [C][O*] |
| E688 / E689 / E68A / E68B | puerta de la sala [C][O*] | número de puerta (= mundo de destino − 1), estado (copia de E062+n), Y, X |
| E500 | `player_action` [O] | 0 = en el suelo, 1 = saltando, 3 = en una escalera |
| E505 / E507 | `player_y` / `player_x` [O] | Y = 0x78 en el suelo de la primera sala; un salto sube ~51 px. X en píxeles de juego (pantalla = X + 32); se anda a 1 px por frame |
| E058 / E059 | contadores de vidas (BCD) [O] | `life_lost` resta de la ranura E058+E501 y, si está a 0, de la otra (alternando E501). Al empezar valen 1 y 0: la segunda muerte da game over [O] |
| E003 | `frame_counter` | |
| E004 | `state_timer` | cuenta atrás; `set_game_state` lo pone a 0x20 |
| 43A9 | `set_game_state(A)` | E000=A, E004=0x20, E001=0 |
| 4442 / 4443 | `reset_substate` / `set_substate(A)` | |
| 435B / 435E | `next_substate_timer(A)` / `next_substate` | E004=A; E001++ |
| 4439 / 443B | `next_game_state` / `next_game_state_timer(A)` | E004=0x20 (o A); E000++; E001=0 |
| 4BFA | `attract_check_start` | en estados 0 y 2, cualquier tecla → estado 1; en el estado 1, espacio/disparo (máscara 0x30) → E002=0x40, estado 3 [C][O] |

## Estados

| # | Entrada | Transiciones observadas | Interpretación |
|---|---|---|---|
| 0 | 42B8 | → 1 a los ~6 s o al pulsar una tecla [O] | intro antes del título [O] |
| 1 | 430C | → 2 a los ~4,5 s; → 3 al pulsar espacio [O]; → 18 con la tecla L pulsada (`98E5` = SNSMAT fila 4, bit 1) [C][O] | pantalla de título [O] |
| 2 | 433C | 4 subestados (tabla 0x4340); → 0 al acabar (~27 s) o al pulsar una tecla, escribiendo FF y después ++ [O] | demo jugada [O] |
| 3 | 43F0 | → 4 a los ~0,8 s [O] | inicio de partida; parpadeo de texto desde 7D71 vía WRTVRM [I] |
| 4 | 4420 | → 5 a los ~0,9 s [O]; → 14 si E690≠0 [C] | entrar a la sala / reaparecer [I] |
| 5 | 4462 | → 6 si E019=0 [C][O]; según flags → 8, 9, 10, 11, 12, 13, 15, 16, 17, 19, 20 [C] | **bucle de juego**; se mantiene con el jugador quieto [O] |
| 6 | 44F2 | → 4 a ~1 s si quedan vidas; → 7 a ~0,5 s si E058 y E059 están a 0 [O] | vida perdida [O]; ¿E501 = personaje activo (Popolon/Aphrodite)? [I] |
| 7 | 454F | → 0 a los ~7 s, escribiendo FF y después ++, y borra el bit 6 de E002 [O]; → 4 si E026≠0 [C] | game over [O]; E026≠0 = ¿continuar? [I] |
| 8 | 4599 | → 5 a los ~3 s, ya en el mundo [O*] | **entrar en un mundo por una puerta**: abajo sobre una puerta abierta (E689 bit 0), con Y = Y_puerta + 0x18 y X entre X_puerta + 14 y + 18; la HUD pasa a «WORLD n» [C][O*] |
| 9 | 473A | → 5 al volver a pulsar F2 [O] | **pausa**: F2 (`9E07` = SNSMAT fila 6, bit 6, por flanco; solo con partida en curso); sonido 0xFD, E61D++ [C][O] |
| 10 | 4769 | → 5 a los ~0,4 s [O]; → 14 si E690≠0 [C] | cambio de pantalla: se dispara con E01A según el lado por el que se sale (1 arriba, 2 abajo, 3 izquierda, 4 derecha) [O]; pone E01A=0 y recalcula E042 con `4792` [C] |
| 11 | 47CF | → 5 al volver a pulsar F1 [O] | **menú de objetos (ITEM)**: F1 (`9E21` = SNSMAT fila 6, bit 5, por flanco); sonido 0x2B [C][O] |
| 12 | 484F | → 5 al pulsar una tecla (E008 bit 5 = M) [C] | disparado por E01B; forzado: pantalla con una bola de cristal [F] |
| 13 | 46AD | → 5 a ~1,5 s [O] | **volver al castillo**: al salir de una sala por un lado cuyo vecino es ≥ F6 (p. ej. subiendo por la escalera de la entrada del mundo 1); cartel «CASTLE !!» y reaparece en la puerta del castillo [C][O*] |
| 14 | 488E | → 10 al pulsar abajo, saliendo por abajo (E01A = 2) [C] | **sala especial de un mundo** (su última sala, una sala de dios): al cargarla, el estado 10 salta al 14 en vez de al 5 [C][O]. Ver `world_maps.md` |
| 15 | 4959 | → 5 a ~1 s; el jefe sigue apareciendo ya en el 5 (E0D2: 2 → 4) [O] | **invocar al jefe**: escribir su nombre en el teclado en la sala del jefe (ver «Jefes») [C][O*]. Mundo 1: YOMAR en la sala 13 → esqueleto de dragón |
| 16 | 48C5 | 5 subestados (tabla 0x48C9); abajo → 16.3 → 16.4 → 5 en la misma sala [O*] | **entrar en la sala de un dios**: abajo sobre un objeto E680 (Y = Y_obj + 0x10, X = X_obj + 6…10; el tipo 3 exige además E099 ≠ 0). E690 = tipo. Hay tiendas (Hefesto, Ares, Demeter), Asclepio (pistas) y Demeter tipo 1 (contraseña) [C][O*] |
| 17 | 49BA | 9 subestados (tabla 0x49BE) | disparado por E024; forzado: la sala se encoge y aparece una escena del castillo por fuera con personajes; ¿cinemática? [F] |
| 18 | 4A69 | → 4 tras validar el código (reinicia RAM E016-E0FF, E200-EAFE, EC00-F03F y restaura desde la contraseña) [C] | **introducir contraseña**: desde el título con L pulsada; pide el código secreto y Return [O] |
| 19 | 4B5B | → 5 a ~2 s [O] | **jefe derrotado**: E02C = 1 cuando su vida (E710) llega a 0. Activa el bit 2 de E061 + E041 (ya no se puede invocar) y suelta la llave (objeto 0x0C en E740) [C][O*] |
| 20 | 4BD8 | → 5 cuando E53F = 0, a ~10 s [O] | **fanfarria de la llave**: al coger la llave 0x0C dentro de un mundo; desbloquea la puerta del mundo siguiente (E064 = 2 tras el mundo 1) [C][O*] |

Prioridad del despacho en el estado 5 [C]: E020 → 15, E01F=1 → 8, E01F=2 → 13, E01C → 16, E031 → 20, E01A → 10, E01B → 12, E024 → 17, E02C → 19. Después van las comprobaciones `9E07` → 9 y `9E21` → 11.

## Jefes [C][O*]
- **Sala del jefe** de cada mundo: tabla en 0x5DFB indexada por E041. `5DE4` devuelve Z si E042 coincide y el jefe no está derrotado (bit 2 de E061 + E041).
- **Al entrar en esa sala** (b00:5141 → b02:9AE5): E615 = 1 (escuchando) y se limpia el búfer de texto E6B0–E6BF.
- **Cada tecla** (`9C13`) se añade al final del búfer (b02:9B3C). Si el final coincide con la palabra del mundo (tabla de 0x9B55, de 3 bytes por mundo: puntero y longitud), E615 = 0 y E020 = 1 → estado 15 (b02:9B02, 9B18).
- **E0D2** es la fase del jefe: 0 ninguno, 2 apareciendo, 4 luchando. El objeto del jefe empieza en E700: E700 = número de jefe, E705 = Y, E706 = X, E710 = vida (20 = 0x14 en el jefe 1).
- **Daño** (b03:BC9F–BD40): solo cuentan las armas permitidas por la tabla de vulnerabilidades de 0xBD0A (un byte por jefe; el bit 0 es el arma principal). Si el arma no vale, suena 0x1D y no hace daño. Cada golpe resta 1 o 2 a E710; en 0 → E02C = 1 (estado 19), sonido 0x84.

| Mundo | Sala del jefe | Palabra | Jefe (5E05) | Vulnerabilidad (0xBD0A[jefe]) |
|---|---|---|---|---|
| 1 | 13 | YOMAR | 1 | 0x21 |
| 2 | 9 | ELOHIM | 4 | 0x21 |
| 3 | 13 | HAHAKLA | 2 | 0x09 |
| 4 | 14 | BARECHET | 11 | 0x03 |
| 5 | 17 | HEOTYMEO | 10 | 0x21 |
| 6 | 13 | LEPHA | 3 | 0x29 |
| 7 | 10 | NAWABRA | 7 | 0x11 |
| 8 | 7 | ASCHER | 8 | 0x05 |
| 9 | 25 | XYWOLEH | 6 | 0x11 |
| 10 | 5 | HAMALECH | 9 | 0x3F |

La tabla BD0A está indexada por número de jefe, no por mundo (corregido; antes se leía por mundo). Bits y fichas de cada jefe en `boss_ai.md`.

Comprobado jugando en el mundo 1 [O*], con invencibilidad, viaje forzado a la sala 13 y la vida del jefe bajada a 1 antes del golpe final; todo lo demás lo hizo el juego: YOMAR → estado 15 → aparece el esqueleto de dragón → golpe con la espada → estado 19 → llave → estado 20 → E064 = 2. Bits: 0 espada, 1 mina, 2 fuego rodante, 3 fuego, 4 flecha de cerámica, 5 flecha (`boss_ai.md`).

## Salas de dioses (objetos E680) en el castillo [O*]
E680–E685: tipo, ?, Y, X, puntero (2 bytes). Recorridas las salas 0–130 del castillo con `tools/openmsx/scan_rooms.py` y probado cada objeto con `try_object.py`:

| Sala | Tipo | Dios / función |
|---|---|---|
| 72 | 1 | Demeter: contraseña |
| 115 | 2 | Hefesto: tienda |
| 87 | 3 | no se entra sin E099 ≠ 0 (¿hace falta un objeto?) |
| 65 | 5 | Ares: tienda |
| 5 | 6 | Hefesto: tienda |
| 55 | 7 | Hefesto: tienda |
| 54 | 8 | Asclepio: pista (un hada recupera vitalidad) |
| 4 | 9 | Ares: tienda |
| 10 | 10 | Ares: tienda |
| 32 | 11 | Demeter: tienda |

Puertas de mundo (E688) en el castillo: sala 104 → 1, 98 → 2, 63 → 3, 57 → 4, 62 → 5, 114 → 6, 6 → 7, 23 → 8, 108 → 9. Ninguna en las salas 0–130 lleva al mundo 10.

## Puertas y llaves [C][O*]
- Al cargar la sala (b02:9971) se rellenan E688–E68B, y E689 = E062+n & 3.
- Si E689 tiene el bit 1 (desbloqueada) y el jugador está en la puerta, se abre sola: 4 pasos de animación (E68D/E68E, sonido 0x22). Al terminar, E689 y E062+n pasan a bit 0 (abierta) y se borra el bit 1 (b02:99C8).
- Al coger el **objeto 0x0C** (rutina de recogida b03:B701) se hace `(E061 + E041 + 1) |= 2`: en el castillo desbloquea la puerta del mundo 1; dentro del mundo k, la del k+1, y además activa E031 (estado 20).
- Con la puerta abierta, abajo en la puerta → E01F = 1 → estado 8 → mundo E688 + 1. Se ha visto con la puerta abierta a mano (E689 = 1) en la sala 104.

## Cómo trazarlo en openMSX
```
openmsx -cart "rom/<tu ROM>.rom" -script tools/openmsx/galious_state_trace.tcl
```
Escribe `logs/state_trace_<segundos>.tsv` con una línea por cada cambio de E000/E001. Columnas: tiempo emulado, `frame_counter`, variable, valor antiguo y nuevo, `writer_pc` (la instrucción que escribe, con nombre sacado de los seeds), `caller` (palabra en la cima de la pila, que solo es fiable si quien escribe es un helper hoja) y una foto de los flags de despacho en cada cambio de estado. En la consola (F10): `galious_trace stop` / `galious_trace start [fichero]`. Sobrevive a `loadstate`: vuelve a poner el watchpoint y anota una línea `machine_switch`.

### Partida automatizada
`tools/openmsx/omsx.py serve <args de openmsx>` arranca openMSX controlable desde fuera (hay que hacer `set power on`). Después:
`omsx.py run SEG [left,right,up,down,space]` emula con teclas pulsadas y pausa, `omsx.py shot NOMBRE` hace una captura (necesita `set renderer SDLGL-PP` y `set throttle on`), `omsx.py ram E500 20` vuelca memoria y `omsx.py 'cualquier comando tcl'` (por ejemplo `savestate x` / `loadstate x`). Controles en el juego: flechas para andar, arriba para saltar y, en una escalera (X entre 116 y 122 en la primera sala), arriba o abajo para trepar.

## Limitaciones de Ghidra/REA
- `jump_table_a` lee su tabla de la dirección de retorno, pero Ghidra cree que el `CALL` retorna y decodifica la tabla como código. Por eso `game_state_00` (42B8), `game_state_02_sub_00` (4348) y `game_state_16_sub_00` (48D3) quedan absorbidos en el llamador. Se leen con `procedure_assembly` del llamador o directamente de los bytes.
- `farcall_b04_6551` (424C) y `farcall_b08_9d5f` (4260) quedan absorbidos en funciones del banco que les hace `JP`.
