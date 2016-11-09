/****************************** Module Header ******************************\
 Module Name:  WinINet.cs
 Project:      CSWebBrowserWithProxy
 Copyright (c) Microsoft Corporation.
 
 This class is used to set the proxy. or restore to the system proxy for the
 current application
 
 This source is subject to the Microsoft Public License.
 See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
 All other rights reserved.
 
 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
 EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.   
\***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Junlee.Util.SystemProxy
{
    public static class WinINet
    {
        public enum SystemProxyOption
        {
            Proxy_None = 0, // direct no proxy
            Proxy_Direct = 1,
            Proxy_PAC = 2
            
        }

        private static void SetSystemProxy(bool enable, bool global, string proxyServer, string pacUrl, string connName)
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
                        Value = { pszValue = Marshal.StringToHGlobalUni(proxyServer) }
                    });
                    optionList.Add(new INTERNET_PER_CONN_OPTION
                    {
                        dwOption = (int)INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_PROXY_BYPASS,
                        Value = { pszValue = Marshal.StringToHGlobalUni("<local>") }
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
                        Value = { pszValue = Marshal.StringToHGlobalUni(pacUrl) }
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
            foreach (INTERNET_PER_CONN_OPTION eachOption in optionList)
            {
                Marshal.StructureToPtr(eachOption, current, false);
                current = (IntPtr)((int)current + Marshal.SizeOf(eachOption));
            }
            INTERNET_PER_CONN_OPTION_LIST optionListStruct = new INTERNET_PER_CONN_OPTION_LIST();
            optionListStruct.pOptions = buffer;
            optionListStruct.Size = Marshal.SizeOf(optionListStruct);
            optionListStruct.Connection = string.IsNullOrEmpty(connName) ? IntPtr.Zero :
                Marshal.StringToHGlobalUni(connName); // TODO: not working if connName contains Chinese
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
                INTERNET_OPTION.INTERNET_OPTION_PROXY_SETTINGS_CHANGED,
                IntPtr.Zero, 0);
            if (!bReturn)
            {
                throw new Exception("InternetSetOption: INTERNET_OPTION_PROXY_SETTINGS_CHANGED");

            }
            bReturn = NativeMethods.InternetSetOption(
                IntPtr.Zero,
                INTERNET_OPTION.INTERNET_OPTION_REFRESH,
                IntPtr.Zero, 0);
            if (!bReturn)
            {
                throw new Exception("InternetSetOption: INTERNET_OPTION_REFRESH");
            }

        }

        private static void SetSystemProxy(bool enable, bool global, string proxyServer, string pacUrl)
        {
            string[] allConnections = null;


            var ret = RemoteAccessService.GetAllConns(ref allConnections);
            if (ret == 2)
                throw new Exception("Cannot get all connections");
            else if (ret == 1)
            {
                // no entries, only set LAN
                SetSystemProxy(enable, global, proxyServer, pacUrl, null);
            }
            else if (ret == 0)
            {
                SetSystemProxy(enable, global, proxyServer, pacUrl, null);
                foreach (string connName in allConnections)
                {
                    //Console.WriteLine($"Connection name: {connName}");
                    SetSystemProxy(enable, global, proxyServer, pacUrl, connName);
                }
            }
        }

        // connName = null for LAN
        public static void SetSystemProxy(SystemProxyOption option, string proxyServer, string pacUrl, string connName)
        {
            switch (option)
            {
                case SystemProxyOption.Proxy_None:
                    SetSystemProxy(false, false, null, null, connName);
                    break;
                case SystemProxyOption.Proxy_Direct:
                    SetSystemProxy(true, true, proxyServer, null, connName);
                    break;
                case SystemProxyOption.Proxy_PAC:
                    SetSystemProxy(true, false, null, pacUrl, connName);
                    break;
                default:
                    break;
            }
        }


        // set ie proxy to all connections
        public static void SetSystemProxy(SystemProxyOption option, string proxyServer, string pacUrl)
        {
            string[] allConnections = null;


            var ret = RemoteAccessService.GetAllConns(ref allConnections);
            if (ret == 2)
                throw new Exception("Cannot get all connections");
            else if (ret == 1)
            {
                // no entries, only set LAN
                SetSystemProxy(option, proxyServer, pacUrl, null);
            }
            else if (ret == 0)
            {
                SetSystemProxy(option, proxyServer, pacUrl, null);
                foreach (string connName in allConnections)
                {
                    //Console.WriteLine($"Connection name: {connName}");
                    SetSystemProxy(option, proxyServer, pacUrl, connName);
                }
            }


        }

    }
}