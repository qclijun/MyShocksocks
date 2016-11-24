using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

using MyShadowsocks.Util;
using MyShadowsocks.Controller;
using MyShadowsocks.Properties;

using NLog;

namespace MyShadowsocks.Encryption {
    public static class MbedTLS {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string DllName = "libsscrypto.dll";


        public const int MbedTLS_Encrypt = 1;
        public const int MbedTLS_Decrypt = 0;

        static MbedTLS() {
            string dllPath = (DllName);
            try {
                if(!File.Exists(dllPath))
                    FileManager.UncompressFile(dllPath, Resources.libsscrypto_dll);
            } catch(IOException ex) {
                logger.Error("failed to write to file {0}. Exception message: {1}",
                    dllPath, ex.Message);
                throw;
            }
            Util.Interops.LoadLibrary(dllPath);
        }

        public static byte[] MD5(byte[] input) {
            byte[] output = new byte[16];
            md5(input, (uint)input.Length, output);
            return output;
        }
        public static void MD5(byte[] input, byte[] result) {
            md5(input, (uint)input.Length, result);
        }



        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cipher_info_from_string(string cipher_name);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void cipher_init(IntPtr ctx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_setup(IntPtr ctx, IntPtr cipher_info);


        //xxx: Check operation before using it
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_setkey(IntPtr ctx, byte[] key, int key_bitlen, int operation);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_set_iv(IntPtr ctx, byte[] iv, int iv_len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_reset(IntPtr ctx);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_update(IntPtr ctx, byte[] input, int ilen, byte[] output, ref int olen);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void cipher_free(IntPtr ctx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void md5(byte[] input, uint ilen, byte[] output);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_get_size_ex();

    }
}
