namespace AVX2.SIMD;

public class Io
{
    public Io() => Console.InputEncoding = System.Text.Encoding.GetEncoding("utf-16");
    
    public List<string> GetLines(string path)
    {
        try
        {
            using StreamReader streamReader = new(path);
            return streamReader.ReadToEnd().Split("\r\n").Where(line => !string.IsNullOrEmpty(line)).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}\r\nОтсутствует файл");
            throw;
        }
    }
}