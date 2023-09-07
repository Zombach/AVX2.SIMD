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
    private List<string> _queries;
    private readonly List<string> _laws;
    private readonly List<string> _decisions;
    public BenchmarkSimd()
    {
        _vectorsHandler = new();
        Io io = new();
        _queries = io.GetLines("номера_запросы.txt").GetRange(0, 100);
        _laws = io.GetLines("номера_законов.txt");
        _decisions = io.GetLines("номера_судебных_решений.txt");
    }

    [Benchmark]
    public void BenchmarkSimdTest_1()
    {
        _vectorsHandler.Start(_queries, _laws, _decisions);
    }

    [Benchmark]
    public void BenchmarkSimdTest_2()
    {
        _queries = new() { "11/81", "134/5-1" };
        _vectorsHandler.Start(_queries, _laws, _decisions);
    }
}