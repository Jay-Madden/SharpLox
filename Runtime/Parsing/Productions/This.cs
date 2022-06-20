using Runtime.Lexing;

namespace Runtime.Parsing.Productions;

public record This(Token Keyword) : Expression
{
    public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitThisExpression(this);
    }

    public override void PrintNode(string indent, bool last)
    {
        
    }
}