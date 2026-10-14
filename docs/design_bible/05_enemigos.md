# 5. Enemigos

Fuentes: [enemy_ai.md](../enemy_ai.md) (ficha por tipo, leída del código), [items_enemies.md](../items_enemies.md) (vida, botín y medidas en el emulador), `extracted/entities/enemies.json`.

## Reglas generales [C]

- **Huecos:** hasta 16 enemigos a la vez (E800 + 0x20·n), con un límite por sala (E635).
- **Generadores:** los enemigos nacen de los generadores de la sala (1134 en total). Algunas salas tienen además **generadores periódicos** que crean un enemigo cada cierto tiempo (b04:61A8), y el Carpet apaga los del mundo 8.
- **Cada tick** (~30/s), cada enemigo ejecuta su rutina de IA y después se le aplica su velocidad. Si sale de la pantalla, desaparece.
- **Descriptor:** vida (FE/FF = invulnerable) y caja de colisión inicial. El botín depende del tipo (ver [items_enemies.md](../items_enemies.md), «Muerte y botín»).
- **Enemigos marcados:** 23 tipos cuentan para los **eventos de sala** (abrir muros, sacar escaleras u objetos al limpiar la sala).
- **Con la Bible activa** no se ejecuta su IA (quedan congelados).

## Arquetipos

Velocidades en px/tick; tiempos en ticks (~30/s).

### Patrulleros (andan por su plataforma, giran en bordes y paredes)

| Tipo | Descripción | Rasgos |
|---|---|---|
| 0x25 | esqueleto | 1,125 px/t; salta entre plataformas; al recibir golpes se enrosca y recupera la vida |
| 0x07 | oruga saltarina | 0,5 px/t; salta huecos de ~48 px |
| 0x18 | rodador | 0,5; solo gira en ciertos fotogramas |
| 0x43 | patrullero ancho | 2 px/t; rayo vertical si estás a ≤32 px a su derecha |
| 0x2C + 0x64 | oruga de dos piezas | se mueven por turnos; si la cabeza muere, la cola acelera |
| 0x0A | caballero | patrulla y busca tu piso usando saltos y escaleras |
| 0x1F | corredor saltarín | 3,6 px/t; salta huecos y cambia de plataforma |
| 0x24 | saltador | salta 1–2 niveles hacia tu piso |

### Voladores y deambuladores

| Tipo | Descripción | Rasgos |
|---|---|---|
| 0x0E | murciélago | colgado 80 t → se descuelga → revolotea 96–127 t → busca techo |
| 0x28 | insecto (ectoplasma) | 3 px/t en 16 direcciones, rebota |
| 0x29 | burbuja | invulnerable, en línea recta, rebota |
| 0x49 | mosca | al golpearla se divide en 2 (0x52) y después en 4 (0x53) |
| 0x4C | errante | suelta esporas cada 16 t |
| 0x19 | murciélagos que se fusionan | por parejas: pequeño → mediano → grande |
| 0x26 | fantasma orbitante | dispara apuntando |
| 0x27 | ave | ataca en picado (~4 px/t) cuando entras en su zona |

### Perseguidores y embestidores

| Tipo | Descripción | Rasgos |
|---|---|---|
| 0x01 | bicho | aviso de 30 t y embestida hasta tu x |
| 0x10 | acechador | quieto hasta que entras en su piso; embiste y vuelve |
| 0x16 | aparición | embiste en línea recta y se teletransporta |
| 0x20 | embestidor invulnerable | lento; acelera si estás delante a <80 px |
| 0x40 | espíritu | persigue a 0,5 px/t atravesando paredes; dispara al reanudar |
| 0x34 | bloque vertical | más rápido cuanto más cerca estás |
| 0x2F | bloque imitador | salta cuando saltas tú |

### Tiradores

| Tipo | Descripción | Rasgos |
|---|---|---|
| 0x05 | serpiente | dispara cada 120 t; acecha si estás en su piso |
| 0x11 | erizo | dispara púas si \|dx\| < 65 |
| 0x17 | centinela | se para y dispara si estás en su nivel |
| 0x2D | soldado | patrulla, salta y dispara en tu fila |
| 0x22 | escupefuego | ráfagas de fuego |
| 0x13 / 0x0C | espectro u hombre-llama | anda disparando, vuela y vuelve a posarse |
| 0x3C | hechicero | se teletransporta y dispara apuntando |
| 0x42 | monstruo del agua | se oculta si estás en el agua |
| 0x69 | cabezas en formación | disparan por turnos |
| 0x32 / 0x51 y 0x3A / 0x50 | estatuas y bocas de pared | disparan si estás en su franja; invulnerables |

### Emboscadas y trampas

| Tipo | Descripción | Rasgos |
|---|---|---|
| 0x12 | armadillo | oculto 4,5 s, emerge en 3 s y rueda [O] |
| 0x14 | criatura del suelo | oculta 4–5 s, emerge, pasea ~5 s y se hunde [O] |
| 0x2A | Uh | se teletransporta cada 16 t a otra plataforma [O] |
| 0x2E | brote | intermitente |
| 0x31 | vapores o pinchos | emergen del suelo o del techo; invulnerables |
| 0x23 | estatua dormida | despierta si pasas por sus dos lados |
| 0x04 | pistón de tiles | sale y entra del techo |
| 0x36 | orbes | saltan en cadena |

### Especiales

| Tipo | Descripción |
|---|---|
| 0x4E | polvo de hada: si esperas en el punto exacto, forma un **hada** (0x4F) que cura (ver [Mecánicas de sala](07_salas.md)) |

## Notas para el remake

- **Lectura de cada enemigo:** cada arquetipo tiene una señal clara (aviso antes de embestir, fase oculta, franja de disparo). En el remake, hacer esas señales visibles con animación de anticipación.
- **Velocidades:** las del original están en px/tick con salas de 256 px. Convertir con las unidades del capítulo 11.
- **Fallos del original** que conviene **no** copiar:
  - la torreta 0x32 dispara hacia el lado contrario al jugador [I];
  - el 0x43 ignora el azar al elegir dirección;
  - el 0x12 elige sentido comparando tu Y con su X.

  Los tres están en [enemy_ai.md](../enemy_ai.md).
