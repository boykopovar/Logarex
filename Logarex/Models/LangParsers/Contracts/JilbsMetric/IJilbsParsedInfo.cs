namespace Logarex.Models.LangParsers.Contracts;

public interface IJilbsParsedInfo
{
    IReadOnlyDictionary<string, int> Operators { get; }
    IReadOnlyDictionary<string, int> BranchingOperators { get; }
    int MaxNesting {get; }
    int UniqueOperatorsCount => Operators.Count;
    int TotalOperatorsCount => Operators.Values.Sum();
    public int AbsoluteComplexity => BranchingOperators.Values.Sum();
    public int RelativeComplexity => AbsoluteComplexity / Operators.Values.Sum();
}