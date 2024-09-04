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
        private static readonly Regex _regexClassicMarkup = new Regex(@"(\\255\\[0-9]{3}\\[0-9]{3}\\[0-9]{3})");
        public static string TrimNonAlphaNumSpaces(this string text, bool ui)
        {
            var regText = ui ? @"[^a-zA-Z0-9\\\{\}\:\`]" : @"[^a-zA-Z0-9\\\{\}\:\!\?]";
            Regex rgx = new Regex(regText);
            return rgx.Replace(text, "").ToLower();
        }

        public static string ReplaceToPolishNew(this string text, Dictionary<string, string> polishDict)
        {
            foreach (var item in polishDict)
            {
                text = text.Replace(item.Key, item.Value);
            }
            //return text;
            return Regex
                .Replace(text, @"\s+", " ")
                .Replace("@", "");
        }
    }
}
