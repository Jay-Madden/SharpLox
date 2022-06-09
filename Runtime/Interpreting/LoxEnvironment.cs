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
            if (_values.ContainsKey(name))
            {
                throw new RuntimeErrorException(null, $"Variable '{name}' has already been defined in this scope");
            }
            _values[name] = value;
        }

        public object Get(Token name)
        {
            if (_values.TryGetValue(name.Lexeme, out var val))
            {
                if (val is null)
                {
                    throw new RuntimeErrorException(name, $"Variable '{name.Lexeme}' has not been initialized");
                }
                return val;
            }

            if (Parent is not null)
            {
                return Parent.Get(name);
            }

            throw new RuntimeErrorException(name, $"Variable {name.Lexeme} does not exist");
        }

        public object GetAt(int depth, Token name)
            => WalkParents(depth).Get(name);

        private LoxEnvironment WalkParents(int distance)
        {
            var env = this;
            for (var i = 0; i < distance; i++)
            {
                env = env!.Parent;
            }
            return env!;
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

        public void AssignAt(int depth, Token name, object val)
            => WalkParents(depth)._values[name.Lexeme] = val;
    }
}