using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Encryption
{
    public enum EncryptorType
    {
        RC4 = 1,
        AES = 2,
        Blowfish = 3,
        Camellia = 4,
    }

    public struct IVEncryptorInfo
    {
        public int KeySize;
        public int IVSize;
        public EncryptorType Type;
        public string InnerLibName;

        public IVEncryptorInfo(string innerLibName, int keySize, int ivSize, EncryptorType type)
        {
            KeySize = keySize;
            IVSize = ivSize;
            Type = type;
            InnerLibName = innerLibName;
        }

        public IVEncryptorInfo(int keySize, int ivSize, EncryptorType type)
            : this("", keySize, ivSize, type)
        {

        }
    }
}
