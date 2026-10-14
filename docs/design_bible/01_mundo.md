# 1. El mundo

Fuentes: [world_maps.md](../world_maps.md), [state_machine.md](../state_machine.md), [entities.md](../entities.md).

## Estructura

El juego es un laberinto de salas fijas con transición sala a sala (sin scroll). Hay dos niveles:

- **Castillo de Grecia** (Castle Greek): el "mundo exterior", con **147 salas** en una cuadrícula de 15 × 16 [C]. La sala 94 no se alcanza por enlaces: es una sala de puro ladrillo, el hueco que dejaría la simetría del mapa [C]. La guía habla de 156 pantallas [G].
- **10 mundos** a los que se entra por puertas del castillo:

| Mundo | Salas | Sala del jefe | Palabra | Jefe | Puerta en el castillo |
|---|---|---|---|---|---|
| 1 | 15 | 13 | YOMAR | Bony Dragon | sala 104 |
| 2 | 15 | 9 | ELOHIM | Sea Anemone | 98 |
| 3 | 16 | 13 | HAHAKLA | Elusive Dragon | 63 |
| 4 | 17 | 14 | BARECHET | Jump Slime | 57 |
| 5 | 18 | 17 | HEOTYMEO | Donra | 62 |
| 6 | 19 | 13 | LEPHA | Arlyane | 114 |
| 7 | 20 | 10 | NAWABRA | Big Lip | 6 |
| 8 | 21 | 7 | ASCHER | Monster | 23 |
| 9 | 27 | 25 | XYWOLEH | King Dragon | 108 |
| 10 | 5 | 5 | HAMALECH | Galious | aparece en uno de 4 sitios del castillo al azar [G] |

En total son 320 salas [C]. Cada sala tiene cuatro enlaces (arriba, abajo, izquierda y derecha) a otra sala o a nada. Un enlace de un mundo a un valor F5–FE significa "volver al castillo" por ese lado (estado 13) [C].

**Rareza del laberinto:** en el mundo 3, la sala 4 enlaza hacia arriba consigo misma: es un pozo sin fin. Subirlo 5 veces seguidas abre un muro (ver [Mecánicas de sala](07_salas.md)) [C][O].

## Progresión

1. En el castillo se recoge la **Great Key** (0x0C) que abre la puerta del mundo 1 [C][G]. Cada Great Key desbloquea la puerta del mundo siguiente (bit 1 de E062 + mundo) [C].
2. Dentro de cada mundo hay que:
   - encontrar la **lápida** y leer la palabra con la **lupa** [G];
   - ir a la **sala del jefe** (marcada con un emblema) y **escribir la palabra** con el teclado [C][O]. Aparece el jefe (estado 15).
3. Al **derrotar al jefe** cae la Great Key del mundo siguiente. Recogerla dispara una fanfarria (estado 20) y marca el mundo como recuperado [C][O].
4. En cada mundo hay **cuatro objetos de mundo**: Holy Water, Cloak, Magic Rod y Map. Ayudan contra su jefe y muestran el mapa [C][G].
5. **Puertas por personaje** [C]: Aphrodite necesita la **Conch Shell** para el mundo 5 y Popolon el **Necklace** para el mundo 7. Algunos mundos tienen pasos que solo uno de los dos puede hacer, como el agua profunda (ver [El jugador](02_jugador.md)).
6. **Mundo 10:** su puerta aparece en uno de cuatro sitios del castillo, elegidos al azar en cada partida [G].
   - Galious solo puede ser dañado con la **Cross** [C].
   - Tras vencerlo, Pampas está en una de 10 piedras de la sala siguiente, al azar [G].
7. **Orden recomendado** por la guía [G]: recorrer el castillo en sentido antihorario recogiendo objetos antes del mundo 1. Los mundos se recuperan en orden 1 → 10, porque cada llave abre el siguiente.

## Viaje

- **Entrar en un mundo:** abajo sobre una puerta abierta (estado 8), con su jingle [C].
- **Salir:** cruzar el borde de la sala 1 del mundo que enlaza al castillo (estado 13) [C].
- **Atajos:**
  - **Wing (0x27):** en el menú de objetos, el número de un mundo abierto lleva a la sala de su puerta [O*].
  - **Angel Ring (0x15):** F1 + Return devuelve al santuario de Demeter (castillo, sala 72), también desde dentro de un mundo [O*].

## Persistencia del mundo

Cada sala guarda bits persistentes [C]:
- piedras rotas;
- objetos recogidos;
- muros cerrados (bit 1);
- paredes rotas (bit 2);
- escaleras abiertas (bit 3);
- sonido de evento ya oído (bit 4).

Lo roto o recogido no vuelve. Los enemigos sí reaparecen al reentrar [I], salvo los que forman parte de un evento ya cumplido.

## Notas de diseño

- **Densidad:** 15–27 salas por mundo. Cada mundo es una mazmorra corta con una llave, un jefe y 4 objetos.
- **Navegación:** la dificultad viene de cuatro cosas:
  - saber qué personaje usar;
  - encontrar los pasos ocultos (paredes que suenan, eventos por limpiar la sala);
  - administrar llaves, monedas y flechas;
  - las trampas de laberinto: escaleras que se retiran y muros que se cierran a tu espalda.
