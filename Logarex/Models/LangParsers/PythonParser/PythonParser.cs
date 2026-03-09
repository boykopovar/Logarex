using Antlr4.Runtime;
using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class PythonParser : IHalsteadLangParser, IJilbsLangParser
{
    public string LanguageName => "Python";

    public HalsteadParseResult HalsteadParse(string source)
    {
        var input = new AntlrInputStream(source);
        var lexer = new Python3Lexer(input);
        var tokens = new CommonTokenStream(lexer);
        var  parser = new Python3Parser(tokens);
        
        parser.RemoveErrorListeners();

        var tree = parser.file_input();
        var visitorHal = new HalsteadPythonVisitor();
        visitorHal.Visit(tree);
        return new HalsteadParseResult
        {
            Metrics = visitorHal.GetResult(),
            Tokens = visitorHal.GetTokes()
        };
    }
    public JilbsParseResult JilbsParse(string source)
    {
        throw new NotImplementedException();
    }
}