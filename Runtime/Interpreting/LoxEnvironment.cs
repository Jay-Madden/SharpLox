using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Runtime.Lexing;

namespace Runtime.Interpreting
{
    public class LoxEnvironment
    {
        private readonly Dictionary<string, object?> _values = new();
        
        public LoxEnvironment? Parent { get; init; }

        public void Define(string name, object? value=null)
        {
            _values[name] = value;
        }

        public object? Get(Token name)
        {
            if (_values.TryGetValue(name.Lexeme, out var val))
            {
                return val;
            }

            if (Parent is not null)
            {
                return Parent.Get(name);
            }

            throw new RuntimeErrorException(name, $"Variable: {name.Lexeme} does not exist");
        }

        public void Assign(Token name, object value)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                _values[name.Lexeme] = value;
                return;
            }

            Parent?.Assign(name, value);
        }
    }
}