using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Controller {
    public class ExceptionEventArgs :EventArgs{
        public Exception Exception { get;  }

        public string Message { get; }

        public ExceptionEventArgs(string msg, Exception ex) {
            Message = msg;
            Exception = ex;
        }

        public ExceptionEventArgs(Exception ex) : this("", ex) { }
        
    }
}
