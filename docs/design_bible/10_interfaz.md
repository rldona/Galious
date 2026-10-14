# 10. Interfaz

Fuentes: [state_machine.md](../state_machine.md) (estados 9, 11, 12 y 18), [player.md](../player.md), y la guía para lo descriptivo [G].

## Marcador (franja superior de 32 px)

| Elemento | Contenido |
|---|---|
| Centro | **VIT** y **EXP** de Popolon y de Aphrodite, en barras [O] |
| Abajo a la izquierda | dónde estás: CASTLE o WORLD n [O] |
| Derecha | contadores de **flechas, monedas y llaves** (3 cifras) [O] |
| Recuadro rojo de la derecha | **arma secundaria** elegida [O] |

## Menú de objetos (F1, estado 11)

Según la guía y lo visto en el emulador:
- **Arriba:** los dos personajes. Se elige uno con el cursor y espacio para **cambiar de personaje** [O].
- **Centro:** los objetos conseguidos (30 casillas) [G].
- **Abajo a la derecha:** las armas secundarias. La elegida pasa al recuadro del marcador [O*]. Encima, los objetos especiales del mundo [G].
- **Abajo a la izquierda:** el **mapa del mundo** si tienes su Map. El punto rojo eres tú; el azul, el jefe si tienes la Lamp [G][C].
- **Acciones dentro del menú:**
  - **número** con el Wing: viajar a un mundo [O*];
  - **Return** con el Angel Ring: volver a Demeter [O*].

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
