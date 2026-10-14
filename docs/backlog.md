# Backlog del remake (Maze of Knight)

Lo que falta por hacer en Unity. El estado de lo ya hecho está en [unity.md](unity.md).

## Objetos

- **Remo** (0x10): en el mundo 5, salas 12 y 13, el tile de la superficie del agua (B6) se vuelve suelo.
- **Vela** (0x16): sin ella, la sala 6 del mundo 5 es oscura (sus paredes se dibujan como el fondo: muros invisibles, con la misma colisión).
- **Campana** (0x14): suena (0x28) al entrar en ciertas salas del castillo, según la puerta de mundo desbloqueada y aún sin abrir. Depende del sonido.

## Personajes

- **Vaciado gradual de la barra:** el daño, la curación y la experiencia van a un valor pendiente que se aplica 1 unidad cada 4 ticks; ahora se aplican de golpe.
- **Vidas de repuesto:** Popolon empieza sin ninguna y Aphrodite con 1; abrir el menú F1 suma una al activo y cerrarlo la gasta; el retrato solo sale si ese personaje tiene vidas (docs/items_enemies.md, "Vitalidad, experiencia y vidas").

## Santuarios y eventos

- **Guardar la partida en Demeter** (castillo, sala 72): sustituye a la contraseña de 45 letras (design bible, cap. 8 y 11).
- **Resurrección de la Muerte** (sala 87): depende de las vidas de repuesto (100 flechas, 100 monedas y 20 llaves; una vez por personaje).
- **Animación de entrar y salir de un santuario del castillo** (el jugador se hunde 16 px en ~1 s; música 0x8D / 0x88).
- **Objetos con condición del castillo:** al matar a los marcados de ciertas salas (E61C = 1) aparecen objetos en las salas 48, 75 y 24 (entre ellos el Hoi).
- **Muro del mundo 3, sala 4:** se abre al subir 5 veces seguidas por el pozo de la sala.

## Enemigos y combate

- **Escudos de los enemigos:** con el bit 2 de IX+2 (o el bit 1 si su lado no coincide) la espada hace "clink" sin daño y los proyectiles rebotan (invierten vx, suben a 8 px/tick, sonido 0x1D) (docs/player.md, "Armas secundarias").
- **Cajas de colisión por fotograma:** ahora se usa la caja inicial de cada tipo; en el original cambia con cada fotograma (`boxFrames` en enemies.json).
- **Simplificaciones de las IA** ([I] en el código): sondeos de salto entre pisos (9A67/9CA3), la fase latente del pistón 0x04, el modo forzado de las cabezas 0x69 (mundo 9, sala 0x19), el reparto de la desviación del rayo 0x06 y los tamaños de 32 px deducidos de los sprites.
- **Experiencia del 0x19 por tamaño** (1, 2, 4, 4) y daño por tamaño con el Hat Helmet.

## Comprobaciones pendientes en el emulador

El barrido de tiles (docs/entities.md, "Barrido de tiles en openMSX") no llegó a activar:
- **Muros que se cierran** de las salas 15, 17, 29, 41 y 76 del castillo.
- **Escaleras que se retiran** de las salas 21, 60, 73, 102, 124 y 136 del castillo.
- **Eventos de sala** (enemigos marcados): escaleras y bloques de muro de los mundos, y los objetos con condición del castillo.
- **Umbral del toque de salto** entre 5 y 9 frames (medido: 4 frames no salta, 10 sí).
- **Salto pulsado en el aire:** en la demo 0 (castillo 101), al aterrizar con ↑ pulsado desde el aire, el original se queda un tick más quieto que el remake antes de andar (player.md, "Comprobado con la demo 0").

## Jefes

- **Final del juego con gráficos y música:** el castillo (20 × 14 tiles, b15:B989), la fuente de 8 × 16 del epílogo y las músicas 0x8B, 0x92 y 0x93; ahora son cuadros y texto con los tiempos del original.
- **Logo de Konami, título y demo** al acabar (estados 0 y 1): ahora se empieza directamente una partida nueva.
- **Animaciones del combate:** la invocación (borde parpadeando 60 frames, la decoración que se deshace en puntos 128), la disolución del jefe vencido y los 9 saltos de la celebración; ahora son esperas con un mensaje.
- **Repetir una letra pide soltarla** (el original lee la tecla una vez por tick y solo si cambia); el remake acepta cada pulsación del sistema.
- **Fondo negro durante el combate** (la decoración se apaga) y el dibujo del jefe con tiles (necesita gráficos).

## Mundo

- **Dagger de la sala 43:** aparece al pulsar →, ↓, ←, ↑ sobre el ascensor horizontal (entities.md, "Objetos que aparecen con una condición"). Los ascensores ya están; falta el objeto.
- **Gráfico de los ascensores:** ahora es un rectángulo de 32 × 16.
- **Pausa al cambiar de sala** (~20 frames en los que el juego no avanza mientras se dibuja la sala nueva).
