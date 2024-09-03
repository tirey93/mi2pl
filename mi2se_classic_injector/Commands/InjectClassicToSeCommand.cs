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
        private readonly string[] _newPolLines;
        private readonly string[] _newOrgLinesNoChange;
        private readonly Dictionary<string, int> _classicOrgLines;
        private readonly List<KeyValuePair<string, int>> _classicMarkupOrgLines;
        private readonly Dictionary<string, string> _bookTranslations;
        private readonly Dictionary<string, string> _polishDictionary;
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
            if (!File.Exists(_settings.NewPolPath))
                errors += "Error: NewPolPath was not found in given path\n";
            if (!File.Exists("literal_codes.txt"))
                errors += "Error: literal_codes was not found in given path\n";
            if (!string.IsNullOrEmpty(errors))
            {
                Console.WriteLine(errors);
                Console.ReadKey();
                HasErrors = true;
                return;
            }

            var isUi = !_settings.NewOrgPath.Contains("speech");
            _newOrgLinesNoChange = File.ReadAllLines(_settings.NewOrgPath).ToArray();
            _newPolLines = File.ReadAllLines(_settings.NewPolPath);
            _newOrgLines = File.ReadAllLines(_settings.NewOrgPath)
                .Select(x => x.TrimNonAlphaNumSpaces(isUi)).ToArray();

            _classicPolLines = File.ReadAllLines(_settings.ClassicPolPath)
                .DivideMergedClassicLines()
                .ReplaceClassicLiterals(_literalSettings);

            var classicOrgLines = File.ReadAllLines(_settings.ClassicOrgPath)
                .DivideMergedClassicLines()
                .ReplaceClassicLiterals(_literalSettings);

            //File.WriteAllLines("engTokens", classicOrgLines.Select(x => x.TrimNonAlphaNumSpaces(false)));

            _classicOrgLines = classicOrgLines
                .Select((value, index) => new { value = value.TrimNonAlphaNumSpaces(isUi), index })
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
                    bookTranslations.Add(value.Replace(".", "").Replace("?", ""), _classicPolLines[classicLine]);
                }
            }
            _bookTranslations = bookTranslations;

            _polishDictionary = File.ReadAllLines("literal_codes.txt").ToDictionary(x => x.Split(" - ")[3], y => y.Split(" - ")[2]);
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

        private void ExecuteForSpeech()
        {
            StringBuilder errors = new StringBuilder();
            StringBuilder result = new StringBuilder();

            for (int newIndex = 0; newIndex < _newOrgLines.Length; newIndex++)
            {
                if (newIndex == 4362)
                {
                }
                var orgNewLine = _newOrgLines[newIndex];
                var newOrgLinesNoChange = _newOrgLinesNoChange[newIndex];

                if (_classicOrgLines.TryGetValue(orgNewLine, out var index))
                {
                    result.AppendLine(_classicPolLines[index].ReplaceToPolishNew(_polishDictionary));
                }
                else if (_classicMarkupOrgLines.TryGetIndexFromNumber(orgNewLine, out index, out var number))
                {
                    result.AppendLine(_regexClassicMarkup.Replace(_classicPolLines[index], number).ReplaceToPolishNew(_polishDictionary));
                }
                else if (_classicMarkupOrgLines.TryGetIndexFromVariables(orgNewLine, out index, out var variable))
                {
                    result.AppendLine(_regexClassicMarkup.Replace(_classicPolLines[index], variable).ReplaceToPolishNew(_polishDictionary));
                }
                else if (IsMatchingToBookQuestions(orgNewLine, out index, out var bookToken)
                    && bookToken.Length > 0
                    && !orgNewLine.Contains("idliketobuy")//hack for now
                    && newIndex != 7182)//hack for now
                {
                    result.AppendLine(_regexClassicMarkup.Replace(_classicPolLines[index], _bookTranslations[bookToken]).ReplaceToPolishNew(_polishDictionary));
                }
                else if (IsMatchingColors(orgNewLine, out index, out number))
                {
                    var token = string.Empty;
                    if(index == 4227 || index == 4226)
                        token = "\\255\\004\\013\\001 \\255 \\039\\000";
                    else
                        token = "\\255\\004\\012\\001 \\255 \\034\\000";
                    var regex = new Regex(@"([1-9][0-9]*)(.*)");
                    var match = regex.Match(number);
                    var strNumber = match.Groups[1].Value;
                    var strColor = match.Groups[2].Value;
                    if (strColor == "red")
                        strColor = "czerwone";
                    else
                        strColor = "czarne";

                    result.AppendLine(_classicPolLines[index].Replace(token, strNumber + " " + strColor).ReplaceToPolishNew(_polishDictionary));
                }
                else if (IsMatchAfterRemoveMarkup(orgNewLine, out index))
                {
                    result.AppendLine(_regexClassicMarkup.Replace(_classicPolLines[index], "").ReplaceToPolishNew(_polishDictionary));
                }
                else if (IsMatchingBuying(orgNewLine, out index, out var item))
                {
                    result.AppendLine(_regexClassicMarkup.Replace(_classicPolLines[index], _classicPolLines[_classicOrgLines[item]]).ReplaceToPolishNew(_polishDictionary));
                }
                else if (IsMatchingForbidden(orgNewLine, out index, out var place))
                {
                    result.AppendLine(_regexClassicMarkup.Replace(_classicPolLines[index], _classicPolLines[_classicOrgLines[place]]).ReplaceToPolishNew(_polishDictionary));
                }
                else
                {
                    var message = $"{newIndex + 1}\t{_newOrgLinesNoChange[newIndex]}";
                    errors.AppendLine(message);
                    result.AppendLine(_newPolLines[newIndex]);
                }
            }

            File.WriteAllText("../../../../errors.tsv", errors.ToString());
            File.WriteAllText("result_speech.txt", result.ToString());
        }

        private void ExecuteForUi()
        {
            StringBuilder errors = new StringBuilder();
            StringBuilder result = new StringBuilder();

            for (int newIndex = 0; newIndex < _newOrgLines.Length; newIndex++)
            {
                if (newIndex == 768)
                {
                }
                var isError = false;
                var orgNewLineNotSplitted = _newOrgLines[newIndex];
                if (((newIndex > 709 && newIndex < 950) || (newIndex > 1025 && newIndex < 1267)) 
                    && orgNewLineNotSplitted.Contains("`"))
                {
                    string orgNewSplitted = ExtractTitle(orgNewLineNotSplitted);

                    foreach (var orgNewLine in orgNewSplitted.Split("\\n"))
                    {
                        if (_classicOrgLines.TryGetValue(orgNewLine, out var index))
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
                    foreach (var orgNewLine in orgNewLineNotSplitted.Split("\\n"))
                    {
                        if (_classicOrgLines.TryGetValue(orgNewLine, out var index))
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
            File.WriteAllText("result_ui.txt", result.ToString());

        }

        private static string ExtractTitle(string orgNewLine)
        {
            var orgNewLineSplitted = orgNewLine.Split("`");

            var orgNewSplitted = string.Empty;
            var type = orgNewLineSplitted[0];
            var title = orgNewLineSplitted[1];
            var author = string.Empty;
            if(orgNewLineSplitted.Length > 2)
                author = orgNewLineSplitted[2];


            return type.Replace("\\n", "") + "\\n" + title.Replace("\\n", "") + "\\n" + author.Replace("\\n", "");
        }

        private bool IsMatchingBuying(string orgNewLine, out int index, out string result)
        {
            index = -1;
            result = string.Empty;
            if (!orgNewLine.Contains("idliketobuyth") && !orgNewLine.Contains("canisellbackth"))
                return false;
            foreach (var item in _literalSettings.BuyingLiterals)
            {
                var trimmedItem = item.TrimNonAlphaNumSpaces(false);
                var regexBuy = new Regex($"idliketobuyth.*{trimmedItem}");
                if (regexBuy.IsMatch(orgNewLine))
                {
                    index = 5465;
                    result = trimmedItem;
                    return true;

                }
                var regexSell = new Regex($"canisellbackth.*{trimmedItem}");
                if (regexSell.IsMatch(orgNewLine))
                {
                    index = 5534;
                    result = trimmedItem;
                    return true;
                }
            }

            return false;
        }

        private bool IsMatchingForbidden(string orgNewLine, out int index, out string place)
        {
            index = -1;
            place = string.Empty;
            if (!orgNewLine.Contains("wecantgotheremonthatstheforbidden"))
                return false;
            foreach (var item in _literalSettings.ForbiddenLiterals)
            {
                var trimmedPlace = item.TrimNonAlphaNumSpaces(false);
                var regexBuy = new Regex($@"wecantgotheremonthatstheforbidden{trimmedPlace}");
                if (regexBuy.IsMatch(orgNewLine))
                {
                    index = 7460;
                    place = trimmedPlace;
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

        private bool IsMatchingColors(string orgNewLine, out int index, out string number)
        {
            index = -1;
            number = null;

            var colorRegexes = new Dictionary<Regex, int>
            {
                { new Regex(@"([1-9][0-9]*red)itis"), 4227 },
                { new Regex(@"([1-9][0-9]*black)itis"), 4227 },
                { new Regex(@"thewinningnumberwillbe([1-9][0-9]*red)"), 4330 },
                { new Regex(@"thewinningnumberwillbe([1-9][0-9]*black)"), 4330 },
                { new Regex(@"ialreadytoldyouthatthenumberwouldbe([1-9][0-9]*red)"), 4332 },
                { new Regex(@"ialreadytoldyouthatthenumberwouldbe([1-9][0-9]*black)"), 4332 },
                { new Regex(@"([1-9][0-9]*red)"), 4226 },
                { new Regex(@"([1-9][0-9]*black)"), 4226 },
            };

            foreach (var colorRegex in colorRegexes)
            {
                var match = colorRegex.Key.Match(orgNewLine);
                if (match.Success)
                {
                    index = colorRegex.Value;
                    number = match.Groups[1].Value;
                    return true;
                }
            }

            return false;
        }

        private bool IsMatchingToBookQuestions(string orgNewLine, out int index, out string bookToken)
        {
            index = -1;
            bookToken = null;

            var questionRegexes = new Dictionary<Regex, int>
            {
                { new Regex(@"^doyouhave(.*)"), 2984 },
                { new Regex(@"^idlike(.*)"), 2985 },
                { new Regex(@"^couldyoufind(.*)"), 2986 },
                { new Regex(@"^ineed(.*)"), 2987 },
                { new Regex(@"^thecoversays(.*)"), 4693 },
                { new Regex(@"^(.*)hmmmillhavetorememberthat"), 3636 },
            };

            foreach (var questionRegex in questionRegexes)
            {
                var match = questionRegex.Key.Match(orgNewLine);
                if (match.Success)
                {
                    index = questionRegex.Value;
                    bookToken = match.Groups[1].Value.Replace(".", "").Replace("?", "");
                    return true;
                }
            }
            return false;
        }
    }
}
