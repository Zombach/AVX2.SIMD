using System.Text;

namespace AVX2.SIMD.Benchmark;

public class Io
{
    public Io() => Console.InputEncoding = Encoding.GetEncoding("utf-16");

    public List<byte[]> GetLines(string path)
    {
        try
        {
            if (!File.Exists(path)) { throw new Exception("Отсутствует файл по данному пути"); }
            return File.ReadAllLines(path).Select(Encoding.UTF8.GetBytes).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}\r\nОтсутствует файл");
            throw;
        }
    }
}