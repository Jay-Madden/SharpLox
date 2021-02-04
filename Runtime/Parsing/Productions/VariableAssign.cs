using System;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record VariableAssign(Token Identifier, Expression Expression): Expression
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitVariableAssign(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            indent = ShowIndent(indent, last);
            Console.WriteLine($"{GetType().Name}: {Identifier.Lexeme}");
            Expression.PrintNode(indent, false);
        }
    }
}