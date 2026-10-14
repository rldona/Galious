# 4. Objetos

Fuentes: [items_enemies.md](../items_enemies.md) (tabla completa con direcciones), [entities.md](../entities.md), [catalog.py](../../tools/extract/catalog.py) y `extracted/entities/items.json`.

## Reglas generales

- **Posesión:** cada objeto tiene una flag de posesión en E06F + tipo. Se recoge al pisarlo y no vuelve a aparecer [C].
- **Escondidos:** en el castillo, **71 de los 129 objetos están bajo piedras** [C][O]. Hay que romper la piedra con la espada para que aparezcan.
- **Por condición:** 5 objetos solo aparecen al cumplir una condición (ver [Mecánicas de sala](07_salas.md)) [C][O].
- **Tras paredes:** 4 objetos están detrás de una pared que se rompe [C][G].
- **Iconos:** los objetos 7–2C comparten un icono de "cofre" que en realidad es su propio dibujo, cargado en los tiles 76–79. Los recursos 2D–2F tienen tiles propios y las armas 1–4 se ven como un destello [C].

## Por función de diseño

### Llaves de progresión

| Objeto | Función | Dónde |
|---|---|---|
| **Great Key** (0x0C) | abre la puerta del mundo siguiente; +32 de vitalidad máxima [G] | castillo, sala 107 (la del mundo 1); en los mundos 1–9 la suelta el jefe |
| **Cross** (0x0B) | sin ella no se puede dañar a Galious [C] | castillo, sala 96, tras la pared que se rompe de la 97 |
| **Necklace** (0x0D) | Popolon puede entrar al mundo 7 | tienda de Demeter, 25 monedas |
| **Conch Shell** (0x23) | Aphrodite puede entrar al mundo 5 | tienda de Hefesto, 25 |
| **Salt** (0x2A) | permite entrar al santuario de la Muerte | castillo, sala 144 |

### Objetos de mundo (uno de cada en cada mundo)

| Objeto | Función [G] |
|---|---|
| **Holy Water** (0x07) | la vida del jefe se reduce a la mitad [C] |
| **Cloak** (0x08) | el jefe hace la mitad de daño |
| **Magic Rod** (0x09) | munición infinita contra el jefe |
| **Map** (0x0A) | mapa del mundo en el menú (no hay en el mundo 10) |

### Mejoras del personaje

| Objeto | Efecto | Dónde |
|---|---|---|
| Shoes (0x11) | escaleras un 25 % más rápido [O*]; según la guía, también andar más rápido | castillo 141 |
| Ring (0x1F) | Aphrodite rompe piedras con 3 golpes [O*] | castillo 35 |
| Pendant (0x1C) | Aphrodite rompe paredes que suenan [O*] | castillo 137 |
| Bread & Water (0x29) | Aphrodite cruza puertas giratorias [G] | castillo 102 |
| Earrings (0x1D) | Popolon dispara 3 flechas [G] | castillo 13 |
| Bracelet (0x1E) | Popolon pone 3 minas [G] | castillo 66 |
| Saber (0x25) | la espada hace más daño [C] | castillo 24, por condición |
| Hat Helmet (0x19) | recibes la mitad de daño [C] | Ares, 50 |
| Vase (0x1B) | doble experiencia [C] | castillo 40, por condición |
| Copper, Silver y Gold Shield (0x28, 0x2B, 0x2C) | detienen proyectiles de demonios menores, medios y de todos; se compran en orden [C][G] | Ares, 30, 40 y 50 |

### Protecciones contra un enemigo (contacto sin daño)

Crown (Uh), Helmet (ectoplasma), Armor (bambú), Triangle (vapores) y Pitcher (bolas de fuego) [C]. Están en el castillo, salas 50, 52, 17, 140 y 19.

### Utilidades y llaves de mecánicas

| Objeto | Efecto | Dónde |
|---|---|---|
| Wing (0x27) | viaje a la puerta de un mundo abierto [O*] | castillo 2 |
| Angel Ring (0x15) | volver al santuario de Demeter [O*] | castillo 105 |
| Bell (0x14) | suena cerca de la puerta del mundo de tu llave [G][C] | castillo 131 |
| Lamp (0x1A) | marca la sala del jefe en el mapa [G][C] | castillo 123 |
| Candle (0x16) | se ven las salas oscuras del mundo 5 [O*] | castillo 75, por condición |
| Oar (0x10) | andar sobre el agua en el mundo 5 [C][G] | Demeter del mundo 5, 30 |
| Carpet (0x18) | la lava del mundo 8 se vuelve piedra [G][C] | Demeter del mundo 8, 30 |
| Hoi (0x13) | sin él, izquierda y derecha van al revés en el mundo 8 [O*] | castillo 48, por condición |
| Decorative Doll (0x12) | aparece un puente en el mundo 2 [O*] | mundo 2, sala 4, tras pared |
| Bible (0x20) | CTRL congela a los enemigos ~5 s, hasta 16 usos [O*] | castillo 11 |
| Harp (0x21) | un golpe rompe todas las piedras [O*] | Hefesto, 30 |
| Dagger (0x26) | UMBRELLA elimina murciélagos [G] | castillo 43, por condición |
| Magnifying Glass (6) | leer las lápidas | Ares, 20 |

### Recursos

Carcaj: +50 flechas bajo piedra (+100 si se compra). Monedas: +10. Llaves: +2 bajo piedra (+1 si se compra) [C]. Hay 13, 25 y 14 piedras con cada uno en el castillo.

## Notas para el remake

- **Pistas visuales:** el original esconde casi todo bajo piedras sin ninguna pista visual. En el remake conviene un indicio sutil (brillo o sonido) en las piedras con objeto, o mantener la regla y añadir el Harp antes.
- **Dependencias de la progresión:** muchos objetos son llaves de mecánicas. Hay que mantener el grafo:
  - Conch Shell → mundo 5;
  - Necklace → mundo 7;
  - Hoi → mundo 8;
  - Carpet → lava del mundo 8;
  - Oar y Candle → mundo 5;
  - Pendant y Ring → Aphrodite rompe;
  - Bread & Water → puertas giratorias;
  - Salt → la Muerte;
  - Cross → Galious.
