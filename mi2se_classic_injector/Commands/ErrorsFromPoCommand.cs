using mi2se_classic_injector.Extensions;
using mi2se_classic_injector.Settings;
using mi2se_classic_injector.Utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace mi2se_classic_injector.Commands
{
    public class ErrorsFromPoCommand
    {
        private readonly MainSettings _settings;
        private readonly string[] _errorLines;
        private readonly string[] _oldPolLines;
        private readonly Dictionary<string, string> _polishDictionary;
        private readonly Dictionary<int, string> _newPolLines;

        public ErrorsFromPoCommand(IOptions<MainSettings> options1) 
        {
            _settings = options1.Value;

            var errors = string.Empty;
            if (!File.Exists(_settings.ErrorPath))
                errors += "Error: ErrorPath was not found in given path\n";
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

            var errorLines = File.ReadAllText(_settings.ErrorPath);
            _polishDictionary = File.ReadAllLines("literal_codes.txt").ToDictionary(x => x.Split(" - ")[1], y => y.Split(" - ")[2]);
            var splitted = errorLines.Split("msgctxt");

            var polLines = new Dictionary<int, string>();

            foreach (var text in splitted)
            {
                if (string.IsNullOrEmpty(text)) continue;
                var textWithCuttedStart = "msgctxt" + text;

                var splitter = new PoSplitter(textWithCuttedStart);
                if (!splitter.IsValid)
                    continue;

                var markup = int.Parse(splitter.Markup);
                var plText = splitter.PlText;

                polLines.Add(markup, plText.ReplaceToPolishNew(_polishDictionary));
            }

            _newPolLines = polLines;
            _oldPolLines = File.ReadAllLines(_settings.NewPolPath);
        }
        public bool HasErrors { get; set; }

        public void Execute()
        {
            var poResult = new StringBuilder();

            for (int i = 0; i < _oldPolLines.Length; i++)
            {
                if(i == 768)
                {
                }
                var line = _oldPolLines[i];
                if(_newPolLines.ContainsKey(i + 1))
                {
                    var res = _newPolLines[i + 1];
                    poResult.AppendLine(_newPolLines[i + 1]);
                }
                else
                {
                    poResult.AppendLine(line);
                }
            }

            File.WriteAllText(_settings.OutputCatalog + "output.txt", poResult.ToString());
        }

        private static string ToPo(string markup, string engStr, string plStr)
        {
            var result = $"msgctxt \"{markup}\"\n";
            result += $"msgid \"{engStr}\"\n";
            result += $"msgstr \"{plStr}\"\n\n";

            return result;
        }
    }
}
