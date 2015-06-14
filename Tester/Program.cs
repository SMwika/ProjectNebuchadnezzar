using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            LicenseValidator lv = new LicenseValidator("b7337eee-d172-4cc7-a9eb-c180662aa950");

            
            Console.WriteLine("Validation: " + lv.Validate());
            Console.ReadLine();
        }
    }
}
