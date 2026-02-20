using Antlr4.Runtime.Tree;
using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class HalsteadPythonVisitor : Python3ParserBaseVisitor<object>
{
    private Dictionary<string, int> _operators =  new Dictionary<string, int>();
    private Dictionary<string, int> _operands =  new Dictionary<string, int>();

    public IParsedInfo GetResult()
    {
        return new PythonParsedInfo(_operators, _operands);
    }

    private void CountOperator(string op)
    {
        if (_operators.ContainsKey(op))
            _operators[op]++;
        else
            _operators[op] = 1;
    }

    private void CountOperand(string op)
    {
        if (_operands.ContainsKey(op))
            _operands[op]++;
        else
            _operands[op] = 1;
    }

    // =
    public override object VisitExpr_stmt(Python3Parser.Expr_stmtContext context)
    {
        var assignTokens = context.ASSIGN();
        if (assignTokens != null)
        {
            foreach (var token in assignTokens)
            {
                CountOperator(token.GetText());
            }
        }
        return base.VisitExpr_stmt(context);
    }

    // +=, -= и т.д.
    public override object VisitAugassign(Python3Parser.AugassignContext context)
    {
        string op = context.GetText();
        CountOperator(op);
        return base.VisitAugassign(context);
    }

    public override object VisitExpr(Python3Parser.ExprContext context)
    {
        if (context.STAR() != null)
            CountOperator(context.STAR().GetText());
        if (context.IDIV() != null)
            CountOperator(context.IDIV().GetText());
        if (context.MOD() != null)
            CountOperator(context.MOD().GetText());
        if (context.LEFT_SHIFT()  != null)
            CountOperator(context.LEFT_SHIFT().GetText());
        if (context.RIGHT_SHIFT() != null)
            CountOperator(context.RIGHT_SHIFT().GetText());
        if (context.ADD() != null)
        {
            foreach (var token in context.ADD())
            {
                CountOperator(token.GetText());
            }
        }
        if (context.MINUS() != null)
        {
            foreach (var token in context.MINUS())
            {
                CountOperator(token.GetText());
            }
        }
        
        return base.VisitExpr(context);
    }

    public override object VisitAtom_expr(Python3Parser.Atom_exprContext context)
    {
        bool isCall = context.trailer()?.Any(t => t.OPEN_PAREN() != null) ?? false;
        if (isCall)
        {
            var atom = context.atom();
            if (atom.name() != null)
            {
                CountOperator(atom.name().GetText());
            }
        }
        return base.VisitAtom_expr(context);
    }

    public override object VisitAtom(Python3Parser.AtomContext context)
    {
        if (context.name() != null)
        {
            if (!IsFunctionName(context))
            {
                string name = context.name().GetText();
                CountOperand(name);
            }
        }
        else if (context.NUMBER() != null)
        {
            CountOperand(context.NUMBER().GetText());
        }
        else if (context.STRING() != null)
        {
            foreach (var str in context.STRING())
            {
                CountOperand(str.GetText());
            }
        }
        // Литералы
        else if (context.ELLIPSIS() != null)
        {
            CountOperand(context.ELLIPSIS().GetText());
        }
        else if (context.TRUE() != null)
        {
            CountOperand(context.TRUE().GetText());
        }
        else if (context.FALSE() != null)
        {
            CountOperand(context.FALSE().GetText());
        }
        else if (context.NONE() != null)
        {
            CountOperand(context.NONE().GetText());
        }
        return base.VisitAtom(context);
    }

    public override object VisitTerminal(ITerminalNode node)
    {
        int tokenType = node.Symbol.Type;
        if (tokenType == Python3Parser.OPEN_PAREN 
            || tokenType == Python3Parser.OPEN_BRACK
            || tokenType == Python3Parser.OPEN_BRACE
            || tokenType == Python3Parser.COLON)
        {
            string op = node.GetText();
            CountOperator(op);
        }
        return base.VisitTerminal(node);
    }

    private bool IsFunctionName(Python3Parser.AtomContext context)
    {
        var parent = context.Parent as Python3Parser.Atom_exprContext;
        if (parent == null) return false;
        var trailers = parent.trailer();
        if (trailers != null)
        {
            foreach (var trailer in trailers)
            {
                if (trailer.OPEN_PAREN() != null)
                    return true;
            }
        }
        return false;
    }

    public override object VisitComparison(Python3Parser.ComparisonContext context)
    {
        var compOps = context.comp_op();
        if (compOps != null)
        {
            foreach (var compOp in compOps)
            {
                string op = compOp.GetText();
                CountOperator(op);
            }
        }
        return base.VisitComparison(context);
    }

    public override object VisitAnd_test(Python3Parser.And_testContext context)
    {
        if (context.AND() != null)
        {
            foreach (var token in context.AND())
            {
                CountOperator(token.GetText());
            }
        }
        return base.VisitAnd_test(context);
    }

    public override object VisitOr_test(Python3Parser.Or_testContext context)
    {
        if (context.OR() != null)
        {
            foreach (var token in context.OR())
            {
                CountOperator(token.GetText());
            }
        }
        return base.VisitOr_test(context);
    }

    public override object VisitNot_test(Python3Parser.Not_testContext context)
    {
        if (context.NOT() != null)
        {
            CountOperator(context.NOT().GetText());
        }
        return base.VisitNot_test(context);
    }

    public override object VisitIf_stmt(Python3Parser.If_stmtContext context)
    {
        CountOperator(context.IF().GetText());
        return base.VisitIf_stmt(context);
    }

    public override object VisitFor_stmt(Python3Parser.For_stmtContext context)
    {
        CountOperator(context.FOR().GetText());
        if (context.IN() != null)
        {
            CountOperator(context.IN().GetText());
        }
        return base.VisitFor_stmt(context);
    }

    public override object VisitWhile_stmt(Python3Parser.While_stmtContext context)
    {
        CountOperator(context.WHILE().GetText());
        return base.VisitWhile_stmt(context);
    }

    public override object VisitBreak_stmt(Python3Parser.Break_stmtContext context)
    {
        CountOperator(context.BREAK().GetText());
        return base.VisitBreak_stmt(context);
    }

    public override object VisitContinue_stmt(Python3Parser.Continue_stmtContext context)
    {
        CountOperator(context.CONTINUE().GetText());
        return base.VisitContinue_stmt(context);
    }

    public override object VisitReturn_stmt(Python3Parser.Return_stmtContext context)
    {
        CountOperator(context.RETURN().GetText());
        return base.VisitReturn_stmt(context);
    }

    public override object VisitYield_expr(Python3Parser.Yield_exprContext context)
    {
        CountOperator(context.YIELD().GetText());
        return base.VisitYield_expr(context);
    }

    public override object VisitRaise_stmt(Python3Parser.Raise_stmtContext context)
    {
        CountOperator(context.RAISE().GetText());
        return base.VisitRaise_stmt(context);
    }
    public override object VisitImport_name(Python3Parser.Import_nameContext context)
    {
        CountOperator(context.IMPORT().GetText());
        return base.VisitImport_name(context);
    }

    public override object VisitImport_from(Python3Parser.Import_fromContext context)
    {
        CountOperator(context.FROM().GetText());
        return base.VisitImport_from(context);
    }

    public override object VisitTry_stmt(Python3Parser.Try_stmtContext context)
    {
        CountOperator(context.TRY().GetText());
        return base.VisitTry_stmt(context);
    }
    public override object VisitImport_as_name(Python3Parser.Import_as_nameContext context)
    {
        if (context.AS() != null)
            CountOperator(context.AS().GetText());
        return base.VisitImport_as_name(context);
    }
    
    public override object VisitWith_stmt(Python3Parser.With_stmtContext context)
    {
        CountOperator(context.WITH().GetText());
        return base.VisitWith_stmt(context);
    }

    public override object VisitPass_stmt(Python3Parser.Pass_stmtContext context)
    {
        CountOperator(context.PASS().GetText());
        return base.VisitPass_stmt(context);
    }
}