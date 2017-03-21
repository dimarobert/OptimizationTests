using System;

namespace OptimizationTests {
    public class TestStringIndexOf : ITestRunner {

        Func<string, char, int> compiled;

        public void Prep() {
            compiled = (Func<string, char, int>)Delegate.CreateDelegate(typeof(Func<string, char, int>), typeof(string).GetMethod("IndexOf", new Type[] { typeof(char) }));

        }

        public void Run() {
            var directResult = Clock.BenchmarkTime(() => "Hello".IndexOf('H'));
            var reflResult = Clock.BenchmarkTime(() => typeof(string).GetMethod("IndexOf", new Type[] { typeof(char) }).Invoke("Hello", new object[] { 'H' }));
            var complResult = Clock.BenchmarkTime(() => compiled("MyHello", 'M'));

            Console.Clear();
            Console.WriteLine($"Direct: {directResult.average}");
            Console.WriteLine($"Reflection: {reflResult.average}");
            Console.WriteLine($"Compiled: {complResult.average}");
        }
    }
}
