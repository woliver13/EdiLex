namespace EdiLex
{
    public class EdiSegment
    {
        public string? Id { get; set; }
        public Dictionary<int, Element>? Elements { get; set; }

        public EdiSegment(string id) { Id = id; }

        public EdiSegment(string input, EdiSeparators separators)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input cannot be empty");

            if (!input.EndsWith(separators.SegmentTerminator.ToString()))
                throw new FormatException("Segment must end with terminator");

            var parts = input.Split(separators.SegmentTerminator)[0].Split(separators.ElementSeparator);
            if (parts.Length < 1 || string.IsNullOrEmpty(parts[0]))
                throw new FormatException("Invalid segment ID");

            Id = parts[0];
            Elements = new Dictionary<int, Element>();
            for (int i = 1; i < parts.Length; i++) // 1-based indexing
            {
                var element = new Element(parts[i], separators.SubElementSeparator, separators.RepeatingElementSeparator);
                Elements[i] = element;
            }
        }

        public string this[int elementIndex, int subElementIndex]
        {
            get
            {
                ArgumentNullException.ThrowIfNull(Elements);
                if (!Elements.ContainsKey(elementIndex))
                    throw new KeyNotFoundException($"Element index {elementIndex} not found.");
                if (!Elements[elementIndex].SubElements.ContainsKey(subElementIndex))
                    throw new KeyNotFoundException($"Sub-element index {subElementIndex} not found for element {elementIndex}.");
                return Elements[elementIndex].SubElements[subElementIndex];
            }
        }
    }
}