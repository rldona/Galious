using UnityEngine;

namespace MazeOfKnight.Game
{
    // Cambios del remake respecto al original (Design Bible, cap. 11). Cada uno se puede desactivar para
    // volver al comportamiento medido en openMSX.
    public static class RemakeRules
    {
        // Original: a media escalera, ← y → no hacen nada (medido en el castillo, sala 1). Remake: si hay sitio a
        // ese lado, el jugador se suelta y cae con control en el aire.
        public static bool LeaveLadderSideways = false;   // desactivado: se juega como el original

        // Original: la piedra del bebé (mundo 10, sala 6) sale de (0..3) + (0..7), así que 1 de cada 32 veces vale 10 y
        // ninguna de las 10 piedras lo tiene: solo se sale perdiendo una vida (docs/ending.md). Remake: una de las 10 al
        // azar, siempre.
        public static bool BabyAlwaysPresent = false;     // desactivado: se juega como el original

        // Original: al cambiar de sala el juego se para 11 ticks (~0,37 s) mientras carga la sala nueva (medido con la
        // demo 1). Remake: se sigue sin pausa. Las demos la llevan siempre.
        public static bool NoRoomChangePause = true;      // activado: el usuario prefiere no tener el parón

        // Original: el Bony Dragon (jefe del mundo 1) lanza 3 huesos en parábola andando y en la cima de cada salto. Remake
        // (con el False Knight de Hollow Knight, petición del usuario del 16 de octubre de 2026): en esos momentos da con la
        // maza delante, hacia el jugador; al caer de un salto, también. Los huesos no salen y lo que hace daño es la bola
        // de la maza donde golpea el suelo (Enemies.MaceHead).
        public static bool FalseKnightMace = true;        // activado: lo ha pedido el usuario

        // Original: el Bony Dragon solo recibe golpes en la cabeza (16 px en su lado izquierdo, y en los fotogramas en que
        // la baja) y tocar a un jefe quita de 8 a 40 de vitalidad, cuando se empieza con 8: con el False Knight dibujado
        // las flechas lo atravesaban sin daño, acercarse para darle con la espada era imposible y un toque mataba (el
        // usuario, 17 de octubre de 2026). Remake, como en Hollow Knight: el False Knight recibe golpes en todo el cuerpo y
        // tocar a un jefe quita BossContactMasks (de 1 a 3 máscaras, más cuanto más fuerte en el original).
        // Original: abajo salen mensajes de lo que pasa (enemigo derrotado, piedra rota, lo recogido…). Remake (el usuario,
        // 17 de octubre de 2026), como Hollow Knight, que no escribe nada: solo los avisos (GameBootstrap.Notice: lo que dice
        // la lápida y por qué no se puede hacer algo); con las teclas de depuración, todos.
        public static bool HollowMessages = true;         // activado: lo ha pedido el usuario
        public static bool HollowBosses = true;           // activado: lo ha pedido el usuario
        public static int BossContactMasks(int original) => System.Math.Max(1, System.Math.Min(3, (original + 6) / 12));

        // Original: en el agua (y en la lava) se hunde despacio hasta el fondo y todo va 4 veces más lento. Remake (petición
        // del usuario del 16 de octubre de 2026, como en Hollow Knight): en el agua el jugador flota en la superficie con
        // los pies SwimDepth px por debajo y nada a izquierda y derecha (a la mitad de velocidad, como el original en el
        // agua); salta y cae con la velocidad de fuera. Si entra por debajo de la superficie, sube 1 px por tick. El agua
        // no quita vida (en el original, 1 cada 4 ticks; Aphrodite, cada 255): el usuario lo pidió así.
        public static bool HollowSwim = true;             // activado: lo ha pedido el usuario
        public const int SwimDepth = 8;

        // Original: la lava quita 1 de vitalidad cada 4 ticks mientras se está dentro (y se hunde hasta el fondo). Remake
        // (petición del usuario, como los peligros de Hollow Knight): tocarla es como chocar con un enemigo (LavaDamage,
        // con su invulnerabilidad) y el jugador vuelve al último sitio de suelo firme desde el que cayó.
        public static bool LavaRespawn = true;            // activado: lo ha pedido el usuario
        public const int LavaDamage = 2;

        // Galious: The Hollow Kingdom (16 de octubre de 2026): solo el Caballero (Popolon). Original: dos personajes, Popolon y
        // Aphrodite, que se cambian en el menú y se pasan las vidas. Remake: Aphrodite no está; Popolon se queda con su vida
        // de reserva y nada la resucita. Nada del juego la exige: Popolon entra en el mundo 5 sin la Conch Shell y en el 7
        // con el Necklace (de la tienda), y su única ventaja, el agua, ya no hace daño con HollowSwim. Su código y sus
        // medidas se conservan (las pruebas del original la usan).
        public static bool SoloKnight = true;             // activado: lo ha pedido el usuario

        // Original: al encender, el logo de Konami, el título, la historia y las demos en bucle (docs/attract.md). Remake
        // (petición del usuario: "no aporta nada"): un menú principal como el de Hollow Knight (Visuals/HollowMainMenu),
        // con Continuar, Nueva partida, Opciones, Extras y Salir, y un menú de pausa (Esc o Start) desde el que se vuelve a
        // él. Sin el arte del menú importado, el ciclo del original.
        public static bool HollowTitle = true;            // activado: lo ha pedido el usuario

        // Original: se guarda en el santuario de Demeter y, al perder todas las vidas, GAME OVER. Remake (Galious: The
        // Hollow Kingdom, como Hollow Knight): bancos en el castillo, en la entrada de cada mundo y antes de cada jefe
        // (Benches); sentarse (↓) rellena todas las máscaras, guarda y fija dónde se reaparece.
        public static bool HollowBenches = true;          // activado: lo ha pedido el usuario
        // Original: vidas de reserva y GAME OVER. Remake: sin vidas; al morir se vuelve al último banco con todas las
        // máscaras y el geo (monedas) se queda en la sombra (Enemies.Shade) donde se murió; al derrotarla se recupera.
        // Morir otra vez antes pierde el de la sombra anterior.
        public static bool HollowDeath = true;            // activado: lo ha pedido el usuario

        // Original: la experiencia (por enemigo muerto; a 48 rellena la vitalidad). Remake (lo pidió el usuario): el alma de
        // Hollow Knight (Soul): cada espadazo que acierta la llena y concentrarse (F o el botón B del mando, quieto)
        // gasta un tercio del orbe para curar 2 máscaras. El orbe del marcador muestra el alma.
        public static bool HollowSoul = true;             // activado: lo ha pedido el usuario

        // Original: cada objeto hace efecto desde que se recoge. Remake (amuletos de Hollow Knight, lo pidió el usuario): los
        // objetos pasivos de combate (Inventory.CharmCost: coronas, cascos, botas, jarrón, escudos…) son amuletos que
        // cuestan muescas y solo hacen efecto equipados; se cambian sentado en un banco. Muescas: 3 más una por jefe
        // vencido (Inventory.Notches). Al recoger uno se equipa solo si caben sus muescas. Llaves, pases de mundo, el mapa y
        // los objetos de progreso siguen activos siempre.
        public static bool HollowCharms = true;           // activado: lo ha pedido el usuario

        // Original: no hay registro de enemigos. Remake (lo pidió el usuario): el diario del cazador de Hollow Knight
        // (Journal): una entrada por criatura con su retrato, cuántas se han derrotado y, a partir de Journal.NotesAt (los
        // jefes, 1), las notas del cazador. Pestaña Diario del inventario. No cambia nada del juego.
        public static bool HollowJournal = true;          // activado: lo ha pedido el usuario

        // Original: el mapa de cada mundo es un objeto escondido (0x0A) y el menú pinta una cuadrícula de salas. Remake (lo
        // pidió el usuario): como en Hollow Knight, Cornifer (Cornifers) vende el mapa de cada mundo (1–9) por geo, y el
        // mapa del menú está dibujado a mano desde la colisión de cada sala: sin el mapa, un boceto a lápiz de las salas
        // visitadas; con él (comprado o encontrado), el mapa entintado entero, con los bancos y dónde estás.
        public static bool HollowMaps = true;             // activado: lo ha pedido el usuario

        // Original: la espada solo golpea hacia delante y ↑ salta. Remake (lo pidió el usuario): como en Hollow Knight,
        // con ↑ pulsado el espadazo va hacia arriba (de pie o en el aire) y con ↓ en el aire hacia abajo; si el de abajo
        // acierta (enemigo, piedra, escudo o pinchos) el Caballero rebota hacia arriba. Para poder apuntar arriba sin
        // saltar, saltar va con su propio botón (A en el mando, Z en el teclado, Controls) y ↑ solo apunta y sube
        // escaleras. Cajas del golpe: SlashHalf a cada lado del centro y SlashReach de alto.
        public static bool HollowSlashes = true;          // activado: lo ha pedido el usuario

        // Original: la música del chip PSG en todo el juego. Remake (lo pidió el usuario): la de fondo es la de Hollow Knight
        // (Visuals/HollowMusic): la de la zona de cada mapa (castillo → Cruces Olvidados, mundo 1 → Sendero Verde…), la de
        // los santuarios y la de la pelea con cada jefe. Las fanfarrias (objetos, invocación, jefe vencido, muerte, final)
        // siguen con el chip, con la otra bajada mientras suenan.
        public static bool HollowMusic = true;            // activado: lo ha pedido el usuario
        // Teclas de depuración (K, G, F2–F11): solo en compilaciones de desarrollo o lanzando el juego con -depuracion. Antes
        // estaban en el juego normal: K daba llaves, G armas, F4 llevaba al final y F7 quitaba el aspecto de Hollow Knight.
        public static readonly bool DebugKeys = Debug.isDebugBuild ||
            System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-depuracion") >= 0;
        // El ambiente de cada zona como en Hollow Knight (Visuals/HollowAmbience): gradación de color, luz del Caballero,
        // oscuridad con un hueco alrededor del Caballero en los Canales, el Abismo y el Nido (la Vela lo abre, como la
        // linterna) y las partículas de la zona.
        public static bool HollowAmbience = true;         // activado: lo ha pedido el usuario
        public const int SlashHalf = 10, SlashReach = 16;
        public const int BounceRise = 5;                  // entrada de OriginalPhysics.RiseTable desde la que rebota

        // Sensación de Hollow Knight (guía "Hollow Knight por dentro", Knight y Combate). F11 activa o quita las tres a la vez.
        //
        // Original: al acertar un golpe el juego sigue sin pararse. Remake: todo el juego se para HitStopTicks ticks
        // (Hollow Knight: ~0,11 s) para que el golpe pese. No cambia lo que pasa, solo cuándo se ve.
        public static bool HitStop = false;
        public const int HitStopTicks = 3;                  // 0,1 s
        // Original: al salir andando de un borde, 4 ticks quieto en el aire sin poder hacer nada. Remake: en los primeros
        // CoyoteTicks de esos 4, una pulsación de ↑ salta como desde el suelo (Hollow Knight: 0,04 s de "coyote time").
        public static bool CoyoteJump = false;
        public const int CoyoteTicks = 2;
        // Original: ↑ pulsado en el aire no hace nada al aterrizar (hace falta otra pulsación en el suelo). Remake: una
        // pulsación en los últimos JumpBufferTicks ticks en el aire salta al tocar el suelo si ↑ sigue pulsado
        // (Hollow Knight: búfer de salto de 0,04 s).
        public static bool JumpBuffer = false;
        public const int JumpBufferTicks = 2;

        public static bool HollowFeel
        {
            get => HitStop && CoyoteJump && JumpBuffer;
            set => HitStop = CoyoteJump = JumpBuffer = value;
        }
    }
}
