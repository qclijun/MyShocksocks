using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sample
{
    public class MyClass
    {
        public static bool CompareFileContent(string fileName1, string fileName2)
        {
            FileInfo file1 = new FileInfo(fileName1);
            FileInfo file2 = new FileInfo(fileName2);
            if (!file1.Exists) throw new FileNotFoundException($"File not exists: {fileName1}");
            if (!file2.Exists) throw new FileNotFoundException($"File not exists: {fileName2}");
            if (file1.FullName == file2.FullName) return true;
            using (var fs1 = new BufferedStream(file1.OpenRead()))
            using (var fs2 = new BufferedStream(file2.OpenRead()))
            {
                int b1 =0, b2=0;
                while ((b1 = fs1.ReadByte()) != -1 && (b2 = fs2.ReadByte()) != -1)
                {
                    if (b1 != b2) return false;
                }
                if (b1 == -1) //fs2.ReadByte() not invoked because '&&'
                {
                    if ((b2 = fs2.ReadByte()) == -1) return true;
                    else return false;
                }
                //assert b2==-1 && b1!=-1
                return false;
                
            }
        }
    }
}
