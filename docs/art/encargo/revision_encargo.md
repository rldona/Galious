# Revisión del encargo de arte

Cada afirmación del documento «Encargo de arte de Maze of Knight» contrastada con el código y los datos del repo
(15 de octubre de 2026). Primero lo que había que corregir, después lo que conviene decidir, y al final lo que estaba bien.

## Correcciones aplicadas al documento

| Decía | Es | Fuente |
|---|---|---|
| «18 arquetipos compartidos entre 42 tipos» de enemigos | **53 tipos** colocados en las salas, **10** que salen de generadores y **44 hijos** (proyectiles, llamaradas, esporas, botín, hada, bebé), dibujados con 18 arquetipos | `docs/enemy_ai.md`; `EnemySystem.cs` (74 clases); `cajas_colision.md` |
| Enemigos a entregar: 42 | 53 de sala + 10 generados, cada uno con su animación; los hijos, más sencillos (la mayoría de 1 o 2 fotogramas) | ídem |
| «El jugador ocupa 12 × 12 px del original» | La caja de contacto cubre **13 × 13 px** (x − 6 … x + 6, y − 14 … y − 2; bordes incluidos); el sprite es de 16 × 16 | `docs/items_enemies.md`, «Jugador, espada y armas» |
| «Estocada: anticipación corta, un fotograma de impacto…» | La espada golpea en el **primer tick** del espadazo (4 ticks, 133 ms): el fotograma de impacto es el primero visible y no cabe anticipación delante. La fuerza va en la recuperación | `docs/player.md`; `CharacterArt.cs` |
| «El juego tiene 320 salas: 146 en el castillo…» | Correcto para lo que se juega. El castillo tiene una sala 147 sin enlaces (no se alcanza) que no hay que vestir | `extracted/unity/castle.json` (`placed`) |
| «F2 lleva a la sala del jefe» | Solo **dentro de un mundo** (en el castillo no hace nada) | `GameBootstrap.cs` |
| Teclas de revisión | Falta **F10**, la rejilla de colisión de 8 px con cada clase teñida y la caja del jugador (nueva); F8 es el texto de pruebas | `CollisionGridOverlay.cs` |
| «Mapas de normales opcionales en los tiles y en los personajes» | Solo en tiles, objetos de sala y decoración: los personajes y enemigos van con tinta plana sin iluminar a propósito (se leen sobre cualquier fondo) | `SpriteOutlineFlash.shader`; guía de estilo |
| «Tiles de 64 × 64» y «personaje de unos 128 px» | Correcto, pero la guía de estilo decía otra cosa (tiles de 16 × 16, personajes a 4 texels por píxel). La guía queda actualizada a **8 texels por píxel** | `12_estilo_visual.md`, «Producción del arte» |
| Nombres de fichero `popolon_andar_03.png`, `mundo05_fondo_medio.png` | El importador agrupa por nombre: `popolon_correr_03.png` (acciones con nombre fijo, número de dos cifras, etiqueta opcional como `_golpe`), `enemigo25_mover_00.png`, `mundo05_fondo_medio.png`. Lista completa en el README | `ArtLibrary.cs`, `README.md` |
| «Capas planas … Entre 6 y 8 planos por sala» | Hecho en el motor: 7 planos a profundidad real (cielo 40, lejano 24, niebla 16, medio 10, niebla baja 5, cercano 3,5, sala 0, primer plano −4) más las motas repartidas en profundidad | `Atmosphere.cs`, `RoomCamera.cs` |

## Para decidir antes de enviarlo

1. **El compilado de prueba lleva datos de la ROM** (salas, música, gráficos del final y de los jefes). Darlo a alguien de
   fuera redistribuye material de Konami. Opciones: entregar capturas y vídeo (el lote `logs/unity/encargo/`), o compilar
   una versión sin `Assets/Generated` ni audio para el artista (habría que hacerla).
2. **Un diseño por tipo para 63 enemigos** (53 + 10) es más del doble de lo que decía el documento. Si el presupuesto era
   para 42, hay que decidir si los hijos y los generados comparten familia con su padre (como ahora con los arquetipos) o
   si se recortan tipos.
3. **Las cajas que cambian con el estado** (27 tipos marcados «sí» en `cajas_colision.md`): el documento solo habla de
   «ajustados a su caja». Cada estado con caja distinta es un lienzo distinto; conviene decirlo en el encargo.
4. **Anticipación de la estocada:** o se acepta que vaya en la pose de guardia (lo que hace el motor), o se cambia la
   regla de «el golpe cae en el tick del original» y se retrasa el daño unos fotogramas. Lo segundo cambia la jugabilidad
   medida; no lo recomiendo.
5. **Los jefes** salen de un bloque de tiles de hasta 15 × 9 (King Dragon, 120 × 72 px): el lienzo es grande (960 × 576
   texels) y cada pose cambia las cajas. El documento dice «tamaño según su caja de colisión»; es según su **bloque**, y
   las cajas van dentro.
6. **Fuente Cinzel (OFL)** está bien como propuesta; falta decidir si se usa también para el texto o solo para títulos
   (la guía usa Optima/Palatino del sistema).

## Lo que ya cuadra

- 320 salas jugables: 146 + 15 + 15 + 16 + 17 + 18 + 19 + 20 + 21 + 27 + 6.
- Los 11 temas, dominantes, acentos y partículas coinciden con `WorldPalette.cs` y la guía.
- 47 iconos de objetos (1 … 0x2F), 6 dioses de los santuarios, 10 jefes, 4 versiones de escudo (sin, cobre, plata, oro).
- Puerta de mundo en 3 estados y apertura en 3 pasos de 6 ticks; piedra rompible de 2 × 2 tiles; escalera en dos mitades.
- Rejilla de 32 × 20 tiles de 8 px; cámara fija por sala con corte seco; parallax leve según el jugador.
- La espada golpea en una franja horizontal (x + 8 … x + 18, y − 7 … y − 4).
- Teclas F5, F6, F7, F9 como dice el documento.
- La lista de capas, luces, bloom, viñeta, partículas y destello al golpear que el documento da por hechas existe en el prototipo.

## Lo que el motor ya hace tras esta revisión («Lo que hacemos nosotros en el motor»)

| Punto del encargo | Estado |
|---|---|
| Cámara en perspectiva con los planos a profundidad real, fija en cada sala | Hecho (`RoomCamera`, lens shift para que la sala no se mueva) |
| Desenfoque por distancia en los planos lejanos y el primer plano | Hecho (shader `LayerBlur`, un desenfoque por plano) |
| Oclusión ambiental calculada a partir de la colisión | Hecho (`AmbientOcclusion`, se rehace si se abre un muro) |
| Luces 2D con mapas de normales y sombras de contacto | Hecho: las luces puntuales leen `_NormalMap`; sombra bajo el jugador y cada enemigo |
| Reproductor de animación a 24 fps con eventos en los fotogramas de impacto | Hecho (`FrameAnimator`, `ArtLibrary`, etiqueta `golpe` alineada al tick del golpe) |
| Importar el arte por nombre de fichero | Hecho (`ArtImporter`: escala, pivote, normales); los jefes se conectan cuando llegue el primero |
