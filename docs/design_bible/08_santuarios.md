# 8. Santuarios y economía

Fuentes: [entities.md](../entities.md) (sección "Santuarios y tiendas"), [state_machine.md](../state_machine.md) (estado 16).

## Cómo funcionan

- **Entrada:** casi siempre está tapada por una piedra del castillo [G]. Se entra con abajo (estado 16) y la música cambia a 0x8D, "Shrine" [O].
- **Tiendas:** tres artículos con su precio en monedas. Se elige con las flechas y se compra con espacio [G][C].
  - Sin monedas suficientes suena 0x2F; al comprar, 0x2D [C].
  - Los escudos se compran **en orden**: cobre, plata y oro [C].
  - Comprar el carcaj da +100 flechas y comprar una llave, +1 llave [C].

## Los santuarios

| Sala | Dios | Función | Precios |
|---|---|---|---|
| castillo 72 | Demeter | **contraseña**: guardar y cargar; también es el destino del Angel Ring | — |
| castillo 32 | Demeter | tienda | carcaj 20, Necklace 25, llave 5 |
| mundo 5 | Demeter | tienda | Oar 30 |
| mundo 8 | Demeter | tienda; según la guía, se abre al matar a todos los enemigos del mundo | Carpet 30 |
| castillo 115 | Hefesto | tienda de armas | Mine 30, llave 5, carcaj 20 |
| castillo 5 | Hefesto | tienda | Harp 30, carcaj 20, llave 5 |
| castillo 55 | Hefesto | tienda | carcaj 20, Conch Shell 25, llave 5 |
| castillo 143 | Ares | tienda | carcaj 20, Copper Shield 30, Magnifying Glass 20 |
| castillo 65 | Ares | tienda | carcaj 20, Silver Shield 40, llave 5 |
| castillo 4 | Ares | tienda | carcaj 20, Gold Shield 50, llave 5 |
| castillo 10 | Ares | tienda | carcaj 20, Hat Helmet 50, llave 5 |
| castillo 54 | Asclepio o Atenea [G] | consejo o curación; sin tienda | — |
| castillo 87 | **la Muerte** | **resurrección**, una sola vez. Para entrar hace falta la **Salt**. Pide 100 flechas y 100 monedas [C]; según la guía, también 20 llaves | — |

En los mundos hay además **santuarios de pista** (Atenea o Demeter), con frases que orientan, como "golpea la pared…" [G]. Cada mundo tiene una "sala especial" (estado 14) [C].

## Economía

- **Ingresos:** monedas bajo piedras (25 en el castillo, +10 cada una) y recompensas de enemigos [C].
- **Gasto total:** la guía calcula **360 monedas** para comprar todo lo necesario [G].
- **Lo que se repone:** flechas (munición de todas las armas) y llaves (cada puerta gasta una). La guía recomienda ahorrar llaves buscando rodeos [G].

## Contraseña

Demeter da una contraseña de 45 letras que guarda el estado. Se introduce con L en la pantalla de título (estado 18) [G][C]. Existen contraseñas especiales y trucos [G]:
- **ZEUS** en pausa: continuar con F5;
- con otro cartucho de Konami en el segundo slot (Knightmare, Q*bert), ventajas.

## Notas para el remake

- **Guardado:** sustituir la contraseña por guardado en bancos o estatuas (estilo HK), conservando a Demeter como punto de guardado.
- **Economía:** mantener la tensión entre flechas, monedas y llaves: es el sistema de recursos del juego. Las llaves son un recurso consumible, no objetos únicos.
