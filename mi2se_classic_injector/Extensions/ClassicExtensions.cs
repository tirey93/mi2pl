﻿using mi2se_classic_injector.Settings;
using System.Text.RegularExpressions;

namespace mi2se_classic_injector.Extensions
{
    public static class ClassicExtensions
    {
        private static readonly Regex _regexClassicMarkup = new Regex(@"(\\255\\[0-9]{3}\\[0-9]{3}\\[0-9]{3})");

        public static string[] DivideMergedClassicLines(this string[] lines)
        {
            var result = new List<string>();
            foreach (var line in lines)
            {
                if (line.Contains(@"\255\003"))
                {
                    var splitted = line.Split(@"\255\003");
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
                if (i == 424)
                {

                }
                var lineAfterReplace = line;
                var markup = string.Empty;
                var match = _regexClassicMarkup.Match(line);
                if (match.Success)
                {
                    markup = match.Groups[1].Value;
                    lineAfterReplace = line.Replace(markup, "{markup}");
                }
                foreach (var literal in splittedLiterals)
                {
                    if (line.Contains(literal.Key))
                    {
                        lineAfterReplace = lineAfterReplace.Replace(literal.Key, literal.Value);
                    }
                }
                if (match.Success)
                {
                    lineAfterReplace = lineAfterReplace.Replace("{markup}", markup);
                }
                result.Add(lineAfterReplace);
                i++;
            }

            return result.ToArray();
        }

        public static bool TryGetIndexFromNumber(this IEnumerable<KeyValuePair<string, int>> markupList, string line, out int index, out string number)
        {
            index = -1;
            number = string.Empty;
            var regexNumber = new Regex("([1-9][0-9]*)");
            var numberMatch = regexNumber.Match(line);
            if (!numberMatch.Success)
                return false;

            line = line.TrimNonAlphaNumSpaces(false);
            foreach (var classicLine in markupList)
            {
                if (_regexClassicMarkup.Replace(classicLine.Key, numberMatch.Value) == line)
                {
                    index = classicLine.Value;
                    number = numberMatch.Value;
                    return true;
                }
            }
            return false;
        }

        public static bool TryGetIndexFromVariables(this IEnumerable<KeyValuePair<string, int>> markupList, string line, out int index, out string variable)
        {
            index = -1;
            variable = string.Empty;
            var regexVariable = new Regex(@"(\{.*\:.*\})");
            var variableMatch = regexVariable.Match(line);
            if (!variableMatch.Success)
                return false;

            line = line.TrimNonAlphaNumSpaces(false);
            foreach (var classicLine in markupList)
            {
                if (_regexClassicMarkup.Replace(classicLine.Key, variableMatch.Value) == line)
                {
                    index = classicLine.Value;
                    variable = variableMatch.Value;
                    return true;
                }
            }
            return false;
        }
    }
}
