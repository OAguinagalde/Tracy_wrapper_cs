# A C# Wrapper for Tracy profiler

This is not a fully featured wrapper, and more of a minimal setup using the Tracy profiler in your C# project. Also, it assumes 64 bit systems.

Currently it does not work on async scopes.

Use the API like this (`test.cs`).

```cs
using tracy;

public static class TestingProgram {
        
    public static void doSomeWork(int ms) {
        System.Threading.Thread.Sleep(ms);
    }

    public static void Main(string[] args) {
        while (true) {
            
            var manualProfile = Tracy.ProfileManually("manualProfile");
            doSomeWork(30);
            manualProfile.End();

            using var _1 = Tracy.ProfileScope("testProfiledScope");

            if (true) {
                using var _2 = Tracy.ProfileScope("testPlots");
                Tracy.PlotValue("myPlot", new System.Random().NextDouble());
                Tracy.SendMessage("finished the if...");
            }

            for (int i = 0; i < 5; i++) {

                using var _3 = Tracy.ProfileScope();
                doSomeWork(10);
            }

            Tracy.FrameMark("LoopEnd");
        }
    }
}
```

# Usage

Add a reference to this project, `dotnet add reference Tracy_wrapper_cs/Tracy_wrapper_cs.csproj`.

The class `tracy.TracyNative` wraps all the native calls, which will try to locate them during runtime in `tracy.dll`. You can easily build the dll for windows by executing the powershell script `Make-TracyDll.ps1` from a visual studio developer command prompt (since the script will call both `git.exe` and `cl.exe` to clone and build the dll).

> You can enter the visual studio developer command prompt by executing the script `> .\Start-VSDevCommandPromt.ps1 -vs2022`. Requires VS2022 or VS2019 to be installed.

Note that the script will compile `TracyClient.cpp` with only the minimum required defines `TRACY_ENABLE` and `TRACY_EXPORTS`. You can manually add others you might need and adapt the `tracy.TracyNative` to whatever other features you might want.

Finally move the dll to the folder where your application executable is located. When executing your application it will locate `tracy.dll` during runtime in its folder.

> Tracy's sampling mechanism and other useful features will not work unless the process is running with privileges

# Using the Profiler

You can build the profiler GUI `Tracy.exe` easily from a visual studio developer command prompt. Locate the profiler's folder `> cd tracy/profiler/build/win32` and call `> msbuild.exe /p:Configuration=Release`. You can now start the profiler from `tracy/profiler/build/win32/x64/Release/Tracy.exe`. Once started, it will automatically find your profiled application (if its running).

If you have any issue building the profiler, you might be missing some dependency which can easily be installed using the visual studio installation tool and selecting the desktop c++ development tools.

# Testing

`test.cs` contains a main method which will just test the wrapper, so you can easily test this by calling `Make-TracyDll.ps1` and then directly calling `dotnet run` from a visual studio developer command prompt, and then opening `Tracy.exe`