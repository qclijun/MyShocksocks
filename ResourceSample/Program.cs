using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using System.Drawing;

namespace ResourceSample {
    class Program {
        static void Main(string[] args) {
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            Assembly a = Assembly.LoadFrom("MyShadowsocks-UI.resources.dll");

            foreach(var name in a.GetManifestResourceNames()) {
                Console.WriteLine(name);
            }
            Console.WriteLine();
            ResourceManager rm = new ResourceManager("MyShadowsocks-UI.g.zh-CN", a);
            ResourceSet set = rm.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach(DictionaryEntry entry in set) {
                Console.WriteLine(entry.Key + " = " + entry.Value);
            }


            // const string imgFile = "ssw128.png";
            // var rw = new ResXResourceWriter("Demo.resx");
            // using(Image image = Image.FromFile(imgFile))
            //{

            //     rw.AddResource("Publisher", "wrox Press");
            //     rw.AddResource("WroxLogo", image);
            //     rw.AddResource("WroxLogo_wpf", File.ReadAllBytes(imgFile));
            //     rw.Close();
            // }




            Console.WriteLine("Here" );
            Console.ReadKey();
        }
    }
}
