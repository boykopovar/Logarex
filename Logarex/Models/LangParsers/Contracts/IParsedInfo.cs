namespace Logarex.Models.LangParsers.Contracts;

public interface IParsedInfo
{
    IReadOnlyDictionary<string, int> Operators { get; }
    IReadOnlyDictionary<string, int> Operands { get; }

    int UniqueOperatorsCount => Operators.Count;
    int UniqueOperandsCount => Operands.Count;

    int TotalOperatorsCount => Operators.Values.Sum();
    int TotalOperandsCount => Operands.Values.Sum();

    int ProgramVocabulary => UniqueOperatorsCount + UniqueOperandsCount;

    int ProgramLength => TotalOperatorsCount + TotalOperandsCount;

    double ProgramVolume =>
        ProgramLength * Math.Log2(ProgramVocabulary);
}