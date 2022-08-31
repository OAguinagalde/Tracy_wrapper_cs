using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace utils {

public class Utils {

    // Notes Oscar: Defer mechanism in c# aparently can be kind of achieved!
    // https://stu.dev/defer-with-csharp8/
    public static DeferDisposable<T> Defer<T>(Action<T> action, T param1) => new DeferDisposable<T>(action, param1);
    public readonly struct DeferDisposable<T1> : IDisposable
    {
        readonly Action<T1> _action;
        readonly T1 _param1;
        public DeferDisposable(Action<T1> action, T1 param1) => (_action, _param1) = (action, param1);
        public void Dispose() => _action.Invoke(_param1);
    }

}

}
