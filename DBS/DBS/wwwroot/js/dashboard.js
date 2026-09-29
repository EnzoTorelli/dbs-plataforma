/* =====================================================================
   Dashboard — gráficos em SVG/HTML puro (sem biblioteca externa)
   - Receita por dia: linha + área, crosshair e tooltip, teclado ← →
   - Pedidos por status: barra segmentada + lista com ícone e texto
   - Mais vendidos: barras horizontais com valor na ponta
   - Sparkline no card de receita
   Redesenha sozinho ao trocar tema escuro/contraste/fonte e ao redimensionar.
   ===================================================================== */
(function () {
    'use strict';

    const raiz = document.querySelector('.dash');
    if (!raiz) return;

    // ---------- Formatação ----------
    const brl = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });
    const brlCompacto = new Intl.NumberFormat('pt-BR', {
        style: 'currency', currency: 'BRL', notation: 'compact', maximumFractionDigits: 1
    });
    const inteiro = new Intl.NumberFormat('pt-BR');
    const pct = new Intl.NumberFormat('pt-BR', { style: 'percent', maximumFractionDigits: 0 });

    const PERIODOS = { '7': 'Últimos 7 dias', '30': 'Últimos 30 dias', '90': 'Últimos 90 dias', 'mes': 'Mês atual' };
    const NS = 'http://www.w3.org/2000/svg';

    const ICONES = {
        pago: '<svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M3 8.5l3 3 7-7"/></svg>',
        pendente: '<svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2.2"><circle cx="8" cy="8" r="6"/><path d="M8 5v3.5l2 1.5"/></svg>',
        cancelado: '<svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M4 4l8 8M12 4l-8 8"/></svg>',
        outro: '<svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2.2"><circle cx="8" cy="8" r="2"/></svg>'
    };

    function cor(nome) {
        return getComputedStyle(raiz).getPropertyValue(nome).trim();
    }

    function svgEl(tag, attrs, pai) {
        const e = document.createElementNS(NS, tag);
        for (const k in attrs) e.setAttribute(k, attrs[k]);
        if (pai) pai.appendChild(e);
        return e;
    }

    function escapar(s) {
        return String(s ?? '').replace(/[&<>"']/g, c =>
            ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    }

    // "2026-09-28T00:00:00" → Date local (sem deslocar o dia por fuso)
    function parseDia(s) {
        const [a, m, d] = String(s).substring(0, 10).split('-').map(Number);
        return new Date(a, m - 1, d);
    }
    const fmtDiaCurto = d => d.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });
    const fmtDiaLongo = d => {
        const s = d.toLocaleDateString('pt-BR', { weekday: 'long', day: '2-digit', month: 'long' });
        return s.charAt(0).toUpperCase() + s.slice(1); // "Quarta-feira, 16 de setembro"
    };

    // Escala "bonita" para o eixo Y (0, 2.500, 5.000...)
    function escala(max) {
        if (!(max > 0)) return { max: 1000, passo: 250 };
        const bruto = max / 4;
        const mag = Math.pow(10, Math.floor(Math.log10(bruto)));
        const n = bruto / mag;
        const passo = (n <= 1 ? 1 : n <= 2 ? 2 : n <= 2.5 ? 2.5 : n <= 5 ? 5 : 10) * mag;
        return { max: Math.ceil(max / passo) * passo, passo };
    }

    // ---------- Tooltip (um por área de gráfico) ----------
    function criarTooltip(area) {
        const el = area.querySelector('.viz-tooltip');
        return {
            mostrar(html, x, y) {
                if (!el) return;
                el.innerHTML = html;
                el.classList.add('visivel');
                const larg = el.offsetWidth, alt = el.offsetHeight, max = area.clientWidth;
                let esq = x + 14;
                if (esq + larg > max) esq = x - larg - 14;
                if (esq < 0) esq = Math.max(0, Math.min(max - larg, x - larg / 2));
                let topo = y - alt - 12;
                if (topo < 0) topo = y + 16;
                el.style.transform = `translate(${Math.round(esq)}px, ${Math.round(topo)}px)`;
            },
            esconder() { el && el.classList.remove('visivel'); }
        };
    }

    function mostrarVazio(cont, texto) {
        cont.innerHTML = `<div class="vazio">${escapar(texto)}</div>`;
    }

    // =================================================================
    // Gráfico de linha — receita por dia
    // =================================================================
    function graficoReceita(cont, pontos) {
        const area = cont.closest('.grafico-area');
        const tt = criarTooltip(area);
        cont.innerHTML = '';
        cont.onkeydown = cont.onfocus = cont.onblur = null;

        if (!pontos.length || pontos.every(p => p.receita === 0)) {
            mostrarVazio(cont, 'Nenhuma venda paga neste período.');
            return;
        }

        const W = cont.clientWidth, H = cont.clientHeight || 260;
        const m = { t: 20, r: 16, b: 28, l: 68 };
        const iw = Math.max(10, W - m.l - m.r), ih = H - m.t - m.b;
        const n = pontos.length;

        const serie = cor('--viz-serie');
        const superficie = cor('--cor-superficie') || '#fff';
        const grade = cor('--cor-borda') || '#e1e0d9';
        const eixo = cor('--cor-texto-fraco') || '#898781';

        const maxValor = Math.max(...pontos.map(p => p.receita));
        const esc = escala(maxValor);
        const x = i => m.l + (n === 1 ? iw / 2 : (i * iw) / (n - 1));
        const y = v => m.t + ih - (v / esc.max) * ih;

        const total = pontos.reduce((s, p) => s + p.receita, 0);
        const svg = svgEl('svg', {
            width: W, height: H, viewBox: `0 0 ${W} ${H}`, role: 'img',
            'aria-label': `Receita por dia: ${brl.format(total)} no período, máximo de ${brl.format(maxValor)} em um dia.`
        }, cont);

        // Grade horizontal + rótulos do eixo Y
        for (let v = 0; v <= esc.max + 1e-9; v += esc.passo) {
            svgEl('line', {
                x1: m.l, x2: W - m.r, y1: Math.round(y(v)) + .5, y2: Math.round(y(v)) + .5,
                stroke: v === 0 ? eixo : grade, 'stroke-opacity': v === 0 ? .5 : 1, 'stroke-width': 1
            }, svg);
            const t = svgEl('text', { x: m.l - 10, y: y(v) + 4, 'text-anchor': 'end', class: 'viz-eixo-texto' }, svg);
            t.textContent = brlCompacto.format(v);
        }

        // Rótulos do eixo X: 1 a cada ~70px (máx. 7), sempre incluindo o último dia
        const maxRotulos = Math.max(2, Math.min(7, Math.floor(iw / 70)));
        const passoX = Math.max(1, Math.ceil(n / maxRotulos));
        pontos.forEach((p, i) => {
            if ((n - 1 - i) % passoX !== 0) return;
            const px = x(i);
            const ancora = px - m.l < 18 ? 'start' : (W - m.r - px < 18 ? 'end' : 'middle');
            const t = svgEl('text', { x: px, y: H - 8, 'text-anchor': ancora, class: 'viz-eixo-texto' }, svg);
            t.textContent = fmtDiaCurto(p.data);
        });

        // Área (10%) + linha (2px)
        const caminho = pontos.map((p, i) => `${i ? 'L' : 'M'}${x(i).toFixed(1)},${y(p.receita).toFixed(1)}`).join('');
        svgEl('path', {
            d: `${caminho}L${x(n - 1).toFixed(1)},${y(0)}L${x(0).toFixed(1)},${y(0)}Z`,
            fill: serie, 'fill-opacity': .1
        }, svg);
        svgEl('path', {
            d: caminho, fill: 'none', stroke: serie, 'stroke-width': 2,
            'stroke-linejoin': 'round', 'stroke-linecap': 'round'
        }, svg);

        // Rótulo seletivo: só o melhor dia
        const iMax = pontos.findIndex(p => p.receita === maxValor);
        svgEl('circle', { cx: x(iMax), cy: y(maxValor), r: 4, fill: serie, stroke: superficie, 'stroke-width': 2 }, svg);
        const rotulo = svgEl('text', {
            x: x(iMax), y: Math.max(12, y(maxValor) - 10), class: 'viz-rotulo',
            'text-anchor': x(iMax) - m.l < 50 ? 'start' : (W - m.r - x(iMax) < 50 ? 'end' : 'middle')
        }, svg);
        rotulo.textContent = brlCompacto.format(maxValor);

        // Ponto final
        if (iMax !== n - 1) {
            svgEl('circle', { cx: x(n - 1), cy: y(pontos[n - 1].receita), r: 4, fill: serie, stroke: superficie, 'stroke-width': 2 }, svg);
        }

        // Camada de hover: guia vertical + ponto destacado
        const guia = svgEl('line', { y1: m.t, y2: m.t + ih, stroke: eixo, 'stroke-width': 1, visibility: 'hidden' }, svg);
        const foco = svgEl('circle', { r: 5, fill: serie, stroke: superficie, 'stroke-width': 2, visibility: 'hidden' }, svg);
        const alvo = svgEl('rect', { x: m.l - 8, y: 0, width: iw + 16, height: H, fill: 'transparent' }, svg);

        let atual = -1;
        function mostrar(i) {
            i = Math.max(0, Math.min(n - 1, i));
            atual = i;
            const p = pontos[i], px = x(i), py = y(p.receita);
            guia.setAttribute('x1', px); guia.setAttribute('x2', px); guia.setAttribute('visibility', 'visible');
            foco.setAttribute('cx', px); foco.setAttribute('cy', py); foco.setAttribute('visibility', 'visible');
            tt.mostrar(
                `<div class="tt-titulo">${escapar(fmtDiaLongo(p.data))}</div>
                 <div class="tt-linha"><span class="tt-chave"><span class="tt-cor"></span>Receita</span><strong>${brl.format(p.receita)}</strong></div>
                 <div class="tt-linha"><span class="tt-chave">Pedidos pagos</span><strong>${inteiro.format(p.pedidos)}</strong></div>`,
                px, py);
        }
        function esconder() {
            atual = -1;
            guia.setAttribute('visibility', 'hidden');
            foco.setAttribute('visibility', 'hidden');
            tt.esconder();
        }

        alvo.addEventListener('pointermove', e => {
            const r = svg.getBoundingClientRect();
            const px = e.clientX - r.left;
            mostrar(n === 1 ? 0 : Math.round(((px - m.l) / iw) * (n - 1)));
        });
        alvo.addEventListener('pointerleave', esconder);

        // Teclado: ← → percorrem os dias; Home/End vão às pontas
        cont.onfocus = () => mostrar(atual < 0 ? n - 1 : atual);
        cont.onblur = esconder;
        cont.onkeydown = e => {
            const mapa = { ArrowLeft: -1, ArrowRight: 1 };
            if (e.key in mapa) { mostrar((atual < 0 ? n - 1 : atual) + mapa[e.key]); e.preventDefault(); }
            else if (e.key === 'Home') { mostrar(0); e.preventDefault(); }
            else if (e.key === 'End') { mostrar(n - 1); e.preventDefault(); }
            else if (e.key === 'Escape') esconder();
        };
    }

    function tabelaReceita(tbody, pontos) {
        tbody.innerHTML = pontos.slice().reverse().map(p =>
            `<tr><td>${escapar(p.data.toLocaleDateString('pt-BR'))}</td>
                 <td class="num">${brl.format(p.receita)}</td>
                 <td class="num">${inteiro.format(p.pedidos)}</td></tr>`).join('');
    }

    // =================================================================
    // Pedidos por status — barra segmentada + lista
    // =================================================================
    function classeStatus(s) {
        const k = String(s || '').trim().toLowerCase();
        return k === 'pago' || k === 'pendente' || k === 'cancelado' ? k : 'outro';
    }
    const COR_STATUS = { pago: '--viz-bom', pendente: '--viz-alerta', cancelado: '--viz-critico', outro: '--viz-neutro' };
    const ORDEM_STATUS = { pago: 0, pendente: 1, cancelado: 2, outro: 3 };

    function graficoStatus(cont, itens) {
        const total = itens.reduce((s, i) => s + i.quantidade, 0);
        if (!total) {
            mostrarVazio(cont, 'Nenhum pedido neste período.');
            return;
        }

        const lista = itens.slice().sort((a, b) =>
            ORDEM_STATUS[classeStatus(a.status)] - ORDEM_STATUS[classeStatus(b.status)] || b.quantidade - a.quantidade);

        const barra = lista.map(i => {
            const c = classeStatus(i.status);
            return `<span style="flex: ${i.quantidade} 1 0; background: var(${COR_STATUS[c]})"
                          title="${escapar(i.status)}: ${inteiro.format(i.quantidade)}"></span>`;
        }).join('');

        const linhas = lista.map(i => {
            const c = classeStatus(i.status);
            return `<li>
                <span class="status-icone" style="background: var(${COR_STATUS[c]})" aria-hidden="true">${ICONES[c]}</span>
                <span>${escapar(i.status)}</span>
                <span class="status-num">
                    <strong>${inteiro.format(i.quantidade)}</strong> · ${pct.format(i.quantidade / total)}
                    <small>${brl.format(i.valor)}</small>
                </span>
            </li>`;
        }).join('');

        cont.innerHTML = `<div class="status-barra" role="img"
                              aria-label="${escapar(lista.map(i => `${i.status}: ${i.quantidade}`).join(', '))}">${barra}</div>
                          <ul class="status-lista">${linhas}</ul>`;
    }

    // =================================================================
    // Mais vendidos — barras horizontais
    // =================================================================
    function graficoTop(cont, itens) {
        const area = cont.closest('.grafico-area');
        const tt = criarTooltip(area);

        if (!itens.length) {
            mostrarVazio(cont, 'Nenhum produto vendido neste período.');
            return;
        }

        const max = Math.max(...itens.map(i => i.quantidade));
        cont.innerHTML = itens.map((it, idx) => `
            <div class="top-linha" tabindex="0" data-i="${idx}"
                 aria-label="${escapar(it.nome)}: ${inteiro.format(it.quantidade)} unidades, ${escapar(brl.format(it.receita))}">
                <span class="top-nome">${escapar(it.nome)}</span>
                <span class="top-trilho"><span class="top-barra" style="width: ${Math.max(2, (it.quantidade / max) * 100)}%"></span></span>
                <span class="top-valor">${inteiro.format(it.quantidade)} un.</span>
            </div>`).join('');

        cont.querySelectorAll('.top-linha').forEach(linha => {
            const it = itens[Number(linha.dataset.i)];
            const mostrar = () => {
                const rA = area.getBoundingClientRect();
                const rB = linha.querySelector('.top-barra').getBoundingClientRect();
                tt.mostrar(
                    `<div class="tt-titulo" style="text-transform:none">${escapar(it.nome)}</div>
                     <div class="tt-linha"><span class="tt-chave"><span class="tt-cor"></span>Unidades</span><strong>${inteiro.format(it.quantidade)}</strong></div>
                     <div class="tt-linha"><span class="tt-chave">Receita</span><strong>${brl.format(it.receita)}</strong></div>`,
                    rB.right - rA.left, rB.top - rA.top);
            };
            linha.addEventListener('pointerenter', mostrar);
            linha.addEventListener('focus', mostrar);
            linha.addEventListener('pointerleave', () => tt.esconder());
            linha.addEventListener('blur', () => tt.esconder());
        });
    }

    // =================================================================
    // Sparkline do card de receita
    // =================================================================
    function sparkline(cont, pontos) {
        cont.innerHTML = '';
        if (pontos.length < 2 || pontos.every(p => p.receita === 0)) return;

        const W = cont.clientWidth, H = cont.clientHeight || 36, pad = 4;
        const max = Math.max(...pontos.map(p => p.receita)) || 1;
        const n = pontos.length;
        const x = i => pad + (i * (W - pad * 2)) / (n - 1);
        const y = v => H - pad - (v / max) * (H - pad * 2);
        const serie = cor('--viz-serie');

        const svg = svgEl('svg', { width: W, height: H, viewBox: `0 0 ${W} ${H}` }, cont);
        const d = pontos.map((p, i) => `${i ? 'L' : 'M'}${x(i).toFixed(1)},${y(p.receita).toFixed(1)}`).join('');
        svgEl('path', { d: `${d}L${x(n - 1)},${H - pad}L${x(0)},${H - pad}Z`, fill: serie, 'fill-opacity': .1 }, svg);
        svgEl('path', { d, fill: 'none', stroke: serie, 'stroke-width': 2, 'stroke-linejoin': 'round', 'stroke-linecap': 'round' }, svg);
        svgEl('circle', {
            cx: x(n - 1), cy: y(pontos[n - 1].receita), r: 3.5,
            fill: serie, stroke: cor('--cor-superficie') || '#fff', 'stroke-width': 2
        }, svg);
    }

    // =================================================================
    // Estado, carregamento e eventos
    // =================================================================
    const el = {
        receita: document.getElementById('grafico-receita'),
        tabela: document.getElementById('tabela-receita'),
        resumoReceita: document.getElementById('resumo-receita'),
        status: document.getElementById('grafico-status'),
        resumoStatus: document.getElementById('resumo-status'),
        top: document.getElementById('grafico-top'),
        resumoTop: document.getElementById('resumo-top'),
        spark: document.getElementById('sparkline-receita'),
        botoes: document.querySelectorAll('.periodo-btn')
    };

    let dados = null;
    let spark = [];
    let requisicao = 0;

    try {
        const bruto = JSON.parse(document.getElementById('dados-sparkline')?.textContent || '[]');
        spark = bruto.map(p => ({ data: parseDia(p.dia), receita: Number(p.receita), pedidos: Number(p.pedidos) }));
    } catch (_) { spark = []; }

    function desenharTudo() {
        if (el.spark) sparkline(el.spark, spark);
        if (!dados) return;
        graficoReceita(el.receita, dados.serie);
        graficoStatus(el.status, dados.status);
        graficoTop(el.top, dados.topProdutos);
    }

    function atualizarResumos() {
        const rotulo = PERIODOS[dados.periodo] || '';
        const total = dados.serie.reduce((s, p) => s + p.receita, 0);
        const pedidos = dados.serie.reduce((s, p) => s + p.pedidos, 0);
        el.resumoReceita.innerHTML =
            `<strong>${brl.format(total)}</strong> <span class="sub">· ${inteiro.format(pedidos)} ${pedidos === 1 ? 'pedido pago' : 'pedidos pagos'} · ${escapar(rotulo.toLowerCase())}</span>`;

        const totalPedidos = dados.status.reduce((s, i) => s + i.quantidade, 0);
        el.resumoStatus.innerHTML =
            `<strong>${inteiro.format(totalPedidos)}</strong> <span class="sub">${totalPedidos === 1 ? 'pedido' : 'pedidos'} · ${escapar(rotulo.toLowerCase())}</span>`;

        el.resumoTop.innerHTML = `<span class="sub">Por unidades vendidas · ${escapar(rotulo.toLowerCase())}</span>`;
    }

    function marcarPeriodo(p) {
        el.botoes.forEach(b => b.setAttribute('aria-checked', String(b.dataset.periodo === p)));
    }

    function carregando(sim) {
        [el.receita, el.status, el.top].forEach(c => c && c.closest('.painel').classList.toggle('carregando', sim));
    }

    async function carregar(periodo) {
        const minha = ++requisicao;
        marcarPeriodo(periodo);
        carregando(true);

        try {
            const resp = await fetch(`/Dashboard/Dados?periodo=${encodeURIComponent(periodo)}`, {
                headers: { Accept: 'application/json' },
                credentials: 'same-origin'
            });
            if (resp.status === 401) { location.href = '/Login'; return; }
            if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
            const json = await resp.json();
            if (minha !== requisicao) return; // chegou uma resposta mais nova

            dados = {
                periodo: json.periodo,
                serie: json.serie.map(p => ({ data: parseDia(p.dia), receita: Number(p.receita), pedidos: Number(p.pedidos) })),
                status: json.status.map(s => ({ status: s.status, quantidade: Number(s.quantidade), valor: Number(s.valor) })),
                topProdutos: json.topProdutos.map(t => ({ nome: t.nome, quantidade: Number(t.quantidade), receita: Number(t.receita) }))
            };
            atualizarResumos();
            tabelaReceita(el.tabela, dados.serie);
            desenharTudo();
        } catch (e) {
            if (minha !== requisicao) return;
            const msg = `<div class="vazio">Não foi possível carregar os dados. <button type="button" data-tentar>Tentar de novo</button></div>`;
            [el.receita, el.status, el.top].forEach(c => { c.innerHTML = msg; });
            raiz.querySelectorAll('[data-tentar]').forEach(b => b.addEventListener('click', () => carregar(periodo)));
            console.error('Dashboard:', e);
        } finally {
            if (minha === requisicao) carregando(false);
        }
    }

    // Período: clique, setas dentro do grupo, e lembra a última escolha
    el.botoes.forEach((b, i) => {
        b.addEventListener('click', () => {
            try { localStorage.setItem('dash_periodo', b.dataset.periodo); } catch (_) { }
            carregar(b.dataset.periodo);
        });
        b.addEventListener('keydown', e => {
            const d = e.key === 'ArrowRight' ? 1 : e.key === 'ArrowLeft' ? -1 : 0;
            if (!d) return;
            const prox = el.botoes[(i + d + el.botoes.length) % el.botoes.length];
            prox.focus(); prox.click(); e.preventDefault();
        });
    });

    // Redesenha ao trocar tema/contraste/fonte pela barra de acessibilidade
    new MutationObserver(desenharTudo).observe(document.documentElement, {
        attributes: true, attributeFilter: ['data-tema', 'data-contraste', 'data-fonte']
    });

    // Redesenha ao redimensionar (com debounce)
    let timer;
    const aoRedimensionar = () => { clearTimeout(timer); timer = setTimeout(desenharTudo, 120); };
    if ('ResizeObserver' in window) new ResizeObserver(aoRedimensionar).observe(el.receita);
    else window.addEventListener('resize', aoRedimensionar);

    // Últimas ordens: abre/fecha a lista de produtos de cada pedido
    raiz.querySelectorAll('[data-alternar-itens]').forEach(btn => {
        btn.addEventListener('click', () => {
            const linha = document.getElementById(btn.dataset.alternarItens);
            if (!linha) return;
            const abrir = linha.hidden;
            linha.hidden = !abrir;
            btn.setAttribute('aria-expanded', String(abrir));
        });
    });

    let inicial = '30';
    try {
        const salvo = localStorage.getItem('dash_periodo');
        if (salvo && PERIODOS[salvo]) inicial = salvo;
    } catch (_) { }

    if (el.spark) sparkline(el.spark, spark);
    carregar(inicial);
})();
