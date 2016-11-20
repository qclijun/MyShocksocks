using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    class DisposeExample : IDisposable
    {

        private MemoryStream ms;

        private bool _disposed = false;

        public DisposeExample()
        {
            ms = new MemoryStream();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            Dispose();
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                //free unmanaged resources
                if (ms != null) ms.Dispose();
            }
            //free managed resources

            _disposed = true;
        }
    }
}
