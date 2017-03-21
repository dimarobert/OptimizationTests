using System;
using System.Collections.Generic;
using System.Linq;

namespace OptimizationTests {
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
}
