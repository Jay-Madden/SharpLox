using System;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record ReturnStatement(Token Keyword, Expression? Value) : Statement
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitReturnStatement(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            indent = ShowIndent(indent, last);
            Console.WriteLine($"{GetType().Name}");
            Value?.PrintNode(indent, false);
        }
    }
}