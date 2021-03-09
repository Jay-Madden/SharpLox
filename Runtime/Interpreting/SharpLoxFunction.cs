using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Runtime.Lexing;
using Runtime.Parsing.Productions;

namespace Runtime.Interpreting
{
    public class SharpLoxFunction : ICallable
    {
        private FuncDeclaration _declaration;

        private LoxEnvironment _closure;
        
        public int Arity => _declaration.parameters.Count();

        public SharpLoxFunction(FuncDeclaration declaration, LoxEnvironment closure)
        {
            _declaration = declaration;
            _closure = closure;
        }
        

        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            var funcEnv = new LoxEnvironment{Parent = _closure};
            foreach (var (token, arg) in _declaration.parameters.Zip(arguments, ValueTuple.Create))
            {
                funcEnv.Define(token.Lexeme, arg);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.body.Statements, funcEnv);
            }
            catch (ReturnValue returnValue)
            {
                return returnValue.Value;
            }
            
            return null!;
        }


        public override string ToString() 
            => $"<func {_declaration.parameters.Count()}>";
    }
}