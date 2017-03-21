using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OptimizationTests {

    class Program {
        static bool stop = false;
        static void Main(string[] args) {

            Task.Run(() => {
                ITestRunner test = new TestStringIndexOf();
                test.Prep();
                while (!stop) {
                    test.Run();
                    Thread.Sleep(40);
                }
            });

            Console.ReadKey();
            stop = true;
        }
    }
}
