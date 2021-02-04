using System;

namespace Runtime.Parsing.Productions
{
    public record IfStatement(Expression Condition, Node IfCase, Node? ElseCase): Statement
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            indent = ShowIndent(indent, last);
            Console.WriteLine($"{GetType().Name}");
            Condition.PrintNode(indent, false);
            IfCase.PrintNode(indent, true);
            ElseCase?.PrintNode(indent, true);
        }
    }
}