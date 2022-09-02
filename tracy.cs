using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace tracy {

    public static class TracyNative {

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ___tracy_source_location_data {
            public readonly IntPtr name;
            public readonly IntPtr function;
            public readonly IntPtr file;
            public readonly uint line;
            public readonly uint color;

            public ___tracy_source_location_data(string name, uint color, string function, string file, uint line) {
                // These 3 are allocating in unmanaged memory, and yes, it never gets freed.
                // The reason is that it is required to always be available by tracy.
                this.name = Marshal.StringToHGlobalAnsi(name);
                this.function = Marshal.StringToHGlobalAnsi(function);
                this.file = Marshal.StringToHGlobalAnsi(file);
                this.line = line;
                this.color = color;
            }

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
        
        // Original: TRACY_API void ___tracy_emit_frame_mark( const char* name )
        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern void ___tracy_emit_frame_mark(IntPtr name);

        // Original: TRACY_API void ___tracy_emit_plot( const char* name, double val )
        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern void ___tracy_emit_plot(IntPtr name, double val);

        // Original: TRACY_API void ___tracy_emit_message( const char* txt, size_t size, int callstack )
        [DllImport("tracy.dll", CharSet = CharSet.Ansi)]
        public static extern void ___tracy_emit_message(IntPtr name, UInt64 size, int callstack);

    }

    public static class Tracy {

        private static ConcurrentDictionary<string, IntPtr> frames = new();
        private static ConcurrentDictionary<string, IntPtr> plots = new();

        private static int srcLocCnt = 0;
        private static ConcurrentDictionary<string, int> srcLocIndices = new();
        private static TracyNative.___tracy_source_location_data[] sourceLocations = new TracyNative.___tracy_source_location_data[50];

        // WARNING: for now it only allows up to 50 different locations at any time, its a wip lol
        private static TracyNative.___tracy_c_zone_context ProfileStart(string name = null, uint color = 0, [CallerMemberName] string function = "unknown", [CallerFilePath] string file = "unknown", [CallerLineNumber] uint line = 0) {
            var index = srcLocIndices.GetOrAdd($"{file}{line}", (_) => {
                lock (srcLocIndices) {
                    var i = srcLocCnt++;
                    sourceLocations[i] = new TracyNative.___tracy_source_location_data(name, color, function, file, line);
                    return i;
                };
            });
            return TracyNative.___tracy_emit_zone_begin(ref (sourceLocations[index]), 1);
        }

        private static void ProfileEnd(TracyNative.___tracy_c_zone_context context) {
            TracyNative.___tracy_emit_zone_end(context);
        }

        public static void PlotValue(string name, double value) {
            var cName = plots.GetOrAdd(name, (name) => {
                lock (plots) {
                    return Marshal.StringToHGlobalAnsi(name);
                }
            });
            TracyNative.___tracy_emit_plot(cName, value);
        }

        public static void FrameMark(string name) {
            var cName = frames.GetOrAdd(name, (name) => {
                lock (frames) {
                    return Marshal.StringToHGlobalAnsi(name);
                }
            });
            TracyNative.___tracy_emit_frame_mark(cName);
        }

        public static void SendMessage(string message) {
            var allocatedAnsiMessage = Marshal.StringToHGlobalAnsi(message);
            // Unlike other tracy functions, this one actually allocates its own space to store the message so there
            // is no need for us to keep the message in memory, so we free after this message
            TracyNative.___tracy_emit_message(allocatedAnsiMessage, (UInt64)message.Length, 0);
            Marshal.FreeHGlobal(allocatedAnsiMessage);
        }

        // Notes Oscar: based on this https://stu.dev/defer-with-csharp8/
        // Also, this: https://stackoverflow.com/questions/2412981/if-my-struct-implements-idisposable-will-it-be-boxed-when-used-in-a-using-statem/2413844#2413844
        // Which measn that we can do this `using var ignoredVar = new ProfileScope(ref loc);` without boxing the struct, so when Dispose is called, its called on
        // the original struct (as oposed to Dispose being called on a copy of the struct because boxing a value type copies the value)
        public readonly struct ProfileScope : IDisposable {
            
            private readonly TracyNative.___tracy_c_zone_context ctx;
            
            public ProfileScope(string name = null, uint color = 0, [CallerMemberName] string function = "unknown", [CallerFilePath] string file = "unknown", [CallerLineNumber] uint line = 0) {
                ctx = Tracy.ProfileStart(name, color, function, file, line);
            }

            // TODO: somehow I can't define this:
            // public ProfileScope() : this(null) {}
            // And also, if I use new ProfileScope() it calls some non-existing constructor without implementation instead of my constructor...

            public void Dispose() => Tracy.ProfileEnd(ctx);
        }
    }
}
