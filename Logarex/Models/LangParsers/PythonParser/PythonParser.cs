using Antlr4.Runtime;
using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class PythonParser : ILangParser
{
    public string LanguageName => "Python";
    
    private static readonly HashSet<int> OperatorTokenTypes =
[
    Python3Lexer.ADD,
    Python3Lexer.MINUS,
    Python3Lexer.STAR,
    Python3Lexer.DIV,
    Python3Lexer.MOD,
    Python3Lexer.POWER,
    Python3Lexer.IDIV,
    Python3Lexer.LEFT_SHIFT,
    Python3Lexer.RIGHT_SHIFT,
    Python3Lexer.XOR,
    Python3Lexer.AND_OP,
    Python3Lexer.OR_OP,

    Python3Lexer.ASSIGN,
    Python3Lexer.ADD_ASSIGN,
    Python3Lexer.SUB_ASSIGN,
    Python3Lexer.MULT_ASSIGN,
    Python3Lexer.DIV_ASSIGN,
    Python3Lexer.MOD_ASSIGN,
    Python3Lexer.AND_ASSIGN,
    Python3Lexer.OR_ASSIGN,
    Python3Lexer.XOR_ASSIGN,
    Python3Lexer.LEFT_SHIFT_ASSIGN,
    Python3Lexer.RIGHT_SHIFT_ASSIGN,
    Python3Lexer.POWER_ASSIGN,
    Python3Lexer.IDIV_ASSIGN,
    Python3Lexer.AT_ASSIGN,

    Python3Lexer.EQUALS,
    Python3Lexer.NOT_EQ_1,
    Python3Lexer.NOT_EQ_2,
    Python3Lexer.LESS_THAN,
    Python3Lexer.GREATER_THAN,
    Python3Lexer.LT_EQ,
    Python3Lexer.GT_EQ,
    Python3Lexer.IS,
    Python3Lexer.IN,

    Python3Lexer.AND,
    Python3Lexer.OR,
    Python3Lexer.NOT,

    Python3Lexer.IF,
    Python3Lexer.ELIF,
    Python3Lexer.ELSE,
    Python3Lexer.FOR,
    Python3Lexer.WHILE,
    Python3Lexer.RETURN,
    Python3Lexer.DEF,
    Python3Lexer.CLASS,
    Python3Lexer.TRY,
    Python3Lexer.EXCEPT,
    Python3Lexer.FINALLY,
    Python3Lexer.WITH,
    Python3Lexer.BREAK,
    Python3Lexer.CONTINUE,
    Python3Lexer.RAISE,
    Python3Lexer.YIELD,
    Python3Lexer.MATCH,
    Python3Lexer.CASE,
    Python3Lexer.ASYNC,
    Python3Lexer.AWAIT,
    Python3Lexer.LAMBDA,
    Python3Lexer.DEL,
    Python3Lexer.GLOBAL,
    Python3Lexer.NONLOCAL,
    Python3Lexer.IMPORT,
    Python3Lexer.FROM,
    Python3Lexer.AS,
    Python3Lexer.ASSERT,
    Python3Lexer.PASS,
    
    Python3Lexer.OPEN_PAREN,
    Python3Lexer.CLOSE_PAREN,
    Python3Lexer.OPEN_BRACK,
    Python3Lexer.CLOSE_BRACK,
    Python3Lexer.OPEN_BRACE,
    Python3Lexer.CLOSE_BRACE,
    Python3Lexer.DOT,
    Python3Lexer.COMMA,
    Python3Lexer.COLON,
    Python3Lexer.SEMI_COLON,
    Python3Lexer.ELLIPSIS,
    Python3Lexer.AT,
    Python3Lexer.ARROW
];

    
    private static readonly HashSet<int> OperandTokenTypes =
    [
        Python3Lexer.NAME,

        Python3Lexer.STRING,
        Python3Lexer.STRING_LITERAL,
        Python3Lexer.BYTES_LITERAL,
        Python3Lexer.NUMBER,
        Python3Lexer.INTEGER,
        Python3Lexer.DECIMAL_INTEGER,
        Python3Lexer.OCT_INTEGER,
        Python3Lexer.HEX_INTEGER,
        Python3Lexer.BIN_INTEGER,
        Python3Lexer.FLOAT_NUMBER,
        Python3Lexer.IMAG_NUMBER,

        Python3Lexer.TRUE,
        Python3Lexer.FALSE,
        Python3Lexer.NONE
    ];


    private bool IsOperand(int type) => OperandTokenTypes.Contains(type);
    private bool IsOperator(int type) => OperatorTokenTypes.Contains(type);

    public IParsedInfo Parse(string source)
    {
        var operators = new Dictionary<string, int>();
        var operands = new Dictionary<string, int>();

        var input = new AntlrInputStream(source);
        var lexer = new Python3Lexer(input);
        var tokens = new CommonTokenStream(lexer);
        var  parser = new Python3Parser(tokens);
        //tokens.Fill();
        
        // foreach (var token in tokens.GetTokens())
        // {
        //     if (token.Channel != TokenConstants.DefaultChannel)
        //         continue;
        //
        //     if (IsOperand(token.Type))
        //     {
        //         if (!operands.ContainsKey(token.Text))
        //             operands[token.Text] = 0;
        //
        //         operands[token.Text]++;
        //     }
        //     else if (IsOperator(token.Type))
        //     {
        //         if (!operators.ContainsKey(token.Text))
        //             operators[token.Text] = 0;
        //
        //         operators[token.Text]++;
        //     }
        // }
        
        //return new PythonParsedInfo(operators, operands);
        
        parser.RemoveErrorListeners();

        var tree = parser.file_input();
        var visitor = new HalsteadPythonVisitor();
        visitor.Visit(tree);
        return visitor.GetResult();
    }


}