namespace Logarex.Models.LangParsers.Contracts;

public interface IJilbsParsedInfo
{
    IReadOnlyDictionary<string, int> Operators { get; }
    IReadOnlyDictionary<string, int> BranchingOperators { get; }
    int MaxNesting { get; }
    int TotalStatements => Operators.Values.Sum();
    public double AbsoluteComplexity => BranchingOperators.Values.Sum();
    public double RelativeComplexity =>
        TotalStatements > 0 ? AbsoluteComplexity / TotalStatements : 0;
}