using System;
using Runtime.Parsing.Productions;

namespace Runtime.Parsing
{
    public class PrettyPrinter//: ISyntaxTreeVisitor<string>
    {
        private string _output = string.Empty;
        
        /*
        public string Print<T>(T node) where T: Expression
        {
            _output = string.Empty;
            
            return node.Accept(this);
        }
        */
        
        public string VisitBinary(Binary binary)
        {
            return $"(Binary {binary.Left} {binary.Right})";
        }

        public string VisitConditional(Ternary ternary)
        {
            return $"(Conditional {ternary.Condition} {ternary.TrueCase} {ternary.FalseCase}";
        }

        public string VisitGrouping(Grouping grouping)
        {
            return $"(Grouping {grouping.Expression})";
        }

        public string VisitLiteral(Literal literal)
        {
            return literal.Value?.ToString() ?? string.Empty;
        }

        public string VisitUnary(Unary unary)
        {
            return $"(unary {unary.Right})";
        }

        public string VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            throw new NotImplementedException();
        }

        public string VisitPrintStatement(PrintStatement printStatement)
        {
            throw new NotImplementedException();
        }

        public string VisitVariableStatement(VariableStatement variableStatement)
        {
            throw new NotImplementedException();
        }

        public string VisitVariableAccess(VariableAccess variableAccess)
        {
            throw new NotImplementedException();
        }

        public string VisitVariableAssign(VariableAssign variableAssign)
        {
            throw new NotImplementedException();
        }
    }
}