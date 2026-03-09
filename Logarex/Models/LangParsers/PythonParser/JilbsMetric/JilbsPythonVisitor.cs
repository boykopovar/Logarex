using Antlr4.Runtime.Tree;
using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class JilbsPythonVisitor : PythonOperatorVisitor
{
    private Dictionary<string, int> _branchingOperators = new();
    private int  _maxNesting = 0;
    public new IJilbsParsedInfo GetResult()
    {
        return new PythonJilbsParsedInfo(_operators, _branchingOperators, _maxNesting);
    }
    private void CoutBranchingOperator(ITerminalNode node)
    {
        var text = node.GetText();
        _branchingOperators[text] = _branchingOperators.GetValueOrDefault(text) + 1;
    }

    public override object VisitIf_stmt(Python3Parser.If_stmtContext context)
    {
        if (context.IF() != null)
        {
            CoutBranchingOperator(context.IF());
        }

        if (context.ELIF() != null)
        {
            foreach (var token in context.ELIF())
            {
                CoutBranchingOperator(token);
            }
        }
        return base.VisitIf_stmt(context);
    }
}