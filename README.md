<p align="center">
  <img src="assets/steam/cubierta_1840x860.png" alt="Galious: The Hollow Kingdom" width="100%">
</p>

# Galious: The Hollow Kingdom

Un remake en **Unity** de *Knightmare II: The Maze of Galious* (Konami, MSX, 1987) vestido con el arte, la música y la
sensación de **Hollow Knight** (Team Cherry, 2017). Es un **proyecto educativo**: aprender cómo está hecho un juego por
dentro, ingeniería inversa incluida, y rehacerlo en un motor moderno.

- **Fiel al original:** las mecánicas se reimplementan a partir de la documentación de la ROM (`docs/`), no traduciendo el
  código ensamblador: física del salto, enemigos, jefes, santuarios, objetos, mapas y salas son los del juego de 1987.
- **Mejorado donde hace falta:** cada cambio es una regla del remake (`RemakeRules.cs`) documentada en el capítulo 11 de
  la biblia de diseño: bancos y sombra al morir, mapas de Cornifer, amuletos, tajos arriba y abajo, nadar, el ambiente
  de cada zona…
- **Con el arte de Hollow Knight:** el Caballero, los enemigos y jefes, el escenario, la música y la interfaz.

📖 **Wiki de cómo se hizo:** la web de `wiki/` (GitHub Pages): arquitectura, Unity, texturas, ambiente, efectos, música,
rendimiento, builds, enemigos, jefes y mundos.

## Jugar

Las builds salen del CI y se suben a la [Release de cada versión](https://github.com/rldona/Galious/releases) (la última, *Latest*):

| Plataforma | Paquete | Cómo |
|---|---|---|
| **Windows** | `Galious-Windows.zip` | Descomprimir y abrir `Galious.exe` (SmartScreen: *Más información* → *Ejecutar de todas formas*). |
| **Linux / SteamOS** | `Galious-Linux.tar.gz` | Ver [Linux y Steam](#linux-y-steam), más abajo. |
| **macOS** | `Galious-macOS.tar.gz` | Descomprimir y abrir `Galious.app` (la primera vez, clic derecho → *Abrir*: no está firmada). |

### Linux y Steam

El juego es nativo de Linux (x86_64): no necesita Proton. Sirve igual para un PC con Linux que para SteamOS (Steam Deck,
Lenovo Legion Go…).

**1. Instalar**

En SteamOS, pasa al **modo escritorio** (botón Steam → *Apagar* → *Cambiar a escritorio*) y abre una terminal (Konsole).
Descarga `Galious-Linux.tar.gz` de la Release y descomprímelo en una carpeta tuya:

```bash
mkdir -p ~/Juegos/Galious && tar -xzf ~/Descargas/Galious-Linux.tar.gz -C ~/Juegos/Galious
```

Si el ejecutable no tiene permiso de ejecución (por ejemplo, si lo copiaste desde un USB en FAT o exFAT):

```bash
chmod +x ~/Juegos/Galious/Galious.x86_64
```

Para probarlo sin Steam: `~/Juegos/Galious/Galious.x86_64`. También queda `Galious.desktop` en la carpeta, con el nombre,
la descripción y el icono, por si quieres un acceso en el menú de aplicaciones.

**2. Añadirlo a Steam como un juego más**

1. Abre Steam (en SteamOS, el icono del escritorio).
2. **Juegos → Añadir un juego que no es de Steam a mi biblioteca…** → **Buscar…** → elige
   `~/Juegos/Galious/Galious.x86_64` → **Añadir programas seleccionados**.
3. En la biblioteca, clic derecho sobre el juego → **Propiedades…**:
   - Nombre: `Galious: The Hollow Kingdom`.
   - Icono: `~/Juegos/Galious/steam/icono_1024.png`.
   - En **Controlador**, déjalo con Steam Input activado (el mando de la Deck o la Legion Go se presenta como uno de Xbox).
     Si la vibración no va, prueba a desactivarlo para este juego.

**3. Ponerle sus imágenes en la biblioteca**

La carpeta `steam/` de la build trae el arte para la biblioteca:

| Imagen | Fichero | Dónde se pone |
|---|---|---|
| Portada vertical | `portada_600x900.png` | En la cuadrícula de la biblioteca: clic derecho sobre el juego → *Administrar* → *Establecer imagen personalizada* |
| Cubierta horizontal | `cubierta_920x430.png` | Igual, en la vista de *Recientes* (la portada ancha) |
| Fondo (héroe) | `heroe_1920x620.png` | En la página del juego: clic derecho sobre el fondo de arriba → *Establecer fondo personalizado* |
| Logo | `logo.png` | En la página del juego: clic derecho sobre el fondo → *Establecer logotipo personalizado* |
| Icono | `icono_1024.png` | En *Propiedades* (paso 2) |

El arte solo se puede cambiar desde el modo escritorio; luego se ve igual en el modo juego.

**4. Jugar**

Vuelve al **modo juego** (icono *Volver al modo juego* del escritorio). El juego aparece en la biblioteca, en la pestaña
**No de Steam**, y se lanza como cualquier otro: a pantalla completa con la resolución de la consola y con el mando.

La partida se guarda en `~/.config/unity3d/rldona/Galious - The Hollow Kingdom/`.

**Controles** (se cambian en *Opciones → Mando / Teclado*):

| Acción | Mando | Teclado |
|---|---|---|
| Moverse, apuntar arriba / abajo | stick izquierdo o cruceta | flechas |
| Saltar | A | Z |
| Atacar (con ↑, hacia arriba; con ↓ en el aire, hacia abajo) | X | Espacio |
| Concentrarse (curarse) | B | F |
| Arma secundaria | Y | M |
| Biblia | LB | Ctrl izquierdo |
| Escribir la palabra del jefe | RB | el teclado |
| Inventario | Select | I |
| Pausa | Start | Esc |

## Compilar y modificar

Es de código abierto para quien tenga acceso: se puede cambiar todo.

1. **Unity 6000.6.5f1** (Unity Hub) con los módulos *Mac*, *Windows Build Support (Mono)*, *Linux Build Support (Mono)*
   y *WebGL*.
2. Abrir `unity/MazeOfKnight`. Todo lo importado ya está en el repo (`Assets/Generated` desde la ROM y
   `Assets/Art/Resources/Hollow` desde Hollow Knight); para regenerarlo, `tools/` (ver `docs/unity.md`).
3. Escena `Assets/Scenes/Game.unity` → Play. El menú **Maze of Knight** compila para cada plataforma
   (`Builds/Galious.app`, `Builds/Linux`, `Builds/Windows`, `Builds/Web`).
4. Pruebas: *Window → General → Test Runner → EditMode* (o `-runTests -testPlatform EditMode`).

En línea de comandos (macOS):

```bash
/Applications/Unity/Hub/Editor/6000.6.5f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath unity/MazeOfKnight -executeMethod MazeOfKnight.EditorTools.GameBuild.BuildMac
```

Cada regla del remake se puede apagar en `Assets/Scripts/Runtime/Game/RemakeRules.cs` para jugar como el original.

### CI

- **Pruebas** (`.github/workflows/tests.yml`): en cada push, con [GameCI](https://game.ci).
- **Builds** (`.github/workflows/builds.yml`): a mano o con una etiqueta `v*`; Windows, Linux y macOS en servidores
  Linux, subidas a la Release. La versión web se compila a mano en Unity (*Maze of Knight → Compilar para la web*).
- **Wiki** (`.github/workflows/wiki.yml`): publica `wiki/` en GitHub Pages.

Necesitan tres secretos del repo (*Settings → Secrets and variables → Actions*): `UNITY_LICENSE` (el contenido de
`Unity_lic.ulf`), `UNITY_EMAIL` y `UNITY_PASSWORD`.

## Estructura

| Carpeta | Qué hay |
|---|---|
| `unity/MazeOfKnight` | El proyecto de Unity: `Assets/Scripts/Runtime` (el juego), `Assets/Scripts/Editor` (importadores y builds), `Assets/Tests` |
| `docs/` | Ingeniería inversa del original (física, enemigos, jefes, mapas, música) y la biblia de diseño del remake |
| `tools/` | Extracción de la ROM (Python), importadores del arte de Hollow Knight (`tools/hollow`) |
| `rom/`, `extracted/` | La ROM y lo extraído de ella |
| `assets/` | Portadas, cubierta, icono y arte para Steam |
| `wiki/` | La web de cómo se hizo |

## Créditos

- *Knightmare II: The Maze of Galious* © Konami 1987.
- *Hollow Knight* © Team Cherry 2017: arte, música y personajes.
- Remake, herramientas y documentación: [rldona](https://github.com/rldona).
