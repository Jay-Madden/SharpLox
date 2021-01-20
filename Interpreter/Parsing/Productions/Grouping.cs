using System;

namespace Interpreter.Parsing.Productions
{
    public record Grouping(Expression Expression) : Expression
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitGrouping(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            var newIndent = ShowIndent(indent, last);
            Console.WriteLine(GetType().Name);
            Expression.PrintNode(newIndent, false);
        }
    }
}