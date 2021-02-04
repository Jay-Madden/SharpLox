namespace Runtime.Parsing.Productions
{
    public record WhileStatement(Expression Condition, Node Body): Statement
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitWhile(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            throw new System.NotImplementedException();
        }
    }
}