using System;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record Ternary(Expression? Condition, Expression? TrueCase, Expression? FalseCase) : Expression
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitConditional(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            indent = ShowIndent(indent, last);
            Console.WriteLine($"{GetType().Name}");
            Condition.PrintNode(indent, false);
            TrueCase.PrintNode(indent, true);
            FalseCase.PrintNode(indent, true);
        }
    }
}