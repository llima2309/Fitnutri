using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AppFitNutri.Core
{
    public class ValidatorsCore
    {
        // ^ início, [A-Za-z0-9] somente alfanumérico, {3,32} tamanho, $ fim
        private static readonly Regex UserNameRegex = new(@"^[A-Za-z0-9]{3,32}$", RegexOptions.Compiled);

        // Pelo menos: 1 minúscula, 1 maiúscula, 1 dígito, 1 especial, min 8 chars
        // Especial = qualquer não alfanumérico
        private static readonly Regex PasswordRegex = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$", RegexOptions.Compiled);

        public static bool IsValidUserName(string value) => !string.IsNullOrWhiteSpace(value) && UserNameRegex.IsMatch(value);
        public static bool IsStrongPassword(string value) => !string.IsNullOrWhiteSpace(value) && PasswordRegex.IsMatch(value);
    }
}
