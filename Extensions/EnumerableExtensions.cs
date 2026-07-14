using System.Collections.Generic;
using System;

namespace HacknetArchipelago.Extensions;

public static class EnumerableExtensions
{
    // Custom implementation of DistinctBy that's in .NET 6+
    // *shakes fist at needing to use .NET Standard 2.0 to appease Hacknet*
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
        this IEnumerable<TSource> source, 
        Func<TSource, TKey> keySelector)
    {
        return source.DistinctBy(keySelector, null);
    }
    
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
        this IEnumerable<TSource> source, 
        Func<TSource, TKey> keySelector, 
        IEqualityComparer<TKey> comparer)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return keySelector == null ? throw new ArgumentNullException(nameof(keySelector)) :
            DistinctByImpl(source, keySelector, comparer);
    }

    private static IEnumerable<TSource> DistinctByImpl<TSource, TKey>(
        IEnumerable<TSource> source, 
        Func<TSource, TKey> keySelector, 
        IEqualityComparer<TKey> comparer)
    {
        var knownKeys = new HashSet<TKey>(comparer);
            
        foreach (TSource element in source)
        {
            if (knownKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }
}