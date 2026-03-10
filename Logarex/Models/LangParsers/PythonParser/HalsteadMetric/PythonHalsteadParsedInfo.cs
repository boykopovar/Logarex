using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class PythonHalsteadParsedInfo : IHalsteadParsedInfo
{
    public IReadOnlyDictionary<string, int> Operators { get; }
    public IReadOnlyDictionary<string, int> Operands { get; }

    public PythonHalsteadParsedInfo(
        IReadOnlyDictionary<string, int> operators,
        IReadOnlyDictionary<string, int> operands)
    {
        Operators = operators;
        Operands = operands;
    }
}