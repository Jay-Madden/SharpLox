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
        public T VisitExpressionStatement(ExpressionStatement expressionStatement);
        public T VisitVariableStatement(VariableStatement variableStatement);
        public T VisitVariableAccess(VariableAccess variableAccess);
        public T VisitPropertySet(PropertySet set);
        public T VisitVariableAssign(VariableAssign variableAssign);
        public T VisitBlock(Block block);
        public T VisitIfStatement(IfStatement ifStatement);
        public T VisitLogical(Logical logical);
        public T VisitWhile(WhileStatement whileStatement);
        public T VisitBreakStatement(BreakStatement breakStatement);
        public T VisitCall(Call call);
        public T VisitPropertyGet(PropertyGet get);
        public T VisitLambda(Lambda lambda);
        public T VisitClassDeclaration(ClassDeclaration classDeclaration);
        public T VisitFuncDeclaration(FuncDeclaration funcDeclaration);
        public T VisitReturnStatement(ReturnStatement returnStatement);
    }
}