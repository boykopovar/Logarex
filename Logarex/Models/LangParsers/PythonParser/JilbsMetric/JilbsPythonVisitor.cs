using Antlr4.Runtime.Tree;
using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class JilbsPythonVisitor : PythonOperatorVisitor
{
    private readonly Dictionary<string, int> _branchingOperators = new();
    private new readonly Dictionary<string, int> _operators = new();
    private int _maxNesting = 0;
    private int _currentNesting = 0;

    public new IJilbsParsedInfo GetResult() =>
        new PythonJilbsParsedInfo(_operators, _branchingOperators, _maxNesting);

    private void CountBranchingOperator(ITerminalNode node)
    {
        var text = node.GetText();
        _branchingOperators[text] = _branchingOperators.GetValueOrDefault(text) + 1;
    }

    private void AddToOperators(string key, int count = 1) =>
        _operators[key] = _operators.GetValueOrDefault(key) + count;

    private void EnterNesting()
    {
        _currentNesting++;
        if (_currentNesting > _maxNesting)
            _maxNesting = _currentNesting;
    }

    private void ExitNesting() => _currentNesting--;

    // -----------------------------------------------------------------------
    // Простые операторы (присваивание, return, import, pass, break и т.д.)
    // -----------------------------------------------------------------------

    public override object VisitSimple_stmt(Python3Parser.Simple_stmtContext context)
    {
        // simple_stmt может содержать несколько операторов через ';' (a=1; b=2)
        int semiCount = context.children?
            .OfType<ITerminalNode>()
            .Count(t => t.Symbol.Type == Python3Parser.SEMI_COLON) ?? 0;
        AddToOperators(context.GetText(), semiCount + 1);
        return base.VisitSimple_stmt(context);
    }

    // -----------------------------------------------------------------------
    // Составные операторы — ветвящиеся (CL + CLI + _operators)
    // -----------------------------------------------------------------------

    public override object VisitIf_stmt(Python3Parser.If_stmtContext context)
    {
        AddToOperators("if");

        if (context.IF() != null)
            CountBranchingOperator(context.IF());

        if (context.ELIF() != null)
            foreach (var token in context.ELIF())
                CountBranchingOperator(token);

        EnterNesting();
        var result = base.VisitIf_stmt(context);
        ExitNesting();
        return result;
    }

    public override object VisitWhile_stmt(Python3Parser.While_stmtContext context)
    {
        AddToOperators("while");
        if (context.WHILE() != null)
            CountBranchingOperator(context.WHILE());

        EnterNesting();
        var result = base.VisitWhile_stmt(context);
        ExitNesting();
        return result;
    }

    public override object VisitFor_stmt(Python3Parser.For_stmtContext context)
    {
        AddToOperators("for");
        if (context.FOR() != null)
            CountBranchingOperator(context.FOR());

        EnterNesting();
        var result = base.VisitFor_stmt(context);
        ExitNesting();
        return result;
    }

    // Нестинг на уровне try_stmt, т.к. тело except — дочерний узел try_stmt
    public override object VisitTry_stmt(Python3Parser.Try_stmtContext context)
    {
        AddToOperators("try");
        EnterNesting();
        var result = base.VisitTry_stmt(context);
        ExitNesting();
        return result;
    }

    // except: только CL, нестинг уже учтён в VisitTry_stmt
    public override object VisitExcept_clause(Python3Parser.Except_clauseContext context)
    {
        if (context.EXCEPT() != null)
            CountBranchingOperator(context.EXCEPT());
        return base.VisitExcept_clause(context);
    }

    // match/case: n ветвей ≡ (n-1) IF-THEN-ELSE с вложенностью (n-2) по методичке
    public override object VisitMatch_stmt(Python3Parser.Match_stmtContext context)
    {
        AddToOperators("match");
        int n = context.case_block()?.Length ?? 0;

        if (n >= 2)
        {
            _branchingOperators["case"] = _branchingOperators.GetValueOrDefault("case") + (n - 1);

            int nestingBoost = n - 2;
            if (nestingBoost > 0)
            {
                _currentNesting += nestingBoost;
                if (_currentNesting > _maxNesting)
                    _maxNesting = _currentNesting;
            }

            var result = base.VisitMatch_stmt(context);

            if (nestingBoost > 0)
                _currentNesting -= nestingBoost;

            return result;
        }

        return base.VisitMatch_stmt(context);
    }

    // -----------------------------------------------------------------------
    // Остальные составные операторы — только _operators
    // -----------------------------------------------------------------------

    public override object VisitWith_stmt(Python3Parser.With_stmtContext context)
    {
        AddToOperators("with");
        return base.VisitWith_stmt(context);
    }

    public override object VisitFuncdef(Python3Parser.FuncdefContext context)
    {
        AddToOperators("def");
        return base.VisitFuncdef(context);
    }
}