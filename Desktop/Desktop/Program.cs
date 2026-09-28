
namespace DBS.WinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                Database.Open().Dispose(); // testa a conexão antes de abrir o app
            }
            catch (Exception ex)
            {
                MessageBox.Show("Não foi possível conectar ao banco:\n\n" + ex.Message,
                    "Erro de conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FrmPrincipal());
        }
    }
}