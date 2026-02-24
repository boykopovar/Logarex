using Antlr4.Runtime.Tree;
using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class HalsteadPythonVisitor : Python3ParserBaseVisitor<object>
{
    private Dictionary<string, int> _operators =  new Dictionary<string, int>();
    private Dictionary<string, int> _operands =  new Dictionary<string, int>();
    private List<TokenInfo> _tokens = new List<TokenInfo>();

    public List<TokenInfo> GetTokes() => _tokens;
    public IParsedInfo GetResult()
    {
        return new PythonParsedInfo(_operators, _operands);
    }

    private void CountOperator(ITerminalNode node)
    {
        var text = node.GetText();
        if (_operators.ContainsKey(text))
            _operators[text]++;
        else
            _operators[text] = 1;
        _tokens.Add(new TokenInfo()
        {
            Text = text,
            IsOperator = true,
            StartIndex = node.Symbol.StartIndex,
            Length = node.Symbol.StopIndex - node.Symbol.StartIndex + 1
        });
    }
    
    private void CountOperatorAddToken(ITerminalNode node)
    {
        var text = node.GetText();
        _tokens.Add(new TokenInfo()
        {
            Text = text,
            IsOperator = true,
            StartIndex = node.Symbol.StartIndex,
            Length = node.Symbol.StopIndex - node.Symbol.StartIndex + 1
        });
    }
    
    private void CountOperand(ITerminalNode node)
    {
        var text = node.GetText();
        if (_operands.ContainsKey(text))
            _operands[text]++;
        else
            _operands[text] = 1;
        _tokens.Add(new TokenInfo()
        {
            Text = text,
            IsOperator = false,
            StartIndex = node.Symbol.StartIndex,
            Length = node.Symbol.StopIndex - node.Symbol.StartIndex + 1
        });
    }

    // =
    public override object VisitExpr_stmt(Python3Parser.Expr_stmtContext context)
    {
        var assignTokens = context.ASSIGN();
        if (assignTokens != null)
        {
            foreach (var token in assignTokens)
            {
                CountOperator(token);
            }
        }
        return base.VisitExpr_stmt(context);
    }

    // +=, -= и т.д.
    public override object VisitAugassign(Python3Parser.AugassignContext context)
    {
        var token = context.children.OfType<ITerminalNode>().FirstOrDefault();
        if (token != null)
            CountOperator(token);
        return base.VisitAugassign(context);
    }

    public override object VisitExpr(Python3Parser.ExprContext context)
    {
        if (context.STAR() != null)
            CountOperator(context.STAR());
        if (context.IDIV() != null)
            CountOperator(context.IDIV());
        if (context.MOD() != null)
            CountOperator(context.MOD());
        if (context.LEFT_SHIFT()  != null)
            CountOperator(context.LEFT_SHIFT());
        if (context.RIGHT_SHIFT() != null)
            CountOperator(context.RIGHT_SHIFT());
        if (context.ADD() != null)
        {
            foreach (var token in context.ADD())
            {
                CountOperator(token);
            }
        }
        if (context.MINUS() != null)
        {
            foreach (var token in context.MINUS())
            {
                CountOperator(token);
            }
        }
        if (context.XOR() != null)
            CountOperator(context.XOR());
        if (context.AND_OP() != null)
            CountOperator(context.AND_OP());
        if (context.OR_OP() != null)
            CountOperator(context.OR_OP());
        if (context.POWER()  != null)
            CountOperator(context.POWER());
        if (context.AT()  != null)
            CountOperator(context.AT());
        
        return base.VisitExpr(context);
    }

    public override object VisitArgument(Python3Parser.ArgumentContext context)
    {
        if (context.STAR() != null)
            CountOperator(context.STAR());
        if (context.POWER()   != null)
            CountOperator(context.POWER());
        return base.VisitArgument(context);
    }

    public override object VisitAtom_expr(Python3Parser.Atom_exprContext context)
    {
        var trailers = context.trailer();
        foreach (var trailer in trailers)
        {
            if (trailer.DOT() != null && trailer.name() != null)
            {
                var nameToken = trailer.name().GetChild(0) as ITerminalNode;
                if (nameToken != null)
                    CountOperator(nameToken);
            }
        }
        
        bool isCall = context.trailer()?.Any(t => t.OPEN_PAREN() != null) ?? false;
        if (isCall)
        {
            var atom = context.atom();
            if (atom.name() != null)
            {
                var nameToken = atom.name().GetChild(0) as  ITerminalNode;
                if (nameToken != null)
                    CountOperator(nameToken);
            }
        }

        if (context.AWAIT()  != null)
            CountOperator(context.AWAIT());
        return base.VisitAtom_expr(context);
    }

    public override object VisitAtom(Python3Parser.AtomContext context)
    {
        if (context.name() != null)
        {
            if (!IsFunctionName(context))
            {
                var nameToken = context.name().GetChild(0) as  ITerminalNode;
                if (nameToken != null)
                    CountOperand(nameToken);
            }
        }
        else if (context.NUMBER() != null)
        {
            CountOperand(context.NUMBER());
        }
        else if (context.STRING() != null)
        {
            foreach (var str in context.STRING())
            {
                CountOperand(str);
            }
        }
        // Литералы
        else if (context.ELLIPSIS() != null)
        {
            CountOperand(context.ELLIPSIS());
        }
        return base.VisitAtom(context);
    }

    public override object VisitLiteral_pattern(Python3Parser.Literal_patternContext context)
    {
        if (context.signed_number() != null)
        {
            foreach (var child in context.signed_number().children)
            {
                if (child is ITerminalNode token)
                {
                    CountOperand(token);
                }
            }
        }
        else if (context.strings() != null)
        {
            foreach (var token in context.strings().STRING())
            {
                CountOperand(token);
            }
        }
        else if (context.NONE()  != null)
        {
            CountOperand(context.NONE());
        }
        return base.VisitLiteral_pattern(context);
    }

    public override object VisitTerminal(ITerminalNode node)
    {
        int tokenType = node.Symbol.Type;
        if (tokenType == Python3Parser.OPEN_PAREN 
            || tokenType == Python3Parser.OPEN_BRACK
            || tokenType == Python3Parser.OPEN_BRACE
            || tokenType == Python3Parser.COLON)
        {
            CountOperator(node);
        }

        if (tokenType == Python3Parser.CLOSE_PAREN
            || tokenType == Python3Parser.CLOSE_BRACK
            || tokenType == Python3Parser.CLOSE_BRACE)
        {
            CountOperatorAddToken(node);
        }

        if (tokenType == Python3Parser.TRUE || tokenType == Python3Parser.FALSE || tokenType == Python3Parser.NONE)
        {
            CountOperand(node);
        }

        if (tokenType == Python3Parser.DOT || tokenType == Python3Parser.COMMA || tokenType == Python3Parser.SEMI_COLON)
        {
            CountOperator(node);
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
                foreach (var child in compOp.children)
                {
                    if (child is ITerminalNode token)
                        CountOperator(token);
                }
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
                CountOperator(token);
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
                CountOperator(token);
            }
        }
        return base.VisitOr_test(context);
    }

    public override object VisitNot_test(Python3Parser.Not_testContext context)
    {
        if (context.NOT() != null)
        {
            CountOperator(context.NOT());
        }
        return base.VisitNot_test(context);
    }

    public override object VisitIf_stmt(Python3Parser.If_stmtContext context)
    {
        if (context.IF() != null)
            CountOperator(context.IF());
        if (context.ELIF() != null)
        {
            foreach (var token in context.ELIF())
            {
                CountOperatorAddToken(token);
            }
        }
        if (context.ELSE() != null)
            CountOperatorAddToken(context.ELSE());
        return base.VisitIf_stmt(context);
    }

    public override object VisitFor_stmt(Python3Parser.For_stmtContext context)
    {
        if (context.FOR() != null)
            CountOperator(context.FOR());
        if (context.IN() != null)
        {
            CountOperator(context.IN());
        }
        return base.VisitFor_stmt(context);
    }

    public override object VisitWhile_stmt(Python3Parser.While_stmtContext context)
    {
        if (context.WHILE() != null)
            CountOperator(context.WHILE());
        return base.VisitWhile_stmt(context);
    }

    public override object VisitBreak_stmt(Python3Parser.Break_stmtContext context)
    {
        if (context.BREAK() != null)
            CountOperator(context.BREAK());
        return base.VisitBreak_stmt(context);
    }

    public override object VisitContinue_stmt(Python3Parser.Continue_stmtContext context)
    {
        if (context.CONTINUE() != null)
            CountOperator(context.CONTINUE());
        return base.VisitContinue_stmt(context);
    }

    public override object VisitReturn_stmt(Python3Parser.Return_stmtContext context)
    {
        if (context.RETURN() != null)
            CountOperator(context.RETURN());
        return base.VisitReturn_stmt(context);
    }

    public override object VisitYield_expr(Python3Parser.Yield_exprContext context)
    {
        if (context.YIELD() != null)
            CountOperator(context.YIELD());
        return base.VisitYield_expr(context);
    }

    public override object VisitRaise_stmt(Python3Parser.Raise_stmtContext context)
    {
        if (context.RAISE() != null)
            CountOperator(context.RAISE());
        return base.VisitRaise_stmt(context);
    }
    public override object VisitImport_name(Python3Parser.Import_nameContext context)
    {
        if (context.IMPORT() != null)
            CountOperator(context.IMPORT());
        return base.VisitImport_name(context);
    }

    public override object VisitImport_from(Python3Parser.Import_fromContext context)
    {
        if (context.FROM() != null)
            CountOperator(context.FROM());
        return base.VisitImport_from(context);
    }

    public override object VisitTry_stmt(Python3Parser.Try_stmtContext context)
    {
        if (context.TRY() != null)
            CountOperator(context.TRY());
        if (context.FINALLY()  != null)
            CountOperatorAddToken(context.FINALLY());
        if (context.ELSE()   != null)
            CountOperatorAddToken(context.ELSE());
        return base.VisitTry_stmt(context);
    }

    public override object VisitExcept_clause(Python3Parser.Except_clauseContext context)
    {
        if (context.EXCEPT() != null)
            CountOperator(context.EXCEPT());
        if (context.AS() != null)
            CountOperator(context.AS());
        return base.VisitExcept_clause(context);
    }

    public override object VisitImport_as_name(Python3Parser.Import_as_nameContext context)
    {
        if (context.AS() != null)
            CountOperator(context.AS());
        return base.VisitImport_as_name(context);
    }
    
    public override object VisitWith_stmt(Python3Parser.With_stmtContext context)
    {
        if (context.WITH() != null)
            CountOperator(context.WITH());
        return base.VisitWith_stmt(context);
    }

    public override object VisitPass_stmt(Python3Parser.Pass_stmtContext context)
    {
        if (context.PASS() != null)
            CountOperator(context.PASS());
        return base.VisitPass_stmt(context);
    }

    public override object VisitFuncdef(Python3Parser.FuncdefContext context)
    {
        if (context.DEF() != null)
            CountOperator(context.DEF());
        if (context.name() != null)
        {
            var nameToken = context.name().GetChild(0) as ITerminalNode;
            if (nameToken != null)
            {
                CountOperator(nameToken);
            }
        }
        return base.VisitFuncdef(context);
    }

    public override object VisitClassdef(Python3Parser.ClassdefContext context)
    {
        if (context.CLASS() != null)
            CountOperator(context.CLASS());
        if (context.name() != null)
        {
            var nameToken = context.name().GetChild(0) as ITerminalNode;
            if (nameToken != null)
                CountOperand(nameToken);
        }
        return base.VisitClassdef(context);
    }

    public override object VisitLambdef(Python3Parser.LambdefContext context)
    {
        if (context.LAMBDA() != null)
            CountOperator(context.LAMBDA());
        return base.VisitLambdef(context);
    }

    public override object VisitAsync_stmt(Python3Parser.Async_stmtContext context)
    {
        if (context.ASYNC() != null)
            CountOperator(context.ASYNC());
        return base.VisitAsync_stmt(context);
    }

    public override object VisitDel_stmt(Python3Parser.Del_stmtContext context)
    {
        if (context.DEL() != null)
            CountOperator(context.DEL());
        return base.VisitDel_stmt(context);
    }

    public override object VisitGlobal_stmt(Python3Parser.Global_stmtContext context)
    {
        if (context.GLOBAL() != null)
            CountOperator(context.GLOBAL());
        return base.VisitGlobal_stmt(context);
    }

    public override object VisitNonlocal_stmt(Python3Parser.Nonlocal_stmtContext context)
    {
        if (context.NONLOCAL() != null)
            CountOperator(context.NONLOCAL());
        return base.VisitNonlocal_stmt(context);
    }

    public override object VisitAssert_stmt(Python3Parser.Assert_stmtContext context)
    {
        if (context.ASSERT()  != null)
            CountOperator(context.ASSERT());
        return base.VisitAssert_stmt(context);
    }

    public override object VisitMatch_stmt(Python3Parser.Match_stmtContext context)
    {
        if (context.MATCH() != null)
            CountOperator(context.MATCH());
        return base.VisitMatch_stmt(context);
    }
    
    public override object VisitCase_block(Python3Parser.Case_blockContext context)
    {
        if (context.CASE() != null)
            CountOperatorAddToken(context.CASE());
        return base.VisitCase_block(context);
    }
    
    public override object VisitVfpdef(Python3Parser.VfpdefContext context)
    {
        if (context.name() != null)
        {
            var nameToken = context.name().GetChild(0) as ITerminalNode;
            if (nameToken != null)
                CountOperand(nameToken);
        }
        return base.VisitVfpdef(context);
    }

    public override object VisitTfpdef(Python3Parser.TfpdefContext context)
    {
        if (context.name() != null)
        {
            var nameToken = context.name().GetChild(0) as ITerminalNode;
            if (nameToken != null)
                CountOperand(nameToken);
        }
        return base.VisitTfpdef(context);
    }
}