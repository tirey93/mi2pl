﻿using mi2se_classic_injector.Settings;
using System.Text.RegularExpressions;

namespace mi2se_classic_injector.Extensions
{
    public static class ClassicExtensions
    {
        private static readonly Regex _regexClassicMarkup = new Regex(".*\\\\255\\\\[0-9]{3}\\\\[0-9]{3}\\\\[0-9]{3}.*");
        private static readonly Regex _regexNumber = new Regex(".*[1-9][0-9]*.*");

        public static string[] DivideMergedClassicLines(this string[] lines)
        {
            var result = new List<string>();
            foreach (var line in lines)
            {
                if (line.Contains("\\255\\003"))
                {
                    var splitted = line.Split("\\255\\003");
                    result.AddRange(splitted);
                }
                else
                {
                    result.Add(line);
                }
            }

            return result.ToArray();
        }

        public static string[] ReplaceClassicLiterals(this string[] lines, LiteralSettings literalSettings)
        {
            var result = new List<string>();
            var splittedLiterals = literalSettings.ClassicLiterals.Select(x => x.Split(",")).ToDictionary(x => x[0], y => y[1]);
            int i = 1;

            foreach (var line in lines)
            {
                if (i == 124)
                {

                }
                var lineAfterReplace = line;
                foreach (var literal in splittedLiterals)
                {
                    if (line.Contains(literal.Key))
                    {
                        lineAfterReplace = lineAfterReplace.Replace(literal.Key, literal.Value);
                    }
                }
                result.Add(lineAfterReplace);
                i++;
            }

            return result.ToArray();
        }

        public static bool TryGetIndexFromMarkup(this IEnumerable<KeyValuePair<string, int>> markupList, string line, out int index)
        {
            var numberMatch = _regexNumber.Match(line);
            if (numberMatch.Success)
            {

                
            }

            index = -1;
            return false;
        }

        private static bool TryGetIndexFromNumber(IEnumerable<KeyValuePair<string, int>> markupList, string line, out int index)
        {
            foreach (var classicLine in markupList)
            {
                if(_regexClassicMarkup.Replace(classicLine.Key, "") == line)
                {

                }
            }

            index = -1;
            return false;
        }
    }
}
