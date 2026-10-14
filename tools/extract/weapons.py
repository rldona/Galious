#!/usr/bin/env python3
"""Armas secundarias (tecla M) para el importador de Unity: extracted/unity/weapons.json.

Comportamiento documentado en docs/player.md, "Armas secundarias" (codigo b03:A72B-AE97, B40C-B5E7, B7D5-B85C,
BB24-BD52; medido en openMSX). Aqui solo se leen de la ROM las constantes y se arma el JSON; si una
comprobacion de opcode falla, la ROM no es la esperada.

Unidades: px y ticks de la logica (1 tick = 2 frames a 60 Hz). Coordenadas como en la ROM: y crece hacia abajo,
y del jugador = fila superior del tile de suelo que pisa (E505), x del jugador = su centro (E507). Los
proyectiles guardan (y, x) = esquina superior izquierda de su sprite o tile.

Campos de cada arma (weapons[]):
  id, name                 numero del arma (E510) y nombre del menu
  cost                     flechas (E046, BCD) que gasta cada disparo; sin flechas no se dispara
  pool                     'projectile' (huecos E540/E560/E580, cuenta E51C, compartidos por las armas 1-4)
                           o 'mine' (E5A0/E5C0/E5E0, cuenta E51D)
  maxPopolon, maxAphrodite a la vez, contando todo lo vivo del mismo pool
  maxItem, maxPopolonWithItem  objeto que sube el limite de Popolon (0x1D Earrings, 0x1E Bracelet; 0 = ninguno)
  air, ladder, ground      se puede disparar en el aire (saltando o cayendo), en la escalera, en el suelo
  blockedOnLift            no se puede sobre el ascensor (bit 0 de EC12/EC22)
  spawnDxRight, spawnDxLeft, spawnDy   posicion del proyectil al crearlo, relativa a (x, y) del jugador.
                           En el mismo tick ya se mueve una vez (vx, vy), salvo lo dicho en firstTickMoves
  spawnAlignY8, spawnMinY  la y se redondea hacia abajo a multiplo de 8 y no baja de spawnMinY (flechas)
  firstTickMoves           false: el primer tick no avanza en x (fuego: vx se pone en el segundo tick)
  speedX, fallSpeed        px/tick en horizontal y al caer (o al bajar/subir por el lateral, fuego rodante)
  motion                   'straight' | 'wrapPlatform' (rodea la plataforma una vuelta) | 'floorFall'
                           (corre por el suelo y cae recto por los bordes) | 'mine' | 'none'
  piercing                 no se destruye al dar a un enemigo (ni a un jefe)
  rehitPeriodTicks         si piercing (o mina explotada): vuelve a dar al mismo enemigo cuando el contador
                           global E604 es multiplo de este valor (16: cada 16 ticks; 32 la explosion)
  damage, saberBonus       vida que quita a un enemigo por golpe, y extra con el Saber (0x25, E094)
  bossDamage, bossMaskBit  vida que quita al jefe (sin Saber) y bit de la mascara b03:BD0A que debe tener
  rockDamagePopolon, rockDamageAphrodite   vida que quita a una piedra (15 de vida). Ring (0x1F) = como Popolon
  hitbox{dy,dx,h,w,alignTo8}   caja contra enemigos/piedras/jefe: [y+dy, y+dy+h] x [x+dx, x+dx+w] (cerrada); con
                           alignTo8 se usan (y & ~7, x & ~7). Hay contacto si se solapa con la caja cerrada del
                           enemigo [ey+dy, ey+dy+alto] x [ex+dx, ex+dx+ancho] (b14:9006, items_enemies.md)
  deflectable              un enemigo con escudo (IX+2 bit 2, o bit 1 segun el lado) lo rebota
  sound                    sonido al disparar (sfx)
"""
import json
from galious_rom import Rom, ROOT

G = 0          # grupo de bancos del codigo del jugador (b03 en A000-BFFF)


def op(rom, addr, expect):
    got = rom.block(addr, len(expect), G)
    assert got == bytes(expect), f'{addr:04X}: {got.hex()} != {bytes(expect).hex()}'
    return got


def build(rom):
    # velocidades: LD BC,0x0800 (flecha) y 0x0400 (ceramica); fuego rodante y fuego, B = 4 / C = 4
    arrow_v = op(rom, 0xAA39, [0x01, 0x00, 0x08])[2]
    ceramic_v = op(rom, 0xAADE, [0x01, 0x00, 0x04])[2]
    rolling_v = op(rom, 0xAB24, [0x06, 0x04])[1]
    fire_v = op(rom, 0xAC17, [0x01, 0x04, 0x30])[1]
    fall_v = op(rom, 0xAC78, [0x3E, 0x04])[1]
    min_y = op(rom, 0xAA64, [0xFE, 0x20])[1]
    boom_ticks = op(rom, 0xAE4F, [0xFD, 0x36, 0x08, 0x46])[3]
    blink = list(rom.block(0xAD2E, 6, G))
    # cajas: tabla b03:B8EF + 2*tipo -> (alto, ancho)
    box = {t: (rom.byte(0xB8EF + 2 * t, G), rom.byte(0xB8F0 + 2 * t, G)) for t in range(1, 6)}
    mask = {1: 0x20, 2: 0x10, 3: 0x04, 4: 0x08, 5: 0x02}     # b03:BCC1
    snd = {1: 0x14, 2: 0x15, 3: 0x16, 4: 0x16, 5: 0x16}       # b03:AA85-AA89, ACD6
    bit = lambda t: mask[t].bit_length() - 1

    def hb(t, dy=0, dx=0, align=False):
        return {'dy': dy, 'dx': dx, 'h': box[t][0], 'w': box[t][1], 'alignTo8': align}

    common = dict(cost=1, damage=1, saberBonus=1, bossDamage=1, rockDamagePopolon=5, rockDamageAphrodite=1,
                  piercing=False, rehitPeriodTicks=0, deflectable=True, blockedOnLift=False,
                  spawnAlignY8=False, spawnMinY=0, firstTickMoves=True, fallSpeed=0)
    w = []
    for t, name, v in ((1, 'Arrow', arrow_v), (2, 'Ceramic Arrow', ceramic_v)):
        w.append({**common, 'id': t, 'name': name, 'pool': 'projectile',
                  'maxPopolon': 2, 'maxAphrodite': 3, 'maxItem': 0x1D, 'maxPopolonWithItem': 3,
                  'air': True, 'ladder': False, 'ground': True,
                  'spawnDxRight': 0, 'spawnDxLeft': -8, 'spawnDy': -8, 'spawnAlignY8': True, 'spawnMinY': min_y,
                  'speedX': v, 'motion': 'straight',
                  'piercing': t == 2, 'rehitPeriodTicks': 16 if t == 2 else 0,
                  'bossMaskBit': bit(t), 'hitbox': hb(t, 2, 2, True), 'sound': snd[t]})
    w.append({**common, 'id': 3, 'name': 'Rolling Fire', 'pool': 'projectile',
              'maxPopolon': 2, 'maxAphrodite': 3, 'maxItem': 0, 'maxPopolonWithItem': 2,
              'air': False, 'ladder': False, 'ground': True, 'blockedOnLift': True,
              'spawnDxRight': -4, 'spawnDxLeft': -4, 'spawnDy': -8,
              'speedX': rolling_v, 'fallSpeed': rolling_v, 'motion': 'wrapPlatform',
              'bossMaskBit': bit(3), 'hitbox': hb(3), 'sound': snd[3]})
    w.append({**common, 'id': 4, 'name': 'Fire', 'pool': 'projectile',
              'maxPopolon': 2, 'maxAphrodite': 3, 'maxItem': 0, 'maxPopolonWithItem': 2,
              'air': False, 'ladder': False, 'ground': True, 'blockedOnLift': True,
              'spawnDxRight': -8, 'spawnDxLeft': -8, 'spawnDy': -8, 'firstTickMoves': False,
              'speedX': fire_v, 'fallSpeed': fall_v, 'motion': 'floorFall',
              'bossMaskBit': bit(4), 'hitbox': hb(4), 'sound': snd[4]})
    w.append({**common, 'id': 5, 'name': 'Mine', 'pool': 'mine',
              'maxPopolon': 2, 'maxAphrodite': 3, 'maxItem': 0x1E, 'maxPopolonWithItem': 3,
              'air': False, 'ladder': False, 'ground': True, 'blockedOnLift': True,
              'spawnDxRight': -8, 'spawnDxLeft': -8, 'spawnDy': -4,
              'speedX': 0, 'motion': 'mine', 'damage': 2, 'saberBonus': 2, 'bossDamage': 2,
              'rockDamagePopolon': 0, 'rockDamageAphrodite': 0, 'rehitPeriodTicks': 32, 'deflectable': False,
              'bossMaskBit': bit(5), 'hitbox': hb(5, 0, 2), 'sound': snd[5]})
    w.append({**common, 'id': 6, 'name': 'Magnifying Glass', 'pool': 'none', 'cost': 0,
              'maxPopolon': 0, 'maxAphrodite': 0, 'maxItem': 0, 'maxPopolonWithItem': 0,
              'air': True, 'ladder': True, 'ground': True,
              'spawnDxRight': 0, 'spawnDxLeft': 0, 'spawnDy': 0, 'speedX': 0, 'motion': 'none',
              'damage': 0, 'saberBonus': 0, 'bossDamage': 0, 'rockDamagePopolon': 0, 'rockDamageAphrodite': 0,
              'deflectable': False, 'bossMaskBit': -1,
              'hitbox': {'dy': 0, 'dx': 0, 'h': 0, 'w': 0, 'alignTo8': False}, 'sound': 0})
    return {
        'tickFrames': 2,
        'newPressRequired': True,          # mantener M no repite el disparo [O]
        'stopsPlayer': False,              # disparar no para al jugador ni tiene animacion [O]
        'projectileSlots': 3, 'mineSlots': 3,
        # vida de cualquier proyectil: muere si x < 8, x >= 248 o y >= 184 tras moverse (b03:AD61)
        'killMinX': 8, 'killMaxX': 248, 'killMaxY': 184,
        'solidTile': 0xBA, 'solidTileBossFight': 0xF0,   # pared: tile >= este valor (b00:554D, E0D2 = combate)
        'weapons': w,
        'mine': {
            # se arma cuando el jugador sale de |dy| y |dx|: no armada mientras 0 <= pyFeet - mineY < 21 y
            # -8 <= px - mineX < 25 (b03:ACE1); hasta entonces no la tocan ni enemigos ni jugador
            'armDy': 0x15, 'armDxMin': -8, 'armDxMax': 0x21 - 8,
            'needsFloorAt': [2, 15],        # tiles solidos en (mineX + 2, py) y (mineX + 15, py) (b03:ACB1)
            'fuseTicks': 0,                 # no hay mecha: explota al tocarla un enemigo, el jefe o el jugador
            'explosionTicks': boom_ticks,
            'explosionShiftY': -12, 'explosionShiftX': -8,             # b03:AE53: la mina se desplaza al estallar
            'explosionHitbox': {'dy': -16, 'dx': 0, 'h': 0x20, 'w': 0x20, 'alignTo8': False},  # tras desplazarse
            'blinkColours': blink, 'blinkEveryTicks': 4,
            'playerDamage': 2,              # unidades de vitalidad (8 = barra llena), una sola vez por mina
            'playerBox': {'dy': -16, 'dx': -8, 'h': 16, 'w': 16},   # caja del jugador contra la mina (b03:B40C)
            'ignoresInvulnerablePlayer': True,                       # con E518 = 1 la mina no lo ve
            'sound': 0x26,
        },
        'rollingFire': {
            # vuelta a la plataforma: encima, y = suelo - 8; al perder el suelo baja por el lateral (x alineada a
            # 8, +8 si iba a la izquierda), por debajo va en sentido contrario (y alineada a 8), sube por el otro
            # lateral y al volver arriba muere al cruzar la x en la que nacio. Muere al chocar con pared/suelo.
            'floorProbeDy': 8, 'ceilingProbeDy': -1, 'wallProbeDxRight': 8, 'wallProbeDxLeft': -1, 'wallProbeDy': 2,
            'laps': 1, 'animEveryTicks': 8,
        },
        'fire': {
            # y en el suelo = suelo - 4; si no hay suelo bajo (x, y+4) ni (x+8, y+4): vx = 0, cae a fallSpeed y
            # al tocar suelo sigue en el mismo sentido (y alineada a 8 y luego +4)
            'floorProbeDy': 4, 'restOffsetFromFloor': -4,
        },
        'deflect': {'vy': -8, 'reverseX': True, 'sound': 0x1D},   # rebotado: sube 8 px/tick, sin dañar
        'sounds': {'enemyHit': 0x1E, 'enemyKilled': 0x20, 'bossHit': 0x27, 'immune': 0x1D, 'rockBroken': 0x20},
        'magicRod': {'item': 0x09, 'flag': 0xE078, 'freeInBossFight': True},   # E0D2 != 0 y E078: no gasta flechas
        'magnifier': {
            # M con la lupa elegida: E517 = 1; b03:B194 lo consume. Si la sala tiene lapida (E678 != 0) y
            # y_jug - 16 - lapY en [-16, 16] y x_jug - 8 - lapX en [-16, 16] -> E01B = tipo -> estado 12
            'dyMin': -16, 'dyMax': 16, 'playerDy': -16, 'dxMin': -16, 'dxMax': 16, 'playerDx': -8,
            'tombstones': tombstones(rom),
        },
    }


def tombstones(rom):
    """Lapidas por mundo (b02:9C83 + 2*(E041-2)): sala, tipo (0 -> E678 = 0xFF), x, y."""
    out = []
    for k in range(10):
        a = rom.word(0x9C83 + 2 * k, G)
        while rom.byte(a, G):
            room, b1, b2 = rom.block(a, 3, G)
            out.append({'world': k + 1, 'e041': k + 2, 'room': room, 'kind': b1 & 15,
                        'x': (((b2 & 1) << 7) | b1 >> 1) & 0xF8, 'y': b2 & 0xF8})
            a += 3
    return out


def main():
    out = ROOT / 'extracted' / 'unity' / 'weapons.json'
    out.parent.mkdir(parents=True, exist_ok=True)
    data = build(Rom())
    out.write_text(json.dumps(data, indent=1))
    print(f'armas: {len(data["weapons"])} -> {out}')


if __name__ == '__main__':
    main()
