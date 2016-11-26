using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MyShadowsocks_UI {
    public class ServerConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            string hostname = values[0] as string;
            if(string.IsNullOrEmpty(hostname)) return "New Server";
            int port = (int)values[1];
            string remarks = values[2] as string;
            return string.IsNullOrEmpty(remarks) ? hostname + ":" + port : remarks + "(" + hostname + ":" + port + ")";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
