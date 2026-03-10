using Antlr4.Runtime.Tree;
using Logarex.Models.LangParsers.Contracts;

namespace Logarex.Models.LangParsers.PythonParser;

public class HalsteadPythonVisitor : PythonOperatorVisitor
{
    private Dictionary<string, int> _operands =  new();
    public new List<TokenInfo> GetTokes() => _tokens;
    public new IHalsteadParsedInfo GetResult()
    {
        return new PythonHalsteadParsedInfo(_operators, _operands);
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
    private void CountOperand(ITerminalNode node) =>  Count(node, false);

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
        if (tokenType == Python3Parser.TRUE || tokenType == Python3Parser.FALSE || tokenType == Python3Parser.NONE)
        {
            CountOperand(node);
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