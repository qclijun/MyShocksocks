using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Shadowsocks.Controller;

namespace Shadowsocks.Util.SystemProxy
{
    public static class WinINet
    {
        public enum IEProxyOption
        {
            Direct = 0, // direct no proxy
            Proxy_Direct = 1,
            Proxy_PAC = 2,
        }
        private static void SetIEProxy(bool enable,bool global,string proxyServer, string pacUrl,string connName)
        {
            List<INTERNET_PER_CONN_OPTION> optionList = new List<INTERNET_PER_CONN_OPTION>();
            if (enable)
            {
                if (global)
                {
                    // global proxy;
                    optionList.Add(new INTERNET_PER_CONN_OPTION
                    {
                        dwOption = (int)INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_FLAGS_UI,
                        Value = {dwValue=(int)(INTERNET_OPTION_PER_CONN_FLAGS_UI.PROXY_TYPE_PROXY
                        |INTERNET_OPTION_PER_CONN_FLAGS_UI.PROXY_TYPE_DIRECT)}
                    });
                    optionList.Add(new INTERNET_PER_CONN_OPTION
                    {
                        dwOption = (int)INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_PROXY_SERVER,
                        Value = { pszValue = Marshal.StringToHGlobalAuto(proxyServer) }
                    });
                    optionList.Add(new INTERNET_PER_CONN_OPTION
                    {
                        dwOption = (int)INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_PROXY_BYPASS,
                        Value = { pszValue = Marshal.StringToHGlobalAuto("<local>") }
                    });
                }
                else
                { //pac
                    optionList.Add(new INTERNET_PER_CONN_OPTION
                    {
                        dwOption = (int)INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_FLAGS_UI,
                        Value = { dwValue = (int)INTERNET_OPTION_PER_CONN_FLAGS_UI.PROXY_TYPE_AUTO_PROXY_URL }
                    });
                    optionList.Add(new INTERNET_PER_CONN_OPTION
                    {
                        dwOption = (int)INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_AUTOCONFIG_URL,
                        Value = { pszValue = Marshal.StringToHGlobalAuto(pacUrl) }
                    });
                }
            }
            else
            { //dirct
                optionList.Add(new INTERNET_PER_CONN_OPTION
                {
                    dwOption = (int)INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_FLAGS_UI,
                    Value = {dwValue=(int)(INTERNET_OPTION_PER_CONN_FLAGS_UI.PROXY_TYPE_AUTO_DETECT
                    |INTERNET_OPTION_PER_CONN_FLAGS_UI.PROXY_TYPE_DIRECT)}
                });

            }

            var len = optionList.Sum(x => Marshal.SizeOf(x));
            IntPtr buffer = Marshal.AllocCoTaskMem(len);
            IntPtr current = buffer;
            foreach(INTERNET_PER_CONN_OPTION eachOption in optionList)
            {
                Marshal.StructureToPtr(eachOption, current, false);
                current = (IntPtr)((int)current + Marshal.SizeOf(eachOption));
            }
            INTERNET_PER_CONN_OPTION_LIST optionListStruct = new INTERNET_PER_CONN_OPTION_LIST();
            optionListStruct.pOptions = buffer;
            optionListStruct.Size = Marshal.SizeOf(optionListStruct);
            optionListStruct.Connection = string.IsNullOrEmpty(connName) ? IntPtr.Zero :
                Marshal.StringToHGlobalAuto(connName);
            optionListStruct.OptionCount = optionList.Count;
            optionListStruct.OptionError = 0;
            int optionListSize = Marshal.SizeOf(optionListStruct);

            IntPtr intptrStruct = Marshal.AllocCoTaskMem(optionListSize);
            Marshal.StructureToPtr(optionListStruct, intptrStruct, true);
            bool bReturn = NativeMethods.InternetSetOption(
                IntPtr.Zero,
                INTERNET_OPTION.INTERNET_OPTION_PER_CONNECTION_OPTION,
                intptrStruct, optionListSize);

            Marshal.FreeCoTaskMem(buffer);
            Marshal.FreeCoTaskMem(intptrStruct);
            if (!bReturn)
            {
                throw new Exception("InternetSetOption: " + Marshal.GetLastWin32Error());
            }

            bReturn = NativeMethods.InternetSetOption(
                IntPtr.Zero,
                INTERNET_OPTION.INTERNET_OPTION_PROXY_SETTING_CHANGED,
                IntPtr.Zero, 0);
            if (!bReturn)
            {
                Logging.Error("InternetSetOption: INTERNET_OPTION_PROXY_SETTINGS_CHANGED");
                
            }
            bReturn = NativeMethods.InternetSetOption(
                IntPtr.Zero,
                INTERNET_OPTION.INTERNET_OPTION_REFRESH,
                IntPtr.Zero, 0);
            if (!bReturn)
            {
                Logging.Error("InternetSetOption: INTERNET_OPTION_REFRESH");
            }

        }

        public static void SetIEProxy(bool enable, bool global, string proxyServer, string pacUrl)
        {
            string[] allConnections = null;


            var ret = RemoteAccessService.GetAllConns(ref allConnections);
            if (ret == 2)
                throw new Exception("Cannot get all connections");
            else if (ret == 1)
            { 
                // no entries, only set LAN
                SetIEProxy(enable, global, proxyServer, pacUrl, null);
            }else if (ret == 0)
            {
                SetIEProxy(enable, global, proxyServer, pacUrl, null);
                foreach (string connName in allConnections)
                {
                    Console.WriteLine($"Connection name: {connName}");
                    SetIEProxy(enable, global, proxyServer, pacUrl, connName);
                }
            }
        }

        public static void SetIEProxy(IEProxyOption option, string proxyServer, string pacUrl)
        {
            switch (option)
            {
                case IEProxyOption.Direct:
                    SetIEProxy(false, false, null, null);
                    break;
                case IEProxyOption.Proxy_Direct:
                    SetIEProxy(true, true, proxyServer, null);
                    break;
                case IEProxyOption.Proxy_PAC:
                    SetIEProxy(true, false, null, pacUrl);
                    break;
                default:
                    break;
            }
        }

    }
}
