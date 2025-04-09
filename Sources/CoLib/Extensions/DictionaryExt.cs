// Written by Colin on ${2024}-1-14

namespace CoLib.Extensions;

public static class DictionaryExt
{
    public static Tv GetOrCreate<Tk, Tv>(this Dictionary<Tk, Tv> dict, Tk key) 
        where Tk : notnull where Tv : new()
    {
        if (dict.TryGetValue(key, out var value)) 
            return value;
        
        value = new Tv();
        dict[key] = value;
        return value;
    }
}