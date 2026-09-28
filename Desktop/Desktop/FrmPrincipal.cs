namespace DBS.WinForms
{
    public partial class FrmPrincipal : Form
    {
        public FrmPrincipal()
        {
            InitializeComponent();
        }

        private void btnVenda_Click(object? sender, EventArgs e)
        {
            using var f = new FrmVenda();
            f.ShowDialog(this);
        }

        private void btnClientes_Click(object? sender, EventArgs e)
        {
            using var f = new FrmClientes();
            f.ShowDialog(this);
        }
    }
}