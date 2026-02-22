namespace Logarex.Models.LangParsers.PythonParser;

public class TokenInfo
{
    public string Text { get; set; } =  string.Empty;
    public bool IsOperator { get; set; }
    public int StartIndex { get; set; }
    public int Length { get; set; }
}