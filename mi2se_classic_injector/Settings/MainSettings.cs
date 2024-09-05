
namespace mi2se_classic_injector.Settings
{
    public class MainSettings
    {
        public string ClassicOrgPath { get; set; }
        public string NewOrgPath { get; set; }
        public string ClassicPolPath { get; set; }
        public string NewPolPath { get; set; }
        public string OutputCatalog { get; set; }
        public string ErrorPath { get; set; }
        public Mode Mode { get; set; }
    }

    public enum Mode
    {
        Default,
        ErrorToPo,
        ErrorFromPo,
        Diff
    }
}
