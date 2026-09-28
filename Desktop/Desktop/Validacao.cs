using System.Net.Mail;
using System.Text.RegularExpressions;

namespace DBS.WinForms
{
    /// <summary>
    /// Validação e formatação de dados de cliente (padrão brasileiro).
    /// </summary>
    public static class Validacao
    {
        // ---------- Utilitários ----------

        /// <summary>Remove tudo que não for dígito 0-9.</summary>
        public static string SoDigitos(string? s) =>
            new((s ?? "").Where(c => c >= '0' && c <= '9').ToArray());

        public static bool EhDigito(char c) => c >= '0' && c <= '9';

        // ---------- Nome ----------

        public static bool NomeValido(string? nome)
        {
            var n = (nome ?? "").Trim();
            return n.Length >= 3 && n.Any(char.IsLetter);
        }

        // ---------- CPF ----------

        /// <summary>
        /// Valida os dois dígitos verificadores do CPF.
        /// Aceita com ou sem máscara. Rejeita sequências repetidas (111.111.111-11 etc.).
        /// </summary>
        public static bool CpfValido(string? cpf)
        {
            var d = SoDigitos(cpf);
            if (d.Length != 11) return false;
            if (d.Distinct().Count() == 1) return false;

            int Digito(int qtd)
            {
                int soma = 0;
                for (int i = 0; i < qtd; i++)
                    soma += (d[i] - '0') * (qtd + 1 - i);   // pesos 10..2 e depois 11..2
                int resto = soma % 11;
                return resto < 2 ? 0 : 11 - resto;
            }

            return Digito(9) == d[9] - '0' && Digito(10) == d[10] - '0';
        }

        /// <summary>000.000.000-00</summary>
        public static string FormatarCpf(string? cpf)
        {
            var d = SoDigitos(cpf);
            return d.Length == 11
                ? $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}"
                : (cpf ?? "").Trim();
        }

        // ---------- Telefone ----------

        private static readonly HashSet<int> DddsValidos = new()
        {
            11, 12, 13, 14, 15, 16, 17, 18, 19,
            21, 22, 24, 27, 28,
            31, 32, 33, 34, 35, 37, 38,
            41, 42, 43, 44, 45, 46, 47, 48, 49,
            51, 53, 54, 55,
            61, 62, 63, 64, 65, 66, 67, 68, 69,
            71, 73, 74, 75, 77, 79,
            81, 82, 83, 84, 85, 86, 87, 88, 89,
            91, 92, 93, 94, 95, 96, 97, 98, 99
        };

        /// <summary>
        /// Celular: DDD + 9 + 8 dígitos (11 dígitos).
        /// Fixo:    DDD + [2-5] + 7 dígitos (10 dígitos).
        /// </summary>
        public static bool TelefoneValido(string? tel)
        {
            var d = SoDigitos(tel);
            if (d.Length != 10 && d.Length != 11) return false;
            if (!DddsValidos.Contains(int.Parse(d[..2]))) return false;

            return d.Length == 11
                ? d[2] == '9'
                : d[2] >= '2' && d[2] <= '5';
        }

        /// <summary>(00) 00000-0000 ou (00) 0000-0000</summary>
        public static string FormatarTelefone(string? tel)
        {
            var d = SoDigitos(tel);
            return d.Length switch
            {
                11 => $"({d[..2]}) {d[2..7]}-{d[7..]}",
                10 => $"({d[..2]}) {d[2..6]}-{d[6..]}",
                _ => (tel ?? "").Trim()
            };
        }

        // ---------- E-mail ----------

        private static readonly Regex RxEmail = new(
            @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9\-]+(\.[A-Za-z0-9\-]+)*\.[A-Za-z]{2,}$",
            RegexOptions.Compiled);

        public static bool EmailValido(string? email)
        {
            var e = (email ?? "").Trim();
            if (e.Length > 254 || e.Contains("..") || !RxEmail.IsMatch(e)) return false;
            try { return new MailAddress(e).Address == e; }
            catch { return false; }
        }

        public static string FormatarEmail(string? email) => (email ?? "").Trim().ToLowerInvariant();
    }
}
