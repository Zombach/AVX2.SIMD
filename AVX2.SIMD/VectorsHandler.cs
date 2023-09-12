using System.Numerics;
using System.Text;

namespace AVX2.SIMD;

//https://learn.microsoft.com/ru-ru/dotnet/standard/simd
public class VectorsHandler
{
    public void Start(IEnumerable<IEnumerable<byte>> queries, IEnumerable<IEnumerable<byte>> lawsSource, IEnumerable<IEnumerable<byte>> decisionsSource)
    {
        //Проверка, наличия поддержки AVX2
        if (!Vector.IsHardwareAccelerated)
        {
            Console.WriteLine("Не поддерживается системой");
            return;
        }
        
        int count = 0;
        DateTime start = DateTime.Now;
        List<byte[]> laws = lawsSource.Select(law => law.ToArray()).ToList();
        List<byte[]> decisions = decisionsSource.Select(decision => decision.ToArray()).ToList();
        using IEnumerator<byte[]> enumerator = queries.Select(query => query.ToArray()).GetEnumerator();
        while (enumerator.MoveNext())
        {
            List<byte[]> lawsResult = GetResults(laws, enumerator.Current);
            List<byte[]> decisionsResult = GetResults(decisions, enumerator.Current);

            Console.WriteLine($"Запрос № {count++}: {ByteToText(enumerator.Current)}");
            if (!lawsResult.Any() && !decisionsResult.Any()) { Console.WriteLine("Совпадений не найдено"); }
            else
            {
                ViewResult(lawsResult, "Найдено законов");
                ViewResult(decisionsResult, "Найдено судебных решений");
            }

            Console.WriteLine();
        }

        DateTime end = DateTime.Now;
        Console.WriteLine(end - start);
    }

    private List<byte[]> GetResults(List<byte[]> source, byte[] checkBytes)
    {
        List<byte[]> values = new();
        using IEnumerator<byte[]> enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            bool isOk = Contains(enumerator.Current, checkBytes);
            if (isOk) { values.Add(enumerator.Current); }
        }

        return values;
    }

    private string ByteToText(IEnumerable<byte> bytes) => Encoding.UTF8.GetString(bytes.ToArray());

    private void ViewResult(List<byte[]> source, string message)
    {
        if (!source.Any()) { return; }
        int count = source.Count;
        Console.WriteLine($"{message}: {count}");
        Console.WriteLine(string.Join("\r\n", source.Take(3).Select(ByteToText)));
        if (count > 3) { Console.WriteLine("..."); }
    }

    /// <summary>
    /// Метод делает проверку, что byte из checkBytes содержаться в textBytes
    /// byte могут содержаться не последовательно, но в порядке очереди checkBytes
    /// Пример textBytes = new byte[] { 1, 2, 3, 4, 5, 3, 4, 7, 8, 2, 3, 4 }
    /// Пример checkBytes = new byte[] { 2, 4, 4, 8, 2 }
    /// для textBytes находим первый элемент из checkBytes => Index 1
    /// checkBytes переходим к следующему элементу '4'
    /// для textBytes смешаемся к Index 2 и начинаем поиск элемента 4 => Index 3
    /// И таким образом, необходимо в textBytes обнаружить все элементы из checkBytes
    /// </summary>
    /// <param name="verifiable">Байтовый набор текста</param>
    /// <param name="mandatoryBytes">Байтовый набор обязательных символов</param>
    /// <returns></returns>
    private bool Contains(byte[] verifiable, byte[] mandatoryBytes)
    {
        //Получаем размер Vector'а типа Byte
        int vectorSize = Vector<byte>.Count;

        int indexText = 0;
        int indexByte = 0;
        for (; indexByte < mandatoryBytes.Length; indexByte++)
        {
            //Создаем Vector проверяющего i - итого байта
            //Весь вектор размером vectorSize состоит из повторяющегося байта
            Vector<byte> byteVector = new(mandatoryBytes[indexByte]);
            bool isNext = false;
            for (; indexText < verifiable.Length - vectorSize; indexText += vectorSize)
            {
                //Создаем Vector текста
                Vector<byte> textVector = new(verifiable, indexText);
                //Сравниваем вектора, если байт совпал, то значение 255, если нет, то 0 
                Vector<byte> result = Vector.Equals(textVector, byteVector);

                //Проверка на наличие совпадений
                if (Vector.GreaterThanAny(result, Vector<byte>.Zero))
                {
                    //Получаем индекс первого вхождения
                    int index = IndexOfValue(result, 255) + 1;
                    //Смешаем текст
                    indexText += index;
                    //Переходим к следующей итерации
                    isNext = true;
                    break;
                }
            }
            if (!isNext) { break; }
        }
        if (indexByte == mandatoryBytes.Length) { return true; }
        //Получаем остаток текста, который не входит в вектор
        int remaining = verifiable.Length % vectorSize;
        for (int i = verifiable.Length - remaining; i < verifiable.Length; i++)
        {
            if (verifiable[i] == mandatoryBytes[indexByte]) { indexByte++; }
            if (indexByte == mandatoryBytes.Length) { return true; }
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