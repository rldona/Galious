# 3. Combate

Fuentes: [player.md](../player.md), [items_enemies.md](../items_enemies.md), [boss_ai.md](../boss_ai.md), [enemy_ai.md](../enemy_ai.md).

## Espada (espacio)

- **El golpe:** la estocada (E508) dura unos pocos frames y suena 0x13 [C][O]. Golpea a los enemigos y las piedras que tengas delante.
- **Daño:** 1 por golpe a enemigos y jefes. El **Saber** (0x25) suma 1 más [C]; la guía dice que reduce a la mitad los golpes necesarios [G].
- **Contra piedras** (15 de vida): Popolon quita 5 y Aphrodite 1 (5 con el Ring). El **Harp** hace que un golpe rompa todas las piedras de la sala [O*].
- **Contra paredes que suenan:** cada golpe en la zona correcta suena 0x1E y resta 1 de su resistencia. Ver [Mecánicas de sala](07_salas.md).
- **Caballeros (0x0A):** en nuestras pruebas, pocos golpes contaban, y los que contaban venían del lado al que miran [O]. Sin confirmar en el código: revisar antes de fijar la regla.

## Armas secundarias (M)

Gastan **flechas** (E046), 1 por disparo [O*]. El arma elegida va en E510 y el proyectil la lleva como tipo [C].

| # | Arma | Comportamiento | Cómo se consigue |
|---|---|---|---|
| 1 | Arrow | proyectil recto | bajo una piedra del castillo (sala 121) |
| 2 | Ceramic Arrow | atraviesa enemigos, más lenta [G] | bajo una piedra (sala 71) |
| 3 | Rolling Fire | recorre la plataforma en la que estás [G] | a la vista (sala 79) |
| 4 | Fire | corre por el suelo y cae a la plataforma de abajo [G] | bajo una piedra (sala 147) |
| 5 | Mine | daño alto, también a ti; contra jefes resta 2 [G][C] | Hefesto, 30 monedas |
| 6 | Magnifying Glass | no es arma: con M delante de una lápida revela la palabra del jefe [G][C] | Ares, 20 monedas |

Límites a la vez [G]: Popolon, 2 flechas y 2 minas; Aphrodite, 3 y 3. Con Earrings, Popolon dispara 3 flechas; con Bracelet, pone 3 minas.

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
