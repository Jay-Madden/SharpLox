using System;
using System.Collections.Generic;

namespace Interpreter.Lexing
{
    public class Lexer
    {
        private int Start;
        private int Current;
        private int Line = 1;

        private readonly Action<int, string> ErrorHandler;
        
        private readonly string Source;

        private readonly List<Token> Tokens = new();
        
        private bool IsAtEnd => Current >= Source.Length;

        public Lexer(string source, Action<int, string> errorHandler)
        {
            Source = source;
            ErrorHandler = errorHandler;
        }

        public List<Token> LexTokens()
        {
            while (!IsAtEnd)
            {
                Start = Current;
                var token = LexToken();
                if (token is null)
                {
                    ErrorHandler(Line, "Unexpected Character");
                    continue;
                }
                
                switch (token.Type)
                {
                    case TokenType.Comment:
                    case TokenType.WhiteSpace:
                        break;
                    case TokenType.NewLine:
                        Line++;
                        break;
                    default:
                        Tokens.Add(token);
                        break;
                }
            }
            
            Tokens.Add(new Token(TokenType.Eof, "", null!, Line));
            return Tokens;
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
                default:
                    return null;
            }
        }
        
        private Token Comment()
        {
            if(LookAhead(TokenChars.Slash))
            {
                while (PeekAhead() != TokenChars.NewLine && !IsAtEnd)
                {
                    Advance();
                }
                return CreateToken(TokenType.Comment);
            }
            return CreateToken(TokenType.Slash);
        }
        
        private bool LookAhead(char expected) {
            if (IsAtEnd || Source[Current] != expected)
            {
                return false;
            }

            Current++;
            return true;
        }
        
        private char PeekAhead() => IsAtEnd ? '\0' : Source[Current];

        private Token CreateToken(TokenType type)
            => new(type, Source[Start..Current], null!, Line);
        
        private char Advance() {
            Current++;
            return Source[Current - 1];
        }
    }
}