# 10. Interfaz

Fuentes: [state_machine.md](../state_machine.md) (estados 9, 11, 12 y 18), [player.md](../player.md), y la guía para lo descriptivo [G].

## Marcador (franja superior de 32 px)

| Elemento | Contenido |
|---|---|
| Centro | **VIT** y **EXP** de Popolon y de Aphrodite, en barras de 48 px, 1 px por unidad [C][O] |
| Abajo a la izquierda | dónde estás: CASTLE o WORLD n [O] |
| Derecha | contadores de **flechas, monedas y llaves** (3 cifras) [O] |
| Recuadro rojo de la derecha | **arma secundaria** elegida [O] |

## Menú de objetos (F1, estado 11)

Detalle y coordenadas en [items_enemies.md](../items_enemies.md), «Inventario (menú F1)»; datos en `extracted/unity/items.json` [C][O].
- **Abrir y cerrar:** F1. El juego se para del todo. Al cerrar, la sala sigue igual. No se abre durante el retroceso, la muerte o una parálisis.
- **Arriba:** los dos personajes (solo los que tienen vidas). El cursor empieza aquí: **←/→ elige personaje** y el cambio se aplica al cerrar. En el mundo 5 hace falta la Conch Shell y en el 7 el Necklace.
- **Centro («ITEM»):** rejilla de 10 × 3 con los objetos 0x0D–0x2A, cada uno en su casilla fija. Solo se dibujan los que tienes. La casilla del escudo enseña el mejor.
- **Abajo, a la derecha del mapa:** fila de objetos de mundo (Holy Water, Cloak, Magic Rod, Map) más Cross y Great Key.
- **Abajo del todo:** recuadro de las armas secundarias. **↑/↓ cambia de fila y ←/→ elige arma**, que pasa al acto al recuadro del marcador.
- **Abajo a la izquierda («MAP»):** mapa del mundo si tienes su Map (no en el castillo). Punto rojo: tu sala; punto azul: la sala del jefe, si tienes además la Lamp. El mapa sale entero, no se descubre.
- **Acciones:**
  - **1–9** con el Wing, solo en el castillo: viajar a la puerta de ese mundo, si ya se abrió;
  - **Return** con el Angel Ring: volver a Demeter (castillo, sala 72).
- **Rarezas del original:** abrir el menú anula el daño que aún no se había descontado de la barra; el cambio de personaje traspasa la vida en juego.

## Otras pantallas

- **Pausa (F2, estado 9):** en la guía, el personaje aparece sentado en un retrete, un chiste visual [G].
- **Bola de cristal (estado 12):** pantalla especial con su propia música (0x8C); se sale con M [C]. Su función de diseño está por confirmar.
- **Contraseña (L en el título, estado 18):** 45 letras; se corrige con ←; Return para validar [G][C].
- **Escribir la palabra del jefe:** se teclea directamente durante el juego, en la sala del jefe [C][O].

## Notas para el remake

- **Marcador:** pasar a un HUD mínimo al estilo HK (máscaras o vasija para la vitalidad) y mostrar las barras de los dos personajes solo al cambiar de personaje.
- **Inventario y mapa:** el menú de objetos pasa a ser una pantalla de inventario más un mapa del mundo. Conservar:
  - el mapa por mundo, que exige su Map;
  - el marcador del jefe, que exige la Lamp;
  - el cambio de personaje desde el menú.
- **Palabras del jefe:** sin teclado (mando), sustituir la escritura por un selector de letras o por un "recitar" en la sala del jefe. El requisito de haber leído la lápida se mantiene.
