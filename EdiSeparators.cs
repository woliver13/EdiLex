namespace EdiLex { 
    public class EdiSeparators
{
    public char ElementSeparator { get; set; } = '*'; // X12 standard
    public char SegmentTerminator { get; set; } = '~'; // X12 standard
    public char SubElementSeparator { get; set; } = ':'; // X12 standard
    public char RepeatingElementSeparator { get; set; } = '>'; // X12 standard
    public char ControlCharacter { get; set; } = '>'; // X12 standard (often ISA16)
}
}