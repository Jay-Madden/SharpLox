using System;

namespace Runtime.Parsing.Productions
{
    public record BreakStatement: Statement 
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitBreakStatement(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            Console.WriteLine($"{GetType().Name}");
        }
    }
}