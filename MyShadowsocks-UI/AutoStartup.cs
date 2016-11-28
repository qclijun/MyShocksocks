using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace MyShadowsocks_UI {
    class AutoStartup {

        static string ExePath = Assembly.GetEntryAssembly().Location;

        static string Key = "MyShadowsocks_" + ExePath.GetHashCode();

        static string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";


        public static bool Set(bool enabled) {
            using(var runKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, Environment.Is64BitOperatingSystem ?
                RegistryView.Registry64 : RegistryView.Registry32)
                .OpenSubKey(keyPath, true)) {
                if(runKey == null) {
                    return false;
                }
                if(enabled) {
                    runKey.SetValue(Key, ExePath);
                } else {
                    runKey.DeleteValue(Key);
                }
                return true;
            }

        }

        public static bool Check() {
            using(var runKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, Environment.Is64BitOperatingSystem ?
                RegistryView.Registry64 : RegistryView.Registry32)
                .OpenSubKey(keyPath, true)) {
               
                if(runKey == null) return false;
                string[] runList = runKey.GetValueNames();
                foreach(var item in runList) {
                    if(item.Equals(Key, StringComparison.OrdinalIgnoreCase))
                        return true;
                    
                }
                return false;
            }
        }



    }
}
