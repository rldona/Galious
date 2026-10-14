# 7. Mecánicas de sala

Fuente: [entities.md](../entities.md), que tiene direcciones, tablas por sala y las pruebas en el emulador.

## Piedras (solo en el castillo)

- **Cuántas:** 245 piedras de 2×2 tiles, con **15 de vida**. Popolon quita 5 por golpe; Aphrodite, 1 (5 con el Ring). El Harp las rompe todas de un golpe [O*].
- **Qué esconden:** 71 tienen un objeto debajo. Según la guía, algunas tapan la entrada a un santuario [G].
- **Persistencia:** una piedra rota no vuelve [O].

## Paredes que suenan (6)

Bloque de 4×3 tiles de ladrillo (objeto de sala de clase 7).
- **Golpes:** cada espadazo en la zona correcta suena (0x1E) y resta 1. Según el tipo aguanta **30, 20, 10 o 3 golpes**.
- **Al romperse:** desaparece para siempre y suena 0x2E.
- **Quién:** Popolon siempre; Aphrodite solo con el **Pendant** [C][O*].
- **Dónde:**

| Sala | Golpes | Qué hay detrás |
|---|---|---|
| castillo 97 | 3 | camino de la Cross |
| mundo 2, sala 4 | 30 | Decorative Doll |
| mundo 4, sala 16 | 10 | Holy Water |
| mundo 5, sala 7 | 20 | paso al Map |
| mundo 6, sala 6 | 20 | Magic Rod |
| mundo 9, sala 26 | 30 | lápida |

## Muros que se cierran a tu espalda (16 salas del castillo)

Objeto de clase 5 [C][O]:
1. Cuando pasas por su columna, anota hacia dónde vas.
2. Si sigues 16 px más en esa dirección, **crece un muro**: un tile cada 15 ticks hasta 3 tiles de alto.
3. La sala queda marcada (bit 1) y al volver ya está cerrado.

Son los "callejones sin salida que se cierran detrás de ti" de la guía. Obligan a seguir hacia delante. El barrido del emulador los vio aparecer en 10 salas.

## Escaleras que se retiran (13, en el castillo)

Cada escalera extra tiene un modo [C][O]:
- **modo 1:** se retira al subirla;
- **modo 2:** se retira al bajarla;
- **modo 3:** se retira en los dos sentidos.

Al terminar de recorrerla desaparece entera. **Vuelve a los 60 segundos**: el temporizador corre aunque salgas de la sala. Impide deshacer el camino durante un minuto.

## Eventos al limpiar una sala

Algunas salas cuentan sus **enemigos marcados**. Al morir el último ocurre algo [C][O]:

- **Mundos**, rutina b08:9D5F:
  - se **abre un bloque de muro** de 4×3, a veces en otra sala del mundo (10 casos);
  - aparece una **escalera oculta**, a veces en otra sala (14 casos).
- **Castillo:** aparece un **objeto** (Hoi, Candle, Saber). Ver la tabla de abajo.
- **Mundo 3, sala 4:** el muro se abre **subiendo 5 veces seguidas** por el pozo que enlaza consigo mismo.
- **Mundo 2, sala 11:** con el Decorative Doll aparece un **puente** sobre el agua.

## Objetos que aparecen con una condición (castillo)

| Sala | Objeto | Condición |
|---|---|---|
| 48 | Hoi | limpiar la sala [O] |
| 75 | Candle | limpiar la sala (2 caballeros) [O*] |
| 24 | Saber | limpiar la sala (4 murciélagos) después de cierto jefe (según la guía, el del mundo 6) [O*] |
| 40 | Vase | después de cierto jefe (según la guía, el del mundo 4): 3 espadazos a la derecha, 3 a la izquierda y 3 saltos [O*] |
| 43 | Dagger | sobre el ascensor: derecha, abajo, izquierda, arriba [O] |

## Hadas (salas 25, 91 y 110 del castillo)

Doce partículas de polvo deambulan sin hacer daño [C][O].
1. Si el jugador se queda **quieto en el punto exacto** unos 4 s, el polvo vuela al centro.
2. Al llegar forma un **hada** que revolotea.
3. Al tocarla, **la vitalidad se llena** y el hada desaparece.

## Otras mecánicas

- **Puertas con llave:** se empuja contra ellas unos 30 ticks y gastan una llave [C].
- **Puertas giratorias:** las cruza Popolon; Aphrodite, con Bread & Water [G][C].
- **Agua:** daño continuo, mucho para Popolon y poco para Aphrodite. Con el Oar se camina sobre ella en el mundo 5. El monstruo del agua (0x42) se esconde si estás dentro.
- **Lava del mundo 8:** el Carpet la convierte en piedra [G][C].
- **Salas oscuras del mundo 5:** con la Candle se ven los muros [O*].
- **Controles invertidos del mundo 8:** sin el Hoi, izquierda y derecha van al revés [O*].
- **Piedras que aparecen** (mundo 5, sala 11): esperando en el borde del agua aparecen piedras para cruzar [G][C].
- **Ascensores:** plataformas horizontales y verticales dibujadas con tiles. Sobre el horizontal de la sala 43 se hace la secuencia de la Dagger.
- **Generadores periódicos:** en ciertas salas nace un enemigo cada cierto tiempo [C].

## Notas para el remake

- **Los secretos no tienen pista visual.** En HK los secretos tienen señales (grietas, sonido, mapas incompletos). Mantener el **sonido al golpear** como pista principal y añadir indicios visuales sutiles.
- **Escaleras y muros:** las escaleras que se retiran y los muros que se cierran son la "firma" del laberinto de Galious. Conservarlos, con animación y sonido claros para que el jugador entienda que es intencionado.
