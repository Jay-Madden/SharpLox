using System;
using System.Collections.Generic;

namespace Runtime.Interpreting.Globals
{
    public class Clock : ICallable
    {
        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
            => DateTime.Now.Millisecond / 1000.0;

        public int Arity => 0;

        public override string ToString() => "<native func>";
    }
}