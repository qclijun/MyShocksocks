using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace Shadowsocks.Util.SystemProxy
{
    public static class RemoteAccessService
    {
        private enum RasFieldSizeConstants
        {
            RAS_MaxEntryName = 256,
            RAS_MaxPath = 260,
        }
        private const int ERROR_SUCCESS = 0;
        private const int RASBASE = 600;
        private const int ERROR_BUFFER_TOO_SMALL = RASBASE + 3;

        [StructLayout(LayoutKind.Sequential,CharSet =CharSet.Auto)]
        private struct RasEntryName
        {
            public int dwSize;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)RasFieldSizeConstants.RAS_MaxEntryName + 1)]
            public string szEntryName;

            public int dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)RasFieldSizeConstants.RAS_MaxPath + 1)]
            public string szPhonebookPath;
        }

        [DllImport("rasapi32.dll", CharSet = CharSet.Auto)]
        private static extern uint RasEnumEntries(
            string reserved,
            string lpszPhonebook,
            [In, Out]RasEntryName[] lpRasEntryName,
            ref int lpcb,
            out int lpcEntries);

        public static uint GetAllConns(ref string[] allConns)
        {
            int lpNames = 0;
            int entryNameSize = 0;
            int lpSize = 0;
            uint retVal = ERROR_SUCCESS;
            RasEntryName[] names = null;

            entryNameSize = Marshal.SizeOf(typeof(RasEntryName));

            retVal = RasEnumEntries(null, null, null, ref lpSize, out lpNames);
            if (retVal == ERROR_BUFFER_TOO_SMALL)
            {
                names = new RasEntryName[lpNames];
                for(int i = 0; i < names.Length; ++i)
                {
                    names[i].dwSize = entryNameSize;
                }
                retVal = RasEnumEntries(null, null, names, ref lpSize, out lpNames);
            }

            if (retVal == ERROR_SUCCESS)
            {

                if (lpNames == 0) //no entries found.
                    return 1;
                allConns = new string[names.Length];
                for(int i = 0; i < names.Length; ++i)
                {
                    allConns[i] = names[i].szEntryName;
                }
                return 0;
            }
            else
            {
                return 2;
            }
        }
    }
}
