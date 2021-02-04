using System;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record VariableAccess(Token Name): Expression
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitVariableAccess(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            Console.WriteLine($"{GetType().Name}: {Name.Lexeme}");
        }
    }
}