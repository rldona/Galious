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
   Genera `extracted/unity/<mapa>.json`: 11 mapas, 320 salas.
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

## Pendiente
- Salidas al castillo y puertas de mundo (enlaces 0xF5–0xFE).
- Agua y lava (daño), objetos de sala (puertas, muros), enemigos, espada y armas.
- Pipeline URP con renderer 2D (luces) y el estilo visual del remake.
