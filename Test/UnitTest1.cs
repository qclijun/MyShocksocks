using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsTrue(Sample.MyClass.CompareFileContent("wo.txt", "abc.txt"));
            Assert.IsTrue(Sample.MyClass.CompareFileContent("gfwlist.txt", @"C:\Users\Jun\Documents\Visual Studio 2015\Projects\MyShadowsocks-windows\Test\bin\Debug\gfwlist2.txt"));
        }
    }
}
