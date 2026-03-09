using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class JilbsParseResult
{
    public IJilbsParsedInfo Metrics { get; set; }
    public List<TokenInfo> Tokens { get; set; }
}