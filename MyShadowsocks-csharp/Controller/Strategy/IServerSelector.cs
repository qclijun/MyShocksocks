using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MyShadowsocks.Model;

namespace MyShadowsocks.Controller.Strategy {
    interface IServerSelector {        
        
        int GetAServerIndex();

        void SetFailure(int index);

        string Name { get; }
    }
}
