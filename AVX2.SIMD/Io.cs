namespace AVX2.SIMD;

public class Io
{
    public Io() => Console.InputEncoding = System.Text.Encoding.GetEncoding("utf-16");

    public int GetDigital(string message)
    {
        Console.Write($"{message} ");
        int result;
        string? line = null;
        do
        {
            if (line is not null)
            {
                Console.WriteLine("Укажите правильное число.");
            }
            line = Console.ReadLine();
        } while (!int.TryParse(line, out result));

        return result;
    }

    public string GetLine(string message)
    {
        Console.Write($"{message} ");
        string? line = null;
        do
        {
            if (line == string.Empty)
            {
                Console.WriteLine("Укажите значение.");
            }
            line = Console.ReadLine();
        } while (string.IsNullOrEmpty(line));

        return line;
    }

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