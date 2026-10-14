#!/usr/bin/env python3
"""Jefes para el importador de Unity: extracted/unity/bosses.json (docs/boss_ai.md).

Comportamiento documentado en docs/boss_ai.md («Invocación», «Derrota», «Cajas de los jefes», «Hijos de los
jefes» y las fichas). Aqui se leen de la ROM las tablas y se arma el JSON; lo que es codigo (posiciones de
inicio, tiempos de las fichas) va escrito a mano con su direccion.

Unidades: px y ticks de la logica (1 tick = 1 pasada del bucle de juego = 2 frames a 60 Hz; 3 si hay mucha
carga). Los estados 15, 19 y 20 (invocacion, derrota, fanfarria) corren 1 vez por frame: sus tiempos van en
*Frames*. Coordenadas como en la ROM: y hacia abajo; el jefe (E705 = y, E707 = x) es la esquina superior
izquierda de su bloque de tiles, que se dibuja en la celda (y >> 3, x >> 3) de la tabla de nombres.

Raiz:
  tickFrames               2
  bosses[]                 un jefe por mundo (10; el jefe 5 no existe)
  invocation, summon, defeat, keyFanfare, deathDuringFight, damage, contact   reglas globales (ver abajo)
  children[]               hijos de los jefes (proyectiles); vida, dano y caja en enemies.json por su tipo

bosses[]:
  id                       numero de jefe (E700). No es el mundo: tabla b00:5E05[E041]
  world, name, word, room  mundo 1-10, nombre (guia), palabra que lo invoca (b02:9B55), sala (b00:5DFB)
  hp, hpHolyWater          vida (b01:7F97); con el Holy Water, hp / 2 redondeando hacia abajo
  weaponMask, weapons      mascara b03:BD0A (bit 0 espada, 1 mina, 2 fuego rodante, 3 fuego, 4 ceramica, 5 flecha)
                           y los numeros de arma (E510: 1 flecha .. 5 mina; 0 = espada) que le hacen dano
  hitTest                  como se decide que un arma le toca (b03:BB7A[id-1]):
                             kind 'box'          caja weaponBox del sprite actual (b14:91EC)
                                  'boxMinSprite' igual, pero solo con sprite >= minSprite (Sea Anemone 0x12, Big Lip 0x19)
                                  'boxNeedsCross' igual, pero solo con la Cross (E07A); sin ella ni suena ni hace dano
                                  'threeBoxes'   King Dragon: 3 cajas por pose (weaponBox, weaponBox2, weaponBox3)
                                  'headBox'      Donra: caja weaponBox del sprite de la cabeza (E719), no del cuerpo
                                  'elusiveNeck'  Elusive Dragon: ventana calculada (ver elusive)
                           mineKind: prueba para una mina YA explotada (b03:BC33, tabla desplazada una posicion a
                           partir del jefe 6): 'contactBox' | 'contactBoxOrHead' | 'none' | 'elusiveNeck'. Ojo: para
                           Galious NO mira la Cross
  contactTest              contacto cuerpo-jugador (b03:B9D9[id-1]): 'box' (contactBox), 'boxOrHead' (contactBox o
                           headContactBox), 'boxOrExtra' (contactBox o contactBox2), 'elusiveNeck'
  contactDamage, contactDamageCloak   vitalidad que quita el cuerpo (b03:BAD2, 8 = barra llena); /2 con el Cloak
  init {x, y}              posicion al aparecer (inicio de la ficha); Elusive Dragon: ver elusive
  sprites[]                {id, wTiles, hTiles, w, h (px), weaponBox, contactBox, contactBox2, weaponBox2,
                           weaponBox3, knockbackRefDx}. Cajas {valid, dy, dx, h, w} desde (y, x) del jefe, bordes
                           INCLUSIVOS como los enemigos (ocupan h+1 x w+1 px). knockbackRefDx: el jugador sale
                           empujado a la derecha si (x_jugador - 6) > x_jefe + knockbackRefDx (b03:BA82; para
                           sprites >= 0x20 la ROM lee bytes de codigo: se copian tal cual)
  part                     por sprite: 'body' | 'head' | 'mouth' (Donra)
  childTypes               tipos de enemigo que crea

invocation (estado 15, ver boss_ai.md):
  scanRows, keyMap         filas 0-7 de la matriz MSX; primera tecla pulsada (fila mas baja, bit mas bajo) -> caracter
  bufferLength             16; cada caracter nuevo entra por la derecha. Hay invocacion si el FINAL del bufer es la
                           palabra: las letras erroneas no importan, no hay tiempo limite, no hay mayusculas
  repeatNeedsRelease       la misma tecla no se repite mientras siga pulsada
  flashFrames, flashColors borde/fondo (VDP R7) alterna cada 2 frames; dissolveFrames, loadFrames
  music                    0x81 al empezar; exitsClosed: las 4 salidas a FF hasta coger la llave
summon (E0D2 = 1..3, ya en el estado 5): countdownTicks, sparkEveryTicks, sparkType, sparkPositions[{x,y}],
  sparkLifeTicks, maxSparks; waitAllEnemiesDead; music 0x90
defeat (estado 19), keyFanfare (estado 20), deathDuringFight: ver boss_ai.md
"""
import json
from galious_rom import Rom, ROOT

NAMES = {1: 'Bony Dragon', 2: 'Elusive Dragon', 3: 'Arlyane', 4: 'Sea Anemone', 6: 'King Dragon', 7: 'Big Lip',
         8: 'Monster', 9: 'Galious', 10: 'Donra', 11: 'Jump Slime'}
SPRITES = {1: range(0x0C, 0x10), 2: range(0x06, 0x0C), 3: range(0x3A, 0x3D), 4: range(0x10, 0x14),
           6: range(0x34, 0x37), 7: range(0x18, 0x1C), 8: range(0x24, 0x2B), 9: range(0x37, 0x39),
           10: range(0x1C, 0x24), 11: range(0x2B, 0x31)}
# posicion inicial (y, x) escrita por el inicio de cada jefe (b02, tabla b01:7F82)
INIT = {1: (0x68, 0x98), 3: (0x90, 0x60), 4: (0x48, 0x68), 6: (0x70, 0x58), 7: (0x40, 0x68),
        8: (0x20, 0x68), 9: (0x40, 0x60), 10: (0x60, 0x70), 11: (0x70, 0x80), 2: (0x20, 0xF8)}
CHILDREN = {1: [0x54, 0x55], 2: [0x57], 3: [0x58], 4: [0x59], 6: [0x57, 0x69], 7: [0x65], 8: [0x68], 9: [0x67],
            10: [0x57], 11: []}
HIT_KIND = {0xBBAD: 'box', 0xBB90: 'elusiveNeck', 0xBB96: 'boxMinSprite', 0xBB9F: 'boxMinSprite',
            0xBBA8: 'boxNeedsCross', 0xBBBF: 'threeBoxes', 0xBC02: 'headBox'}
MIN_SPRITE = {0xBB96: 0x12, 0xBB9F: 0x19}
MINE_KIND = {0xBC69: 'contactBox', 0xBC4B: 'elusiveNeck', 0xBC4E: 'contactBoxOrHead', 0xBC57: 'contactBoxOrHead',
             0xBC60: 'contactBoxOrHead', 0: 'none'}
CONTACT_KIND = {0xBADE: 'box', 0xB9EF: 'elusiveNeck', 0xB9FA: 'boxOrHead', 0xBA1E: 'boxOrHead',
                0xBA42: 'boxOrExtra'}
MASK_WEAPON = [(0x01, 0), (0x20, 1), (0x10, 2), (0x04, 3), (0x08, 4), (0x02, 5)]    # b03:BCC1
G0, G1, G14 = 0, 1, 4          # grupos de bancos: b01-03, b04-06, b13-15 (b14 en 8000)
s8 = lambda v: v - 256 if v > 127 else v
EMPTY = {'valid': False, 'dy': 0, 'dx': 0, 'h': 0, 'w': 0}


def box(b):
    return {'valid': True, 'dy': s8(b[0]), 'dx': s8(b[1]), 'h': b[2], 'w': b[3]}


def per_sprite_table(rom, base, ptr_index, group=G14):
    """Tabla de cajas por jefe (b14:91EC / 928A): puntero[E700] -> sprite base + 4 bytes por sprite."""
    p = rom.word(base + 2 * ptr_index, group)
    first = rom.byte(p, group)
    return lambda s: rom.block(p + 1 + 4 * (s - first), 4, group) if s >= first else None


def inline_table(rom, addr, group=G0):
    """Tabla con sprite base delante y 4 bytes por sprite (b03:BBDB...)."""
    first = rom.byte(addr, group)
    return lambda s: rom.block(addr + 1 + 4 * (s - first), 4, group)


def build(rom):
    words, bosses = {}, []
    world_of = {rom.byte(0x5E05 + w + 1): w for w in range(1, 11)}
    hit_tab = [rom.word(0xBB7A + 2 * i, G0) for i in range(11)]
    mine_tab = [rom.word(0xBC33 + 2 * i, G0) for i in range(11)]
    contact_tab = [rom.word(0xB9D9 + 2 * i, G0) for i in range(11)]
    kd = [inline_table(rom, a) for a in (0xBBDB, 0xBBE8, 0xBBF5)]           # King Dragon, 3 cajas de arma
    kd_head = lambda s: rom.block(0xBA12 + 4 * (s - 0x34), 4, G0)            # King Dragon, contacto cabeza
    donra_head = lambda s: rom.block(0xBA32 + 4 * (s - 0x1E), 4, G0)         # Donra, contacto cabeza
    slime_extra = lambda s: rom.block(0xBA56 + 4 * (s - 0x2B), 4, G0)        # Jump Slime, segunda caja
    for k in range(1, 11):
        p, n = rom.word(0x9B55 + 3 * (k - 1), G0), rom.byte(0x9B57 + 3 * (k - 1), G0)
        words[k] = rom.block(p, n, G0).decode()
    for bid in sorted(NAMES):
        w = world_of[bid]
        hp = rom.byte(0x7F97 + bid, G0)
        mask = rom.byte(0xBD0A + bid - 1, G0)
        hit = hit_tab[bid - 1]
        weap = per_sprite_table(rom, 0x91EC, bid)
        cont = per_sprite_table(rom, 0x928A, bid)
        sprites = []
        for s in SPRITES[bid]:
            part = 'body'
            if bid == 10:
                part = 'body' if s < 0x1E else 'head' if s < 0x22 else 'mouth'
            e = {'id': s, 'wTiles': rom.byte(0x9D90 + 2 * s, G14), 'hTiles': rom.byte(0x9D91 + 2 * s, G14),
                 'part': part, 'weaponBox': dict(EMPTY), 'weaponBox2': dict(EMPTY), 'weaponBox3': dict(EMPTY),
                 'contactBox': dict(EMPTY), 'contactBox2': dict(EMPTY),
                 'knockbackRefDx': rom.byte(0xBA9A + s, G0)}
            e['w'], e['h'] = 8 * e['wTiles'], 8 * e['hTiles']
            kind = HIT_KIND.get(hit, '')
            if bid == 6:
                e['weaponBox'], e['weaponBox2'], e['weaponBox3'] = (box(t(s)) for t in kd)
                e['contactBox'], e['contactBox2'] = box(cont(s)), box(kd_head(s))
            elif bid == 10:
                if part == 'head':
                    e['weaponBox'], e['contactBox'] = box(weap(s)), box(donra_head(s))
                elif part == 'body':
                    e['contactBox'] = box(cont(s))
            elif bid == 2:
                pass                                   # cajas calculadas: ver 'elusive'
            else:
                if kind in ('box', 'boxNeedsCross') or (kind == 'boxMinSprite' and s >= MIN_SPRITE[hit]):
                    e['weaponBox'] = box(weap(s))
                e['contactBox'] = box(cont(s))
                if bid == 11:
                    e['contactBox2'] = box(slime_extra(s))
            sprites.append(e)
        y, x = INIT[bid]
        bosses.append({
            'id': bid, 'world': w, 'name': NAMES[bid], 'word': words[w], 'room': rom.byte(0x5DFB + w + 1),
            'hp': hp, 'hpHolyWater': hp // 2, 'weaponMask': mask,
            'weapons': [wn for m, wn in MASK_WEAPON if mask & m],
            'hitTest': {'kind': HIT_KIND[hit], 'routine': f'{hit:04X}', 'minSprite': MIN_SPRITE.get(hit, 0),
                        'mineKind': MINE_KIND[mine_tab[bid - 1]], 'mineRoutine': f'{mine_tab[bid - 1]:04X}'},
            'contactTest': CONTACT_KIND[contact_tab[bid - 1]],
            'contactDamage': rom.byte(0xBAD2 + bid, G0), 'contactDamageCloak': rom.byte(0xBAD2 + bid, G0) // 2,
            'init': {'x': x, 'y': y}, 'sprites': sprites, 'childTypes': CHILDREN[bid],
            'defeatMusic': 0x8B if w == 10 else 0x91, 'dropsKey': w != 10,
        })

    key_pos = []
    for w in range(1, 10):
        b = rom.byte(0x4BCD + w + 1)
        key_pos.append({'world': w, 'y': (b & 0xF0) - 8, 'x': (b & 0x0F) << 4})
    sparks = [{'x': rom.byte(0x8003 + 2 * i, G0), 'y': rom.byte(0x8004 + 2 * i, G0)} for i in range(8)]
    keymap = ''.join(chr(c) if 0x20 < c < 0x7F else '.' for c in rom.block(0x9C3B, 64, G0))
    w16 = lambda a, g, n: [s16(rom.word(a + 2 * i, g)) for i in range(n)]
    data = {
        'tickFrames': 2,
        'bosses': bosses,
        'invocation': {
            'state': 15, 'listenWhen': 'en la sala del jefe, jefe no derrotado (E061+E041 bit 2) y E0D2 = 0',
            'scanRows': 8, 'keyMap': keymap, 'bufferLength': 16, 'matchTail': True, 'caseInsensitive': True,
            'repeatNeedsRelease': True, 'timeoutTicks': 0, 'wrongLetterResets': False,
            'music': 0x81, 'exitsClosed': True,
            'flashFrames': rom.byte(0x9F63, G0), 'flashColors': [1, 14], 'flashEveryFrames': 2,
            'dissolveFrames': 128, 'loadFrames': 5, 'totalFrames': 193,        # [O] mundo 1
        },
        'summon': {
            'countdownTicks': rom.byte(0x7FD2, G0), 'sparkEveryTicks': 8, 'sparkType': 0x63,
            'sparkPositions': sparks, 'sparkLifeTicks': 80, 'maxSparks': 4,   # [O]: limite de sprites (4 c/u)
            'waitAllEnemiesDead': True, 'music': rom.byte(0x7FD7, G0), 'solidTileDuringFight': 0xF0,
            'typicalTicksToFight': 105,                                       # [O] mundo 1, tras el estado 15
        },
        'defeat': {
            'state': 19, 'sound': 0x84, 'dissolveFrames': 128, 'keyType': 0x0C, 'keyPositions': key_pos,
            'keyMusicLoop': 0x91, 'galiousOpensRightExitTo': rom.byte(0x4BA3),
            'exitsStayClosedUntilKey': True, 'cannotRefight': True,
        },
        'keyFanfare': {
            'state': 20, 'pickupSounds': [0xFC, 0x85], 'music': 0x8B, 'hops': rom.byte(0xBE6B, G0),
            'hopVy': -10, 'hopGravity': 0.5, 'hopFrames': 40, 'messageFrames': 0x78,
            'message': ' I HAVE TAKEN WORLD n BACK !', 'totalFrames': 480,
            'unlocksNextWorldDoor': True, 'restoresRoomAndExits': True,
        },
        'deathDuringFight': {'respawnInBossRoom': True, 'resummonWithoutWord': True, 'hpRestored': True},
        'damage': {
            'swordHitsPerSwing': 1, 'perHit': 1, 'minePerHit': 2,
            'invalidWeaponSound': 0x1D, 'hitSound': 0x27,
            'ceramicPierces': True, 'ceramicRehitTicks': 16, 'mineRehitTicks': 32,
            'projectileDestroyedOnHit': True, 'projectileDestroyedEvenIfImmune': True,
            'needsVisibleFlag': True,
        },
        'contact': {
            'playerBox': {'dy': -14, 'dx': -6, 'h': 15, 'w': 15},   # b03:BAEA compara con 16 px (inclusivo)
            'knockbackVx': 4, 'knockbackVy': -1.5, 'setsInvulnerable': True,
        },
        'elusive': {
            'xRight': 0xF8, 'xLeft': 0, 'vxRight': -2, 'yByIndex': list(rom.block(0x8341, 8, G0)),
            'contactDyMax': 0x28, 'weaponDyExtra': 0x18, 'weaponHeadWindow': 0x40,
        },
        'children': children(rom, w16),
    }
    return data


def s16(v):
    return v - 0x10000 if v & 0x8000 else v


def children(rom, w16):
    fx = lambda v: round(v / 256, 4)
    bones = [{'b': b, 'vx': fx(s16(rom.word(0x6F43 + 6 * b, G1))), 'vy': fx(s16(rom.word(0x6F45 + 6 * b, G1))),
              'ay': fx(s16(rom.word(0x6F47 + 6 * b, G1)))} for b in range(3)]
    spore_vy = [fx(v) for v in w16(0xAE2C, G1, 8)]
    spore_vx = [fx(v) for v in w16(0xAE3C, G1, 8)]
    lip_vx = [fx(v) for v in w16(0xA5B3, G1, 9)]
    lip_vy = [fx(v) for v in w16(0xA5C5, G1, 9)]
    lip_spr = list(rom.block(0xA5D7, 9, G1))
    dir_vx = [fx(v) for v in w16(0x7E49, G1, 16)]
    dir_vy = [fx(v) for v in w16(0x7E69, G1, 16)]
    ball = [s8(b) for b in rom.block(0x9F66, 5, G1)]
    return [
        {'type': 0x54, 'parent': 1, 'name': 'bone', 'motion': 'ballistic', 'variants': bones,
         'note': '3 a la vez (B = 2, 1, 0) en (x, y) del jefe; al recibir un golpe (espada o arma) se parte en dos '
                 '0x55 (B = 0 y 1 = variantes 1 y 2 de 0x54) y desaparece; atraviesa paredes'},
        {'type': 0x55, 'parent': 1, 'name': 'bone half', 'motion': 'ballistic', 'invulnerableTicks': 8},
        {'type': 0x57, 'parent': 2, 'name': 'flame row', 'motion': 'tileRow', 'growPx': 8,
         'lifeTicks': [12, 12, 8], 'grows': ['right', 'left', 'left'], 'sound': 0x1C,
         'note': 'caja dy 0, alto 8, ancho 8*n (n = 1..12 o 1..8); B = 0 crece a la derecha (x fija), B = 1/2 '
                 'avanza x -= 8 cada tick con el extremo derecho fijo; invulnerable'},
        {'type': 0x58, 'parent': 3, 'name': 'fan shot', 'motion': 'aimed', 'speed': 3.5,
         'angleUnitsPerTurn': 256, 'spreadStep': 8, 'spreadIndexOffset': -3, 'jitter': [-8, 7],
         'note': '6 a la vez con B = 5..0: angulo al jugador + (B-3)*8 + rnd(-8..7) (256 = vuelta)'},
        {'type': 0x59, 'parent': 4, 'name': 'spore', 'motion': 'ballistic', 'gravity': 0.5,
         'vyByIndex': spore_vy, 'vxByIndex': spore_vx, 'note': 'B = rnd 0..15, indice B >> 1, vx negativa si B impar'},
        {'type': 0x65, 'parent': 7, 'name': 'lip shot', 'motion': 'straight', 'gridVx': lip_vx, 'gridVy': lip_vy,
         'gridSprite': lip_spr,
         'note': 'indice = 3*col + fila; col 0 si x_jug <= x, 1 si x_jug <= x+0x40, si no 2; fila igual con y'},
        {'type': 0x67, 'parent': 9, 'name': 'orb', 'motion': 'orbit', 'count': 8,
         'startAngles': list(rom.block(0x8ED7, 8, G0)), 'angleStep': rom.byte(0xA5E6, G1),
         'radiusStart': rom.byte(0xA5EA, G1), 'radiusMin': 0x18, 'radiusMax': 0x32, 'radiusSpeed': 0.4375,
         'launchSpeedMul': 8, 'launchDirVx': dir_vx, 'launchDirVy': dir_vy,
         'launchAngleByDir': list(rom.block(0xA798, 16, G1)), 'launchOutTicks': 9, 'launchBackTicks': 7,
         'note': 'pos = centro + r*sin; x usa angulo + 0x40; bumeran: el centro de su orbita sale 9 ticks a '
                 '8 px/tick y vuelve 7, luego salta al centro de Galious'},
        {'type': 0x68, 'parent': 8, 'name': 'bouncing ball', 'motion': 'bounce', 'speed': 4,
         'dirs': [{'vy': ball[i], 'vx': ball[i + 1]} for i in range(4)], 'waitTicks': 8, 'lifeTicks': 0xC0,
         'minY': 0x20},
        {'type': 0x63, 'parent': 0, 'name': 'summon spark', 'motion': 'none', 'lifeTicks': 80, 'hwSprites': 4},
    ]


def main():
    out = ROOT / 'extracted' / 'unity' / 'bosses.json'
    out.parent.mkdir(parents=True, exist_ok=True)
    data = build(Rom())
    out.write_text(json.dumps(data, indent=1, ensure_ascii=False))
    print(f'jefes: {len(data["bosses"])} -> {out}')


if __name__ == '__main__':
    main()
