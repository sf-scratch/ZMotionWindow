using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ZMotionWindow.Rules
{
    internal class IsDigitalValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult result;
            string dateStr = value as string;
            if (dateStr != null && float.TryParse(dateStr, out float date))
            {
                result = ValidationResult.ValidResult;
            }
            else
            {
                result = new ValidationResult(false, "必须为数字");
            }

            return result;
        }
    }
}
