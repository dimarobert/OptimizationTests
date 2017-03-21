using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OptimizationTests {
    class Program {
        static bool stop = false;
        static void Main(string[] args) {

            var watch = new Stopwatch();

            Task.Run(() => {
                var method = typeof(string).GetMethod("IndexOf", new Type[] { typeof(char) });
                Func<string, char, int> compiled = (Func<string, char, int>)Delegate.CreateDelegate(typeof(Func<string, char, int>), method);

                while (!stop) {
                    watch.Reset();
                    watch.Start();
                    "Hello".IndexOf('l');
                    watch.Stop();
                    Console.WriteLine($"Direct: {watch.ElapsedTicks}");

                    watch.Reset();
                    watch.Start();
                    method.Invoke("Hello", new object[] { 'l' });
                    watch.Stop();
                    Console.WriteLine($"Reflection: {watch.ElapsedTicks}");

                    watch.Reset();



                    watch.Start();
                    compiled("MyHello", 'l');
                    watch.Stop();
                    Console.WriteLine($"Compiled: {watch.ElapsedTicks}");

                    Thread.Sleep(400);
                    Console.Clear();
                }
            });

            Console.ReadKey();
            stop = true;
        }
    }
}
