using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Jun.Net {
    public static class NetUtils {

        public static bool IsPortInUse(int port) {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();
            //foreach(var ep in tcpEndPoints) {
            //    if(ep.Port == port) return true;
            //}
            //return false;
           
            return tcpEndPoints.Any(ep => ep.Port == port);            
        }

        private const int MaxPort = 65535;

        public static int GetFreePortFrom(int beginPort) {
            int defaultPort = beginPort;

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();
            for(int port = beginPort;port <= MaxPort;++port) {
                if(tcpEndPoints.All(ep => ep.Port != port)) return port;
            }
            
            throw new Exception("No free port found.");
        }

    }
}
