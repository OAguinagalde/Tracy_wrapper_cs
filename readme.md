# A C# Wrapper for Tracy profiler

Use the API like this (`test.cs`).

```cs
using tracy;
public static class TestingProgram {
        
    // For now we need to make the profiling locations static to ensure its lifetime...
    // TODO Figure out how to get rid of this in a relatively performant way
    private static SourceLocation loc1;
    private static SourceLocation loc2;
    private static SourceLocation loc3;
    
    public static void doSomeWork(int ms) {
        System.Threading.Thread.Sleep(ms);
    }

    public static void Main(string[] args) {
        while (true) {
            using var _1 = new ProfileScope(ref loc1, "testArea");
            doSomeWork(30);
            if (true) {
                using var _2 = new ProfileScope(ref loc2, "if");
                doSomeWork(10);
                for (int i = 0; i < 20; i++) {
                    using var _3 = new ProfileScope(ref loc3, "for");
                    doSomeWork(10);
                }
            }
        }
    }
}
```

# Usage

Add a reference to this project, `dotnet add reference Tracy_wrapper_cs/Tracy_wrapper_cs.csproj`.

The class `tracy.TracyNative` wraps all the native calls, which will try to locate them during runtime in `tracy.dll`. You can easily build the dll for windows by executing the powershell script `Make-TracyDll.ps1` from a visual studio developer command prompt (since the script will call both `git.exe` and `cl.exe` to clone and build the dll). Finally move the dll to the folder where your application executable is located.

# Using the Profiler

You can build the profiler GUI `Tracy.exe` easily from a visual studio developer command prompt. Locate the profiler's folder `> cd tracy/profiler/build/win32` and call `> msbuild.exe`. You can now start the profiler from `tracy/profiler/build/win32/x64/Debug/Tracy.exe`. Once started, it will automatically find your profiled application (if its running).

# Testing

`test.cs` contains a main method which will just test the wrapper, so you can easily test this by calling `Make-TracyDll.ps1` and then directly calling `dotnet run` from a visual studio developer command prompt, and then opening `Tracy.exe`