using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OptimizationTests {

    public class TestStringFormat : ITestRunner {
        object[] parameter, parameters;
        Func<string, object[], string> compiled;

        public void Prep() {
            compiled = (Func<string, object[], string>)Delegate.CreateDelegate(typeof(Func<string, object[], string>), typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object[]) }));

            parameter = new object[] { "cool" };
            parameters = new object[] { "{0}", parameter };
        }

        public void Run() {

            var directResult = Clock.BenchmarkTime(() => string.Format("{0}", parameter));
            var reflResult = Clock.BenchmarkTime(() => typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object[]) }).Invoke(null, parameters));
            var complResult = Clock.BenchmarkTime(() => compiled("{0}", parameter));

            Console.Clear();
            Console.WriteLine($"Direct: {directResult.average}");
            Console.WriteLine($"Reflection: {reflResult.average}");
            Console.WriteLine($"Compiled: {complResult.average}");

        }
    }
}
