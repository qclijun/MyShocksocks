using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Encryption
{

    public abstract class IVEncryptor :EncryptorBase
    {
        public const int MaxKeyLength = 64;
        public const int MaxIVLength = 16;
        public const int MaxInputSize = 0x8000;
        private readonly static byte[] _tempBuf = new byte[MaxInputSize];

        //private Dictionary<string, IVEncryptorInfo> _ciphers;

        //private static readonly ConcurrentDictionary<string, byte[]>
        //    CachedKeys = new ConcurrentDictionary<string, byte[]>();

        private byte[] _encryptIV;
        private byte[] _decryptIV;
        private bool _decryptIVReceived;
        private bool _encryptIVSent;
       // private int _cipher;

        //private string _innerLibName;
        //private IVEncryptorInfo _cipherInfo;
        private byte[] _iv;
        private byte[] _key;
        private int _keyLen;
        private int _ivLen;

        //protected string InnerLibName { get { return _innerLibName; } }

        public int KeyLen { get { return _keyLen; } }
        public int IVLen { get { return _ivLen; } }

        protected byte[] Key { get { return _key; } }

        //protected IVEncryptor(string method, string password, bool oneTimeAuth, bool isUdp)
        //    :base(method,password,oneTimeAuth,isUdp){

        //    //InitKey();
        //}

        //protected abstract Dictionary<string, IVEncryptorInfo> GetCiphers();
        
        private void InitKey()
        {
            string k = Method + ":" + Password;
            _ciphers = GetCiphers();
            
            if(!_ciphers.TryGetValue(Method,out _cipherInfo))
            {
                throw new ArgumentException($"method({Method}) not found");
            }
            _keyLen = _cipherInfo.KeySize;
            _ivLen = _cipherInfo.IVSize;
            _innerLibName = _cipherInfo.InnerLibName;
            _key = CachedKeys.GetOrAdd(k, (nk) =>
            {
                byte[] passBuf = Encoding.UTF8.GetBytes(Password);
                byte[] key = new byte[32];
                BytesToKey(passBuf, key);
                return key;
            });

        }


        public static byte[] GenerateKey(string password)
        {
            return GenerateKey(Encoding.UTF8.GetBytes(password));
        }

        //产生一个长度为32的byte array， 可以取前面的16 24 32作为Key
        public static byte[] GenerateKey(byte[] bytes)
        {            
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


        public static void BytesToKey(byte[] bytes, byte[] key)
        {
            byte[] result = new byte[bytes.Length + 16];
            int i = 0;
            byte[] md5sum = null;
            while (i < key.Length)
            {
                if (i == 0) md5sum = MbedTLS.MD5(bytes);
                else
                {
                    md5sum.CopyTo(result, 0);
                    bytes.CopyTo(result, md5sum.Length);
                    md5sum = MbedTLS.MD5(result);
                }
                md5sum.CopyTo(key, i);
                i += md5sum.Length;
            }
        }

        public static byte[] GenerateIV(int ivLen)
        {
            byte[] iv = new byte[ivLen];
            using(RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }




        protected virtual void InitCipher(byte[] iv, bool isCipher)
        {
            if (_ivLen > 0)
            {
                if (isCipher)
                {
                    
                    _encryptIV = new byte[_ivLen];
                    Array.Copy(iv, _encryptIV, _ivLen);

                }
                else
                {
                    _decryptIV = new byte[_ivLen];
                    Array.Copy(iv, _decryptIV, _ivLen);
                }
            }
        }


        protected abstract void CipherUpdate(bool isCipher, int length, byte[] buf, byte[] outBuf);

        #region OneTimeAuth

        public const int OneTimeAuthFlag = 0x10;
        public const int AddrTypeMask = 0xF;
        public const int OneTimeAuthBytes = 10;
        public const int CLenBytes = 2;
        public const int AuthBytes = OneTimeAuthBytes + CLenBytes;

        private uint _otaChunkCounter;
        private byte[] _otaChunkKeyBuffer;



        private void OtaAuthBuffer(byte[] buf, ref int length)
        {
            if (OneTimeAuth && IVLen > 0)
            {
                if (!IsUdp)
                {
                    OtaAuthBuffer4Tcp(buf, ref length);
                }
                else
                {
                    OtaAuthBuffer4Udp(buf, ref length);
                }
            }
        }

        private void OtaAuthBuffer4Udp(byte[] buf, ref int length)
        {
            throw new NotImplementedException();
        }

        private void OtaAuthBuffer4Tcp(byte[] buf, ref int length)
        {
            throw new NotImplementedException();
        }

        #endregion



        public override void Encrypt(byte[] buf, int length, byte[] outBuf, out int outLength)
        {
            if (!_encryptIVSent)
            {
                //Generate IV
                RandByte(outBuf, _ivLen);
                InitCipher(outBuf, true);
                outLength = length + _ivLen;
                OtaAuthBuffer(buf, ref length);
                _encryptIVSent = true;
                lock (_tempBuf)
                {
                    CipherUpdate(true, length, buf, _tempBuf);
                    outLength = length + IVLen;
                    Array.Copy(_tempBuf, 0, outBuf, IVLen, length);
                }

            }
            else
            {
                OtaAuthBuffer(buf, ref length);
                outLength = length;
                CipherUpdate(true, length, buf, outBuf);
            }
        }

        public override void Decrypt(byte[] buf, int length, byte[] outbuf, out int outlength)
        {
            if (!_decryptIVReceived)
            {
                _decryptIVReceived = true;
                InitCipher(buf, false);
                outlength = length - IVLen;
                lock (_tempBuf)
                {
                    Array.Copy(buf, IVLen, _tempBuf, 0, length - IVLen);
                    CipherUpdate(false, length - IVLen, _tempBuf, outbuf);
                }
            }
            else
            {
                outlength = length;
                CipherUpdate(false, length, buf, outbuf);
            }
        }


        private static void RandByte(byte[] outBuf, int length)
        {
            using(RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(outBuf,0,length);
            }
        }
    }
}
