#!/usr/bin/env python3
"""Ascensores del castillo para el importador de Unity: extracted/unity/lifts.json (docs/entities.md, "Ascensores").

  lifts.py   -> extracted/unity/lifts.json   (lo llama unity_bundle.py)

Dos clases, las dos solo en el castillo (E041 = 1), de 32 x 16 px (dos bloques):
  - 6 ascensores verticales (E4A0, 16 bytes cada uno) que recorren una columna de salas y siguen moviendose aunque
    el jugador este en otra sala o en un mundo. El de la sala actual se copia en EC10.
  - 15 ascensores horizontales de una sola sala (EC20), que vuelven a su sitio cada vez que se entra en la sala.

Fuentes [C] (rutinas en b01 con el banco 9 en A000; medidas [O] con tools/openmsx/lift_measure.py):
  b01:63C2   inicio de partida: 6 verticales desde b09:BB54 (sala) y b09:BB24 (8 bytes: y, x, velocidad 8.8,
             sala/y de arriba, sala/y de abajo); indice 0 en su lista de salas, bajando, IX+0 = 6..1
  b00:5FA8   si la partida empieza en la sala 146 (demo 0), el ascensor 6 se pone ahi subiendo (b00:5FBA)
  b01:63EA   al entrar en una sala del castillo: horizontal de b09:BA53 (15 x {sala, puntero}) a 8 bytes como arriba
  b01:643C   cada tick: mueve los 6 verticales (647A); y < 0x28 subiendo -> sala anterior de la lista, y = 0xB9;
             y >= 0xC0 bajando -> sala siguiente, y = 0x28; listas en b09:BA80[id - 1]. Copia el de la sala a EC10
  b01:64EE   y += velocidad; da la vuelta subiendo en la sala de arriba con y + 1 < y_arriba y bajando en la de abajo
             con y >= y_abajo
  b01:6531   cada tick: horizontal. Da la vuelta con la x de ANTES de moverse: a la derecha si x_antes >= maxX, a la
             izquierda si x_antes <= minX (medido: 174 176 178 176 ... 50 48 46 48)
  b03:A916   lleva al jugador: y = y del ascensor; en el horizontal tambien x += lo que se ha movido
  b03:B0C2   colision (despues de mover al jugador), ver 'box'
  b03:A8CE   "sobre el ascensor" (bit 0 de EC12 o EC22) cuenta como suelo al estar de pie, al saltar y al caer
  b03:A7F8   salida de la sala: si el vertical cambia de sala con el jugador encima, el jugador va con el (arriba
             y = 0xB9, abajo y = 0x28). Si se salta desde un vertical (E52B/E52D) y la y baja de 0x28, sale por arriba
             y ese ascensor sube tambien de sala en el tick siguiente

Campos de lifts.json:
  shafts[]   {id, rooms[] (de arriba abajo), startRoom, startIndex, y, x, speed (1/256 px por tick, + = bajando),
              topRoom, topY, bottomRoom, bottomY}
  slides[]   {room, y, x, speed (+ = derecha), minX, maxX}
  demo0      {id, room, index, y, speed}: estado del ascensor 6 al empezar en la sala 146
  box        caja y reglas de colision (px relativos a x, y del ascensor = esquina de arriba a la izquierda; x, y del
             jugador = centro de los pies)
"""
import json
from galious_rom import Rom, ROOT

G = 2   # grupo de bancos con b09 en A000


def s16(v):
    return v - 0x10000 if v & 0x8000 else v


def rooms_of(rom, lift_id, bottom_room):
    p = rom.word(0xBA80 + 2 * (lift_id - 1), G)
    rooms = []
    while True:
        r = rom.byte(p + len(rooms), G)
        rooms.append(r)
        if r == bottom_room:
            return rooms


def build(rom):
    shafts = []
    for i in range(6):
        lid = 6 - i
        b = rom.block(0xBB24 + 8 * i, 8, G)
        room = rom.byte(0xBB54 + i, G)
        rooms = rooms_of(rom, lid, b[6])
        assert rooms[0] == room == b[4]
        shafts.append({'id': lid, 'rooms': rooms, 'startRoom': room, 'startIndex': 0, 'y': b[0], 'x': b[1],
                       'speed': s16(b[2] | b[3] << 8), 'topRoom': b[4], 'topY': b[5], 'bottomRoom': b[6],
                       'bottomY': b[7]})
    slides = []
    for i in range(15):
        room = rom.byte(0xBA53 + 3 * i, G)
        b = rom.block(rom.word(0xBA54 + 3 * i, G), 8, G)
        slides.append({'room': room, 'y': b[0], 'x': b[1], 'speed': s16(b[2] | b[3] << 8), 'minX': b[5], 'maxX': b[7]})
    p = rom.block(0x5FBA, 9)   # b00:5FBA -> E4A1..E4A9 del ascensor 6
    demo0 = {'id': 6, 'room': p[0], 'up': bool(p[1] & 2), 'index': p[2], 'y': p[4], 'speed': s16(p[7] | p[8] << 8)}
    return {
        'shafts': shafts, 'slides': slides, 'demo0': demo0,
        'box': {
            'width': 32, 'height': 16,
            'standDxMin': -2, 'standDxMax': 34, 'standDyMin': 0, 'standDyMax': 7,
            'sideDyMin': 1, 'sideDyMax': 31, 'pushLeftX': -8, 'pushLeftFrom': -8, 'pushLeftTo': -5,
            'pushRightX': 40, 'pushRightFrom': 37, 'pushRightTo': 40,
            'headDyMin': 24, 'headDyMax': 31, 'headPushY': 32,
            'leaveCooldownTicks': 7,
            'roomTopY': 0x28, 'roomBottomY': 0xC0, 'enterFromBelowY': 0xB9, 'enterFromAboveY': 0x28,
            'notes': 'b03:B0C2, por cada ascensor de la sala, despues de mover al jugador. Sin efecto durante la '
                     'espera tras bajarse (7 ticks). Con los pies 1..31 px por debajo de la cara de arriba: '
                     'x - lx en -8..-5 -> x = lx - 8; en 37..40 -> x = lx + 40. Con x - lx en -2..34: pies 0..7 px '
                     'por debajo -> encima (si ya lo estaba, o si no hay suelo de tiles bajo los pies); 24..31 px '
                     'por debajo y sin suelo de tiles -> y = ly + 32 (lo empuja por debajo). Al dejar de estar encima '
                     'empieza la espera.',
        },
    }


def main():
    out = ROOT / 'extracted' / 'unity' / 'lifts.json'
    out.parent.mkdir(parents=True, exist_ok=True)
    data = build(Rom())
    out.write_text(json.dumps(data, indent=1, ensure_ascii=False))
    print(f"ascensores: {len(data['shafts'])} verticales, {len(data['slides'])} horizontales -> {out}")


if __name__ == '__main__':
    main()
