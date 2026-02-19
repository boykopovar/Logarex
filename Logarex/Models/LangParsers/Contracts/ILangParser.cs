namespace Logarex.Models.LangParsers.Contracts;

public interface ILangParser
{
    string LanguageName { get; }
    IParsedInfo Parse(string source);
}