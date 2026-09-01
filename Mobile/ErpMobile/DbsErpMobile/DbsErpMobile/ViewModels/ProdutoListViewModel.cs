using System.Collections.ObjectModel;
using DbsErpMobile.Models;

namespace DbsErpMobile.ViewModels;

public class ProdutoListViewModel
{
    public ObservableCollection<Produto> Produtos { get; set; }

    public ProdutoListViewModel()
    {
        // dados mockados, temporários - depois trocamos pela API
        Produtos = new ObservableCollection<Produto>
        {
            new Produto { Id = 1, Nome = "Memoria RAM 8GB", Descricao = "DDR4", Preco = 199.90m, Estoque = 10 },
            new Produto { Id = 2, Nome = "RTX 3060", Descricao = "Placa de video NVIDIA", Preco = 2500.00m, Estoque = 5 },
            new Produto { Id = 4, Nome = "Desktop Dell Pro Micro", Descricao = "Intel i5 / 8GB / 256GB SSD", Preco = 6899.00m, Estoque = 5 },
            new Produto { Id = 6, Nome = "Mouse Gamer REDRAGON", Descricao = "7 Botoes", Preco = 158.99m, Estoque = 5 },
        };
    }
}