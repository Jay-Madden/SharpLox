using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Runtime.Interpreting;
using Runtime.Lexing;
using Runtime.Parsing;
using Runtime.Parsing.Productions;
using Expression = Runtime.Parsing.Productions.Expression;

namespace Runtime.SemanticAnalysis
{
    public class NameResolver: ISyntaxTreeVisitor<object?>
    {
        private readonly Interpreter _interpreter;

        private readonly Stack<Dictionary<string, bool>> _scopes = new();
        
        private readonly Action<Token, string> _errorCallBack;

        private FunctionType _currentFunction = FunctionType.None;
        
        public NameResolver(Interpreter interpreter, Action<Token, string> errorCallBack )
        {
            _interpreter = interpreter;
            _errorCallBack = errorCallBack;
        }

        public object? VisitBinary(Binary binary)
        {
            Resolve(binary.Left);
            Resolve(binary.Right);
            return null;
        }

        public object? VisitConditional(Ternary ternary)
        {
            Resolve(ternary.Condition);
            Resolve(ternary.TrueCase);
            Resolve(ternary.FalseCase);
            return null;
        }

        public object? VisitGrouping(Grouping grouping)
        {
            Resolve(grouping.Expression);
            return null;
        }

        public object? VisitLiteral(Literal literal)
        {
            return null;
        }

        public object? VisitUnary(Unary unary)
        {
            Resolve(unary.Right);
            return null;
        }

        public object? VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            Resolve(expressionStatement.Expression);
            return null;
        }
        
        public object? VisitVariableStatement(VariableStatement variableStatement)
        {
            Declare(variableStatement.Identifier);
            if (variableStatement.Expression is not null)
            {
                Resolve(variableStatement.Expression);
            }

            Define(variableStatement.Identifier);
            return null;
        }

        public object? VisitVariableAccess(VariableAccess variableAccess)
        {
            if (_scopes.Count > 0 && 
                _scopes.Peek().TryGetValue(variableAccess.Name.Lexeme, out var val) && 
                val == false)
            {
                _errorCallBack(variableAccess.Name, "Can't read local variable in its own initializer.");
            }
            ResolveLocal(variableAccess.Name, variableAccess);
            return null;
        }

        public object? VisitVariableAssign(VariableAssign variableAssign)
        {
            Resolve(variableAssign.Expression);
            ResolveLocal(variableAssign.Identifier, variableAssign);
            return null;
        }

        public object? VisitBlock(Block block)
        {
            BeginScope();
            Resolve(block.Statements);
            EndScope();
            return null;        
        }

        public object? VisitIfStatement(IfStatement ifStatement)
        {
            Resolve(ifStatement.Condition);
            Resolve(ifStatement.IfCase);
            if (ifStatement.ElseCase is not null)
            {
                Resolve(ifStatement.ElseCase);
            }
            return null;        
        }

        public object? VisitLogical(Logical logical)
        {
            Resolve(logical.Left);
            Resolve(logical.Right);
            return null;
        }

        public object? VisitWhile(WhileStatement whileStatement)
        {
            Resolve(whileStatement.Condition);
            Resolve(whileStatement.Body);
            return null;
        }

        public object? VisitBreakStatement(BreakStatement breakStatement)
        {
            return null;
        }

        public object? VisitCall(Call call)
        {
            Resolve(call.callee);
            Resolve(call.args);
            return null;
        }

        public object? VisitLambda(Lambda lambda)
        {
            ResolveLambda(lambda, FunctionType.Function);
            return null;
        }

        public object? VisitFuncDeclaration(FuncDeclaration funcDeclaration)
        {
            Declare(funcDeclaration.name);
            Define(funcDeclaration.name);

            ResolveFunction(funcDeclaration, FunctionType.Function);
            return null;
        }

        public object? VisitReturnStatement(ReturnStatement returnStatement)
        {
            if (_currentFunction == FunctionType.None) {
                _errorCallBack(returnStatement.Keyword, "Can't return from top-level code.");
            }
            
            if (returnStatement.Value is not null)
            {
                Resolve(returnStatement.Value);
            }

            return null;
        }

        private void Declare(Token name)
        {
            if (_scopes.Count == 0)
            {
                return;
            }

            var scope = _scopes.Peek();
            scope.Add(name.Lexeme, false);
        }

        private void Define(Token name)
        {
            if (_scopes.Count == 0)
            {
                return;
            }
            
            _scopes.Peek()[name.Lexeme] = true;
        }

        private void ResolveLocal(Token name, Expression expression)
        {
            var scopesList = _scopes.ToList();
            for (var i = 0; i < scopesList.Count; i++) {
                if (scopesList[i].ContainsKey(name.Lexeme)) {
                    _interpreter.Resolve(expression, i);
                    return;
                }
            }
        }

        private void ResolveLambda(Lambda lambda, FunctionType type)
        {
            ResolveCallable(lambda.Parameters, lambda.Body, type);
        }
        
        private void ResolveFunction(FuncDeclaration func, FunctionType type)
        {
            ResolveCallable(func.parameters, func.body, type);
        }

        private void ResolveCallable(IEnumerable<Token> tokens, Block body, FunctionType type)
        {
            var enclosing = _currentFunction;
            _currentFunction = type;
            
            BeginScope();
            foreach (var token in tokens)
            {
               Declare(token); 
               Define(token);
            }
            Resolve(body);
            
            _currentFunction = enclosing;
            
            EndScope();

        }

        public void Resolve(IEnumerable<Node> statements) {
            foreach (var statement in statements) 
            {
                Resolve(statement);
            }
        }
        
        private void Resolve(Node node) 
            => node.Accept(this);

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            _scopes.Pop();
        }
    }
}