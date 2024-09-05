using mi2se_classic_injector.Extensions;
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
    public class DiffCommand
    {
        private readonly MainSettings _settings;
        private readonly string[] _newPolLines;
        private readonly string[] _orgPolLines;
        private readonly Dictionary<string, string> _polishDictionary;

        public DiffCommand(IOptions<MainSettings> options1) 
        {
            _settings = options1.Value;

            var errors = string.Empty;
            if (!File.Exists(_settings.NewPolPath))
                errors += "Error: NewPolPath was not found in given path\n";
            if (!File.Exists(_settings.NewOrgPath))
                errors += "Error: NewOrgPath was not found in given path\n";
            if (!File.Exists("literal_codes.txt"))
                errors += "Error: literal_codes was not found in given path\n";
            if (!string.IsNullOrEmpty(errors))
            {
                Console.WriteLine(errors);
                Console.ReadKey();
                HasErrors = true;
                return;
            }

            _newPolLines = File.ReadAllLines(_settings.NewPolPath);
            _orgPolLines = File.ReadAllLines(_settings.NewOrgPath);
            _polishDictionary = File.ReadAllLines("literal_codes.txt").ToDictionary(x => x.Split(" - ")[2], y => y.Split(" - ")[4]);
        }
        public bool HasErrors { get; set; }

        public void Execute()
        {
            var poResult = new StringBuilder();

            for (int i = 0; i < _orgPolLines.Length; i++)
            {
                var line = _orgPolLines[i];
                var newLine = _newPolLines[i].ReplaceToPolishNew(_polishDictionary);
                poResult.AppendLine(line.ReplaceToPolishNew(_polishDictionary));
            }

            File.WriteAllText(_settings.OutputCatalog + "output.txt", poResult.ToString());
        }
    }
}
