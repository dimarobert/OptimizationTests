using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OptimizationTests {

    public class Clock {
        interface IStopwatch {
            bool IsRunning { get; }
            TimeSpan Elapsed { get; }

            void Start();
            void Stop();
            void Reset();
        }



        class TimeWatch : IStopwatch {
            Stopwatch stopwatch = new Stopwatch();

            public TimeSpan Elapsed {
                get { return stopwatch.Elapsed; }
            }

            public bool IsRunning {
                get { return stopwatch.IsRunning; }
            }



            public TimeWatch() {
                if (!Stopwatch.IsHighResolution)
                    throw new NotSupportedException("Your hardware doesn't support high resolution counter");

                //prevent the JIT Compiler from optimizing Fkt calls away
                long seed = Environment.TickCount;

                //use the second Core/Processor for the test
                Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2);

                //prevent "Normal" Processes from interrupting Threads
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

                //prevent "Normal" Threads from interrupting this thread
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
            }



            public void Start() {
                stopwatch.Start();
            }

            public void Stop() {
                stopwatch.Stop();
            }

            public void Reset() {
                stopwatch.Reset();
            }
        }



        class CpuWatch : IStopwatch {
            TimeSpan startTime;
            TimeSpan endTime;
            bool isRunning;



            public TimeSpan Elapsed {
                get {
                    if (IsRunning)
                        throw new NotImplementedException("Getting elapsed span while watch is running is not implemented");

                    return endTime - startTime;
                }
            }

            public bool IsRunning {
                get { return isRunning; }
            }



            public void Start() {
                startTime = Process.GetCurrentProcess().TotalProcessorTime;
                isRunning = true;
            }

            public void Stop() {
                endTime = Process.GetCurrentProcess().TotalProcessorTime;
                isRunning = false;
            }

            public void Reset() {
                startTime = TimeSpan.Zero;
                endTime = TimeSpan.Zero;
            }
        }



        public static (double normalized, double average) BenchmarkTime(Action action, int iterations = 10000) {
            return Benchmark<TimeWatch>(action, iterations);
        }

        static (double normalized, double average) Benchmark<T>(Action action, int iterations) where T : IStopwatch, new() {
            //clean Garbage
            GC.Collect();

            //wait for the finalizer queue to empty
            GC.WaitForPendingFinalizers();

            //clean Garbage
            GC.Collect();

            //warm up
            action();

            var stopwatch = new T();
            var timings = new double[10];
            for (int i = 0; i < timings.Length; i++) {
                stopwatch.Reset();
                stopwatch.Start();
                for (int j = 0; j < iterations; j++)
                    action();
                stopwatch.Stop();
                timings[i] = stopwatch.Elapsed.TotalMilliseconds;
            }
            return (timings.NormalizedMean(), timings.Average());
        }

        public static (double normalized, double average) BenchmarkCpu(Action action, int iterations = 10000) {
            return Benchmark<CpuWatch>(action, iterations);
        }
    }

    public static class ICollectionExt {

        public static double NormalizedMean(this ICollection<double> values) {
            if (values.Count == 0)
                return double.NaN;

            var deviations = values.Deviations().ToArray();
            var meanDeviation = deviations.Sum(t => Math.Abs(t.mean)) / values.Count;
            return deviations.Where(t => t.mean > 0 || Math.Abs(t.mean) <= meanDeviation).Average(t => t.val);
        }

        public static IEnumerable<(double val, double mean)> Deviations(this ICollection<double> values) {
            if (values.Count == 0)
                yield break;

            var avg = values.Average();
            foreach (var d in values)
                yield return (d, avg - d);
        }
    }

    class Program {
        static bool stop = false;
        static void Main(string[] args) {

            var watch = new Stopwatch();

            Task.Run(() => {
                Func<string, char, int> compiled = (Func<string, char, int>)Delegate.CreateDelegate(typeof(Func<string, char, int>), typeof(string).GetMethod("IndexOf", new Type[] { typeof(char) }));

                while (!stop) {
                    var directResult = Clock.BenchmarkTime(() => "Hello".IndexOf('H'));
                    var reflResult = Clock.BenchmarkTime(() => typeof(string).GetMethod("IndexOf", new Type[] { typeof(char) }).Invoke("Hello", new object[] { 'H' }));
                    var complResult = Clock.BenchmarkTime(() => compiled("MyHello", 'M'));

                    Console.Clear();
                    Console.WriteLine($"Direct: {directResult.average}");
                    Console.WriteLine($"Reflection: {reflResult.average}");
                    Console.WriteLine($"Compiled: {complResult.average}");

                    Thread.Sleep(40);
                }
            });

            Console.ReadKey();
            stop = true;
        }
    }
}
