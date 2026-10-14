# 11. Notas para el remake (Maze of Knight)

Las decisiones de este capítulo son **propuestas [I]**, no hechos del original. Sirven de punto de partida para la fase 5 (Unity) y hay que validarlas jugando.

## Unidades y conversión

| Original | Propuesta en Unity |
|---|---|
| 1 sprite o personaje = 16 px | **1 unidad** de Unity |
| Sala de 256 × 160 px | 16 × 10 unidades (un bloque de 32 px = 2 unidades) |
| Tick de lógica a ~30 Hz | lógica en `FixedUpdate` a 60 Hz, con tiempos ×2 y velocidades ×½ por tick (o en px/s directamente) |
| Velocidad en px/tick | × 30 / 16 = unidades/s (andar: 1,7 px/tick ≈ 3,2 u/s) |
| Salto de 54 / 42 px | 3,4 / 2,6 unidades de alto (reescalar si el feel HK lo pide) |

## Qué conservar (identidad de Galious)

1. **Estructura:** un castillo conectado y 10 mundos-mazmorra con llave, palabra, lápida y jefe.
2. **Dos personajes intercambiables**, con capacidades complementarias:
   - Popolon: salto alto, romper paredes, cruzar puertas giratorias;
   - Aphrodite: agua y disparos múltiples.
   - Los objetos que compensan las diferencias: Ring, Pendant y Bread & Water.
3. **Secretos del laberinto:** piedras con objetos, paredes que suenan, muros que se cierran a tu espalda, escaleras que se retiran durante un minuto, eventos al limpiar salas, hadas.
4. **Economía de tres recursos** (flechas, monedas y llaves) y tiendas de dioses.
5. **Jefes con una regla de debilidad** (arma, zona o momento) y los objetos de mundo que los facilitan.
6. **Dependencias de objetos** (capítulo 4) para que la exploración siga siendo un metroidvania con llaves.

## Qué adaptar al estilo Hollow Knight

| Original | Adaptación propuesta |
|---|---|
| Salas fijas con corte | transición con fundido y cámara que encuadra la sala; parallax de fondo y luces 2D (URP) |
| Salto rígido | salto con control de altura (ya existe en Popolon), aceleración y coyote time; Aphrodite con salto más corto pero quizá un planeo |
| Espada corta horizontal | ataque con dirección (arriba y abajo) y **pogo** hacia abajo; conservar que solo cuente en la zona vulnerable de los jefes |
| Daño por contacto continuo | invulnerabilidad tras el golpe y retroceso legible |
| Armas secundarias con flechas | mantener el recurso flechas como "alma" o munición; la mina daña también al jugador |
| Escaleras | en el original, a media escalera solo se sube o se baja. **Ya en el prototipo:** con ← o → te sueltas hacia un lado con sitio libre y caes con control (`RemakeRules.LeaveLadderSideways`, a petición del usuario) |
| Dash | no existe en el original. Si se añade, que no rompa los muros que se cierran ni las escaleras que se retiran (dar tiempo a reaccionar) |
| Contraseña | guardado en santuarios de Demeter |
| Teclear la palabra | selector o recitado con mando |
| Mundo 10 en un sitio al azar de 4 | se puede conservar como rejugabilidad, con la campana como pista |

## Datos para el importador de Unity

Las herramientas de `tools/extract/` generan, **en local y desde la ROM del usuario**:
- salas (`rooms.json`, `blocks.json`), **colisión por sala** (`collision/<mapa>.json`), enlaces, entidades (`entities.json`) y eventos de sala (`room_events.json`);
- catálogos de enemigos y objetos (`enemies.json`, `items.json`);
- jefes (`boss_index.json`), música (`score.json`) y efectos (`sfx.json`).

Propuesta para la fase 5: un importador que lea esos JSON y cree los ScriptableObjects y Tilemaps de cada sala con **tiles y sprites nuevos**, mapeando cada tile original a una clase (sólido, fondo, escalera, agua, lava) y cada objeto de sala a su prefab (puertas con llave, giratorias, de un sentido, muros que se cierran, paredes que se rompen). Así el remake reproduce la **disposición** del original sin distribuir sus gráficos.

**Antes de importar: todo resuelto.**
- **Colisión de cada tile:**
  - sólido ≥ 0xBA (≥ 0xF0 con jefe);
  - escalera 9A/9B;
  - agua B6/B7;
  - lava B8/B9.

  Cuadrícula por sala en `extracted/maps/collision/`; ver [world_maps.md](../world_maps.md#colisión-de-los-tiles-co).
- **Agua y lava:** animación por color (agua) y por patrón (lava), daño por personaje y Carpet. Ver [world_maps.md](../world_maps.md#agua-y-lava-co).
- **Sala del jefe:** la decoración se apaga y el jefe se dibuja con tiles, comprobado en los 10. En el remake, el jefe será un sprite o rig propio y el "apagón" un cambio de iluminación. Ver [boss_ai.md](../boss_ai.md#cómo-se-dibuja-el-jefe-oc).
- **Variables:**
  - E1C5: tempo del combate;
  - E1C9: música en pausa;
  - E500 = 4: retroceso del jugador;
  - objeto de sala de clase 3: puerta giratoria de un solo sentido.

  Ver [music.md](../music.md), [boss_ai.md](../boss_ai.md) y [entities.md](../entities.md).

**Cerrado también:**
- E1C6 es una variable muerta del driver;
- el jefe se mueve con posición lógica al píxel y dibujo ajustado a la rejilla de 8 px;
- la puesta en escena (decoración apagada y jefe hecho de tiles) se cumple en los 10 jefes.

Para el remake, el jefe puede moverse suave (sprite o rig propio); el salto de 8 px es una limitación del MSX, no parte del diseño.
