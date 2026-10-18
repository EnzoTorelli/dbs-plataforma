using DbsErpMobile.Models;

namespace DbsErpMobile.Services;

public class ClienteService
{
    public Cliente ClienteSelecionado { get; private set; }

    public void Selecionar(Cliente cliente)
    {
        ClienteSelecionado = cliente;
    }

    public void Limpar()
    {
        ClienteSelecionado = null;
    }
}
