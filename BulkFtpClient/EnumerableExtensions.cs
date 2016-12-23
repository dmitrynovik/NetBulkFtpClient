using System;
using System.Collections.Generic;
using System.Linq;

namespace BulkFtpClient.Ftp
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || !items.Any();
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
        {
            return new HashSet<T>(items);
        }

        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items != null && action != null)
                foreach (var i in items)
                    action(i);
        }

        public static void AddMany<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null || items == null)
                return;

            items.Each(collection.Add);
        }

        public static ICollection<T> RemoveMany<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            if (collection == null || predicate == null)
                return collection;

            return collection.Where(x => !predicate(x)).ToList();
        }

        public static TVal GetOrDefault<TKey, TVal>(this IDictionary<TKey, TVal> map, TKey key)
        {
            return map.ContainsKey(key) ? map[key] : default(TVal);
        }
    }
}
