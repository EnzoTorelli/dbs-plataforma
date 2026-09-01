using DbsErpMobile.Models;
using DbsErpMobile.Services;

namespace DbsErpMobile.Views;

[QueryProperty(nameof(Produto), "produto")]
public partial class ProdutoDetailPage : ContentPage
{
    private readonly CarrinhoService _carrinhoService;
    private int _quantidade = 1;
    private Produto _produto;

    public Produto Produto
    {
        get => _produto;
        set
        {
            _produto = value;
            AtualizarTela();
        }
    }

    public ProdutoDetailPage(CarrinhoService carrinhoService)
    {
        InitializeComponent();
        _carrinhoService = carrinhoService;
    }

    private void AtualizarTela()
    {
        NomeLabel.Text = _produto.Nome;
        DescricaoLabel.Text = _produto.Descricao;
        PrecoLabel.Text = $"R$ {_produto.Preco:F2}";
        EstoqueLabel.Text = $"Estoque disponível: {_produto.Estoque}";
    }

    private void OnDiminuirClicked(object sender, EventArgs e)
    {
        if (_quantidade > 1) _quantidade--;
        QuantidadeLabel.Text = _quantidade.ToString();
    }

    private void OnAumentarClicked(object sender, EventArgs e)
    {
        if (_quantidade < _produto.Estoque) _quantidade++;
        QuantidadeLabel.Text = _quantidade.ToString();
    }

    private async void OnAdicionarClicked(object sender, EventArgs e)
    {
        _carrinhoService.Adicionar(_produto, _quantidade);
        await DisplayAlertAsync("Sucesso", $"{_quantidade}x {_produto.Nome} adicionado ao carrinho!", "OK");
        await Shell.Current.GoToAsync("..");
    }
}