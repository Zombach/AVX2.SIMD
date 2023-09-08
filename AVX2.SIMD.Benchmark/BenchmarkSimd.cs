using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace AVX2.SIMD.Benchmark;

[Config(typeof(Config))]
public class BenchmarkSimd
{
    private class Config : ManualConfig
    {
        public Config()
        {
            AddColumn(new BaselineColumn());
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }

    private readonly VectorsHandler _vectorsHandler;
    private IEnumerable<IEnumerable<byte>> _queries;
    private readonly IEnumerable<IEnumerable<byte>> _laws;
    private readonly IEnumerable<IEnumerable<byte>> _decisions;
    public BenchmarkSimd()
    {
        _vectorsHandler = new();
        Io io = new();
        _queries = io.GetLines("номера_запросы.txt");
        _laws = io.GetLines("номера_законов.txt");
        _decisions = io.GetLines("номера_судебных_решений.txt");
    }

    [Benchmark]
    public void BenchmarkSimdTest_1()
    {
        _vectorsHandler.Start(_queries.Take(100), _laws, _decisions);
    }

    [Benchmark]
    public void BenchmarkSimdTest_2()
    {
        byte[] bytes = "118/18"u8.ToArray();
        _queries = new List<IEnumerable<byte>>() { bytes };
        _vectorsHandler.Start(_queries, _laws, _decisions);
    }
}