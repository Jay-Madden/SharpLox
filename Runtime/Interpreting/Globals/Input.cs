using System;
using System.Collections.Generic;
using System.Linq;

namespace Runtime.Interpreting.Globals
{
    public class Input : ICallable
    {
        public object Call(Interpreter interpreter, IEnumerable<object> arguments) 
            => Console.ReadLine() ?? "";

        public int Arity => 0;
    }
}