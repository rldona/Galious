# 9. Audio

Fuente: [music.md](../music.md), con el formato de partituras, la tabla de canciones, los efectos y la validación.

La música y los efectos del original son de Konami: el remake necesita **banda sonora y efectos nuevos**. Aquí se describe **qué comunica cada sonido y cuándo suena**, para que lo nuevo cumpla la misma función. Las partituras decodificadas (`extracted/music/score_*.mid`) sirven de referencia de estructura, pero no se versionan.

## Músicas

| Id | Cuándo | Función | Estructura |
|---|---|---|---|
| 0x86 | demo / historia | presentación | bucle de 11–22 s |
| 0x87 / 0x88 | castillo | tema principal del mundo exterior (la 0x88 es la versión sin entrada) | 84,8 s; vuelve a 1,6 s |
| 0x8A | dentro de los mundos | tema de mazmorra | 32 s en bucle |
| 0x8B | después de objetos clave o del jefe | variante del tema de mundo | 38,5 s |
| 0x8D | santuarios | calma, tienda | 22,4 s |
| 0x8F | sala del jefe, esperando la palabra | tensión, "Devil's Room" | 16 s |
| 0x90 | combate con el jefe | **se acelera** al cuarto y al octavo de la vida del jefe | 22,8 s |
| 0x8C | bola de cristal | — | 8,3 s |
| 0x92 / 0x93 | cinemática final | final | 12 s / 60 s |

**Jingles:**
- invocar al jefe (0x81);
- jefe derrotado (0x84);
- objeto importante (0x85);
- entrar en un mundo (0x8E);
- aparece el hada (0x83);
- vida perdida o fin de partida (0x95).

## Efectos que comunican reglas (conservar su función)

| Efecto | Qué dice al jugador |
|---|---|
| espadazo (0x13) | ataque |
| **golpe en pared o piedra (0x1E)** | **aquí hay algo**: la pista de las paredes que se rompen |
| piedra rota (0x20), algo se abre (0x2E) | se ha abierto un camino o ha aparecido algo |
| golpe sin efecto (0x1D) | el arma no sirve contra este jefe |
| entrar en el agua (0x24), daño en el agua (0x25) | peligro de ahogarse |
| vitalidad baja (0x2A) | aviso |
| puertas con llave (0x21, 0x22) y giratoria (0x23) | paso abierto |
| campana (0x28) | la puerta del mundo está cerca |
| comprar o recoger (0x2D), sin monedas (0x2F) | economía |

**Prioridad de efectos:** uno nuevo solo interrumpe al actual si su número es mayor o igual. Así los avisos importantes no se pisan. Conviene una regla equivalente en el mezclador del remake.
