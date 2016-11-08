using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks.Model
{
    [Serializable]
    public class HotkeyConfiguration
    {
        private string switchSystemProxy;
        private string changeToPac;
        private string changeToGlobal;
        private string switchAllowLan;
        private string showLogs;
        private string serverMoveUp;
        private string serverMoveDown;

        public HotkeyConfiguration()
        {

        }
    }
}
