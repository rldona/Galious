# Material para el encargo de arte

Lo que el encargo de dirección de arte promete en «Material que os damos», reunido en una carpeta. El encargo en sí es
el documento compartido (artefacto «Encargo de arte de Maze of Knight»); la guía de estilo es
[12_estilo_visual.md](../../design_bible/12_estilo_visual.md).

| Pieza | Dónde | Cómo se regenera |
|---|---|---|
| Cajas de colisión de los 53 enemigos de sala, 10 generados, 44 hijos y 10 jefes | [cajas_colision.md](cajas_colision.md) | `python3 tools/extract/art_boxes.py` |
| Hojas del arte provisional (personajes, enemigos, objetos) | [personajes.png](personajes.png), [enemigos.png](enemigos.png), [objetos.png](objetos.png) | menú **Maze of Knight → Lote del encargo de arte** |
| Capturas de salas con la rejilla de colisión encima (7 del castillo, 1 por mundo) | `logs/unity/encargo/sala_*.png` (no se versionan: llevan la disposición de las salas del original; se entregan aparte) | el mismo menú, o en batch `Unity -batchmode -projectPath unity/MazeOfKnight -executeMethod MazeOfKnight.EditorTools.VisualPreview.ArtPackBatch` |
| El juego compilado para probar | `unity/MazeOfKnight/Builds/Galious.app` | menú **Maze of Knight → Compilar para macOS** |

> El compilado lleva dentro las salas, la música y algún gráfico sacados de la ROM de Konami. Antes de dárselo a alguien
> de fuera hay que decidir qué se entrega: capturas y vídeo, o un compilado sin esos datos (ver la revisión del encargo).

## Escala y lienzos

- **8 texels por píxel del original.** Un tile de 8 px se entrega a 64 × 64; la sala (256 × 160 px) ocupa 2048 × 1280;
  un sprite de 16 × 16 px, 128 × 128. En Unity, 128 texels por unidad (el importador lo fija solo).
- **Personajes:** lienzo de 48 × 32 px del original (384 × 256 texels) con los pies en el borde de abajo, centrados (el importador pone el pivote abajo, en el centro).
  La caja de contacto es 13 × 13 px y la hoja de la espada llega a x + 18. El yelmo y la cresta pueden salir de la caja;
  el cuerpo que «se toca», no.
- **Enemigos y jefes:** cada fotograma en un lienzo con la proporción exacta de su caja ([cajas_colision.md](cajas_colision.md));
  el juego escala el dibujo a la caja. Si un tipo cambia de caja según el estado, cada estado va con su lienzo.
- **Fondos:** 24 × 15 unidades (la sala es 16 × 10): 3072 × 1920 texels el plano a la altura de la sala. Los planos
  lejanos pueden ir a la mitad; el primer plano, con todo lo que no es silueta transparente.

## Nombres de fichero

Todo son PNG con alfa, un fotograma por fichero, en `unity/MazeOfKnight/Assets/Art/Resources/Dibujado/<carpeta>/`.
El juego los agrupa en clips por nombre: **`<tipo>_<accion>_<NN>[_<etiqueta>].png`**, con `NN` el número de fotograma de
dos cifras desde `00`, y una etiqueta opcional en minúsculas. Lo que no es animación va sin número: `<tipo>_<nombre>.png`.
Los números de tipo o de mundo van pegados a la palabra (`enemigo25`, `jefe01`, `mundo05`), para no confundirlos con el
número de fotograma. Si un clip no existe, el juego sigue con el arte provisional de esa pieza.

| Carpeta | Pivote | Clips |
|---|---|---|
| `personajes/` | pies | `popolon_quieto`, `popolon_correr`, `popolon_girar`, `popolon_salto_preparar`, `popolon_salto_subir`, `popolon_salto_caer`, `popolon_escalera`, `popolon_estocada` (un fotograma con la etiqueta `golpe`), `popolon_golpe`, `popolon_muerte`, `popolon_celebrar`; lo mismo con `aphrodite_`. Escudos en capa aparte y con los mismos fotogramas: `popolon_escudo_cobre_correr_03.png`, metales `cobre`, `plata`, `oro`. |
| `enemigos/` | pies | `enemigoXX_quieto`, `enemigoXX_mover` (y más adelante `anticipar`, `atacar`, `golpe`, `muerte`), con `XX` el tipo en hexadecimal de dos cifras de [cajas_colision.md](cajas_colision.md). |
| `jefes/` | pies | `jefeNN_<pose>_<accion>`, con `NN` el mundo. Pendiente de conectar en el motor cuando llegue el primero. |
| `objetos_sala/` | pies | `piedra`, `pared_rompible`, `puerta_mundo_cerrada`, `puerta_mundo_media`, `puerta_mundo_abierta`, `puerta_llave`, `puerta_giratoria`, `ascensor`, `cartel_<dios>`. |
| `decoracion/` | centro | `mundoNN_farola` (va sobre el punto de luz), `mundoNN_liana_01`…`_04` (cuelgan de los techos), `mundoNN_estatua`, `mundoNN_columna_rota`, `mundoNN_pedestal` (en suelos con pared al lado). El juego las coloca solo por sala. |
| `tiles/` | centro | `mundoNN_suelo_borde`, `_lateral_izq`, `_lateral_der`, `_esquina_*`, `_interior`, `_fondo`, `_escalera_izq`, `_escalera_der`, `_agua`, `_lava`, con variantes `_v2`, `_v3`. 64 × 64 cada uno. `mundo00` es el castillo. |
| `fondos/` | centro | `mundoNN_cielo`, `_fondo_lejano`, `_fondo_medio`, `_fondo_cercano`, `_primer_plano`, `_niebla`; piezas sueltas `mundoNN_pieza_<nombre>`. |
| `interfaz/` | centro | `marco_<parte>` (9 partes), `icono_<objeto>`, `retrato_<personaje o dios>`, `titulo_mundoNN`. |
| `efectos/` | centro | `estela_estocada_NN`, `chispa_NN`, `esquirla_NN`, `polvo_NN`, `muerte_enemigo_NN`, `invocacion_NN`. |

- **Mapas de normales** (opcionales, para tiles y objetos de sala): el mismo nombre con `_n` al final
  (`mundo00_suelo_borde_n.png`). El importador los engancha solos y las luces 2D los usan. Los personajes y enemigos no
  se iluminan (tinta plana, se leen sobre cualquier fondo), así que no los necesitan.
- **Nada de luz ni sombra proyectada** que dependa de la sala: la pone el juego (luces 2D, oclusión ambiental calculada
  desde la colisión, sombras de contacto bajo cada personaje).
- Fuentes por capas (PSD, Krita o Procreate) además del PNG, en una carpeta aparte fuera del proyecto de Unity.

## Cómo encajan los tiempos

El juego corre a 30 ticks por segundo con la física del original; el arte se reproduce a 24 fps con su propio reloj.
Las poses clave caen en los ticks medidos:

- **Estocada:** golpea en el primer tick (4 ticks en total, 133 ms). El fotograma etiquetado `golpe` se muestra justo
  en ese tick; antes no hay tiempo de anticipación visible, así que la anticipación va en la pose de guardia y la fuerza,
  en la recuperación (2 o 3 fotogramas después del golpe).
- **Correr:** la velocidad la pone el juego (1,25 px/tick; 2,5 con los Zapatos); el ciclo de 8 a 10 fotogramas en
  bucle no tiene que cuadrar con la zancada píxel a píxel.
- **Escalera:** el fotograma avanza con la altura, como el original, no con el tiempo.
- **Puerta de mundo:** 3 pasos de 6 ticks (cerrada, a medio abrir, abierta).
- **Darse la vuelta** (`girar`) y **preparar el salto** se muestran enteros si el personaje sigue en el suelo; saltar o
  recibir un golpe los corta.

## Cómo revisar en el juego

| Tecla | Qué hace |
|---|---|
| F7 | pone o quita el estilo del remake (compara con los tiles provisionales) |
| F6 | recorre las paletas de las 11 zonas sobre la sala actual |
| F9 | cajas de colisión: verde el jugador, amarillo la espada, rojo los enemigos que hacen daño |
| F10 | rejilla de colisión de 8 px con cada clase teñida y la caja del jugador |
| F8 | texto de pruebas (sala, posición, estado) |
| F5 | desbloquea las puertas de los mundos 1 a 9 |
| F2 | dentro de un mundo, lleva a la sala de su jefe |
| F1 | menú de objetos; G da las seis armas y flechas |

Las comprobaciones del encargo («en gris», «con los ojos entornados», «con las cajas», «en hoja de zona») se hacen sobre
capturas del juego: `Maze of Knight → Capturas del estilo visual` guarda 13 en `logs/unity/visual_NN.png`.
