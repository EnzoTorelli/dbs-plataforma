using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DBS.Controllers
{
    public class AnalyticsController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Analytics";
            ViewData["Pagina"] = "Analytics";

            var modelo = AnalyticsService.GerarRelatorio();
            return View(modelo);
        }
    }

    // =========================================================
    // MODELOS DE DADOS
    // =========================================================

    public class VendaMensal
    {
        public int Mes { get; set; }
        public string NomeMes { get; set; } = string.Empty;
        public decimal Receita { get; set; }
        public int QtdVendas { get; set; }
    }

    public class ClienteRanking
    {
        public string Nome { get; set; } = string.Empty;
        public decimal TotalGasto { get; set; }
        public int QtdCompras { get; set; }
        public string Classificacao { get; set; } = string.Empty; // "VIP", "Ativo", "Inativo"
    }

    public class ProdutoRanking
    {
        public string Nome { get; set; } = string.Empty;
        public int QtdVendida { get; set; }
        public decimal ReceitaGerada { get; set; }
    }

    public class PrevisaoReceita
    {
        public string Mes { get; set; } = string.Empty;
        public decimal ValorPrevisto { get; set; }
        public bool EPrevisao { get; set; }
    }

    public class AnalyticsViewModel
    {
        // KPIs principais
        public decimal ReceitaTotalSemestre { get; set; }
        public decimal MediaMensalReceita { get; set; }
        public decimal TicketMedio { get; set; }
        public int TotalVendasSemestre { get; set; }

        // Variação mês a mês
        public decimal VariacaoUltimoMes { get; set; }

        // Listas para gráficos e tabelas
        public List<VendaMensal> VendasMensais { get; set; } = new();
        public List<ClienteRanking> RankingClientes { get; set; } = new();
        public List<ProdutoRanking> RankingProdutos { get; set; } = new();
        public List<PrevisaoReceita> PrevisaoProximoMes { get; set; } = new();

        // Regressão linear
        public decimal PrevisaoMes7 { get; set; }
        public double CoeficienteAngular { get; set; } // "tendência de crescimento"
        public string InterpretacaoTendencia { get; set; } = string.Empty;

        // Segmentação de clientes
        public int ClientesVip { get; set; }
        public int ClientesAtivos { get; set; }
        public int ClientesInativos { get; set; }
    }

    // =========================================================
    // SERVIÇO DE ANALYTICS — toda a lógica de ML e estatística
    // =========================================================

    public static class AnalyticsService
    {
        // Dados mockados representando 6 meses de operação do DBS ERP
        private static readonly List<VendaMensal> _vendasHistoricas = new()
        {
            new VendaMensal { Mes = 1, NomeMes = "Outubro/24",  Receita = 32400m, QtdVendas = 87  },
            new VendaMensal { Mes = 2, NomeMes = "Novembro/24", Receita = 38750m, QtdVendas = 102 },
            new VendaMensal { Mes = 3, NomeMes = "Dezembro/24", Receita = 51200m, QtdVendas = 134 },
            new VendaMensal { Mes = 4, NomeMes = "Janeiro/25",  Receita = 41600m, QtdVendas = 109 },
            new VendaMensal { Mes = 5, NomeMes = "Fevereiro/25",Receita = 44900m, QtdVendas = 118 },
            new VendaMensal { Mes = 6, NomeMes = "Março/25",    Receita = 48320m, QtdVendas = 127 },
        };

        private static readonly List<ClienteRanking> _clientes = new()
        {
            new ClienteRanking { Nome = "Maria Silva",     TotalGasto = 12400m, QtdCompras = 18 },
            new ClienteRanking { Nome = "João Pereira",    TotalGasto = 9870m,  QtdCompras = 14 },
            new ClienteRanking { Nome = "Ana Costa",       TotalGasto = 8650m,  QtdCompras = 12 },
            new ClienteRanking { Nome = "Carlos Souza",    TotalGasto = 6200m,  QtdCompras = 9  },
            new ClienteRanking { Nome = "Fernanda Lima",   TotalGasto = 5900m,  QtdCompras = 8  },
            new ClienteRanking { Nome = "Pedro Alves",     TotalGasto = 4300m,  QtdCompras = 6  },
            new ClienteRanking { Nome = "Luciana Ramos",   TotalGasto = 3100m,  QtdCompras = 5  },
            new ClienteRanking { Nome = "Roberto Mendes",  TotalGasto = 1800m,  QtdCompras = 3  },
            new ClienteRanking { Nome = "Patrícia Nunes",  TotalGasto = 950m,   QtdCompras = 2  },
            new ClienteRanking { Nome = "Thiago Barbosa",  TotalGasto = 400m,   QtdCompras = 1  },
        };

        private static readonly List<ProdutoRanking> _produtos = new()
        {
            new ProdutoRanking { Nome = "Notebook Pro 15",     QtdVendida = 43, ReceitaGerada = 21500m },
            new ProdutoRanking { Nome = "Monitor Ultrawide",   QtdVendida = 37, ReceitaGerada = 14800m },
            new ProdutoRanking { Nome = "Teclado Mecânico",    QtdVendida = 89, ReceitaGerada = 10680m },
            new ProdutoRanking { Nome = "Headset Gamer",       QtdVendida = 61, ReceitaGerada = 9150m  },
            new ProdutoRanking { Nome = "Mouse Sem Fio",       QtdVendida = 74, ReceitaGerada = 7400m  },
        };

        public static AnalyticsViewModel GerarRelatorio()
        {
            // --- KPIs básicos ---
            decimal receitaTotal = _vendasHistoricas.Sum(v => v.Receita);
            decimal mediaReceita = receitaTotal / _vendasHistoricas.Count;
            int totalVendas = _vendasHistoricas.Sum(v => v.QtdVendas);
            decimal ticketMedio = receitaTotal / totalVendas;

            decimal receitaMes5 = _vendasHistoricas[4].Receita;
            decimal receitaMes6 = _vendasHistoricas[5].Receita;
            decimal variacao = ((receitaMes6 - receitaMes5) / receitaMes5) * 100;

            // --- Regressão Linear Simples (OLS) ---
            // X = índice do mês (1..6), Y = receita
            // Fórmula: b = (n*ΣXY - ΣX*ΣY) / (n*ΣX² - (ΣX)²)
            //          a = (ΣY - b*ΣX) / n
            int n = _vendasHistoricas.Count;
            double somaX = _vendasHistoricas.Sum(v => (double)v.Mes);
            double somaY = _vendasHistoricas.Sum(v => (double)v.Receita);
            double somaXY = _vendasHistoricas.Sum(v => (double)v.Mes * (double)v.Receita);
            double somaX2 = _vendasHistoricas.Sum(v => (double)(v.Mes * v.Mes));

            double b = (n * somaXY - somaX * somaY) / (n * somaX2 - somaX * somaX);
            double a = (somaY - b * somaX) / n;

            // Previsão para o mês 7
            decimal previsaoMes7 = (decimal)(a + b * 7);

            string interpretacao = b > 0
                ? $"Tendência de crescimento de R$ {b:F2} por mês — mercado em expansão."
                : $"Tendência de queda de R$ {Math.Abs(b):F2} por mês — recomenda-se ação corretiva.";

            // --- Segmentação de Clientes (regras baseadas em dados) ---
            // VIP: gasto > R$ 8.000 | Ativo: gasto > R$ 2.000 | Inativo: abaixo disso
            foreach (var c in _clientes)
            {
                c.Classificacao = c.TotalGasto >= 8000m ? "VIP"
                                : c.TotalGasto >= 2000m ? "Ativo"
                                : "Inativo";
            }

            // --- Série completa com previsão para o gráfico ---
            var serieCompleta = _vendasHistoricas.Select(v => new PrevisaoReceita
            {
                Mes = v.NomeMes,
                ValorPrevisto = v.Receita,
                EPrevisao = false
            }).ToList();

            serieCompleta.Add(new PrevisaoReceita
            {
                Mes = "Abril/25 (prev.)",
                ValorPrevisto = previsaoMes7,
                EPrevisao = true
            });

            return new AnalyticsViewModel
            {
                ReceitaTotalSemestre = receitaTotal,
                MediaMensalReceita = mediaReceita,
                TicketMedio = ticketMedio,
                TotalVendasSemestre = totalVendas,
                VariacaoUltimoMes = variacao,
                VendasMensais = _vendasHistoricas,
                RankingClientes = _clientes.OrderByDescending(c => c.TotalGasto).ToList(),
                RankingProdutos = _produtos.OrderByDescending(p => p.QtdVendida).ToList(),
                PrevisaoProximoMes = serieCompleta,
                PrevisaoMes7 = previsaoMes7,
                CoeficienteAngular = b,
                InterpretacaoTendencia = interpretacao,
                ClientesVip = _clientes.Count(c => c.Classificacao == "VIP"),
                ClientesAtivos = _clientes.Count(c => c.Classificacao == "Ativo"),
                ClientesInativos = _clientes.Count(c => c.Classificacao == "Inativo"),
            };
        }
    }
}