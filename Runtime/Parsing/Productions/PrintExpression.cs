using System;

namespace Runtime.Parsing.Productions
{
    public record PrintStatement(Expression Expression): Statement 
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitPrintStatement(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            var newIndent = ShowIndent(indent, last);
            Console.WriteLine(GetType().Name);
            Expression.PrintNode(newIndent, false);
        }
    }
}