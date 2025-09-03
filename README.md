# EdiLex
Lexical Parser for EDI X12 documents written with Grok.

# EDI X12 Parser Documentation

This project provides a C# library for parsing and manipulating EDI X12 documents, with support for handling multiple ST-SE transaction sets. It includes a robust test suite to validate functionality. The project is structured as a .NET 8.0 class library with a test project configuration, using Shouldly for assertions and Xunit for testing.

## Project Structure

The project is organized into the following files, as defined in `EdiX12Parser.csproj`:

- **EdiSeparators.cs**: Defines the `EdiSeparators` class for managing EDI X12 separator characters.
- **EdiSegmentParser.cs**: Contains the `EdiSegmentParser` class for parsing individual EDI segments.
- **EdiDocument.cs**: Implements the `EdiDocument` class for managing entire EDI documents, including loading and updating segment counts.
- **EdiSegment.cs**: Defines the `EdiSegment` class for representing individual EDI segments.
- **Element.cs**: Contains the `Element` class for handling segment elements, sub-elements, and repeating elements.
- **Tests/EdiX12ParserTests.cs**: Includes the test suite for validating parser functionality.

The project targets .NET 8.0 and includes NuGet dependencies for Shouldly (v4.2.1), Xunit (v2.9.0), and `xunit.runner.visualstudio` (v2.8.2). The namespace for core classes is `EdiX12Parser`, with tests under `EdiX12Parser.Tests`.

## Classes and Functionality

### EdiSeparators

**Namespace**: `EdiX12Parser`

**Purpose**: Defines the separator characters used in EDI X12 documents, adhering to industry standards.

**Properties**:
- `ElementSeparator` (`char`, default: `*`): Separates elements within a segment.
- `SegmentTerminator` (`char`, default: `~`): Marks the end of a segment.
- `SubElementSeparator` (`char`, default: `:`): Separates sub-elements within an element.
- `RepeatingElementSeparator` (`char`, default: `>`): Separates repeating elements.
- `ControlCharacter` (`char`, default: `>`): Used for specific control purposes (e.g., ISA16).

**Usage**:
```csharp
var separators = new EdiSeparators
{
    ElementSeparator = '|',
    SegmentTerminator = '~',
    SubElementSeparator = ':',
    RepeatingElementSeparator = '>',
    ControlCharacter = '?'
};
```

### EdiSegmentParser

**Namespace**: `EdiX12Parser`

**Purpose**: Parses individual EDI X12 segments into `EdiSegment` objects.

**Properties**:
- `ElementSeparator` (`char`, default: `*`): Element separator.
- `SegmentTerminator` (`char`, default: `~`): Segment terminator.
- `SubElementSeparator` (`char`, default: `:`): Sub-element separator.
- `RepeatingElementSeparator` (`char`, default: `>`): Repeating element separator.
- `ControlCharacter` (`char`, default: `>`): Control character.

**Methods**:
- `EdiSegment ParseSegment(string input)`: Parses a single EDI segment string into an `EdiSegment` object. Throws `ArgumentException` for empty input and `FormatException` for invalid segment ID or missing terminator.

**Usage**:
```csharp
var parser = new EdiSegmentParser();
var segment = parser.ParseSegment("ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~");
Console.WriteLine(segment.Id); // Outputs: ISA
Console.WriteLine(segment.Elements[1].Value); // Outputs: 00
```

### EdiDocument

**Namespace**: `EdiX12Parser`

**Purpose**: Represents an entire EDI X12 document, managing a collection of segments and providing methods for loading and updating segment counts.

**Properties**:
- `Segments` (`List<EdiSegment>`): List of segments in the document.
- `Separators` (`EdiSeparators`): Separator configuration for the document.

**Methods**:
- `EdiDocument(EdiSeparators separators)`: Constructor initializing the document with specified separators.
- `void Load(Stream stream)`: Loads an EDI document from a stream, parsing each segment. Throws `ArgumentException` for null or empty streams and `FormatException` for invalid segments.
- `void UpdateSESegmentCount()`: Updates the SE01 element in each SE segment to reflect the number of segments in its corresponding ST-SE transaction set (inclusive of ST and SE). Skips invalid cases (e.g., no SE, SE before ST, missing SE01).
- `override string ToString()`: Serializes the document to a string, with segments separated by newlines and the segment terminator.

**Usage**:
```csharp
var document = new EdiDocument(new EdiSeparators());
var input = "ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~\n" +
            "GS*PO*SENDERID*RECEIVERID*20250902*1602*1*X*004010~\n" +
            "ST*850*0001~\n" +
            "N1*ST*NAME*FI>REP1>REP2>REP3~\n" +
            "SE*2*0001~\n" +
            "ST*850*0002~\n" +
            "N1*ST*NAME2*FI>REP4>REP5~\n" +
            "SE*2*0002~\n" +
            "GE*2*1~\n" +
            "IEA*1*000000001~";
using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
document.Load(stream);
document.UpdateSESegmentCount();
Console.WriteLine(document.ToString()); // Outputs updated document with correct SE01 counts
```

### EdiSegment

**Namespace**: `EdiX12Parser`

**Purpose**: Represents a single EDI X12 segment, including its ID and elements.

**Properties**:
- `Id` (`string`): The segment identifier (e.g., "ISA", "ST").
- `Elements` (`Dictionary<int, Element>`): Dictionary of elements, indexed starting at 1.

**Methods**:
- `EdiSegment()`: Default constructor.
- `EdiSegment(string input, EdiSeparators separators)`: Parses a segment string into an `EdiSegment` object. Throws `ArgumentException` for empty input and `FormatException` for invalid segment ID or missing terminator.
- `string this[int elementIndex, int subElementIndex]`: Indexer to access sub-elements by element and sub-element indices (1-based). Throws `KeyNotFoundException` for invalid indices.

**Usage**:
```csharp
var separators = new EdiSeparators();
var segment = new EdiSegment("LX*1:SUB1:SUB2:SUB3~", separators);
Console.WriteLine(segment.Id); // Outputs: LX
Console.WriteLine(segment[1, 2]); // Outputs: SUB1
```

### Element

**Namespace**: `EdiX12Parser`

**Purpose**: Represents an element within a segment, including its value, sub-elements, and repeating elements.

**Properties**:
- `Value` (`string`): The full element value.
- `SubElements` (`Dictionary<int, string>`): Sub-elements, indexed starting at 1.
- `RepeatingElements` (`Dictionary<int, string>`): Repeating elements, indexed starting at 1.

**Methods**:
- `Element(string value, char subElementSeparator, char repeatingElementSeparator)`: Constructor parsing the element value into sub-elements and repeating elements.
- `private Dictionary<int, string> ParseSubElements(string value, char separator)`: Parses sub-elements using the specified separator.
- `private Dictionary<int, string> ParseRepeatingElements(string value, char separator)`: Parses repeating elements using the specified separator.

**Usage**:
```csharp
var element = new Element("FI>REP1>REP2>REP3", ':', '>');
Console.WriteLine(element.Value); // Outputs: FI>REP1>REP2>REP3
Console.WriteLine(element.RepeatingElements[2]); // Outputs: REP1
```

## Test Suite

**Namespace**: `EdiX12Parser.Tests`

**File**: `Tests/EdiX12ParserTests.cs`

**Purpose**: Validates the functionality of the EDI X12 parser using Xunit and Shouldly for fluent assertions.

**Test Cases**:
- **Segment Parsing**:
  - `ParseSegment_ValidSegment`: Verifies correct parsing of a valid ISA segment.
  - `ParseSegment_EmptyInput`: Ensures empty input throws `ArgumentException`.
  - `ParseSegment_InvalidSegmentId`: Ensures invalid segment ID throws `FormatException`.
  - `ParseSegment_MissingTerminator`: Ensures missing terminator throws `FormatException`.
  - `ParseSegment_ValidWithCustomSeparators`: Verifies parsing with custom separators.
  - `ParseSegment_MultipleSegments_ProcessesOnlyFirst`: Ensures only the first segment is processed.
- **EdiSegment Constructor**:
  - `EdiSegmentConstructor_ValidSegmentString`: Verifies correct segment construction.
  - `EdiSegmentConstructor_EmptyInput`: Ensures empty input throws `ArgumentException`.
  - `EdiSegmentConstructor_InvalidSegmentId`: Ensures invalid segment ID throws `FormatException`.
  - `EdiSegmentConstructor_MissingTerminator`: Ensures missing terminator throws `FormatException`.
- **Indexer**:
  - `EdiSegmentIndexer_ValidIndices`: Verifies correct sub-element access.
  - `EdiSegmentIndexer_InvalidElementIndex`: Ensures invalid element index throws `KeyNotFoundException`.
  - `EdiSegmentIndexer_InvalidSubElementIndex`: Ensures invalid sub-element index throws `KeyNotFoundException`.
- **Element Parsing**:
  - `ParseElements_ValidElements`: Verifies correct element parsing.
  - `ParseSubElements_ValidSubElements`: Verifies correct sub-element parsing.
  - `ParseSubElements_NoSubElements`: Ensures no sub-elements return an empty list.
  - `ParseSubElements_EmptyElement`: Ensures empty elements return an empty list.
  - `ParseRepeatingElements_ValidRepeatingElements`: Verifies correct repeating element parsing.
  - `ParseRepeatingElements_NoRepeatingElements`: Ensures no repeating elements return an empty list.
- **Document Operations**:
  - `EdiDocument_ToString_ValidDocument`: Verifies correct document serialization.
  - `EdiDocument_UpdateSESegmentCount_SegmentCountChanged`: Verifies SE01 update for a single ST-SE transaction set.
  - `EdiDocument_UpdateSESegmentCount_MultipleTransactionSets`: Verifies SE01 updates for multiple ST-SE transaction sets.
  - `EdiDocument_UpdateSESegmentCount_NoSESegment`: Ensures no SE segment does not throw.
  - `EdiDocument_UpdateSESegmentCount_EmptyDocument`: Ensures empty document does not throw.
  - `EdiDocument_UpdateSESegmentCount_NoSTSegment`: Ensures SE01 is unchanged without an ST segment.
  - `EdiDocument_Load_ValidStream`: Verifies correct document loading from a stream.
  - `EdiDocument_Load_EmptyStream`: Ensures empty stream throws `ArgumentException`.
  - `EdiDocument_Load_InvalidSegment`: Ensures invalid segment throws `FormatException`.
  - `EdiDocument_Load_MultipleTransactionSets`: Verifies correct loading of multiple ST-SE transaction sets.

**Usage**:
Run tests using an Xunit test runner (e.g., Visual Studio or `dotnet test`). All tests use Shouldly for fluent assertions, ensuring clear and expressive validation of parser behavior.

## Project Setup

1. **Prerequisites**:
   - .NET 8.0 SDK
   - Visual Studio or another IDE with Xunit support (optional for running tests)

2. **Building the Project**:
   ```bash
   dotnet build EdiX12Parser.csproj
   ```

3. **Running Tests**:
   ```bash
   dotnet test EdiX12Parser.csproj
   ```

4. **Dependencies**:
   - Shouldly (v4.2.1): For fluent assertions in tests.
   - Xunit (v2.9.0): Testing framework.
   - `xunit.runner.visualstudio` (v2.8.2): Test runner for Visual Studio integration.

## Example Usage

```csharp
using EdiX12Parser;
using System.IO;
using System.Text;

// Create a parser and document
var parser = new EdiSegmentParser();
var document = new EdiDocument(new EdiSeparators());

// Load an EDI document from a string
var ediContent = "ISA*00*          *00*          *ZZ*SENDERID     *ZZ*RECEIVERID  *250902*1602*U*00401*000000001*0*P*>~\n" +
                 "GS*PO*SENDERID*RECEIVERID*20250902*1602*1*X*004010~\n" +
                 "ST*850*0001~\n" +
                 "N1*ST*NAME*FI>REP1>REP2>REP3~\n" +
                 "SE*2*0001~\n" +
                 "GE*1*1~\n" +
                 "IEA*1*000000001~";
using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ediContent));
document.Load(stream);

// Update SE segment counts
document.UpdateSESegmentCount();

// Output the updated document
Console.WriteLine(document.ToString());
```

This will output the EDI document with the SE01 element updated to `3` for the ST-SE transaction set, reflecting the correct segment count (ST, N1, SE).

## Notes
- The parser uses 1-based indexing for elements, sub-elements, and repeating elements, aligning with EDI X12 conventions.
- The `UpdateSESegmentCount` method supports multiple ST-SE transaction sets, updating each SE01 element to reflect the number of segments in its respective transaction set.
- All separator characters are configurable, with industry-standard defaults (`*`, `~`, `:`, `>`).
- The test suite provides comprehensive coverage, validating parsing, serialization, and edge cases.
- The project is designed for modularity, with each class in a separate file for maintainability.

For further details, refer to the source code and test cases in the respective `.cs` files.