using System;
using System.Runtime.Serialization;

namespace MyShadowsocks.Encryption {
    [Serializable]
    internal class EncryptorException : Exception {
        public EncryptorException() {
        }

        public EncryptorException(string message) : base(message) {
        }

        public EncryptorException(string message, Exception innerException) : base(message, innerException) {
        }

        protected EncryptorException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}