using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using static utils.Utils;
using ZoneContext = tracy.TracyNative.___tracy_c_zone_context;

namespace tracy {

    public static class TracyNative {

        [StructLayout(LayoutKind.Sequential)]
        public struct ___tracy_source_location_data {
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
        public readonly struct ___tracy_c_zone_context {
            public readonly uint id;
            public readonly int active;

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
        // Made for debugging. It just prints the content of the `___tracy_source_location_data` and calls the original `___tracy_emit_zone_begin`.
        // Original: ___tracy_c_zone_context wrapperStart(const struct ___tracy_source_location_data* loc, int active)
        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern ___tracy_c_zone_context wrapperStart(ref ___tracy_source_location_data loc, int active);

        // Notes Oscar: You can find the code for this in `build-tracy-as-dll.cpp`.
        // Made for debugging. It prints the content of the `___tracy_c_zone_context` and calls the original `___tracy_emit_zone_end`.
        // Original: void wrapperEnd(struct ___tracy_c_zone_context ctx)
        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern void wrapperEnd(___tracy_c_zone_context ctx);

    }

    public struct SourceLocation {
        public TracyNative.___tracy_source_location_data data;
        public bool isInitialized;
    }

    public static class Tracy {

        public static TracyNative.___tracy_c_zone_context ProfileStart(ref SourceLocation loc, string name = null, uint color = 0, [CallerMemberName] string function = "unknown", [CallerFilePath] string file = "unknown", [CallerLineNumber] uint line = 0) {
            if (!loc.isInitialized) {
                loc.data.name = Marshal.StringToHGlobalAnsi(name);
                loc.data.function = Marshal.StringToHGlobalAnsi(function);
                loc.data.file = Marshal.StringToHGlobalAnsi(file);
                loc.data.line = line;
                loc.data.color = color;
                loc.isInitialized = true;
            }
            return TracyNative.___tracy_emit_zone_begin(ref loc.data, 1);
        }

        public static void ProfileEnd(TracyNative.___tracy_c_zone_context context) {
            TracyNative.___tracy_emit_zone_end(context);
        }

    }

    // Notes Oscar: based on tihs https://stu.dev/defer-with-csharp8/
    public readonly struct ProfileScope : IDisposable {
        
        private readonly ZoneContext ctx;
        
        // TODO Oscar: check if using this is actually allocation when used like this...
        // using var profiledScope = ProfileScope(ref codeLocation, "zoneName");
        public ProfileScope(ref SourceLocation loc, string name = null, uint color = 0, [CallerMemberName] string function = "unknown", [CallerFilePath] string file = "unknown", [CallerLineNumber] uint line = 0) {
            ctx = Tracy.ProfileStart(ref loc, name, color, function, file, line);
        }
        
        public void Dispose() => Tracy.ProfileEnd(ctx);
    }
}
