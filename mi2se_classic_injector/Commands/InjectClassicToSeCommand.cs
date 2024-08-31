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
        private readonly Dictionary<string, string> _bookTranslations;
        private readonly string[] _classicPolLines;
        private readonly Regex _regexClassicMarkup = new Regex("(\\\\255\\\\[0-9]{3}\\\\[0-9]{3}\\\\[0-9]{3})");

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
                .Select(x => x.TrimWhiteSpaces()).ToArray();

            _classicPolLines = File.ReadAllLines(_settings.ClassicPolPath)
                .DivideMergedClassicLines()
                .ReplaceClassicLiterals(_literalSettings);

            _classicOrgLines = File.ReadAllLines(_settings.ClassicOrgPath)
                .DivideMergedClassicLines()
                .ReplaceClassicLiterals(_literalSettings)
                .Select((value, index) => new { value = value.TrimWhiteSpaces(), index })
                .GroupBy(pair => pair.value)
                .ToDictionary(pair => pair.Key, pair => pair.FirstOrDefault().index);

            var bookTranslations = new Dictionary<string, string>();
            var regexBooks = new Regex("thecoversays`(.*).`");
            //foreach (var orgLine in _newOrgLines)
            //{
            //    var regexMatch = regexBooks.Match(orgLine);
            //    if (regexMatch.Success)
            //    {
            //        var classicLine = _classicOrgLines[regexMatch.Groups[1].Value];
            //        bookTranslations.Add(regexMatch.Value, _classicPolLines[classicLine]);
            //    }
            //}
            _bookTranslations = bookTranslations;
        }

        public void Execute()
        {
            StringBuilder errors = new StringBuilder();
            var classicMarkupOrgLines = _classicOrgLines
                .Where(x => _regexClassicMarkup.IsMatch(x.Key)).ToList();

            //classicMarkupOrgLines.TryGetIndexFromMarkup("I'm not holding the {name:local_1}.", out var r);

            for (int newIndex = 0; newIndex < _newOrgLines.Length; newIndex++)
            {
                if(newIndex == 4051)
                {
                }
                var orgNewLine = _newOrgLines[newIndex];
                var noChangeLine = _newOrgLinesNoChange[newIndex];
                
                if(_classicOrgLines.TryGetValue(orgNewLine, out var index) 
                    || classicMarkupOrgLines.TryGetIndexFromMarkup(orgNewLine, out index))
                {
                    var res = _classicPolLines[index];
                }
                else
                {
                    var message = $"{newIndex + 1}\t{_newOrgLinesNoChange[newIndex]}";
                    errors.AppendLine(message);
                }
            }

            File.WriteAllText("errors.tsv", errors.ToString());
        }
    }
}
