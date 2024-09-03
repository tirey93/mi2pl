using mi2se_classic_injector.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace mi2se_classic_injector.Commands
{
    public class ErrorsToPoCommand
    {
        private readonly MainSettings _settings;
        private readonly string[] _errorLines;
        private readonly string[] _newPolLines;
        private readonly Dictionary<string, string> _polishDictionary;

        public ErrorsToPoCommand(IOptions<MainSettings> options1) 
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

            _errorLines = File.ReadAllLines(_settings.ErrorPath);
            _newPolLines = File.ReadAllLines(_settings.NewPolPath);
            _polishDictionary = File.ReadAllLines("literal_codes.txt").ToDictionary(x => x.Split(" - ")[2], y => y.Split(" - ")[1]);
        }
        public bool HasErrors { get; set; }

        public void Execute()
        {
            var poResult = new StringBuilder();

            for (int i = 0; i < _errorLines.Length; i++)
            {
                var line = _errorLines[i];
                var number = line.Split("\t")[0];
                var clearLine = line.Split("\t")[1];

                poResult.Append(ToPo(number, clearLine, _newPolLines[int.Parse(number) - 1]));
            }

            File.WriteAllText(_settings.OutputCatalog + "output.po", poResult.ToString());
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
