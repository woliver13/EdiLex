namespace EdiLex
{
    public class Element
    {
        public string Value { get; }
        public Dictionary<int, string> SubElements { get; }
        public Dictionary<int, string> RepeatingElements { get; }

        public Element(string value, char subElementSeparator, char repeatingElementSeparator)
        {
            Value = value ?? "";
            ArgumentNullException.ThrowIfNull(value);
            SubElements = ParseSubElements(value, subElementSeparator);
            RepeatingElements = ParseRepeatingElements(value, repeatingElementSeparator);
        }

        private Dictionary<int, string> ParseSubElements(string value, char separator)
        {
            var result = new Dictionary<int, string>();
            if (string.IsNullOrEmpty(value))
                return result;

            var subElements = value.Split(separator);
            for (int i = 0; i < subElements.Length; i++)
            {
                result[i + 1] = subElements[i]; // 1-based indexing
            }
            return result;
        }

        private Dictionary<int, string> ParseRepeatingElements(string value, char separator)
        {
            var result = new Dictionary<int, string>();
            if (string.IsNullOrEmpty(value))
                return result;

            var repeatingElements = value.Split(separator);
            for (int i = 0; i < repeatingElements.Length; i++)
            {
                result[i + 1] = repeatingElements[i]; // 1-based indexing
            }
            return result;
        }
    }
}