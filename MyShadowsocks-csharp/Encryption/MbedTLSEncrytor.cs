using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Encryption
{
    public class MbedTLSEncrytor : IVEncryptor, IDisposable
    {

        private IntPtr _encryptCtx = IntPtr.Zero;
        private IntPtr _decryptCtx = IntPtr.Zero;

        private MbedTLSEncrytor(string method, string password,bool oneTimeAuth, bool isUdp)
            : base(method, password, oneTimeAuth, isUdp)
        {

        }

        public static IVEncryptor NewInstance(string method, string password, bool oneTimeAuth, bool isUdp)
        {
            return new MbedTLSEncrytor(method, password, oneTimeAuth, isUdp);
        }

        private static Dictionary<string, IVEncryptorInfo> _ciphers = new Dictionary<string, IVEncryptorInfo> {
            { "aes-128-cfb", new IVEncryptorInfo("AES-128-CFB128", 16, 16, EncryptorType.AES) },
            { "aes-192-cfb", new IVEncryptorInfo("AES-192-CFB128", 24, 16, EncryptorType.AES) },
            { "aes-256-cfb", new IVEncryptorInfo("AES-256-CFB128", 32, 16, EncryptorType.AES) },
            { "aes-128-ctr", new IVEncryptorInfo("AES-128-CTR", 16, 16, EncryptorType.AES) },
            { "aes-192-ctr", new IVEncryptorInfo("AES-192-CTR", 24, 16, EncryptorType.AES) },
            { "aes-256-ctr", new IVEncryptorInfo("AES-256-CTR", 32, 16, EncryptorType.AES) },
            { "bf-cfb", new IVEncryptorInfo("BLOWFISH-CFB64", 16, 8, EncryptorType.Blowfish) },
            { "camellia-128-cfb", new IVEncryptorInfo("CAMELLIA-128-CFB128", 16, 16, EncryptorType.Camellia) },
            { "camellia-192-cfb", new IVEncryptorInfo("CAMELLIA-192-CFB128", 24, 16, EncryptorType.Camellia) },
            { "camellia-256-cfb", new IVEncryptorInfo("CAMELLIA-256-CFB128", 32, 16, EncryptorType.Camellia) },
            { "rc4-md5", new IVEncryptorInfo("ARC4-128", 16, 16, EncryptorType.RC4) }
        };



        protected override void InitCipher(byte[] iv, bool isCipher)
        {
            base.InitCipher(iv, isCipher);
            IntPtr ctx = Marshal.AllocHGlobal(MbedTLS.cipher_get_size_ex());
            if (isCipher)
            {
                _encryptCtx = ctx;
            }
            else
            {
                _decryptCtx = ctx;
            }
            byte[] realkey = Key;
            if (Method == "rc4-md5")
            {
                byte[] temp = new byte[KeyLen + IVLen];
                //realkey = new byte[KeyLen];
                Array.Copy(Key, 0, temp, 0, KeyLen);
                Array.Copy(iv, 0, temp, KeyLen, IVLen);
                realkey = MbedTLS.MD5(temp);
            }
            MbedTLS.cipher_init(ctx);
            if (MbedTLS.cipher_setup(ctx, MbedTLS.cipher_info_from_string(InnerLibName)) != 0)
                throw new EncryptorException("Cannot initialize mbed TLS cipher  context.");
            if (MbedTLS.cipher_setkey(ctx, realkey, KeyLen * 8, isCipher ? MbedTLS.MbedTLS_Encrypt :
                MbedTLS.MbedTLS_Decrypt) != 0)
                throw new EncryptorException("Cannot set mbed TLS cipher key.");
            if (MbedTLS.cipher_set_iv(ctx, iv, IVLen) != 0)
                throw new EncryptorException("Cannot set mbed TLS cipher IV.");
            if (MbedTLS.cipher_reset(ctx) != 0)
                throw new EncryptorException("Cannot finalize mbed TLS cipher context.");
        }


        protected override void CipherUpdate(bool isCipher, int length, byte[] buf, byte[] outBuf)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(this.ToString());

            }
            if (MbedTLS.cipher_update(isCipher ? _encryptCtx : _decryptCtx, buf, length, outBuf, ref length) != 0)
                throw new EncryptorException("Cannot update mbed TLS cipher context");
        }



        #region IDisposable




        private bool _disposed = false;
        private readonly object _lock = new object();

        public void Dispose()
        {
            Disponse(true);
            GC.SuppressFinalize(this);
        }

        ~MbedTLSEncrytor()
        {
            Disponse(false);
        }


        protected virtual void Disponse(bool disposing)
        {
            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;
            }
            if (disposing)
            {
                // free managed objects
            }

            //free unmanaged objects
            if (_encryptCtx != IntPtr.Zero)
            {
                MbedTLS.cipher_free(_encryptCtx);
                Marshal.FreeHGlobal(_encryptCtx);
                _encryptCtx = IntPtr.Zero;
            }
            if (_decryptCtx != IntPtr.Zero)
            {
                MbedTLS.cipher_free(_decryptCtx);
                Marshal.FreeHGlobal(_decryptCtx);
                _decryptCtx = IntPtr.Zero;
            }

        }

        #endregion

 


    }
}
