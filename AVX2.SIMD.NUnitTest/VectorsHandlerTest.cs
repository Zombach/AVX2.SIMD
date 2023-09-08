namespace AVX2.SIMD.NUnitTest
{
    public class Tests
    {
        private VectorsHandler _vectorsHandler;

        [SetUp]
        public void Setup() => _vectorsHandler = new();

        [Test]
        public void Test1()
        {
            Io io = new();
            List<string> queries = io.GetLines("номера_запросы.txt").GetRange(0, 100);
            List<string> laws = io.GetLines("номера_законов.txt");
            List<string> decisions = io.GetLines("номера_судебных_решений.txt");
            _vectorsHandler.Start(queries, laws, decisions);
            Assert.Pass();
        }
    }
}