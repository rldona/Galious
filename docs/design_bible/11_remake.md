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

## RemakeRules.SoloKnight (activado)

**Galious: The Hollow Kingdom** (el usuario, 16 de octubre de 2026): el juego conserva la base y las reglas de Galious pero evoluciona con sistemas de Hollow Knight. El primer cambio: solo juega el Caballero (Popolon). En el original hay dos personajes que se cambian en el menú y se pasan las vidas. Nada exige a Aphrodite: Popolon entra en el mundo 5 sin la Conch Shell y en el 7 con el Necklace (de la tienda), rompe paredes y cruza puertas giratorias sin objetos, y la única ventaja de ella, el agua, ya no hace daño con `HollowSwim`. Con la regla, se empieza con Popolon, se queda la vida de reserva de Aphrodite (hasta que lleguen los bancos y la muerte de Hollow Knight), el menú no deja cambiar de personaje y la Muerte no la resucita; el marcador muestra solo sus máscaras. Su código y sus medidas se conservan (las pruebas del original la usan). Los objetos que solo le servían a ella (Pendant, Ring, Bread & Water, Conch Shell) se aprovecharán como amuletos. **Activado.**

## RemakeRules.HollowSoul (activado)

En el original, cada enemigo muerto da experiencia y a 48 se rellena la vitalidad (×2 con el Vase). El usuario pidió sustituirla por el **alma** de Hollow Knight (`Soul`): cada espadazo que acierta da 11 (22 con el Vase; las armas secundarias no dan), hasta 99, y el orbe del marcador la muestra. **Concentrarse**: mantener F (o el botón B del mando) quieto en el suelo, sin la vida llena y fuera del agua, durante 0,9 s gasta 33 (un tercio del orbe) y cura 2 máscaras; si se sigue manteniendo, otra. Soltar, recibir un golpe o moverse lo corta sin gastar. El Caballero usa las animaciones Focus y Focus Get Once de Hollow Knight y sus efectos de luz (Light Effects: Focus Effect, la luz que sube mientras se concentra, y Burst Effect, el estallido al curarse; `Visuals/FocusFx`). El orbe es la vasija de Hollow Knight: la cara del Caballero (Soul Orb Glow) se ve hasta donde llega el líquido, con su superficie ondulada animada. El alma se pierde al morir y se guarda en el banco. **Activado.** Test: `SoulTests`.

## RemakeRules.HollowCharms (activado)

En el original cada objeto hace efecto desde que se recoge y el menú F1 los enseña en las posiciones de la pantalla de MSX. El usuario pidió el inventario y los amuletos de Hollow Knight: los objetos **pasivos de combate** pasan a ser **amuletos** que cuestan muescas y solo hacen efecto equipados (`Inventory.CharmCost`): Corona, Yelmo, Armadura, Triángulo, Jarra, Lámpara, Pendientes, Pulsera y Arpa cuestan 1; Botas, Jarrón, Sable y el escudo, 2; el Casco de sombrero (la mitad de daño), 3. Los tres escudos son un solo amuleto, del nivel del mejor que se tenga. Muescas: 3 al empezar y una más por jefe vencido (hasta 13); todos juntos cuestan 20, así que hay que elegir. Se equipan o se quitan **sentado en un banco**, como en Hollow Knight; al recoger uno se equipa solo si le caben las muescas (para que no parezca que no hace nada). Las llaves, los pases de mundo (Collar, Concha), los objetos de mundo (agua bendita, capa, vara, mapa, cruz, Gran Llave) y los de progreso (Remo, Muñeca, Hoi, Campana, Vela, Alfombra, Biblia, Daga, Ala, Anillo del ángel, Sal…) siguen activos siempre: quitarlos podría dejar la partida sin salida. Que un objeto ya se tiene (para que no vuelva a salir en el castillo o en los santuarios) cuenta aunque el amuleto no esté equipado (`Inventory.Owns`). El menú tiene tres pestañas (Inventario, Amuletos y Mapa, con Q/E o los gatillos) y, la primera vez que se recoge cada objeto, una **lámina** como las de Hollow Knight dice qué hace y cómo se usa (`ItemTexts`, `Visuals/ItemTutorial`). Las partidas guardadas de antes cuentan sus objetos como amuletos equipados. Pendiente (backlog del usuario): los iconos de los amuletos deben ser los objetos del original dibujados al estilo de Hollow Knight, no los amuletos de Hollow Knight. **Activado.** Test: `CharmTests`.

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
