using System.Numerics;
using System.Text;

namespace AVX2.SIMD;

//https://learn.microsoft.com/ru-ru/dotnet/standard/simd
public class VectorsHandler
{
    public void Start(List<string> queries, List<string> laws, List<string> decisions)
    {
        //Проверка, наличия поддержки AVX2
        if (!Vector.IsHardwareAccelerated)
        {
            Console.WriteLine("Не поддерживается системой");
            return;
        }

        DateTime time = DateTime.Now;
        int count = 0;
        queries.ForEach(query =>
        {
            //Преобразования текста в байты
            byte[] checkBytes = Encoding.UTF8.GetBytes(query);

            List<int> lawsIndexes = new();
            for (int i = 0; i < laws.Count; i++)
            {
                //Преобразования текста в байты
                byte[] textBytes = Encoding.UTF8.GetBytes(laws[i]);
                //Запуск сравнения векторов
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
    /// <param name="textBytes">Байтовый набор текста</param>
    /// <param name="checkBytes">Байтовый набор обязательных символов</param>
    /// <returns></returns>
    private bool Contains(byte[] textBytes, byte[] checkBytes)
    {
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