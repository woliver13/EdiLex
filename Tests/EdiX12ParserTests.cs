namespace EdiLex.UnitTests
{
    [TestClass]
    public class EdiX12ParserTests
    {
        private readonly EdiSegmentParser _parser;

        public EdiX12ParserTests()
        {
            _parser = new EdiSegmentParser();
        }

        [TestMethod]
        public void ParseSegment_ValidSegment_ReturnsCorrectSegment()
        {
            // Arrange
            string input = "ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~";

            // Act
            var segment = _parser.ParseSegment(input);

            // Assert
            Assert.IsNotNull(segment);
            Assert.AreEqual("ISA", segment.Id);
            Assert.AreEqual(16, segment.Elements.Count);
            Assert.AreEqual("00", segment.Elements[1].Value);
            Assert.AreEqual("ZZ", segment.Elements[4].Value);
            Assert.AreEqual("250902", segment.Elements[9].Value);
            Assert.AreEqual(">", segment.Elements[16].Value);
            Assert.IsEmpty(segment.Elements[1].SubElements);
            Assert.IsEmpty(segment.Elements[1].RepeatingElements);
        }

        [TestMethod]
        public void EdiSegmentConstructor_ValidSegmentString_ReturnsCorrectSegment()
        {
            // Arrange
            string input = "ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~";
            var separators = new EdiSeparators();

            // Act
            var segment = new EdiSegment(input, separators);

            // Assert
            Assert.IsNotNull(segment);
            Assert.AreEqual("ISA", segment.Id);
            Assert.AreEqual(16, segment.Elements.Count);
            Assert.AreEqual("00", segment.Elements[1].Value);
            Assert.AreEqual("ZZ", segment.Elements[4].Value);
            Assert.AreEqual("250902", segment.Elements[9].Value);
            Assert.AreEqual(">", segment.Elements[16].Value);
            Assert.IsEmpty(segment.Elements[1].SubElements);
            Assert.IsEmpty(segment.Elements[16].RepeatingElements);
        }

        [TestMethod]
        public void EdiSegmentIndexer_ValidIndices_ReturnsCorrectSubElement()
        {
            // Arrange
            string input = "LX*1:SUB1:SUB2:SUB3~";
            var segment = _parser.ParseSegment(input);

            // Act
            var subElement = segment[1, 1];

            // Assert
            Assert.AreEqual("1", subElement);
            Assert.AreEqual("SUB1", segment[1, 2]);
            Assert.AreEqual("SUB2", segment[1, 3]);
            Assert.AreEqual("SUB3", segment[1, 4]);
        }

        [TestMethod]
        public void EdiSegmentIndexer_InvalidElementIndex_ThrowsKeyNotFoundException()
        {
            // Arrange
            string input = "LX*1:SUB1:SUB2:SUB3~";
            var segment = _parser.ParseSegment(input);

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => segment[2, 1]);
        }

        [TestMethod]
        public void EdiSegmentIndexer_InvalidSubElementIndex_ThrowsKeyNotFoundException()
        {
            // Arrange
            string input = "LX*1:SUB1:SUB2:SUB3~";
            var segment = _parser.ParseSegment(input);

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => segment[1, 5]);
        }

        [TestMethod]
        public void ParseSegment_EmptyInput_ThrowsArgumentException()
        {
            // Arrange
            string input = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _parser.ParseSegment(input));
        }

        [TestMethod]
        public void EdiSegmentConstructor_EmptyInput_ThrowsArgumentException()
        {
            // Arrange
            string input = "";
            var separators = new EdiSeparators();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new EdiSegment(input, separators));
        }

        [TestMethod]
        public void ParseSegment_InvalidSegmentId_ThrowsFormatException()
        {
            // Arrange
            string input = "**00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~";

            // Act & Assert
            Assert.Throws<FormatException>(() => _parser.ParseSegment(input));
        }

        [TestMethod]
        public void EdiSegmentConstructor_InvalidSegmentId_ThrowsFormatException()
        {
            // Arrange
            string input = "**00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~";
            var separators = new EdiSeparators();

            // Act & Assert
            Assert.Throws<FormatException>(() => new EdiSegment(input, separators));
        }

        [TestMethod]
        public void ParseElements_ValidElements_ReturnsCorrectElementList()
        {
            // Arrange
            string input = "REF*BM*123456789~";

            // Act
            var segment = _parser.ParseSegment(input);

            // Assert
            Assert.IsNotNull(segment);
            Assert.AreEqual("REF", segment.Id);
            Assert.AreEqual(2, segment.Elements.Count);
            Assert.AreEqual("BM", segment.Elements[1].Value);
            Assert.AreEqual("123456789", segment.Elements[2].Value);
            Assert.IsEmpty(segment.Elements[1].SubElements);
            Assert.IsEmpty(segment.Elements[2].RepeatingElements);
        }

        [TestMethod]
        public void ParseSubElements_ValidSubElements_ReturnsCorrectSubElementList()
        {
            // Arrange
            string input = "LX*1:SUB1:SUB2:SUB3~";

            // Act
            var segment = _parser.ParseSegment(input);

            // Assert
            Assert.IsNotNull(segment);
            Assert.AreEqual("LX", segment.Id);
            Assert.AreEqual(1, segment.Elements.Count);
            Assert.AreEqual("1:SUB1:SUB2:SUB3", segment.Elements[1].Value);
            Assert.AreEqual(4, segment.Elements[1].SubElements.Count);
            Assert.AreEqual("1", segment.Elements[1].SubElements[1]);
            Assert.AreEqual("SUB1", segment.Elements[1].SubElements[2]);
            Assert.AreEqual("SUB2", segment.Elements[1].SubElements[3]);
            Assert.AreEqual("SUB3", segment.Elements[1].SubElements[4]);
            Assert.IsEmpty(segment.Elements[1].RepeatingElements);
        }

        [TestMethod]
        public void ParseSubElements_NoSubElements_ReturnsEmptySubElementList()
        {
            // Arrange
            string input = "LX*1*ELEMENT~";

            // Act
            var segment = _parser.ParseSegment(input);

            // Assert
            Assert.IsNotNull(segment);
            Assert.AreEqual("LX", segment.Id);
            Assert.AreEqual(2, segment.Elements.Count);
            Assert.AreEqual("ELEMENT", segment.Elements[2].Value);
            Assert.IsEmpty(segment.Elements[2].SubElements);
            Assert.IsEmpty(segment.Elements[2].RepeatingElements);
        }

        [TestMethod]
        public void ParseRepeatingElements_ValidRepeatingElements_ReturnsCorrectList()
        {
            // Arrange
            string input = "N1*ST*NAME*FI>REP1>REP2>REP3~";

            // Act
            var segment = _parser.ParseSegment(input);

            // Assert
            Assert.IsNotNull(segment);
            Assert.AreEqual("N1", segment.Id);
            Assert.AreEqual(3, segment.Elements.Count);
            Assert.AreEqual("FI>REP1>REP2>REP3", segment.Elements[3].Value);
            Assert.AreEqual(4, segment.Elements[3].RepeatingElements.Count);
            Assert.AreEqual("FI", segment.Elements[3].RepeatingElements[1]);
            Assert.AreEqual("REP1", segment.Elements[3].RepeatingElements[2]);
            Assert.AreEqual("REP2", segment.Elements[3].RepeatingElements[3]);
            Assert.AreEqual("REP3", segment.Elements[3].RepeatingElements[4]);
            Assert.IsEmpty(segment.Elements[3].SubElements);
        }

        [TestMethod]
        public void ParseRepeatingElements_NoRepeatingElements_ReturnsEmptyList()
        {
            // Arrange
            string input = "N1*ST*NAME*FI*SINGLE~";

            // Act
            var segment = _parser.ParseSegment(input);

            // Assert
            Assert.IsNotNull(segment);
            Assert.AreEqual("N1", segment.Id);
            Assert.AreEqual(3, segment.Elements.Count);
            Assert.AreEqual("FI", segment.Elements[3].Value);
            Assert.IsEmpty(segment.Elements[3].RepeatingElements);
            Assert.IsEmpty(segment.Elements[3].SubElements);
        }

        [TestMethod]
        public void ParseSegment_MissingTerminator_ThrowsFormatException()
        {
            // Arrange
            string input = "ISA*00*          *00*          *ZZ*SENDERID";

            // Act & Assert
            Assert.Throws<FormatException>(() => _parser.ParseSegment(input));
        }

        [TestMethod]
        public void EdiSegmentConstructor_MissingTerminator_ThrowsFormatException()
        {
            // Arrange
            string input = "ISA*00*          *00*          *ZZ*SENDERID";
            var separators = new EdiSeparators();

            // Act & Assert
            Assert.Throws<FormatException>(() => new EdiSegment(input, separators));
        }

        [TestMethod]
        public void ParseSegment_ValidWithCustomSeparators_ReturnsCorrectSegment()
        {
            // Arrange
            string input = "ISA|00|          |00|          |ZZ|SENDERID     |ZZ|RECEIVERID  |250902|1602|U|00401*000000001|0|P|?~";
            _parser.ElementSeparator = '|';
            _parser.SegmentTerminator = '~';
            _parser.ControlCharacter = '?';

            // Act
            var segment = _parser.ParseSegment(input);

            // Assert
            Assert.IsNotNull(segment);
            Assert.AreEqual("ISA", segment.Id);
            Assert.AreEqual(16, segment.Elements.Count);
            Assert.AreEqual("00", segment.Elements[1].Value);
            Assert.AreEqual("?", segment.Elements[16].Value);
            Assert.IsEmpty(segment.Elements[1].SubElements);
            Assert.IsEmpty(segment.Elements[16].RepeatingElements);
        }

        [TestMethod]
        public void ParseSegment_MultipleSegments_ProcessesOnlyFirst()
        {
            // Arrange
            string input = "ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~GS*PO*SENDERID*RECEIVERID*20250902*1602*1*X*004010~";

            // Act
            var segment = _parser.ParseSegment(input);

            // Assert
            Assert.IsNotNull(segment);
            Assert.AreEqual("ISA", segment.Id);
            Assert.AreEqual(16, segment.Elements.Count);
            Assert.IsEmpty(segment.Elements[1].SubElements);
            Assert.IsEmpty(segment.Elements[16].RepeatingElements);
        }

        [TestMethod]
        public void ParseSubElements_EmptyElement_ReturnsEmptyList()
        {
            // Arrange
            string input = "LX*1**~";

            // Act
            var segment = _parser.ParseSegment(input);

            // Assert
            Assert.IsNotNull(segment);
            Assert.AreEqual("LX", segment.Id);
            Assert.AreEqual(2, segment.Elements.Count);
            Assert.AreEqual("", segment.Elements[2].Value);
            Assert.IsEmpty(segment.Elements[2].SubElements);
            Assert.IsEmpty(segment.Elements[2].RepeatingElements);
        }
    }
}
