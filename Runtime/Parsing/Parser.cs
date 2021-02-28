using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Lexing;
using Runtime.Parsing.Productions;

namespace Runtime.Parsing
{
    public class Parser
    {
        private readonly List<Token> _tokens;

        private int _current;

        private bool _isAtEnd => Peek().Type == TokenType.Eof;

        private readonly bool _isRepl;

        private readonly Action<Token, string> _errorCallBack;

        public Parser(IEnumerable<Token> tokens, Action<Token, string> errCallback, bool isRepl)
        {
            _isRepl = isRepl;
            _tokens = tokens.ToList();
            _errorCallBack = errCallback;
        }

        public IEnumerable<Node> Parse()
        {
            var stmts = new List<Node>();
            while (!_isAtEnd)
            {
                var parsed = ParseDeclaration();
                if (parsed is not null)
                {
                    stmts.Add(parsed);
                }
            }
            return stmts;
        }

        private Node? ParseDeclaration()
        {
            try
            {
                if (Match(TokenType.Var))
                {
                    return ParseVariableDeclaration();
                }
                else
                {
                    return ParseStatement();
                }
            }
            catch (ParseErrorException)
            {
                Synchronize();
                return null;
            }
        }

        private Node? ParseVariableDeclaration()
        {
            var identifier = Consume(TokenType.Identifier, "Identifier expected");

            if (identifier is null)
            {
                return null;
            }

            var expression = Match(TokenType.EqualAssign) ? ParseExpression() : null!;

            Consume(TokenType.Semicolon, "Expected Semicolon after declaration");
            
            return new VariableStatement(identifier, expression);
        }

        private Node ParseStatement()
        {
            if (Match(TokenType.Print))
            {
                return ParsePrintStatement();
            }

            if (Match(TokenType.For))
            {
                return ParseForStatement();
            }

            if (Match(TokenType.LeftBrace))
            {
                return ParseBlock();
            }

            if (Match(TokenType.While))
            {
                return ParseWhileStatement();
            }

            if (Match(TokenType.If))
            {
                return ParseIfStatement();
            }
            return ParseExpressionStatement();
        }

        private Node ParseForStatement()
        {
            Node? initializer;
            if (Match(TokenType.Semicolon))
            {
                initializer = null;
            }
            else if (Match(TokenType.Var))
            {
                initializer = ParseVariableDeclaration();
            }
            else
            {
                initializer = ParseExpressionStatement();
            }

            Expression? condition = null;
            if (!Check(TokenType.Semicolon))
            {
                condition = ParseExpression();
            }
            Consume(TokenType.Semicolon, "Expect ';' after loop condition.");
            
            Expression? increment = null;
            if (!Check(TokenType.RightParen)) {
                increment = ParseExpression();
            }
            
            var body = ParseStatement();

            if (increment is not null)
            {
                body = new Block(new[]
                {
                    body,
                    new ExpressionStatement(increment)
                });
            }

            condition ??= new Literal(true);
            body = new WhileStatement(condition, body);

            if (initializer is not null)
            {
                body = new Block(new[] {initializer, body});
            }

            return body;
        }

        private Statement ParseWhileStatement()
        {
            var condition = ParseExpression();
            
            if (!Check(TokenType.LeftBrace))
            {
                _errorCallBack(Peek(), "while loop bodies must be a block");
            }
            
            var body = ParseStatement();

            return new WhileStatement(condition, body);

        }
        private Statement ParseIfStatement()
        {
            var condition = ParseExpression();

            if (!Check(TokenType.LeftBrace))
            {
                _errorCallBack(Peek(), "if statement bodies must be a block");
            }
            
            var body = ParseStatement();

            Node? elseBlock;
            if (Match(TokenType.Else))
            {
                if (!Check(TokenType.LeftBrace))
                {
                    _errorCallBack(Peek(), "else statement bodies must be a block");
                }
                elseBlock = ParseStatement();
            }
            else
            {
                elseBlock = null;
            }

            return new IfStatement(condition, body, elseBlock);
        }

        private Block ParseBlock()
        {
            var stmts = new List<Node>();

            while (!Check(TokenType.RightBrace) && !_isAtEnd)
            {
                var decl = ParseDeclaration();
                if (decl is null)
                {
                    continue;
                }
                
                stmts.Add(decl);
            }

            Consume(TokenType.RightBrace, "Expected Closing brace");
            return new Block(stmts);
        }
        
        private PrintStatement ParsePrintStatement()
        {
            var expression = ParseExpression();
            Consume(TokenType.Semicolon, "Expected ';' after value");
            return new PrintStatement(expression);
        }
        
        private Node ParseExpressionStatement()
        {
            var expression = ParseExpression();

            if (_isRepl && !Check(TokenType.Semicolon))
            {
                return expression;
            }

            Consume(TokenType.Semicolon, "Expected ';' after expression");
            return new ExpressionStatement(expression);
        }

        private Expression ParseExpression()
        {
            return ParseVariableAssignment();
        }

        private Expression ParseVariableAssignment()
        {
            var expression = ParseConditional();

            if (Match(TokenType.EqualAssign))
            {
                var equals = Previous();
                var value = ParseVariableAssignment();

                if (expression is VariableAccess access)
                {
                    var name = access.Name;
                    return new VariableAssign(name, value);
                }

                _errorCallBack(equals, "Invalid Assignment Target");
            }

            return expression;
        }

        private Expression ParseConditional()
        {
            var expr = ParseLogicalOr();
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

        private Expression ParseLogicalOr()
        {
            var lhs = ParseLogicalAnd();

            while (Match(TokenType.Or))
            {
                var op = Previous();
                var rhs = ParseLogicalAnd();
                return new Logical(lhs, op, rhs);
            }

            return lhs;
        }
        
        private Expression ParseLogicalAnd()
        {
            var lhs = ParseEquality();

            while (Match(TokenType.And))
            {
                var op = Previous();
                var rhs = ParseEquality();
                return new Logical(lhs, op, rhs);
            }

            return lhs;
        }
        
        private Expression ParseEquality()
        {
            var expr = ParseComparison(); 
            while (Match(TokenType.EqualCompare, TokenType.NotEqual)) {
                var op = Previous();
                var right = ParseComparison();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expression ParseComparison()
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

        private Expression ParseAdditive()
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

        private Expression ParseMultiplicative()
        {
            var expr = ParseUnary(); 
            while (Match(TokenType.Star, TokenType.Slash, TokenType.Percent)) 
            {
                var op = Previous();
                var right = ParseUnary();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expression ParseUnary()
        {
            if (Match(TokenType.Not, TokenType.Minus)) 
            {
                var op = Previous();
                var right = ParseUnary();
                return new Unary(op, right);
            }
            return ParseVariableAccess();
        }

        private Expression ParseVariableAccess()
        {
            if (Match(TokenType.Identifier))
            {
                return new VariableAccess(Previous());
            }

            return ParsePrimary();
        }

        private Expression ParsePrimary()
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

            throw new ParseErrorException();
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