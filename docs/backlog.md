# Backlog del remake (Maze of Knight)

Lo que falta por hacer en Unity. El estado de lo ya hecho está en [unity.md](unity.md).

## Enemigos y combate

- **Simplificaciones de las IA sin datos en las fichas** ([I] en el código):
  - quién elige la fase latente del pistón 0x04;
  - la tabla de umbrales 7626 de las cabezas 0x69;
  - el reparto por segmento de la desviación del rayo 0x06 (flujo 77FB);
  - 9D1C/9D2D y la x de la plataforma al bajar en los saltos entre pisos;
  - cómo cambia IX+9 en el chorro 0x60;
  - el orden del giro del hueso 0x54.
- **Rebote de los proyectiles:** el remake lo deja subir recto (vx invertida, vy −8) sin mirar paredes ni enemigos. Falta ver en el código qué hace después.
- **Dibujo de los jefes:** se usa un solo fotograma por jefe, sacado de la VRAM del combate (`boss_gfx.py`). El original cambia los nombres de tile con cada fotograma. El del mundo 3 (Elusive Dragon) solo enseña la cabeza, que es lo que se veía en el volcado.

## Comprobaciones pendientes en el emulador

- **Eventos de sala de los mundos** (enemigos marcados): las escaleras y los bloques de muro salen del código [C]. Solo el del mundo 1, sala 12, está visto [O].
- **Espadazos en el aire justo antes de aterrizar:** en la demo 0 (castillo 101), con espadazos cada 5 ticks y un aterrizaje hundido (169 → 168), el original anda un tick más tarde que el remake. Un espadazo suelto en el aire se comporta igual en los dos (player.md, "Comprobado con la demo 0").
- **Campana en el mundo 10:** su lista de salas depende de E06E y no se ha mirado.

## Prueba de partida completa

- `Editor/Walkthrough/` recorre cada mapa con el motor del jugador (todas las teclas, tick a tick) y lleva la progresión del original hasta el final (menú **Maze of Knight → Prueba de partida completa**, informe en `logs/unity/walkthrough.md`). Escrito pero sin resultados: la búsqueda del castillo (con los ascensores) no acabó la primera pasada en 6 minutos. Falta reducir estados: buscar sala a sala desde sus entradas, quitar del estado lo que no cambia el movimiento y guardar los estados compactos en vez de copias del motor.
- Encontrado al prepararla: las puertas de mundo están todas abiertas (`GameBootstrap.allDoorsOpen`). En el original cada una se desbloquea con la Gran Llave del mundo anterior (la del mundo 1, con la del castillo) y se abre con una animación (docs/state_machine.md, puertas). `BossFight` y `ShowWorld10Door` cambian el RoomModel importado en memoria (`right`, `doorWorld`).

## Estilo visual

- Arte dibujado para sustituir el generado, una fuente libre incluida en el proyecto (design_bible/12_estilo_visual.md).

