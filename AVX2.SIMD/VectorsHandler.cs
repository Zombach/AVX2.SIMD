using System.Numerics;
using System.Text;

namespace AVX2.SIMD;

//https://learn.microsoft.com/ru-ru/dotnet/standard/simd
public class VectorsHandler
{
    public void Start(IEnumerable<IEnumerable<byte>> queries, IEnumerable<IEnumerable<byte>> laws, IEnumerable<IEnumerable<byte>> decisions)
    {
        queries = queries.ToArray();
        laws = laws.ToArray();
        decisions = decisions.ToArray();

        //Проверка, наличия поддержки AVX2
        if (!Vector.IsHardwareAccelerated)
        {
            Console.WriteLine("Не поддерживается системой");
            return;
        }

        int count = 0;
        DateTime start = DateTime.Now;
        using IEnumerator<IEnumerable<byte>> enumerator = queries.GetEnumerator();
        while (enumerator.MoveNext())
        {
            IEnumerable<IEnumerable<byte>> lawsResult = GetResults(laws, enumerator.Current).ToList();
            IEnumerable<IEnumerable<byte>> decisionsResult = GetResults(decisions, enumerator.Current).ToList();

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

    private IEnumerable<IEnumerable<byte>> GetResults(IEnumerable<IEnumerable<byte>> source, IEnumerable<byte> checkBytes)
    {
        source = source.ToArray();
        checkBytes = checkBytes.ToArray();

        IEnumerable<IEnumerable<byte>> values = new List<IEnumerable<byte>>();
        using IEnumerator<IEnumerable<byte>> enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            bool isOk = Contains(enumerator.Current, checkBytes);
            if (isOk) { values = values.Append(enumerator.Current); }
        }

        return values;
    }

    private string ByteToText(IEnumerable<byte> bytes) => Encoding.UTF8.GetString(bytes.ToArray());

    private void ViewResult(IEnumerable<IEnumerable<byte>> source, string message)
    {
        source = source.ToList();
        if (!source.Any()) { return; }

        int count = source.Count();
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
    private bool Contains(IEnumerable<byte> verifiable, IEnumerable<byte> mandatoryBytes)
    {
        byte[] textBytes = verifiable.ToArray();
        byte[] checkBytes = mandatoryBytes.ToArray();

        //Получаем размер Vector'а типа Byte
        int vectorSize = Vector<byte>.Count;

        int indexText = 0;
        int indexByte = 0;
        for (; indexByte < checkBytes.Length; indexByte++)
        {
            //Создаем Vector проверяющего i - итого байта
            //Весь вектор размером vectorSize состоит из повторяющегося байта
            Vector<byte> byteVector = new(checkBytes[indexByte]);
            bool isNext = false;
            for (; indexText < textBytes.Length - vectorSize; indexText += vectorSize)
            {
                //Создаем Vector текста
                Vector<byte> textVector = new(textBytes, indexText);
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
        if (indexByte == checkBytes.Length) { return true; }
        //Получаем остаток текста, который не входит в вектор
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