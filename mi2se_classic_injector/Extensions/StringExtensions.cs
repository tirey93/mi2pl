using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mi2se_classic_injector.Extensions
{
    public static class StringExtensions
    {
        public static string TrimWhiteSpaces(this string text)
        {
            return text.Replace(" ", "").ToLower();
        }
    }
}
