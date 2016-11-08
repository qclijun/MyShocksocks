using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace Sample
{

    struct A
    {
        public int x;
        public int y;
    }

    struct B
    {
       public A a;
       public int z;
    }

    [Serializable]
    class MyObject
    {
        public int n1 = 0;
        private int n2 = 0;
        string str = "";

        internal MyObject(int xx)
        {
            n2 = xx;
        }

        public MyObject() { }

        public MyObject(int xx,int yy)
        {
            n2 = xx;
        }

        public int N2 { get;
           // set;
        }
        
        public void setStr(string txt)
        {
            str = txt;
        }
       

        public override string ToString()
        {
            return $"Object {base.ToString()} n1={n1},n2={n2}, N2={N2} str={str}";
        }
    }

    class Program
    {
        private static int GetFreePort()
        {
            int defaultPort = 8123;
            try
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

                HashSet<int> used = new HashSet<int>();
                foreach(var ep in tcpEndPoints)
                {
                    used.Add(ep.Port);
                }
                foreach(var i in used)
                {
                    Console.WriteLine(i);
                }

                for(int port = defaultPort; port <= 65535; ++port)
                {
                    if (!used.Contains(port)) return port;
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            throw new Exception("No free port found.");
        }



        static void Main(string[] args)
        {

            string currPath = Directory.GetCurrentDirectory();
            MyObject obj = new MyObject(55);
            obj.n1 = 33;
            //obj.N2 = 34556;
            obj.setStr("woooo");
            Console.WriteLine(obj);

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("MyFile.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, obj);
            stream.Close();

            formatter = new BinaryFormatter();
            stream = new FileStream("MyFile.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            MyObject obj2 = (MyObject)formatter.Deserialize(stream);
            stream.Close();

            Console.WriteLine(obj2);


            //JavaScript serializer only public fields or properties.
            var serializer = new JavaScriptSerializer();
            var result2 = serializer.Serialize(obj);
            Console.WriteLine(result2);

            var result = JsonConvert.SerializeObject(obj);
            Console.WriteLine(result);
            MyObject obj3= JsonConvert.DeserializeObject<MyObject>(result);
            Console.WriteLine("jsonConvert: "+obj3);

            Console.WriteLine("Here");
            Console.ReadLine();
        }
    }
}
