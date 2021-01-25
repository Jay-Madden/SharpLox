using System.Collections.Generic;
using Runtime.Parsing.Productions;

namespace Runtime.Parsing
{
    public interface ISyntaxTreeVisitor<out T>
    {
        public T VisitBinary(Binary binary);
        public T VisitConditional(Ternary ternary);
        public T VisitGrouping(Grouping grouping);
        public T VisitLiteral(Literal literal);
        public T VisitUnary(Unary unary);
    }
}