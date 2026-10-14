#!/usr/bin/env python3
"""Objetos e inventario (menu F1) para el importador de Unity: extracted/unity/items.json.

Comportamiento documentado en docs/items_enemies.md, "Inventario (menu F1)" y "Que hace cada objeto"
(codigo b00:44D5/47CF, b01:65A8-6919, b01:6126-6214, b02:9A25-9AE4, b03:B701-B7D2 y los lectores de cada flag).
Aqui se leen de la ROM las constantes (con comprobacion de opcodes: si falla, la ROM no es la esperada) y se
arma el JSON. Lo que no sale de la ROM (nombres, tiendas) va a mano, citando el documento.

Unidades: px y ticks de la logica (1 tick = 2 frames, 30 ticks/s). Pantalla MSX de 256 x 192 px; el marcador
ocupa las filas de tiles 0-3 y la sala (o el menu) las 4-23, asi que y_pantalla = 8 * (fila_bufer + 4).

items[] (uno por tipo 0x01-0x2F; JsonUtility: todos los objetos tienen los mismos campos, lo que no aplica
va a su valor por defecto):
  type, hex, name           tipo del objeto (E06F + type es su flag) y nombre (guia de GameFAQs / manual)
  category                  'weapon' | 'worldItem' | 'progression' | 'upgrade' | 'immunity' | 'utility'
                            | 'shield' | 'resource'
  flag                      direccion de la flag de posesion ("E0xx"); 0x2B/0x2C ademas suben E097 (nivel)
  perCharacter              false en todos: las flags son unicas para los dos personajes. Lo que depende del
                            personaje esta en effect.character
  perWorld, worldBit        objetos de mundo (0x07-0x0A): uno por mundo, guardado en el bit worldBit de
                            E061 + E041 (castillo E041 = 1, mundo k = k + 1); al entrar en el mundo se copia a
                            la flag y al salir de el (o con el Angel Ring) la flag se borra (b00:461F / 4730)
  consumable                la flag se borra al usarse (Great Key: al abrirse una puerta, b02:99FB)
  menu{area, slot, x, y, w, h}   casilla en el menu F1, en px de pantalla. area: 'items' (rejilla 10 x 3,
                            tipos 0x0D-0x2A), 'world' (fila de 6, tipos 0x07-0x0C), 'weapons' (recuadro de
                            armas, tipos 1-6) o 'none' (no se dibuja: 0x2B/0x2C se ven en la casilla de 0x28,
                            recursos 0x2D-0x2F en el marcador). slot = indice dentro del area
  effect{...}               ver EFFECT abajo
  confidence                '[C]' codigo, '[O]' observado, '[O*]' observado forzando la flag, '[G]' solo la guia
  code                      direcciones que lo implementan
  rooms                     donde aparece como objeto de sala (extracted/entities/items.json)
  shops[]                   {map, room, god, price} donde se compra (docs/design_bible/08_santuarios.md)

EFFECT: effect.kind decide que campos se usan:
  subWeapon       weaponId (1-6): fisica, coste y limites en extracted/unity/weapons.json
  bossHpDivide    divisor: vida del jefe = floor(vida / divisor) al crearse el jefe (bossHp: vida por jefe)
  bossContactDamageDivide   divisor: dano por contacto con el cuerpo del jefe = floor(dano / divisor)
                  (bossContactDamage: dano por jefe, indice = numero de jefe 1-11)
  freeAmmoInBossFight       durante el combate con el jefe (E0D2 != 0) las armas secundarias no gastan
                  flechas; hace falta tener al menos 1 para disparar
  worldMap        el menu dibuja el mapa del mundo (worldMaps[]) y la posicion del jugador (sprite rojo)
  bossMarker      con el Map, ademas, la sala del jefe (sprite azul)
  canDamageBoss   bossId: sin el objeto, los golpes a ese jefe no cuentan (ni suenan)
  worldKey        abre la puerta del mundo siguiente; maxVitalityStep / maxVitalityLimit: el personaje
                  activo gana maxVitalityStep de vitalidad maxima si la que tiene es < maxVitalityLimit, y se
                  cura entera; se consume al abrirse la puerta
  worldEntry      world, character: ese personaje no cruza la puerta del mundo sin el objeto; en ese mundo,
                  ademas, el menu no deja cambiar de personaje sin el
  shrineEntry     godType: entrar en la sala de dios de ese tipo (3 = la Muerte, castillo 87)
  contactImmunity enemyTypes: el contacto con esos tipos no hace nada (ni dano ni retroceso)
  walkOnWater     world, rooms, tile: en esas salas el tile de superficie del agua es suelo
  speedMultiplier walkSpeed/walkSpeedWith, ladderSpeed/ladderSpeedWith (px/tick); andar no se acelera en
                  el salto (E500 = 1)
  bridge          world, room: aparece el puente (entities.md)
  noReversedControls  world: sin el objeto, izquierda y derecha van al reves en todo ese mundo
  doorChime       sound, roomsByWorld: en el castillo, al entrar en una de las salas de la lista del mundo
                  con puerta desbloqueada y sin abrir de numero mas alto, suena 'sound'
  warpHome        menu + Return: vuelves a (map 'castle', room). Desde un mundo, waitTicks de espera
  lightDarkRoom   world, room: sin el objeto la sala se dibuja con muros invisibles (misma colision)
  lavaToStone     world: las salas con lava (E619) tienen las filas 12-19 de muro F0; ademas no se arma el
                  generador de bolas de fuego (E653) en ninguna sala
  damageHalve     dano recibido de enemigos y proyectiles = 1 si es 1, si no floor(dano / 2)
  experienceMultiplier  multiplier
  breakWalls      character: ese personaje puede romper las paredes que suenan (el otro siempre puede)
  maxProjectiles  character, weapons, value: limite simultaneo de Popolon con el objeto (sin el, 2)
  rockDamage      character, value, rockHp: golpe de espada a una piedra (sin el objeto, 1)
  freezeEnemies   key, durationTicks, maxUses: congela enemigos, generadores y jefe
  breakAllRocks   el primer espadazo en cada visita a una sala rompe todas sus piedras
  swordDamageBonus value (espada, flechas, fuegos), mineValue (mina): vida extra por golpe; no con jefes
  killBatsWord    word, enemyType: escribir la palabra mata a todos los de ese tipo de la sala
  warpToDoor      keys: en el menu, en el castillo, la tecla '1'-'9' lleva a la sala de la puerta de ese
                  mundo si ya se ha abierto alguna vez (doorRooms)
  shield          level, blocks: tipos de proyectil que para ese nivel (incluye los anteriores)
  revolvingDoors  character: ese personaje cruza las puertas giratorias con el objeto (el otro siempre)
  resource        counter ('arrows' E046, 'coins' E048, 'keys' E04A) y amount (BCD, tope 999)

Bloques globales:
  menu            maquetacion y reglas del menu F1 (docs/items_enemies.md "Inventario (menu F1)")
  vitality        vitalidad, experiencia y vidas (docs/items_enemies.md "Vitalidad y experiencia")
  worldMaps[]     por mundo 1-10: rectangulo del mapa en el menu y celda de cada sala; el marcador se pinta
                  en x = 8 * col + 1, y = 144 + 4 * row (+1 en pantalla, como todo sprite MSX)

  python3 tools/extract/items_export.py      (lo llama tambien unity_bundle.py)
"""
import json
from galious_rom import Rom, ROOT


def grp(bank):
    """Grupo de bancos (galious_rom) para una direccion de 6000-BFFF del banco 'bank'."""
    return (bank - 1) // 3


def op(rom, bank, addr, expect):
    got = rom.block(addr, len(expect), None if addr < 0x6000 else grp(bank))
    assert got == bytes(expect), f'b{bank:02d}:{addr:04X}: {got.hex()} != {bytes(expect).hex()}'
    return got


def zlist(rom, bank, addr):
    out = []
    while (b := rom.byte(addr, grp(bank))) != 0:
        out.append(b)
        addr += 1
    return out


NAMES = {
    0x01: 'Arrow', 0x02: 'Ceramic Arrow', 0x03: 'Rolling Fire', 0x04: 'Fire', 0x05: 'Mine',
    0x06: 'Magnifying Glass', 0x07: 'Holy Water', 0x08: 'Cloak', 0x09: 'Magic Rod', 0x0A: 'Map',
    0x0B: 'Cross', 0x0C: 'Great Key', 0x0D: 'Necklace', 0x0E: 'Crown', 0x0F: 'Helmet', 0x10: 'Oar',
    0x11: 'Shoes', 0x12: 'Decorative Doll', 0x13: 'Hoi', 0x14: 'Bell', 0x15: 'Angel Ring', 0x16: 'Candle',
    0x17: 'Armor', 0x18: 'Carpet', 0x19: 'Hat Helmet', 0x1A: 'Lamp', 0x1B: 'Vase', 0x1C: 'Pendant',
    0x1D: 'Earrings', 0x1E: 'Bracelet', 0x1F: 'Ring', 0x20: 'Bible', 0x21: 'Harp', 0x22: 'Triangle',
    0x23: 'Conch Shell', 0x24: 'Pitcher', 0x25: 'Saber', 0x26: 'Dagger', 0x27: 'Wing',
    0x28: 'Copper Shield', 0x29: 'Bread & Water', 0x2A: 'Salt', 0x2B: 'Silver Shield', 0x2C: 'Gold Shield',
    0x2D: 'Quiver', 0x2E: 'Coins', 0x2F: 'Keys',
}

# docs/design_bible/08_santuarios.md (tiendas de b02:9253)
SHOPS = {
    0x05: [('castle', 115, 'Hephaestus', 30)], 0x06: [('castle', 143, 'Ares', 20)],
    0x0D: [('castle', 32, 'Demeter', 25)], 0x10: [('world05', 0, 'Demeter', 30)],
    0x18: [('world08', 0, 'Demeter', 30)], 0x19: [('castle', 10, 'Ares', 50)],
    0x21: [('castle', 5, 'Hephaestus', 30)], 0x23: [('castle', 55, 'Hephaestus', 25)],
    0x28: [('castle', 143, 'Ares', 30)], 0x2B: [('castle', 65, 'Ares', 40)], 0x2C: [('castle', 4, 'Ares', 50)],
}

WORLD_BIT = {0x07: 5, 0x08: 6, 0x09: 7, 0x0A: 4}       # b03:B772-B789
WEAPON_X = None                                          # se lee de la ROM (b01:68E4)


def menu_slot(t, weapon_x):
    """Casilla del objeto en el menu F1 (b01:6715-67B6), en px de pantalla."""
    if 0x0D <= t <= 0x2A:                 # rejilla: bufer EE46 = fila 6, columna 6; 10 x 3 casillas de 2x2 tiles
        k = t - 0x0D
        return {'area': 'items', 'slot': k, 'x': 8 * (6 + 2 * (k % 10)), 'y': 8 * (6 + 4 + 2 * (k // 10)),
                'w': 16, 'h': 16}
    if 0x07 <= t <= 0x0C:                 # fila de objetos de mundo: bufer EF4F = fila 14, columna 15
        j = t - 0x07
        return {'area': 'world', 'slot': j, 'x': 8 * (15 + 2 * j), 'y': 8 * (14 + 4), 'w': 16, 'h': 16}
    if 1 <= t <= 6:                       # armas: bufer EFAE = fila 17; columnas 14,17,20,22,24,27 (b01:678A)
        return {'area': 'weapons', 'slot': t - 1, 'x': weapon_x[t - 1], 'y': 8 * (17 + 4), 'w': 16, 'h': 8}
    return {'area': 'none', 'slot': -1, 'x': 0, 'y': 0, 'w': 0, 'h': 0}


def build(rom):
    # --- constantes leidas de la ROM ---------------------------------------------------------------
    # columnas de las armas en el menu: avance 3,3,2,2,3 tiles (b01:678C-6796) desde la columna 14
    op(rom, 1, 0x678C, [0xD6, 0x03, 0xFE, 0x02, 0x01, 0x03, 0x00])
    cols, c = [], 14
    for left in range(6, 0, -1):
        cols.append(c)
        c += 3 if (left - 3) & 0xFF >= 2 else 2
    weapon_x = [8 * c for c in cols]
    cursor_x = list(rom.block(0x68E4, 6, 0))                    # b01:68E4[E04E]: x del cursor de armas
    op(rom, 3, 0xA6B9, [0x11, 0x00, 0x01])                      # escalera: LD DE,0100 / 0140 (b03:A6B6)
    op(rom, 3, 0xA6BF, [0x11, 0x40, 0x01])
    op(rom, 3, 0xA232, [0x01, 0x00, 0x02])                      # andar 0x200, con Shoes 0x280 (b03:A229)
    op(rom, 3, 0xA23A, [0x01, 0x80, 0x02])
    speed = lambda a: rom.word(a, 0) / 256                      # velocidades 8.8 en px/tick
    op(rom, 3, 0xA22D, [0x01, 0x80, 0x00])                      # en el agua 0x080
    bible = op(rom, 2, 0x9BDF, [0x21, 0x01, 0x96])[2]           # E0D5 = 1, E0D6 = 0x96 ticks
    bible_max = op(rom, 2, 0x9BD3, [0xFE, 0x10])[1]             # E531 < 16 usos
    key_lim = op(rom, 3, 0xAEA6, [0xFE, 0x48])[1]               # max vitalidad < 0x48 -> +8
    key_step = op(rom, 3, 0xAEA9, [0xC6, 0x08])[1]
    op(rom, 3, 0xB3C5, [0x48, 0x0D, 0x28, 0x05, 0x78, 0x1F])    # Hat Helmet: si dano != 1, dano >> 1
    op(rom, 3, 0xB841, [0x21, 0x8E, 0xE0, 0xCD, 0xDB, 0x4D, 0x06, 0x01, 0x28, 0x02, 0x06, 0x05])  # Ring
    op(rom, 1, 0x7F63, [0x3A, 0x76, 0xE0, 0xB7, 0x78, 0x28, 0x02, 0xCB, 0x3F])  # Holy Water: SRL
    op(rom, 3, 0xBAC4, [0x3A, 0x77, 0xE0, 0xB7, 0x7E, 0x28, 0x03, 0x1F, 0xE6, 0x7F])  # Cloak
    boss_hp = [rom.byte(0x7F97 + b, 0) for b in range(12)]     # b01:7F97[jefe]
    boss_dmg = [rom.byte(0xBAD2 + b, 0) for b in range(12)]    # b03:BAD2[jefe]
    boss_hp[0] = boss_dmg[0] = 0                                 # el indice 0 no es un jefe
    # escudos: b04:65BC[nivel - 1] -> lista de tipos que para (acabada en 0)
    shield = {}
    for lvl in (1, 2, 3):
        p = rom.word(0x65BC + 2 * (lvl - 1), 1)
        shield[lvl] = sorted(set(zlist(rom, 4, p)))
    # Bell: b09:BE4F[mundo] -> salas (acabada en 0); el mundo 10 tiene 4 posiciones de puerta (BE63[E06E])
    bell = [{'world': k, 'rooms': zlist(rom, 9, rom.word(0xBE4F + 2 * k, 2))} for k in range(1, 10)]
    bell10 = [{'doorPosition': p, 'rooms': zlist(rom, 9, rom.word(0xBE63 + 2 * p, 2))} for p in range(4)]
    # Wing: sala de la puerta de cada mundo (b02:9990[mundo - 1])
    door_rooms = [rom.byte(0x9990 + k, 0) for k in range(9)]
    # valores iniciales E051-E05B (b00:4CE6)
    init = list(rom.block(0x4CE6, 11))

    items_src = json.loads((ROOT / 'extracted' / 'entities' / 'items.json').read_text())

    def rooms_of(t):
        return items_src.get(f'{t:#04x}', {}).get('rooms', [])

    E = {}   # efectos por tipo
    for t in range(1, 7):
        E[t] = ({'kind': 'subWeapon', 'weaponId': t}, '[C][O*]', 'b03:A72B, A96D-A9CE, AA0A; weapons.json')
    E[0x07] = ({'kind': 'bossHpDivide', 'divisor': 2, 'bossHp': boss_hp}, '[C]', 'b01:7F63')
    E[0x08] = ({'kind': 'bossContactDamageDivide', 'divisor': 2, 'bossContactDamage': boss_dmg}, '[C]',
               'b03:BABA-BAD0')
    E[0x09] = ({'kind': 'freeAmmoInBossFight'}, '[C]', 'b03:AA8E-AA98 (y A995: hace falta E046 > 0)')
    E[0x0A] = ({'kind': 'worldMap'}, '[C][O*]', 'b01:661B-668C')
    E[0x0B] = ({'kind': 'canDamageBoss', 'bossId': 9}, '[C]', 'b03:BBA8')
    E[0x0C] = ({'kind': 'worldKey', 'maxVitalityStep': key_step, 'maxVitalityLimit': key_lim}, '[C][O]',
               'b03:B78C-B7BD, AE9A-AEBB; b00:4BE8; b02:99F2-99FB')
    E[0x0D] = ({'kind': 'worldEntry', 'world': 7, 'character': 'popolon'}, '[C][O*]', 'b02:9A3A-9A45; b01:684B')
    E[0x23] = ({'kind': 'worldEntry', 'world': 5, 'character': 'aphrodite'}, '[C][O*]',
               'b02:9A2C-9A38; b01:683D')
    E[0x2A] = ({'kind': 'shrineEntry', 'godType': 3, 'map': 'castle', 'room': 87}, '[C][O*]', 'b02:9A80-9A88')
    for t, enemy in ((0x0E, 0x2A), (0x0F, 0x28), (0x17, 0x2E), (0x22, 0x31), (0x24, 0x08)):
        E[t] = ({'kind': 'contactImmunity', 'enemyTypes': [enemy]}, '[C]', 'b03:B1F7-B22D')
    E[0x10] = ({'kind': 'walkOnWater', 'world': 5, 'rooms': [12, 13], 'tile': 0xB6}, '[C]', 'b03:B037-B056')
    E[0x11] = ({'kind': 'speedMultiplier', 'walkSpeed': speed(0xA233), 'walkSpeedWith': speed(0xA23B),
                'ladderSpeed': speed(0xA6BA), 'ladderSpeedWith': speed(0xA6C0)}, '[C][O]',
               'b03:A1E7-A1EF, A232-A23A (A5FE), A6B6-A6BF')
    E[0x12] = ({'kind': 'bridge', 'world': 2, 'room': 11}, '[C][O*]', 'b08:9F87')
    E[0x13] = ({'kind': 'noReversedControls', 'world': 8}, '[C][O*]', 'b03:A8A0-A8C7')
    E[0x14] = ({'kind': 'doorChime', 'sound': 0x28, 'roomsByWorld': bell, 'roomsWorld10': bell10}, '[C]',
               'b00:5D24-5D65 (desde 51A3, al cargar la sala)')
    E[0x15] = ({'kind': 'warpHome', 'key': 'Return', 'map': 'castle', 'room': 72, 'waitTicks': 0x78}, '[C][O*]',
               'b02:9AC9-9AE4; b00:47DE-4807, 4835-4841')
    E[0x16] = ({'kind': 'lightDarkRoom', 'world': 5, 'room': 6}, '[C][O*]', 'b00:55F7-560B (mapa fijo b08:8DAE)')
    E[0x18] = ({'kind': 'lavaToStone', 'world': 8}, '[C][O]', 'b00:5575-558F; b04:6200')
    E[0x19] = ({'kind': 'damageHalve'}, '[C]', 'b03:B3BF-B3CD')
    E[0x1A] = ({'kind': 'bossMarker'}, '[C][O*]', 'b01:668F-66BE (tabla b09:B0FC)')
    E[0x1B] = ({'kind': 'experienceMultiplier', 'multiplier': 2}, '[C]', 'b03:B62E-B636')
    E[0x1C] = ({'kind': 'breakWalls', 'character': 'aphrodite'}, '[C][O*]', 'b09:A668 (b00:4DDB)')
    E[0x1D] = ({'kind': 'maxProjectiles', 'character': 'popolon', 'weapons': [1, 2], 'value': 3}, '[C]',
               'b03:A99E-A9CA')
    E[0x1E] = ({'kind': 'maxProjectiles', 'character': 'popolon', 'weapons': [5], 'value': 3}, '[C]',
               'b03:A9B7-A9CA')
    E[0x1F] = ({'kind': 'rockDamage', 'character': 'aphrodite', 'value': 5, 'rockHp': 15}, '[C][O*]',
               'b03:B841-B851')
    E[0x20] = ({'kind': 'freezeEnemies', 'key': 'CTRL', 'durationTicks': bible, 'maxUses': bible_max}, '[C][O]',
               'b02:9BB6-9BEB; b06:B3B4; b00:520B')
    E[0x21] = ({'kind': 'breakAllRocks'}, '[C][O*]', 'b01:6398-63C1')
    E[0x25] = ({'kind': 'swordDamageBonus', 'value': 1, 'mineValue': 2}, '[C]', 'b03:B586-B591, B5D1-B5DE')
    E[0x26] = ({'kind': 'killBatsWord', 'word': 'UMBRELLA', 'enemyType': 0x0E}, '[C]', 'b02:9BEE-9C08; b03:B5EA')
    E[0x27] = ({'kind': 'warpToDoor', 'keys': '1-9', 'doorRooms': door_rooms}, '[C][O*]', 'b02:9A99-9AC8')
    for t, lvl in ((0x28, 1), (0x2B, 2), (0x2C, 3)):
        E[t] = ({'kind': 'shield', 'level': lvl, 'blocks': shield[lvl]}, '[C]',
                'b03:B74B-B758, B3E7-B40B; b04:659F-65BB')
    E[0x29] = ({'kind': 'revolvingDoors', 'character': 'aphrodite'}, '[C]', 'b09:A345-A375')
    for t, (cnt, amt) in {0x2D: ('arrows', 50), 0x2E: ('coins', 10), 0x2F: ('keys', 2)}.items():
        E[t] = ({'kind': 'resource', 'counter': cnt, 'amount': amt}, '[C][O]', 'b03:B7BE-B7D2')

    cat = {}
    for t in range(1, 7):
        cat[t] = 'weapon'
    for t in range(7, 0x0B):
        cat[t] = 'worldItem'
    for t in (0x0B, 0x0C, 0x0D, 0x23, 0x2A, 0x12, 0x13, 0x16, 0x18, 0x10):
        cat[t] = 'progression'
    for t in (0x0E, 0x0F, 0x17, 0x22, 0x24):
        cat[t] = 'immunity'
    for t in (0x11, 0x19, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x25, 0x29):
        cat[t] = 'upgrade'
    for t in (0x14, 0x15, 0x1A, 0x20, 0x21, 0x26, 0x27):
        cat[t] = 'utility'
    for t in (0x28, 0x2B, 0x2C):
        cat[t] = 'shield'
    for t in (0x2D, 0x2E, 0x2F):
        cat[t] = 'resource'

    items = []
    for t in range(1, 0x30):
        eff, conf, code = E[t]
        items.append({
            'type': t, 'hex': f'0x{t:02X}', 'name': NAMES[t], 'category': cat[t],
            'flag': f'E{0xE06F + t:04X}' if t < 0x2D else '',
            'perCharacter': False,
            'perWorld': t in WORLD_BIT, 'worldBit': WORLD_BIT.get(t, -1),
            'consumable': t == 0x0C,
            'menu': menu_slot(t, weapon_x),
            'effect': eff, 'confidence': conf, 'code': code,
            'rooms': rooms_of(t),
            'shops': [{'map': m, 'room': r, 'god': g, 'price': p} for m, r, g, p in SHOPS.get(t, [])],
        })

    # --- mapas del menu (b01:661B): b09:AF2A[mundo] -> (destino en el bufer, ancho, alto, tiles) ---------
    maps = []
    for k in range(1, 11):
        p = rom.word(0xAF2A + 2 * k, 2)
        dest = rom.word(p, 2) - 0xED80
        w, h = rom.byte(p + 2, 2), rom.byte(p + 3, 2)
        tiles = list(rom.block(p + 4, w * h, 2))
        e041 = k + 1
        cells_ptr = rom.word(0xB038 + 2 * e041, 2)
        wj = json.loads((ROOT / 'extracted' / 'unity' / f'world{k:02d}.json').read_text())
        cells = []
        for r in sorted(x['id'] for x in wj['rooms']):
            v = rom.byte(cells_ptr + r, 2)
            cells.append({'room': r, 'col': v & 15, 'row': v >> 4})
        boss = rom.byte(0xB0FC + e041, 2)
        maps.append({'world': k, 'x': 8 * (dest % 32), 'y': 8 * (dest // 32 + 4), 'widthTiles': w,
                     'heightTiles': h, 'tiles': tiles, 'roomCells': cells,
                     'bossRoom': wj.get('bossRoom', 0), 'bossCol': boss & 15, 'bossRow': boss >> 4})

    menu = {
        # apertura (b00:44D5): F1 por flanco, solo en el estado 5 con partida en curso y E500 fuera de 4-7
        'openKey': 'F1', 'openSound': 0x2B, 'moveSound': 0x2C, 'blockedPlayerStates': [4, 5, 6, 7],
        'pausesGame': True,
        # al cerrar con F1 la sala sigue igual (enemigos donde estaban); Wing y Angel Ring cargan sala nueva
        'closeKeepsRoom': True,
        'cancelPendingDamageOnOpen': True,        # b01:65C6: incremento pendiente negativo -> 0
        'labels': [{'text': 'ITEM', 'x': 112, 'y': 72}, {'text': 'MAP', 'x': 48, 'y': 136}],
        'portraits': [{'character': 'popolon', 'x': 96, 'y': 40}, {'character': 'aphrodite', 'x': 144, 'y': 40}],
        'characterCursorX': [0x91, 0x61],          # sprite: E04D = 0 Aphrodite, 1 Popolon; y = 0x27 (+1)
        'characterCursorY': 0x27,
        'weaponBox': {'x': 104, 'y': 160, 'w': 136, 'h': 24},
        'weaponCursorX': cursor_x, 'weaponCursorY': 0xA3,
        'blinkFrames': 8,                          # cursor de la fila activa y marcas del mapa: bit 3 de E003
        'mapMarkerColorPlayer': 8, 'mapMarkerColorBoss': 5,
    }
    vitality = {
        # b00:4CE6 -> E051-E05B al empezar partida
        'expMax': init[0], 'vitStart': init[1], 'vitMaxStart': init[2],
        'livesSpareAphrodite': init[7], 'livesSparePopolon': init[8], 'startCharacter': 'popolon',
        'barTiles': 6, 'barPxPerUnit': 1,           # b01:60C9: 6 tiles de 8 px, 1 px por unidad
        'drainTicks': 4,                            # b01:6171: 1 unidad cada 4 ticks (E604 & 3 == 0)
        'maxVitalityStep': key_step, 'maxVitalityLimit': key_lim,
        'lowVitalitySound': 0x2A, 'lowVitalityBelow': 6,   # b03:B3D9: al recibir dano con vida < 6
    }
    return {'items': items, 'menu': menu, 'vitality': vitality, 'worldMaps': maps}


def main():
    data = build(Rom())
    out = ROOT / 'extracted' / 'unity' / 'items.json'
    out.write_text(json.dumps(data))
    print(f'objetos: {len(data["items"])} tipos -> {out.relative_to(ROOT)}')


if __name__ == '__main__':
    main()
