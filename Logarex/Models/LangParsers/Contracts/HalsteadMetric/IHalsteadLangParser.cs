using Logarex.Models.LangParsers.PythonParser;

namespace Logarex.Models.LangParsers.Contracts;

public interface IHalsteadLangParser
{
    string LanguageName { get; }
    public HalsteadParseResult HalsteadParse(string source);
}