using System.Text;
using Xunit;


namespace EdiLex.UnitTests
{

    public class EdiDocumentTests
    {
        private readonly EdiSegmentParser _parser;

        public EdiDocumentTests()
        {
            _parser = new EdiSegmentParser();
        }

        [Fact]
        public void EdiDocument_ToString_ValidDocument_ReturnsCorrectString()
        {
            // Arrange
            var document = new EdiDocument(new EdiSeparators());
            document.Segments.Add(_parser.ParseSegment("ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~"));
            document.Segments.Add(_parser.ParseSegment("GS*PO*SENDERID*RECEIVERID*20250902*1602*1*X*004010~"));
            document.Segments.Add(_parser.ParseSegment("ST*850*0001~"));
            document.Segments.Add(_parser.ParseSegment("N1*ST*NAME*FI>REP1>REP2>REP3~"));
            document.Segments.Add(_parser.ParseSegment("SE*4*0001~"));

            // Act
            string result = document.ToString();

            // Assert
            string expected = "ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~\n" +
                              "GS*PO*SENDERID*RECEIVERID*20250902*1602*1*X*004010~\n" +
                              "ST*850*0001~\n" +
                              "N1*ST*NAME*FI>REP1>REP2>REP3~\n" +
                              "SE*4*0001~";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EdiDocument_UpdateSESegmentCount_SegmentCountChanged_UpdatesSE01()
        {
            // Arrange
            var document = new EdiDocument(new EdiSeparators());
            document.Segments.Add(_parser.ParseSegment("ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~"));
            document.Segments.Add(_parser.ParseSegment("GS*PO*SENDERID*RECEIVERID*20250902*1602*1*X*004010~"));
            document.Segments.Add(_parser.ParseSegment("ST*850*0001~"));
            document.Segments.Add(_parser.ParseSegment("N1*ST*NAME*FI>REP1>REP2>REP3~"));
            document.Segments.Add(_parser.ParseSegment("SE*2*0001~")); // Incorrect count (2)
            document.Segments.Add(_parser.ParseSegment("GE*1*1~")); // After SE, not counted

            // Act
            document.UpdateSESegmentCount();

            // Assert
            var seSegment = document.Segments.FirstOrDefault(s => s.Id == "SE");
            Assert.NotNull(seSegment);
            Assert.Equal("3", seSegment.Elements[1].Value); // Counts ST, N1, SE (3 segments)
        }

        [Fact]
        public void EdiDocument_UpdateSESegmentCount_MultipleTransactionSets_UpdatesAllSE01()
        {
            // Arrange
            var document = new EdiDocument(new EdiSeparators());
            document.Segments.Add(_parser.ParseSegment("ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~"));
            document.Segments.Add(_parser.ParseSegment("GS*PO*SENDERID*RECEIVERID*20250902*1602*1*X*004010~"));
            document.Segments.Add(_parser.ParseSegment("ST*850*0001~"));
            document.Segments.Add(_parser.ParseSegment("N1*ST*NAME*FI>REP1>REP2>REP3~"));
            document.Segments.Add(_parser.ParseSegment("SE*2*0001~")); // Incorrect count (2)
            document.Segments.Add(_parser.ParseSegment("ST*850*0002~"));
            document.Segments.Add(_parser.ParseSegment("N1*ST*NAME2*FI>REP4>REP5~"));
            document.Segments.Add(_parser.ParseSegment("REF*BM*987654321~"));
            document.Segments.Add(_parser.ParseSegment("SE*2*0002~")); // Incorrect count (2)
            document.Segments.Add(_parser.ParseSegment("GE*2*1~"));
            document.Segments.Add(_parser.ParseSegment("IEA*1*000000001~"));

            // Act
            document.UpdateSESegmentCount();

            // Assert
            var seSegments = document.Segments.Where(s => s.Id == "SE").ToList();
            Assert.Equal(2, seSegments.Count);
            Assert.Equal("3", seSegments[0].Elements[1].Value); // First ST, N1, SE (3 segments)
            Assert.Equal("4", seSegments[1].Elements[1].Value); // Second ST, N1, REF, SE (4 segments)
        }

        [Fact]
        public void EdiDocument_UpdateSESegmentCount_NoSESegment_DoesNotThrow()
        {
            // Arrange
            var document = new EdiDocument(new EdiSeparators());
            document.Segments.Add(_parser.ParseSegment("ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~"));
            document.Segments.Add(_parser.ParseSegment("GS*PO*SENDERID*RECEIVERID*20250902*1602*1*X*004010~"));

            // Act
            document.UpdateSESegmentCount();

            // Assert
            Assert.Equal(2, document.Segments.Count); // No changes
        }

        [Fact]
        public void EdiDocument_UpdateSESegmentCount_EmptyDocument_DoesNotThrow()
        {
            // Arrange
            var document = new EdiDocument(new EdiSeparators());

            // Act
            document.UpdateSESegmentCount();

            // Assert
            Assert.Empty(document.Segments);
        }

        [Fact]
        public void EdiDocument_UpdateSESegmentCount_NoSTSegment_DoesNotUpdate()
        {
            // Arrange
            var document = new EdiDocument(new EdiSeparators());
            document.Segments.Add(_parser.ParseSegment("ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~"));
            document.Segments.Add(_parser.ParseSegment("GS*PO*SENDERID*RECEIVERID*20250902*1602*1*X*004010~"));
            document.Segments.Add(_parser.ParseSegment("N1*ST*NAME*FI>REP1>REP2>REP3~"));
            document.Segments.Add(_parser.ParseSegment("SE*2*0001~")); // Incorrect count

            // Act
            document.UpdateSESegmentCount();

            // Assert
            var seSegment = document.Segments.FirstOrDefault(s => s.Id == "SE");
            Assert.NotNull(seSegment);
            Assert.Equal("2", seSegment.Elements[1].Value); // Unchanged, no ST segment
        }

        [Fact]
        public void EdiDocument_Load_ValidStream_ReturnsCorrectDocument()
        {
            // Arrange
            var input = "ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~\n" +
                        "GS*PO*SENDERID*RECEIVERID*20250902*1602*1*X*004010~\n" +
                        "ST*850*0001~\n" +
                        "N1*ST*NAME*FI>REP1>REP2>REP3~\n" +
                        "SE*3*0001~";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            var document = new EdiDocument(new EdiSeparators());

            // Act
            document.Load(stream);

            // Assert
            Assert.Equal(5, document.Segments.Count);
            Assert.Equal("ISA", document.Segments[0].Id);
            Assert.Equal("GS", document.Segments[1].Id);
            Assert.Equal("ST", document.Segments[2].Id);
            Assert.Equal("N1", document.Segments[3].Id);
            Assert.Equal("SE", document.Segments[4].Id);
            Assert.Equal("3", document.Segments[4].Elements[1].Value);
            Assert.Equal("FI>REP1>REP2>REP3", document.Segments[3].Elements[3].Value);
            Assert.Equal("REP1", document.Segments[3].Elements[3].RepeatingElements[2]);
        }

        [Fact]
        public void EdiDocument_Load_EmptyStream_ThrowsArgumentException()
        {
            // Arrange
            var input = "";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            var document = new EdiDocument(new EdiSeparators());

            // Act & Assert
            Assert.Throws<ArgumentException>(() => document.Load(stream));
        }

        [Fact]
        public void EdiDocument_Load_InvalidSegment_ThrowsFormatException()
        {
            // Arrange
            var input = "ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~\n" +
                        "**INVALID~"; // Invalid segment ID
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            var document = new EdiDocument(new EdiSeparators());

            // Act & Assert
            Assert.Throws<FormatException>(() => document.Load(stream));
        }

        [Fact]
        public void EdiDocument_Load_MultipleTransactionSets_ReturnsCorrectDocument()
        {
            // Arrange
            var input = "ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~\n" +
                        "GS*PO*SENDERID*RECEIVERID*20250902*1602*1*X*004010~\n" +
                        "ST*850*0001~\n" +
                        "N1*ST*NAME*FI>REP1>REP2>REP3~\n" +
                        "SE*3*0001~\n" +
                        "ST*850*0002~\n" +
                        "N1*ST*NAME2*FI>REP4>REP5~\n" +
                        "REF*BM*987654321~\n" +
                        "SE*4*0002~\n" +
                        "GE*2*1~\n" +
                        "IEA*1*000000001~";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            var document = new EdiDocument(new EdiSeparators());

            // Act
            document.Load(stream);

            // Assert
            Assert.Equal(11, document.Segments.Count);
            Assert.Equal("ISA", document.Segments[0].Id);
            Assert.Equal("GS", document.Segments[1].Id);
            Assert.Equal("ST", document.Segments[2].Id);
            Assert.Equal("N1", document.Segments[3].Id);
            Assert.Equal("SE", document.Segments[4].Id);
            Assert.Equal("3", document.Segments[4].Elements[1].Value);
            Assert.Equal("ST", document.Segments[5].Id);
            Assert.Equal("N1", document.Segments[6].Id);
            Assert.Equal("REF", document.Segments[7].Id);
            Assert.Equal("SE", document.Segments[8].Id);
            Assert.Equal("4", document.Segments[8].Elements[1].Value);
            Assert.Equal("GE", document.Segments[9].Id);
            Assert.Equal("IEA", document.Segments[10].Id);
        }
    }
}