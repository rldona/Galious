# 12. Estilo visual del remake

Cómo debe verse **Maze of Knight**. Tomamos de *Hollow Knight* su **lenguaje visual**: profundidad por capas, paletas casi monocromas, luz pintada y personajes de alto contraste. No tomamos sus diseños. Ningún personaje, enemigo, escenario o recurso suyo se copia ni se calca.

Todo lo de este capítulo es aspecto. La lógica, la colisión, la física y los tiempos siguen siendo los del original (capítulos 1–10). El arte se pinta encima de la rejilla de colisión de 8 px y nunca la cambia.

Estado: **prototipo con arte provisional**. Las siluetas se generan por código (`ShapePainter.cs`) y sirven para fijar el ambiente antes de producir arte dibujado.

## Principios

1. **Legibilidad primero.**
   - La capa de juego (suelos, paredes, escaleras, personajes) siempre se distingue del fondo.
   - El primer plano decorativo solo ocupa los márgenes.
   - Lo que se ve tiene la forma de la caja de colisión.
2. **Una paleta por mapa.** Casi monocroma, con un único color de acento para la luz. Ver la tabla de abajo.
3. **Profundidad con capas, no con detalle.** Lo lejano es más claro, más frío y con menos contraste. Lo cercano es más oscuro, hasta la silueta negra del primer plano.
4. **La luz cuenta la sala.** El ambiente es tenue y destacan unas pocas fuentes de luz: farolas, rayos desde arriba y la luz del jugador. El bloom recoge los brillos.
5. **Personajes de tinta.**
   - Contorno oscuro y sin iluminación: se leen igual en cualquier fondo.
   - Destello blanco al recibir un golpe.
6. **Griego, no gótico-insecto.**
   - El castillo es un templo de mármol: columnas, arcos de medio punto, estatuas.
   - Popolon y Aphrodite necesitan siluetas propias: yelmo con cresta, escudo redondo, túnica.

## Capas de una sala

La cámara es fija por sala, como en el original. La profundidad sale de un parallax según dónde está el jugador: una cámara virtual le sigue un 8 %, y cada capa se desplaza en proporción a su distancia.

| Capa | Orden | Parallax (1 = la sala) | Contenido | Material |
|---|---|---|---|---|
| Cielo | −100 | fija | degradado de la paleta | sin luz |
| Siluetas lejanas | −60 | 0,15 | el tema del mundo, pequeño y claro, fundido abajo | sin luz |
| Niebla lejana | −55 | 0,3 | banda de niebla que deriva despacio | sin luz |
| Siluetas medias | −40 | 0,45 | el tema, más grande y más oscuro | sin luz |
| Niebla baja | −35 | 0,7 | bruma a ras del suelo | sin luz |
| Rayos y farolas | −30 / −29 | 1 | rayos desde arriba y halos (aditivos) | aditivo |
| Motas | −25 | 1 | polvo, esporas, burbujas o brasas según el tema | aditivo |
| Siluetas cercanas | −20 | 0,75 | dos o tres piezas grandes, translúcidas | sin luz |
| Sala (macizo, borde, escaleras, agua) | −6 / −5 | 1 | la colisión del original con tiles del estilo | iluminado (luces 2D) |
| Piedras, puertas, ascensores | −4 … 6 | 1 | provisionales | iluminado |
| Personajes, enemigos, objetos | 7 – 11 | 1 | provisionales | contorno + destello |
| Primer plano | 50 | 1,6 | silueta negra en los márgenes: matorral abajo, columnas o rocas en los bordes, lianas o estalactitas arriba | sin luz |
| Fundido | 1000 | — | negro al cambiar de mapa | sin luz |

**Macizo de la sala.** Los tiles sólidos se oscurecen según su distancia al aire: el borde (1 tile) lleva el color de suelo, a 2 tiles es más oscuro y más adentro casi negro. Encima de cada suelo pisable va un borde iluminado con el color *Rim*. Así el macizo se lee como masa y su borde como el sitio donde se pisa.

## Paletas y temas

Definidas en `WorldPalette.cs`. Cada paleta tiene estos colores:
- **cielo:** arriba y abajo;
- **siluetas:** lejanas y medias;
- **sala:** suelo y borde;
- **niebla**;
- **luz:** ambiente (color e intensidad) y acento;
- **motas**;
- **agua.**

| Mapa | Tema | Dominante | Acento | Motas |
|---|---|---|---|---|
| Castillo | templo de mármol: columnas, arcos, lianas | azul pizarra | farolas cálidas | polvo |
| Mundo 1 | cueva: estalactitas y estalagmitas | verde agua | turquesa | polvo |
| Mundo 2 | arrecife: coral ramificado, algas, burbujas | azul marino | celeste | burbujas que suben |
| Mundo 3 | guarida: costillas, espinas | ámbar | naranja | polvo |
| Mundo 4 | cristal: racimos de cristales | violeta | lila | destellos |
| Mundo 5 | ruinas hundidas: columnas rotas, algas | azul | blanco azulado | burbujas |
| Mundo 6 | bosque de hongos | verde | amarillo lima | esporas que caen |
| Mundo 7 | abismo: algas, medusas | azul noche | verde luminiscente | burbujas |
| Mundo 8 | magma: estalactitas, cadenas | rojo | naranja | brasas que suben rápido |
| Mundo 9 | osario: columnas, arcos de hueso | gris hueso | crema | polvo |
| Mundo 10 | trono: arcos apuntados, columnas | carmesí | rosa | ceniza |

## Luz y posproceso

- **Luz global 2D** con el color y la intensidad de ambiente de la paleta (0,6–0,75).
- **Luz del jugador:** puntual, de radio 3,2 u e intensidad 0,55, que le acompaña.
- **Farolas:** hasta 2 por sala, en celdas vacías junto a una pared de la mitad de arriba. Siempre en el mismo sitio para cada sala. Luz puntual del color de acento con un parpadeo leve, y un halo aditivo.
- **Rayos de luz:** 1 o 2 por sala, desde arriba, con una inclinación de hasta ±15°. Aditivos y con una respiración lenta.
- **Posproceso** (Volume global):
  - bloom con umbral 0,75, intensidad 0,9 y dispersión 0,72;
  - viñeta de intensidad 0,38 con el color del cielo oscurecido;
  - contraste +12 y saturación −6.
- **Combate contra un jefe:** el fondo negro de `BossEffects` tapa las capas de fondo. Pendiente: hacerlo como cambio de luz (cap. 11).

## Transiciones

- **Cambio de sala:** corte seco, como el original. Probamos un fundido desde negro de 0,33 s y quedaba menos fluido.
- **Cambio de mapa** (entrar o salir de un mundo): fundido de 0,8 s.
- Nada pesado al cambiar de sala: las capas de un tema se generan todas al cambiar de paleta y los tiles solo se recolorean entonces.

## Personajes y efectos

- **Popolon y Aphrodite** (`CharacterArt.cs`, arte provisional pintado por código a 4 texels por píxel del original, con contorno de tinta):
  - **Popolon:** yelmo corintio claro (la «máscara», con rendija en T), cresta roja transversal y capa granate; en el escudo, un sol.
  - **Aphrodite:** yelmo con alas, melena dorada y capa violeta; en el escudo, una rosa.
  - **Escudo:** solo si se tiene uno de las tiendas, con su metal (cobre, plata u oro; el mejor que se lleve, `Inventory.ShieldLevel`). Sin escudo se ve el brazo.
  - **Fotogramas:** quieto (2, respira), andar (4, avanzan con la distancia recorrida), preparar el salto, subir, caer, escalera (2, con la altura), espadazo (2: estocada hasta x + 18 y recogida) y golpe recibido.
  - **Estela del espadazo:** media luna aditiva sobre la zona que golpea, que se apaga en los 4 ticks del golpe.
  - Los pies están en el pivote y la hoja llega donde golpea la espada del original: el dibujo no cambia ninguna caja.
  - **Maze of Knight → Hoja de personajes** guarda todos los fotogramas en `logs/unity/personajes.png`.

- **Shader `MazeOfKnight/SpriteOutlineFlash`:**
  - sprite sin iluminar con contorno de tinta de 1 texel;
  - destello de color al golpear: blanco 0,12 s en los enemigos, rojo 0,15 s en el jugador.
- **Pendiente:**
  - enemigos con forma propia;
  - chispas al golpear;
  - un temblor de cámara pequeño en los golpes fuertes;
  - partículas al romper piedras y paredes.

## Interfaz (pendiente)

- **Marcos de filigrana blanca** sobre negro. El friso griego (meandro) sustituye a las volutas.
- **Tipografía serif** de inspiración clásica y licencia libre, por ejemplo Cinzel (OFL).
- **Vitalidad:** una fila de iconos en lugar de la barra, sin cambiar las unidades del original.

## Producción del arte

- **Tamaño de una capa de fondo:**
  - 768 × 480 px a 32 px por unidad, que son 24 × 15 u (la sala es 16 × 10);
  - el margen deja sitio al parallax.
- **Formato:**
  - en gris con alfa, que se tiñe con la paleta;
  - o a todo color, poniendo el tinte en blanco en `Atmosphere`.
- **Primer plano:** la misma textura, escalada para cubrir la sala con un 10 % de margen. Todo fuera de los márgenes, transparente.
- **Tiles de la sala:**
  - 16 × 16 px por tile de 8 px del original;
  - variantes de borde, interior y fondo, y un borde superior.
- **Bocetos generados:**
  - Solo como referencia de composición y paleta. No van al juego.
  - Se guardan en `docs/art/bocetos/` con el prompt usado.
  - El arte final se dibuja a mano.

## Controles de prueba

- **F7:** pone o quita el estilo del remake y vuelve a los tiles provisionales.
- **F6:** recorre las paletas de los 11 mapas sobre la sala actual.
- **Maze of Knight → Capturas del estilo visual** (o `VisualPreview.CaptureBatch` en batch): guarda 13 capturas en `logs/unity/visual_NN.png`, de varias salas del castillo con cada paleta.

## Código

| Fichero | Qué hace |
|---|---|
| `Editor/RenderSetup.cs` | crea `Assets/Settings/Render/MazeURP2D.asset` (URP con el renderer 2D) y lo pone como pipeline del proyecto |
| `Runtime/Game/Visuals/Atmosphere.cs` | capas, parallax, niebla, luces, motas, posproceso, fundido, destellos, F6 y F7 |
| `Runtime/Game/Visuals/WorldPalette.cs` | paletas y temas por mapa |
| `Runtime/Game/Visuals/ShapePainter.cs` | arte provisional: siluetas por tema, puntos suaves, rayos, niebla y degradados |
| `Art/Resources/Shaders/SpriteOutlineFlash.shader` | contorno y destello de los personajes |
| `Art/Resources/Shaders/Additive.shader` | brillos aditivos |
| `Runtime/Game/MapView.cs` | tiles del estilo: macizo en 3 tonos y borde iluminado |
| `Runtime/Game/Visuals/CharacterArt.cs` | Popolon y Aphrodite: fotogramas, animación y estela del espadazo |
