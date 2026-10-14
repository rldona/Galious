# 6. Jefes

Fuentes: [boss_ai.md](../boss_ai.md) (fichas leídas del código), [state_machine.md](../state_machine.md) (invocación, recompensa).

## Ciclo de un jefe

1. **Sala del jefe:** suena la música 0x8F, "Devil's Room" [C]. El jefe no está.
2. **Invocación:** se escribe la palabra del mundo (leída antes en la lápida con la lupa).
   - Estado 15, jingle 0x81 [C][O].
   - Aparece el jefe con la vida de su tabla (a la mitad con el Holy Water) [C].
3. **Combate:** música 0x90, "Great Demons". Según la vida restante, la música sube de tempo en dos escalones, al cuarto y al octavo de la vida (E1C5) [C][G].
4. **Derrota:** estado 19 y jingle 0x84. Cae la Great Key, cuya fanfarria es el estado 20 [C][O].
5. Los objetos del mundo cambian las reglas del combate: Holy Water, vida a la mitad; Cloak, mitad de daño recibido [G]; Magic Rod, munición infinita [G].

## Daño

- **Armas:** solo las de la máscara de cada jefe (b03:BD0A). Bits: 0 espada, 1 mina, 2 fuego rodante, 3 fuego, 4 flecha de cerámica, 5 flecha.
- **Zona vulnerable:** algunos jefes además solo reciben daño en ciertas partes o momentos.
- **Cuánto:** la mina resta 2 y el resto 1. Si el arma no vale, suena 0x1D [C].

## Los diez jefes

Tiempos en ticks (~30/s).

| Mundo | Jefe | Vida | Patrón | Ataque | Le hace daño |
|---|---|---|---|---|---|
| 1 | **Bony Dragon** | 24 | pasea poco (0,5 px/t); salta cada 80–207 t y descansa 60 t al caer | 3 huesos en parábola hacia la izquierda, cada 16–47 t y en lo alto del salto | flecha, espada |
| 2 | **Sea Anemone** | 40 | fija; cerrada 80–207 t, abierta 80–207 t | esporas en surtidor cada 8 t mientras está abierta | flecha, espada; **solo abierta** |
| 3 | **Elusive Dragon** | 64 | asoma por la izquierda o la derecha a 5 alturas; luego invisible e invulnerable, y reaparece | llamarada tras 30 t con la boca abierta | fuego, espada |
| 4 | **Jump Slime** | 112 | se arrastra a 0,5 px/t; si estás cerca salta (vx 1,5, vy −4,5 o −6, gravedad 0,25) y reposa 32 t | aplastar | **mina** (resta 2), espada |
| 5 | **Donra** | 72 | vuela por una caja; la cabeza sigue tu altura | llamarada de ~64 px desde la cabeza | flecha, espada; **solo la cabeza** |
| 6 | **Arlyane** | 56 | camina a 1 px/t y te persigue si estás a su altura o debajo | 0–3 ráfagas de 6 disparos en abanico | flecha, fuego, espada |
| 7 | **Big Lip** | 64 | ida y vuelta a 1 px/t; cada ~350 t, o al tocarle, se para 96 t y saca la lengua | disparo en 8 direcciones cada 16 t mientras se mueve | flecha de cerámica, espada; **solo boca o lengua** |
| 8 | **Monster** | 80 | anda a 0,5 px/t, cae por los bordes y sube a tu piso; se para 48 t cada ~128 t | 5 bolas diagonales que rebotan en las paredes | **fuego rodante**, espada |
| 9 | **King Dragon** | 80 | 48 t paseando y 48 t parado; retrocede al recibir un golpe | llamarada cada 16 t parado; al empezar crea **4 nubes de tormenta** | flecha de cerámica, espada |
| 10 | **Galious** | 248 | fijo; cada ~250 t se teletransporta a una de 8 posiciones (invulnerable mientras) | 8 bolas en órbita que lanza en bumerán | todas, **pero solo con la Cross** |

Consejos de la guía [G], coherentes con la tabla: flechas al Bony Dragon y a Donra (a la cabeza); fuego al Elusive Dragon; 3 minas cada vez que salta el Jump Slime; ponerse encima del Monster con fuego rodante; flechas de cerámica a la lengua de Big Lip y al King Dragon desde la escalera; Galious: anular las bolas y usar fuego rodante.

## Final

Tras vencer a Galious, Pampas está en una de 10 piedras de la sala siguiente, al azar; las demás sueltan ectoplasma. Luego viene la cinemática final, con las músicas 0x92 y 0x93 [G][C].

## Notas para el remake

- **Una regla por jefe:** cada jefe enseña una (arma concreta, zona vulnerable, momento vulnerable). Mantenerlas y hacerlas legibles: la zona vulnerable debe parpadear o brillar al abrirse.
- **Vida:** la del original es baja en golpes (24–112; 248 para Galious) porque cada golpe resta 1. En un combate al estilo HK conviene reescalar vida y daño juntos, manteniendo la proporción y la ventaja del Holy Water.
- **Cross:** sin ella, Galious no recibe daño y ni siquiera suena el aviso de arma inútil. En el remake, dar una señal clara de que falta la Cross.
