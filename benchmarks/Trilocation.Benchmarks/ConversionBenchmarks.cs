using BenchmarkDotNet.Attributes;
using Trilocation.Core;

namespace Trilocation.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 5)]
    public class ConversionBenchmarks
    {
        private TriLocation _locationRes5;
        private TriLocation _locationRes15;
        private TriLocation _locationRes24;
        private TriLocation _locationRes30;

        [GlobalSetup]
        public void Setup()
        {
            _locationRes5 = new TriLocation(60.17, 24.94, 5);
            _locationRes15 = new TriLocation(60.17, 24.94, 15);
            _locationRes24 = new TriLocation(60.17, 24.94, 24);
            _locationRes30 = new TriLocation(60.17, 24.94, 30);
        }

        [Benchmark]
        public TriLocation LatLonToTriLocation_Res5()
            => new TriLocation(60.17, 24.94, 5);

        [Benchmark]
        public TriLocation LatLonToTriLocation_Res15()
            => new TriLocation(60.17, 24.94, 15);

        [Benchmark]
        public TriLocation LatLonToTriLocation_Res24()
            => new TriLocation(60.17, 24.94, 24);

        [Benchmark]
        public TriLocation LatLonToTriLocation_Res30()
            => new TriLocation(60.17, 24.94, 30);

        [Benchmark]
        public (double, double) TriLocationToLatLon_Res5()
            => _locationRes5.ToLatLon();

        [Benchmark]
        public (double, double) TriLocationToLatLon_Res15()
            => _locationRes15.ToLatLon();

        [Benchmark]
        public (double, double) TriLocationToLatLon_Res24()
            => _locationRes24.ToLatLon();

        [Benchmark]
        public (double, double) RoundTrip_Res15()
        {
            var loc = new TriLocation(60.17, 24.94, 15);
            return loc.ToLatLon();
        }

        [Benchmark]
        public (double, double) RoundTrip_Res24()
        {
            var loc = new TriLocation(60.17, 24.94, 24);
            return loc.ToLatLon();
        }
    }
}
