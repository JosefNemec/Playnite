namespace System;

public static class DictionaryExtensions
{
    public static void AddOrUpdate<TKey, TVal>(this Dictionary<TKey, TVal> source, TKey key, TVal value) where TKey : notnull
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (source.ContainsKey(key))
        {
            source[key] = value;
        }
        else
        {
            source.Add(key, value);
        }
    }
}