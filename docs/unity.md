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
- **Controles:**
  - ← → (o A D, o mando) andar;
  - ↑ saltar o subir escalera (también botón sur del mando);
  - ↓ bajar escalera;
  - Tab cambiar de personaje;
  - Esc salir.

## Hecho en el prototipo
- Puertas de mundo y salidas al castillo; espada y piedras; agua, lava y vitalidad; puertas con llave.
- Mecanismos de sala (`RoomObjects`): puertas giratorias (clases 3 y 4), muros que se cierran (5), paredes que se rompen (7) y escaleras extra que se retiran 60 s.
- Enemigos (`Enemies/`): combate medido (daño por tipo, retroceso, invulnerabilidad, espada) y **los 53 tipos de las salas más sus 25 hijos** (proyectiles, la mosca que se divide, el murciélago grande 0x37 y el hada 0x4F), según las fichas de `enemy_ai.md`. Ficheros: `EnemyTypes.cs` (los 6 primeros), `EnemyWalkers.cs` (los que andan y saltan), `EnemyFlyers.cs` (voladores, deambuladores y trepamuros), `EnemyFixed.cs` (trampas y torretas) y `Projectiles.cs` (hijos). El disparo 0x3F del soldado paraliza 60 ticks (`PlayerMotor.Stunned`) y el soldado carga entonces contra el jugador.
  - Simplificaciones [I], todas marcadas en el código: sondeos de salto entre pisos (9A67/9CA3) reducidos a "suelo bajo los pies y cuerpo libre"; los que caen chocan con paredes y techos y se posan en cualquier sólido bajo su caja (para no quedar metidos en un muro); el rayo 0x06 reparte su desviación por igual entre segmentos; el pistón 0x04 hace daño pero no es sólido (comprobado en el barrido de tiles: sus tiles están por debajo de 0xBA); los tamaños de 32 px (0x40, 0x37, murciélagos de nivel 2) están deducidos de los sprites; la fase latente del pistón y el modo forzado de las cabezas 0x69 (mundo 9, sala 0x19) no se usan.

## Pendiente
- Cajas de colisión (docs/items_enemies.md, "Cajas de colisión"): el jugador toca con x − 6 … x + 6, y − 14 … y − 2; la espada golpea en y − 7 … y − 4, x + 8 … x + 18 (o x − 18 … x − 8); cada enemigo usa su caja de la tabla b14:9006 (bordes incluidos), con la 4E/4F de 24 px del esqueleto según hacia dónde mire. [I] Las cajas que cambian con cada fotograma se aproximan con la inicial; en los que crecen (pinchos, brotes, los que emergen) se recorta su alto según la etapa.
- Final del juego (`EndingScreen.cs`, datos en `ending.json` → `Resources/Ending.json`; docs/ending.md): al vencer a Galious solo se abre la salida derecha de su sala, a la sala 6 del mundo 10, con 10 piedras (0x5C, 5 golpes cada una); al romperlas sale el bebé de la elegida al cargar la sala ((0..3) + (0..7): 1 de cada 32 veces ninguna, como en el original; `RemakeRules.BabyAlwaysPresent` lo cambia) o un fantasma 0x39 en espiral. Al tocar al bebé: la sala se congela, se borra en anillos, el castillo (la bandera de Galious baja y sube la de la cruz, el bebé entre Popolon y Aphrodite, "NICE!" / "GREAT!") y el epílogo con los créditos, con los tiempos del original (~108 s); después, partida nueva. F4 (depuración) lleva a la sala del bebé y F3 salta a la pantalla siguiente del final. [I] Sin gráficos ni música.
- Jefes (`BossFight.cs`, `Bosses.cs`, `BossChildren.cs`, datos en `bosses.json` → `Resources/Bosses.json`): en la sala del jefe se escribe su palabra (búfer de 16, sin distinguir mayúsculas; I, G y K dejan de hacer otra cosa) y se cierran las salidas; invocación (~97 ticks), chispas cada 8 ticks durante 65 y el jefe aparece cuando no queda ningún enemigo. Los 10 jefes con su IA, vida (a la mitad con Holy Water), daño de contacto (a la mitad con Cloak), armas que les afectan (b03:BD0A), pruebas de choque propias (Sea Anemone y Big Lip solo abiertos, Donra en la cabeza, Elusive Dragon por el cuello, King Dragon con 3 cajas, Galious solo con la Cross) y sus hijos (huesos que se parten, llamaradas, abanicos, esporas, tiros de Big Lip, bolas de Galious en órbita y en bumerán, bolas de Monster). La Magic Rod hace gratis los disparos del combate. Al vencerlo cae la Gran Llave (sin ella no se abren las salidas) y viene la celebración; morir en pleno combate lo vuelve a invocar con toda su vida. F2 (depuración) lleva a la sala del jefe del mundo. [I] Sin gráficos: rectángulos del tamaño de cada fotograma; sin la animación de la invocación ni los saltos de la celebración; el mundo 10 tras Galious (la salida a la sala 6 y el final) queda en el backlog. Ya no se mueve con WASD: solo las flechas, como en el original.
- Santuarios y tiendas (`ShrineVisit.cs`, `ShrineScreen.cs`, datos en `shrines.json` → `Resources/Shrines.json`): en el castillo se entra con ↓ delante del dios (con su piedra ya rota; a la Muerte, con la Salt) y en los mundos subiendo por la escalera de la fila 0 de la sala de debajo del santuario. Tiendas con los precios del original (los escudos en orden, carcaj +100 flechas, llave +1, tope 999; no mira si ya tienes el objeto), pistas de Atenea y Demeter, consejo de Asclepio. [I] La contraseña de Demeter se sustituirá por guardar la partida; la resurrección de la Muerte espera a las vidas de repuesto; sin la animación de bajar al santuario.
- Eventos de sala: en las salas con evento, al morir el último enemigo marcado (23 tipos) aparecen las escaleras ocultas (las que llevan a los santuarios de los mundos 2–6, 8 y 9) y se abren los bloques de muro, en esa sala o en otra, y se quedan así.
- Inventario (`Inventory.cs`, `InventoryMenu.cs`, datos en `items.json` → `Resources/Items.json`): menú F1 (o I; pausa el juego) con retratos, objetos, fila del mundo, mapa del mundo (con el Map; la Lamp marca la sala del jefe) y armas; ↑ ↓ fila, ← → personaje o arma, F1 cierra y aplica, Return Angel Ring (a la sala 72), 1–9 Ala (puertas ya abiertas). El personaje solo se cambia aquí (ya no con Tab). Objetos de mundo guardados por mundo. Efectos: Zapatos (2,5 px/tick andando, 1,25 en escaleras), Hat Helmet (mitad del daño, 1 sigue en 1), inmunidades (Corona, Casco, Armadura, Triángulo, Jarra), escudos (paran proyectiles de frente si no das un espadazo), Gran Llave (+8 de vitalidad máxima hasta 72), Collar y Concha (mundos 7 y 5), Hoi (controles al revés en el mundo 8 sin él), Carpet (piedra sobre la lava del mundo 8 y sin surtidores), Biblia (CTRL, 150 ticks, 16 usos), Arpa (el primer espadazo de la sala rompe todas las piedras), Saber, Daga (UMBRELLA), Vase y experiencia (a 48 rellena la vitalidad). Lo que falta está en [backlog.md](backlog.md); también el vaciado gradual de la barra (1 unidad cada 4 ticks).
- Armas secundarias (`Weapons.cs`, datos en `weapons.json` → `Resources/Weapons.json`): M dispara el arma elegida en el menú F1 (G de depuración da las seis y 50 flechas). 1 flecha por disparo; límites por personaje (Earrings, Bracelet); flecha 8 px/tick, cerámica 4 que atraviesa (vuelve a dañar cada 16 ticks), fuego rodante que rodea la plataforma una vez, fuego que corre por el suelo y cae, mina que se arma al apartarse y estalla al tocarla (70 ticks, 2 por golpe cada 32, también al jugador), lupa que lee la lápida del mundo. Dañan piedras (la mina no). [I] Los escudos de algunos enemigos (rebote) aún no se usan.
- Muerte y botín (`Loot.cs`): explosión de 24 ticks (4 en los grandes, una cada 16 [I]) y sorteo según el tipo: regla (qué muertes de ese tipo sortean en la visita a la sala, con 2 tipos seguidos como mucho, o "el último vivo") y nivel (por el daño). El objeto (flechas, monedas o llaves) se queda quieto en la rejilla de 8 px hasta recogerlo o salir de la sala; tope 999. La experiencia (BF27) está en el catálogo pero aún no se usa.
- Generadores periódicos (`Periodic.cs`, `EnemyGenerated.cs`): los 8 generadores de 51 salas con sus tiempos en ticks (tandas de 3 del E650/E662, espera aleatoria del E653, que el Carpet apaga, pausa del E659 con el jugador en el agua, fin del E668 a los 5 o 10) y sus tipos 0x08, 0x09, 0x0D, 0x1B, 0x21, 0x30, 0x35, 0x38, 0x3E y 0x6A.
- Lo que falta está en [backlog.md](backlog.md).
- Salas de dios de los mundos y puerta del mundo 10.
- Piedras que aparecen (clase 6, mundo 5) y muros que abren los eventos de sala (dependen de los enemigos).
- Pipeline URP con renderer 2D (luces) y el estilo visual del remake.
