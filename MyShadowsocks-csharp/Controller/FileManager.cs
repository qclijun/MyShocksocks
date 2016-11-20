using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MyShadowsocks.Controller
{
    static class FileManager
    {
        public static bool ByteArrayToFile(string fileName, byte[] content)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    fs.Write(content, 0, content.Length);
            }catch(Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex.ToString());
            }
            return false;
        }
        public static void UncompressFile(string fileName, byte[] content)
        {
            //byte[] buffer = new byte[4096];
            //int n;
            using (var fs = File.Create(fileName))
                using(var input = new System.IO.Compression.GZipStream(new MemoryStream(content),
                    System.IO.Compression.CompressionMode.Decompress, false))
            {
                //while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
                //    fs.Write(buffer, 0, n);
                input.CopyTo(fs);
            }
        }

    }
}
