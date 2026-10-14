# 4. Objetos

Fuentes: [items_enemies.md](../items_enemies.md) (tabla completa con direcciones), [entities.md](../entities.md), [catalog.py](../../tools/extract/catalog.py) y `extracted/entities/items.json`.

## Reglas generales

- **Posesión:** cada objeto tiene una flag de posesión en E06F + tipo, **compartida por los dos personajes** (no hay inventario por personaje; algunos objetos solo sirven a uno). Se recoge al pisarlo y no vuelve a aparecer [C].
- **Objetos de mundo:** se guardan por mundo (bits de E061 + mundo) y solo cuentan dentro de su mundo [C]. Efectos exactos, menú F1 y vitalidad en [items_enemies.md](../items_enemies.md) y `extracted/unity/items.json`.
- **Escondidos:** en el castillo, **71 de los 129 objetos están bajo piedras** [C][O]. Hay que romper la piedra con la espada para que aparezcan.
- **Por condición:** 5 objetos solo aparecen al cumplir una condición (ver [Mecánicas de sala](07_salas.md)) [C][O].
- **Tras paredes:** 4 objetos están detrás de una pared que se rompe [C][G].
- **Iconos:** los objetos 7–2C comparten un icono de "cofre" que en realidad es su propio dibujo, cargado en los tiles 76–79. Los recursos 2D–2F tienen tiles propios y las armas 1–4 se ven como un destello [C].

## Por función de diseño

### Llaves de progresión

| Objeto | Función | Dónde |
|---|---|---|
| **Great Key** (0x0C) | abre la puerta del mundo siguiente; +8 de vitalidad máxima al personaje activo mientras sea < 72, y lo cura [C] | castillo, sala 107 (la del mundo 1); en los mundos 1–9 la suelta el jefe |
| **Cross** (0x0B) | sin ella no se puede dañar a Galious [C] | castillo, sala 96, tras la pared que se rompe de la 97 |
| **Necklace** (0x0D) | Popolon puede entrar al mundo 7 | tienda de Demeter, 25 monedas |
| **Conch Shell** (0x23) | Aphrodite puede entrar al mundo 5 | tienda de Hefesto, 25 |
| **Salt** (0x2A) | permite entrar al santuario de la Muerte | castillo, sala 144 |

### Objetos de mundo (uno de cada en cada mundo)

| Objeto | Función [C] |
|---|---|
| **Holy Water** (0x07) | la vida del jefe se reduce a la mitad al aparecer |
| **Cloak** (0x08) | el contacto con el cuerpo del jefe hace la mitad de daño (no sus disparos) |
| **Magic Rod** (0x09) | contra el jefe las armas secundarias no gastan flechas (hace falta tener 1) |
| **Map** (0x0A) | mapa del mundo en el menú, con tu sala (no hay en el mundo 10) |

### Mejoras del personaje

| Objeto | Efecto | Dónde |
|---|---|---|
| Shoes (0x11) | andar (2 → 2,5 px/tick) y escaleras (1 → 1,25) un 25 % más rápido [C][O] | castillo 141 |
| Ring (0x1F) | Aphrodite rompe piedras con 3 golpes [O*] | castillo 35 |
| Pendant (0x1C) | Aphrodite rompe paredes que suenan [O*] | castillo 137 |
| Bread & Water (0x29) | Aphrodite cruza puertas giratorias [C] | castillo 102 |
| Earrings (0x1D) | Popolon dispara 3 flechas a la vez [C] | castillo 13 |
| Bracelet (0x1E) | Popolon pone 3 minas a la vez [C] | castillo 66 |
| Saber (0x25) | +1 de daño por golpe (+2 con la mina); no contra jefes [C] | castillo 24, por condición |
| Hat Helmet (0x19) | daño recibido a la mitad, redondeando hacia abajo (1 se queda en 1) [C] | Ares, 50 |
| Vase (0x1B) | doble experiencia [C] | castillo 40, por condición |
| Copper, Silver y Gold Shield (0x28, 0x2B, 0x2C) | detienen los proyectiles de su lista (nivel 1, 2 o 3, cada uno incluye los anteriores) si no estás dando un espadazo y los miras de frente; se compran en orden [C] | Ares, 30, 40 y 50 |

### Protecciones contra un enemigo (contacto sin daño)

Crown (Uh), Helmet (ectoplasma), Armor (bambú), Triangle (vapores) y Pitcher (bolas de fuego) [C]. Están en el castillo, salas 50, 52, 17, 140 y 19.

### Utilidades y llaves de mecánicas

| Objeto | Efecto | Dónde |
|---|---|---|
| Wing (0x27) | desde el menú, en el castillo: viaje a la puerta de un mundo ya abierto (teclas 1–9) [C][O*] | castillo 2 |
| Angel Ring (0x15) | volver al santuario de Demeter [O*] | castillo 105 |
| Bell (0x14) | en el castillo, suena al entrar en las salas cercanas a la puerta desbloqueada y sin abrir [C] | castillo 131 |
| Lamp (0x1A) | marca la sala del jefe en el mapa (hace falta el Map) [C] | castillo 123 |
| Candle (0x16) | la única sala oscura (mundo 5, sala 6) deja de tener muros invisibles [C][O*] | castillo 75, por condición |
| Oar (0x10) | andar sobre el agua en el mundo 5 [C][G] | Demeter del mundo 5, 30 |
| Carpet (0x18) | la lava del mundo 8 se vuelve piedra [G][C] | Demeter del mundo 8, 30 |
| Hoi (0x13) | sin él, izquierda y derecha van al revés en el mundo 8 [O*] | castillo 48, por condición |
| Decorative Doll (0x12) | aparece un puente en el mundo 2 [O*] | mundo 2, sala 4, tras pared |
| Bible (0x20) | CTRL congela a los enemigos 150 ticks (5 s), 16 usos en total [C][O] | castillo 11 |
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
