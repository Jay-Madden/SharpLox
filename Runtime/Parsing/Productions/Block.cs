using System.Collections.Generic;

namespace Runtime.Parsing.Productions
{
    public record Block(IEnumerable<Node> Statements): Statement
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitBlock(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            var newIndent = ShowIndent(indent, last);
            foreach (var statement in Statements)
            {
               statement.PrintNode(newIndent, false); 
            }
        }
    }
}