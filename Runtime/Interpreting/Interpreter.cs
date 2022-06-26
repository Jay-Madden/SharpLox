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

        private readonly SharpLoxEnvironment _sharpLoxGlobals = new();
        
        private readonly Dictionary<Expression, int> _locals = new(ReferenceEqualityComparer.Instance);

        private SharpLoxEnvironment _sharpLoxEnvironment;

        public Interpreter()
        {
            _sharpLoxEnvironment = _sharpLoxGlobals;
            
            _sharpLoxGlobals.Define(NativeFunctions.Clock, new Clock());
            _sharpLoxGlobals.Define(NativeFunctions.Input, new Input());
            _sharpLoxGlobals.Define(NativeFunctions.Print, new Print());
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
        
        public void ExecuteBlock(IEnumerable<Node> statements, SharpLoxEnvironment environment)
        {
            var prev = _sharpLoxEnvironment;

            try
            {
                _sharpLoxEnvironment = environment;
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
                _sharpLoxEnvironment = prev;
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

        public object VisitThisExpression(This @this)
        {
            return ResolveVariable(@this.Keyword, @this);
        }

        public object VisitVariableStatement(VariableStatement variableStatement)
        {
            object? value = null;
            if (variableStatement.Expression is not null)
            {
                value = Evaluate(variableStatement.Expression);
            }
            
            _sharpLoxEnvironment.Define(variableStatement.Identifier.Lexeme, value);
            return null!;
        }

        public object VisitVariableAccess(VariableAccess variableAccess)
        {
            return ResolveVariable(variableAccess.Name, variableAccess);
        }

        public object VisitPropertySet(PropertySet set)
        {
            var name = Evaluate(set.Name);

            if (name is not SharpLoxInstance instance)
            {
                throw new RuntimeErrorException(set.Identifier, "Only instances can have fields");
            }
            
            var val = Evaluate(set.Value);
            
            instance.Set(set.Identifier, val);
            return val;
        }

        public object VisitSuperExpression(Super super)
        {
            var distance = _locals[super];

            var superClass = (SharpLoxClass)_sharpLoxEnvironment.GetAt(distance,
                new Token(TokenType.Identifier, "super", null!, 0));

            var instance = (SharpLoxInstance)_sharpLoxEnvironment.GetAt(distance - 1,
                new Token(TokenType.Identifier, "this", null!, 0));

            var method = superClass.GetMethod(super.method);

            if (method is null)
            {
                throw new RuntimeErrorException(super.keyword, "Method does not exist on super class");
            }
            
            return method.Bind(instance);
        }

        public object VisitVariableAssign(VariableAssign variableAssign)
        {
            var value = Evaluate(variableAssign.Expression);
            
            if (_locals.TryGetValue(variableAssign, out var dist))
            {  
                _sharpLoxEnvironment.AssignAt(dist, variableAssign.Identifier, value);
            }
            else
            {
                _sharpLoxGlobals.Assign(variableAssign.Identifier, value);
            }
            return value;
        }

        public object VisitBlock(Block block)
        {
            ExecuteBlock(block.Statements, new SharpLoxEnvironment{Parent = _sharpLoxEnvironment});
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

        public object VisitPropertyGet(PropertyGet get)
        {
            var val = Evaluate(get.Expression);

            if (val is not SharpLoxInstance instance)
            {
                throw new RuntimeErrorException(get.Identifier, "Only instances can have properties");
            }

            return instance.Get(get.Identifier);
        }

        public object VisitLambda(Lambda lambda)
        {
            return new SharpLoxCallable(lambda.Parameters, lambda.Body,
                new SharpLoxEnvironment{Parent = _sharpLoxEnvironment}, false);
        }

        public object VisitClassDeclaration(ClassDeclaration classDeclaration)
        {
            _sharpLoxEnvironment.Define(classDeclaration.Name.Lexeme);

            object? super = null;
            if (classDeclaration.SuperClass is not null)
            {
                super = Evaluate(classDeclaration.SuperClass);
                if (super is not SharpLoxClass)
                {
                    throw new RuntimeErrorException(classDeclaration.SuperClass.Name, "Inherited object must be a class");
                }

                _sharpLoxEnvironment = new SharpLoxEnvironment {Parent = _sharpLoxEnvironment};
                _sharpLoxEnvironment.Define("super", super);
            }
            
            var methods = classDeclaration.Methods
                .Cast<FuncDeclaration>()
                .ToDictionary(
                    x => x.Name.Lexeme,
                    y => new SharpLoxCallable(y.Parameters, y.Body, _sharpLoxEnvironment, y.Name.Lexeme == "init")
                );
            
            var c = new SharpLoxClass(classDeclaration.Name.Lexeme, (SharpLoxClass)super!, methods);
            
            
            if (classDeclaration.SuperClass is not null)
            {
                _sharpLoxEnvironment = _sharpLoxEnvironment.Parent!;
            }
            
            _sharpLoxEnvironment.Assign(classDeclaration.Name, c);
            return null!;
        }

        public object VisitFuncDeclaration(FuncDeclaration funcDeclaration)
        {
            var func = new SharpLoxCallable(funcDeclaration.Parameters, funcDeclaration.Body,
                new SharpLoxEnvironment {Parent = _sharpLoxEnvironment}, false);
            _sharpLoxEnvironment.Define(funcDeclaration.Name.Lexeme, func);
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
                return _sharpLoxEnvironment.GetAt(val, name);
            }
            return _sharpLoxGlobals.Get(name);
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