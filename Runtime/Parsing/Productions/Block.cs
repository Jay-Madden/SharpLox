using System.Collections.Generic;

namespace Runtime.Parsing.Productions
{
    public record Block(List<Node> Statements): Statement
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitBlock(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            throw new System.NotImplementedException();
        }
    }
}