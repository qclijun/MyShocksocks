using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Encryption
{
    interface IEncryptor 
    {
        void Encrypt(byte[] buf, int length, byte[] outBuf, out int outLength);
        void Decrypt(byte[] buf, int length, byte[] outBuf, out int outLength);
    }
}
