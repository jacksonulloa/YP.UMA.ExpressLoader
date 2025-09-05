using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace YP.ZReg.Utils.Helpers
{
    public static class TxtProcessor
    {
        private static readonly Regex Permitidos = new(@"^[A-Za-z0-9-]+$", RegexOptions.Compiled);
        public static bool ValidarFecha(string fecha)
        {
            if (string.IsNullOrWhiteSpace(fecha) || fecha.Length != 8)
                return false;

            return DateTime.TryParseExact(
                fecha,
                "ddMMyyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }
        public static bool ValidarNombre(string nombre, out string nombreNormalizado)
        {
            nombreNormalizado = null;
            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            // 1) Quitar tildes/dieresis usando Normalization Form D
            string sinAcentos = new string(
                nombre.Normalize(NormalizationForm.FormD)
                      .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                      .ToArray()
            ).Normalize(NormalizationForm.FormC);

            // 2) Expresión regular: solo letras A-Z, espacios y punto
            // ^[A-Za-z. ]+$   → válido solo si contiene esos caracteres
            if (!Regex.IsMatch(sinAcentos, @"^[A-Za-z0-9. ]+$"))
                return false;

            nombreNormalizado = sinAcentos;
            return true;
        }
        public static bool ValidarLlave(string identificador, bool isPrincipal)
        {
            if (string.IsNullOrWhiteSpace(identificador) && !isPrincipal)
                return true;

            // 1) Normalizar y quitar tildes/dieresis si hubiera
            string sinAcentos = new string(
                identificador.Normalize(NormalizationForm.FormD)
                             .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                             .ToArray()
            ).Normalize(NormalizationForm.FormC);

            // 2) Validar: solo letras y números, sin espacios ni símbolos
            return Regex.IsMatch(sinAcentos, @"^[A-Za-z0-9]+$");
        }
        public static bool TryParse4Importes(string valor48, out decimal a1, out decimal a2, out decimal a3, out decimal a4)
        {
            a1 = a2 = a3 = a4 = 0m;

            if (string.IsNullOrEmpty(valor48) || valor48.Length != 48) return false;
            if (!Regex.IsMatch(valor48, @"^\d{48}$")) return false;

            string s1 = valor48.Substring(0, 12);
            string s2 = valor48.Substring(12, 12);
            string s3 = valor48.Substring(24, 12);
            string s4 = valor48.Substring(36, 12);

            return TryParseImporte12(s1, out a1)
                && TryParseImporte12(s2, out a2)
                && TryParseImporte12(s3, out a3)
                && TryParseImporte12(s4, out a4);
        }
        private static bool TryParseImporte12(string s, out decimal importe)
        {
            importe = 0m;
            if (s == null || s.Length != 12) return false;
            if (!s.All(char.IsDigit)) return false;

            string parteEntera = s.Substring(0, s.Length - 2);
            string parteDecimal = s.Substring(s.Length - 2, 2);

            if (!ulong.TryParse(parteEntera, out ulong entero)) return false;
            if (!uint.TryParse(parteDecimal, out uint dec2)) return false; // 00..99

            importe = entero + (dec2 / 100m);
            return true;
        }
        public static bool ValidarReglas(string valor48)
        {
            if (!TryParse4Importes(valor48, out var a1, out var a2, out var a3, out var a4))
                return false;

            if (a1 < 1.00m) return false;   // mínimo para el 1ro
            if (a4 < 1.00m) return false;   // mínimo para el 4to

            return (a1 + a2 + a3) >= a4;
        }
        public static bool ValidarDocumento(string valor, out string sanitizado)
        {
            sanitizado = null;
            if (string.IsNullOrWhiteSpace(valor)) return false;

            // 1) Trim
            string trimmed = valor.Trim();

            // Si después de Trim hay espacios internos → inválido
            if (trimmed.Contains(' '))
                return false;

            // 2) Normalizar (quitar acentos/dieresis)
            string normalizado = new string(
                trimmed.Normalize(NormalizationForm.FormD)
                       .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                       .ToArray()
            ).Normalize(NormalizationForm.FormC);

            // 3) Validar con regex
            if (!Permitidos.IsMatch(normalizado))
                return false;

            sanitizado = normalizado;
            return true;
        }
        public static bool ValidarTipoRegistro(String tipo, string valor)
        {
            return tipo.Equals("R") && !valor.Equals("N") ? false : true;
        }
        public static string ToDecimalString(this string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "0.00";

            if (long.TryParse(raw, out var value))
            {
                // divide entre 100 para considerar 2 decimales
                return (value / 100.0m).ToString("0.00",
                    System.Globalization.CultureInfo.InvariantCulture);
            }

            return raw; // fallback si no se pudo parsear
        }
        public static decimal ToDecimal(this string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return 0m;
            decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var result);
            return result;
        }
        public static DateTime? ConvertToDate(string fecha)
        {
            DateTime result = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(fecha)) return null;

            return DateTime.TryParseExact(
                fecha.Trim(),
                "ddMMyyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt)
                ? dt
                : (DateTime?)null;
        }
        public static decimal? ConvertToDecimal(string input, int decimals)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            if (decimals < 0) throw new ArgumentOutOfRangeException(nameof(decimals));

            input = input.Trim();
            bool negative = input.StartsWith("-");
            string digits = negative ? input.Substring(1) : input;

            if (!digits.All(char.IsDigit)) return null; // inválido si hay no-dígitos

            // Si no hay dígitos, interpreta como 0
            if (digits.Length == 0) return 0m;

            // Parse como entero y divide por 10^decimals usando decimal (evita dobles/Math.Pow)
            decimal value = decimal.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture);

            decimal divisor = 1m;
            for (int i = 0; i < decimals; i++) divisor *= 10m;

            value /= divisor;
            return negative ? -value : value;
        }
    }
}
