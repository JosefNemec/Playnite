namespace Playnite;

public class GlobalRandom
{
    private static readonly Random generator = new Random();
    private static readonly string randomStringChars = "ABCDEFGHIJKLMNOPQRSTYVWXZabcdefghijklmnopqrstyvwxz0123456789";

    public static int Next()
    {
        return generator.Next();
    }

    public static int Next(int minValue, int maxValue)
    {
        return generator.Next(minValue, maxValue);
    }

    public static int Next(int maxValue)
    {
        return generator.Next(maxValue);
    }

    public static void NextBytes(byte[] buffer)
    {
        generator.NextBytes(buffer);
    }

    public static double NextDouble()
    {
        return generator.NextDouble();
    }

    public static DateTime GetRandomDateTime()
    {
        var startDate = new DateTime(1970, 1, 1);
        int range = (DateTime.Today - startDate).Days;
        return startDate.AddDays(Next(range));
    }

    public static string GetRandomString(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentException("0 is not a valid length");
        }

        var randomSetLeng = randomStringChars.Length - 1;
        var result = new StringBuilder(length);
        lock (generator)
        {
            for (int i = 0; i < length; i++)
            {
                result.Append(randomStringChars[generator.Next(0, randomSetLeng)]);
            }

            return result.ToString();
        }
    }
}
