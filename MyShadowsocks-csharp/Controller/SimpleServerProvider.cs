using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShadowsocks.Model;

namespace MyShadowsocks.Controller {
    class SimpleServerProvider : IServerProvider {
        public Server GetAServer() {
            //return Program.ServerList[Program.Config.Index];
            return Program.ServerList[2];
        }
    }
}
