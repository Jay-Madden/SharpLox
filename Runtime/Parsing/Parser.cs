using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Runtime.Lexing;
using Runtime.Parsing.Productions;

namespace Runtime.Parsing
{
    public class Parser
    {
        private readonly List<Token> _tokens;

        private int _current;

        private bool _isAtEnd => Peek().Type == TokenType.Eof;

        private readonly Action<Token, string> _errorCallBack;

        public Parser(IEnumerable<Token> tokens, Action<Token, string> errCallback)
        {
            _tokens = tokens.ToList();
            _errorCallBack = errCallback;
        }

        public IEnumerable<Statement> Parse()
        {
            var stmts = new List<Statement>();
            while (!_isAtEnd)
            {
                var parsed = ParseStatement();
                if (parsed is not null)
                {
                    stmts.Add(parsed); 
                }
            }
            return stmts;
        }

        private Statement? ParseStatement()
        {
            if (Match(TokenType.Print))
            {
                return ParsePrintStatement();
            }
            else
            {
                return ParseExpressionStatement();
            }
        }

        private PrintStatement ParsePrintStatement()
        {
            var expression = ParseExpression();
            Consume(TokenType.Semicolon, "Expected ';' after value");
            return new PrintStatement(expression);
        }

        private ExpressionStatement ParseExpressionStatement()
        {
            var expression = ParseExpression();
            Consume(TokenType.Semicolon, "Expected ';' after expression");
            return new ExpressionStatement(expression);
        }

        private Expression? ParseExpression()
        {
            return ParseConditional();
        }

        private Expression? ParseConditional()
        {
            var expr = ParseEquality();
            while(Match(TokenType.Question))
            {
                var trueCase = ParseConditional();
                if (Match(TokenType.Colon))
                {
                    var falseCase = ParseConditional();
                    expr = new Ternary(expr, trueCase, falseCase);
                }
                else
                {
                    _errorCallBack(Peek(), "Ternary operations are separated with a colon");
                }
            }
            return expr;
        }
        
        private Expression? ParseEquality()
        {
            var expr = ParseComparison(); 
            while (Match(TokenType.EqualCompare, TokenType.NotEqual)) {
                var op = Previous();
                var right = ParseComparison();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expression? ParseComparison()
        {
            var expr = ParseAdditive(); 
            while (Match(TokenType.Greater,
                    TokenType.GreaterEqual, 
                    TokenType.Less, 
                    TokenType.LessEqual)) 
            {
                var op = Previous();
                var right = ParseAdditive();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expression? ParseAdditive()
        {
            var expr = ParseMultiplicative(); 
            while (Match(TokenType.Minus, TokenType.Plus))
            {
                var op = Previous();
                var right = ParseMultiplicative();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expression? ParseMultiplicative()
        {
            var expr = ParseUnary(); 
            while (Match(TokenType.Star, TokenType.Slash)) 
            {
                var op = Previous();
                var right = ParseUnary();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expression? ParseUnary()
        {
            if (Match(TokenType.Not, TokenType.Minus)) 
            {
                var op = Previous();
                var right = ParseUnary();
                return new Unary(op, right);
            }
            return ParsePrimary();
            
        }

        private Expression? ParsePrimary()
        {
            if (Match(TokenType.False))
            {
                return new Literal(false);
            }
            if (Match(TokenType.True))
            {
                return new Literal(true);
            }
            if (Match(TokenType.Nil))
            {
                return new Literal(null!);
            }

            if(Match(TokenType.Number))
            {
                return new Literal(double.Parse((string)Previous().Literal));
            }
            if(Match(TokenType.String))
            {
                return new Literal(Previous().Literal);
            }

            if (Match(TokenType.LeftParen))
            {
                var expr = ParseExpression();
                Consume(TokenType.RightParen, "Expect ')' after expression.");
                return new Grouping(expr);
            }
            _errorCallBack(Peek(), "Expected Expression");

            return null;
        }
        
        private Token? Consume(TokenType type, String message) 
        {
            if (Check(type))
            {
                return Advance();
            }

            _errorCallBack(Peek(), message);
            return null;
        }

        private void Synchronize()
        {
            Advance();
            while (!_isAtEnd)
            {
                if (Previous().Type == TokenType.Semicolon)
                {
                    return;
                }
                
                switch (Peek().Type)
                {
                    case TokenType.Class:
                    case TokenType.Func:
                    case TokenType.Var:
                    case TokenType.For:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                }

                Advance();
            }
        }


        private bool Match(params TokenType[]  types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (_isAtEnd)
            {
                return false;
            }
            return Peek().Type == type;
        }
        
        private Token Advance() 
        {
            if (!_isAtEnd)
            {
                _current++;
            }
            return Previous();
        }

        private Token Peek()
            => _tokens[_current];
        
        private Token Previous()
            => _tokens[_current-1];


    }
}