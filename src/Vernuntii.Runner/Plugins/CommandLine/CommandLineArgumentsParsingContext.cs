using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Plugins.CommandLine;

/// <summary>
/// Represents the context of the command-line arguments parsing.
/// </summary>
public sealed class CommandLineArgumentsParsingContext
{
    public ParseResult? ParseResult { get; set; }

    [MemberNotNullWhen(true, nameof(ParseResult))]
    public bool HasParseResult => ParseResult is not null;

    internal CommandLineArgumentsParsingContext()
    {
    }
}
