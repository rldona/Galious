# Backlog del remake (Maze of Knight)

Lo que falta por hacer en Unity. El estado de lo ya hecho está en [unity.md](unity.md).

## Objetos

- **Remo** (0x10): en el mundo 5, salas 12 y 13, el tile de la superficie del agua (B6) se vuelve suelo.
- **Vela** (0x16): sin ella, la sala 6 del mundo 5 es oscura (sus paredes se dibujan como el fondo: muros invisibles, con la misma colisión).
- **Campana** (0x14): suena (0x28) al entrar en ciertas salas del castillo, según la puerta de mundo desbloqueada y aún sin abrir. Depende del sonido.
- **Objetos contra los jefes:** Holy Water (vida del jefe a la mitad), Cloak (daño de su cuerpo a la mitad), Magic Rod (disparos gratis en el combate), Cross (sin ella no se daña a Galious). Depende de los jefes.

## Personajes

- **Vaciado gradual de la barra:** el daño, la curación y la experiencia van a un valor pendiente que se aplica 1 unidad cada 4 ticks; ahora se aplican de golpe.
- **Vidas de repuesto:** Popolon empieza sin ninguna y Aphrodite con 1; abrir el menú F1 suma una al activo y cerrarlo la gasta; el retrato solo sale si ese personaje tiene vidas (docs/items_enemies.md, "Vitalidad, experiencia y vidas").

## Santuarios y eventos

- **Guardar la partida en Demeter** (castillo, sala 72): sustituye a la contraseña de 45 letras (design bible, cap. 8 y 11).
- **Resurrección de la Muerte** (sala 87): depende de las vidas de repuesto (100 flechas, 100 monedas y 20 llaves; una vez por personaje).
- **Animación de entrar y salir de un santuario del castillo** (el jugador se hunde 16 px en ~1 s; música 0x8D / 0x88).
- **Objetos con condición del castillo:** al matar a los marcados de ciertas salas (E61C = 1) aparecen objetos en las salas 48, 75 y 24 (entre ellos el Hoi).
- **Muro del mundo 3, sala 4:** se abre al subir 5 veces seguidas por el pozo de la sala.
