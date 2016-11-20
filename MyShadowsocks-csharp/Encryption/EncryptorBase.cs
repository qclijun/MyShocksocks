using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Encryption
{
    public class EncryptorException : Exception,ISerializable
    {
        public EncryptorException():this(""){ }

        public EncryptorException(string message) : this(message, null) { }
        public EncryptorException(string message, Exception inner) : base(message, inner)
        {

        }
    }

    public abstract class EncryptorBase : IEncryptor
    {

        //public const int MaxInputSize = 0x8000;

        //public string Method { get; private set; }
        //public string Password { get; private set; }

        //public bool OneTimeAuth { get; private set; }

        //public bool IsUdp { get; private set; }

        //protected EncryptorBase(string method, string password, bool oneTimeAuth, bool isUdp)
        //{
        //    Method = method.ToLower();
        //    Password = password;
        //    OneTimeAuth = oneTimeAuth;
        //    IsUdp = isUdp;
        //}


        public abstract void Decrypt(byte[] buf, int length, byte[] outbuf, out int outlength);
       
        public abstract void Encrypt(byte[] buf, int length, byte[] outbuf, out int outlength);
    }
}
