using Logarex.Models.LangParsers.PythonParser;

namespace Logarex.Models.LangParsers.Contracts;

public interface ILangParser
{
    string LanguageName { get; }
    IParseResult Parse(string source);
}