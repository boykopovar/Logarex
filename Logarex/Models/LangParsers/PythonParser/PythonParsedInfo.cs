using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class PythonParsedInfo : IParsedInfo
{
    public IReadOnlyDictionary<string, int> Operators { get; }
    public IReadOnlyDictionary<string, int> Operands { get; }

    public PythonParsedInfo(
        IReadOnlyDictionary<string, int> operators,
        IReadOnlyDictionary<string, int> operands)
    {
        Operators = operators;
        Operands = operands;
    }
}