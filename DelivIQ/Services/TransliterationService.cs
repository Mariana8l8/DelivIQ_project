using System.Text;

namespace DelivIQ.Services
{
    public class TransliterationService : ITransliterationService
    {
        private static readonly Dictionary<string, string> Map = new()
        {
            ["а"] = "a", ["б"] = "b", ["в"] = "v", ["г"] = "h", ["ґ"] = "g",
            ["д"] = "d", ["е"] = "e", ["є"] = "ie", ["ж"] = "zh", ["з"] = "z",
            ["и"] = "y", ["і"] = "i", ["ї"] = "i", ["й"] = "i", ["к"] = "k",
            ["л"] = "l", ["м"] = "m", ["н"] = "n", ["о"] = "o", ["п"] = "p",
            ["р"] = "r", ["с"] = "s", ["т"] = "t", ["у"] = "u", ["ф"] = "f",
            ["х"] = "kh", ["ц"] = "ts", ["ч"] = "ch", ["ш"] = "sh", ["щ"] = "shch",
            ["ю"] = "iu", ["я"] = "ia", ["ь"] = "", ["'"] = "", ["’"] = ""
        };

        public string ToLatin(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var stringb = new StringBuilder(text.Length);
            foreach (var ch in text)
            {
                var s = ch.ToString();
                var lower = s.ToLowerInvariant();
                if (Map.TryGetValue(lower, out var repl))
                {
                    if (char.IsUpper(ch))
                    {
                        if (repl.Length == 0) { }
                        else if (repl.Length == 1)
                        {
                            stringb.Append(repl.ToUpperInvariant());
                        }
                        else
                        {
                            stringb.Append(char.ToUpperInvariant(repl[0]));
                            stringb.Append(repl.Substring(1));
                        }
                    }
                    else
                    {
                        stringb.Append(repl);
                    }
                }
                else
                {
                    stringb.Append(ch);
                }
            }

            return stringb.ToString();
        }
    }
}
