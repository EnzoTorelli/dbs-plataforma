using System.Collections.ObjectModel;
using DbsErpMobile.Models;

namespace DbsErpMobile.ViewModels;

public class ClienteListViewModel
{
    public ObservableCollection<Cliente> Clientes { get; set; }

    public ClienteListViewModel()
    {
        // Dados mockados temporários.
        // Depois substituiremos pela API/banco real.
        Clientes = new ObservableCollection<Cliente>
        {
            new Cliente
            {
                Id = 1,
                Nome = "João da Silva",
                Cpf = "123.456.789-00",
                Email = "joao@email.com",
                Telefone = "(11) 99999-1111",
                DataCadastro = DateTime.Now.AddMonths(-5)
            },

            new Cliente
            {
                Id = 2,
                Nome = "Maria Oliveira",
                Cpf = "987.654.321-00",
                Email = "maria@email.com",
                Telefone = "(11) 98888-2222",
                DataCadastro = DateTime.Now.AddMonths(-3)
            },

            new Cliente
            {
                Id = 3,
                Nome = "Carlos Santos",
                Cpf = "456.789.123-00",
                Email = "carlos@email.com",
                Telefone = "(11) 97777-3333",
                DataCadastro = DateTime.Now.AddMonths(-1)
            },

            new Cliente
            {
                Id = 4,
                Nome = "Empresa Tech Solutions",
                Cpf = "12.345.678/0001-90",
                Email = "contato@techsolutions.com",
                Telefone = "(11) 96666-4444",
                DataCadastro = DateTime.Now.AddMonths(-8)
            }
        };
    }
}
