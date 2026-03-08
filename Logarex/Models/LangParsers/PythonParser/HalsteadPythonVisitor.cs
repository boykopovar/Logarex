using Antlr4.Runtime.Tree;
using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class HalsteadPythonVisitor : Python3ParserBaseVisitor<object>
{
    private Dictionary<string, int> _operators =  new();
    private Dictionary<string, int> _operands =  new();
    private List<TokenInfo> _tokens = new();

    public List<TokenInfo> GetTokes() => _tokens;
    public IParsedInfo GetResult()
    {
        return new PythonParsedInfo(_operators, _operands);
    }

    private void Count(ITerminalNode node, bool isOperator)
    {
        var text = node.GetText();
        var dict = isOperator ? _operators : _operands;
        dict[text] = dict.GetValueOrDefault(text) + 1;
        _tokens.Add(new TokenInfo
        {
            Text = text,
            IsOperator = isOperator,
            StartIndex = node.Symbol.StartIndex,
            Length = node.Symbol.StopIndex - node.Symbol.StartIndex + 1
        });
    }
    
    private void CountOperator(ITerminalNode node) => Count(node, true);
    private void CountOperand(ITerminalNode node) =>  Count(node, false);
    
    private void CountOperatorAddToken(ITerminalNode node)
    {
        var text = node.GetText();
        _tokens.Add(new TokenInfo
        {
            Text = text,
            IsOperator = true,
            StartIndex = node.Symbol.StartIndex,
            Length = node.Symbol.StopIndex - node.Symbol.StartIndex + 1
        });
    }
    
    private void TryOperator(ITerminalNode? node)
    {
        if (node != null) CountOperator(node);
    }

    private void TryListOperator(IEnumerable<ITerminalNode>? nodes)
    {
        if (nodes == null) return;
        foreach (var node in nodes)
        {
            CountOperator(node);
        }
    }
    private void TryOperAddToken(ITerminalNode? node)
    {
        if (node != null) CountOperatorAddToken(node);
    }

    // =
    public override object VisitExpr_stmt(Python3Parser.Expr_stmtContext context)
    {
        TryListOperator(context.ASSIGN());
        return base.VisitExpr_stmt(context);
    }

    // +=, -= и т.д.
    public override object VisitAugassign(Python3Parser.AugassignContext context)
    {
        var token = context.children.OfType<ITerminalNode>().FirstOrDefault();
        TryOperator(token);
        return base.VisitAugassign(context);
    }

    public override object VisitExpr(Python3Parser.ExprContext context)
    {
        TryOperator(context.STAR());
        TryOperator(context.IDIV());
        TryOperator(context.MOD());
        TryOperator(context.LEFT_SHIFT());
        TryOperator(context.RIGHT_SHIFT());
        TryListOperator(context.ADD());
        TryListOperator(context.MINUS());
        TryOperator(context.XOR());
        TryOperator(context.AND_OP());
        TryOperator(context.OR_OP());
        TryOperator(context.POWER());
        TryOperator(context.AT());
        return base.VisitExpr(context);
    }

    public override object VisitArgument(Python3Parser.ArgumentContext context)
    {
        TryOperator(context.STAR());
        TryOperator(context.POWER());
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
                TryOperator(nameToken);
            }
        }
        
        bool isCall = context.trailer()?.Any(t => t.OPEN_PAREN() != null) ?? false;
        if (isCall)
        {
            var atom = context.atom();
            if (atom.name() != null)
            {
                var nameToken = atom.name().GetChild(0) as  ITerminalNode;
                TryOperator(nameToken);
            }
        }
        TryOperator(context.AWAIT());
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
        TryListOperator(context.AND());
        return base.VisitAnd_test(context);
    }

    public override object VisitOr_test(Python3Parser.Or_testContext context)
    {
        TryListOperator(context.OR());
        return base.VisitOr_test(context);
    }

    public override object VisitNot_test(Python3Parser.Not_testContext context)
    {
        TryOperator(context.NOT());
        return base.VisitNot_test(context);
    }

    public override object VisitIf_stmt(Python3Parser.If_stmtContext context)
    {
        TryOperator(context.IF());
        if (context.ELIF() != null)
        {
            foreach (var token in context.ELIF())
            {
                CountOperatorAddToken(token);
            }
        }
        TryOperAddToken(context.ELSE());
        return base.VisitIf_stmt(context);
    }

    public override object VisitFor_stmt(Python3Parser.For_stmtContext context)
    {
        TryOperator(context.FOR());
        TryOperator(context.IN());
        return base.VisitFor_stmt(context);
    }

    public override object VisitWhile_stmt(Python3Parser.While_stmtContext context)
    {
        TryOperator(context.WHILE());
        return base.VisitWhile_stmt(context);
    }

    public override object VisitBreak_stmt(Python3Parser.Break_stmtContext context)
    {
        TryOperator(context.BREAK());
        return base.VisitBreak_stmt(context);
    }

    public override object VisitContinue_stmt(Python3Parser.Continue_stmtContext context)
    {
        TryOperator(context.CONTINUE());
        return base.VisitContinue_stmt(context);
    }

    public override object VisitReturn_stmt(Python3Parser.Return_stmtContext context)
    {
        TryOperator(context.RETURN());
        return base.VisitReturn_stmt(context);
    }

    public override object VisitYield_expr(Python3Parser.Yield_exprContext context)
    {
        TryOperator(context.YIELD());
        return base.VisitYield_expr(context);
    }

    public override object VisitRaise_stmt(Python3Parser.Raise_stmtContext context)
    {
        TryOperator(context.RAISE());
        return base.VisitRaise_stmt(context);
    }
    public override object VisitImport_name(Python3Parser.Import_nameContext context)
    {
        TryOperator(context.IMPORT());
        return base.VisitImport_name(context);
    }

    public override object VisitImport_from(Python3Parser.Import_fromContext context)
    {
        TryOperator(context.FROM());
        return base.VisitImport_from(context);
    }

    public override object VisitTry_stmt(Python3Parser.Try_stmtContext context)
    {
        TryOperator(context.TRY());
        TryOperAddToken(context.FINALLY());
        TryOperAddToken(context.ELSE());
        return base.VisitTry_stmt(context);
    }

    public override object VisitExcept_clause(Python3Parser.Except_clauseContext context)
    {
        TryOperator(context.EXCEPT());
        TryOperator(context.AS());
        return base.VisitExcept_clause(context);
    }

    public override object VisitImport_as_name(Python3Parser.Import_as_nameContext context)
    {
        TryOperator(context.AS());
        return base.VisitImport_as_name(context);
    }
    
    public override object VisitWith_stmt(Python3Parser.With_stmtContext context)
    {
        TryOperator(context.WITH());
        return base.VisitWith_stmt(context);
    }

    public override object VisitPass_stmt(Python3Parser.Pass_stmtContext context)
    {
        TryOperator(context.PASS());
        return base.VisitPass_stmt(context);
    }

    public override object VisitFuncdef(Python3Parser.FuncdefContext context)
    {
        TryOperator(context.DEF());
        if (context.name() != null)
        {
            var nameToken = context.name().GetChild(0) as ITerminalNode;
            TryOperator(nameToken);
        }
        return base.VisitFuncdef(context);
    }

    public override object VisitClassdef(Python3Parser.ClassdefContext context)
    {
        TryOperator(context.CLASS());
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
        TryOperator(context.LAMBDA());
        return base.VisitLambdef(context);
    }

    public override object VisitAsync_stmt(Python3Parser.Async_stmtContext context)
    {
        TryOperator(context.ASYNC());
        return base.VisitAsync_stmt(context);
    }

    public override object VisitDel_stmt(Python3Parser.Del_stmtContext context)
    {
        TryOperator(context.DEL());
        return base.VisitDel_stmt(context);
    }

    public override object VisitGlobal_stmt(Python3Parser.Global_stmtContext context)
    {
        TryOperator(context.GLOBAL());
        return base.VisitGlobal_stmt(context);
    }

    public override object VisitNonlocal_stmt(Python3Parser.Nonlocal_stmtContext context)
    {
        TryOperator(context.NONLOCAL());
        return base.VisitNonlocal_stmt(context);
    }

    public override object VisitAssert_stmt(Python3Parser.Assert_stmtContext context)
    {
        TryOperator(context.ASSERT());
        return base.VisitAssert_stmt(context);
    }

    public override object VisitMatch_stmt(Python3Parser.Match_stmtContext context)
    {
        TryOperator(context.MATCH());
        return base.VisitMatch_stmt(context);
    }
    
    public override object VisitCase_block(Python3Parser.Case_blockContext context)
    {
        TryOperAddToken(context.CASE());
        return base.VisitCase_block(context);
    }
    
    public override object VisitVfpdef(Python3Parser.VfpdefContext context)
    {
        CountName(context.name());
        return base.VisitVfpdef(context);
    }

    public override object VisitTfpdef(Python3Parser.TfpdefContext context)
    {
        CountName(context.name());
        return base.VisitTfpdef(context);
    }

    private void CountName(Python3Parser.NameContext? name)
    {
        if (name != null)
        {
            var nameToken = name.GetChild(0) as ITerminalNode;
            if (nameToken != null)
                CountOperand(nameToken);
        }
    }
}