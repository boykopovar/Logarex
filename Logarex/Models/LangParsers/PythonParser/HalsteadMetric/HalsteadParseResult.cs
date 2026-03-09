using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class HalsteadParseResult
{
    public IHalsteadParsedInfo Metrics { get; set; }
    public List<TokenInfo> Tokens { get; set; }
}