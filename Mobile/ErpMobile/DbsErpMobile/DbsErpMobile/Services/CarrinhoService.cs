using System.Collections.ObjectModel;
using DbsErpMobile.Models;

namespace DbsErpMobile.Services;

public class CarrinhoService
{
    public ObservableCollection<ItemPedido> Itens { get; } = new();

    public void Adicionar(Produto produto, int quantidade)
    {
        var itemExistente = Itens.FirstOrDefault(i => i.IdProduto == produto.Id);
        if (itemExistente != null)
        {
            itemExistente.Quantidade += quantidade;
        }
        else
        {
            Itens.Add(new ItemPedido
            {
                IdProduto = produto.Id,
                NomeProduto = produto.Nome,
                Quantidade = quantidade,
                PrecoUnitario = produto.Preco
            });
        }
    }

    public decimal ValorTotal => Itens.Sum(i => i.Subtotal);

    public void Limpar() => Itens.Clear();
}