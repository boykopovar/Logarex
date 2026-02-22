using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class IParseResult
{
    public IParsedInfo Metrics { get; set; }
    public List<TokenInfo> Tokens { get; set; }
}