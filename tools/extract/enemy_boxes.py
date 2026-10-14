#!/usr/bin/env python3
"""Cajas de colision de enemigos, jugador, espada y armas (docs/items_enemies.md, «Cajas de colisión»).

Prueba de solape (b03:B96D, la misma para contacto, espada y armas) [C]: dos cajas A y B se tocan si
    A.y0 <= B.y0 + B.h  y  B.y0 <= A.y0 + A.h   (igual en x)
con bordes INCLUSIVOS: el «alto/ancho» de la tabla es la distancia entre el primer y el ultimo pixel, la caja
ocupa h + 1 por w + 1 pixeles. Origen de la caja de un enemigo: (IX+5 + dy, IX+7 + dx); dy y dx con signo.

  boxes()           -> tabla b14:9006, 0x7A cajas [dy, dx, h, w] (indice = IX+11)
  player_boxes()    -> cajas del jugador (contacto/recogida), espada y armas secundarias
  enemy_box_info(t) -> campos por tipo para enemies.json:
      box             indice inicial (5.o byte del descriptor b04:67B6 + 5*tipo)
      boxes           todos los indices que puede tomar el tipo (incluye box)
      boxFrames       [{sprite, box}]: caja segun el sprite IX+0A. Si la IA la recalcula desde una tabla por
                      sprite (b06:B067..B085 o tabla propia) es exacta; si la anima con XOR (b06:B0D6/B0DD) es
                      la pareja nominal (la fase sprite/caja puede cruzarse tras un cambio de estado) [C]
      boxSizeChanges  true si la caja cambia de tamano de forma apreciable con el estado
      boxSpecial      ''|'columnDown'|'columnUpDown'|'row'|'lowerSegment'|'none' (ver docs): caja calculada
      boxNote         estados -> caja, en castellano
"""
from galious_rom import Rom

BOX_TABLE, NBOXES, BOX_GROUP = 0x9006, 0x7A, 4      # b14 se ve en 8000-9FFF con el grupo 4 (b03:B954 -> 420A)
AI = 1                                             # grupo de bancos de la IA (b04-b06)

# Ganchos que la IA deja en la pila: al volver, IX+11 = tabla[(IX+0A - base) / 4] (b06:B08F/B088) [C]
HOOKS = {0x05: (0xB09F, 0xA0, 8), 0x08: (0xB0D0, 0xC4, 6), 0x0C: (0xB0A7, 0xA0, 9), 0x12: (0xB0C8, 0xA0, 8),
         0x13: (0xB0B6, 0xA0, 11), 0x14: (0xB0C1, 0xA0, 7), 0x2C: (0xB0B0, 0xA0, 6), 0x64: (0xB0B0, 0xA0, 6),
         0x28: (0x93CC, 0xC4, 8), 0x31: (0x8C17, 0xC4, 8)}
# Tablas de palabras (sprite, caja) que lee 736F: IX+0A = byte bajo, IX+11 = byte alto [C]
WORDS = {0x0F: [(0x73AA, 8)], 0x18: [(0x6CC4, 6)], 0x54: [(0x6CC4, 6)], 0x55: [(0x6CC4, 6)],
         0x2E: [(0xA0D1, 5)], 0x25: [(0x9A61, 3)]}

# Parejas sprite -> caja deducidas del codigo (escrituras de IX+0A/IX+11 y mascaras XOR) [C]
PAIRS = {
    0x01: [(0xA0, 0x00), (0xA4, 0x13), (0xB4, 0x00), (0xB8, 0x13), (0xA8, 0x00), (0xAC, 0x01), (0xC0, 0x01)],
    0x03: [(0xC4, 0x66), (0xC8, 0x74), (0xCC, 0x77), (0xD0, 0x77)],
    0x07: [(0xA0, 0x26), (0xA4, 0x27)],
    0x0A: [(0xB0, 0x4E), (0xC0, 0x4F), (0xA0, 0x55), (0xA8, 0x55)],
    0x0B: [(0xA0, 0x3F), (0xA4, 0x05), (0xA8, 0x05), (0xAC, 0x3F)],
    0x0E: [(0xA0, 0x67), (0xAC, 0x69), (0xB0, 0x0F)],
    0x11: [(0xA4, 0x58), (0xA8, 0x58), (0xB4, 0x58), (0xB8, 0x58), (0xB0, 0x59), (0xC0, 0x5A), (0xAC, 0x00),
           (0xBC, 0x00)],
    0x15: [(0x08, 0x42), (0x09, 0x43)],
    0x17: [(0xC8, 0x13), (0xCC, 0x09), (0xD0, 0x13), (0xD4, 0x09)],
    0x19: [(0xD0, 0x4A), (0xD4, 0x4B), (0xC8, 0x48), (0xCC, 0x49), (0xC0, 0x46), (0xC4, 0x47)],
    0x1F: [(0xA0, 0x00), (0xA4, 0x13), (0xAC, 0x13), (0xB0, 0x00), (0xA8, 0x00), (0xB4, 0x00)],
    0x21: [(0xA0, 0x58), (0xA4, 0x59)],
    0x24: [(0xA0, 0x00), (0xA4, 0x00), (0xAC, 0x00), (0xB0, 0x00), (0xA8, 0x09), (0xB4, 0x09)],
    0x25: [(0xAC, 0x4E), (0xB8, 0x4F)],
    0x27: [(0xC4, 0x11), (0xC8, 0x00), (0xCC, 0x00)],
    0x29: [(0xC4, 0x0E), (0xC8, 0x68)],
    0x2D: [(0xA0, 0x0C), (0xA4, 0x00), (0xAC, 0x0D), (0xB0, 0x00)],
    0x33: [(0xC4, 0x5F), (0xC8, 0x60)],
    0x36: [(0xC4, 0x12), (0xC8, 0x11)],
    0x37: [(0x0E, 0x3D), (0x0A, 0x3C)],
    0x38: [(0xC4, 0x66), (0xC8, 0x67)],
    0x3E: [(0xC4, 0x0F), (0xC8, 0x0E)],
    0x3F: [(0xA8, 0x05), (0xB4, 0x21)],
    0x47: [(0xC0, 0x5C), (0xC4, 0x5D), (0x0A, 0x3D), (0x0B, 0x5B)],
    0x49: [(0xB4, 0x6A), (0xB8, 0x6B)],
    0x4F: [(0xA4, 0x00), (0xA8, 0x13)],
    0x52: [(0xAC, 0x69), (0xB0, 0x0F)],
    0x53: [(0xA4, 0x56), (0xA8, 0x48)],
    0x58: [(0xA0, 0x6F), (0xA4, 0x0D)],
}
EXTRA = {0x0B: [0x3E, 0x04], 0x21: [0x5A]}          # cajas que no tienen un sprite fijo
SIZE_CHANGES = {0x01, 0x03, 0x0A, 0x0B, 0x0C, 0x0E, 0x0F, 0x11, 0x13, 0x14, 0x18, 0x19, 0x25, 0x27, 0x2E, 0x31,
                0x36, 0x38, 0x41, 0x47, 0x49, 0x54, 0x55, 0x58, 0x60, 0x62, 0x57}
SPECIAL = {0x60: 'columnDown', 0x62: 'columnUpDown', 0x57: 'row', 0x06: 'lowerSegment', 0x5B: 'none'}
NOTES = {
    0x01: 'patrulla 00<->13 cada 8 f (sprite ^4 a la derecha, ^0C a la izquierda); embestida AC/C0 -> 01 (29 de alto, '
          'sube 13 px); A8 -> 00',
    0x03: 'quieta C4 -> 66 (6x14); avanza C8 -> 74, CC/D0 -> 77',
    0x05: 'gancho B080: caja según sprite cada frame',
    0x07: 'anda 26<->27 cada 10 f (sprite ^4, caja ^1); en el salto conserva la caja',
    0x08: 'gancho B085 (base C4): CC -> 0C, D0 -> 50',
    0x0A: 'anda a la derecha B0 -> 4E y a la izquierda C0 -> 4F (24 de ancho); escalera A0/A8 -> 55',
    0x0B: 'nada A0/3F <-> A4/05 (^3A cada 8 f); A8/05 <-> AC/3F; al girar A0 -> 3E (y A4 -> 04)',
    0x0C: 'gancho B067: crece A0 -> 51 (8x6), A4/A8 -> 52, AC -> 53, B0 -> 54; vuelo B4..C0 -> 2A/16',
    0x0E: 'colgado A0 -> 67; se suelta AC -> 69; revolotea AC/69 <-> B0/0F (^66 cada 4-6 f)',
    0x0F: 'crece por etapas (IX+1E 0..3): derecha C0/33, C8/35, D0/36, D8/00; izquierda A0/32, A8/34, B0/36, '
          'B8/00; anima solo el sprite',
    0x11: 'anda A4/B4 -> 58; descansa B0 -> 59 / C0 -> 5A alternando; dispara AC/BC -> 00',
    0x12: 'gancho B07B: emerge y rueda con cajas de 16 (13/16/2A); oculto con IX+2 bit 2',
    0x13: 'gancho B071: anda A0..B0 (2D/2E/1A/2F), se desvanece B4 -> 30 (6x7), vuela BC/C0 -> 15/31',
    0x14: 'gancho B076: emerge A0 -> 38 (4 alto), A4 -> 1E (4), A8 -> 19 (8); pasea AC..B8 -> 16/13/2A/13',
    0x15: 'sprites 08/09 alternando cada 8 f -> 42/43',
    0x17: '13 <-> 09 con la animación (^1A)',
    0x18: 'tabla 6CC4 por IX+1F: A0/1E (4 alto), A8/0B, AC/0B, A4/1F (7), B4/25 (12x14), B0/0B',
    0x19: 'nivel 0 D0/4A <-> 4B (5x5); nivel 1 C8/48 <-> 49; nivel 2 C0/46 <-> 47 (^1)',
    0x1F: 'corre 00 <-> 13 (^13); salto A8/B4 -> 00',
    0x21: '58 <-> 59 a la derecha, 58 <-> 5A a la izquierda (^1 / ^2)',
    0x24: 'anda 00; tras saltar A8/B4 -> 09; al aterrizar 00',
    0x25: 'anda AC -> 4E / B8 -> 4F (24 de ancho); enroscado tabla 9A61 por IX+19: 2 A0/4D, 1 A4/0B, 0 A8/19',
    0x27: 'dormida C4 -> 11 (4x10); despierta C8/CC -> 00',
    0x28: 'tabla 93CC (base C4) según la dirección: 62, 63, 0D, 64, 0C, 65',
    0x29: '0E <-> 68 cada 8 f (^66)',
    0x2C: 'gancho B06C: A0/AC -> 0B, A4/A8 -> 0C, B0/B4 -> 0D',
    0x2D: 'derecha A0/0C <-> A4/00, izquierda AC/0D <-> B0/00 (^0C/^0D cada 4 f)',
    0x2E: 'tabla A0D1 por IX+1E: C4/26 (6 alto), C8/27 (8), CC/28 (12), D0/29 y D4/29 (15) y vuelta',
    0x31: 'tabla 8C17 (base C4). Suelo: C8/73, CC/74, D0/75; techo: D8/77, DC/74, E0/75. C4/D4 no dan contacto '
          '(IX+2 sin bit 6); sprite 00 oculto',
    0x33: 'derecha C4 -> 5F, izquierda C8 -> 60',
    0x36: '12 <-> 11 cada 12 f (^3)',
    0x37: '3D <-> 3C (^1)',
    0x38: 'quieta C4 -> 66; cae C8 -> 67',
    0x3E: '0F <-> 0E (^1)',
    0x3F: 'A8 -> 05; con E624 B4 -> 21',
    0x41: 'tabla 8DC5 por dirección: horizontal A4 -> 3A, vertical A8 -> 3B, diagonal C8/BC -> 75',
    0x47: 'proyectil C0/5C <-> C4/5D; explosión 0A -> 3D (20x20, x-8 y-8) y 0B -> 5B (32x32)',
    0x49: '6A <-> 6B (^1)',
    0x4F: 'A4 -> 00; A8 -> 13',
    0x52: '69 <-> 0F (^66)',
    0x53: '56 <-> 48 (^1E)',
    0x54: 'como 0x18 (tabla 6CC4)',
    0x55: 'como 0x18 (tabla 6CC4)',
    0x58: '6F (8x8) <-> 0D (14x14) (^62)',
    0x06: 'caja 10 con origen en IX+12/IX+13 (segmento inferior) salvo en el estado 4',
    0x57: 'sin tabla: dy 0, dx 0, alto 8, ancho 8*IX+1F (b03:B926)',
    0x60: 'sin tabla: dy 0, dx 0, alto 8*((IX+9 & 7) - 1), ancho 7 (b03:B916)',
    0x62: 'sin tabla: dx 0, alto 8*n, ancho 7 con n = IX+9 & 7; dy = -8*n si IX+15 bit 0, si no 0 (b03:B933)',
    0x5B: 'explosión: IX+11 guarda el tipo muerto, no una caja; IX+2 = 80, sin contacto ni golpes',
}


def boxes(rom=None):
    rom = rom or Rom()
    s8 = lambda v: v - 256 if v >= 128 else v
    out = []
    for i in range(NBOXES):
        dy, dx, h, w = rom.block(BOX_TABLE + 4 * i, 4, BOX_GROUP)
        out.append([s8(dy), s8(dx), h, w])
    return out


def player_boxes():
    """Relativas al jugador: y = pies (E505), x = centro (E507). Mismo criterio de bordes inclusivos."""
    box = lambda dy, dx, h, w: {'dy': dy, 'dx': dx, 'h': h, 'w': w}
    return {
        # b03:B8A0: contacto con enemigos y recogida de objetos; no depende de la postura (escalera, salto)
        'playerContactBox': box(-14, -6, 12, 12),
        # b03:B8FB: espada, con E51B != 0 (un golpe por espadazo). E503 = 0 derecha, != 0 izquierda
        'swordBoxRight': box(-7, 8, 3, 10),
        'swordBoxLeft': box(-7, -18, 3, 10),
        # b03:B8B2: armas en E540 + 0x20*n (tipo 1-5), relativas a (IY+5, IY+7). gridSnap: origen
        # ((y & 0xF8) + dy, (x & 0xF8) + dx). exploded: mina con IY+2 bit 2
        'weaponBoxes': [
            dict(weapon=1, gridSnap=True, exploded=False, **box(2, 2, 3, 12)),
            dict(weapon=2, gridSnap=True, exploded=False, **box(2, 2, 3, 12)),
            dict(weapon=3, gridSnap=False, exploded=False, **box(0, 0, 8, 8)),
            dict(weapon=4, gridSnap=False, exploded=False, **box(0, 0, 4, 8)),
            dict(weapon=5, gridSnap=False, exploded=False, **box(0, 2, 4, 12)),
            dict(weapon=5, gridSnap=False, exploded=True, **box(-16, 0, 32, 32)),
        ],
    }


def enemy_box_info(t, rom=None):
    rom = rom or Rom()
    d4 = rom.block(0x67B6 + 5 * t, 5, AI)[4]
    frames = list(PAIRS.get(t, []))
    if t in HOOKS:
        table, base, n = HOOKS[t]
        frames = [(base + 4 * i, rom.byte(table + i, AI)) for i in range(n)]
    for table, n in WORDS.get(t, []):
        frames += [(rom.byte(table + 2 * i, AI), rom.byte(table + 2 * i + 1, AI)) for i in range(n)]
    if t == 0x41:
        frames = [(rom.byte(0x8DC5 + 8 * i, AI), rom.byte(0x8DC5 + 8 * i + 7, AI)) for i in range(8)]
    seen, uniq = set(), []
    for s, b in frames:
        if (s, b) not in seen:
            seen.add((s, b))
            uniq.append({'sprite': s, 'box': b})
    special = SPECIAL.get(t, '')
    used = sorted({d4, *(f['box'] for f in uniq), *EXTRA.get(t, [])}) if t != 0x5B else []
    return {'box': d4, 'boxes': used, 'boxFrames': uniq, 'boxSizeChanges': t in SIZE_CHANGES,
            'boxSpecial': special, 'boxNote': NOTES.get(t, 'caja fija' if not uniq else '')}


if __name__ == '__main__':
    r = Rom()
    for i, b in enumerate(boxes(r)):
        print(f'{i:02X} {b}')
    for t in range(1, 108):
        print(f'{t:02X}', enemy_box_info(t, r))
