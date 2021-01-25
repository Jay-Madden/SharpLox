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

        public Expression? Parse()
        {
            try 
            {
                return Expression();
            } 
            catch (ParseErrorException error) 
            {
                return null;
            }
        }

        private Expression Expression()
        {
            return Conditional();
        }

        private Expression Conditional()
        {
            var expr = Equality();
            while(Match(TokenType.Question))
            {
                var trueCase = Conditional();
                if (Match(TokenType.Colon))
                {
                    var falseCase = Conditional();
                    expr = new Ternary(expr, trueCase, falseCase);
                }
                else
                {
                    _errorCallBack(Peek(), "Ternary operations are separated with a colon");
                }
            }
            return expr;
        }
        
        private Expression Equality()
        {
            var expr = Comparison(); 
            while (Match(TokenType.EqualCompare, TokenType.NotEqual)) {
                var op = Previous();
                var right = Comparison();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expression Comparison()
        {
            var expr = Additive(); 
            while (Match(TokenType.Greater,
                    TokenType.GreaterEqual, 
                    TokenType.Less, 
                    TokenType.LessEqual)) 
            {
                var op = Previous();
                var right = Additive();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expression Additive()
        {
            var expr = Multiplicative(); 
            while (Match(TokenType.Minus, TokenType.Plus))
            {
                var op = Previous();
                var right = Multiplicative();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expression Multiplicative()
        {
            var expr = Unary(); 
            while (Match(TokenType.Star, TokenType.Slash)) 
            {
                var op = Previous();
                var right = Unary();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expression Unary()
        {
            if (Match(TokenType.Not, TokenType.Minus)) 
            {
                var op = Previous();
                var right = Unary();
                return new Unary(op, right);
            }
            return Primary();
            
        }

        private Expression Primary()
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
                Expression expr = Expression();
                Consume(TokenType.RightParen, "Expect ')' after expression.");
                return new Grouping(expr);
            }
            _errorCallBack(Peek(), "Expected Expression");


            throw new ParseErrorException();
        }
        
        private Token Consume(TokenType type, String message) {
            if (Check(type))
            {
                return Advance();
            }

            _errorCallBack(Peek(), message);
            throw new ParseErrorException();
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