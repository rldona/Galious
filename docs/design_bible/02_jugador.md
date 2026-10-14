# 2. El jugador

Fuentes: [player.md](../player.md), [items_enemies.md](../items_enemies.md), [entities.md](../entities.md).

## Controles

| Entrada | Acción |
|---|---|
| ← → | andar; también dirige el salto en el aire [O] |
| ↑ | **saltar**; subir escaleras [O] |
| ↓ | bajar escaleras; entrar por puertas, santuarios y objetos de sala [O] |
| Espacio (botón A) | **espada** [C][O] |
| M (botón B) | **arma secundaria** elegida [C][O] |
| F1 | menú de objetos: cambiar de personaje y de arma, ver objetos; con el Wing, viajar; con el Angel Ring, volver [O*] |
| F2 | pausa |
| CTRL | usar la Bible: congela a los enemigos [O*] |
| Teclado A–Z | escribir la palabra del jefe en su sala; palabras secretas (UMBRELLA con la Dagger; ZEUS en pausa activa continuar con F5) [C][G] |

## Los dos personajes

Popolon (E501 = 1) y Aphrodite (E501 = 0). Cada uno tiene su vitalidad, su experiencia y sus vidas.
- **Cambio a voluntad:** en el menú de objetos [O].
- **Cambio forzado:** al quedarse uno sin vidas [C].

| | Popolon | Aphrodite |
|---|---|---|
| Salto máximo | **54 px**, con altura variable según cuánto mantienes ↑ [O][G] | 42 px, de altura fija [O][G] |
| Agua | se ahoga: −1 de vitalidad cada ~0,15 s [O] | aguanta: −1 cada 8–10 s [O]. Puede bajar al fondo de algunas salas de agua [C] |
| Lava | muere en ~1,2 s [O] | igual: no resiste la lava [C][O] |
| Piedras | 3 golpes (quita 5 de 15) [O*] | 15 golpes (quita 1); con el **Ring**, como Popolon [O*] |
| Paredes que suenan | las rompe [O*] | solo con el **Pendant** [O*] |
| Puertas de mundo | necesita el **Necklace** para el mundo 7 [C] | necesita la **Conch Shell** para el mundo 5 [C] |
| Puertas giratorias | las cruza [G] | solo con **Bread & Water** [G][C] |
| Disparos a la vez | 2 flechas y 2 minas; con Earrings, 3 flechas; con Bracelet, 3 minas [G] | 3 flechas y 3 minas [G] |

Diseño: el juego obliga a alternar. Aphrodite es para el agua y los disparos múltiples; Popolon para saltar alto, romper y cruzar puertas giratorias.

## Movimiento (números del original)

| Parámetro | Valor |
|---|---|
| Andar | 2 px/tick = 60 px/s [O]. Igual para los dos; la guía dice que los Shoes lo aceleran, pero no lo hemos visto en el paso [G] |
| Salto | 2 ticks de preparación; subida 6 6 6 6 6 5 5 4 4 3 2 1 px/tick (Aphrodite empieza en la 3.ª entrada); 4 ticks arriba; bajada 1 2 3 4 4 5 5 6, con tope de 6. Soltar ↑ corta la subida (solo Popolon); chocar con el techo también (medido con Popolon) [O] |
| Aire | 2 px/tick subiendo, 3 px/tick bajando. Caer por un borde no deja mover en horizontal [O] |
| Escaleras | 0,5 px/frame (1 px/tick). Con Shoes, 0,62 px/frame [O*] |
| Alinearse con una escalera | x del jugador = x de la escalera + 8 [O] |
| Caída | se cae por los huecos de las plataformas. Caer a la sala de abajo es normal |
| Colisión | caja de 16 px (x − 8 … x + 7) contra las paredes; apoyo en el suelo solo bajo x − 4 … x + 4 [O]; sólido si tile ≥ 0xBA. Durante el combate con un jefe solo son sólidos F0–FF, porque los números 5A–EF pasan a dibujar al jefe [C][O] |
| Daño al tocar enemigos | retroceso (estado E500 = 4: sale despedido 4 px por tick, alejándose) y daño. Con las protecciones (Crown, Helmet, Armor, Triangle y Pitcher), los enemigos correspondientes no hacen daño por contacto [C] |

## Vitalidad, experiencia y vidas

- **Vitalidad:** empieza en 32 y sube 32 con cada Great Key, hasta 192 [G]. El juego la cambia poco a poco: cada 4 frames aplica un paso de un incremento pendiente por personaje (b01:618D) [C].
- **Daño recibido:** los enemigos tienen una clase de daño; el Hat Helmet lo reduce a la mitad [C]. Por debajo de 6 suena el aviso de vitalidad baja [C].
- **Experiencia:** sube al matar enemigos. Al llenarse, repone la vitalidad [G]. El Vase duplica la experiencia ganada [C].
- **Otras curaciones:** hadas (ver [Mecánicas de sala](07_salas.md)) y Asclepio [G].
- **Vidas:** contador por personaje (E058/E059). Al perder una vida, si al personaje no le quedan, se pasa al otro; si a ninguno, fin de partida [C].
- **Resurrección:** la Muerte resucita una sola vez (ver [Santuarios](08_santuarios.md)) [G][C].

## Recursos

Tres contadores en BCD, hasta 999 [C]:

| Contador | RAM | Para qué |
|---|---|---|
| Flechas | E046 | munición de todas las armas secundarias: cada disparo gasta 1 [O*] |
| Monedas | E048 | se gastan en las tiendas |
| Llaves | E04A | abren las puertas con llave; cada puerta gasta 1 [C] |

Se consiguen bajo piedras, en las tiendas y al matar enemigos: la recompensa depende de la clase del enemigo [C].
