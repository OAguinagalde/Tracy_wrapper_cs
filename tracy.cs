using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace tracy {

    public static class TracyNative {

        [StructLayout(LayoutKind.Sequential)]
        public struct ___tracy_source_location_data
        {
            public string name;
            public string function;
            public string file;
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
            loc.name = name;
            loc.function = function;
            loc.file = file;
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
        
        static TracyNative.___tracy_source_location_data loc;
        
        public static void Main(string[] args) {

            while (true) {
                var context = Tracy.Trace(ref loc, "testArea");
                
                // Notes Oscar: For some reason, If I dont sleep(30), then it seems to work fine????
                // WHY???????????????????? But not always tho, sometimes the name shows as "Frame" instead of testArea...
                // Literally restarting the program with `dotnet run` and starting tracy.exe many times in a row and sometimes its good sometimes its not...
                // System.Threading.Thread.Sleep(30);
                
                Tracy.TraceEnd(context);
            }

        }
    }
}
