using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Encryption {
    public class MbedEncryptor : IDisposable {

        private IntPtr _ctx = IntPtr.Zero;

        public bool Initialized => _ctx != IntPtr.Zero;

        public bool Finished { get; private set; } = false;


        public string Method {
            get; private set;
        }

        public bool IsCipher { get; private set; } = true;

        public IVEncryptorInfo Info { get; private set; }

        public byte[] IV { get; private set; }
        public byte[] Key { get; private set; }

        public int KeySize => Info.KeySize;
        public int IVSize => Info.IVSize;
        public string FormalName => Info.InnerLibName;




        public MbedEncryptor(string method) {
            Method = method.ToLower();
            Info = _ciphers[Method]; //找不到就会抛出异常
            Init();
        }

        public MbedEncryptor(string method, bool isCipher) : this(method) {
            IsCipher = isCipher;
        }

        private void Init() {
            IntPtr ctx = Marshal.AllocHGlobal(MbedTLS.cipher_get_size_ex());
            MbedTLS.cipher_init(ctx);
            if(MbedTLS.cipher_setup(ctx, MbedTLS.cipher_info_from_string(FormalName)) != 0)
                throw new EncryptorException("Cannot initialize mbed TLS cipher  context.");
            _ctx = ctx;
        }

        public void SetMethod(string method) {
            Method = method;
            Info = _ciphers[method];
            if(MbedTLS.cipher_setup(_ctx, MbedTLS.cipher_info_from_string(Info.InnerLibName)) != 0)
                throw new EncryptorException("Cannot initialize mbed TLS cipher  context.");
        }


        #region IVEncryptorInfo Cache       
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
        #endregion

        private void Rc4Md5_UpdateKey() {
            Contract.Assert(Key != null);
            Contract.Assert(IV != null);

            byte[] temp = new byte[KeySize + IVSize];
            Array.Copy(Key, 0, temp, 0, KeySize);
            Array.Copy(IV, 0, temp, KeySize, IVSize);
            MbedTLS.MD5(temp, Key);
        }

        public void SetKey(string password) {
            Key = CreateKey(password, Info.KeySize);
            if(Method == "rc4-md5") {
                Rc4Md5_UpdateKey();
            }
            if(MbedTLS.cipher_setkey(_ctx, Key, KeySize * 8, IsCipher ? MbedTLS.MbedTLS_Encrypt :
                MbedTLS.MbedTLS_Decrypt) != 0)
                throw new EncryptorException("Cannot set mbed TLS cipher key.");
            Reset();
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

        private static void GenerateIV(byte[] buffer, int ivSize) {
            Contract.Requires(buffer.Length >= ivSize);
            using(RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
                rng.GetBytes(buffer, 0, ivSize);
            }
        }
        public static byte[] CreateIV(int ivSize) {
            byte[] ret = new byte[ivSize];
            GenerateIV(ret, ivSize);
            return ret;
        }


        public void SetIV(byte[] iv) {
            Contract.Requires(Initialized);
            Contract.Requires(iv != null && iv.Length >= IVSize);

            IV = iv;
            if(Method == "rc4-md5") {
                Rc4Md5_UpdateKey();
            }
            if(MbedTLS.cipher_set_iv(_ctx, iv, IVSize) != 0)
                throw new EncryptorException("Cannot set mbed TLS cipher IV.");
            Reset();
        }

        //修改key or iv后需调用。
        private void Reset() {
            if(MbedTLS.cipher_reset(_ctx) != 0)
                throw new EncryptorException("Cannot finalize mbed TLS cipher context.");
            Finished = true;
        }


        public void SetKV(string password, byte[] iv) {
            Contract.Requires(Initialized);
            Contract.Requires(password != null);
            Contract.Requires(iv != null && iv.Length >= IVSize);

            Key = CreateKey(password, Info.KeySize);
            IV = iv;
            if(Method == "rc4-md5") {
                Rc4Md5_UpdateKey();
            }
            if(MbedTLS.cipher_setkey(_ctx, Key, KeySize * 8, IsCipher ? MbedTLS.MbedTLS_Encrypt :
    MbedTLS.MbedTLS_Decrypt) != 0)
                throw new EncryptorException("Cannot set mbed TLS cipher key.");
            if(MbedTLS.cipher_set_iv(_ctx, iv, IVSize) != 0)
                throw new EncryptorException("Cannot set mbed TLS cipher IV.");
            Reset();
        }

        //use random iv
        public void SetKV(string password) {
            SetKV(password, CreateIV(IVSize));
        }


        private void CipherUpdate(byte[] buf, int length, byte[] outBuf) {
            Contract.Requires(buf != outBuf);//outBuf不能跟buf相同,outBuf的长度不小于length+block_size;
            if(_disposed) {
                throw new ObjectDisposedException(this.ToString());
            }
            if(MbedTLS.cipher_update(_ctx, buf, length, outBuf, ref length) != 0)
                throw new EncryptorException("Cannot update mbed TLS cipher context");
            //int ret = MbedTLS.cipher_update(_ctx, buf, length, outBuf, ref length);

        }


        public void EncryptFirstPackage(byte[] buf, int length, byte[] outBuf, out int outLength) {
            Contract.Requires(Finished);
            Contract.Requires(buf.Length >= length);
            Contract.Requires(outBuf.Length >= length + IVSize);
            Contract.Requires(IsCipher);

            outLength = length + IVSize;
            byte[] tempBuf = new byte[outLength];
            CipherUpdate(buf, length, tempBuf);

            //copy iv:
            Buffer.BlockCopy(IV, 0, outBuf, 0, IVSize);

            //copy result
            Buffer.BlockCopy(tempBuf, 0, outBuf, IVSize, length);
        }

        public void Encrypt(byte[] buf, int length, byte[] outBuf, out int outLength) {
            Contract.Requires(Finished);
            Contract.Requires(buf.Length >= length);
            Contract.Requires(outBuf.Length >= length);
            Contract.Requires(IsCipher);

            outLength = length;
            CipherUpdate(buf, length, outBuf);
        }


        public void DecryptFirstPackage(byte[] buf, int length, byte[] outBuf, out int outLength) {
            Contract.Requires(Finished);
            Contract.Requires(length > IVSize); //buf前IVSize个字节为IV
            Contract.Requires(buf.Length >= length);
            Contract.Requires(outBuf.Length >= length - IVSize);
            Contract.Requires(IsCipher == false);

            outLength = length - IVSize;

            //update iv
            if(IV == null) IV = new byte[IVSize];
            //copy first IVSize bytes in 'buf' to 'iv'
            Buffer.BlockCopy(buf, 0, IV, 0, IVSize);
            SetIV(IV);


            byte[] tempBuf = new byte[outLength];
            Buffer.BlockCopy(buf, IVSize, tempBuf, 0, outLength);

            CipherUpdate(tempBuf, outLength, outBuf);
        }

        public void Decrypt(byte[] buf, int length, byte[] outBuf, out int outLength) {
            Contract.Requires(Finished);
            Contract.Requires(buf.Length >= length);
            Contract.Requires(outBuf.Length >= length);
            Contract.Requires(IsCipher == false);

            outLength = length;
            CipherUpdate(buf, length, outBuf);
        }


        #region IDisposable




        private bool _disposed = false;


        public void Dispose() {
            Disponse(true);
            GC.SuppressFinalize(this);
        }

        ~MbedEncryptor() {
            Disponse(false);
        }


        protected virtual void Disponse(bool disposing) {

            if(_disposed)
                return;
            _disposed = true;
            if(disposing) {
                // free managed objects
            }

            //free unmanaged objects
            if(_ctx != IntPtr.Zero) {
                MbedTLS.cipher_free(_ctx);
                Marshal.FreeHGlobal(_ctx);
                _ctx = IntPtr.Zero;
            }

        }

        #endregion




    }
}
