using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using Runtime.Interpreting.Globals;
using Runtime.Lexing;
using Runtime.Parsing;
using Runtime.Parsing.Productions;
using Expression = Runtime.Parsing.Productions.Expression;

namespace Runtime.Interpreting
{
    public class Interpreter : ISyntaxTreeVisitor<object>
    {

        private readonly LoxEnvironment _loxGlobals = new();
        
        private readonly Dictionary<Expression, int> _locals = new(ReferenceEqualityComparer.Instance);

        private LoxEnvironment _loxEnvironment;

        public Interpreter()
        {
            _loxEnvironment = _loxGlobals;
            
            _loxGlobals.Define(NativeFunctions.Clock, new Clock());
            _loxGlobals.Define(NativeFunctions.Input, new Input());
            _loxGlobals.Define(NativeFunctions.Print, new Print());
        }
        
        private bool _isBreak;

        public void Interpret(IEnumerable<Node> statements)
        {
            foreach (var stmt in statements)
            {
                if (stmt is Expression expr)
                {
                    Console.WriteLine(Evaluate(expr));
                    return;
                }
                
                Evaluate(stmt);
            }
        }
        
        public void ExecuteBlock(IEnumerable<Node> statements, LoxEnvironment environment)
        {
            var prev = _loxEnvironment;

            try
            {
                _loxEnvironment = environment;
                foreach (var stmt in statements)
                {
                    Evaluate(stmt);
                    if (_isBreak)
                    {
                        return;
                    }
                }
            }
            finally
            {
                _loxEnvironment = prev;
            }
        }
        
        public object VisitBinary(Binary binary)
        {
            var left = Evaluate(binary.Left);
            var right = Evaluate(binary.Right);

            return (binary.Token.Type, CheckNumberOperand<double>(left, right)) switch
            {
                (TokenType.Greater, true) => (double) left > (double) right,
                (TokenType.GreaterEqual, true) => (double) left >= (double) right,
                (TokenType.Less, true) => (double) left < (double) right,
                (TokenType.LessEqual, true) => (double) left <= (double) right,
                (TokenType.EqualCompare, _) => IsEqual(left, right),
                (TokenType.NotEqual, _) => !IsEqual(left, right),
                (TokenType.Plus, _) => (left, right) switch
                    { 
                        (string l, string r) => l + r,
                        (double l, double r) => l + r,
                        _ => throw new RuntimeErrorException(binary.Token, "Operands must be two numbers or two strings.")
                    },
                (TokenType.Minus, true) => (double) left - (double) right,
                (TokenType.Star, true) => (double) left * (double) right,
                (TokenType.Slash, true) => (double) left / (double) right,
                (TokenType.Percent, true) => (double) left % (double) right,
                (_, false) => throw new RuntimeErrorException(binary.Token, "Operands must be numbers."),
                _ => null!
            };
        }

        public object VisitConditional(Ternary ternary)
            => IsTruthy(Evaluate(ternary.Condition)) 
                ? Evaluate(ternary.TrueCase)
                : Evaluate(ternary.FalseCase);

        public object VisitGrouping(Grouping grouping) 
            => Evaluate(grouping.Expression);

        public object VisitLiteral(Literal literal) 
            => literal.Value;

        public object VisitUnary(Unary unary)
        {
            var right = Evaluate(unary.Right);

            return (unary.Operator.Type, CheckNumberOperand<double>(right)) switch
            {
                (TokenType.Minus, false) => throw new RuntimeErrorException(unary.Operator, "Operand must be a number."),
                (TokenType.Minus, _) => -(double) right,
                (TokenType.Not, _) => IsTruthy(right),
                _ => null!
            };
        }

        public object VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            Evaluate(expressionStatement.Expression);
            return null!;
        }

        public object VisitVariableStatement(VariableStatement variableStatement)
        {
            object? value = null;
            if (variableStatement.Expression is not null)
            {
                value = Evaluate(variableStatement.Expression);
            }
            
            _loxEnvironment.Define(variableStatement.Identifier.Lexeme, value);
            return null!;
        }

        public object VisitVariableAccess(VariableAccess variableAccess)
        {
            return ResolveVariable(variableAccess.Name, variableAccess);
        }

        public object VisitVariableAssign(VariableAssign variableAssign)
        {
            var value = Evaluate(variableAssign.Expression);
            
            if (_locals.TryGetValue(variableAssign.Expression, out var dist))
            {  
                _loxEnvironment.AssignAt(dist, variableAssign.Identifier, value);
            }
            else
            {
                _loxGlobals.Assign(variableAssign.Identifier, value);
            }
            return value;
        }

        public object VisitBlock(Block block)
        {
            ExecuteBlock(block.Statements, new LoxEnvironment{Parent = _loxEnvironment});
            return null!;
        }

        public object VisitIfStatement(IfStatement ifStatement)
        {
            if (IsTruthy(Evaluate(ifStatement.Condition)))
            {
                Evaluate(ifStatement.IfCase);
            }
            else if(ifStatement.ElseCase is not null)
            {
                Evaluate(ifStatement.ElseCase);
            }
            return null!;
        }

        public object VisitLogical(Logical logical)
        {
            var lhs = Evaluate(logical.Left);

            if (logical.Op.Type == TokenType.Or)
            {
                if (IsTruthy(lhs))
                {
                    return lhs;
                }
            }
            else
            {
                if (!IsTruthy(lhs))
                {
                    return lhs;
                }
            }

            return Evaluate(logical.Right);
        }

        public object VisitWhile(WhileStatement whileStatement)
        {
            while (IsTruthy(Evaluate(whileStatement.Condition)))
            {
                Evaluate(whileStatement.Body);
                if (_isBreak)
                {
                    _isBreak = false;
                    break;
                }
            }

            return null!;
        }

        public object VisitBreakStatement(BreakStatement breakStatement)
        {
            _isBreak = true;
            return null!;
        }

        public object VisitCall(Call call)
        {
            var callee = Evaluate(call.callee);
            var args = call.args.Select(Evaluate);

            if (callee is not ICallable func)
            {
                throw new RuntimeErrorException(call.paren, "Only functions and classes are callable");
            }
            
            return func.Call(this, args);
        }

        public object VisitLambda(Lambda lambda)
        {
            return new SharpLoxCallable(lambda.Parameters, lambda.Body, _loxEnvironment);
        }

        public object VisitFuncDeclaration(FuncDeclaration funcDeclaration)
        {
            var func = new SharpLoxCallable(funcDeclaration.parameters, funcDeclaration.body, _loxEnvironment);
            _loxEnvironment.Define(funcDeclaration.name.Lexeme, func);
            return null!;
        }

        public object VisitReturnStatement(ReturnStatement returnStatement)
        {
            throw new ReturnValue(
                returnStatement.Value is not null 
                    ? Evaluate(returnStatement.Value)
                    : null!);
        }

        public void Resolve(Expression expression, int depth)
        {
            _locals.Add(expression, depth);
        }

        private object ResolveVariable(Token name, Expression expression)
        {
            if (_locals.TryGetValue(expression, out var val))
            {
                return _loxEnvironment.GetAt(val, name);
            }
            return _loxGlobals.Get(name);
        }

        private object Evaluate<T>(T value) where T: Node
            => value.Accept(this);

        private bool IsTruthy(object? obj)
            => obj is not null and (not bool or true);
        
       private bool IsEqual(object? a, object? b) 
            => a?.Equals(b) ?? b == null;
        
        private bool CheckNumberOperand<T>(params object[] operands) 
            => operands.All(x => x is T);
    }
}