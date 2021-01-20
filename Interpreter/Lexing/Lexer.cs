using System;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Lexing
{
    public class Lexer
    {
        private int _start;
        private int _current;
        private int _line = 1;

        private readonly Action<int, string> _errorHandler;
        
        private readonly string _source;

        private readonly List<Token> _tokens = new();

        private string _currentLexeme => _source[_start.._current];
        
        private bool _isAtEnd => _current >= _source.Length;

        public Lexer(string source, Action<int, string> errorHandler)
        {
            _source = source;
            _errorHandler = errorHandler;
        }

        public List<Token> LexTokens()
        {
            while (!_isAtEnd)
            {
                _start = _current;
                var token = LexToken();
                if (token is null)
                {
                    continue;
                }
                
                switch (token.Type)
                {
                    case TokenType.Comment:
                    case TokenType.WhiteSpace:
                        break;
                    case TokenType.NewLine:
                        _line++;
                        break;
                    default:
                        _tokens.Add(token);
                        break;
                }
            }
            
            _tokens.Add(new Token(TokenType.Eof, "", null!, _line));
            return _tokens;
        }

        private Token? LexToken()
        {
            switch (Advance())
            {
                case TokenChars.LeftParen:
                    return CreateToken(TokenType.LeftParen);
                case TokenChars.RightParen:
                    return CreateToken(TokenType.RightParen);
                case TokenChars.LeftBrace:
                    return CreateToken(TokenType.LeftBrace);
                case TokenChars.RightBrace:
                    return CreateToken(TokenType.RightBrace);
                case TokenChars.Comma:
                    return CreateToken(TokenType.Comma);
                case TokenChars.Dot:
                    return CreateToken(TokenType.Dot);
                case TokenChars.Plus:
                    return CreateToken(TokenType.Plus);
                case TokenChars.Minus:
                    return CreateToken(TokenType.Minus);
                case TokenChars.Star:
                    return CreateToken(TokenType.Star);
                case TokenChars.Semicolon:
                    return CreateToken(TokenType.Semicolon);
                case TokenChars.Not:
                    return CreateToken(
                        LookAhead(TokenChars.Equal) ?
                            TokenType.NotEqual :
                            TokenType.Not);
                case TokenChars.Equal:
                    return CreateToken(
                        LookAhead(TokenChars.Equal) ?
                            TokenType.EqualCompare :
                            TokenType.EqualAssign);
                case TokenChars.Greater:
                    return CreateToken(
                        LookAhead(TokenChars.Equal) ?
                            TokenType.GreaterEqual :
                            TokenType.Greater);
                case TokenChars.Lesser:
                    return CreateToken(
                        LookAhead(TokenChars.Equal) ?
                            TokenType.LessEqual :
                            TokenType.Less);
                case TokenChars.Slash:
                    return Comment();
                case TokenChars.Space:
                case TokenChars.Carriage:
                case TokenChars.Tab:
                    return CreateToken(TokenType.WhiteSpace);
                case TokenChars.NewLine:
                    return CreateToken(TokenType.NewLine);
                case TokenChars.DoubleQuote:
                    return String();
                case { } c when char.IsDigit(c):
                    return Number();
                case { } c when char.IsLetter(c):
                    return Identifier();
                default:
                    _errorHandler(_line, "Unexpected Character");
                    return null;
            }
        }

        private Token Identifier()
        {
            while (char.IsLetterOrDigit(PeekAhead()))
            {
                Advance();
            }

            if (ReservedKeywords.Keywords.TryGetValue(_currentLexeme, out var keyword))
            {
                return CreateToken(keyword);
            }

            return CreateToken(TokenType.Identifier);
        }

        private Token? Number()
        {
            while (char.IsDigit(PeekAhead()))
            {
                Advance();
            }

            if (PeekAhead() == TokenChars.Dot && char.IsDigit(PeekNext()))
            {
                Advance();
                while (char.IsDigit(PeekAhead()))
                {
                    Advance();
                }
            }

            if (!double.TryParse(_currentLexeme, out _))
            {
                _errorHandler(_line, "Invalid number literal provided");
                return null;
            }

            return CreateToken(TokenType.Number, _currentLexeme);
        }
        private Token Comment()
        {
            if(PeekAhead() == TokenChars.Slash)
            {
                Advance();
                while (PeekAhead() != TokenChars.NewLine && !_isAtEnd)
                {
                    Advance();
                }
                return CreateToken(TokenType.Comment);
            }
            else if (PeekAhead() == TokenChars.Star)
            {
                Advance();
                while (!_isAtEnd && PeekAhead() != TokenChars.Star && PeekNext() != TokenChars.Slash)
                {
                    Advance();
                }
                Advance(2);
                return CreateToken(TokenType.Comment);
            }
            return CreateToken(TokenType.Slash);
        }

        private Token? String()
        {
            while (PeekAhead() != TokenChars.DoubleQuote && !_isAtEnd)
            {
                if (PeekAhead() == TokenChars.NewLine)
                {
                    _line++;
                }
                Advance();
            }

            if (_isAtEnd)
            {
                _errorHandler(_line, "Unterminated String");
                return null;
            }

            Advance();

            //Return the created token with the value of the string and the "" trimmed off the ends
            return CreateToken(TokenType.String, _source[(_start + 1)..(_current - 1)]);
        }
        
        private bool LookAhead(char expected) {
            if (_isAtEnd || _source[_current] != expected)
            {
                return false;
            }

            _current++;
            return true;
        }
        
        private char PeekAhead() => _isAtEnd ? '\0' : _source[_current];
        
        private char PeekNext() {
            if (_current + 1 >= _source.Length)
            {
                return '\0';
            }
            return _source[_current + 1];
        } 

        private Token CreateToken(TokenType type)
            => new(type, _currentLexeme, null!, _line);
        
        private Token CreateToken(TokenType type, string source)
            => new(type, source, source, _line);
        
        private char Advance(int count=1) {
            _current += count;
            return _source[_current - 1];
        }
    }
}