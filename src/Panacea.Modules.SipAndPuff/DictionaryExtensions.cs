using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.SipAndPuff
{
    static class DictionaryExtensions
    {
        internal static string Print<T>(this Tree<T> dict, Func<Tree<T>, string> toString, int depth = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Concat(Enumerable.Repeat("| ", depth)) + "|- " + toString(dict));
            
            foreach (var item in dict.Children)
            {
                sb.Append(item.Print(toString, depth + 1));
            }
            return sb.ToString();
        }
    }
}
