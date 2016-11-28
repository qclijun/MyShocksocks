
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;

class Program {

    static void Main() {
        

        
        

        Console.WriteLine("Ready to exit.");
        Console.ReadKey();
    }

    static void f(object o) {

    }

    static Func<int> Natural() {
        int seed = 0;
        return () => seed++;
    }

}
