using System.Collections.Generic;

namespace MazeOfKnight.Game
{
    // Nombre, descripción y uso de cada objeto en castellano, para el inventario, los amuletos y las láminas de tutorial
    // (Galious: The Hollow Kingdom). Los efectos son los medidos (docs/items_enemies.md, "Qué hace cada objeto"), contados
    // para el Caballero (RemakeRules.SoloKnight); los amuletos (RemakeRules.HollowCharms) lo dicen al final.
    public static class ItemTexts
    {
        public struct Entry
        {
            public string Name, Text, Use;
        }

        static Entry E(string name, string text, string use = null) => new Entry { Name = name, Text = text, Use = use };

        static readonly Dictionary<int, Entry> all = new Dictionary<int, Entry>
        {
            [0x01] = E("Flecha", "Una flecha sencilla que vuela recta hasta chocar. Cada disparo gasta una flecha del carcaj.",
                "Elígela en el inventario y dispara con M (botón Y)."),
            [0x02] = E("Flecha de cerámica", "Más lenta que la sencilla, pero atraviesa a los enemigos y a los jefes y sigue volando. Gasta una flecha.",
                "Elígela en el inventario y dispara con M (botón Y)."),
            [0x03] = E("Fuego rodante", "Una llama que rueda por la plataforma y le da la vuelta por los lados y por debajo. Gasta una flecha.",
                "Elígelo en el inventario y lánzalo con M (botón Y)."),
            [0x04] = E("Fuego", "Una llama que corre por el suelo y cae por los bordes hasta el siguiente. Gasta una flecha.",
                "Elígelo en el inventario y lánzalo con M (botón Y)."),
            [0x05] = E("Mina", "Se deja en el suelo y estalla al tocarla un enemigo, con el doble de daño que la espada. Caben dos a la vez.",
                "Elígela en el inventario y ponla con M (botón Y), de pie en el suelo."),
            [0x06] = E("Lupa", "Deja leer lo que está escrito en las lápidas de los jefes. No gasta flechas.",
                "Elígela en el inventario y úsala con M (botón Y) delante de una lápida."),
            [0x07] = E("Agua bendita", "Debilita al jefe de este mundo: aparece con la mitad de vida. Solo vale en el mundo donde se encuentra."),
            [0x08] = E("Capa", "Protege del cuerpo del jefe de este mundo: su contacto hace la mitad de daño. Solo vale en el mundo donde se encuentra."),
            [0x09] = E("Vara mágica", "Durante la pelea con el jefe de este mundo, las armas no gastan flechas (hace falta tener al menos una). Solo vale en el mundo donde se encuentra."),
            [0x0A] = E("Mapa", "El mapa de este mundo, con la sala donde estás. Solo vale en el mundo donde se encuentra.",
                "Míralo en la pestaña Mapa del inventario."),
            [0x0B] = E("Cruz", "Sin ella, los golpes a Galious no le hacen nada. Solo vale en el mundo donde se encuentra."),
            [0x0C] = E("Gran Llave", "Abre la puerta del mundo siguiente. Al cogerla, la vitalidad máxima crece y se cura entera. Se gasta al abrir la puerta."),
            [0x0D] = E("Collar", "Un sello antiguo. Sin él no se cruza la puerta del mundo 7."),
            [0x0E] = E("Corona", "Los Uh del mundo 2 no te hacen nada al tocarte."),
            [0x0F] = E("Yelmo", "Los ectoplasmas no te hacen nada al tocarte."),
            [0x10] = E("Remo", "En el lago del mundo 5 se puede andar sobre el agua."),
            [0x11] = E("Botas", "Andar y subir escaleras es una cuarta parte más rápido."),
            [0x12] = E("Muñeca", "Hace aparecer el puente sobre el agua en el mundo 2."),
            [0x13] = E("Hoi", "En el mundo 8, izquierda y derecha dejan de ir al revés."),
            [0x14] = E("Campana", "En el castillo, suena cerca de la puerta del próximo mundo que se puede abrir."),
            [0x15] = E("Anillo del ángel", "Lleva al santuario de Demeter, en el castillo. Desde un mundo se pierden sus objetos (agua bendita, capa, vara, mapa y cruz).",
                "En el inventario, elígelo y pulsa Aceptar. No vale durante la pelea con un jefe."),
            [0x16] = E("Vela", "Ilumina la sala oscura del mundo 5: sin ella sus muros no se ven, aunque estén."),
            [0x17] = E("Armadura", "El bambú no te hace nada al tocarte."),
            [0x18] = E("Alfombra", "En el mundo 8 la lava se vuelve piedra, y las bolas de fuego de las salas de lava dejan de salir."),
            [0x19] = E("Casco de sombrero", "El daño de enemigos y proyectiles se queda en la mitad (1 sigue siendo 1). No protege del cuerpo de los jefes."),
            [0x1A] = E("Lámpara", "Con el mapa, marca además la sala del jefe del mundo."),
            [0x1B] = E("Jarrón", "Cada golpe que acierta da el doble de alma."),
            [0x1C] = E("Colgante", "El de Aphrodite, para romper las paredes que suenan. El Caballero ya las rompe sin él."),
            [0x1D] = E("Pendientes", "Hasta tres flechas a la vez en vez de dos."),
            [0x1E] = E("Pulsera", "Hasta tres minas a la vez en vez de dos."),
            [0x1F] = E("Anillo", "El de Aphrodite, para romper piedras de pocos golpes. El Caballero ya lo hace sin él."),
            [0x20] = E("Biblia", "Congela a los enemigos, sus generadores y el jefe durante 5 segundos. Sirve 16 veces en toda la partida. Tocarlos sigue haciendo daño.",
                "Úsala con Ctrl."),
            [0x21] = E("Arpa", "El primer espadazo en cada sala rompe todas sus piedras, estén donde estén."),
            [0x22] = E("Triángulo", "Las llamas y los vapores no te hacen nada al tocarte."),
            [0x23] = E("Concha", "La de Aphrodite, para cruzar la puerta del mundo 5. El Caballero entra sin ella."),
            [0x24] = E("Jarra", "Las bolas de fuego no te hacen nada al tocarte."),
            [0x25] = E("Sable", "Un punto más de daño con la espada, las flechas y los fuegos, y dos con las minas. No cuenta contra los jefes."),
            [0x26] = E("Daga", "Escribir UMBRELLA con murciélagos en la sala los mata a todos.",
                "Escribe la palabra con el teclado en cualquier momento."),
            [0x27] = E("Ala", "En el castillo, lleva a la puerta de cualquier mundo que ya se haya abierto.",
                "En el inventario, elígela, pulsa Aceptar y escoge el mundo (o pulsa 1–9)."),
            [0x28] = E("Escudo de cobre", "Mirando de frente y sin dar un espadazo, para los proyectiles pequeños."),
            [0x29] = E("Pan y agua", "Los de Aphrodite, para cruzar las puertas giratorias. El Caballero ya las cruza sin ellos."),
            [0x2A] = E("Sal", "Abre la sala de la Muerte en el castillo."),
            [0x2B] = E("Escudo de plata", "Mirando de frente y sin dar un espadazo, para casi todos los proyectiles de los enemigos."),
            [0x2C] = E("Escudo de oro", "Mirando de frente y sin dar un espadazo, para también los proyectiles de los jefes."),
            [0x2D] = E("Flechas", "Munición de todas las armas."),
            [0x2E] = E("Geo", "La moneda del reino. Se gasta en los santuarios y se pierde en la sombra al morir."),
            [0x2F] = E("Llaves", "Cada una abre una de las puertas pequeñas con cerradura."),
        };

        public static Entry Get(int type) => all.TryGetValue(type, out var e) ? e : E($"Objeto 0x{type:X2}", "");

        // El texto para el panel o la lámina, con la nota de los amuletos.
        public static string Describe(int type)
        {
            var e = Get(type);
            if (!Inventory.IsCharm(type)) return e.Text;
            return e.Text + "\n\nAmuleto: solo hace efecto equipado. Se cambia sentado en un banco.";
        }
    }
}
