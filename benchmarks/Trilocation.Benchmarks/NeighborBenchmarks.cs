using BenchmarkDotNet.Attributes;
using Trilocation.Core;

namespace Trilocation.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 5)]
    public class NeighborBenchmarks
    {
        private TriLocation _locationRes10;
        private TriLocation _locationRes15;

        [GlobalSetup]
        public void Setup()
        {
            _locationRes10 = new TriLocation(60.17, 24.94, 10);
            _locationRes15 = new TriLocation(60.17, 24.94, 15);
        }

        [Benchmark]
        public TriLocation[] GetNeighbors_Res10()
            => _locationRes10.GetNeighbors();

        [Benchmark]
        public TriLocation[] GetNeighbors_Res15()
            => _locationRes15.GetNeighbors();

        [Benchmark]
        public TriLocation[] GetNeighborsWithin_Res10_Ring1()
            => _locationRes10.GetNeighborsWithin(1);

        [Benchmark]
        public TriLocation[] GetNeighborsWithin_Res10_Ring2()
            => _locationRes10.GetNeighborsWithin(2);
    }
}
