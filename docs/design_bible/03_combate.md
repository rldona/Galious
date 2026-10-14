# 3. Combate

Fuentes: [player.md](../player.md), [items_enemies.md](../items_enemies.md), [boss_ai.md](../boss_ai.md), [enemy_ai.md](../enemy_ai.md).

## Espada (espacio)

- **El golpe:** la estocada (E508) dura unos pocos frames y suena 0x13 [C][O]. Golpea a los enemigos y las piedras que tengas delante.
- **Daño:** 1 por golpe a enemigos y jefes. El **Saber** (0x25) suma 1 más [C]; la guía dice que reduce a la mitad los golpes necesarios [G].
- **Contra piedras** (15 de vida): Popolon quita 5 y Aphrodite 1 (5 con el Ring). El **Harp** hace que un golpe rompa todas las piedras de la sala [O*].
- **Contra paredes que suenan:** cada golpe en la zona correcta suena 0x1E y resta 1 de su resistencia. Ver [Mecánicas de sala](07_salas.md).
- **Caballeros (0x0A):** en nuestras pruebas, pocos golpes contaban, y los que contaban venían del lado al que miran [O]. Sin confirmar en el código: revisar antes de fijar la regla.

## Armas secundarias (M)

Cada disparo gasta **1 flecha** (E046); sin flechas no se dispara. Pide pulsación nueva y no para al jugador. Las flechas se disparan también en el aire; las demás armas solo de pie en el suelo, y ninguna en la escalera. Velocidades en px por tick (1 tick = 2 frames) [C][O]. Detalle, cajas y fórmulas en [player.md](../player.md#armas-secundarias); números en `extracted/unity/weapons.json`.

| # | Arma | Comportamiento | Daño (enemigo / jefe) | Cómo se consigue |
|---|---|---|---|---|
| 1 | Arrow | recta, 8 px/tick; desaparece al dar | 1 / 1 | bajo una piedra del castillo (sala 121) |
| 2 | Ceramic Arrow | recta, 4 px/tick; **atraviesa** enemigos y jefes (repite golpe cada 16 ticks) | 1 / 1 | bajo una piedra (sala 71) |
| 3 | Rolling Fire | 4 px/tick; **da una vuelta** a la plataforma (encima, lateral, debajo, lateral) y muere donde nació | 1 / 1 | a la vista (sala 79) |
| 4 | Fire | 4 px/tick por el suelo; en los bordes **cae recto** (4 px/tick) y sigue por la plataforma de abajo | 1 / 1 | bajo una piedra (sala 147) |
| 5 | Mine | quieta; se arma al alejarte y estalla al tocarla (sin mecha). Explosión de 32×32 durante 70 ticks: 2 por golpe, hasta 3 golpes. Al jugador le quita 2 unidades de vitalidad | 2 por golpe / 2 | Hefesto, 30 monedas |
| 6 | Magnifying Glass | no es arma: con M cerca de una lápida pasa al estado 12, el mensaje con la palabra del jefe [C][G] | — | Ares, 20 monedas |

- **Límites a la vez** [C][O]: Popolon, 2 proyectiles (3 flechas con Earrings) y 2 minas (3 con Bracelet); Aphrodite, 3 y 3. Los proyectiles 1–4 comparten los 3 huecos.
- **Saber** (0x25): +1 por golpe (+2 la mina).
- **Magic Rod** (0x09): no se gastan flechas contra el jefe.
- **Piedras:** las armas 1–4 les quitan 5 con Popolon (1 con Aphrodite, 5 con el Ring). La mina no las daña.

## Daño a los enemigos

- **Vida:** cada tipo tiene vida en su descriptor (b04:67B6 + 5·tipo, 4.º byte). FE o FF significa invulnerable [C]. Por ejemplo, esqueleto 8, murciélago 1, estatuas invulnerables; valores en [items_enemies.md](../items_enemies.md).
- **Al morir:** el enemigo suelta una recompensa según su clase (flechas, monedas, llaves o nada) [C].
- **Jefes:** solo les dañan las armas de su máscara (b03:BD0A), además de un test de colisión propio, como "solo abierta" o "solo la cabeza". Si el arma no vale, suena 0x1D. La mina resta 2 y lo demás 1 [C]. Ver [Jefes](06_jefes.md).

## Daño al jugador

- **Contacto con enemigos.** Cinco objetos anulan el daño por contacto de un enemigo concreto (b03:B1F7) [C]:

  | Objeto | Protege de |
  |---|---|
  | Crown | Uh, 0x2A |
  | Helmet | ectoplasma, 0x28 |
  | Armor | disparos de bambú, 0x2E |
  | Triangle | nubes blancas o vapores, 0x31 |
  | Pitcher | bolas de fuego, 0x08 |

- **Proyectiles enemigos:** los escudos (Copper, Silver y Gold, niveles 1–3 en E097) los detienen según la categoría del demonio que dispara: menor, medio o todos [G][C].
- **Hat Helmet:** reduce el daño a la mitad [C].
- **Agua:** daño continuo según el personaje (ver [El jugador](02_jugador.md)).

## Control del combate

- **Bible + CTRL:** congela a todos los enemigos y al jefe unos 5 s, hasta 16 usos [O*][C]. La guía dice 15 s.
- **Dagger + UMBRELLA:** elimina a los murciélagos de la pantalla [G].
- **Cross:** imprescindible para dañar a Galious [C].
- **Holy Water / Cloak / Magic Rod** de cada mundo:
  - la vida del jefe se reduce a la mitad con el Holy Water [C];
  - el Cloak reduce a la mitad el daño que te hace el jefe [G];
  - con la Magic Rod, munición infinita durante el combate con el jefe [G].

## Ritmo

Toda la lógica corre a ~30 ticks/s [O]. Las ventanas de ataque de enemigos y jefes de los capítulos 5 y 6 están en ticks.
