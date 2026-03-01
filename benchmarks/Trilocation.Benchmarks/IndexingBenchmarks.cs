using BenchmarkDotNet.Attributes;
using Trilocation.Core;
using Trilocation.Core.Indexing;

namespace Trilocation.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 5)]
    public class IndexingBenchmarks
    {
        private ulong _indexRes10;
        private ulong _indexRes20;
        private ulong _indexRes30;

        [GlobalSetup]
        public void Setup()
        {
            var loc10 = new TriLocation(60.17, 24.94, 10);
            var loc20 = new TriLocation(60.17, 24.94, 20);
            var loc30 = new TriLocation(60.17, 24.94, 30);
            _indexRes10 = loc10.Index;
            _indexRes20 = loc20.Index;
            _indexRes30 = loc30.Index;
        }

        [Benchmark]
        public int GetResolution_Res10() => CumulativeIndex.GetResolution(_indexRes10);

        [Benchmark]
        public int GetResolution_Res20() => CumulativeIndex.GetResolution(_indexRes20);

        [Benchmark]
        public int GetResolution_Res30() => CumulativeIndex.GetResolution(_indexRes30);

        [Benchmark]
        public ulong GetParent_Res10() => CumulativeIndex.GetParent(_indexRes10);

        [Benchmark]
        public ulong GetParent_Res20() => CumulativeIndex.GetParent(_indexRes20);

        [Benchmark]
        public ulong[] GetChildren_Res10() => CumulativeIndex.GetChildren(_indexRes10);

        [Benchmark]
        public ulong CumulativeCount_All()
        {
            ulong sum = 0;
            for (int i = 0; i <= 30; i++)
            {
                sum += CumulativeIndex.CumulativeCount(i);
            }
            return sum;
        }
    }
}
