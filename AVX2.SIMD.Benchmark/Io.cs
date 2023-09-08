namespace AVX2.SIMD;

public class Io
{
    public Io() => Console.InputEncoding = System.Text.Encoding.GetEncoding("utf-16");

    public IEnumerable<IEnumerable<byte>> GetLines(string path)
    {
        try
        {
            if (!File.Exists(path)) { throw new Exception("Отсутствует файл по данному пути"); }

            using Stream stream = File.OpenRead(path);

            byte[] expectedBytes = "\r\n"u8.ToArray();
            int expectedByte = expectedBytes.First();

            long currentPosition = 0;
            long streamLength = stream.Length;
            List<long> indexes = new() { currentPosition };
            while (currentPosition < streamLength)
            {
                int readByte = stream.ReadByte();
                if (readByte == expectedByte)
                {
                    stream.Position--;
                    bool isEquals = true;
                    foreach (byte item in expectedBytes)
                    {
                        if (item == stream.ReadByte()) { continue; }
                        isEquals = false;
                        break;
                    }

                    if (isEquals) { indexes.Add(currentPosition - expectedBytes.Length); }
                    stream.Position = currentPosition + 1;
                }
                currentPosition++;
            }
            
            List<byte[]> source = new();
            for (int i = 0; i < indexes.Count - 1; i += 2)
            {
                Span<byte> bytes = new(new byte[indexes[i + 1] - indexes[i]]);
                stream.Position = indexes[i];
                int read = stream.Read(bytes);
                source.Add(bytes.ToArray());
            }

            return source;
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}\r\nОтсутствует файл");
            throw;
        }
    }
}