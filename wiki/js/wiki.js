// Galious: The Hollow Kingdom — wiki de cómo se hizo. Vanilla JS: partículas, parallax, índice con scrollspy,
// visor de imágenes, comparador de colisión, pestañas y gráfico.
(function () {
  "use strict";
  document.documentElement.classList.remove("sin-js");
  const quieto = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const $ = (s, r = document) => r.querySelector(s);
  const $$ = (s, r = document) => Array.from(r.querySelectorAll(s));

  // ---------- Partículas del héroe (motas de luz que suben, como el polvo de los Cruces Olvidados) ----------
  const lienzo = $("#particulas");
  if (lienzo && lienzo.getContext && !quieto) {
    const ctx = lienzo.getContext("2d");
    let w = 0, h = 0, motas = [], activo = true, dpr = Math.min(window.devicePixelRatio || 1, 2);
    const crear = (inicio) => ({
      x: Math.random() * w, y: inicio ? Math.random() * h : h + 10,
      r: Math.random() * 1.8 + 0.4, v: Math.random() * 0.35 + 0.08,
      d: Math.random() * Math.PI * 2, a: Math.random() * 0.6 + 0.2, f: Math.random() * 0.02 + 0.004,
    });
    const medir = () => {
      const b = lienzo.getBoundingClientRect();
      w = b.width; h = b.height;
      lienzo.width = w * dpr; lienzo.height = h * dpr;
      ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
      const n = Math.round(Math.min(110, w * h / 14000));
      motas = Array.from({ length: n }, () => crear(true));
    };
    const pintar = () => {
      if (!activo) return;
      ctx.clearRect(0, 0, w, h);
      for (const m of motas) {
        m.y -= m.v; m.d += m.f; m.x += Math.sin(m.d) * 0.3;
        if (m.y < -10) Object.assign(m, crear(false));
        const brillo = m.a * (0.6 + 0.4 * Math.sin(m.d * 3));
        const g = ctx.createRadialGradient(m.x, m.y, 0, m.x, m.y, m.r * 4);
        g.addColorStop(0, `rgba(220,236,255,${brillo})`);
        g.addColorStop(1, "rgba(185,220,255,0)");
        ctx.fillStyle = g;
        ctx.beginPath(); ctx.arc(m.x, m.y, m.r * 4, 0, Math.PI * 2); ctx.fill();
      }
      requestAnimationFrame(pintar);
    };
    medir();
    window.addEventListener("resize", medir);
    new IntersectionObserver((e) => { const antes = activo; activo = e[0].isIntersecting; if (activo && !antes) pintar(); }).observe(lienzo);
    pintar();
  }

  // ---------- Parallax del héroe ----------
  const capas = $$(".heroe .capa");
  const heroe = $(".heroe");
  if (capas.length && !quieto) {
    let pend = false;
    const mover = () => {
      pend = false;
      const y = window.scrollY;
      if (y > heroe.offsetHeight) return;
      capas.forEach((c) => { const k = parseFloat(c.dataset.velocidad || "0.2"); c.style.transform = `translate3d(0, ${y * k}px, 0) scale(1.04)`; });
    };
    window.addEventListener("scroll", () => { if (!pend) { pend = true; requestAnimationFrame(mover); } }, { passive: true });
    mover();
  }

  // ---------- Barra, progreso, índice, volver arriba ----------
  const barra = $(".barra"), indice = $(".indice"), arriba = $(".arriba"), progreso = $(".barra .progreso");
  const alBajar = () => {
    const y = window.scrollY, lim = heroe ? heroe.offsetHeight * 0.6 : 200;
    barra.classList.toggle("visible", y > lim);
    indice.classList.toggle("visible", y > lim);
    arriba.classList.toggle("visible", y > 900);
    const total = document.documentElement.scrollHeight - window.innerHeight;
    progreso.style.width = (total > 0 ? (y / total) * 100 : 0) + "%";
  };
  window.addEventListener("scroll", alBajar, { passive: true });
  alBajar();
  arriba.addEventListener("click", () => window.scrollTo({ top: 0, behavior: quieto ? "auto" : "smooth" }));
  $(".boton-menu").addEventListener("click", () => {
    const abierto = document.body.classList.toggle("menu-abierto");
    $(".boton-menu").setAttribute("aria-expanded", abierto ? "true" : "false");
  });
  $$(".indice a").forEach((a) => a.addEventListener("click", () => {
    document.body.classList.remove("menu-abierto");
    $(".boton-menu").setAttribute("aria-expanded", "false");
  }));

  // Scrollspy: la sección cuya cabecera ha pasado el tercio superior de la pantalla.
  const enlaces = new Map($$(".indice a").map((a) => [a.getAttribute("href").slice(1), a]));
  const secciones = $$("section[id]").filter((s) => enlaces.has(s.id));
  const espiar = () => {
    let actual = secciones[0];
    const lim = window.innerHeight * 0.33;
    for (const s of secciones) if (s.getBoundingClientRect().top <= lim) actual = s;
    enlaces.forEach((a, id) => a.classList.toggle("activo", actual && id === actual.id));
  };
  window.addEventListener("scroll", espiar, { passive: true });
  espiar();

  // ---------- Aparición al bajar y barras del gráfico ----------
  const revelar = new IntersectionObserver((es) => es.forEach((e) => {
    if (!e.isIntersecting) return;
    e.target.classList.add("visto");
    $$(".valor[data-ancho]", e.target).forEach((v) => { v.style.width = v.dataset.ancho + "%"; });
    revelar.unobserve(e.target);
  }), { rootMargin: "0px 0px -8% 0px" });
  $$(".revelar").forEach((el) => revelar.observe(el));

  // ---------- Visor de imágenes ----------
  const visor = $(".visor"), vImg = $("img", visor), vPie = $("figcaption", visor), vCont = $(".contador", visor);
  let grupo = [], pos = 0, foco = null;
  const datos = (img) => ({
    src: img.dataset.grande || img.currentSrc || img.src,
    pie: (img.closest("figure") && $("figcaption", img.closest("figure")) ? $("figcaption", img.closest("figure")).textContent : img.alt) || "",
  });
  const mostrar = () => {
    const d = datos(grupo[pos]);
    vImg.src = d.src; vImg.alt = d.pie; vPie.textContent = d.pie;
    vCont.textContent = grupo.length > 1 ? `${pos + 1} / ${grupo.length}` : "";
    $(".ant", visor).hidden = $(".sig", visor).hidden = grupo.length < 2;
    // Precarga la siguiente.
    if (grupo.length > 1) { const s = new Image(); s.src = datos(grupo[(pos + 1) % grupo.length]).src; }
  };
  const abrir = (img) => {
    const cont = img.closest("[data-grupo]");
    grupo = cont ? $$("img.ampliable", cont).filter((i) => i.offsetParent !== null) : [img];
    pos = Math.max(0, grupo.indexOf(img));
    foco = document.activeElement;
    mostrar();
    visor.classList.add("abierto");
    document.body.style.overflow = "hidden";
    $(".cerrar", visor).focus();
  };
  const cerrar = () => { visor.classList.remove("abierto"); document.body.style.overflow = ""; vImg.removeAttribute("src"); if (foco) foco.focus(); };
  const paso = (d) => { pos = (pos + d + grupo.length) % grupo.length; mostrar(); };
  document.addEventListener("click", (e) => {
    const img = e.target.closest("img.ampliable");
    if (img) { e.preventDefault(); abrir(img); }
  });
  document.addEventListener("keydown", (e) => {
    if (e.target.matches && e.target.matches("img.ampliable") && (e.key === "Enter" || e.key === " ")) { e.preventDefault(); abrir(e.target); return; }
    if (!visor.classList.contains("abierto")) return;
    if (e.key === "Escape") cerrar();
    else if (e.key === "ArrowRight") paso(1);
    else if (e.key === "ArrowLeft") paso(-1);
  });
  $$("img.ampliable").forEach((i) => { i.tabIndex = 0; i.setAttribute("role", "button"); });
  $(".cerrar", visor).addEventListener("click", cerrar);
  $(".ant", visor).addEventListener("click", () => paso(-1));
  $(".sig", visor).addEventListener("click", () => paso(1));
  visor.addEventListener("click", (e) => { if (e.target === visor) cerrar(); });
  let toqueX = null;
  visor.addEventListener("touchstart", (e) => { toqueX = e.touches[0].clientX; }, { passive: true });
  visor.addEventListener("touchend", (e) => {
    if (toqueX === null) return;
    const dx = e.changedTouches[0].clientX - toqueX;
    if (Math.abs(dx) > 50 && grupo.length > 1) paso(dx < 0 ? 1 : -1);
    toqueX = null;
  });

  // ---------- Comparador antes / después ----------
  $$(".comparador").forEach((c) => {
    const r = $("input[type=range]", c), encima = $(".encima", c), linea = $(".linea", c), tir = $(".tirador", c);
    const poner = (v) => { encima.style.clipPath = `inset(0 0 0 ${v}%)`; linea.style.left = v + "%"; tir.style.left = v + "%"; };
    r.addEventListener("input", () => poner(r.value));
    poner(r.value);
  });
  const selCol = $("#selector-colision");
  if (selCol) {
    const c = $("#comparador-salas"), base = $(".base", c), encima = $(".encima", c), pie = $("#pie-comparador");
    selCol.addEventListener("click", (e) => {
      const b = e.target.closest("button");
      if (!b) return;
      $$("button", selCol).forEach((x) => x.setAttribute("aria-pressed", x === b ? "true" : "false"));
      base.src = `img/mundos/${b.dataset.sala}.jpg`;
      encima.src = `img/mundos/${b.dataset.sala}_colision.jpg`;
      base.alt = `Sala ${b.textContent} con el arte de Hollow Knight`;
      encima.alt = `La misma sala con la colisión del original marcada`;
      pie.textContent = b.dataset.pie;
    });
  }

  const selOri = $("#selector-original");
  if (selOri) {
    const c = $("#comparador-original"), base = $(".base", c), encima = $(".encima", c), pie = $("#pie-original");
    selOri.addEventListener("click", (e) => {
      const b = e.target.closest("button");
      if (!b) return;
      $$("button", selOri).forEach((x) => x.setAttribute("aria-pressed", x === b ? "true" : "false"));
      base.src = `img/comparar/${b.dataset.par}_original.png`;
      encima.src = `img/comparar/${b.dataset.par}_remake.jpg`;
      base.alt = `${b.textContent} en el original de MSX`;
      encima.alt = `${b.textContent} en el remake con el arte de Hollow Knight`;
      pie.textContent = b.dataset.pie;
    });
  }

  // ---------- Pestañas de los mundos ----------
  const pest = $("#pestanas-mundos");
  if (pest) {
    pest.addEventListener("click", (e) => {
      const b = e.target.closest("button");
      if (!b) return;
      $$("button", pest).forEach((x) => { const si = x === b; x.setAttribute("aria-selected", si ? "true" : "false"); x.tabIndex = si ? 0 : -1; });
      $$(".panel-mundo").forEach((p) => p.classList.toggle("activo", p.id === b.getAttribute("aria-controls")));
    });
    pest.addEventListener("keydown", (e) => {
      if (e.key !== "ArrowRight" && e.key !== "ArrowLeft") return;
      const bs = $$("button", pest), i = bs.indexOf(document.activeElement);
      if (i < 0) return;
      const n = bs[(i + (e.key === "ArrowRight" ? 1 : -1) + bs.length) % bs.length];
      n.focus(); n.click();
    });
  }
})();
