using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Runtime.Lexing;
using Runtime.Parsing.Productions;

namespace Runtime.Interpreting
{
    public class SharpLoxCallable : ICallable
    {
        private readonly IEnumerable<Token> _parameters;
        
        private readonly Block _body;
        
        private LoxEnvironment _closure;
        
        public int Arity { get; }

        public SharpLoxCallable(IEnumerable<Token> parameters, Block body, LoxEnvironment closure)
        {
            _parameters = parameters.ToList();
            _body = body;
            _closure = closure;
            Arity = _parameters.Count();
        }

        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            var funcEnv = new LoxEnvironment{Parent = _closure};
            foreach (var (token, arg) in _parameters.Zip(arguments, ValueTuple.Create))
            {
                funcEnv.Define(token.Lexeme, arg);
            }

            try
            {
                interpreter.ExecuteBlock(_body.Statements, funcEnv);
            }
            catch (ReturnValue returnValue)
            {
                return returnValue.Value;
            }
            
            return null!;
        }


        public override string ToString() 
            => $"<func {_parameters.Count()}>";
    }
}