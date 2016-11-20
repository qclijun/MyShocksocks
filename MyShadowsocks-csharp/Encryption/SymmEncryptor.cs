using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Encryption
{
    abstract class SymmEncryptor : EncryptorBase
    {
        public int KeySize { get; } = 32;
        public int IVSize { get; } = 16;
        public byte[] Key { get;  }
        public byte[] IV { get;  }
        public string MethodName { get;  }

        public static SymmEncryptor Create(string methodName)
        {
            return null;
        }


        private static byte[] GenerateKey(byte[] bytes, int keySize)
        {
            Debug.Assert(keySize >= 16 && keySize <= 32);

            byte[] key = new byte[32];
            byte[] result = new byte[bytes.Length + 16];
            byte[] md5sum = new byte[16];
            int i = 0;
            while (i < key.Length)
            {
                if (i == 0) MbedTLS.MD5(bytes, md5sum);
                else
                {
                    Buffer.BlockCopy(md5sum, 0, result, 0, md5sum.Length);
                    Buffer.BlockCopy(bytes, 0, result, md5sum.Length, bytes.Length);
                    MbedTLS.MD5(result, md5sum);
                }
                Buffer.BlockCopy(md5sum, 0, key, i, md5sum.Length);
                i += md5sum.Length;
            }
            return key;
        }


        

    }
}
