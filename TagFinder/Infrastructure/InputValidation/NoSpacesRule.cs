using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TagFinder
{
    public class NoSpacesRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;

            if (input.Contains(' '))
                return new ValidationResult(false, "Without spaces.");

            return new ValidationResult(true, null);
        }
    }
}
