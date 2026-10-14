#!/usr/bin/env python3
"""Control remoto de openMSX para automatizar Galious.

  omsx.py serve [args de openmsx...]   arranca openMSX (-control stdio) y escucha en SOCK
  omsx.py '<comando tcl>' [...]        envia cada comando y escribe su respuesta
  omsx.py -f fichero.tcl               envia el fichero como un unico comando
  omsx.py run SEG [teclas]             emula SEG segundos (pulsando teclas) y pausa
  omsx.py shot NOMBRE                  captura logs/omsx/NOMBRE.png
  omsx.py ram INICIO LONGITUD          vuelca RAM en hex

Teclas para run: left right up down space, separadas por comas (fila 8 de la matriz MSX).

El servidor atiende un comando por conexion. Las lineas <log> de openMSX van a LOG.
"""
import html, os, re, socket, subprocess, sys, threading, queue

HERE = os.path.dirname(os.path.abspath(__file__))
RUN = os.path.join(os.path.dirname(os.path.dirname(HERE)), 'logs', 'omsx')
SOCK = os.path.join(RUN, 'omsx.sock')
LOG = os.path.join(RUN, 'openmsx.log')
REPLY = re.compile(r'<reply result="(ok|nok)">(.*?)</reply>', re.S)


def serve(args):
    os.makedirs(RUN, exist_ok=True)
    if os.path.exists(SOCK):
        os.unlink(SOCK)
    p = subprocess.Popen(['openmsx', '-control', 'stdio', *args],
                         stdin=subprocess.PIPE, stdout=subprocess.PIPE, text=True, bufsize=1)
    p.stdin.write('<openmsx-control>\n')
    p.stdin.flush()
    replies = queue.Queue()

    def reader():
        buf = ''
        with open(LOG, 'a') as log:
            for line in p.stdout:
                buf += line
                while True:
                    m = REPLY.search(buf)
                    if not m:
                        break
                    replies.put((m.group(1), html.unescape(m.group(2))))
                    buf = buf[m.end():]
                if '<reply' not in buf:
                    log.write(buf)
                    log.flush()
                    buf = ''
        replies.put(('nok', 'openMSX ha terminado'))

    threading.Thread(target=reader, daemon=True).start()
    srv = socket.socket(socket.AF_UNIX)
    srv.bind(SOCK)
    srv.listen()
    print(f'openMSX pid {p.pid}; socket {SOCK}', flush=True)
    while p.poll() is None:
        conn, _ = srv.accept()
        with conn:
            cmd = b''
            while not cmd.endswith(b'\0'):
                chunk = conn.recv(65536)
                if not chunk:
                    break
                cmd += chunk
            p.stdin.write('<command>' + html.escape(cmd.rstrip(b'\0').decode()) + '</command>\n')
            p.stdin.flush()
            res, text = replies.get()
            conn.sendall((res + '\n' + text).encode())
    os.unlink(SOCK)


def send(cmd, tries=5):
    import time
    for i in range(tries):  # el servidor atiende de uno en uno: reintentar si la conexion falla
        try:
            c = socket.socket(socket.AF_UNIX)
            c.connect(SOCK)
            c.sendall(cmd.encode() + b'\0')
            c.shutdown(socket.SHUT_WR)
            break
        except OSError:
            c.close()
            if i == tries - 1:
                raise
            time.sleep(0.05)
    data = b''
    while chunk := c.recv(65536):
        data += chunk
    res, _, text = data.decode().partition('\n')
    return res == 'ok', text


KEYS = {'space': 0x01, 'left': 0x10, 'up': 0x20, 'down': 0x40, 'right': 0x80}


def run(secs, keys=''):
    import time
    mask = sum(KEYS[k] for k in keys.split(',') if k)
    up = f'keymatrixup 8 {mask}; ' if mask else ''
    send(f'after time {secs} {{{up}set pause on}}')
    if mask:
        send(f'keymatrixdown 8 {mask}')
    send('set pause off')
    while send('set pause')[1] != 'true':
        time.sleep(0.05)


if __name__ == '__main__':
    if sys.argv[1:2] == ['serve']:
        serve(sys.argv[2:])
    elif sys.argv[1] == 'run':
        run(float(sys.argv[2]), sys.argv[3] if len(sys.argv) > 3 else '')
        print(send('peek 0xE000')[1], send('peek 0xE001')[1])
    elif sys.argv[1] == 'shot':
        print(send(f'screenshot -raw {RUN}/{sys.argv[2]}.png')[1] or f'{RUN}/{sys.argv[2]}.png')
    elif sys.argv[1] == 'ram':
        a, n = int(sys.argv[2], 16), int(sys.argv[3], 16)
        print(send(f'binary encode hex [debug read_block memory {a} {n}]')[1])
    else:
        cmds = [open(sys.argv[2]).read()] if sys.argv[1] == '-f' else sys.argv[1:]
        ok_all = True
        for cmd in cmds:
            ok, text = send(cmd)
            ok_all &= ok
            print(text if ok else 'ERROR: ' + text)
        sys.exit(0 if ok_all else 1)
