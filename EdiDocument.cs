using System.Text;

namespace EdiLex
{
    public class EdiDocument
    {
        public List<EdiSegment> Segments { get; }
        public EdiSeparators Separators { get; }

        public EdiDocument(EdiSeparators separators)
        {
            Separators = separators ?? new EdiSeparators();
            Segments = new List<EdiSegment>();
        }

        public void Load(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                throw new ArgumentException("Stream cannot be null or empty.");

            using var reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Stream content cannot be empty.");

            var segmentStrings = content.Split(new[] { Separators.SegmentTerminator }, StringSplitOptions.RemoveEmptyEntries);
            Segments.Clear();
            foreach (var segmentString in segmentStrings.Select(s => s.Trim()))
            {
                if (!string.IsNullOrEmpty(segmentString))
                {
                    var segment = new EdiSegment(segmentString + Separators.SegmentTerminator, Separators);
                    Segments.Add(segment);
                }
            }
        }

        public void UpdateSESegmentCount()
        {
            // Find all ST segments
            var stIndices = Segments.Select((s, i) => new { Segment = s, Index = i })
                                   .Where(x => x.Segment.Id == "ST")
                                   .Select(x => x.Index)
                                   .ToList();

            foreach (var stIndex in stIndices)
            {
                // Find the next SE segment after the current ST
                var seIndex = Segments.FindIndex(stIndex, s => s.Id == "SE");
                if (seIndex == -1 || seIndex <= stIndex)
                    continue; // No corresponding SE or SE before ST, skip

                var seSegment = Segments[seIndex];
                if (!seSegment.Elements.ContainsKey(1))
                    continue; // SE segment missing SE01, skip

                // Count segments from ST to SE, inclusive
                int segmentCount = seIndex - stIndex + 1;
                seSegment.Elements[1] = new Element(segmentCount.ToString(), Separators.SubElementSeparator, Separators.RepeatingElementSeparator);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var segment in Segments)
            {
                sb.Append(segment.Id);
                foreach (var element in segment.Elements.OrderBy(e => e.Key))
                {
                    sb.Append(Separators.ElementSeparator).Append(element.Value.Value);
                }
                sb.Append(Separators.SegmentTerminator).Append('\n');
            }
            return sb.ToString();
        }
    }
}