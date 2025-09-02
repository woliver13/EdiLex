namespace EdiLex
{
    public class EdiSegmentParser
    {
        public char ElementSeparator { get; set; } = '*';
        public char SegmentTerminator { get; set; } = '~';
        public char SubElementSeparator { get; set; } = ':';
        public char RepeatingElementSeparator { get; set; } = '>';
        public char ControlCharacter { get; set; } = '>';

        public EdiSegment ParseSegment(string input)
        {
            var separators = new EdiSeparators
            {
                ElementSeparator = ElementSeparator,
                SegmentTerminator = SegmentTerminator,
                SubElementSeparator = SubElementSeparator,
                RepeatingElementSeparator = RepeatingElementSeparator,
                ControlCharacter = ControlCharacter
            };
            return new EdiSegment(input, separators);
        }
    }
}