using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using TraceLocation = tracy.TracyNative.___tracy_source_location_data;

namespace tracy {

    public static class TracyNative {

        [StructLayout(LayoutKind.Sequential)]
        public struct ___tracy_source_location_data
        {
            public IntPtr name;
            public IntPtr function;
            public IntPtr file;
            public uint line;
            public uint color;

            // Original:
            // struct ___tracy_source_location_data {
            //     const char* name;
            //     const char* function;
            //     const char* file;
            //     uint32_t line;
            //     uint32_t color;
            // };
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ___tracy_c_zone_context
        {
            public uint id;
            public int active;

            // Original:
            // struct ___tracy_c_zone_context {
            //     uint32_t id;
            //     int active;
            // };
        }

        // Original: TRACY_API TracyCZoneCtx ___tracy_emit_zone_begin( const struct ___tracy_source_location_data* srcloc, int active )
        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern ___tracy_c_zone_context ___tracy_emit_zone_begin(ref ___tracy_source_location_data srcloc, int active);

        // Original: TRACY_API void ___tracy_emit_zone_end( TracyCZoneCtx ctx )
        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern void ___tracy_emit_zone_end(___tracy_c_zone_context ctx);
        
        // Notes Oscar: You can find the code for this in `build-tracy-as-dll.cpp`.
        // It literally just prints the content of the `___tracy_source_location_data` and calls the original `___tracy_emit_zone_begin`.
        // It shows that data is reaching the "C side" properly, as everything shows fine, but somehow things still break?
        // They break even more if I dont use this wrapper tho, no idea why...
        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern ___tracy_c_zone_context wrapperStart(ref ___tracy_source_location_data loc, int active);
        // Original: ___tracy_c_zone_context wrapperStart(const struct ___tracy_source_location_data* loc, int active)

        // Notes Oscar: You can find the code for this in `build-tracy-as-dll.cpp`.
        // It prints the content of the `___tracy_c_zone_context` and calls the original `___tracy_emit_zone_end`.
        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern void wrapperEnd(___tracy_c_zone_context ctx);
        // Original: void wrapperEnd(struct ___tracy_c_zone_context ctx)

        [StructLayout(LayoutKind.Sequential)]
        public struct testStruct
        {
            public string name;
            public string function;
            public string file;
            public uint line;
            public uint color;
        }

        // Notes Oscar: Ignore this, this is just for me testing that lifetimes were working as I thought.
        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern testStruct MyTest(ref testStruct obj);
        // Original: testStruct MyTest(const testStruct* object)

    }

    public static class Tracy {
        
        public static TracyNative.___tracy_c_zone_context Trace(ref TracyNative.___tracy_source_location_data loc, string name = null, uint color = 0, [CallerMemberName] string function = "unknown", [CallerFilePath] string file = "unknown", [CallerLineNumber] uint line = 0) {
            // System.Console.WriteLine($"loc {loc} func {function}, file {file}, line {line}, name {name}, color {color}");
            loc.name = Marshal.StringToHGlobalAnsi(name);
            loc.function = Marshal.StringToHGlobalAnsi(function);
            loc.file = Marshal.StringToHGlobalAnsi(file);
            loc.line = line;
            loc.color = color;
            return TracyNative.wrapperStart(ref loc, 1);
            // return TracyNative.___tracy_emit_zone_begin(ref loc, 1);
        }

        public static void TraceEnd(TracyNative.___tracy_c_zone_context context) {
            TracyNative.wrapperEnd(context);
            // TracyNative.___tracy_emit_zone_end(context);
        }

        public static TracyNative.testStruct MyTestWrapper(ref TracyNative.testStruct obj, [CallerMemberName] string function = "unknown", [CallerFilePath] string file = "unknown", [CallerLineNumber] uint line = 0, string name = null, uint color = 0) {
            // System.Console.WriteLine($"MyTestWrapper: testStruct {obj}, func {function}, file {file}, line {line}, name {name}, color {color}");
            obj.name = name;
            obj.function = function;
            obj.file = file;
            obj.line = line;
            obj.color = color;
            return TracyNative.MyTest(ref obj);
        }
    }

    public static class Program {
        
        // For now we need to make the profiling locations static to ensure its lifetime...
        // It shouldn't be a problem however as longs as long as we use them only on debugging builds
        // TODO Oscar: Figure out how to get rid of this in a relatively performant way
        static TraceLocation loc1;
        static TraceLocation loc2;
        static TraceLocation loc3;
        
        public static void Main(string[] args) {

            while (true) {
                // TODO Oscar: Figure out how to make this use a simpler syntax
                using var _1 = Defer(ctx => Tracy.TraceEnd(ctx), Tracy.Trace(ref loc1, "testArea"));
                
                wait(30);

                if (true) {
                    using var _2 = Defer(ctx => Tracy.TraceEnd(ctx), Tracy.Trace(ref loc2, "if"));
                    
                    wait(10);

                    for (int i = 0; i < 20; i++) {
                        using var _3 = Defer(ctx => Tracy.TraceEnd(ctx), Tracy.Trace(ref loc3, "for"));
                    
                        wait(10);
                    
                    }
                }
            }
        }

        // Notes Oscar: Defer mechanism in c# aparently can be kind of achieved!
        // https://stu.dev/defer-with-csharp8/
        static DeferDisposable<T> Defer<T>(Action<T> action, T param1) => new DeferDisposable<T>(action, param1);
        internal readonly struct DeferDisposable<T1> : IDisposable {
            readonly Action<T1> _action;
            readonly T1 _param1;
            public DeferDisposable(Action<T1> action, T1 param1) => (_action, _param1) = (action, param1);
            public void Dispose() => _action.Invoke(_param1);
        }

        public static void wait(int ms) {
            System.Threading.Thread.Sleep(ms);
        }
    }


}
