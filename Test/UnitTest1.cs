using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using MyShadowsocks.Encryption;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
  

        private void RunEncrypttionRound(MbedEncryptor encryptor, MbedEncryptor decryptor) {
            byte[] plain = new byte[6657];
            byte[] cipher = new byte[plain.Length + 16];
            byte[] plain2 = new byte[plain.Length + 16];
            int outLen = 0;
            int outLen2 = 0;

            var random = new Random();
            random.NextBytes(plain);

            encryptor.EncryptFirstPackage(plain, plain.Length, cipher, out outLen);
            decryptor.DecryptFirstPackage(cipher, outLen, plain2, out outLen2);
            Assert.AreEqual(plain.Length, outLen2);
            for(int i = 0;i < outLen2;++i) {
                Assert.AreEqual(plain[i], plain2[i]);
            }

            encryptor.Encrypt(plain, 1000, cipher, out outLen);
            decryptor.Decrypt(cipher, outLen, plain2, out outLen2);
            Assert.AreEqual(outLen2 , 1000);
            for(int i = 0;i < outLen2;++i) {
                Assert.AreEqual(plain[i], plain2[i]);
            }

            encryptor.Encrypt(plain, 1233, cipher, out outLen);
            decryptor.Decrypt(cipher, outLen, plain2, out outLen2);
            Assert.AreEqual(outLen2, 1233);
            for(int i = 0;i < outLen2;++i) {
                Assert.AreEqual(plain[i], plain2[i]);
            }

        }


        private async Task RunEncryptionMultiThread(MbedEncryptor encryptor, MbedEncryptor decryptor) {
            byte[] plain = new byte[16384];
            byte[] cipher = new byte[plain.Length + 16];
            byte[] plainOut = new byte[plain.Length + 16];
            int outLen = 0;
            int outLen2 = 0;

            byte[] plain2 = new byte[16384];
            byte[] cipher2 = new byte[plain.Length + 16];
            byte[] plainOut2 = new byte[plain.Length + 16];
            int outLen3 = 0;
            int outLen4 = 0;


            var random = new Random();
            random.NextBytes(plain);
            random.NextBytes(plain2);

            //two task share same encryptor
            Task t1 = Task.Run(() => {
                encryptor.EncryptFirstPackage(plain, plain.Length, cipher, out outLen);
                decryptor.DecryptFirstPackage(cipher, outLen, plainOut, out outLen2);
                Assert.AreEqual(plain.Length, outLen2);
                for(int i = 0;i < outLen2;++i) {
                    Assert.AreEqual(plain[i], plainOut[i]);
                }

                encryptor.Encrypt(plain, 1000, cipher, out outLen);
                decryptor.Decrypt(cipher, outLen, plainOut, out outLen2);
                Assert.AreEqual(outLen2, 1000);
                for(int i = 0;i < outLen2;++i) {
                    Assert.AreEqual(plain[i], plainOut[i]);
                }
                
                encryptor.Encrypt(plain, 12333, cipher, out outLen);
                decryptor.Decrypt(cipher, outLen, plainOut, out outLen2);
                Assert.AreEqual(outLen2, 12333);
                for(int i = 0;i < outLen2;++i) {
                    Assert.AreEqual(plain[i], plainOut[i]);
                }
                Console.WriteLine("task1");
            });
            
            Task t2 = Task.Run(() => {
                Console.WriteLine("task2");
                encryptor.EncryptFirstPackage(plain2, plain2.Length, cipher2, out outLen3);
                decryptor.DecryptFirstPackage(cipher2, outLen3, plainOut2, out outLen4);
                Assert.AreEqual(plain2.Length, outLen4);
                for(int i = 0;i < outLen4;++i) {
                    Assert.AreEqual(plain2[i], plainOut2[i]);
                }

                encryptor.Encrypt(plain2, 1000, cipher2, out outLen3);
                decryptor.Decrypt(cipher2, outLen3, plainOut2, out outLen4);
                Assert.AreEqual(outLen4, 1000);
                for(int i = 0;i < outLen4;++i) {
                    Assert.AreEqual(plain2[i], plainOut2[i]);
                }

                encryptor.Encrypt(plain2, 13333, cipher2, out outLen3);
                decryptor.Decrypt(cipher2, outLen3, plainOut2, out outLen4);
                Assert.AreEqual(outLen4, 13333);
                for(int i = 0;i < outLen4;++i) {
                    Assert.AreEqual(plain2[i], plainOut2[i]);
                }
            });
            await t1;
            await t2;

        }




        [TestMethod]
        public void RunSingleThreadTest() {
            try {
                for(int i = 0;i < 100;++i) {
                    string password = "barfoo";
                    MbedEncryptor encryptor = new MbedEncryptor("aes-256-cfb", true);
                    encryptor.SetKV(password);
                    MbedEncryptor decryptor = new MbedEncryptor("aes-256-cfb", false);
                    decryptor.SetKey(password);
                    RunEncrypttionRound(encryptor, decryptor);
                }
            } catch {
                throw;
            }
        }
    }
}
