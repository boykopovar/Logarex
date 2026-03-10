using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class PythonJilbsParsedInfo : IJilbsParsedInfo
{
    public IReadOnlyDictionary<string, int> Operators { get; }
    public IReadOnlyDictionary<string, int> BranchingOperators { get; }
    public int MaxNesting { get; }

    public PythonJilbsParsedInfo(
        IReadOnlyDictionary<string, int> operators,
        IReadOnlyDictionary<string, int> branchingOperators,
        int maxNesting)
    {
        Operators = operators;
        BranchingOperators = branchingOperators;
        MaxNesting = maxNesting;
    }
}