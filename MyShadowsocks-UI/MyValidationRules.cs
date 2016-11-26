using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyShadowsocks_UI {
    public class NotEmptyRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if(string.IsNullOrEmpty(value as string))
                return new ValidationResult(false, "input is empty");
            return new ValidationResult(true, null);
        }
    }


    public class HostnameRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            string hostname = value as string;
            var type = Uri.CheckHostName(hostname);
            if(type == UriHostNameType.Unknown) {
                return new ValidationResult(false, "Invalid host name.");
            }
            return new ValidationResult(true, null);
        }
    }



}
