using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sample
{
    [Serializable]
    class MySerializableClass 
    {
        public static void SerializeObj(string filename)
        {
            Employee emp1 = new Employee("aaa","bbb");
            XmlSerializer x = new XmlSerializer(typeof(Employee));
            using(var writer = new StreamWriter(filename))
            {
                x.Serialize(writer, emp1);
            }
        } 
    }

    

    public class Employee
    {
        public string EmpName;
        public string EmpID;
        private int xx =i++;
        private static int i = 0;

        public int XX { get { return xx; } }

        public Employee() { }
        public Employee(string empName, string empID)
        {
            EmpName = empName;
            EmpID = empID;
        }
    }
}
