using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Encryption {
    public class MbedTLSEncrytor : IDisposable {

        private IntPtr _encryptCtx = IntPtr.Zero;
        private IntPtr _decryptCtx = IntPtr.Zero;

        public bool EncryptInit => _encryptCtx != IntPtr.Zero;
        public bool DecryptInit => _decryptCtx != IntPtr.Zero;

        public string Method {
            get; private set;
        }
        private string _password;

        public IVEncryptorInfo Info { get; private set; }

        public byte[] IV { get; private set; }

        private byte[] _encryptIV;
        private byte[] _decryptIV;




        public byte[] Key { get; private set; }

        public int KeySize => Info.KeySize;
        public int IVSize => Info.IVSize;
        public string FormalName => Info.InnerLibName;




        public MbedTLSEncrytor(string method,string password) {
            Method = method.ToLower();
            Info = _ciphers[Method]; //找不到就会抛出异常
            _password = password;
            SetKey(password);
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

        public static IEnumerable<string> SupportedMethods() {
            return _ciphers.Keys;
        }


        public void SetKey(string password) {
            Key = CreateKey(password, Info.KeySize);
        }

        private static byte[] CreateKey(string password, int keySize) {
            Contract.Requires(keySize >= 16 && keySize <= 32);
            Contract.Requires(password != null);

            return CreateKey(Encoding.UTF8.GetBytes(password), keySize);

        }

        private static byte[] CreateKey(byte[] bytes, int keySize) {


            byte[] key = new byte[32];
            const int Md5Length = 16;
            byte[] result = new byte[bytes.Length + Md5Length];
            byte[] md5sum = new byte[Md5Length];
            int i = 0;
            while(i < key.Length) {
                if(i == 0)
                    MbedTLS.MD5(bytes, md5sum);
                else {
                    Buffer.BlockCopy(md5sum, 0, result, 0, md5sum.Length);
                    Buffer.BlockCopy(bytes, 0, result, md5sum.Length, bytes.Length);
                    MbedTLS.MD5(result, md5sum);
                }
                Buffer.BlockCopy(md5sum, 0, key, i, md5sum.Length);
                i += md5sum.Length;
            }
            return key;
        }

        public static void GenerateIV(byte[] buffer, int ivSize) {
            Contract.Requires(buffer.Length >= ivSize);
            using(RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
                rng.GetBytes(buffer, 0, ivSize);
            }
        }

        public static byte[]  CreateIV(int ivSize) {
            byte[] ret = new byte[ivSize];
            GenerateIV(ret, ivSize);
            return ret;
        }

        private void init() {

        }


        public void InitCipher(byte[] key, byte[] iv, bool isCipher) {
            IntPtr ctx = Marshal.AllocHGlobal(MbedTLS.cipher_get_size_ex());
            if(isCipher) {
                _encryptIV = iv;
                _encryptCtx = ctx;
            } else {
                _decryptIV = iv;
                _decryptCtx = ctx;
            }
            byte[] realkey = key;
            if(Method == "rc4-md5") {
                byte[] temp = new byte[KeySize + IVSize];
                //realkey = new byte[KeySize];
                Array.Copy(key, 0, temp, 0, KeySize);
                Array.Copy(iv, 0, temp, KeySize, IVSize);
                realkey = MbedTLS.MD5(temp);
            }
            Key = realkey;
            MbedTLS.cipher_init(ctx);
            if(MbedTLS.cipher_setup(ctx, MbedTLS.cipher_info_from_string(FormalName)) != 0)
                throw new EncryptorException("Cannot initialize mbed TLS cipher  context.");
            if(MbedTLS.cipher_setkey(ctx, realkey, KeySize * 8, isCipher ? MbedTLS.MbedTLS_Encrypt :
                MbedTLS.MbedTLS_Decrypt) != 0)
                throw new EncryptorException("Cannot set mbed TLS cipher key.");
            if(MbedTLS.cipher_set_iv(ctx, iv, IVSize) != 0)
                throw new EncryptorException("Cannot set mbed TLS cipher IV.");
            if(MbedTLS.cipher_reset(ctx) != 0)
                throw new EncryptorException("Cannot finalize mbed TLS cipher context.");
        }


        private void CipherUpdate(bool isCipher, int length, byte[] buf, byte[] outBuf) {
            if(_disposed) {
                throw new ObjectDisposedException(this.ToString());

            }
            if(MbedTLS.cipher_update(isCipher ? _encryptCtx : _decryptCtx, buf, length, outBuf, ref length) != 0)
                throw new EncryptorException("Cannot update mbed TLS cipher context");
        }


        public void EncryptFirstPackage(byte[] buf, int length, byte[] outBuf, out int outLength) {
            Contract.Requires(EncryptInit); //必须调用InitCipher(..,..,true);
            Contract.Requires(outBuf.Length >= length + IVSize);
            

            outLength = length + IVSize;
            byte[] tempBuf = new byte[outLength];
            CipherUpdate(true, length, buf, tempBuf);

            //copy iv:
            Buffer.BlockCopy(_encryptIV, 0, outBuf, 0, IVSize);

            //copy result
            Buffer.BlockCopy(tempBuf, 0, outBuf, IVSize, length);
        }

        public void Encrypt(byte[] buf, int length, byte[] outBuf, out int outLength) {
            Contract.Requires(EncryptInit); //必须调用InitCipher(..,..,true);
            Contract.Requires(outBuf.Length >= length);

            outLength = length;
            CipherUpdate(true, length, buf, outBuf);
        }


        public  void DecryptFirstPackage(byte[]buf, int length, byte[] outBuf, out int outLength) {
            Contract.Requires( length > IVSize); //buf前IVSize个字节为IV
            Contract.Requires(outBuf.Length >= length - IVSize);
            Contract.Requires(Key != null); //解密前必须已知key

            outLength = length - IVSize;

            //copy first IVSize bytes in 'buf' to '_decryptIV'
            _decryptIV = new byte[IVSize];
            Buffer.BlockCopy(buf, 0, _decryptIV, 0, IVSize);

            InitCipher(Key, _decryptIV, false);


            byte[] tempBuf = new byte[outLength];
            Buffer.BlockCopy(buf, IVSize, tempBuf, 0, outLength);

            CipherUpdate(false, outLength, tempBuf, outBuf);

        }

        public void Decrypt(byte[] buf,int length, byte[] outBuf, out int outLength) {
            Contract.Requires(DecryptInit);
            Contract.Requires(outBuf.Length >= length);

            outLength = length;
            CipherUpdate(false, length, buf, outBuf);
        }


        #region IDisposable




        private bool _disposed = false;
        private readonly object _lock = new object();

        public void Dispose() {
            Disponse(true);
            GC.SuppressFinalize(this);
        }

        ~MbedTLSEncrytor() {
            Disponse(false);
        }


        protected virtual void Disponse(bool disposing) {
            lock (_lock) {
                if(_disposed)
                    return;
                _disposed = true;
            }
            if(disposing) {
                // free managed objects
            }

            //free unmanaged objects
            if(_encryptCtx != IntPtr.Zero) {
                MbedTLS.cipher_free(_encryptCtx);
                Marshal.FreeHGlobal(_encryptCtx);
                _encryptCtx = IntPtr.Zero;
            }
            if(_decryptCtx != IntPtr.Zero) {
                MbedTLS.cipher_free(_decryptCtx);
                Marshal.FreeHGlobal(_decryptCtx);
                _decryptCtx = IntPtr.Zero;
            }

        }

        #endregion




    }
}
