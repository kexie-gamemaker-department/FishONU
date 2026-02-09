using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace FishONU.Utils
{
    public static class Algorithm
    {
        public static void FisherYatesShuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var j = Random.Range(0, i + 1);
                (list[j], list[i]) = (list[i], list[j]);
            }
        }

        public static bool TryPop<T>(this IList<T> list, out T item)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            if (list.Count == 0)
            {
                item = default(T);
                return false;
            }

            var index = list.Count - 1;
            item = list[index];
            list.RemoveAt(index);

            return true;
        }

        public static void InsertRandom<T>(this IList<T> list, T item)
        {
            int index = Random.Range(0, list.Count + 1);
            list.Insert(index, item);
        }
    }
}