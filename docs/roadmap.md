# Roadmap: Galious en GitHub

Repo **privado** `rldona/Galious` (decisión del usuario, 17 de octubre de 2026): lleva todo, también la ROM, lo importado de
ella y el arte de Hollow Knight, para que el CI compile el juego completo. Solo lo ve el usuario.

| # | Tarea | Qué incluye | Estado |
|---|-------|-------------|--------|
| 1 | **Repo privado `rldona/Galious`** | Quitar del `.gitignore` la ROM (`rom/`, `extracted/`), `Assets/Generated` y `Assets/Art/Resources/Hollow`; crear el repo privado y subir la historia entera. | |
| 2 | **Nombre, descripción e iconos** | `productName` «Galious: The Hollow Kingdom», empresa, versión; icono de la aplicación (macOS, Windows y Linux) desde el emblema; portada vertical (600 × 900), cubierta horizontal (920 × 430), héroe (3840 × 1240) y logo para la biblioteca de Steam/SteamOS, en `assets/steam` y junto a cada build. | |
| 3 | **Golpe del Caballero** | La caja de la espada, del alto y ancho del tajo dibujado (el halo de la textura), en los cuatro sentidos: menos frustrante dar a piedras, enemigos y jefes. Regla del remake, con test. | |
| 4 | **Jefes más fáciles de golpear** | Más zonas que reciben golpes: el False Knight también en la maza; revisar los demás jefes contra su dibujo de Hollow Knight. | |
| 5 | **CI (GitHub Actions + GameCI)** | Compila Windows, Linux, macOS y WebGL en cada push a `main` y a mano; los tests EditMode antes. Necesita los secretos `UNITY_EMAIL`, `UNITY_PASSWORD` y `UNITY_LICENSE` (los mete el usuario). Las builds quedan como artefactos del repo (privados). | |
| 6 | **Wiki en GitHub Pages** | Web didáctica de cómo se hizo: arquitectura, Unity, texturas, ambiente, efectos, música, rendimiento, builds, enemigos, jefes, mundos y salas con capturas. Se publica con Actions. Ojo: GitHub Pages en un repo privado necesita GitHub Pro y la web es **pública** para quien tenga el enlace. | |
| 7 | **README** | Portada, la idea del proyecto, cómo instalarlo, probarlo y modificarlo. | |
