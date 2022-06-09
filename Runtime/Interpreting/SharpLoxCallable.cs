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
            // Function arguments get their own environment
            var funcArgEnv = new LoxEnvironment{Parent = _closure};
            foreach (var (token, arg) in _parameters.Zip(arguments, ValueTuple.Create))
            {
                funcArgEnv.Define(token.Lexeme, arg);
            }

            // the body gets a new environment with the parent set to the func args one
            var bodyEnv = new LoxEnvironment {Parent = funcArgEnv};

            try
            {
                interpreter.ExecuteBlock(_body.Statements, bodyEnv);
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