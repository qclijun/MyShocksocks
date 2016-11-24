using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShadowsocks.Encryption;
using MyShadowsocks.Model;



namespace MyShadowsocks.Controller {
    internal sealed class EncryptorPool : IDisposable {
        

        private ConcurrentBag<MbedEncryptor> encryptorPool;
        private ConcurrentBag<MbedEncryptor> decryptorPool;

        private const int InitEncryptors = 3;

        public EncryptorPool() {

            encryptorPool = new ConcurrentBag<MbedEncryptor>();
            decryptorPool = new ConcurrentBag<MbedEncryptor>();
        }

        public MbedEncryptor GetEncryptor(Server s) {
            MbedEncryptor encryptor;
            if(!encryptorPool.TryTake(out encryptor)) {
                //Not  found
                encryptor = new MbedEncryptor(s.Method, true);

            } else if(encryptor.Method != s.Method) {
                //found but method not match
                encryptor.SetMethod(s.Method);
            }
            //found and method match, set key and iv
            encryptor.SetKV(s.Password);
            return encryptor;
        }

        public MbedEncryptor GetDecryptor(Server s) {
            MbedEncryptor decryptor;
            if(!decryptorPool.TryTake(out decryptor)) {
                //Not  found
                decryptor = new MbedEncryptor(s.Method, false);

            } else if(decryptor.Method != s.Method) {
                //found but method not match
                decryptor.SetMethod(s.Method);
            }
            //found and method match, set key only 
            decryptor.SetKey(s.Password);
            return decryptor;
        }

        public void FreeEncryptor(MbedEncryptor e) {
            if(e != null) {
                if(e.IsCipher) encryptorPool.Add(e);
                else decryptorPool.Add(e);
            }
                
        }


        private bool disposed = false;
        public void Dispose() {
            if(disposed) return;
            disposed = true;
            foreach(var e in encryptorPool) {
                e.Dispose();
            }
            encryptorPool = null;
            foreach(var e in decryptorPool) {
                e.Dispose();
            }
            decryptorPool = null;
        }




    }
}
