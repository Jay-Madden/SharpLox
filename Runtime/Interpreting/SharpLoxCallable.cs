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
        
        private SharpLoxEnvironment _closure;
        
        public int Arity { get; }

        private bool _isInitializer;

        public SharpLoxCallable(IEnumerable<Token> parameters, Block body, SharpLoxEnvironment closure,
            bool isInitializer)
        {
            _parameters = parameters.ToList();
            _body = body;
            _closure = closure;
            _isInitializer = isInitializer;
            Arity = _parameters.Count();
        }

        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            // Function arguments get their own environment
            var funcArgEnv = new SharpLoxEnvironment{Parent = _closure};
            foreach (var (token, arg) in _parameters.Zip(arguments, ValueTuple.Create))
            {
                funcArgEnv.Define(token.Lexeme, arg);
            }

            // the body gets a new environment with the parent set to the func args one
            var bodyEnv = new SharpLoxEnvironment {Parent = funcArgEnv};

            try
            {
                interpreter.ExecuteBlock(_body.Statements, bodyEnv);
            }
            catch (ReturnValue returnValue)
            {
                if (_isInitializer)
                {
                    return _closure.GetAt(0, new Token(TokenType.Identifier, "this", null!, 0));
                }
                return returnValue.Value;
            }
            
            return null!;
        }

        public SharpLoxCallable Bind(SharpLoxInstance instance)
        {
            var env = new SharpLoxEnvironment{Parent = _closure};
            env.Define("this", instance);
            return new SharpLoxCallable(_parameters, _body, env, _isInitializer);
        }

        public override string ToString() 
            => $"<func {_parameters.Count()}>";
    }
}