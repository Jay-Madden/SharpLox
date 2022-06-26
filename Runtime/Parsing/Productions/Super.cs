using Runtime.Lexing;

namespace Runtime.Parsing.Productions;

public record Super(Token keyword, Token method) : Expression
{
    public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitSuperExpression(this);
    }

    public override void PrintNode(string indent, bool last)
    {
        
    }
}