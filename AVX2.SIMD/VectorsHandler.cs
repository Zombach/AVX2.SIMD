using System.Numerics;
using System.Text;

namespace AVX2.SIMD;

//https://learn.microsoft.com/ru-ru/dotnet/standard/simd
public class VectorsHandler
{
    public void Start(List<string> queries, List<string> laws, List<string> decisions)
    {
        if (!Vector.IsHardwareAccelerated)
        {
            Console.WriteLine("Не поддерживается системой");
            return;
        }

        DateTime time = DateTime.Now;
        int count = 0;
        queries.ForEach(query =>
        {
            byte[] checkBytes = Encoding.UTF8.GetBytes(query);

            List<int> lawsIndexes = new();
            for (int i = 0; i < laws.Count; i++)
            {
                byte[] textBytes = Encoding.UTF8.GetBytes(laws[i]);
                bool isOk = Contains(textBytes, checkBytes);
                if (isOk) { lawsIndexes.Add(i); }
            }

            List<int> decisionsIndexes = new();
            for (int i = 0; i < decisions.Count; i++)
            {
                byte[] textBytes = Encoding.UTF8.GetBytes(decisions[i]);
                bool isOk = Contains(textBytes, checkBytes);
                if (isOk) { decisionsIndexes.Add(i); }
            }

            Console.WriteLine($"Запрос № {count++}: {query}");
            if (lawsIndexes.Count is 0 && decisionsIndexes.Count is 0) { Console.WriteLine("Совпадений не найдено"); }
            if (lawsIndexes.Count > 0)
            {
                List<int> indexes = lawsIndexes.Count <= 3 ? lawsIndexes : lawsIndexes.GetRange(0, 3);
                Console.WriteLine($"Найдено законов: {lawsIndexes.Count}");
                Console.WriteLine(string.Join("\r\n", indexes.Select(index => laws[index])));
                if (lawsIndexes.Count > 3) { Console.WriteLine("..."); }
            }
            if (decisionsIndexes.Count > 0)
            {
                List<int> indexes = decisionsIndexes.Count <= 3 ? decisionsIndexes : decisionsIndexes.GetRange(0, 3);
                Console.WriteLine($"Найдено судебных решений: {decisionsIndexes.Count}");
                Console.WriteLine(string.Join("\r\n", indexes.Select(index => decisions[index])));
                if (decisionsIndexes.Count > 3) { Console.WriteLine("..."); }
            }

            Console.WriteLine();
        });
        DateTime time2 = DateTime.Now;
        Console.WriteLine(time2 - time);
    }

    private bool Contains(byte[] textBytes, byte[] checkBytes)
    {
        int vectorSize = Vector<byte>.Count;

        int indexText = 0;
        int indexByte = 0;
        for (; indexByte < checkBytes.Length; indexByte++)
        {
            Vector<byte> byteVector = new(checkBytes[indexByte]);

            bool isNext = false;
            for (; indexText < textBytes.Length - vectorSize; indexText += vectorSize)
            {
                Vector<byte> textVector = new(textBytes, indexText);

                Vector<byte> result = Vector.Equals(textVector, byteVector);

                if (Vector.GreaterThanAny(result, Vector<byte>.Zero))
                {
                    int index = IndexOfValue(result, 255) + 1;
                    indexText += index;
                    isNext = true;
                    break;
                }
            }
            if (!isNext) { break; }
        }
        if (indexByte == checkBytes.Length) { return true; }
        int remaining = textBytes.Length % vectorSize;
        for (int i = textBytes.Length - remaining; i < textBytes.Length; i++)
        {
            if (textBytes[i] == checkBytes[indexByte]) { indexByte++; }
            if (indexByte == checkBytes.Length) { return true; }
        }
        return false;
    }

    private int IndexOfValue(Vector<byte> vector, byte value)
    {
        int index = 0;
        for (int i = 0; i < Vector<byte>.Count; i++)
        {
            if (vector[i] == value)
            {
                index = i;
                break;
            }
        }
        return index;
    }
}