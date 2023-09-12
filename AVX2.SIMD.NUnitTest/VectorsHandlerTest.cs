using System.Text;
using AVX2.SIMD.Benchmark;
using NUnit.Framework;

namespace AVX2.SIMD.NUnitTest
{
    public class Tests
    {
        private VectorsHandler _vectorsHandler;
        private IEnumerable<IEnumerable<byte>> _queries;
        private IEnumerable<IEnumerable<byte>> _laws;
        private IEnumerable<IEnumerable<byte>> _decisions;

        [SetUp]
        public void Setup()
        {
            _vectorsHandler = new();
            Io io = new();
            _queries = io.GetLines($"номера_запросы.txt");
            _laws = io.GetLines("номера_законов.txt");
            _decisions = io.GetLines("номера_судебных_решений.txt");
        }

        [TestCase(1)]
        [TestCase(25)]
        [TestCase(50)]
        [TestCase(100)]
        [TestCase(1000)]
        public void Test1(int count)
        {
            _vectorsHandler.Start(_queries.Take(count), _laws, _decisions);
        }

        [TestCase("118/81")]
        [TestCase("14Ю1")]
        [TestCase("14/81")]
        [TestCase("118/41")]
        public void Test1(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            _queries = new List<IEnumerable<byte>> { bytes };
            _vectorsHandler.Start(_queries, _laws, _decisions);
        }
    }
}