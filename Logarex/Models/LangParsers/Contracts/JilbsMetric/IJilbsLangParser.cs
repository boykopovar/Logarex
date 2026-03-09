using Logarex.Models.LangParsers.PythonParser;

namespace Logarex.Models.LangParsers.Contracts;

public interface IJilbsLangParser
{
    protected string LanguageName { get; }
    public JilbsParseResult JilbsParse(string source);
}