using System.Runtime.InteropServices;

namespace tracy
{

    public static class TracyNative
    {

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

        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern ___tracy_c_zone_context ___tracy_emit_zone_begin(ref ___tracy_source_location_data srcloc, int active);
        // Original: TRACY_API TracyCZoneCtx ___tracy_emit_zone_begin( const struct ___tracy_source_location_data* srcloc, int active )

        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern void ___tracy_emit_zone_end(___tracy_c_zone_context ctx);
        // Original: TRACY_API void ___tracy_emit_zone_end( TracyCZoneCtx ctx )

    }

    public static class Tracy
    {
        public static TracyNative.___tracy_c_zone_context Trace(string name, string function, string file, uint line, uint color)
        {
            var srcLocStatic = new TracyNative.___tracy_source_location_data();
            srcLocStatic.name = name;
            srcLocStatic.function = function;
            srcLocStatic.file = file;
            srcLocStatic.line = line;
            srcLocStatic.color = color;
            return TracyNative.___tracy_emit_zone_begin(ref srcLocStatic, 1);
        }

        public static void TraceEnd(TracyNative.___tracy_c_zone_context context)
        {
            TracyNative.___tracy_emit_zone_end(context);
        }
    }
    
    public static class Program {
        public static void Main(string[] args) {

            while (true) {
                var context = Tracy.Trace("myZone", "main", "tracy.cs", 77, 100);
                // Adding this sleep makes it so that when connecting with tracy.exe or capturing with capture.exe, it breaks
                // Without the sleep, it will kind of of work. sometimes.
                // System.Threading.Thread.Sleep(30);
                Tracy.TraceEnd(context);
            }

        }
    }
}
