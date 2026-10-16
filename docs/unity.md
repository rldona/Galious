# Fase 5: proyecto Unity e importador

Proyecto: `unity/MazeOfKnight`, con Unity **6000.6.5f1**. Paquetes:
- URP 17.6 (luces 2D más adelante);
- 2D Tilemap y Tilemap Extras;
- Pixel Perfect;
- Input System;
- Newtonsoft JSON;
- Test Framework.

## Abrir el proyecto en el editor

1. **Unity Hub → Add → Add project from disk** y elegir la carpeta `unity/MazeOfKnight`. También se puede abrir directamente:
   ```bash
   /Applications/Unity/Hub/Editor/6000.6.5f1/Unity.app/Contents/MacOS/Unity -projectPath unity/MazeOfKnight
   ```
2. Si ya están importadas las salas, al abrir el proyecto se muestra la vista general del castillo. Si no, usar **Maze of Knight → Importar salas**, que la abre al terminar.
3. Para ver otro mapa: **Maze of Knight → Abrir mapa → Castillo / Mundo 1…10**. Las escenas están en `Assets/Generated/Scenes`.
4. La vista general es una escena de consulta, no se juega: se recorre en la ventana *Scene* (rueda para el zoom, botón central para moverse).

## Qué se versiona y qué no

| Se versiona | No se versiona (sale de la ROM del usuario) |
|---|---|
| `Assets/Scripts` (código), `Assets/Art` (arte propio; por ahora tiles provisionales), `Packages`, `ProjectSettings` | `Assets/Generated/` (salas, mapas y escenas importados), `extracted/unity/` (paquete de datos), `Library/` |

## Cómo importar

1. Exportar el paquete de salas (necesita los datos de las fases 1–3 en `extracted/`):
   ```bash
   python3 tools/extract/collision.py
   python3 tools/extract/unity_bundle.py
   ```
   Genera `extracted/unity/<mapa>.json`: 11 mapas, 321 salas; además `ending.json` (final del juego, `ending.py`).
2. En Unity, menú **Maze of Knight → Importar salas**. O en batch:
   ```bash
   /Applications/Unity/Hub/Editor/6000.6.5f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath unity/MazeOfKnight -executeMethod MazeOfKnight.EditorTools.GaliousImporter.ImportAndRenderBatch -quit -logFile logs/unity/import.log
   ```
   El modo batch también renderiza cada vista general a `logs/unity/<mapa>_overview.png`. No usar `-nographics`: hace falta GPU para renderizar.

## Qué crea el importador

- `Assets/Generated/Maps/<mapa>.asset` (**MapData**): número de mapa (E041), sala y palabra del jefe, y la lista de salas.
- `Assets/Generated/Rooms/<mapa>/room_NNN.asset` (**RoomData**), con su **RoomModel**:
  - posición en la rejilla y enlaces (−1 sin salida; 0xF5–0xFE salida al castillo);
  - colisión 32 × 20, más la de combate en la sala del jefe;
  - enemigos, objetos de sala, objeto, piedras, escaleras extra, puerta de mundo y sala de dios.
- `Assets/Generated/Scenes/<mapa>_overview.unity`: todas las salas en su rejilla. Una capa de Tilemap por clase de colisión, sobre un fondo en damero por sala.
  - Las salas a las que no se llega por enlaces (`placed = false`) van en una columna aparte.
  - El santuario de cada mundo (`shrine = true`, la última sala, sin disposición propia) sale en morado.

## Convenciones

| | Valor |
|---|---|
| Unidad | 1 u = 16 px del original; un tile de 8 px = 0,5 u; una sala = 16 × 10 u |
| Celdas | `RoomCoords.Cell(room, col, fila)`: la fila 0 es la de arriba; el eje y de Unity crece hacia arriba |
| Coordenadas del original | x, y de pantalla; la sala empieza en y = 32. `RoomCoords.PixelToWorld` las convierte |
| Clases de tile | `TileClass`: Empty, Solid, Ladder, Water, Lava, BossPassable (letras `.`, `#`, `H`, `~`, `L`, `j` de collision.py) |

## Validación

Las vistas generadas se compararon a ojo con los mapas montados desde capturas del emulador (`extracted/maps/<mapa>_visual.png`):
- **castillo:** misma forma y posición de las 146 salas, agua en 137–143 y lava en 96 y 144–147;
- **mundo 8:** coincide sala a sala, con la lava en 15–20 y el santuario (21) en su sitio.

La colisión ya estaba validada en openMSX: 91/91 caídas (ver [world_maps.md](world_maps.md)).

## Escena de juego y compilado

- **Escena:** `Assets/Scenes/Game.unity` (menú **Maze of Knight → Crear escena de juego**). Carga `Resources/Maps/castle` y coloca a Popolon donde empieza el original: castillo, sala 1, x = 192, y = 120.
- **Movimiento:** `PlayerMotor` aplica los números medidos en el original, tick a tick a 30 ticks/s (docs/player.md, «Física del movimiento»). Los tests de `Assets/Tests/EditMode` comparan sus curvas con las medidas:
  ```bash
  /Applications/Unity/Hub/Editor/6000.6.5f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath unity/MazeOfKnight -runTests -testPlatform EditMode -testResults "$PWD/logs/unity/tests.xml"
  ```
- **Compilado para macOS:** menú **Maze of Knight → Compilar para macOS**, o en batch con `-executeMethod MazeOfKnight.EditorTools.GameBuild.BuildMac -quit`. Sale en `unity/MazeOfKnight/Builds/MazeOfKnight.app`.
  - Lleva dentro las salas de la ROM: es para probar en local y no se versiona.
- **Fusión con Hollow Knight (desde el 16 de octubre de 2026):** el juego arranca con los cuadros provisionales (F7 sigue poniendo el estilo del remake; el arte dibujado está en la rama `arte-dibujado`). Los protagonistas son el Caballero (Popolon) y Hornet (Aphrodite), sacados de la carpeta de Hollow Knight decodificado:
  ```bash
  python3 -m venv .venv && .venv/bin/pip install pillow
  .venv/bin/python -I tools/hollow/import_hollow.py "$HOME/workspace/games/Hollow Knight Decodificado"
  ```
  - Escribe `Assets/Art/Resources/Hollow/personajes/` (fuera de git): un PNG por fotograma con el nombre de `ArtLibrary`, mirando a la derecha, con los pies abajo en el centro, a 8 texels por píxel, y `fps.txt` con la velocidad de cada clip y el fotograma donde empieza su bucle (el «Run» del Caballero tiene 6 de arranque y luego repite). El Caballero va a ×1,375: su cuerpo sin los cuernos mide 16 px, lo que el sprite de Popolon del original (22 px con ellos). Hornet va a ×1,125 (30 px): 1,36 veces más alta que él, algo menos que en Hollow Knight (1,66), que era demasiado grande para estas salas. La escala de cada uno está en `CHARACTERS`.
  - Qué animación de Hollow Knight hace cada acción está en `CHARACTERS` del script. Hollow Knight no tiene escaleras: el Caballero sube de espaldas con los fotogramas de entrar por una puerta y Hornet con los de agarrarse a una pared (Wall Impact), en ida y vuelta.
  - El arte es de Team Cherry: el compilado que lo lleva es solo para probar en local, no se reparte.
  - Efectos (`tools/hollow/efectos.py` → `Assets/Art/Resources/Hollow/efectos/`, `Visuals/HollowFx`): las animaciones de chispas, grieta y polvo de Hollow Knight, y piezas sueltas del atlas de impactos de los enemigos (destello, raya del tajo, abanico de chispas, gotas), que el atlas trae sin recortes y se separan por manchas. `HitEffects` los usa en todos los golpes si están importados; si no, sigue con sus partículas:
    - espada contra un enemigo: la raya del tajo, un destello en estrella y un abanico de chispas hacia el otro lado;
    - enemigo muerto: destello grande, gotas y abanicos a los dos lados;
    - escudo que para un golpe: las chispas de metal;
    - piedra golpeada: una nube de polvo y un destello pequeño; rota, una nube alargada, polvo que sube y esquirlas;
    - jugador herido: el rayo blanco y un destello con su raya;
    - puerta de mundo que se abre: polvo que sube del hueco.
  - Enemigos (`tools/hollow/enemigos.py` → `Assets/Art/Resources/Hollow/enemigos/` y `enemigos_centro/`, `Visuals/HollowEnemyArt`): cada tipo del original lleva una criatura de Hollow Knight con un papel parecido (la tabla `ENEMIES`: murciélago → Vengefly, esqueleto → Husk, caballero → Husk con escudo, brote → planta trampa, rodador → Baldur, murciélagos que se fusionan → Gruzzer, reptador → Crawlid, erizo → Tiktik, espíritu → Lumafly, fantasma → Folly, hechicero → Soul Twister, estatua → Moss Knight, bocas → Gulka, torretas → la del Mawlek, pinchos → los del Coliseo). El tamaño sale de su caja de contacto original: una sola escala para los dos ejes (no se deforma), la menor de las que hacen que su cuerpo (la silueta sin patas, alas finas ni armas) mida K veces la caja de alto o K veces de ancho, así que no pasa de ella más que eso ni por arriba ni por los lados (`HollowEnemyArtTests`). Se apoya abajo en el centro de esa caja (los que andan) o en su centro (los que vuelan); ninguna colisión cambia. Acciones: «atacar» cuando lanza algo (cada enemigo sabe quién lo creó, `Enemy.Parent`, solo para el aspecto), «girar» al darse la vuelta, «mover» mientras se mueve y «quieto» si no; al morir deja su animación de muerte de Hollow Knight donde estaba (y no se pinta la explosión del original). Girar y morir se buscan solos en la colección si la tabla no los da. Los proyectiles de los enemigos también tienen su equivalente (púas para flechas y púas, bolas de fuego, glóbulos naranjas de los Gulka, orbe del Soul Twister, gotas y salpicaduras de agua, el rayo del Crystal Hunter…) y los alargados giran hacia donde vuelan. Cada acción usa su propio lienzo de tk2d, alineado por el centro del cuerpo. Los que andan apoyan los pies en el suelo que haya bajo el centro de su caja (la de los jefes del original baja de él).
  - Jefes (tipo 0x100 + id): Bony Dragon → False Knight, Sea Anemone → Uumuu, Elusive Dragon → Mantis Lords, Jump Slime → Dung Defender, Donra → Nosk alado, Arlyane → Traitor Lord, Big Lip → Mawlek, Monster → Broken Vessel, King Dragon → Grimm, Galious → la Radiance; lo que crean (huesos → lanzas de los Mantis Lords, llamaradas → murciélagos de fuego de Grimm, hoces, medusitas, escupitajos, orbes, globos de infección), y en el final el bebé es un gusarapo, sus piedras rocas de geo y los fantasmas babosas fantasma. El dibujo de Hollow Knight sustituye a la imagen del jefe del original; su tamaño se fija con la caja de su primera aparición (la del original cambia con cada pose) y al derrotarlo queda su animación de muerte. El Elusive Dragon se apoya a lo ancho en la zona que recibe golpes (su caja de contacto es todo el cuello). `-capturaJefes <carpeta> <mundo>` fotografía un combate (uno por ejecución: un combate empezado sigue activo en los demás mundos).
  - `-capturaPersonajes <carpeta>` en el compilado guarda capturas de los dos corriendo, atacando y saltando, de los efectos y de una sala del castillo por cada tipo de enemigo.
- **Controles:**
  - ← → (o A D, o mando) andar;
  - ↑ saltar o subir escalera (también botón sur del mando);
  - ↓ bajar escalera;
  - Tab cambiar de personaje;
  - Esc salir.

## Hecho en el prototipo
- Puertas de mundo y salidas al castillo; espada y piedras; agua, lava y vitalidad; puertas con llave.
- Puertas de mundo (`WorldDoors.cs`): empiezan cerradas; la Gran Llave del castillo desbloquea la del mundo 1 y la de cada jefe, la del mundo siguiente (la del 9 hace aparecer la del 10). Desbloqueada, se abre sola al ponerse delante, en 3 pasos de 6 ticks (el hueco se abre de abajo arriba, sonido 0x22) y gasta la llave; solo se entra con ella abierta. Se guardan en la partida. F5 (depuración) abre las puertas 1–9.
- Cajas de colisión (F9): encendidas por defecto mientras se prueba la fusión con Hollow Knight; F9 las quita.
- Sensación de Hollow Knight (`RemakeRules.HitStop`, `CoyoteJump`, `JumpBuffer`; design bible, cap. 11): F11 activa o quita las tres a la vez; por defecto, desactivadas.
- Mecanismos de sala (`RoomObjects`): puertas giratorias (clases 3 y 4), muros que se cierran (5), paredes que se rompen (7) y escaleras extra que se retiran 60 s.
- Enemigos (`Enemies/`): combate medido (daño por tipo, retroceso, invulnerabilidad, espada) y **los 53 tipos de las salas más sus 25 hijos** (proyectiles, la mosca que se divide, el murciélago grande 0x37 y el hada 0x4F), según las fichas de `enemy_ai.md`. Ficheros: `EnemyTypes.cs` (los 6 primeros), `EnemyWalkers.cs` (los que andan y saltan), `EnemyFlyers.cs` (voladores, deambuladores y trepamuros), `EnemyFixed.cs` (trampas y torretas) y `Projectiles.cs` (hijos). El disparo 0x3F del soldado paraliza 60 ticks (`PlayerMotor.Stunned`) y el soldado carga entonces contra el jugador.
  - Escudos (IX+2 bits 1 y 2): al esqueleto 0x25 y al caballero 0x0A solo se les hace daño por la espalda [C][O: sala 104, por delante 8 → 8, por detrás 8 → 5 con 3 espadazos]; los orbes 0x36, las cabezas 0x69, el rayo 0x06 y el 0x12 escondido hacen «clinc» y rebotan los proyectiles. Cajas de colisión por estado en todos los tipos que cambian de tamaño. Saltos entre pisos con las tablas 9ADF/9C68. Modo forzado de las cabezas 0x69 en el mundo 9, sala 0x19. El 0x19 da 1, 2, 4 o 4 de experiencia según su tamaño.
  - Simplificaciones [I] que quedan, marcadas en el código y en [backlog.md](backlog.md): los que caen chocan con paredes y techos y se posan en cualquier sólido bajo su caja; el pistón 0x04 hace daño con la cabeza pero no es sólido (sus tiles están por debajo de 0xBA).

## Pendiente
- Cajas de colisión (docs/items_enemies.md, "Cajas de colisión"): el jugador toca con x − 6 … x + 6, y − 14 … y − 2; la espada golpea en y − 7 … y − 4, x + 8 … x + 18 (o x − 18 … x − 8); cada enemigo usa la caja de la tabla b14:9006 (bordes incluidos) que toca a su estado.
- Ascensores (`Lifts.cs`, datos en `lifts.json` → `Resources/Lifts.json`; docs/entities.md, "Ascensores"): los 6 verticales del castillo recorren sus columnas de salas siempre (también con el jugador en otra sala) y lo llevan de una sala a otra; los 15 horizontales vuelven a su sitio al entrar. Colisión del original (encima, apartar por los lados, empujar por debajo, 7 ticks tras bajarse), cuentan como suelo, saltando desde un vertical se sale por arriba y el ascensor sigue, y encima no se usan el fuego ni la mina. La demo 0 recorre las salas del original. Se dibujan como dos bloques con marco (dibujo del remake). Sobre el horizontal de la sala 43 sale la Dagger (→ ↓ ← ↑).
- Ciclo de presentación (`AttractScreen.cs`, `DemoPlayer.cs`, `ScrollingText.cs`, datos en `attract.json` → `Resources/Attract.json`; docs/attract.md): logo, título, y por turnos una de las dos demos (pulsaciones grabadas reproducidas con el motor, como el original) o la historia que sube; espacio o M en el título → "PLAY START" y partida nueva; al acabar el final se vuelve al logo. En lugar del logo de Konami y el dibujo del título salen el nombre del remake y una línea que reconoce el juego original. La demo 1 coincide tick a tick con la del original; la demo 0, que empieza sobre un ascensor, recorre sus mismas salas. `skipAttract` en el GameBootstrap empieza directamente la partida.
- Final del juego (`EndingScreen.cs`, datos en `ending.json` → `Resources/Ending.json`; docs/ending.md): al vencer a Galious solo se abre la salida derecha de su sala, a la sala 6 del mundo 10, con 10 piedras (0x5C, 5 golpes cada una); al romperlas sale el bebé de la elegida al cargar la sala ((0..3) + (0..7): 1 de cada 32 veces ninguna, como en el original; `RemakeRules.BabyAlwaysPresent` lo cambia) o un fantasma 0x39 en espiral. Al tocar al bebé: la sala se congela, se borra en anillos, el castillo (la bandera de Galious baja y sube la de la cruz, el bebé entre Popolon y Aphrodite, "NICE!" / "GREAT!") y el epílogo con los créditos, con los tiempos del original (~108 s); después, vuelta al logo y al título. F4 (depuración) lleva a la sala del bebé y F3 salta a la pantalla siguiente del final. Con los gráficos importados, el castillo y la fuente del epílogo salen de la VRAM del original (`ending_vram.py`, `ending_gfx.py`); músicas 0x92 y 0x93.
- Jefes (`BossFight.cs`, `Bosses.cs`, `BossChildren.cs`, datos en `bosses.json` → `Resources/Bosses.json`): en la sala del jefe se escribe su palabra (búfer de 16, sin distinguir mayúsculas; I, G y K dejan de hacer otra cosa) y se cierran las salidas; invocación (~97 ticks), chispas cada 8 ticks durante 65 y el jefe aparece cuando no queda ningún enemigo. Los 10 jefes con su IA, vida (a la mitad con Holy Water), daño de contacto (a la mitad con Cloak), armas que les afectan (b03:BD0A), pruebas de choque propias (Sea Anemone y Big Lip solo abiertos, Donra en la cabeza, Elusive Dragon por el cuello, King Dragon con 3 cajas, Galious solo con la Cross) y sus hijos (huesos que se parten, llamaradas, abanicos, esporas, tiros de Big Lip, bolas de Galious en órbita y en bumerán, bolas de Monster). La Magic Rod hace gratis los disparos del combate. Al vencerlo cae la Gran Llave (sin ella no se abren las salidas) y viene la celebración; morir en pleno combate lo vuelve a invocar con toda su vida. F2 (depuración) lleva a la sala del jefe del mundo. Efectos (`BossEffects.cs`): el borde parpadea 60 frames y la decoración se deshace en puntos durante 128 al invocarlo, fondo negro durante el combate, el jefe vencido se deshace en puntos y el jugador da 9 saltitos en la celebración. Repetir una letra pide soltarla, como en el original. El jefe se dibuja con su imagen del original (un fotograma, sacado de la VRAM con `boss_vram.py` / `boss_gfx.py`) si está importada. Solo las flechas, no WASD, como en el original.
- Santuarios y tiendas (`ShrineVisit.cs`, `ShrineScreen.cs`, datos en `shrines.json` → `Resources/Shrines.json`): en el castillo se entra con ↓ delante del dios (con su piedra ya rota; a la Muerte, con la Salt) y en los mundos subiendo por la escalera de la fila 0 de la sala de debajo del santuario. Tiendas con los precios del original (los escudos en orden, carcaj +100 flechas, llave +1, tope 999; no mira si ya tienes el objeto), pistas de Atenea, consejo de Asclepio. En el castillo el jugador se hunde 16 px en 64 frames al entrar y sube al salir. La Muerte resucita al otro personaje (100 flechas, 100 monedas y 20 llaves, una vez por personaje) y se sale sola a los 180 frames. Demeter guarda la partida en vez de dar la contraseña (`SaveGame.cs`, `partida.json` en la carpeta de datos de la aplicación); se carga con L en el título.
- Eventos de sala: en las salas con evento, al morir el último enemigo marcado (23 tipos) aparecen las escaleras ocultas (las que llevan a los santuarios de los mundos 2–6, 8 y 9) y se abren los bloques de muro, en esa sala o en otra, y se quedan así.
- Inventario (`Inventory.cs`, `InventoryMenu.cs`, datos en `items.json` → `Resources/Items.json`): menú F1 (o I; pausa el juego) con retratos, objetos, fila del mundo, mapa del mundo (con el Map; la Lamp marca la sala del jefe) y armas; ↑ ↓ fila, ← → personaje o arma, F1 cierra y aplica, Return Angel Ring (a la sala 72), 1–9 Ala (puertas ya abiertas). El personaje solo se cambia aquí (ya no con Tab). Objetos de mundo guardados por mundo. Efectos: Zapatos (2,5 px/tick andando, 1,25 en escaleras), Hat Helmet (mitad del daño, 1 sigue en 1), inmunidades (Corona, Casco, Armadura, Triángulo, Jarra), escudos (paran proyectiles de frente si no das un espadazo), Gran Llave (+8 de vitalidad máxima hasta 72), Collar y Concha (mundos 7 y 5), Hoi (controles al revés en el mundo 8 sin él), Carpet (piedra sobre la lava del mundo 8 y sin surtidores), Biblia (CTRL, 150 ticks, 16 usos), Arpa (el primer espadazo de la sala rompe todas las piedras), Saber, Daga (UMBRELLA), Vase, Oar (la superficie del agua de las salas 12 y 13 del mundo 5 es suelo), Candle (sin ella, la sala 6 del mundo 5 tiene los muros invisibles), Campana (suena al entrar en las salas cercanas a la puerta desbloqueada y sin abrir) y experiencia (a 48 rellena la vitalidad). Daño, cura y relleno se aplican 1 unidad cada 4 ticks. Vidas de reserva (Aphrodite 1, Popolon 0): al perder una se gasta la del que juega o pasa a jugar el otro; sin ninguna, GAME OVER y vuelta al logo. El menú solo deja elegir a quien tenga vidas.
- Objetos con condición del castillo (`CastleItems.cs`): Hoi (48) y Candle (75) al limpiar la sala, Saber (24) además con el jefe del mundo 6 vencido, Vase (40) con el del mundo 4 y 3 espadazos a la derecha, 3 a la izquierda y 3 saltos, Dagger (43) sobre el ascensor con → ↓ ← ↑.
- Mundo 3, sala 4: subir 5 veces seguidas por el pozo abre el muro de arriba a la derecha. Mundo 5, sala 11: las piedras que aparecen para cruzar el agua (`SteppingStones.cs`). Puerta del mundo 10 en la sala 9, 30, 39 o 1 del castillo (al azar en cada partida), tras coger la llave del mundo 9.
- Sonido (`Psg.cs`, `Sound.cs`, datos en `Resources/Audio` de `audio_export.py`): un AY-3-8910 emulado reproduce los registros del PSG grabados de cada canción (recortados a una vuelta, con su bucle) y los efectos (guiones del driver, en el canal C, con su prioridad). Música por sala (castillo 0x87/0x88, mundos 0x8A, sala del jefe 0x8F, invocación 0x81, combate 0x90, jefe vencido 0x84 y 0x91, llave 0x8B, santuarios 0x82/0x8D, puerta de mundo 0x8E, objeto 0x85, vida perdida 0x95, historia 0x86, final 0x92/0x93) y los efectos principales (espada, piedras, puertas, agua, aviso de vitalidad, tiendas, eventos, resurrección, campana).
- Pausa al cambiar de sala: 11 ticks (~22 frames) sin que se mueva nada, también en las demos.
- Armas secundarias (`Weapons.cs`, datos en `weapons.json` → `Resources/Weapons.json`): M dispara el arma elegida en el menú F1 (G de depuración da las seis y 50 flechas). 1 flecha por disparo; límites por personaje (Earrings, Bracelet); flecha 8 px/tick, cerámica 4 que atraviesa (vuelve a dañar cada 16 ticks), fuego rodante que rodea la plataforma una vez, fuego que corre por el suelo y cae, mina que se arma al apartarse y estalla al tocarla (70 ticks, 2 por golpe cada 32, también al jugador), lupa que lee la lápida del mundo. Dañan piedras (la mina no). Los enemigos con escudo las rebotan.
- Muerte y botín (`Loot.cs`): explosión de 24 ticks (4 en los grandes, una cada 16 [I]) y sorteo según el tipo: regla (qué muertes de ese tipo sortean en la visita a la sala, con 2 tipos seguidos como mucho, o "el último vivo") y nivel (por el daño). El objeto (flechas, monedas o llaves) se queda quieto en la rejilla de 8 px hasta recogerlo o salir de la sala; tope 999. La experiencia (BF27, ×2 con el Vase) va al personaje que juega.
- Generadores periódicos (`Periodic.cs`, `EnemyGenerated.cs`): los 8 generadores de 51 salas con sus tiempos en ticks (tandas de 3 del E650/E662, espera aleatoria del E653, que el Carpet apaga, pausa del E659 con el jugador en el agua, fin del E668 a los 5 o 10) y sus tipos 0x08, 0x09, 0x0D, 0x1B, 0x21, 0x30, 0x35, 0x38, 0x3E y 0x6A.
- Lo que falta está en [backlog.md](backlog.md).
- Estilo visual del remake ([design_bible/12_estilo_visual.md](design_bible/12_estilo_visual.md)): URP con el renderer 2D (`RenderSetup.cs`, menú **Maze of Knight → Configurar render 2D (URP)**; la escena de juego lo configura al crearse), capas con parallax, niebla, luces 2D, motas, bloom y viñeta, fundido al cambiar de mapa, paleta y tema por mapa, contorno y destello de los personajes (`Visuals/`). Arte provisional generado por código; F7 lo quita y F6 prueba las paletas. Popolon y Aphrodite dibujados por código con animación y estela del espadazo (`CharacterArt.cs`). Enemigos por arquetipos (`EnemyArt.cs`). Piedras, puertas, ascensores y objetos (`PropArt.cs`). Interfaz con marcador, menú F1 y santuarios del remake (`UiKit.cs`, `RemakeHud.cs`; F8 el texto de pruebas). Golpes con chispas, combate del jefe como cambio de luz y retratos de los dioses. Cámara en perspectiva con los planos a profundidad real (`RoomCamera`), desenfoque por distancia (`LayerBlur`), oclusión ambiental desde la colisión y sombras de contacto, luces con mapas de normales, reproductor a 24 fps (`FrameAnimator`) y arte dibujado importado por nombre de fichero (`ArtLibrary`, `Editor/ArtImporter.cs`); rejilla de colisión con F10. El encargo de arte y su material están en [art/encargo/](art/encargo/README.md). Pendiente: el arte dibujado en sí y conectar los jefes dibujados.
