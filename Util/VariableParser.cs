using SuchByte.MacroDeck.Variables;
using System.Linq;
using System.Text.RegularExpressions;

namespace ziopuzzle.WebRequest.Util
{
    public static partial class VariableParser
    {
        public static string Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input ?? string.Empty;
            }

            return VariablePatternRegex().Replace(input, match =>
            {
                string varName = match.Groups[1].Value.Trim();
                var macroDeckVar = VariableManager.Variables.FirstOrDefault(v => v.Name == varName);

                return macroDeckVar != null ? macroDeckVar.Value.ToString() : match.Value;
            });
        }

        [GeneratedRegex(@"\{\{(.+?)\}\}")]
        private static partial Regex VariablePatternRegex();
    }
}