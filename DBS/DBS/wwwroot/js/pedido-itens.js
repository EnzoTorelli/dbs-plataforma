/* =====================================================================
   Itens do pedido (Vendas e Ordens)
   - Abre/fecha a linha de detalhe ao clicar no resumo dos itens
   - filtrarPedidos(): busca por texto (cliente, nº, produto) + status,
     escondendo junto a linha de detalhe do pedido
   ===================================================================== */
(function () {
    'use strict';

    document.addEventListener('click', function (e) {
        const btn = e.target.closest('[data-alternar-itens]');
        if (!btn) return;
        const linha = document.getElementById(btn.dataset.alternarItens);
        if (!linha) return;
        const abrir = linha.hidden;
        linha.hidden = !abrir;
        btn.setAttribute('aria-expanded', String(abrir));
    });

    /**
     * @param {string} tabelaId  id da <table>
     * @param {string} termo     texto digitado na busca
     * @param {string} status    '' (todos), 'pago', 'pendente', 'cancelado'
     */
    window.filtrarPedidos = function (tabelaId, termo, status) {
        termo = (termo || '').trim().toLowerCase();
        status = (status || '').toLowerCase();

        document.querySelectorAll(`#${tabelaId} tbody tr.linha-pedido`).forEach(function (linha) {
            const texto = linha.textContent.toLowerCase() + ' ' + (linha.dataset.itens || '');
            const mostrar = (!termo || texto.includes(termo)) &&
                            (!status || linha.dataset.status === status);

            linha.style.display = mostrar ? '' : 'none';

            const detalhe = linha.nextElementSibling;
            if (detalhe && detalhe.classList.contains('itens-detalhe'))
                detalhe.style.display = mostrar ? '' : 'none';
        });
    };
})();
