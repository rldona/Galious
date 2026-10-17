# 11. Notas para el remake (Maze of Knight)

Las decisiones de este capítulo son **propuestas [I]**, no hechos del original. Sirven de punto de partida para la fase 5 (Unity) y hay que validarlas jugando.

## Unidades y conversión

| Original | Propuesta en Unity |
|---|---|
| 1 sprite o personaje = 16 px | **1 unidad** de Unity |
| Sala de 256 × 160 px | 16 × 10 unidades (un bloque de 32 px = 2 unidades) |
| Tick de lógica a ~30 Hz | lógica en `FixedUpdate` a 60 Hz, con tiempos ×2 y velocidades ×½ por tick (o en px/s directamente) |
| Velocidad en px/tick | × 30 / 16 = unidades/s (andar: 1,7 px/tick ≈ 3,2 u/s) |
| Salto de 54 / 42 px | 3,4 / 2,6 unidades de alto (reescalar si el feel HK lo pide) |

## Qué conservar (identidad de Galious)

1. **Estructura:** un castillo conectado y 10 mundos-mazmorra con llave, palabra, lápida y jefe.
2. **Dos personajes intercambiables**, con capacidades complementarias:
   - Popolon: salto alto, romper paredes, cruzar puertas giratorias;
   - Aphrodite: agua y disparos múltiples.
   - Los objetos que compensan las diferencias: Ring, Pendant y Bread & Water.
3. **Secretos del laberinto:** piedras con objetos, paredes que suenan, muros que se cierran a tu espalda, escaleras que se retiran durante un minuto, eventos al limpiar salas, hadas.
4. **Economía de tres recursos** (flechas, monedas y llaves) y tiendas de dioses.
5. **Jefes con una regla de debilidad** (arma, zona o momento) y los objetos de mundo que los facilitan.
6. **Dependencias de objetos** (capítulo 4) para que la exploración siga siendo un metroidvania con llaves.

## Qué adaptar al estilo Hollow Knight

| Original | Adaptación propuesta |
|---|---|
| Salas fijas con corte | transición con fundido y cámara que encuadra la sala; parallax de fondo y luces 2D (URP) |
| Salto rígido | salto con control de altura (ya existe en Popolon), aceleración y coyote time; Aphrodite con salto más corto pero quizá un planeo |
| Espada corta horizontal | ataque con dirección (arriba y abajo) y **pogo** hacia abajo; conservar que solo cuente en la zona vulnerable de los jefes |
| Daño por contacto continuo | invulnerabilidad tras el golpe y retroceso legible |
| Armas secundarias con flechas | mantener el recurso flechas como "alma" o munición; la mina daña también al jugador |
| Escaleras | en el original, a media escalera solo se sube o se baja. Probado en el prototipo: con ← o → soltarse hacia un lado con sitio libre (`RemakeRules.LeaveLadderSideways`). **Desactivado**: el usuario prefiere por ahora el comportamiento original |
| Dash | no existe en el original. Si se añade, que no rompa los muros que se cierran ni las escaleras que se retiran (dar tiempo a reaccionar) |
| Contraseña | guardado en santuarios de Demeter |
| Teclear la palabra | selector o recitado con mando |
| Mundo 10 en un sitio al azar de 4 | se puede conservar como rejugabilidad, con la campana como pista |

## Datos para el importador de Unity

Las herramientas de `tools/extract/` generan, **en local y desde la ROM del usuario**:
- salas (`rooms.json`, `blocks.json`), **colisión por sala** (`collision/<mapa>.json`), enlaces, entidades (`entities.json`) y eventos de sala (`room_events.json`);
- catálogos de enemigos y objetos (`enemies.json`, `items.json`);
- jefes (`boss_index.json`), música (`score.json`) y efectos (`sfx.json`).

Propuesta para la fase 5: un importador que lea esos JSON y cree los ScriptableObjects y Tilemaps de cada sala con **tiles y sprites nuevos**, mapeando cada tile original a una clase (sólido, fondo, escalera, agua, lava) y cada objeto de sala a su prefab (puertas con llave, giratorias, de un sentido, muros que se cierran, paredes que se rompen). Así el remake reproduce la **disposición** del original sin distribuir sus gráficos.

**Antes de importar: todo resuelto.**
- **Colisión de cada tile:**
  - sólido ≥ 0xBA (≥ 0xF0 con jefe);
  - escalera 9A/9B;
  - agua B6/B7;
  - lava B8/B9.

  Cuadrícula por sala en `extracted/maps/collision/`; ver [world_maps.md](../world_maps.md#colisión-de-los-tiles-co).
- **Agua y lava:** animación por color (agua) y por patrón (lava), daño por personaje y Carpet. Ver [world_maps.md](../world_maps.md#agua-y-lava-co).
- **Sala del jefe:** la decoración se apaga y el jefe se dibuja con tiles, comprobado en los 10. En el remake, el jefe será un sprite o rig propio y el "apagón" un cambio de iluminación. Ver [boss_ai.md](../boss_ai.md#cómo-se-dibuja-el-jefe-oc).
- **Variables:**
  - E1C5: tempo del combate;
  - E1C9: música en pausa;
  - E500 = 4: retroceso del jugador;
  - objeto de sala de clase 3: puerta giratoria de un solo sentido.

  Ver [music.md](../music.md), [boss_ai.md](../boss_ai.md) y [entities.md](../entities.md).

**Cerrado también:**
- E1C6 es una variable muerta del driver;
- el jefe se mueve con posición lógica al píxel y dibujo ajustado a la rejilla de 8 px;
- la puesta en escena (decoración apagada y jefe hecho de tiles) se cumple en los 10 jefes.

Para el remake, el jefe puede moverse suave (sprite o rig propio); el salto de 8 px es una limitación del MSX, no parte del diseño.

## RemakeRules.BabyAlwaysPresent (desactivado)

Original (docs/ending.md): la piedra del bebé sale de (0..3) + (0..7); con 10 (1 de cada 32 veces) ninguna de las 10 piedras lo tiene y la sala, sin salidas, solo se deja perdiendo una vida. Opción del remake: una piedra al azar de las 10, siempre. Desactivada: se juega como el original.

## RemakeRules.NoRoomChangePause (activado)

En el original, al cambiar de sala el juego se para 11 ticks (~0,37 s) mientras dibuja la sala nueva (medido con la demo 1; `OriginalPhysics.RoomChangePauseTicks`). Se nota como un parón. Con la regla activada el jugador sigue sin pausa. **Activado**: el usuario no quiere el parón. Las demos la llevan siempre, porque sin ella se desincronizan de la grabación.

## RemakeRules.FalseKnightMace (activado)

En el original (docs/boss_ai.md) el Bony Dragon, jefe del mundo 1, lanza 3 huesos en parábola cada 16..47 ticks mientras pasea y en la cima de cada salto. Con el arte de Hollow Knight es el False Knight, y el usuario pidió que se comporte como él (16 de octubre de 2026): en esos momentos (no en la cima del salto, sí al caer de él) se para y da con la maza hacia el jugador. Los huesos no salen; lo que hace daño (2, por los 3 huesos de 1) es la bola de la maza donde golpea el suelo, `Enemies.MaceHead`: de 45 a 64 px por delante del centro de su caja y 19 px de alto, solo en los ticks 12..19 de los 30 del golpe (los fotogramas en que la bola baja y toca el suelo). No se le puede golpear. **Activado.** Test: `BossTests.BonyDragonSwingsTheMaceInsteadOfThrowingBones` (con y sin la regla).

## RemakeRules.HollowMessages (activado)

En el original salen abajo mensajes de lo que pasa; aquí se habían añadido más ("Enemigo derrotado", "Piedra rota", "Clinc", lo recogido, "Partida guardada"…). Hollow Knight no escribe nada en pantalla, y el usuario pidió quitarlos (17 de octubre de 2026). Con la regla, `GameBootstrap.Say` no escribe nada y solo salen los avisos (`GameBootstrap.Notice`): lo que dice la lápida al leerla con la Lupa, por qué no se puede entrar en una puerta o usar el Ala, que no hay partida o no se pudo guardar, y GAME OVER. Con las teclas de depuración salen todos. Los avisos de los menús (inventario, Cornifer, santuarios) no cambian. **Activado.**

## RemakeRules.HollowBosses (activado)

En el original (docs/boss_ai.md, cajas de los jefes) el Bony Dragon solo recibe golpes en la cabeza: 16 × 32 px en su lado izquierdo, que en los fotogramas en que no la baja queda por encima de una flecha disparada desde su suelo. Y tocar a un jefe quita su `contactDamage` (de 8 el Bony Dragon a 40 Galious), cuando el jugador empieza con 8 de vitalidad. Con el False Knight dibujado no se ve dónde está la cabeza: el usuario (17 de octubre de 2026) vio que las flechas lo atravesaban sin daño, que acercarse para darle con la espada era imposible y que un toque le quitaba todas las máscaras. Remake, como en Hollow Knight: el False Knight recibe golpes en todo el cuerpo (su caja de golpes es la de contacto) y tocar a cualquier jefe quita `BossContactMasks`, de 1 a 3 máscaras según su daño del original ((daño + 6) / 12: 1 el Bony Dragon, el Elusive Dragon, el Sea Anemone, Donra y el Jump Slime; 2 Arlyane y Big Lip; 3 Monster, King Dragon y Galious), una menos con la Cloak (mínimo 1). **Activado.** Test: `BossTests.ArrowsHitTheFalseKnightAndTouchingCostsOneMask` (con y sin la regla).

Ampliado el 17 de octubre de 2026 (el usuario: «si se golpea su maza le tiene que hacer daño»): con la regla, **todos** los jefes reciben golpes también en sus cajas de contacto (todo el cuerpo dibujado), además de en las de golpe del original; si se les puede dar en ese momento lo siguen decidiendo su prueba de choque (`CanBeHitNow`: el Sea Anemone abierto, el Elusive Dragon asomado, Galious con la Cross) y con qué arma, su máscara. Y la bola de la maza del False Knight (`Enemies.MaceHead`) recibe los golpes por él (`Boss.PartOf`); un espadazo que alcanza la maza y el cuerpo cuenta una vez. Test: `BossTests.HittingTheFalseKnightsMaceHurtsHim`.

## RemakeRules.SlashBoxFromArt (activado)

En el original la espada golpea una franja de 4 px de alto (y − 7 … y − 4) de x + 8 a x + 18 (b03:B8FB), y los tajos de arriba y abajo del remake, 20 × 18 px. El tajo de Hollow Knight que se ve es mucho mayor: hacia delante, de x + 0 a x + 22 y de 4 a 19 px sobre los pies; arriba y abajo, unos 20 × 22 (medido en las texturas `Hollow/personajes/popolon_estela` y `Hollow/tajos`). El usuario (17 de octubre de 2026) lo encontraba frustrante: el golpe no daba a lo que tocaba el tajo. Con la regla, la caja del golpe es la del tajo dibujado: delante, de x + 2 a x + 22 y de y − 19 a y − 4; arriba y abajo, `ArtSlashVertical` = 22 px de alto. **Activado.** Tests: `EnemyTests.SwordReachesEighteenPixels` y `PlayerMotorTests.SwordDoesNotReachTwentyPixels` (con y sin la regla).

## RemakeRules.SoloKnight (activado)

**Galious: The Hollow Kingdom** (el usuario, 16 de octubre de 2026): el juego conserva la base y las reglas de Galious pero evoluciona con sistemas de Hollow Knight. El primer cambio: solo juega el Caballero (Popolon). En el original hay dos personajes que se cambian en el menú y se pasan las vidas. Nada exige a Aphrodite: Popolon entra en el mundo 5 sin la Conch Shell y en el 7 con el Necklace (de la tienda), rompe paredes y cruza puertas giratorias sin objetos, y la única ventaja de ella, el agua, ya no hace daño con `HollowSwim`. Con la regla, se empieza con Popolon, se queda la vida de reserva de Aphrodite (hasta que lleguen los bancos y la muerte de Hollow Knight), el menú no deja cambiar de personaje y la Muerte no la resucita; el marcador muestra solo sus máscaras. Su código y sus medidas se conservan (las pruebas del original la usan). Los objetos que solo le servían a ella (Pendant, Ring, Bread & Water, Conch Shell) se quedan en el inventario, diciendo que el Caballero ya no los necesita. En el final, Aphrodite no sale en la escena del castillo (se tapa con el negro del arco de detrás), solo habla Popolon («NICE!») y el epílogo, con las mismas filas para no cambiar los tiempos, dice que Popolon protegerá el Reino Hueco como su caballero en vez de casarse con ella. **Activado.**

## RemakeRules.HollowSoul (activado)

En el original, cada enemigo muerto da experiencia y a 48 se rellena la vitalidad (×2 con el Vase). El usuario pidió sustituirla por el **alma** de Hollow Knight (`Soul`): cada espadazo que acierta da 11 (22 con el Vase; las armas secundarias no dan), hasta 99, y el orbe del marcador la muestra. **Concentrarse**: mantener F (o el botón B del mando) quieto en el suelo, sin la vida llena y fuera del agua, durante 0,9 s gasta 33 (un tercio del orbe) y cura 2 máscaras; si se sigue manteniendo, otra. Soltar, recibir un golpe o moverse lo corta sin gastar. El Caballero usa las animaciones Focus y Focus Get Once de Hollow Knight y sus efectos de luz (Light Effects: Focus Effect, la luz que sube mientras se concentra, y Burst Effect, el estallido al curarse; `Visuals/FocusFx`). El orbe es la vasija de Hollow Knight: la cara del Caballero (Soul Orb Glow) se ve hasta donde llega el líquido, con su superficie ondulada animada. El alma se pierde al morir y se guarda en el banco. **Activado.** Test: `SoulTests`.

## RemakeRules.HollowCharms (activado)

En el original cada objeto hace efecto desde que se recoge y el menú F1 los enseña en las posiciones de la pantalla de MSX. El usuario pidió el inventario y los amuletos de Hollow Knight: los objetos **pasivos de combate** pasan a ser **amuletos** que cuestan muescas y solo hacen efecto equipados (`Inventory.CharmCost`): Corona, Yelmo, Armadura, Triángulo, Jarra, Lámpara, Pendientes, Pulsera y Arpa cuestan 1; Botas, Jarrón, Sable y el escudo, 2; el Casco de sombrero (la mitad de daño), 3. Los tres escudos son un solo amuleto, del nivel del mejor que se tenga. Muescas: 3 al empezar y una más por jefe vencido (hasta 13); todos juntos cuestan 20, así que hay que elegir. Se equipan o se quitan **sentado en un banco**, como en Hollow Knight; al recoger uno se equipa solo si le caben las muescas (para que no parezca que no hace nada). Las llaves, los pases de mundo (Collar, Concha), los objetos de mundo (agua bendita, capa, vara, mapa, cruz, Gran Llave) y los de progreso (Remo, Muñeca, Hoi, Campana, Vela, Alfombra, Biblia, Daga, Ala, Anillo del ángel, Sal…) siguen activos siempre: quitarlos podría dejar la partida sin salida. Que un objeto ya se tiene (para que no vuelva a salir en el castillo o en los santuarios) cuenta aunque el amuleto no esté equipado (`Inventory.Owns`). El menú tiene tres pestañas (Inventario, Amuletos y Mapa, con Q/E o los gatillos) y, la primera vez que se recoge cada objeto, una **lámina** como las de Hollow Knight dice qué hace y cómo se usa (`ItemTexts`, `Visuals/ItemTutorial`). Las partidas guardadas de antes cuentan sus objetos como amuletos equipados. Los iconos de los objetos (del 0x07 al 0x2C, amuletos incluidos) son los del original redibujados al estilo de Hollow Knight, como pidió el usuario: `tools/hollow/iconos.py` lee el icono de 16 × 16 de la ROM (b00:58E1) y lo agranda suavizado, con colores apagados y fríos, luz de arriba y contorno de tinta; las armas y los recursos siguen con iconos de Hollow Knight. **Activado.** Test: `CharmTests`.

## RemakeRules.HollowJournal (activado)

El original no lleva registro de los enemigos. El usuario pidió el **diario del cazador** de Hollow Knight: cada criatura del juego ya lleva el dibujo de una de Hollow Knight, así que su entrada es la de esa criatura (retrato, nombre, descripción y notas del cazador en castellano, importados en local por `tools/hollow/diario.py`; las cuatro que no tienen entrada allí llevan un texto propio). Los tipos que comparten criatura comparten entrada (61 en total, los jefes al final). `Journal` cuenta las derrotas por tipo (los vulnerables al morir, los jefes al vencerlos y los invulnerables, como pinchos y torretas, al verlos, porque no se pueden derrotar); la sombra del jugador no cuenta. Las notas del cazador se leen a partir de 10 derrotas (los jefes, 1). La primera vez que se registra una criatura sale un aviso abajo a la derecha, como en Hollow Knight. Pestaña Diario del inventario: lista de criaturas (las no encontradas, sin nombre) y la elegida con su retrato. Se guarda con la partida. No cambia nada del juego. **Activado.** Test: `JournalTests`.

## RemakeRules.HollowMaps (activado)

En el original el mapa de cada mundo es un objeto escondido (0x0A) y el menú pinta una cuadrícula de salas. El usuario pidió el **Cornifer** de Hollow Knight, uno en cada mundo, que vende el mapa, y el mapa **dibujado a mano**. Cornifer está en los mundos 1–9 (el 10 no tiene mapa en el original). Conseguir el mapa tiene que costar (lo pidió el usuario), así que está en la sala más alejada de la entrada contando salas (de 5 a 12 según el mundo), con suelo para él y lejos de donde aparecen enemigos (los que salen a su lado no aparecen, como junto a los bancos); a igual distancia, mejor en una sala sin enemigos. Cuesta 20 + 10 geo por mundo (de 30 a 110). Se le oye tararear al acercarse; con ↓ a su lado se le escucha: la primera vez se presenta, luego saluda con lo que dice en la zona de Hollow Knight de ese mundo y pregunta si se quiere el mapa (Sí / No). Comprarlo da el objeto Map de ese mundo, como encontrarlo, que sigue en su sitio. En la pestaña Mapa, como en Hollow Knight: sin el mapa de la zona no se ve nada (el pergamino tachado y "No tienes el mapa de esta zona"); con él, todas las salas, desde su colisión, las no visitadas solo su contorno a trazos y las visitadas entintadas, con sus bancos, la máscara del Caballero donde estás y, con la Lámpara, la marca del jefe. El castillo también tiene su Cornifer (lo pidió el usuario: antes se conocía entero desde el principio): habla como el de los Cruces Olvidados y vende su mapa por 20 geo; no está en la sala más alejada (con 146 salas, al llegar ya se habría recorrido todo) sino, como en Hollow Knight, a media distancia, en la más alejada de las que están a 6–10 salas de la de inicio, fuera de las salas con puerta de mundo. Sin su mapa, el castillo tampoco se ve. Las salas visitadas se guardan con la partida. Sus frases, su dibujo y su tarareo se importan en local (`tools/hollow/cornifer.py`). **Activado.** Test: `CorniferTests`.

## Mando y opciones (paso 7 de Galious: The Hollow Kingdom)

No cambia ninguna regla del juego. Todo se puede jugar con mando: moverse con el stick o la cruceta, A saltar, X atacar, B concentrarse, Y el arma, LB la Biblia (Ctrl en el teclado), Select el inventario, Start la pausa y RB un **selector de letras** para escribir las palabras que en el original se teclean (las de los jefes y UMBRELLA con la Daga; `Visuals/WordPad`). Los botones se cambian en Opciones (`Controls`, en PlayerPrefs); moverse, ↑ para saltar con el teclado, Esc y Start son fijos. Los menús responden también al stick. **Opciones**, como las de Hollow Knight (`GameSettings`, `Visuals/OptionsMenu`): Audio (volumen general, sonido y música de 0 a 10; la música y los efectos salen del mismo chip y el efecto ocupa el canal C, que mientras suena va con el volumen de los efectos: con los dos al máximo suena como el original), Vídeo (resolución, pantalla completa, sincronía vertical, límite de fotogramas), Brillo (exposición del posproceso, con una escena de muestra), Mando (el dibujo del mando de Hollow Knight y sus botones) y Teclado. Salir del juego y salir al menú principal piden confirmación. Al arrancar, el aviso de Hollow Knight de que el juego guarda solo (en los bancos). Test: `ControlsTests`.

## RemakeRules.HollowSlashes (activado)

En el original la espada solo golpea hacia delante y ↑ salta. El usuario pidió los espadazos de Hollow Knight: con ↑ pulsado el golpe va hacia arriba (de pie o en el aire) y con ↓ en el aire hacia abajo; el de abajo que acierta (enemigo, escudo o piedra) hace rebotar al Caballero hacia arriba, como el "pogo" de Hollow Knight. Para apuntar arriba sin saltar, saltar pasa a su propio botón (A en el mando y Z en el teclado, cambiables en Opciones) y ↑ solo apunta y sube escaleras (si hay una justo encima, ↑ la coge, como antes). Las cajas del golpe: 10 px a cada lado del centro y 16 de alto, encima de la cabeza o debajo de los pies; las paredes que se rompen solo se golpean de frente. Dibujo: UpSlash y DownSlash del Caballero y sus tajos. Test: `PlayerMotorTests.HollowSlashesGoUpOnTheGroundDownInTheAirAndBounce`.

## Sonidos de golpe y vibración

Con la espada (o un arma) suenan los efectos del original al acertar: 0x1E al enemigo, 0x27 al jefe, 0x20 al morir y 0x1D al chocar con un escudo; antes no sonaba nada al golpear enemigos. Con mando, vibración como en Hollow Knight (`Rumble`, se quita en Opciones > Mando); en macOS va por el framework de mandos de Apple (GameController + CoreHaptics, plugin `Plugins/macOS/GameHaptics.bundle` de `tools/native/`), porque el Input System no hace vibrar los mandos allí: pulsos al golpear enemigos, piedras, paredes o escudos, al matar, al recibir daño y al curarse, y un zumbido que crece mientras se concentra.

## RemakeRules.HollowMusic (activado)

La música de fondo es la de Hollow Knight (lo pidió el usuario): en cada mapa la de su zona (castillo → Cruces Olvidados, 1 → Sendero Verde, 2 → Canales Reales, 3 → la Colmena, 4 → Picos de Cristal, 5 → Ciudad de las Lágrimas, 6 → Páramos Fúngicos, 7 → el Abismo, 8 → Nido Profundo, 9 → Confines del Reino, 10 → Palacio Blanco; las que en Hollow Knight van por capas, mezcladas), en los santuarios la de las salas con un personaje y en cada jefe la de la pelea con el de Hollow Knight que hace de él (False Knight, Mantis Lords, Hornet, Uumuu, Grimm, Mawlek, Broken Vessel, Radiance, Nosk, Dung Defender). Las fanfarrias del original (objetos, invocación, jefe vencido, muerte y el final) siguen con el chip, con la otra bajada mientras suenan. Cambia con un fundido y sigue el volumen de música de Opciones. Las pistas de Hollow Knight vienen muy bajas (−25 a −29 dB de media) y quedaban por debajo de los efectos del chip: se normalizan a −14 LUFS al importarlas y, con el volumen general a 7 (el de serie), suenan enteras.

## Decorados y campanita

En el original las lápidas, las entradas a los santuarios del castillo y el marco de las puertas de los mundos son baldosas del fondo, y el escenario de Hollow Knight solo pinta la colisión: se ponen como sprites de Hollow Knight (`tools/hollow/decorados.py`, `GameBootstrap.Decor`): las lápidas (la de la palabra del jefe, una tablilla que brilla), las entradas a las tiendas, una boca de cueva iluminada por dentro (la de la zona del mapa si la hay), y las de los otros santuarios (escondidas mientras las tape su piedra), y en las puertas de los mundos una fachada de Hollow Knight según el mundo al que lleva (la cúpula de los Creyentes; el arco de Sin Ojos en el 7 y el 8; el Templo del Huevo Negro en el 9), encajada entre el suelo y el techo, con un estandarte rojo de la troupe de Grimm a cada lado (donde no hay pared) e iluminada (un halo que late y una luz 2D) para que se distingan. Con la fachada, el hueco de la puerta no lleva el sello de la puerta de jefe (parecía una piedra): se ve abierto, pero sigue sin poder pasarse hasta que se abre, y delante de una puerta cerrada se avisa en el centro de la pantalla, entre las flechas de filigrana de los menús, de que hace falta la Gran Llave (y, abierta, "↓ Entrar"). Los adornos del suelo del escenario no se ponen delante de las puertas, las entradas, los bancos, Cornifer ni las lápidas.

Con el arte de Hollow Knight, el tamaño de cada enemigo se fija al aparecer (el de su forma completa en los que crecen, como los pinchos y los brotes) y solo cambian sus fotogramas: en el original su caja cambia con cada pose y el dibujo se estiraba y encogía. Las flechas (armas 1 y 2) se dibujan 7 px más arriba que su caja, a media altura del Caballero; la caja no se mueve. Las flechas y las otras armas usan los proyectiles de Hollow Knight (`tools/hollow/armas.py`). La campanita 0x2E suena al romperse una pared (como en el original) y también, añadida, cuando desaparece una escalera o se cierra un muro.

## Santuarios como en Hollow Knight

La pantalla del santuario del original (nombre, retrato de 2 × 2 casillas, ventana de texto y tres huecos con la mano) se dibuja como en Hollow Knight (`ShrineScreen.DrawHollow`): cada dios lo hace un personaje de Hollow Knight con su animación de reposo (`tools/hollow/dioses.py`: Hefesto, el Forjador de Aguijones; Ares, Sly; Deméter, Iselda; Atenea, el Anciano Hu; Asclepio, Jiji; la Muerte, Grimm), su nombre con la filigrana, lo que dice en castellano (`ShrineTexts`; con el Caballero solo, la Muerte y la pista de Atenea a Aphrodite se adaptan) y, en las tiendas, los tres objetos con su icono, nombre, precio en geo y descripción. Se maneja también con mando: cruceta o stick para elegir, A (o Espacio, Intro, Z) para comprar o responder y B, Esc o ↓ para salir; el ↓ o el stick con que se entra no cuentan. Las reglas (precios, escudos en orden, carcaj, llaves, guardar con Deméter) no cambian. Al entrar en los del castillo el Caballero no se hunde 16 px en el suelo como en el original (se le veía por encima de la piedra): la pantalla funde a negro en 0,45 s y, al salir, vuelve de negro con el Caballero de pie en su sitio.

## RemakeRules.HollowAmbience (activado)

El ambiente de cada mundo como lo pone Team Cherry (lo pidió el usuario; `Visuals/HollowAmbience`, texturas con `tools/hollow/ambiente.py`). En Hollow Knight cada escena lleva un `SceneManager` con la saturación y las curvas de color, la luz ambiente, el color de la luz del Caballero y un nivel de oscuridad (de −1 a 2: la viñeta deja un hueco alrededor del Caballero, que la linterna agranda), y la cámara enciende las partículas de su zona. Aquí, por mundo, con su zona de Hollow Knight:

| Mapa | Zona | Oscuridad | Partículas |
|---|---|---|---|
| castillo | Cruces Olvidados | 0 | polvo |
| 1 | Sendero Verde | 0 | hojas |
| 2 | Canales Reales | 1 | goteo y niebla baja |
| 3 | la Colmena | 0 | hexágonos |
| 4 | Pico de Cristal | 0 | cristales que flotan |
| 5 | Ciudad de las Lágrimas | 0 | lluvia |
| 6 | Páramos Fúngicos | 0 | esporas |
| 7 | el Abismo | 1 | humo |
| 8 | Nido Profundo | 1 | hilos de telaraña y polvo |
| 9 | Confines del Reino | 0 | ceniza con viento |
| 10 | Palacio Blanco | −1 | pétalos |

Los niveles de oscuridad salen de los datos del juego (en los Canales, el Abismo y el Nido la mayoría de las salas son de nivel 1); el hueco es más amplio y el velo menos opaco que en Hollow Knight, porque aquí la sala entera está en pantalla y hay que ver por dónde se va. La Vela hace de linterna: con ella el hueco se abre. La luz de las puertas de los mundos atraviesa la oscuridad.

Los valores de color de cada escena (saturación, curvas, luz ambiente) no están en la carpeta de Hollow Knight decodificado, solo los campos en el código. Se han medido con `tools/hollow/medir_ambiente.py`: el color medio, la saturación y el brillo de todas las salas de cada zona (las imágenes de `hollow-knight-mundos`, reconstruidas desde sus ficheros de nivel) frente a nuestras salas sin gradación (`-ambienteNeutro -capturaSalas`). De ahí salen el filtro de color y la saturación de cada mundo, a la que se suman los 17 puntos que la cámara de Hollow Knight añade siempre (`SceneManager.AdjustSaturationForPlatform`) y que las imágenes no llevan. Las salas de Hollow Knight son más luminosas y menos saturadas que las nuestras, con bruma entre los planos y sombra junto a la roca; el usuario pidió ese toque ambiental: cada sala lleva oclusión ambiental (`AmbientOcclusion`, desde la colisión), una bruma del color medio de la zona entre el fondo y el plano de juego, más espesa abajo, y jirones de niebla (`Fog.png`) que pasan despacio.

## Fluidez, fotogramas y pruebas

- **Interpolación:** la lógica sigue a 30 pasos por segundo, como el original, pero el Caballero, los enemigos, los jefes, los proyectiles, la estela, el tajo y la luz del Caballero se dibujan entre la posición del paso anterior y la del actual (`PlayerController.TickAlpha`). Sin ello, a 60 o 120 FPS el movimiento se veía a saltos de 30 por segundo: andando, el dibujo se quedaba quieto el 50 % de los fotogramas a 60 FPS y el 74 % a 120; con ella, ninguno (medido con `-rendimiento`). No se interpola en los cambios de sala ni en saltos de más de 24 px, ni mientras el juego está parado por un golpe. Las imágenes de jefe del original van en su rejilla de 8 px.
- **Fotogramas:** límite de 60 o 120 (120 por defecto); con sincronía vertical, el refresco de la pantalla. Sin límite el juego pasaba de 700 FPS en un M1 Pro.
- **Teclas de depuración** (K, G, F2–F11) y los textos de daño ("-1 (0x1F1)") solo con `-depuracion` o en compilaciones de desarrollo.
- Las capturas, el recorrido y la medida guardan la partida aparte (`partida_pruebas.json`).

## Música del menú principal

El menú principal suena con el tema del título de Hollow Knight (Title, importado en local por `tools/hollow/musica.py`), que entra tras el aviso y se apaga al salir, con el volumen de música de Opciones. La música de la partida sigue siendo la del original.

## RemakeRules.HollowBenches y HollowDeath (activados)

En el original se guarda en el santuario de Demeter (con la contraseña) y, al perder todas las vidas, GAME OVER. Como en Hollow Knight (petición del usuario, 16 de octubre de 2026):

- **Bancos** (`Benches`, 18): uno en la sala donde se empieza en el castillo, uno en la entrada de cada mundo (o en la sala tranquila más cercana: la entrada del mundo 10 es lava) y otro en una **sala vecina de la del jefe** (elegido por el usuario; el mundo 4 no tiene, su única vecina son pozos de escalera). Nunca en la sala del jefe. Siempre **lejos de escaleras**, puertas, objetos y paredes que se abren: un suelo de 4 casillas con 3 filas de aire encima y hasta 3 casillas libres a cada lado por encima del suelo. Los de la entrada, solo en salas **sin enemigos**; el de junto al jefe puede estar en una sala con ellos (en los mapas del original solo la entrada y la sala del jefe están vacías), pero lejos de donde aparecen, y los que aparecerían a menos de 72 px no salen (salvo los que cuentan para abrir una pared). Sentado, no se recibe daño (los que andan pueden llegar). Cada zona usa su banco de Hollow Knight (`tools/hollow/bancos.py`).
- **Sentarse** (↓ encima; "↓ Descansar" sale sobre el banco): el Caballero se sienta con su animación, con motas de luz (`Visuals/BenchRest`: un anillo que se abre al sentarse y, mientras descansa, motas que suben; sin destello, lo quitó el usuario), se rellenan **todas las máscaras** una a una, se guarda la partida (con la cresta abajo a la derecha) y ese banco pasa a ser el sitio de reaparición. Cualquier dirección, salto o espada lo levanta.
- **Muerte**: sin vidas ni GAME OVER. Se vuelve al último banco (o al del castillo) con todas las máscaras, despertando con la animación de Hollow Knight; si el combate de un jefe estaba en marcha se deja (el jefe espera otra vez su palabra). El geo (monedas) se queda en la **sombra** (`Enemies.Shade`, la Hollow Shade) donde se murió: flota, va a por el jugador y su toque hace daño; al derrotarla (4 espadazos) se recupera. Morir otra vez antes pierde el de la sombra anterior. "Continuar" despierta en el banco donde se guardó.

**Activados.** Tests: `BenchTests` (con los mapas reales: un banco en el castillo y en cada mundo, en suelo firme, sin enemigos ni escaleras pegadas). Capturas: `-capturaBancos <carpeta>`.

## RemakeRules.HollowTitle (activado)

En el original, al encender salen el logo de Konami, el título, la historia y las demos en bucle (docs/attract.md); el remake los imitaba con texto. El usuario pidió quitarlo ("no aporta nada") y poner un menú principal como el de Hollow Knight: sobre las nubes y agujas lejanas del reino (una vista lejana del Palacio Blanco, oscurecida y en violeta, con niebla que pasa, motas y viñeta), el título **GALIOUS — THE HOLLOW KINGDOM** con las filigranas de los menús de Hollow Knight (no su logo) y las opciones Continuar (si hay partida guardada), Nueva partida, Opciones (volumen, pantalla completa), Extras (el homenaje y a quién pertenece cada cosa) y Salir. En la partida, Esc o Start abren el menú de pausa: Continuar, Opciones y Salir al menú principal. Toda la interfaz usa la fuente de Hollow Knight (Perpetua) si está importada. Se maneja con teclado o mando. Sin el arte del menú, el ciclo del original. **Activado.** Capturas: `-capturaMenu <carpeta>`.

## RemakeRules.HollowSwim (activado)

En el original, en el agua (como en la lava) el jugador se hunde despacio hasta el fondo (y = 184) y todo va 4 veces más lento; anda a la mitad de velocidad y salta igual de alto pero muy despacio (docs/player.md). El usuario pidió que se nade como en Hollow Knight (16 de octubre de 2026): en el agua el jugador flota en la superficie con los pies `SwimDepth` (8) px por debajo, nada a izquierda y derecha a la mitad de velocidad y salta y cae a la velocidad de fuera del agua; si entra por debajo de la superficie, sube 1 px por tick. El Caballero usa las animaciones de nado de Hollow Knight (Surface Swim y Surface Idle); Hornet no tiene, así que corre o se queda quieta flotando. El agua no quita vida, como en Hollow Knight (en el original, 1 cada 4 ticks; Aphrodite, 1 cada 255): el usuario lo pidió así. El agua se pinta por delante de los personajes, translúcida, para que se vean medio dentro. **Activado.** Test: `PlayerMotorTests.HollowSwimFloatsSwimsAndJumpsOut`; los tests del agua del original la desactivan.

## RemakeRules.LavaRespawn (activado)

En el original la lava quita 1 de vitalidad cada 4 ticks mientras se está dentro y el jugador se hunde hasta el fondo. El usuario pidió que se comporte como los peligros de Hollow Knight: tocarla es como chocar con un enemigo (`LavaDamage`, 2, con su invulnerabilidad y su parpadeo) y el jugador vuelve al último sitio de suelo firme desde el que cayó (el borde de la plataforma o el punto donde saltó), aunque esté en otra sala. **Activado.** Comprobado con `-capturaSalas` (mundo 8, sala 16); el registro dice adónde vuelve (`[Lava]`).

## Sensación de Hollow Knight: HitStop, CoyoteJump y JumpBuffer (desactivados)

Tres técnicas de la guía «Hollow Knight por dentro» (en la carpeta de Hollow Knight decodificado), pasadas a los ticks del original (30 por segundo). No cambian la física medida: solo perdonan o marcan el tiempo. **F11** activa o quita las tres a la vez (`RemakeRules.HollowFeel`) para probarlas; por defecto se juega como el original.

- **HitStop:** al acertar un golpe (enemigo herido o muerto), todo el juego se para 3 ticks (0,1 s; Hollow Knight, unos 0,11 s), también la animación del jugador. Las pulsaciones de esos ticks se guardan. No se aplica en las demos.
- **CoyoteJump:** el original deja al jugador 4 ticks quieto en el aire al salir andando de un borde, sin poder hacer nada. Con la regla, una pulsación de ↑ en los 2 primeros salta como desde el suelo (sigue a la misma altura, así que el salto es idéntico). Hollow Knight da 0,04 s.
- **JumpBuffer:** en el original, ↑ pulsado en el aire no hace nada al aterrizar. Con la regla, una pulsación nueva en los 2 últimos ticks en el aire salta en el primer tick en el suelo si ↑ sigue pulsado. Hollow Knight da 0,04 s.

Tests: `HollowFeelTests` (desactivadas, el motor hace lo medido; activadas, los márgenes exactos).
