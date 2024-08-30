using mi2se_classic_injector.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mi2se_classic_injector.Extensions
{
    public static class ClassicExtensions
    {
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
    }
}
