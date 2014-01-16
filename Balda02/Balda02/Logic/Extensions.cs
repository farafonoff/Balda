using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Windows;
#if SILVERLIGHT
using System.Windows.Data;
#else
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
#endif

namespace Balda.Logic
{
    public static class Extensions
    {

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static string Flatten<T>(this IList<T> list)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; ++i)
            {
                sb.Append(list[i]);
            }
            return sb.ToString();
        }

        public static IList<int> ToInts(this IList<int> list,string src)
        {
            list.Clear();
            for (int i = 0; i < src.Length; ++i)
            {
                list.Add(int.Parse(new string(src[i],1)));
            }
            return list;
        }
        public static IList<char> ToChars(this IList<char> list, string src)
        {
            list.Clear();
            for (int i = 0; i < src.Length; ++i)
            {
                list.Add(src[i]);
            }
            return list;
        }


    }
}
