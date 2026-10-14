# Galious Design Bible

Documento de diseño de **Maze of Knight**, remake de *Knightmare II: The Maze of Galious* (Konami, MSX, 1987) para Unity con estética al estilo de Hollow Knight. Recoge **qué hace el juego original**, con sus reglas y números, para reimplementarlo desde aquí y no traduciendo el código ensamblador.

Es la síntesis de la ingeniería inversa de las fases 0–3. Cada capítulo enlaza al documento técnico de `docs/` donde está la prueba: direcciones de la ROM, capturas y grabaciones.

## Capítulos

| # | Capítulo | Qué contiene |
|---|---|---|
| 1 | [El mundo](01_mundo.md) | Castillo y 10 mundos, salas, conexiones, progresión, puertas y llaves |
| 2 | [El jugador](02_jugador.md) | Controles, Popolon y Aphrodite, física, vitalidad, experiencia, vidas |
| 3 | [Combate](03_combate.md) | Espada, armas secundarias, daño, invulnerabilidad, inmunidades |
| 4 | [Objetos](04_objetos.md) | Los 47 objetos: función de diseño, dónde están y cómo se consiguen |
| 5 | [Enemigos](05_enemigos.md) | Arquetipos y parámetros de los 53 tipos |
| 6 | [Jefes](06_jefes.md) | Los 10 jefes: invocación, patrones, debilidades |
| 7 | [Mecánicas de sala](07_salas.md) | Piedras, paredes, muros que se cierran, escaleras, hadas, eventos, agua |
| 8 | [Santuarios y economía](08_santuarios.md) | Dioses, tiendas, precios, resurrección, contraseña |
| 9 | [Audio](09_audio.md) | Músicas y efectos: cuándo suenan y qué comunican |
| 10 | [Interfaz](10_interfaz.md) | Marcador, menú de objetos, pausa, contraseñas |
| 11 | [Notas para el remake](11_remake.md) | Unidades, decisiones de adaptación, qué conservar y qué cambiar |
| 12 | [Estilo visual](12_estilo_visual.md) | Capas, paletas por mundo, luces, posproceso, transiciones y producción del arte |

## Convenciones

- **Confianza** de cada dato, como en el resto de `docs/`:
  - **[C]** leído en el código de la ROM;
  - **[O]** observado en el emulador (openMSX);
  - **[O*]** observado forzando la RAM;
  - **[G]** solo lo dice la guía de GameFAQs (Vahan Nisanian, v1.3), sin contrastar;
  - **[I]** interpretación nuestra.

  Lo que no lleva marca se resume de un documento técnico enlazado, donde sí la tiene.
- **Unidades del original:**
  - **Píxel:** la pantalla de juego mide 256 × 160 px (32 × 20 tiles de 8 px) bajo un marcador de 32 px. Una sala se compone de 8 × 5 bloques de 32 × 32 px.
  - **Tick:** el bucle del juego corre a **~30 ciclos por segundo**, uno cada dos frames de vídeo a 60 Hz [O]. Los tiempos en "ticks" son ciclos del juego; los tiempos en "frames" son de vídeo a 60 Hz.
  - **Velocidad:** en px/tick, salvo que se diga otra cosa. Muchas velocidades de la ROM son punto fijo 8.8: 0x0100 = 1 px por tick.
- **Nombres:** los de objetos y jefes son los de la guía, que sigue los manuales. Los de enemigos son descriptivos y nuestros.
- **Propiedad intelectual:** la ROM, los gráficos, la música y los textos son de Konami. Esta biblia describe reglas y números; los recursos del remake son nuevos. Nada derivado directamente de la ROM se versiona (`rom/`, `extracted/` y `logs/` están en `.gitignore`).

## Documentos técnicos de referencia

- [state_machine.md](../state_machine.md): estados del juego, invocación de jefes, puertas.
- [world_maps.md](../world_maps.md): mapas y formato de salas.
- [entities.md](../entities.md): entidades, mecánicas de sala, santuarios.
- [player.md](../player.md): controles, personajes.
- [items_enemies.md](../items_enemies.md): objetos y enemigos.
- [enemy_ai.md](../enemy_ai.md): IA de enemigos.
- [boss_ai.md](../boss_ai.md): IA de jefes.
- [music.md](../music.md): música y efectos.
