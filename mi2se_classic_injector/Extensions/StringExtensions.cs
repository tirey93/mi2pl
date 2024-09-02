using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mi2se_classic_injector.Extensions
{
    public static class StringExtensions
    {
        public static string TrimNonAlphaNumSpaces(this string text, bool ui)
        {
            var regText = ui ? @"[^a-zA-Z0-9\\\{\}\:\`]" : @"[^a-zA-Z0-9\\\{\}\:]";
            Regex rgx = new Regex(regText);
            return rgx.Replace(text, "").ToLower();
        }
    }
}
