using mi2se_classic_injector.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mi2se_classic_injector.Extensions;
using System.Text.RegularExpressions;

namespace mi2se_classic_injector.Commands
{
    public class InjectClassicToSeCommand
    {
        private readonly MainSettings _settings;
        private readonly LiteralSettings _literalSettings;
        private readonly string[] _newOrgLines;
        private readonly string[] _newOrgLinesNoChange;
        private readonly Dictionary<string, int> _classicOrgLines;
        private readonly List<KeyValuePair<string, int>> _classicMarkupOrgLines;
        private readonly Dictionary<string, string> _bookTranslations;
        private readonly string[] _classicPolLines;
        private readonly Regex _regexClassicMarkup = new Regex(@"(\\255\\[0-9]{3}\\[0-9]{3}\\[0-9]{3})");

        public bool HasErrors { get; set; }

        public InjectClassicToSeCommand(IOptions<MainSettings> options, IOptions<LiteralSettings> options1)
        {
            _settings = options.Value;
            _literalSettings = options1.Value;

            var errors = string.Empty;
            if (!File.Exists(_settings.NewOrgPath))
                errors += "Error: NewOrgPath was not found in given path\n";
            if (!File.Exists(_settings.ClassicOrgPath))
                errors += "Error: ClassicOrgPath was not found in given path\n";
            if (!File.Exists(_settings.ClassicPolPath))
                errors += "Error: ClassicPolPath was not found in given path\n";
            if (!string.IsNullOrEmpty(errors))
            {
                Console.WriteLine(errors);
                Console.ReadKey();
                HasErrors = true;
                return;
            }

            _newOrgLinesNoChange = File.ReadAllLines(_settings.NewOrgPath).ToArray();
            _newOrgLines = File.ReadAllLines(_settings.NewOrgPath)
                .Select(x => x.TrimNonAlphaNumSpaces()).ToArray();

            _classicPolLines = File.ReadAllLines(_settings.ClassicPolPath)
                .DivideMergedClassicLines()
                .ReplaceClassicLiterals(_literalSettings);

            var classicOrgLines = File.ReadAllLines(_settings.ClassicOrgPath)
                .DivideMergedClassicLines()
                .ReplaceClassicLiterals(_literalSettings);
            //File.WriteAllLines("engTokens", classicOrgLines.Select(x => x.TrimNonAlphaNumSpaces()));

            _classicOrgLines = classicOrgLines
                .Select((value, index) => new { value = value.TrimNonAlphaNumSpaces(), index })
                .GroupBy(pair => pair.value)
                .ToDictionary(pair => pair.Key, pair => pair.FirstOrDefault().index);
            _classicMarkupOrgLines = _classicOrgLines
                .Where(x => _regexClassicMarkup.IsMatch(x.Key)).ToList();


            var bookTranslations = new Dictionary<string, string>();
            var regexBooks = new Regex("thecoversays(.*)");
            foreach (var orgLine in _newOrgLines)
            {
                var regexMatch = regexBooks.Match(orgLine);
                var value = regexMatch.Groups[1].Value;
                if (regexMatch.Success && value.Length > 0)
                {
                    var classicLine = _classicOrgLines[value];
                    bookTranslations.Add(value, _classicPolLines[classicLine]);
                }
            }
            _bookTranslations = bookTranslations;
        }

        public void Execute()
        {
            //IsMatchingToBookQuestions("doyouhaveanimatronics", out var r, out var x);
            if (_settings.NewOrgPath.Contains("speech"))
            {
                ExecuteForSpeech();
            }
            else if (_settings.NewOrgPath.Contains("ui"))
            {
                ExecuteForUi();
            }
            else
            {
                Console.WriteLine("unknown type of org file");
            }

        }

        private void ExecuteForUi()
        {
            StringBuilder errors = new StringBuilder();

            for (int newIndex = 0; newIndex < _newOrgLines.Length; newIndex++)
            {
                if (newIndex == 1045)
                {
                }
                var isError = false;
                var orgNewLineNotSplitted = _newOrgLines[newIndex];
                var orgNewLineSplitted = orgNewLineNotSplitted.Split("\\n");
                if (orgNewLineSplitted.Length > 3)
                {
                    var orgNewSplitted = string.Empty;
                    var title = string.Join("\\n", [orgNewLineSplitted[1], orgNewLineSplitted[2]]).Replace("\\n", "");
                    orgNewSplitted = orgNewLineSplitted[0] + "\\n" + title;
                    for (int i = 3; i < orgNewLineSplitted.Length; i++)
                    {
                        orgNewSplitted += "\\n" + orgNewLineSplitted[i];
                    }

                    foreach (var orgNewLine in orgNewSplitted.Split("\\n"))
                    {
                        if (_classicOrgLines.TryGetValue(orgNewLine, out var index)
                                            || _classicMarkupOrgLines.TryGetIndexFromMarkup(orgNewLine, out index))
                        {
                            var res = _classicPolLines[index];
                        }
                        else
                        {
                            isError = true;
                        }
                    }
                }
                else
                {
                    foreach (var orgNewLine in orgNewLineSplitted)
                    {
                        if (_classicOrgLines.TryGetValue(orgNewLine, out var index)
                                            || _classicMarkupOrgLines.TryGetIndexFromMarkup(orgNewLine, out index))
                        {
                            var res = _classicPolLines[index];
                        }
                        else
                        {
                            isError = true;
                        }
                    }
                }
                
                if (isError)
                {
                    var message = $"{newIndex + 1}\t{_newOrgLinesNoChange[newIndex]}";
                    errors.AppendLine(message);
                }
            }
            File.WriteAllText("../../../../errors_ui.tsv", errors.ToString());

        }

        private void ExecuteForSpeech()
        {
            StringBuilder errors = new StringBuilder();

            for (int newIndex = 0; newIndex < _newOrgLines.Length; newIndex++)
            {
                if (newIndex == 711)
                {
                }
                var orgNewLine = _newOrgLines[newIndex];

                if (_classicOrgLines.TryGetValue(orgNewLine, out var index)
                                        || _classicMarkupOrgLines.TryGetIndexFromMarkup(orgNewLine, out index))
                {
                    var res = _classicPolLines[index];
                }
                else if (IsMatchingToBookQuestions(orgNewLine, out var regex, out var bookToken)
                    && !orgNewLine.Contains("idliketobuy")//hack for now
                    && newIndex != 7182)//hack for now
                {

                }
                else if (IsMatchingColors(orgNewLine, out regex, out var number))
                {

                }
                else if (IsMatchAfterRemoveMarkup(orgNewLine, out index))
                {

                }
                else if (IsMatchingBuying(orgNewLine, out index))
                {

                }
                else if (IsMatchingForbidden(orgNewLine, out index))
                {

                }
                else
                {
                    var message = $"{newIndex + 1}\t{_newOrgLinesNoChange[newIndex]}";
                    errors.AppendLine(message);
                }
            }

            File.WriteAllText("../../../../errors.tsv", errors.ToString());
        }

        private bool IsMatchingForbidden(string orgNewLine, out int index)
        {
            index = -1;
            if (!orgNewLine.Contains("wecantgotheremonthatstheforbidden"))
                return false;
            foreach (var item in _literalSettings.ForbiddenLiterals)
            {
                var regexBuy = new Regex($@"wecantgotheremonthatstheforbidden{item.TrimNonAlphaNumSpaces()}");
                if (regexBuy.IsMatch(orgNewLine))
                {
                    index = 7461;
                    return true;

                }
            }

            return false;
        }

        private bool IsMatchingBuying(string orgNewLine, out int index)
        {
            index = -1;
            if(!orgNewLine.Contains("idliketobuyth") && !orgNewLine.Contains("canisellbackth"))
                return false;
            foreach (var item in _literalSettings.BuyingLiterals)
            {
                var regexBuy = new Regex($"idliketobuyth.*{item.TrimNonAlphaNumSpaces()}");
                if (regexBuy.IsMatch(orgNewLine))
                {
                    index = 5465;
                    return true;

                }
                var regexSell = new Regex($"canisellbackth.*{item.TrimNonAlphaNumSpaces()}");
                if (regexSell.IsMatch(orgNewLine))
                {
                    index = 5534;
                    return true;
                }
            }

            return false;
        }

        private bool IsMatchAfterRemoveMarkup(string orgNewLine, out int index)
        {
            index = -1;
            foreach (var markupLine in _classicMarkupOrgLines)
            {
                var lineNoMarkup = _regexClassicMarkup.Replace(markupLine.Key, "");
                if (lineNoMarkup == orgNewLine)
                {
                    index = markupLine.Value;
                    return true;
                }
            }
            return false;
        }

        private bool IsMatchingColors(string orgNewLine, out Regex regex, out string number)
        {
            regex = null;
            number = null;

            var colorRegexes = new List<Regex>
            {
                new Regex(@"[1-9][0-9]*red"),
                new Regex(@"[1-9][0-9]*black"),
            };

            foreach (var colorRegex in colorRegexes)
            {
                var match = colorRegex.Match(orgNewLine);
                if (match.Success)
                {
                    regex = colorRegex;
                    number = match.Groups[1].Value;
                    return true;
                }
            }

            return false;
        }

        private bool IsMatchingToBookQuestions(string orgNewLine, out Regex regex, out string bookToken)
        {
            regex = null;
            bookToken = null;

            var questionRegexes = new List<Regex>
            {
                new Regex(@"^doyouhave(.*)"),
                new Regex(@"^idlike(.*)"),
                new Regex(@"^couldyoufind(.*)"),
                new Regex(@"^ineed(.*)"),
                new Regex(@"^thecoversays(.*)"),
                new Regex(@"^(.*)hmmmillhavetorememberthat"),
            };

            foreach (var questionRegex in questionRegexes)
            {
                var match = questionRegex.Match(orgNewLine);
                if (match.Success)
                {
                    regex = questionRegex;
                    bookToken = match.Groups[1].Value;
                    return true;
                }
            }
            return false;
        }
    }
}
