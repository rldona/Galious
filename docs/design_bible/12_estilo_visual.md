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

La cámara es fija por sala, como en el original, pero con el estilo puesto es una **cámara en perspectiva** (`RoomCamera`), como en Hollow Knight: la sala está en z = 0, a 10 unidades de la cámara, y cada capa es un plano a su profundidad (z > 0 detrás, z < 0 delante), escalado para cubrir en pantalla lo mismo que cubriría en z = 0. La cámara se desplaza un 12 % de lo que el jugador se aparta del centro, y un desplazamiento del encuadre (*lens shift*) deja la sala clavada en la pantalla: el parallax de las demás capas sale solo de su profundidad, z / (10 + z) del movimiento de la cámara. Los planos lejanos y el primer plano se ven desenfocados (shader `LayerBlur`, un desenfoque por plano en texels del dibujo); el plano de juego, nítido. Con el estilo quitado (F7) la cámara vuelve a ser ortográfica.

| Capa | Orden | z | Parallax | Desenfoque | Contenido | Material |
|---|---|---|---|---|---|---|
| Cielo | −100 | 40 | 0,8 | — | degradado de la paleta | sin luz |
| Siluetas lejanas | −60 | 24 | 0,71 | 3 | el tema del mundo, pequeño y claro, fundido abajo | sin luz |
| Niebla lejana | −55 | 16 | 0,62 | 2 | banda de niebla que deriva despacio | sin luz |
| Siluetas medias | −40 | 10 | 0,5 | 1,6 | el tema, más grande y más oscuro | sin luz |
| Niebla baja | −35 | 5 | 0,33 | 1 | bruma a ras del suelo | sin luz |
| Rayos y farolas | −30 / −29 | 0 | 0 | — | rayos desde arriba y halos (aditivos) | aditivo |
| Motas | −25 | −0,5 … 5,5 | según cada mota | — | polvo, esporas, burbujas o brasas según el tema | aditivo |
| Siluetas cercanas | −20 | 3,5 | 0,26 | 0,6 | dos o tres piezas grandes, translúcidas | sin luz |
| Sala (macizo, borde, escaleras, agua) | −6 / −5 | 0 | 0 | — | la colisión del original con tiles del estilo | iluminado (luces 2D) |
| Piedras, puertas, ascensores | −4 … 6 | 0 | 0 | — | provisionales | iluminado |
| Oclusión ambiental y sombras de contacto | −2 | 0 | 0 | — | negro con alfa calculado desde la colisión; óvalo bajo cada personaje | sin luz |
| Personajes, enemigos, objetos | 7 – 11 | 0 | 0 | — | provisionales | contorno + destello |
| Primer plano | 50 | −4 | −0,67 | 2,5 | silueta negra en los márgenes: matorral abajo, columnas o rocas en los bordes, lianas o estalactitas arriba | sin luz |
| Rejilla de colisión (F10) | 998 / 999 | 0 | 0 | — | solo para revisar el arte | sin luz |
| Fundido | 1000 | 0 | 0 | — | negro al cambiar de mapa | sin luz |

El orden de dibujo lo decide `sortingOrder`, no la distancia (`transparencySortMode = Orthographic`), así que las capas pueden estar a cualquier z sin cambiar el orden. Las capturas a textura (`VisualPreview`) rehacen la proyección para su proporción (`RoomCamera.ApplyProjection`).

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
- **Combate contra un jefe:** en el original la decoración se deshace en puntos y queda el fondo negro (`BossEffects`). Con el estilo, es un cambio de luz con el mismo avance (`BossEffects.Darkness`, 0–1): las capas del fondo se oscurecen hacia un rojo casi negro, la luz global baja a 0,5 y se vuelve roja, se apagan los rayos y las farolas, la viñeta se refuerza en rojo, una luz roja acompaña al jefe y la luz del jugador sube. El parpadeo del borde al invocarlo es un destello de la luz global.
- **La sala está iluminada:** los tilemaps usan `Resources/Materials/SpriteLit` (Sprite-Lit-Default de URP 2D, lo crea `RenderSetup`), así que las luces 2D alumbran suelos, paredes y escaleras. El material del pipeline (`default2DMaterial`) sale vacío en el compilado.
- **Oclusión ambiental** (`AmbientOcclusion`): una textura de negro con alfa por sala, a 4 texels por tile, calculada desde la colisión: cada punto de aire se oscurece según los tiles macizos a menos de 1,6 tiles (más cuanto más cerca y más tiles: el suelo contra la pared, los rincones, la base de los salientes), hasta un 55 %. Se rehace si la colisión cambia (muros que se abren) y se suma a la que venga pintada en el arte.
- **Sombras de contacto:** un óvalo negro suave bajo el jugador y cada enemigo, en el suelo que tienen debajo (hasta 5 tiles), del ancho de su caja y más tenue cuanto más alto estén. En la escalera no hay sombra.
- **Mapas de normales:** las luces puntuales (jugador, farolas, jefe) los leen (`normalMapQuality` Fast, distancia 1,5). El arte dibujado los entrega como `<nombre>_n.png` y el importador los engancha como textura secundaria `_NormalMap`. Solo sirven en lo iluminado (tiles, objetos de sala); los personajes van con tinta plana sin iluminar.

## Transiciones

- **Cambio de sala:** corte seco, como el original. Probamos un fundido desde negro de 0,33 s y quedaba menos fluido.
- **Cambio de mapa** (entrar o salir de un mundo): fundido de 0,8 s.
- Nada pesado al cambiar de sala: las capas de un tema se generan todas al cambiar de paleta y los tiles solo se recolorean entonces.

## Personajes y efectos

- **Popolon y Aphrodite** (`CharacterArt.cs`, arte provisional pintado por código a 4 texels por píxel del original, con contorno de tinta):
  - **Popolon:** yelmo corintio claro (la «máscara», con rendija en T), cresta roja transversal y capa granate; en el escudo, un sol.
  - **Aphrodite:** yelmo con alas, melena dorada y capa violeta; en el escudo, una rosa.
  - **Escudo:** solo si se tiene uno de las tiendas, con su metal (cobre, plata u oro; el mejor que se lleve, `Inventory.ShieldLevel`). Sin escudo se ve el brazo.
  - **Fotogramas:** quieto (2, respira), andar (4, avanzan con la distancia recorrida), preparar el salto, subir, caer, escalera (2, con la altura), espadazo (3) y golpe recibido.
  - **Espadazo:** una estocada recta con impulso, como la del original (golpea en la franja horizontal x + 8 … x + 18, y − 7 … y − 4): en los ticks 4 y 3 se lanza con el pie adelantado y la hoja hasta x + 18 (el golpe es en el primero), en el 2 la recoge a media distancia y en el 1 queda en guardia, con la hoja horizontal.
  - **Estela:** destello horizontal aditivo a lo largo de la hoja, más brillante en la punta, con dos líneas de velocidad; fuerte en el tick del golpe y se apaga al recoger.
  - Los pies están en el pivote y la hoja llega donde golpea la espada del original: el dibujo no cambia ninguna caja.
  - **Maze of Knight → Hoja de personajes** guarda todos los fotogramas en `logs/unity/personajes.png`.

- **Shader `MazeOfKnight/SpriteOutlineFlash`:**
  - sprite sin iluminar con contorno de tinta de 1 texel;
  - destello de color al golpear: blanco 0,12 s en los enemigos, rojo 0,15 s en el jugador.
- **Enemigos** (`EnemyArt.cs`, arte provisional por código, 2 fotogramas): un arquetipo según cómo se mueve cada tipo (su clase en `Enemies/`), con el lenguaje de los personajes (caparazón oscuro, máscara clara con ojos huecos, contorno de tinta) y un color de acento por tipo:
  - escarabajos andantes, con variantes de cuernos, escudo (caballero), lanza (soldado), esqueleto y ancho;
  - gusanos y babosas; alados (murciélagos, aves, moscas); espíritus translúcidos (fantasmas, almas, cabezas);
  - bolas con pinchos, caparazón, piedra o mecha; peces; llamas y chorros de agua;
  - trampas de piedra con cara tallada (boca, agujero de flechas, pistón); hechiceros con túnica; pinchos;
  - proyectiles (orbes), huesos, dardos; botín (monedas, flechas, llaves), explosión, hada y el bebé;
  - las armas del jugador: flecha, cerámica, fuego y mina.
  - El dibujo, recortado a su silueta, se estira a la caja que pinta el juego (`Enemy.DrawRect`, la de colisión): lo que se ve es lo que golpea, como en el original, donde la caja sale del sprite. Mirando a la izquierda se coloca desde la esquina derecha (flipX voltea sobre el pivote). Los jefes con su imagen importada la conservan.
  - **Maze of Knight → Hoja de enemigos** guarda todos los arquetipos en `logs/unity/enemigos.png`.
- **Piedras, puertas, ascensores y objetos** (`PropArt.cs`, arte provisional por código, con contorno de tinta):
  - piedra: bloque tallado a facetas (cara de arriba clara, la derecha oscura) con una grieta quebrada, que llena sus 2 × 2 tiles; debajo no se dibuja el macizo ni el borde del suelo; puerta de mundo: pórtico griego (columnas, frontón y la entrada oscura con un brillo); cerrada, una losa de sillares con bandas de bronce y el sello dorado de la Gran Llave, que sube por detrás del friso al abrirse (los mismos pasos que el original, con polvo y un temblor pequeño); puerta con llave: reja de bronce con cerradura, que encoge con la apertura; puerta giratoria: madera con un eje; ascensor: losa de mármol con franja dorada;
  - los 47 objetos con su icono (armas, objetos de mundo, Cross, Great Key, escudos de cobre, plata y oro, corona, arpa, Biblia…) y un halo suave detrás;
  - piedras y puertas son tiles de 8 px recortados de un dibujo mayor: ocupan exactamente sus celdas de colisión.
  - **Maze of Knight → Hoja de objetos** guarda los iconos, la piedra, las puertas y el ascensor en `logs/unity/objetos.png`.
- **Golpes** (`HitEffects.cs`, solo aspecto: el juego no se para en el golpe):
  - espada o arma contra un enemigo: chispas blancas y amarillas desde el borde de su caja, a la altura de la espada, hacia fuera; destello blanco del enemigo y una sacudida corta de la cámara (más fuerte con un jefe);
  - escudo («clinc»): chispas azuladas que rebotan hacia el jugador;
  - enemigo muerto: estallido de chispas y esquirlas;
  - piedra golpeada: esquirlas y chispas; rota, lluvia de trozos y polvo;
  - golpe al jugador: chispas rojas, destello rojo y la sacudida más fuerte.
- **Pendiente:** esquirlas al golpear y romper las paredes de clase 7.

## Interfaz

Con el estilo puesto (F7), la interfaz del remake sustituye al texto de pruebas, que sigue disponible con F8. Se dibuja con OnGUI (`UiKit.cs`, `RemakeHud.cs`).
- **Tipografía:** Optima para títulos y Palatino para el texto, del sistema (con Georgia de reserva). Más adelante, una fuente libre incluida en el proyecto, por ejemplo Cinzel (OFL).
- **Marcos:** filigrana blanca de doble filete con un meandro griego en cada esquina, sobre negro translúcido.
- **Marcador:**
  - arriba a la izquierda, un medallón con el personaje que juega;
  - su vitalidad como barra ornamentada que crece con el máximo (como la del original: una unidad por paso, con marcas cada 8, una por Gran Llave); el daño y la cura pendientes se ven antes de aplicarse;
  - debajo, la vitalidad y las vidas del otro personaje, y la experiencia (0–48);
  - arriba a la derecha, flechas, monedas y llaves con sus iconos, y el arma elegida;
  - abajo, la vida del jefe en combate (o el aviso de escribir la palabra) y los mensajes;
  - al cambiar de mapa, su nombre en grande con el tema debajo (como los títulos de zona), 3,5 s.
- **Menú de objetos (F1) y santuarios:** las mismas posiciones del original, centradas en la ventana, con marco de filigrana, los retratos de los personajes, los iconos de `PropArt` y un marco dorado para lo elegido.
- **Dioses de los santuarios** (`PropArt.God`): busto en un medallón dorado. Demeter con corona de espigas; Hephaestus barbudo, con gorro de herrero, martillo y el brillo de la fragua; Ares con yelmo y cresta; Athena con el yelmo alzado y una lechuza; Asclepius anciano con el bastón y la serpiente; la Muerte, calavera encapuchada con guadaña.
- **Capturas:** el juego compilado acepta `-capturaInterfaz <carpeta>`: empieza la partida, guarda el marcador, da unos objetos, abre el menú F1, guarda las capturas y sale (`UiCapture.cs`). OnGUI no sale en `Camera.Render`, por eso las capturas del editor no la muestran.

## Producción del arte

El encargo de arte y el material para el dibujante están en [docs/art/encargo/](../art/encargo/README.md) (cajas de colisión, hojas del arte provisional, capturas con la rejilla, nombres de fichero y tiempos).

- **Escala de entrega: 8 texels por píxel del original** (128 texels por unidad). Un tile de 8 px se entrega a 64 × 64; la sala, a 2048 × 1280; un sprite de 16 × 16, a 128 × 128. El arte provisional generado por código va a 4 texels por píxel (personajes y enemigos) y a 2 (tiles); al llegar el dibujado se sustituye pieza a pieza.
- **Tamaño de una capa de fondo:** 24 × 15 u (la sala es 16 × 10): 3072 × 1920 texels a la altura de la sala; los planos lejanos pueden ir a la mitad. El margen deja sitio al parallax. El primer plano cubre la sala con un 10 % de margen y todo lo que no es silueta va transparente.
- **Formato:** PNG con alfa, sin luz ni sombra proyectada pintada (la pone el juego); en gris para teñir con la paleta, o a todo color poniendo el tinte en blanco en `Atmosphere`.
- **Tiles de la sala:** borde superior pisable, laterales, esquinas, interior y fondo profundo, escalera en dos mitades, agua y lava, con variantes para romper la repetición; la sombra de contacto va pintada en el tile además de la calculada.
- **Arte dibujado por nombre** (`ArtLibrary`, `Editor/ArtImporter.cs`): los PNG van en `Assets/Art/Resources/Dibujado/<carpeta>/` con el nombre `<tipo>_<accion>_<NN>[_<etiqueta>].png` (fotograma `NN` de dos cifras; etiqueta opcional, por ejemplo `golpe`) o `<tipo>_<nombre>.png` para una pieza suelta. El importador fija la escala, el filtro, el pivote (los pies en personajes, enemigos, jefes y objetos de sala) y engancha `<nombre>_n.png` como mapa de normales. Si un clip existe sustituye al generado; si no, se sigue con el provisional. Clips de los personajes: `quieto`, `correr`, `girar`, `salto_preparar`, `salto_subir`, `salto_caer`, `escalera`, `estocada`, `golpe`, `muerte`, `celebrar`, y el escudo en capa aparte (`popolon_escudo_oro_correr_03.png`). Enemigos: `enemigoXX_quieto` y `enemigoXX_mover` (tipo en hexadecimal), estirados a su caja como el generado. Los jefes se conectan cuando llegue el primero.
- **Reproductor a 24 fps** (`FrameAnimator`): independiente del tick del juego (30 ticks/s), avisa al entrar en cada fotograma (también en los que se salten) y puede arrancar en un fotograma dado: la estocada empieza en su fotograma `golpe` justo en el tick del golpe del original (el primero del espadazo), así la pose clave cae donde está medida y la recuperación va detrás. Las acciones de una vez (girar, estocada, golpe) se terminan de ver si el personaje sigue en el suelo; la escalera avanza con la y, como el original.
- **Cómo se produce el arte (decidido el 15 de octubre de 2026: no hay artista externo; el arte lo hace Claude Design con ayuda de Claude).** Cada pieza se dibuja como SVG en `tools/art/` (`svgkit.py` con la paleta y los filtros de grano, `castillo.py` para tiles, fondos, piedra, puerta y marcador, `popolon.py` con una pose paramétrica por fotograma, `esqueleto.py`) y `python3 tools/art/build_art.py` guarda los SVG fuente en `art/svg/` y rasteriza los PNG con `rsvg-convert` directamente en `Assets/Art/Resources/Dibujado/`, con los nombres que lee el juego. La dirección (moodboard, papeles de color, pantalla clave) vive en el lienzo de Claude Design «Dirección de arte de Maze of Knight». Los bocetos que no van al juego, si los hay, en `docs/art/bocetos/`.
- **Qué hay dibujado:** el castillo (tiles con variantes del borde y del interior, 4 planos, piedra, puerta de mundo en 3 estados, puerta con llave, puerta giratoria, ascensor, marco y medallón del marcador), las piezas de decoración del castillo (aplique de pared, 3 lianas, estatua, columna rota) y 27 tipos de enemigo del castillo con sus hijos (`tools/art/enemigos.py`, 77 fotogramas). **Popolon y Aphrodite siguen con el arte generado por código** (`CharacterArt.cs`): el usuario prefirió su diseño, su estocada y su animación a la versión dibujada (15 de octubre de 2026), que queda en `popolon.py` sin construirse. Donde no hay pieza dibujada sigue el arte generado.
- **Mundos** (`tools/art/mundos.py`): un tema por mundo con la paleta de `WorldPalette.cs`: material del terreno, cuatro planos y sus piezas de decoración (una luz, algo que cuelga, algo que se apoya). Los diez están hechos (`THEMES` en `mundos.py`: material, planos, luz, colgante y pieza apoyada por mundo):

| Mundo | Material del terreno | Planos | Luz | Cuelga | Se apoya |
|---|---|---|---|---|---|
| 1 cueva | lajas de roca irregular | estalactitas y estalagmitas, columnas de roca | racimo de cristal | estalactita | estalagmita |
| 2 arrecife | caliza porosa con agujeros | coral ramificado y algas sobre columnas hundidas | anémona | alga | abanico de coral |
| 3 guarida | costillas apiladas | arcos de costilla y espinas dorsales | brasero | tendón con garra | púa de hueso |
| 4 cristal | facetas de cristal | racimos de cristal arriba y abajo | racimo de cristal | estalactita | racimo |
| 5 ruinas hundidas | sillares | arcos y columnas rotas del castillo, con algas | anémona pálida | alga | columna rota |
| 6 bosque de hongos | madera con vetas | setas gigantes, raíces desde arriba | seta luminosa | raíz | seta |
| 7 abismo | roca negra | algas altas y medusas luminiscentes | medusa | tentáculo con ventosas | alga alta |
| 8 magma | roca | estalactitas, cadenas y resplandor bajo | brasero | cadena | pico de obsidiana |
| 9 osario | hueso con cuencas | arcos de costilla, montones de cráneos, columnas | calavera con vela | cadena | montón de cráneos |
| 10 trono | sillares de pórfido | arcos apuntados, columnas y estandartes | brasero | estandarte | pilar |

Todos comparten la niebla, el grano, el fundido del plano lejano y los márgenes negros del primer plano (`wrap_planes`). Las piezas de cada mundo se revisan sobre salas del castillo con F6 antes de entrar en los mundos.
- **Piezas de decoración** (`Atmosphere.BuildDecor`): las piezas de la carpeta `decoracion` (`mundoNN_farola`, `_liana_01…`, `_estatua`, `_columna_rota`, `_pedestal`) se colocan solas en cada sala con su número como semilla, siempre igual: la farola sobre cada punto de luz, 2 a 5 lianas colgando de techos con aire debajo, hasta 2 estatuas en suelos con pared al lado y 3 tiles de aire encima. Van a z = 0,6 (farola), 1,2 o 2,2 (lianas y estatuas), escaladas por su profundidad y con la posición compensada para caer en pantalla sobre su celda; nunca donde se pisa.
- **Variantes de tile** (`MapView.Variant`): `mundoNN_macizo_borde_v2`, `_v3`… y `_macizo_interior_v2`… se reparten por celda con un hash de sala, columna y fila (el tile base sale dos de cada tres veces).
- **Enemigos dibujados por estado** (`EnemyArt.DrawnFor`): entre los clips `enemigoXX_*` del tipo se elige el de proporción más parecida a la caja del momento (el murciélago colgado 17 × 10 frente a volando 11 × 12; el brote en sus 4 alturas); a igual proporción, los clips `mover`, `volar`, `andar`, `correr`, `nadar` o `rodar` si se mueve.

## Controles de prueba

- **F7:** pone o quita el estilo del remake y vuelve a los tiles provisionales.
- **F8:** el texto de pruebas (controles, sala, posición, estado) encima del marcador.
- **F9:** las cajas de colisión encima del juego: verde el jugador, amarillo la espada, rojo los enemigos que hacen daño (`HitboxOverlay.cs`).
- **F6:** recorre las paletas de los 11 mapas sobre la sala actual.
- **F10:** la rejilla de colisión de 8 px encima de la sala, con cada clase teñida (macizo, escalera, agua, lava, pasable en el combate), una línea más fuerte cada 4 tiles y la caja de contacto del jugador en verde (`CollisionGridOverlay.cs`). Va en el mundo, no en OnGUI, así que sale en las capturas.
- **Maze of Knight → Lote del encargo de arte** (o `VisualPreview.ArtPackBatch` en batch): las tres hojas del arte provisional y una captura por zona con la rejilla, en `logs/unity/encargo/`.
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
| `Runtime/Game/Visuals/EnemyArt.cs` | enemigos por arquetipos, botín y armas del jugador |
| `Runtime/Game/Visuals/PropArt.cs` | piedras, puertas, ascensores e iconos de los objetos |
| `Runtime/Game/Visuals/UiKit.cs`, `RemakeHud.cs` | piezas de la interfaz y marcador del remake |
| `Runtime/Game/Visuals/UiCapture.cs` | capturas de la interfaz con `-capturaInterfaz` (marcador, menú, cajas, golpes, santuario y combate del jefe) |
| `Runtime/Game/Visuals/HitEffects.cs` | chispas, esquirlas y sacudida al golpear |
| `Runtime/Game/Visuals/ColorCanvas.cs` | lienzo de color con contorno de tinta para personajes y enemigos |
| `Runtime/Game/RoomCamera.cs` | cámara fija por sala: en perspectiva con el estilo (planos a profundidad real, lens shift), ortográfica sin él |
| `Art/Resources/Shaders/LayerBlur.shader` | capas del ambiente con desenfoque por distancia |
| `Runtime/Game/Visuals/AmbientOcclusion.cs` | oclusión ambiental calculada desde la colisión de la sala |
| `Runtime/Game/Visuals/FrameAnimator.cs` | clips y reproductor a 24 fps con eventos por fotograma |
| `Runtime/Game/Visuals/ArtLibrary.cs` | arte dibujado agrupado en clips por nombre de fichero |
| `Runtime/Game/Visuals/CollisionGridOverlay.cs` | rejilla de colisión (F10) para revisar el arte |
| `Editor/ArtImporter.cs` | importación del arte dibujado: escala, pivote y mapas de normales |
