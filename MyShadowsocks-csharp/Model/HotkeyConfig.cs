using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Model
{
    [Serializable]
    public class HotkeyConfiguration
    {
        public string SwitchSystemProxy { get; set; }
        public string ChangeToPac { get; set; }
        public string ChangeToGlobal { get; set; }
        public string SwitchAllowLan { get; set; }
        public string ShowLogs { get; set; }
        public string ServerMoveUp { get; set; }
        public string ServerMoveDown { get; set; }

        public HotkeyConfiguration()
        {

        }

        public HotkeyConfiguration Clone() {
            return MemberwiseClone() as HotkeyConfiguration;
        }
    }
}
