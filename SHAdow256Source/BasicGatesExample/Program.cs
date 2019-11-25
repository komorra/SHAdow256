using Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicGatesExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = Gate.CreateVar("A");
            var b = Gate.CreateVar("B");
            var c = Gate.CreateVar("C");

            var q = !(a ^ b ^ c);

            a.Value = true;
            b.Value = true;
            c.Value = false;

            Console.WriteLine("Created following gates:");
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);
            Console.WriteLine("The output is !(A ^ B ^ C):" + q);
            Console.WriteLine();

            var expanded = q.Expand();
            Console.WriteLine("Output gate can be expanded into: " + expanded.Count + " gates");
            Console.WriteLine("These gates are:");
            foreach(var ex in expanded)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine();

            var result = q.Evaluate();
            Console.WriteLine("Result of entire system !(A ^ B ^ C) is: " + result);

            Console.ReadKey();
        }
    }
}
